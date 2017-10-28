using System;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using PR2_Level_Generator;

namespace Level_Generator_Console
{
	// Master class
	class LevelGen
	{
		public LevelGen()
		{
			string folderPath = Assembly.GetExecutingAssembly().Location;
			folderPath = Directory.GetParent(folderPath).ToString();
			configPath = folderPath + "\\Gen.ini";
			Generator = new GenSimpleRace();

			if (File.Exists(configPath))
			{
				string[] str = File.ReadAllText(configPath).Split('\n');
				settingsPath = str[0];
				levelsPath = str[1];
				generationManager.username = str[2];
				generationManager.login_token = str[3];
			}
			else
			{ // Create directories for levels and settings
				levelsPath = folderPath + "\\levels";
				settingsPath = folderPath + "\\settings";

				if (!Directory.Exists(levelsPath))
					Directory.CreateDirectory(levelsPath);
				if (!Directory.Exists(settingsPath))
					Directory.CreateDirectory(settingsPath);

				SaveConfig();
			}

			InitializeUserCommandDictionary();
		}

		public string settingsPath;
		public string levelsPath;
		private string configPath;
		private void SaveConfig()
		{
			string str = settingsPath + "\n" + levelsPath +
				"\n" + generationManager.username + "\n" + generationManager.login_token;
			File.WriteAllText(configPath, str);
		}

		GenerationManager generationManager = new GenerationManager();
		ILevelGenerator Generator
		{
			get => generationManager.generator;
			set => generationManager.generator = value;
		}
		private string GenType
		{ get { return Generator.GetType().AssemblyQualifiedName; } }
		private MapLE Map { get { return Generator.Map; } }

		public void Main(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				Console.WriteLine("Blank command given.");
				return;
			}
			else
			{
				if (userCommands.Keys.Contains(args[0]))
				{
					string[] commandArgs = new string[args.Length - 1];
					Array.Copy(args, 1, commandArgs, 0, commandArgs.Length);
					Console.WriteLine(userCommands[args[0]](commandArgs));
				}
				else
					Console.WriteLine("\'" + args[0] + "\' is not a valid command. Use the command \'commands\' for a list of all available commands.");
			}
		}

		#region "Commands"
		private delegate string UserCommand(params string[] args);
		private SortedDictionary<string, UserCommand> userCommands;
		private void InitializeUserCommandDictionary()
		{
			userCommands = new SortedDictionary<string, UserCommand>();
			userCommands.Add("info", GetInfo);
			userCommands.Add("get", GetSettings);
			userCommands.Add("set", SetSettings);
			userCommands.Add("new_gen", NewGenerator);
			userCommands.Add("generate", GenerateLevel);
			userCommands.Add("set_token", SetToken);
			userCommands.Add("set_username", SetUsername);
			userCommands.Add("get_token", GetToken);
			userCommands.Add("get_username", GetUsername);
			userCommands.Add("upload", UploadLevel);
			userCommands.Add("set_levels_path", SetLevelsPath);
			userCommands.Add("set_settings_path", SetSettingsPath);
			userCommands.Add("get_paths", GetPaths);
			userCommands.Add("save_settings", SaveSettings);
			userCommands.Add("commands", GetCommandsList);
			userCommands.Add("types", GetTypesList);
			userCommands.Add("save", SaveLevel);
			userCommands.Add("save_pr3", SaveLevelAsPR3);
			userCommands.Add("load_settings", LoadSettings);
		}

		private string GetCommandsList(params string[] args)
		{
			return "Available commands:\n" + string.Join("\n", userCommands.Keys);
		}
		private string GetTypesList(params string[] args)
		{
			return "Available types:\nGenSimpleRace\nGenMaze\nGenFallRace\n";
		}
		private string GetBlockIDs(params string[] args)
		{
			return "Block IDs:\nBasic Blocks: 0-3\nBrick: 4\nDown, Up, Left, Right: 5-8\nMine: 9" +
			  "\nItem: 10\nPlayer Starts: 11-14\nIce: 15\nFinish: 16\nCrumble: 17\nVanish: 18" +
			  "\nMove: 19\nWater: 20\nGravity Right/Left: 21-22\nPush: 23\nNet: 24\nInf Item: 25" +
			  "\nHappy, Sad: 26-27\nHeart: 28\nTime: 29\nEgg: 30\n";

		}

		private string GetInfo(params string[] args)
		{
			StringBuilder str = new StringBuilder();
			str.Append("Generator Type: " + GenType);
			str.Append("\n\n" + GetGeneratorParams());
			str.Append("\n\n" + GetMapSettings());

			return str.ToString();
		}
		private string GetGeneratorParams()
		{
			StringBuilder str = new StringBuilder("Paramaters: ");
			string[] paramNames = Generator.GetParamNames();
			for (int i = 0; i < paramNames.Length; i++)
				str.Append("\n" + paramNames[i] + ": " + Generator.GetParamValue(paramNames[i]).ToString("R"));

			return str.ToString();
		}
		private string GetMapSettings()
		{
			StringBuilder str = new StringBuilder("Map Settings: ");
			string[] settingNames = Generator.Map.SettingNames;
			for (int i = 0; i < settingNames.Length; i++)
				str.Append("\n" + settingNames[i] + ": " + Generator.Map.GetSetting(settingNames[i]));

			return str.ToString();
		}

		private string GetSettings(params string[] args)
		{
			if (args.Length == 0)
				return "Please provide at least one parameter name to get. E.g. '-get title'.";

			StringBuilder ret = new StringBuilder();
			for (int i = 0; i < args.Length; i++)
				ret.Append(generationManager.GetParamOrSetting(args[i]) + ", ");
			ret.Length -= 2;

			return ret.ToString();
		}
		private string SetSettings(params string[] args)
		{
			if (args.Length == 0)
				return "Please provide at least one pair of parameter name/value to set. E.g. '-set title Example'.";

			StringBuilder ret = new StringBuilder();
			for (int i = 0; i < args.Length; i += 2)
			{
				if (args.Length <= i + 1)
				{
					ret.Append("Error: No value given for '" + args[i] + "'.\n");
					break;
				}

				bool success = generationManager.SetParamOrSetting(args[i], args[i + 1]);
				if (!success)
					ret.Append("Error: Could not set '" + args[i] + "'.\n");
			}
			if (ret.Length == 0)
				ret.Append("Success.");
			else
				ret.Length--;

			return ret.ToString();
		}
		private string NewGenerator(params string[] args)
		{
			if (args.Length < 1)
				return "Please provide a generator type. E.g. '-new_gen GenMaze'.";

			Type t = Type.GetType(args[0]);
			if (t == null)
				t = Type.GetType("PR2_Level_Generator." + args[0] + ", PR2_Level_Generator");

			if (t == null)
				return "Invalid type given. Use command 'types' for a list of available types.";
			else
			{
				object obj = Activator.CreateInstance(t);
				if (!(obj is ILevelGenerator))
					Console.WriteLine("Invalid type given. Use command 'types' for a list of available types.");
				else
					Generator = obj as ILevelGenerator;
			}

			return "New generator created.";
		}

		private string GenerateLevel(params string[] args)
		{
			Generator.GenerateMap();
			return null;
		}

		private string SetToken(params string[] args)
		{
			if (args.Length < 1)
				return "Please provide a PR2 login token. E.g. '-set_token 1-0000000000000000000000000000000'.";

			generationManager.login_token = args[0];
			SaveConfig();
			return "Token set.";
		}
		private string SetUsername(params string[] args)
		{
			if (args.Length < 1)
				return "Please provide a username. E.g. '-set_username Gummy_Bear'.";

			generationManager.username = args[0];
			SaveConfig();
			return "Username set.";
		}
		private string GetToken(params string[] args)
		{
			if (string.IsNullOrEmpty(generationManager.login_token))
				return "Token has not been set.";
			else
				return generationManager.login_token;
		}
		private string GetUsername(params string[] args)
		{
			if (string.IsNullOrEmpty(generationManager.username))
				return "Username has not been set.";
			else
				return generationManager.username;

		}

		private string UploadLevel(params string[] args)
		{
			if (string.IsNullOrEmpty(generationManager.login_token) || string.IsNullOrEmpty(generationManager.username))
				return "Please set a username and login token.";
			else
				return generationManager.UploadLevel();
		}
		private string GetSaveData()
		{
			// Append parameters to the note.
			double? oldSeed = null;
			if (Generator.GetParamNames().Contains("Seed"))
			{
				oldSeed = Generator.GetParamValue("seed");
				Generator.SetParamValue("seed", Generator.LastSeed);
			}
			string oldNote = Map.GetSetting("note");
			Map.SetSetting("note", oldNote + "Gen: " + GenType.Split('.').Last() + "\n" + GetGeneratorParams());
			if (oldSeed != null)
				Generator.SetParamValue("seed", oldSeed.Value);

			string ret = Map.GetData();
			Map.SetSetting("note", oldNote);
			return ret;
		}
		private string SaveLevel(params string[] args)
		{
			if (args.Length < 1)
			{
				return "Please specify a file location. Path may be relative to settings save path, or absolute.\n" +
				  "Current levels save path: " + levelsPath + "\nE.g. '-save example.txt'.";
			}

			string path = Path.Combine(levelsPath, args[0]);

			if (!Directory.GetParent(path).Exists)
				return "Failed to save level; directory does not exist.";

			string LData = GetSaveData();
			File.WriteAllText(path, LData);

			return "Level saved.";
		}
		private string SaveLevelAsPR3(params string[] args)
		{
			if (args.Length < 1)
			{
				return "Please specify a file location. Path may be relative to settings save path, or absolute.\n" +
				  "Current levels save path: " + levelsPath + "\nE.g. '-save example.txt'.";
			}

			string path = Path.Combine(levelsPath, args[0]);

			if (!Directory.GetParent(path).Exists)
				return "Failed to save level; directory does not exist.";

			string data = Map.GetData();
			data = LevelConverter.ConvertLevelToPR3(data);
			File.WriteAllText(path, data);

			return "Level saved as PR3 level.";
		}

		private string SetLevelsPath(params string[] args)
		{
			if (args.Length < 1)
				return "Please give a folder path. E.g. '-set_levels_path C:\\levels'.";

			levelsPath = Path.Combine(levelsPath, args[0]);
			SaveConfig();

			return "Levels path set.";
		}
		private string SetSettingsPath(params string[] args)
		{
			if (args.Length < 1)
				return "Please give a folder path. E.g. '-set_settings_path C:\\settings'.";

			settingsPath = Path.Combine(settingsPath, args[0]);
			SaveConfig();

			return "Settings path set.";
		}
		private string GetPaths(params string[] args)
		{
			return "Settings path: " + settingsPath +
			  "\nLevels path: " + levelsPath;
		}
		private string SaveSettings(params string[] args)
		{
			if (args.Length < 1)
			{
				return "Please specify a file location. Path may be relative to settings save path, or absolute.\n" +
				  "Current settings save path: " + settingsPath + "E.g. '-save_settings random.txt'.";
			}

			string path = Path.Combine(settingsPath, args[0]);

			if (!Directory.GetParent(path).Exists)
				return "Failed to save settings; directory does not exist.";

			StringBuilder str = new StringBuilder(GenType);
			string[] paramNames = Generator.GetParamNames();
			for (int i = 0; i < paramNames.Length; i++)
				str.Append("\n" + paramNames[i] + ": " + Generator.GetParamValue(paramNames[i]).ToString("R"));
			string[] settingNames = Generator.Map.SettingNames;
			for (int i = 0; i < settingNames.Length; i++)
				str.Append("\n" + settingNames[i] + ": " + Generator.Map.GetSetting(settingNames[i]));

			File.WriteAllText(path, str.ToString());

			return "Settings saved.";
		}
		private string LoadSettings(params string[] args)
		{
			if (args.Length < 1)
				return "Please specify a file location. E.g. '-load_settings random.txt'.";

			string path = Path.Combine(settingsPath, args[0]);

			if (!File.Exists(path))
				return "Failed to load settings; file does not exist.";

			string[] str = File.ReadAllText(path).Split('\n');
			Type t = Type.GetType(str[0]);
			// How do I set generator to be a new instance of type t?
			Generator = Activator.CreateInstance(t) as ILevelGenerator;
			int settingCount = Generator.GetParamNames().Length + Generator.Map.SettingNames.Length;
			for (int i = 1; i < settingCount + 1; i++)
			{
				string[] nameValue = str[i].Split(':');
				SetSettings(nameValue[0], nameValue[1]);
			}

			return "Settings loaded.";
		}
		#endregion

	}
}

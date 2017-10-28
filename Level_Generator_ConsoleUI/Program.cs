using System;
using System.Collections.Generic;
using System.Linq;

namespace Level_Generator_ConsoleUI
{
	class Program
	{
		static LevelGen generator = new LevelGen();

		static string[][] ParseLine(string line)
		{
			if (!line.StartsWith("-"))
				return null;

			int index = -1;
			List<List<string>> list = new List<List<string>>();

			do
			{
				if (line[index + 1] == '-')
				{
					list.Add(new List<string>());
					index++;
				}

				int quote = line.IndexOf('"', index + 1);
				int space = line.IndexOf(' ', index + 1);
				int newIndex = 0;
				if (quote != -1 && (quote < space || space == -1))
				{
					newIndex = line.IndexOf('"', quote + 1);
					list.Last().Add(line.Substring(quote + 1, newIndex - quote - 1));
					newIndex++;
				}
				else
				{
					if (space == -1)
						newIndex = line.Length;
					else
						newIndex = space;
					list.Last().Add(line.Substring(index + 1, newIndex - index - 1));
				}

				index = newIndex;
			} while (index != line.Length);

			string[][] ret = new string[list.Count][];
			for (int i = 0; i < ret.Length; i++)
				ret[i] = list[i].ToArray();
			return ret;
		}

		static void Main(string[] args)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
			Console.Title = "Level Generator";

			Console.Write("Type \'e\' at any time to exit.\n");
			Console.Write("\n\n>");

			while (true)
			{
				string line = Console.ReadLine();
				if (line == "e")
					break;
				if (line == "help")
				{
					Console.Write("All commands to be processed by the level generator should be prefaced with a '-' character.");
					Console.Write("\nAfter the command name, you can provide arguments for that command, separated by spaces.");
					Console.Write("\nIf an argument contains a space or the '-' character, surround it in quotation marks.");
					Console.Write("\nExample: -set title \"Quick Race\"\n");
				}

				string[][] commands = ParseLine(line);
				if (commands != null)
				{
					for (int i = 0; i < commands.Length; i++)
						generator.Main(commands[i]);
				}

				Console.Write("\n>");
			}
		}

	}
}

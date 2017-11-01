using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using PR2_Level_Generator;

namespace Level_Generator_ConsoleUI
{
	class GenFallRace : ILevelGenerator
	{
		public GenFallRace()
		{
			parameters = new SortedDictionary<string, double>();
			parameters.Add("sections", 10);
			parameters.Add("section_length", 4);
			parameters.Add("fall_dist", 16);
			parameters.Add("width", 9);
			parameters.Add("max_difficulty", 5);
			parameters.Add("min_difficulty", 0);
			parameters.Add("seed", 0);
			Map = new MapLE();
		}

		SortedDictionary<string, double> parameters;
		public string[] GetParamNames()
		{
			string[] ret = new string[parameters.Count];
			parameters.Keys.CopyTo(ret, 0);
			return ret;
		}
		public double GetParamValue(string name)
		{
			return parameters[name];
		}
		public void SetParamValue(string name, double value)
		{
			parameters[name] = value;
		}

		#region "Parameters"
		public int Sections
		{
			get { return (int)parameters["sections"]; }
			set { parameters["sections"] = value; }
		}
		public int Section_Length
		{
			get { return (int)parameters["section_length"]; }
			set { parameters["section_length"] = value; }
		}
		public int Fall_Dist
		{
			get { return (int)parameters["fall_dist"]; }
			set { parameters["fall_dist"] = value; }
		}
		public int Width
		{
			get { return (int)parameters["width"]; }
			set { parameters["width"] = value; }
		}
		public int Max_Difficulty
		{
			get { return (int)parameters["max_difficulty"]; }
			set { parameters["max_difficulty"] = value; }
		}
		public int Min_Difficulty
		{
			get { return (int)parameters["min_difficulty"]; }
			set { parameters["min_difficulty"] = value; }
		}
		public int Seed
		{
			get { return (int)parameters["seed"]; }
			set { parameters["seed"] = value; }
		}
		#endregion

		public MapLE Map { get; private set; }

		public int LastSeed { get; private set; }
		private Random R;

		public Task<bool> GenerateMap(CancellationTokenSource cts)
		{
			Map.ClearBlocks();
			Map.artCodes[3] = "";

			int rSeed = Seed;
			if (rSeed == 0)
				rSeed = Environment.TickCount;
			R = new Random(rSeed);
			LastSeed = rSeed;

			// Encase the actual level.
			int height = Sections * (Section_Length * Fall_Dist + 6);
			for (int iX = 0; iX < Width; iX++)
				Map.AddBlock(iX, -1, 0);
			for (int iX = 0; iX < Width; iX++)
				Map.AddBlock(iX, height - 1, 0);

			for (int iY = 0; iY < height - 1; iY++)
				Map.AddBlock(0, iY, 0);
			for (int iY = 0; iY < height - 1; iY++)
				Map.AddBlock(Width - 1, iY, 0);


			// Starting positions
			int start = Width / 2;
			Map.AddBlock(start, 0, BlockID.P2);
			Map.AddBlock(start, 0, BlockID.P4);
			if (Width % 2 == 0)
				start -= 1;
			Map.AddBlock(start, 0, BlockID.P1);
			Map.AddBlock(start, 0, BlockID.P3);

			// Main loop
			for (int i = 0; i < Sections; i++)
				GenerateSection(i);

			// Finish
			Map.ReplaceBlock(start, height - 4, BlockID.Finish);
			Map.AddBlock(start, height - 1, 0);
			if (Width % 2 == 0)
			{
				Map.ReplaceBlock(start + 1, height - 4, BlockID.Finish);
				Map.AddBlock(start + 1, height - 1, 0);
			}

			Console.WriteLine("Map Generated.");
			return Task.FromResult(true);
		}

		private void GenerateSection(int i)
		{
			int prevSlot = -1;
			int y = 0;
			for (int iSeg = 0; iSeg < Section_Length; iSeg++)
			{
				y = i * (Section_Length * Fall_Dist + 6);
				y += (iSeg + 1) * Fall_Dist - 1;

				// Place row of nets
				for (int iX = 1; iX < Width - 1; iX++)
					Map.AddBlock(iX, y, BlockID.Net);

				// Determine spot to remove
				int removeAt;
				do { removeAt = R.Next(1, Width - 1); }
				while (prevSlot != -1 && (Math.Abs(prevSlot - removeAt) > Max_Difficulty || Math.Abs(prevSlot - removeAt) < Min_Difficulty));
				prevSlot = removeAt;
				Map.DeleteBlock(removeAt, y);

				// BG lines
				string str = "d" + (removeAt * 30 + 15) + ";" + ((y - Fall_Dist) * 30 + 45);
				str += ";0;" + (Fall_Dist * 30 - 90);
				if (Map.artCodes[3].Length != 0)
					str = "," + str;
				Map.artCodes[3] += str;
			}

			// Place safe blocks
			y += 3;
			int center = Width / 2;
			Map.AddBlock(center, y, 0);
			if (Width % 2 == 0)
			{
				Map.AddBlock(center - 1, y, 0);
				center--;
			}

			y += 3;
			for (int iX = 1; iX < center; iX++)
				Map.AddBlock(iX, y, 0);

			if (Width % 2 == 0)
				center += 2;
			else
				center += 1;
			for (int iX = center; iX < Width; iX++)
				Map.AddBlock(iX, y, 0);
		}

		public string GetSaveString()
		{
			StringBuilder ret = new StringBuilder();
			ret.Append(this.GetType().ToString());
			foreach (KeyValuePair<string, double> kvp in parameters)
			{
				ret.Append("\n" + kvp.Key + ":" + kvp.Value);
			}

			return ret.ToString();
		}

	}
}

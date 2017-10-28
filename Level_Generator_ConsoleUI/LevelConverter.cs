using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PR2_Level_Generator;

namespace Level_Generator_ConsoleUI
{
    class LevelConverter
    {
        static int[] blockIDs_To3 = { 1, 23031, 23032, 23033, 5, 27, 26, 28, 29, 12, 11, 0, 0, 0, 0, 9, 7, 6, 19, 13, 20, 16, 15, 14, 18, 10, 8, 17, 30, 1, 1 };
        static string[] items_To3 = { "none", "l", "po", "li", "t", "su", "j", "sp", "sw", "bo" };
        static string[] pr2ItemStrings = { "none", "laser gun", "mine", "lightning", "teleport", "super jump", "jet pack", "speed burst", "sword", "ice wave" };

        static public string ConvertLevelToPR3(string PR2Data)
        {
            MapLE pr2Map = new MapLE();
            pr2Map.LoadLevel(PR2Data);

            string pr3LevelData = pr2Map.GetDataParam(false).Split('`')[2];
            pr3LevelData = convertBlocksToPR3Data(pr3LevelData);

            StringBuilder xmlData = new StringBuilder();
            xmlData.Append("<Params><p_ip>000.000.000.000</p_ip>");
            xmlData.Append("<p_title>" + pr2Map.GetSetting("title") + "</p_title>");
            xmlData.Append("<p_comment>" + pr2Map.GetSetting("note") + "</p_comment>");
            xmlData.Append("<p_mode>" + pr2Map.GetSetting("gameMode") + "</p_mode>");
            xmlData.Append("<p_items>" + convertItemsToPR3(pr2Map.GetSetting("items")) + "</p_items>");
            xmlData.Append("<p_alien>0</p_alien>");
            xmlData.Append("<p_sfchm>" + pr2Map.GetSetting("cowboyChance") + "</p_sfchm>");
            xmlData.Append("<p_snow>0</p_snow>");
            xmlData.Append("<p_wind>0</p_wind>");
            xmlData.Append("<p_seconds>" + pr2Map.GetSetting("max_time") + "</p_seconds>");
            xmlData.Append("<p_song_id>random</p_song_id>");
            xmlData.Append("<p_gravity>" + pr2Map.GetSetting("gravity") + "</p_gravity>");
            xmlData.Append("<p_bg_image>" + "BG7" + "</p_bg_image>");
            xmlData.Append("<p_level_data>" + pr3LevelData + "</p_level_data>");
            xmlData.Append("<p_publish>" + pr2Map.GetSetting("live") + "</p_publish>");
            xmlData.Append("</Params>");

            return xmlData.ToString();
        }

        private static string convertBlocksToPR3Data(string pr2BlockData)
        {
            pr2BlockData = pr2BlockData.Replace(';', ':');
            List<string> blocks = pr2BlockData.Split(',').ToList();
            for (int i = 0; i < blocks.Count; i++)
            {
                string[] parts = blocks[i].Split(':');
                if (parts.Length == 3 || i == 0)
                {
                    int id;
                    if (i == 0)
                        id = 0;
                    else
                        id = int.Parse(parts[2]);
                    blocks.Insert(i, "b" + blockIDs_To3[id]);
                    i++;
                    blocks[i] = parts[0] + ":" + parts[1];
                }
            }

            string p_level_data = "v2 | {\"blockStr\":\"";
            p_level_data += string.Join(",", blocks);
            p_level_data += "\",\"artArray\":[]}";

            return p_level_data;
        }
        private static string convertArtToPR3(string artLayerData, double depth)
        {
            string pr3LayerData = depth.ToString() + ",101,1,Layer " + depth;
            string[] artSteps = artLayerData.Split(',');
            List<string> pr3Steps = new List<string>();

            //int lastX = 0;
            //int lastY = 0;

            for (int i = 0; i < artSteps.Length; i++)
            {
                char stepType = artSteps[i][0];
                string thisStep = artSteps[i].Substring(1);
                switch (stepType)
                {
                    case 'd': // startX;startY;offsetX;offsetY to m offsetX:offsetY:size:color:b/e:alpha,offsetX:offsetY,etc
                        string[] positions = thisStep.Split(';');
                        pr3Steps.Add("m");
                        break;
                    case 'c':

                        break;
                    case 't':

                        break;
                    case 'm':

                        break;
                }
            }

            return pr3LayerData;
        }

        private static string convertItemsToPR3(string pr2Items)
        {
            List<string> items = new List<string>();
            string[] pr2 = pr2Items.Split(',');
            for (int i = 0; i < pr2.Length; i++)
            {
                int itemID = -1;
                if (!int.TryParse(pr2[i], out itemID))
                    itemID = Array.IndexOf(pr2ItemStrings, pr2[i]);
                items.Add(items_To3[itemID]);
            }

            return string.Join(",", items);
        }
    }
}

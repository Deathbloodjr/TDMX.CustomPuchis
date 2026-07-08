using LightWeightJsonParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CustomPuchis.Patches
{
    internal class CustomPuchiLoader
    {
        public static List<PuchiMetaData> LoadAllPuchiMetaData(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            List<PuchiMetaData> result = new List<PuchiMetaData>();

            var manifestFiles = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                .Where(file => file.EndsWith("puchi.json", StringComparison.OrdinalIgnoreCase));

            foreach (string filePath in manifestFiles)
            {
                var puchiData = LoadPuchiMetaData(filePath);
                if (puchiData != null)
                {
                    var duplicateIndex = result.FindIndex((x) => x.StringId == puchiData.StringId);
                    if (duplicateIndex != -1)
                    {
                        ModLogger.Log("Duplicate custom puchi found with Id of " +  puchiData.StringId);
                        continue;
                    }
                    else
                    {
                        result.Add(puchiData);
                        ModLogger.Log(result[result.Count - 1].NameInfo.enText + " has been loaded");
                    }
                }
                else
                {
                    ModLogger.Log("Error loading puchiData", LogType.Warning);
                }
            }

            // TODO: Sort AllCustomPuchi
            // TODO: Read in Order.json
            // TODO: Re-initialize Order.json (in other words, add any from AllCustomPuchi that aren't in Order.json, and remove any from Order.json that aren't in AllCustomPuchi, all based on Id)


            return result;
        }

        private static PuchiMetaData LoadPuchiMetaData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ModLogger.Log("Error loading puchiData: File not found", LogType.Warning);
                return null;
            }

            try
            {
                string jsonString = File.ReadAllText(filePath);

                var jsonNode = LWJson.Parse(jsonString);

                PuchiMetaData data = new PuchiMetaData();
                data.FolderPath = Path.GetDirectoryName(filePath);
                data.Id = jsonNode["Id"].AsString();
                if (jsonNode["Order"] != null)
                {
                    data.Order = jsonNode["Order"].AsInteger();
                }
                if (jsonNode["Name"] != null)
                {
                    data.NameInfo = ParseWordListData(data.NameKey, jsonNode["Name"].AsObject());
                }
                if (jsonNode["Description"] != null)
                {
                    data.DescriptionInfo = ParseWordListData(data.DescriptionKey, jsonNode["Description"].AsObject());
                }
                if (jsonNode["Files"] != null)
                {
                    var fileArray = jsonNode["Files"].AsArray().ArrayData;
                    for (int i = 0; i < fileArray.Count; i++)
                    {
                        data.Files.Add(fileArray[i].AsString());
                    }
                }
                else
                {
                    data.Files = DiscoverImagesNaturally(data.FolderPath);
                }

                if (data.Files.Count == 0)
                {
                    ModLogger.Log("Error loading puchi, no image files found: " + filePath, LogType.Warning);
                    return null;
                }
                if (jsonNode["Sprite"] != null)
                {
                    data.SpriteFile = jsonNode["Sprite"].AsString();
                }
                else
                {
                    data.SpriteFile = data.Files[0];
                }

                return data;
            }
            catch (Exception e)
            {
                ModLogger.Log(e.Message, LogType.Error);
                return null;
            }
        }

        private static WordListInfo ParseWordListData(string key, LWJsonObject wordListObj)
        {
            WordListInfo data = new WordListInfo();
            data.key = key;
            try
            {
                // FontTypes should be the same for each language for the most part
                // No need to declare it for every character, but it can be overwritten if needed
                // Also, I don't know which values are needed for each
                // These numbers line up with the enum DataConst.FontType:
                // None     = -1
                // Japanese = 0
                // EFIGS    = 1
                // ChineseT = 2
                // ChineseS = 3
                // Korean   = 4

                data.jpFontType = 0;
                data.enFontType = 1;
                data.frFontType = 1;
                data.itFontType = 1;
                data.deFontType = 1;
                data.esFontType = 1;
                data.tcFontType = 2;
                data.scFontType = 3;
                data.krFontType = 4;

                if (wordListObj["jpText"] != null) data.jpText = wordListObj["jpText"].AsString();
                if (wordListObj["jpFont"] != null) data.jpFontType = wordListObj["jpFont"].AsInteger();
                if (wordListObj["enText"] != null) data.enText = wordListObj["enText"].AsString();
                if (wordListObj["enFont"] != null) data.enFontType = wordListObj["enFont"].AsInteger();
                if (wordListObj["frText"] != null) data.frText = wordListObj["frText"].AsString();
                if (wordListObj["frFont"] != null) data.frFontType = wordListObj["frFont"].AsInteger();
                if (wordListObj["itText"] != null) data.itText = wordListObj["itText"].AsString();
                if (wordListObj["itFont"] != null) data.itFontType = wordListObj["itFont"].AsInteger();
                if (wordListObj["deText"] != null) data.deText = wordListObj["deText"].AsString();
                if (wordListObj["deFont"] != null) data.deFontType = wordListObj["deFont"].AsInteger();
                if (wordListObj["esText"] != null) data.esText = wordListObj["esText"].AsString();
                if (wordListObj["esFont"] != null) data.esFontType = wordListObj["esFont"].AsInteger();
                if (wordListObj["tcText"] != null) data.tcText = wordListObj["tcText"].AsString();
                if (wordListObj["tcFont"] != null) data.tcFontType = wordListObj["tcFont"].AsInteger();
                if (wordListObj["scText"] != null) data.scText = wordListObj["scText"].AsString();
                if (wordListObj["scFont"] != null) data.scFontType = wordListObj["scFont"].AsInteger();
                if (wordListObj["krText"] != null) data.krText = wordListObj["krText"].AsString();
                if (wordListObj["krFont"] != null) data.krFontType = wordListObj["krFont"].AsInteger();
            }
            catch (Exception e)
            {
                ModLogger.Log(e.Message, LogType.Error);
                return data;
            }
            return data;
        }

        private static List<string> DiscoverImagesNaturally(string folderPath)
        {
            // Get all PNGs case-insensitively using our streaming EnumerateFiles rule
            // Possibly expand this to other image types later, but PNG should be fine for awhile
            var rawFiles = Directory.EnumerateFiles(folderPath, "*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase));

            // Sort them naturally using a regex padding trick so 2 comes before 10
            return rawFiles
                .Select(Path.GetFileName)
                .OrderBy(f => Regex.Replace(f, @"\d+", m => m.Value.PadLeft(5, '0')))
                .ToList();
        }
    }
}

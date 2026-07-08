using LightWeightJsonParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomPuchis.Patches
{
    internal class SaveDataManager
    {
        static SaveData saveData;

        const string SaveFileName = "CustomPuchiSave.json";

        public static void SavePuchiData(PuchiMetaData puchiData, bool isCustomPuchi)
        {
            SaveData saveData = new SaveData();
            saveData.IsUsingCustomPuchi = isCustomPuchi;
            if (puchiData != null)
            {
                saveData.CustomPuchiStringId = puchiData.StringId;
            }
            else
            {
                saveData.CustomPuchiStringId = string.Empty;
            }

            LWJsonObject json = new LWJsonObject();
            json.Add("IsUsingCustomPuchi", saveData.IsUsingCustomPuchi)
                .Add("CustomPuchiStringId", saveData.CustomPuchiStringId);


            string saveFilePath = GetSaveFilePath();
            var jsonString = json.ToString();
            File.WriteAllText(saveFilePath, jsonString);
        }

        public static void LoadPuchiData()
        {
            saveData = new SaveData();
            string saveFilePath = GetSaveFilePath();
            if (File.Exists(saveFilePath))
            {
                string jsonString = File.ReadAllText(saveFilePath);
                var node = LWJson.Parse(jsonString);

                if (node["IsUsingCustomPuchi"] != null)
                {
                    saveData.IsUsingCustomPuchi = node["IsUsingCustomPuchi"].AsBoolean();
                    saveData.CustomPuchiStringId = node["CustomPuchiStringId"].AsString();
                }
            }
            else
            {
                saveData.IsUsingCustomPuchi = false;
                saveData.CustomPuchiStringId = string.Empty;
            }
        }

        public static string GetSavedCurrentPuchi()
        {
            if (saveData != null && saveData.IsUsingCustomPuchi)
            {
                return saveData.CustomPuchiStringId;
            }
            return string.Empty;
        }

        private static string GetSaveFilePath()
        {
            if (!Directory.Exists(Plugin.Instance.ConfigCustomPuchiSaveDirectory.Value))
            {
                Directory.CreateDirectory(Plugin.Instance.ConfigCustomPuchiSaveDirectory.Value);
            }
            string filePath = Path.Combine(Plugin.Instance.ConfigCustomPuchiSaveDirectory.Value, SaveFileName);
            return filePath;
        }
    }
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using static DonCosDataInterface;
using static WordDataInterface;

namespace CustomPuchis.Patches
{
    internal class CustomPuchiManager
    {
        private static List<PuchiMetaData> AllCustomPuchi = new List<PuchiMetaData>();
        private static string rootFolderLoaded = string.Empty;

        private static List<DonCosDataInterface.DonCosInfoAccesser> OfficialDonCosInfoAccesser = new List<DonCosDataInterface.DonCosInfoAccesser>();

        private static DonCosDataInterface donCosDataInterface
        {
            get
            {
                var commonObjects = TaikoSingletonMonoBehaviour<CommonObjects>.Instance;
                if (commonObjects == null) return null;

                var dataManager = commonObjects.MyDataManager;
                if (dataManager == null) return null;

                return dataManager.DonCosData;
            }
        }

        private static WordDataInterface wordDataInterface
        {
            get
            {
                var commonObjects = TaikoSingletonMonoBehaviour<CommonObjects>.Instance;
                if (commonObjects == null) return null;

                var dataManager = commonObjects.MyDataManager;
                if (dataManager == null) return null;

                return dataManager.WordData;
            }
        }

        private static string latestLanguage;

        [HarmonyPatch(typeof(DataManager))]
        [HarmonyPatch(nameof(DataManager.Awake))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void DataManager_Awake_Postfix(DataManager __instance)
        {
            //donCosDataInterface = __instance.DonCosData;
            //wordDataInterface = __instance.WordData;
            if (donCosDataInterface != null)
            {
                var donCosInfoAccessers = donCosDataInterface.donCosInfoAccessers;
                int numMakeFree = 3;
                for (int i = 0; i < donCosInfoAccessers.Count; i++)
                {
                    //ModLogger.Log("donCosInfoAccessers["+i+"].UniqueId: " + donCosInfoAccessers[i].UniqueId);
                    //ModLogger.Log("donCosInfoAccessers["+i+"].StringId: " + donCosInfoAccessers[i].StringId);
                    //ModLogger.Log("donCosInfoAccessers["+i+"].Body: " + donCosInfoAccessers[i].Body);
                    //ModLogger.Log("donCosInfoAccessers["+i+"].Head: " + donCosInfoAccessers[i].Head);
                    //ModLogger.Log("donCosInfoAccessers["+i+"].IsBaseOff: " + donCosInfoAccessers[i].IsBaseOff);
                    //ModLogger.Log("donCosInfoAccessers["+i+"].Kigurumi: " + donCosInfoAccessers[i].Kigurumi);
                    //ModLogger.Log("donCosInfoAccessers["+i+"].Order: " + donCosInfoAccessers[i].Order);
                    //ModLogger.Log("donCosInfoAccessers["+i+"].Price: " + donCosInfoAccessers[i].Price);
                    //ModLogger.Log("donCosInfoAccessers["+i+"].Puchi: " + donCosInfoAccessers[i].Puchi);
                    if (donCosInfoAccessers[i].Puchi != -1 && numMakeFree > 0)
                    {
                        donCosInfoAccessers[i].Price = 0;
                        numMakeFree--;
                    }
                    OfficialDonCosInfoAccesser.Add(donCosInfoAccessers[i]);
                }
                AddCustomPuchi();
            }
        }

        [HarmonyPatch(typeof(DataManager))]
        [HarmonyPatch(nameof(DataManager.ExchangeWordData))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void DataManager_ExchangeWordData_Postfix(DataManager __instance, string language)
        {
            latestLanguage = language;
            for (int i = 0; i < AllCustomPuchi.Count; i++)
            {
                __instance.WordData.WordDataInterfaces(AllCustomPuchi[i].NameKey, AllCustomPuchi[i].Name(language), AllCustomPuchi[i].NameFont(language));
                __instance.WordData.WordDataInterfaces(AllCustomPuchi[i].DescriptionKey, AllCustomPuchi[i].Description(language), AllCustomPuchi[i].DescriptionFont(language));
            }
        }

        public static void Load()
        {
            ModLogger.Log("Load");
            LoadAllPuchiMetaData();
            AddCustomPuchi();
        }

        public static void Unload()
        {
            ModLogger.Log("Unload");
            RemoveCustomPuchi();
            rootFolderLoaded = string.Empty;
        }

        public static void Reload()
        {
            ModLogger.Log("Reload");
            LoadAllPuchiMetaData();
            AddCustomPuchi();
        }


        private static void LoadAllPuchiMetaData()
        {
            ModLogger.Log("LoadAllPuchiMetaData");
            if (rootFolderLoaded != string.Empty)
            {
                var fullLoadedFolder = Path.GetFullPath(rootFolderLoaded);
                ModLogger.Log("fullLoadedFolder: " + fullLoadedFolder, LogType.Debug);
            }
            var fullDataDirectory = Path.GetFullPath(Plugin.Instance.ConfigCustomPuchiDataDirectory.Value);
            ModLogger.Log("fullDataDirectory: " + fullDataDirectory, LogType.Debug);

            if (rootFolderLoaded != "" &&
                Path.GetFullPath(rootFolderLoaded) == Path.GetFullPath(Plugin.Instance.ConfigCustomPuchiDataDirectory.Value))
            {
                // No need to reload everything, it's the same
                return;
            }

            RemoveCustomPuchi();
            AllCustomPuchi = CustomPuchiLoader.LoadAllPuchiMetaData(Plugin.Instance.ConfigCustomPuchiDataDirectory.Value);
            rootFolderLoaded = Plugin.Instance.ConfigCustomPuchiDataDirectory.Value;
        }

        private static void RemoveCustomPuchi()
        {
            ModLogger.Log("RemoveCustomPuchi");
            try
            {
                var DonCosInfoAccessers = donCosDataInterface.donCosInfoAccessers;
                var WordListInfoAccessers = wordDataInterface.wordListInfoAccessers;

                if (DonCosInfoAccessers == null)
                {
                    ModLogger.Log("Error removing custom puchi: DonCosData == null", LogType.Error);
                    return;
                }

                for (int i = 0; i < AllCustomPuchi.Count; i++)
                {
#if IL2CPP
                    var donCosAccessers = new List<DonCosDataInterface.DonCosInfoAccesser>();
                    for (int j = 0; j < DonCosInfoAccessers.Count; j++)
                    {
                        donCosAccessers.Add(DonCosInfoAccessers[j]);
                    }
                    var wordListInfoAccessers = new List<WordDataInterface.WordListInfoAccesser>();
                    for (int j = 0; j < WordListInfoAccessers.Count; j++)
                    {
                        wordListInfoAccessers.Add(WordListInfoAccessers[j]);
                    }
#else
                    var donCosAccessers = DonCosInfoAccessers;
                    var wordListInfoAccessers = WordListInfoAccessers;
#endif
                    var donCosIndex = donCosAccessers.FindIndex((x) => x.StringId == AllCustomPuchi[i].StringId);
                    if (donCosIndex != -1)
                    {
                        DonCosInfoAccessers.RemoveAt(donCosIndex);
                        ModLogger.Log("Removed puchi from donCosDataInterface: " + AllCustomPuchi[i].StringId, LogType.Debug);
                    }

                    var nameIndex = wordListInfoAccessers.FindIndex((x) => x.Key == AllCustomPuchi[i].NameKey);
                    if (nameIndex != -1)
                    {
                        WordListInfoAccessers.RemoveAt(nameIndex);
                        ModLogger.Log("Removed puchi name from wordDataInterface: " + AllCustomPuchi[i].StringId, LogType.Debug);
                    }

                    var descIndex = wordListInfoAccessers.FindIndex((x) => x.Key == AllCustomPuchi[i].DescriptionKey);
                    if (descIndex != -1)
                    {
                        WordListInfoAccessers.RemoveAt(descIndex);
                        ModLogger.Log("Removed puchi description from wordDataInterface: " + AllCustomPuchi[i].StringId, LogType.Debug);
                    }
                }
                AllCustomPuchi.Clear();
            }
            catch (Exception e)
            {
                ModLogger.Log(e.Message, LogType.Error);
            }
        }

        private static void AddCustomPuchi()
        {
            ModLogger.Log("AddCustomPuchi");
            if (donCosDataInterface == null ||
                wordDataInterface == null)
            {
                return;
            }

            var DonCosInfoAccessers = donCosDataInterface.donCosInfoAccessers;
#if IL2CPP
            var donCosInfoAccessers = new List<DonCosDataInterface.DonCosInfoAccesser>();
            for (int j = 0; j < DonCosInfoAccessers.Count; j++)
            {
                donCosInfoAccessers.Add(DonCosInfoAccessers[j]);
            }
#else
            var donCosInfoAccessers = DonCosInfoAccessers;
#endif
            for (int i = 0; i < AllCustomPuchi.Count; i++)
            {
                if (donCosInfoAccessers.FindIndex((x) => x.StringId == AllCustomPuchi[i].StringId) == -1)
                {
                    DonCosDataInterface.DonCosInfoAccesser accesser = new DonCosDataInterface.DonCosInfoAccesser(
                        1000 + i,                                  // UniqueId
                        AllCustomPuchi[i].StringId,                // StringId
                        AllCustomPuchi[i].Order + 999000,          // Order
                        0,                                         // Price
                        -1,                                        // Head
                        -1,                                        // Body
                        100000 + i,                                // Puchi
                        -1,                                        // Kigurumi
                        false                                      // isBaseOff
                    );
                    DonCosInfoAccessers.Add(accesser);

                    // I'm not sure if it's necessary to add word data here
                    // It'd be convenient if it isn't necessary here

                    wordDataInterface.WordDataInterfaces(AllCustomPuchi[i].NameKey, AllCustomPuchi[i].Name(latestLanguage), AllCustomPuchi[i].NameFont(latestLanguage));
                    wordDataInterface.WordDataInterfaces(AllCustomPuchi[i].DescriptionKey, AllCustomPuchi[i].Description(latestLanguage), AllCustomPuchi[i].DescriptionFont(latestLanguage));

                    ModLogger.Log("Added puchi " + AllCustomPuchi[i].StringId);
                }
            }
        }

        public static PuchiMetaData GetPuchiFromUniqueId(int uniqueId)
        {
            if (IsCustomPuchiByPartsId(uniqueId))
            {
                int id = uniqueId - 1000;
                if (AllCustomPuchi.Count > id)
                {
                    return AllCustomPuchi[uniqueId - 1000];
                }
                else
                {
                    ModLogger.Log("Custom Puchi of uniqueId " + uniqueId + " not found", LogType.Error);
                }
            }
            return null;
        }

        public static PuchiMetaData GetPuchiFromPartsId(int partsId)
        {
            if (IsCustomPuchiByPartsId(partsId))
            {
                int id = partsId - 100000;
                if (AllCustomPuchi.Count > id)
                {
                    return AllCustomPuchi[partsId - 100000];
                }
                else
                {
                    ModLogger.Log("Custom Puchi of partsId " + partsId + " not found", LogType.Error);
                }
            }
            return null;
        }

        public static PuchiMetaData GetPuchiFromStringId(string stringId)
        {
            var index = AllCustomPuchi.FindIndex((x) => x.StringId == stringId);
            if (index != -1)
            {
                return AllCustomPuchi[index];
            }
            else
            {
                ModLogger.Log("Couldn't find custom puchi with StringId of :" + stringId, LogType.Error);
                return null;
            }
        }

        public static bool IsOfficialPuchi(int UniqueId)
        {
            // If FindIndex returns -1, then the UniqueId wasn't found in the list of Official accessers
            // Meaning it isn't an official puchi
            if (OfficialDonCosInfoAccesser.FindIndex((x) => x.UniqueId == UniqueId) != -1)
            {
                return true;
            }
            return false;
        }

        public static bool IsCustomPuchiByPartsId(int PartsId)
        {
            if (PartsId >= 100000)
            {
                return true;
            }
            return false;
        }


    }
}

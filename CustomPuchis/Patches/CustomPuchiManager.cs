using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CustomPuchis.Patches
{
    internal class CustomPuchiManager
    {
        private static List<PuchiMetaData> AllCustomPuchi = new List<PuchiMetaData>();
        private static string rootFolderLoaded = string.Empty;

        static List<DonCosDataInterface.DonCosInfoAccesser> OfficialDonCosInfoAccesser = new List<DonCosDataInterface.DonCosInfoAccesser>();

        [HarmonyPatch(typeof(DataManager))]
        [HarmonyPatch(nameof(DataManager.Awake))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void DataManager_Awake_Postfix(DataManager __instance)
        {
            var donCosInfoAccessers = __instance.DonCosData.donCosInfoAccessers;
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
                OfficialDonCosInfoAccesser.Add(donCosInfoAccessers[i]);
            }
            AddCustomPuchi(__instance.DonCosData, __instance.WordData);
        }

        [HarmonyPatch(typeof(DataManager))]
        [HarmonyPatch(nameof(DataManager.ExchangeWordData))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void DataManager_ExchangeWordData_Postfix(DataManager __instance)
        {
            for (int i = 0; i < AllCustomPuchi.Count; i++)
            {
                __instance.WordData.WordDataInterfaces(AllCustomPuchi[i].NameKey, AllCustomPuchi[i].Name.jpText, AllCustomPuchi[i].Name.jpFontType);
                __instance.WordData.WordDataInterfaces(AllCustomPuchi[i].DescriptionKey, AllCustomPuchi[i].Description.jpText, AllCustomPuchi[i].Description.jpFontType);
            }
        }




        public static void LoadAllPuchiMetaData()
        {
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

            AllCustomPuchi = CustomPuchiLoader.LoadAllPuchiMetaData();
        }

        public static void RemoveCustomPuchi()
        {
            
        }

        public static void AddCustomPuchi(DonCosDataInterface DonCosData, WordDataInterface WordDataInterface)
        {
            var donCosInfoAccessers = DonCosData.donCosInfoAccessers;
            for (int i = 0; i < AllCustomPuchi.Count; i++)
            {
                if (donCosInfoAccessers.FindIndex((x) => x.StringId == AllCustomPuchi[i].StringId) == -1)
                {
                    DonCosDataInterface.DonCosInfoAccesser accesser = new DonCosDataInterface.DonCosInfoAccesser(
                        1000 + i,                                  // UniqueId
                        AllCustomPuchi[i].StringId,                // StringId
                        AllCustomPuchi[i].Order + 999000,          // Order
                        0,                                        // Price
                        -1,                                        // Head
                        -1,                                        // Body
                        1000 + 1,                                  // Puchi
                        -1,                                        // Kigurumi
                        false                                      // isBaseOff
                    );
                    donCosInfoAccessers.Add(accesser);

                    WordDataInterface.WordDataInterfaces(AllCustomPuchi[i].NameKey, AllCustomPuchi[i].Name.jpText, AllCustomPuchi[i].Name.jpFontType);
                    WordDataInterface.WordDataInterfaces(AllCustomPuchi[i].DescriptionKey, AllCustomPuchi[i].Description.jpText, AllCustomPuchi[i].Description.jpFontType);
                }
            }
        }

        public static bool IsCustomPuchi(int UniqueId)
        {
            // If FindIndex returns -1, then the UniqueId wasn't found in the list of Official accessers
            // Meaning it's a custom puchi
            if (OfficialDonCosInfoAccesser.FindIndex((x) => x.UniqueId == UniqueId) == -1)
            {
                return true;
            }
            return false;
        }


    }
}

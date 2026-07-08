using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MultiplayManager;

namespace CustomPuchis.Patches
{
    internal class CustomPuchiPatch
    {
        static bool isCustomPuchi = false;
        static PuchiMetaData customPuchiMetaData;

        static bool isTryingPuchi = false;

        [HarmonyPatch(typeof(DonCommon))]
        [HarmonyPatch(nameof(DonCommon.LoadAll))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPrefix]
        public static void DonCommon_LoadAll_Prefix(DonCommon __instance, int coreId, int headId, int bodyId, int cosplayId, ref int puchiId)
        {
            ModLogger.Log("DonCommon_LoadAll_Prefix", LogType.Debug);
            //ModLogger.Log("coreId: " + coreId);
            //ModLogger.Log("headId: " + headId);
            //ModLogger.Log("bodyId: " + bodyId);
            //ModLogger.Log("cosplayId: " + cosplayId);
            ModLogger.Log("puchiId: " + puchiId, LogType.Debug);

            var puchiHandler = __instance.puchi.transform.Find("Puchi").gameObject.GetComponent<CustomPuchiHandler>();
            if (puchiHandler == null)
            {
                puchiHandler = __instance.puchi.transform.Find("Puchi").gameObject.AddComponent<CustomPuchiHandler>();
            }

            // Allows viewing all puchi when in Customize menu
            // Probably doesn't allow it in Shop menu, but whatever, buy it all instantly anyway
            if (isTryingPuchi)
            {
                if (CustomPuchiManager.IsCustomPuchiByPartsId(puchiId))
                {
                    int customId = puchiId;
                    var tmpCustomPuchiMetaData = CustomPuchiManager.GetPuchiFromPartsId(customId);
                    if (tmpCustomPuchiMetaData != null)
                    {
                        puchiId = 11000; // Master Bachio, to ensure the sprites change

                        puchiHandler.Initialize(tmpCustomPuchiMetaData);
                    }
                    else
                    {
                        isCustomPuchi = false;
                        puchiHandler.Initialize();
                    }
                }
                else
                {
                    isCustomPuchi = false;
                    puchiHandler.Initialize();
                }
            }
            else
            {
                puchiId = 11000; // Master Bachio, to ensure the sprites change
                puchiHandler.Initialize(customPuchiMetaData);
            }
        }


        [HarmonyPatch(typeof(CustomizeMyDon))]
        [HarmonyPatch(nameof(CustomizeMyDon.ChangeTab))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPrefix]
        public static void CustomizeMyDon_ChangeTab_Prefix(CustomizeMyDon __instance, DataConst.CustomizeTab nextTab)
        {
            isTryingPuchi = nextTab == DataConst.CustomizeTab.Puchi;
            // Have some other place to disable IsTryingPuchi
        }

        [HarmonyPatch(typeof(CustomizeMyDon))]
        [HarmonyPatch(nameof(CustomizeMyDon.ChangeState))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPrefix]
        public static void CustomizeMyDon_ChangeState_Prefix(CustomizeMyDon __instance, CustomizeMyDon.State nextState)
        {
            if (nextState == CustomizeMyDon.State.Closing)
            {
                isTryingPuchi = false;
            }
        }


        [HarmonyPatch(typeof(CustomizeMyDon))]
        [HarmonyPatch(nameof(CustomizeMyDon.ApplySelectedItem))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void CustomizeMyDon_ApplySelectedItem_Postfix(CustomizeMyDon __instance)
        {
            ModLogger.Log("CustomizeMyDon_ApplySelectedItem_Postfix", LogType.Debug);
            ModLogger.Log("__instance.currentTab: " + __instance.currentTab.ToString());
            if (__instance.currentTab == DataConst.CustomizeTab.Puchi)
            {
                TaikoSingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.PlayData.GetDonClothes(0, out var donClothes);

                isCustomPuchi = false;
                customPuchiMetaData = null;

                if (CustomPuchiManager.IsCustomPuchiByPartsId(donClothes.Puchi))
                {
                    customPuchiMetaData = CustomPuchiManager.GetPuchiFromPartsId(donClothes.Puchi);
                    if (customPuchiMetaData != null)
                    {
                        isCustomPuchi = true;
                    }
                }
                SaveDataManager.SavePuchiData(customPuchiMetaData, isCustomPuchi);
            }
        }

        [HarmonyPatch(typeof(PlayDataManager))]
        [HarmonyPatch(nameof(PlayDataManager.LoadData))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void PlayDataManager_LoadData_Postfix(PlayDataManager __instance, string containerName, string blobName)
        {
            if (blobName.StartsWith("save"))
            {
                SaveDataManager.LoadPuchiData();
                var currentPuchi = SaveDataManager.GetSavedCurrentPuchi();
                if (currentPuchi != string.Empty)
                {
                    isCustomPuchi = true;
                    customPuchiMetaData = CustomPuchiManager.GetPuchiFromStringId(currentPuchi);
                }
            }
        }
    }
}

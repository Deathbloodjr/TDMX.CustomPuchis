using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomPuchis.Patches
{
    internal class DonThumbnailPatch
    {
        [HarmonyPatch(typeof(DonThumbnailTable))]
        [HarmonyPatch(nameof(DonThumbnailTable.GetThumbnailSprite))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPrefix]
        public static bool DonThumbnailTable_GetThumbnailSprite_Prefix(DonThumbnailTable __instance, ref Sprite __result, DataConst.DressUpParts parts, int partsId)
        {
            //ModLogger.Log("DonThumbnailTable_GetThumbnailSprite_Prefix", LogType.Debug);

            if (parts != DataConst.DressUpParts.Puchi)
            {
                return true;
            }

            //ModLogger.Log("DonThumbnailTable_GetThumbnailSprite_Prefix Puchi", LogType.Debug);
            //ModLogger.Log("DonThumbnailTable_GetThumbnailSprite_Prefix partsId: " + partsId, LogType.Debug);

            if (CustomPuchiManager.IsCustomPuchiByPartsId(partsId))
            {
                var puchiMetaData = CustomPuchiManager.GetPuchiFromPartsId(partsId);
                if (puchiMetaData != null)
                {
                    //ModLogger.Log("Puchi of partsId " + partsId + " found", LogType.Debug);
                    var sprite = SpriteUtility.LoadSprite(Path.Combine(puchiMetaData.FolderPath, puchiMetaData.SpriteFile));
                    __result = sprite;
                    return false;
                }
                else
                {
                    //ModLogger.Log("Puchi of partsId " + partsId + " not found", LogType.Debug);
                }
            }

            return true;
        }
    }
}

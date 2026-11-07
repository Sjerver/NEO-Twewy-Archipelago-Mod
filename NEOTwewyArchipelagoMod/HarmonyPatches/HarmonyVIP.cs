using HarmonyLib;
using Il2Cpp;
using Il2CppHnLib;
using Il2CppMaster;
using Il2CppScenario;
using Il2CppUI.Shop;
using Il2CppUI.Think;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Il2CppCustomComponents.LinedRectGraphic.LinedRect;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(SaveDataShop))]
    public static class VIPPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddBrandPoint")]
        public static void PreAddBrandPoint(SaveDataShop __instance, Brand.ELabel brand, ref int addPoint,BattlePlayer.ELabel playerId = BattlePlayer.ELabel.Invalid,bool isEnableAbility = false)
        {
            if (addPoint >= 0)
            {
                addPoint = 0;
            }
            else
            {
                SaveDataShop.IBrandInfo brandInfo = __instance.GetBrandInfo(brand);

                int nextVIP = Math.Min(brandInfo.VipLevel + 1, brandInfo.MaxVipLevel);
                addPoint = brandInfo.NextLevelPoint(nextVIP) - brandInfo.Point;
                //MelonLogger.Msg($"Brand {brand} CurrentPoints {brandInfo.Point} + {addPoint}. NextLevelPoint {brandInfo.NextLevelPoint(nextVIP)}");
            }
        }

        public static void TriggerBrandLevelUp(QueuedReward reward)
        {
            int brandID = reward.GameID();

            SaveDataShop saveData = SaveLoadController.Get<SaveDataShop>();
            saveData.AddBrandPoint((Brand.ELabel)brandID,- 1, BattlePlayer.ELabel.Invalid, false);
            Core.vipDisplayQueue.Enqueue((Brand.ELabel)brandID);
        }
    }
    
}

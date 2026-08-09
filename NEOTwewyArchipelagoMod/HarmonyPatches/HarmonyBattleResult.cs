using HarmonyLib;
using Il2Cpp;
using Il2CppMaster;
using Il2CppUI.Battle;
using Il2CppUI.Shop;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(BattleResultUI))]
    public static class BattleResultUIPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("UpdatePrizeList")]
        public static void UpdatePrizeList(BattleResultUI __instance,Dive diveMaster)
        {
            //MelonLogger.Msg($"UpdatePrizeList Postfix {__instance.mDivePrizeList.Count}");
            foreach (BattleResultUI.PrizeInfo prize in __instance.mDivePrizeList)
            {
                //MelonLogger.Msg($"{diveMaster.Id} Prize: {prize.PrizeType} - {prize.PrizeStatus} - {prize.CharacterPt} - {prize.Item}");
                // 10401 Prize: CharacterPt - Bronze - 2 - Invalid

                GameLocationID diveRewardID = new GameLocationID(diveMaster.Id + ArchipelagoData.DIVE_RANK_MODIFIER[(SaveDataDive.EPrizeStatus)prize.PrizeStatus]);
            }
        }
    }
}

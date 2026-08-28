using HarmonyLib;
using Il2Cpp;
using Il2CppHnLib;
using Il2CppMaster;
using Il2CppNoise.Pig;
using Il2CppUI.Battle;
using Il2CppUI.Shop;
using Il2CppUI.Utility;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Il2CppCustomComponents.LinedRectGraphic.LinedRect;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(BattleResultUI))]
    public static class BattleResultUIPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("UpdatePrizeList")]
        public static void UpdatePrizeList(BattleResultUI __instance, Dive diveMaster)
        {
            //Dive list
            //MelonLogger.Msg($"UpdatePrizeList Postfix {__instance.mDivePrizeList.Count}");
            for (int index = 0; index < __instance.mDivePrizeList.Count; index++)
            {
                BattleResultUI.PrizeInfo prize = __instance.mDivePrizeList[index];
                //MelonLogger.Msg($"{diveMaster.Id} Prize: {prize.PrizeType} - {prize.PrizeStatus} - {prize.CharacterPt} - {prize.Item}");
                // 10401 Prize: CharacterPt - Bronze - 2 - Invalid

                GameLocationID diveRewardID = new(diveMaster.Id + ArchipelagoData.DIVE_RANK_MODIFIER[(SaveDataDive.EPrizeStatus)prize.PrizeStatus]);
                ArchipelagoLocationID locationID = diveRewardID.ToArchipelagoLocation(LocationType.DiveReward);
                if (Core.save.TryGetArchipelagoItem(locationID, out ArchipelagoItem item))
                {
                    //MelonLogger.Msg($"Item {item.name} has been gotten");
                    Core.save.enqueueLocation(locationID);
                    Core.save.addCheckedLocation(locationID);

                    if (item.id == NEOTwewyDataManager.DIVE_FP_ID)
                    {
                        //MelonLogger.Msg("FP Dive Reward");
                        prize.PrizeType = BattleResultUI.PrizeInfo.Type.CharacterPt;
                        prize.CharacterPt = (int)item.count;
                        prize.Item = AllItems.ELabel.Invalid;
                    } else {
                        //MelonLogger.Msg($"Item Dive Reward {item.id}");
                        prize.PrizeType = BattleResultUI.PrizeInfo.Type.Item;
                        prize.CharacterPt = 0;

                        AllItems itemObject = MasterDataBase<AllItems>.Get((int)item.id);
                        prize.Item = (AllItems.ELabel)itemObject.mId;
                        if(item.count > 1) { 
                            //If Count is more than 1 show archipelaog item in "immediate reward" screen and reward real item later
                            prize.Item = (AllItems.ELabel)Core.ARCHIPELAGO_ITEM_ID;
                            Core.archiItemDisplayQueue.Enqueue(item);
                        }
                        if (item.id == Core.ARCHIPELAGO_ITEM_ID){
                            Core.archiItemDisplayQueue.Enqueue(item);
                        }
                    }
                }
                __instance.mDivePrizeList[index] = prize;
            }


            if(__instance.mNoizePrizeList != null)
            { //TODO: Find out if this is blue boss noize?
                for (int index = 0; index < __instance.mNoizePrizeList.Count; index++)
                {
                    BattleResultUI.PrizeInfo prize = __instance.mNoizePrizeList[index];
                    //MelonLogger.Msg($"{diveMaster.Id} Prize: {prize.PrizeType} - {prize.PrizeStatus} - {prize.CharacterPt} - {prize.Item}");
                }
            }
            
        }
    }

    [HarmonyPatch(typeof(BattleResultData))]
    public static class BattleResultDataPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddDropInfo")]
        public static bool AddDropInfo(ref Badge.ELabel badge, int recordID, int difficulty)
        {
            Initialization.InitializeNoiseDrops();

            List<int> PIG_NOISE_REPORT_ID = [57, 60, 59, 58, 61, 66, 65, 63, 64, 62];
            if (PIG_NOISE_REPORT_ID.Contains(recordID)) { return true; }
           

            //MelonLogger.Msg($"Originally was Badge {badge} from {recordID} on {difficulty}");
            //Modify drop to what we expect it to be (to catch different versions of the same "record" enemy)
            EnemyReport currentReport = MasterDataBase<EnemyReport>.Get(recordID);
            EnemyData currentData = MasterDataBase<EnemyData>.Get((int)currentReport.mEnemydata);
            badge = currentData.mDrop[difficulty];

            GameLocationID gameID = new(recordID + difficulty * 100);
            ArchipelagoLocationID archiLocation = gameID.ToArchipelagoLocation(LocationType.NoiseDrop);

            //For now just prevent drops entirely to test
            //#TODO Consider moving this to where calc takes place? So we dont trigger "BLING"
            if (Core.prevent_out_of_logic && !DropRateCalc.DropRateReached(archiLocation)) { return false; }

            if (Core.save.TryGetArchipelagoItem(archiLocation, out ArchipelagoItem actualItem))
            {
                //MelonLogger.Msg($"Now is Badge {badge} from {recordID} on {difficulty} aka {actualItem.locationName}");

                if (actualItem.id == Core.ARCHIPELAGO_ITEM_ID && !Core.save.IsLocationChecked(archiLocation))
                {
                    Core.archiItemDisplayQueue.Enqueue(actualItem);
                }

                Core.save.enqueueLocation(archiLocation);
                Core.save.addCheckedLocation(archiLocation);
            }

            return true;
            
        }
    }

    //BattleDropItemManager
    [HarmonyPatch(typeof(BattleDropItemManager))]
    public static class BattleDropItemManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetDropPig")]
        public static void PreSetDropPig(EnemyBase enemy, DifficultyLevel.ELabel dropDifficulty,ref Badge.ELabel badge)
        {
            Initialization.InitializePigDrops();
            
            //MelonLogger.Msg($"Pig ID : {PigManager.PigManagerInstance.Pigdata.Id} Pig Drop: {PigManager.PigManagerInstance.Pigdata.Drop}");
            PigData pigData = MasterDataBase<PigData>.Get((int)PigManager.PigManagerInstance.Pigdata.Id);
            badge = pigData.mDrop;

            GameLocationID gameID = new(pigData.mId);
            ArchipelagoLocationID archiLocation = gameID.ToArchipelagoLocation(LocationType.PigDrop);

            if (Core.save.TryGetArchipelagoItem(archiLocation, out ArchipelagoItem actualItem))
            {
                //MelonLogger.Msg($"Pig ID : {PigManager.PigManagerInstance.Pigdata.Id} Pig Drop: {PigManager.PigManagerInstance.Pigdata.Drop}");
                if (actualItem.id == Core.ARCHIPELAGO_ITEM_ID && !Core.save.IsLocationChecked(archiLocation))
                {
                    Core.archiItemDisplayQueue.Enqueue(actualItem);
                }

                Core.save.enqueueLocation(archiLocation);
                Core.save.addCheckedLocation(archiLocation);
            }
        }

    }
}

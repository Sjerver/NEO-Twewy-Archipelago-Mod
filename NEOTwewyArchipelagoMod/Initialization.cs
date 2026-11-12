using Il2CppMaster;
using Il2CppUI.Utility;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Il2CppCustomComponents.LinedRectGraphic.LinedRect;
using static MelonLoader.MelonLogger;

namespace NEOTwewyArchipelagoMod
{
    public static class Initialization
    {
        private static bool __noiseDropInitialized = false;
        private static bool __shopGoodsInitialized = false;
        private static bool __pigInitialized = false;
        private static bool __snsPointsInitialized = false;

        public static bool isSynced() { return Core.syncState != SyncState.WrongSeed; }

        public static void InitializeAll()
        {
            InitializeNoiseDrops();
            InitializeShopGoods();
            InitializePigDrops();
            InitializeSNSPoints();
        }

        public static void InitializeNoiseDrops()
        {
            if (!__noiseDropInitialized && isSynced())
            {
                var enemyDataArray = MasterDataBase<EnemyData>.Array;
                var recordArray = MasterDataBase<EnemyReport>.Array;

                foreach (EnemyReport enemyReport in recordArray)
                {
                    List<int> enemyDataIds = new List<int>();
                    enemyDataIds.Add((int)enemyReport.mEnemydata);


                    foreach (EnemyData enemyData in enemyDataArray)
                    {
                        if (!enemyDataIds.Contains((int)enemyData.mId)) continue;

                        for (int index = 0; index < enemyData.mDrop.Count; index++)
                        {
                            GameLocationID dropID = new(enemyReport.mId + index * 100);
                            ArchipelagoLocationID locationID = dropID.ToArchipelagoLocation(LocationType.NoiseDrop);

                            if (Core.save.TryGetArchipelagoItem(locationID, out var item))
                            {
                                //MelonLogger.Msg($"{item.locationName} contains {item.name} / {item.id} used to be {enemyData.mDrop[index]} ");
                                if (item.itemType != ItemType.AllItems)
                                {
                                    enemyData.mDrop[index] = UIUtility.GetBadgeLabel(MasterDataBase<AllItems>.Get((int)Core.ARCHIPELAGO_ITEM_ID));
                                    continue;
                                }
                                AllItems itemObject = MasterDataBase<AllItems>.Get((int)item.id);
                                ItemConst.EItemType itemType = itemObject.ItemType;

                                if (itemType != ItemConst.EItemType.Badge || item.count > 1)
                                { //Replace with Archpelago item

                                    enemyData.mDrop[index] = UIUtility.GetBadgeLabel(MasterDataBase<AllItems>.Get((int)Core.ARCHIPELAGO_ITEM_ID));
                                }
                                else
                                { //Use the real badge

                                    enemyData.mDrop[index] = UIUtility.GetBadgeLabel(itemObject);
                                }
                            }

                        }
                    }
                }
                __noiseDropInitialized = true;
            }
        }

        public static void InitializeShopGoods()
        {
            if (__shopGoodsInitialized || !isSynced())
                return;

            __shopGoodsInitialized = true;
            //MelonLogger.Msg($"Creating shop UI data {dataID}");
            var goods = MasterDataBase<ShopGoods>.Array;
            //MelonLogger.Msg($"ShopGoods loaded: {goods.Length}");
            foreach (ShopGoods good in goods)
            {
                GameLocationID gameLocationID = new GameLocationID(good.mId);
                if (Core.save.TryGetArchipelagoItem(gameLocationID.ToArchipelagoLocation(LocationType.ShopGood), out ArchipelagoItem archiItem))
                {//If the shop good is an location
                    //if (Core.DEBUG) { MelonLogger.Msg($"At {archiItem.locationName} replaced {good.mItem} with {archiItem.name} "); }
                    if (NEOTwewyDataManager.NON_SHOP_ITEMS.Contains(archiItem.id) || archiItem.itemType != ItemType.AllItems)
                    { //Certain items need to be handled via archipelago
                        good.mItem = (AllItems.ELabel)Core.ARCHIPELAGO_ITEM_ID;
                    }
                    else
                    {
                        good.mItem = (AllItems.ELabel)archiItem.id;
                    }

                    AllItems itemObject = MasterDataBase<AllItems>.Get((int)good.mItem);
                    //MelonLogger.Msg($"ItemObject {itemObject}");
                    ItemConst.EItemType itemType = itemObject.ItemType;
                    //MelonLogger.Msg($"itemType {itemType}");
                    int[] itemCount;
                    if (itemType == ItemConst.EItemType.Badge)
                    { //Check if the item is a pin
                        Badge badge = MasterDataBase<Badge>.Get((int)UIUtility.GetBadgeLabel(itemObject));
                        //MelonLogger.Msg($"badge {badge}");
                        if (badge.mPsychic == Psychic.EName.Invalid)
                        {// Non combat Pin
                            if (badge.mBadgeCategory == ItemConst.EBadgeCategoryType.Material)
                            {
                                //MaterialPins
                                int consistentCount = (int)archiItem.count;
                                itemCount = [consistentCount, consistentCount, consistentCount, consistentCount];
                            }
                            else
                            {
                                if (good.mItem == (AllItems.ELabel)Core.ARCHIPELAGO_ITEM_ID)
                                {//Archipelago Badge can be bought once per shop good
                                    itemCount = [1, 1, 1, 1];
                                }
                                else
                                {//Money Pins can only be bought once
                                    itemCount = [1, 1, 1, 1];
                                }
                            }
                        }
                        else
                        { //Normal Combat Pins
                            itemCount = [1, 2, 3, 99];
                        }
                    }
                    else if (itemType == ItemConst.EItemType.Costume)
                    { //Threads
                        itemCount = [1, 2, 2, 6];
                    }
                    else
                    {  //Book/Music
                        itemCount = [1, 1, 1, 1];
                    }
                    good.mItemCount[0] = itemCount[0]; good.mItemCount[1] = itemCount[1]; good.mItemCount[2] = itemCount[2]; good.mItemCount[3] = itemCount[3];
                    //if (Core.DEBUG) { MelonLogger.Msg($"itemCount  = [{good.mItemCount[0]},{good.mItemCount[1]},{good.mItemCount[2]},{good.mItemCount[3]},"); }
                }
            }
        }

        public static void InitializePigDrops()
        {
            if (__pigInitialized || !isSynced())
                return;

            PigLocator.Initialize();

            var pigDataArray = MasterDataBase<PigData>.Array;

            foreach (PigData pigData in pigDataArray)
            {
                //MelonLogger.Msg($"gameID {pigData.mId}");
                GameLocationID gameLocationID = new GameLocationID(pigData.mId);

                ArchipelagoLocationID archipelagoLocationId;
                try
                {
                    archipelagoLocationId = gameLocationID.ToArchipelagoLocation(LocationType.PigDrop);
                }
                catch
                {
                    continue;
                }

                if (Core.save.TryGetArchipelagoItem(archipelagoLocationId, out ArchipelagoItem item))
                {
                    //MelonLogger.Msg($"gameID {gameLocationID} archiID {item.locationID} newItem {item.name}");

                    if (item.itemType != ItemType.AllItems)
                    {
                        pigData.mDrop = UIUtility.GetBadgeLabel(MasterDataBase<AllItems>.Get((int)Core.ARCHIPELAGO_ITEM_ID));
                        continue;
                    }

                    AllItems itemObject = MasterDataBase<AllItems>.Get((int)item.id);
                    ItemConst.EItemType itemType = itemObject.ItemType;

                    if (itemType != ItemConst.EItemType.Badge || item.count > 1)
                    { //Replace with Archpelago item

                        pigData.mDrop = UIUtility.GetBadgeLabel(MasterDataBase<AllItems>.Get((int)Core.ARCHIPELAGO_ITEM_ID));
                    }
                    else
                    { //Use the real badge

                        pigData.mDrop = UIUtility.GetBadgeLabel(itemObject);
                    }
                }

            }
            __pigInitialized = true;
        }

        public static void InitializeSNSPoints()
        {
            if (!__snsPointsInitialized && isSynced())
            {
                __snsPointsInitialized = true;
                //MelonLogger.Msg($"Creating shop UI data {dataID}");
                var skills = MasterDataBase<Skill>.Array;

                

                foreach (Skill skill in skills)
                {
                    GameLocationID gameLocationID = new GameLocationID(skill.Id);
                    ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.Skill);

                    if (ModOptions.fp_costs.TryGetValue((int)archipelagoLocationID.Value, out int fp))
                    {
                        skill.mPoint = fp;
                    }
                    if(Core.DEBUG) { skill.mPoint = 1; }
                    
                    


                    //Also initialize "container" rewards
                    if (!archipelagoLocationID.TryGetItem(out ArchipelagoItem item)) { continue; }
                    if (skill.mShopReward != AllItems.ELabel.Invalid)
                    {

                        if (item.itemType != ItemType.AllItems || item.count > 1)
                        {
                            skill.mShopReward = (AllItems.ELabel)Core.ARCHIPELAGO_ITEM_ID;
                            continue;
                        }
                        else
                        {
                            skill.mShopReward = (AllItems.ELabel)item.id;
                        }

                    }
                }
            }
        }
    }
}

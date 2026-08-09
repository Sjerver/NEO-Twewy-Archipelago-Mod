using HarmonyLib;
using Il2Cpp;
using Il2CppMaster;
using Il2CppUI;
using Il2CppUI.Controller;
using Il2CppUI.Panel;
using Il2CppUI.Shop;
using Il2CppUI.Utility;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Il2Cpp.SaveDataShop;
using static MelonLoader.MelonLogger;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(ShopScrollController))]
    public static class ShopScrollControllerPatch
    {

        public static ShopScrollController ShopScrollControllerInstance { get; private set; }
        private static bool initialized = false;

        [HarmonyPrefix]
        [HarmonyPatch("CreateUIInfoData")]
        public static void Prefix(ShopScrollController __instance, ref int dataID)
        {
            ShopScrollControllerInstance = __instance;
            if (initialized)
                return;

            initialized = true;
            //MelonLogger.Msg($"Creating shop UI data {dataID}");
            var goods = MasterDataBase<ShopGoods>.Array;
            //MelonLogger.Msg($"ShopGoods loaded: {goods.Length}");
            foreach (ShopGoods good in goods)
            {
                GameLocationID gameLocationID = new GameLocationID(good.mId);
                if (Core.save.TryGetShopItem(gameLocationID, out ArchipelagoItem archiItem))
                {//If the shop good is an location
                    if (Core.DEBUG) { MelonLogger.Msg($"At {archiItem.locationName} replaced {good.mItem} with {archiItem.name} "); }
                    if (NEOTwewyDataManager.NON_SHOP_ITEMS.Contains(archiItem.id))
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
                    if (Core.DEBUG) { MelonLogger.Msg($"itemCount  = [{good.mItemCount[0]},{good.mItemCount[1]},{good.mItemCount[2]},{good.mItemCount[3]},"); }
                }
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("CreateUIInfoData")]
        //public static void Postfix(ShopScrollController __instance, UIInfoBase __result)
        //{
        //    var shopInfo = __result.TryCast<ShopItemUIInfo>();
        //    if (shopInfo == null)
        //        return;
        //    if (Core.DEBUG) { MelonLogger.Msg($"ShopGoods ID: {shopInfo.mMasterShopGoods.mId} with {shopInfo.mMasterShopGoods.mItem}"); }
        //}

        [HarmonyPrefix]
        [HarmonyPatch("Purchace")]
        public static void Purchase(ShopScrollController __instance, int purchaseCount)
        {
            //!Core.save.getCheckedLocations().Contains((long)__0)
            ShopItemUIInfo shopInfo = __instance.GetSelectUIInfo<ShopItemUIInfo>();
            if (Core.DEBUG) { MelonLogger.Msg($"ShopGoods ID: {shopInfo.mMasterShopGoods.mId} with {shopInfo.mMasterShopGoods.mItem}"); }

            GameLocationID gamelocationID = new GameLocationID(shopInfo.mMasterShopGoods.mId);
            if (Core.save.TryGetShopItem(gamelocationID, out ArchipelagoItem archiItem))
            {// If the bought shop good is an archipelago location, mark it as checked
                ArchipelagoLocationID archiLocation = gamelocationID.ToArchipelagoLocation(LocationType.ShopGood);
                Core.save.enqueueLocation(archiLocation);
                Core.save.addCheckedLocation(archiLocation);
            }
        }


    }

    [HarmonyPatch(typeof(ShopItemUIInfo))]
    public static class ShopItemUIInfoPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("UpdateParam")]
        public static void MoneyPinShop(ShopItemUIInfo __instance)
        {
            if (__instance.MasterBadge is not null)
            { //Is the relevant item a badge?
                if (__instance.MasterBadge.mBadgeCategory == ItemConst.EBadgeCategoryType.Money)
                {// Is it a money badge/pin?
                    //MelonLogger.Msg("Money pin custom handling");
                    int currentlyOwnedAmount = SaveLoadController.Get<SaveDataBadge>().GetStackCount((Badge.ELabel)__instance.MasterBadge.ItemId);
                    if (__instance.mMasterShopGoods.mExchange == GoodsExchange.ELabel.Invalid && currentlyOwnedAmount < 99)
                    {//Is this not an exchange good and we have less than 99 of the pin
                        GameLocationID id = new GameLocationID(__instance.mMasterShopGoods.Id);
                        int timesBoughtGood = SaveLoadController.Get<SaveDataShop>().GetShopGoodsPurchases((ShopGoods.ELabel)id.Value);

                        
                        Core.save.TryGetShopItem(id, out ArchipelagoItem archiItem);
                        //MelonLogger.Msg($"Money pin {archiItem.name} currentlyOwnedAmount: {currentlyOwnedAmount} Location {archiItem.locationName} timesBought {timesBoughtGood} currentlyToSellMax {__instance.mMasterShopGoods.GetItemCountNow()} ");
                        
                        //Only mark the item as sold out if the player has bought all the items available in the shop good instead of checking player inventory
                        int itemCountNow = __instance.mMasterShopGoods.GetItemCountNow();
                        __instance.IsSoldOut = timesBoughtGood >= itemCountNow;
                        //MelonLogger.Msg($"Money pin is soldOut? {__instance.IsSoldOut}");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ShopBuySaleDialog))]
    public static class ShopBuySaleDialogPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Initialize")]
        public static void Initialize(ShopBuySaleDialog __instance, int price,ref int maxCount, bool isSale)
        {
            if (ShopDescriptionControllerPatch.CurrentShopGoods != null && maxCount == 0)
            {//Do we have a current shop good saved and is the maxCount 0
                Badge badge = MasterDataBase<Badge>.Get((int)UIUtility.GetBadgeLabel(MasterDataBase<AllItems>.Get((int)ShopDescriptionControllerPatch.CurrentShopGoods.Item)));
                if (badge.mBadgeCategory == ItemConst.EBadgeCategoryType.Money)
                {//Overwrite behaviour and set maxCount based on items bought instead of difference between shop good and inventory
                    int timesBoughtGood = SaveLoadController.Get<SaveDataShop>().GetShopGoodsPurchases((ShopGoods.ELabel)ShopDescriptionControllerPatch.CurrentShopGoods.Id);
                    int itemCountNow = ShopDescriptionControllerPatch.CurrentShopGoods.GetItemCountNow();
                    //MelonLogger.Msg($"ShopBuySaleDialog Initialize for Money Pin {badge.ItemName} timesBoughtGood={timesBoughtGood} itemCountNow={itemCountNow} ogMaxCount = {maxCount} new {Math.Max(0, itemCountNow - timesBoughtGood)}");
                    maxCount = Math.Max(0, itemCountNow -timesBoughtGood);
                }

            }
            //MelonLogger.Msg($"ShopBuySaleDialog Initialize price={price} max={maxCount} sale={isSale}");
        }
    }

    [HarmonyPatch(typeof(ShopDescriptionController))]
    public static class ShopDescriptionControllerPatch
    {
        public static ShopGoods CurrentShopGoods { get; set; }

        [HarmonyPostfix]
        [HarmonyPatch("UpdateParam")]
        public static void UpdateParam(ShopDescriptionController __instance, ShopScrollObject shopSelectObject, ItemUIInfo info)
        {
            // Should not trigger while we are trying to sell
            ShopScrollController scrollController = __instance.GetComponentInParent<ShopScrollController>();
            if (scrollController != null && scrollController.IsSellMode)
            {
                return;
            }

            if (shopSelectObject == null)
            {
                //MelonLogger.Error("shopSelectObject null");
                return;
            }
            
            CurrentShopGoods = shopSelectObject.MasterShopGoods;
            
            if (CurrentShopGoods == null)
            {
                //MelonLogger.Error("MasterShopGoods null");
                return;
            }

            //MelonLogger.Msg($"ShopDescriptionControllerPatch: Updating description for ShopGoods {mMasterShopGoods.mId} with item {mMasterShopGoods.mItem}");
            if (CurrentShopGoods.Item == (AllItems.ELabel)Core.ARCHIPELAGO_ITEM_ID)
            {
                //MelonLogger.Msg($"ShopDescriptionControllerPatch: Updating description for Archipelago item {mMasterShopGoods.mId}");
                if (Core.save.TryGetShopItem(new GameLocationID(CurrentShopGoods.mId), out ArchipelagoItem archiItem))
                {
                    if (__instance.mValuablesDescription == null)
                    {
                        //MelonLogger.Error("mValuablesDescription null");
                        return;
                    }

                    //MelonLogger.Msg($"ShopDescriptionControllerPatch: Found Archipelago item {archiItem.name} with {info.MasterBadge.ItemName} for ShopGoods {mMasterShopGoods.mId}");

                    __instance.mValuablesDescription.mItemNameText.text = $"{archiItem.name}";
                    __instance.mValuablesDescription.mFlavorText.text = $"A pin reminiscent of {archiItem.player}'s {archiItem.itemGame}.";

                }
            }

            
        }
    }

}

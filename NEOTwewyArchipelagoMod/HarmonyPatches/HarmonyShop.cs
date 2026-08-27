using HarmonyLib;
using Il2Cpp;
using Il2CppHnLib;
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
        

        [HarmonyPrefix]
        [HarmonyPatch("CreateUIInfoData")]
        public static void Prefix(ShopScrollController __instance, ref int dataID)
        {
            ShopScrollControllerInstance = __instance;
            Initialization.InitializeShopGoods();
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

                    if (archiItem.IsItemFromOurSlot())
                    {
                        AllItems itemObject = MasterDataBase<AllItems>.Get((int)archiItem.id);

                        string atlasPath = UIUtility.GetItemAtlasPath(itemObject);
                        string spriteName = UIUtility.GetItemImageName(itemObject);

                        __instance.mValuablesDescription.mItemImage.m_Sprite = UISpriteBank.Instance.GetSprite(atlasPath, spriteName);
                    }
                    
                }
            }

            
        }
    }

    [HarmonyPatch(typeof(ShopScrollObject))]
    public static class ShopScrollObjectPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetImage")]
        public static void PostSetImage(ItemUIInfo info, ShopScrollObject __instance)
        {
            ShopItemUIInfo shopInfo = info.TryCast<ShopItemUIInfo>();
            if (shopInfo == null) { return; }
            ShopGoods goods = shopInfo.MasterShopGoods;
            if (goods.Item == (AllItems.ELabel)Core.ARCHIPELAGO_ITEM_ID && Core.save.TryGetShopItem(new GameLocationID(goods.mId), out ArchipelagoItem archiItem))
            {
                if (archiItem.IsItemFromOurSlot())
                {
                    AllItems itemObject = MasterDataBase<AllItems>.Get((int)archiItem.id);

                    string atlasPath = UIUtility.GetItemAtlasPath(itemObject);
                    string spriteName = UIUtility.GetItemImageName(itemObject);

                    __instance.mBadgeImage.m_Sprite = UISpriteBank.Instance.GetSprite(atlasPath, spriteName);
                }
            }
        }
    }

}

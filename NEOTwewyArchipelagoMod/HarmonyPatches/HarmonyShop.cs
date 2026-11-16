using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using Il2Cpp;
using Il2CppHnLib;
using Il2CppInterop.Runtime;
using Il2CppMaster;
using Il2CppSteamworks;
using Il2CppUI;
using Il2CppUI.Controller;
using Il2CppUI.MainMenu;
using Il2CppUI.MainMenu.Badge;
using Il2CppUI.Panel;
using Il2CppUI.Shop;
using Il2CppUI.Utility;
using JetBrains.Annotations;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Il2Cpp.SaveDataShop;
using static Il2CppTMPro.SpriteAssetUtilities.TexturePacker;
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
            UISpriteBankPatch.CharacterIconLoader = new UIImageLoaderPersonal();
            Initialization.InitializeShopGoods();
        }

        [HarmonyPrefix]
        [HarmonyPatch("Purchace")]
        public static void Purchase(ShopScrollController __instance, int purchaseCount)
        {
            //!Core.save.getCheckedLocations().Contains((long)__0)
            ShopItemUIInfo shopInfo = __instance.GetSelectUIInfo<ShopItemUIInfo>();
            //if (Core.DEBUG) { MelonLogger.Msg($"ShopGoods ID: {shopInfo.mMasterShopGoods.mId} with {shopInfo.mMasterShopGoods.mItem}"); }

            GameLocationID gamelocationID = new GameLocationID(shopInfo.mMasterShopGoods.mId);
            ArchipelagoLocationID archiLocation = gamelocationID.ToArchipelagoLocation(LocationType.ShopGood);
            Core.save.enqueueLocation(archiLocation);
        }


    }

    [HarmonyPatch(typeof(ShopItemUIInfo))]
    public static class ShopItemUIInfoPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("UpdateParam")]
        public static void MoneyPinShop(ShopItemUIInfo __instance)
        {
            //TODO: Do we want to rework this so it applies for all items?
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

                        ArchipelagoLocationID arch_loc = id.ToArchipelagoLocation(LocationType.ShopGood);
                        Core.save.TryGetArchipelagoItem(arch_loc, out ArchipelagoItem archiItem);
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

            //MelonLogger.Msg($"ShopDescriptionControllerPatch: Updating description for ShopGoods {CurrentShopGoods.mId} with item {CurrentShopGoods.mItem}");
            ArchipelagoLocationID archipelagoLocationID = new GameLocationID(CurrentShopGoods.mId).ToArchipelagoLocation(LocationType.ShopGood);
            if (Core.save.TryGetArchipelagoItem(archipelagoLocationID, out ArchipelagoItem archiItem)) 
            {
                if (ModOptions.shop_hints)
                {
                    //MelonLogger.Msg("Create hint call");
                    archipelagoLocationID.CreateHint();
                }


                //MelonLogger.Msg($"ShopDescriptionControllerPatch: Updating description for Archipelago item {mMasterShopGoods.mId}");
                if (CurrentShopGoods.Item == (AllItems.ELabel)Core.ARCHIPELAGO_ITEM_ID)
                {
                    if (__instance.mValuablesDescription == null)
                    {
                        //MelonLogger.Error("mValuablesDescription null");
                        return;
                    }

                    //MelonLogger.Msg($"ShopDescriptionControllerPatch: Found Archipelago item {archiItem.name} with {info.MasterBadge.ItemName} for ShopGoods {mMasterShopGoods.mId}");
                    __instance.mValuablesDescription.mItemNameText.text = $"{archiItem.name}";
                    __instance.mValuablesDescription.mFlavorText.text = $"A pin reminiscent of {archiItem.player}'s {archiItem.itemGame}.";
                    if(archiItem.IsProgression())
                    {
                        __instance.mValuablesDescription.mFlavorText.text += "\nIt seems important.";
                    }
                    //TODO: Can we consolidate this with the descriptions in HarmonyDialogGet?
                    if(archiItem.itemType == ItemType.SocialNode) {
                        __instance.mValuablesDescription.mFlavorText.text = $"Unlocks their respective node in the Social Network.";
                    } else if(archiItem.itemType == ItemType.VIP)
                    {
                        __instance.mValuablesDescription.mFlavorText.text = $"Increases VIP level for this brand by 1.";
                    } else if(archiItem.itemType == ItemType.AllItems && archiItem.id == NEOTwewyDataManager.DIVE_FP_ID)
                    {
                        __instance.mValuablesDescription.mFlavorText.text = $"Friendship points can be used to unlock skills in the Social Network.";
                    } else if (archiItem.itemType == ItemType.Skill)
                    {
                        Skill skill = MasterDataBase<Skill>.Get((int)archiItem.id);
                        __instance.mValuablesDescription.mFlavorText.text = $"{TextManager.GetText(skill.Info)}";
                    }

                        IconReplacement.SetCustomSprite(archiItem, Sprite => SetSprite(__instance, Sprite));
                }
            }

            
        }

        public static void SetSprite(ShopDescriptionController __instance, Sprite sprite)
        {
            if (__instance.mValuablesDescription != null)
            {
                var image = __instance.mValuablesDescription.mItemImage;
                image.sprite = sprite;
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
            if (goods.Item == (AllItems.ELabel)Core.ARCHIPELAGO_ITEM_ID && Core.save.TryGetArchipelagoItem(new GameLocationID(goods.mId).ToArchipelagoLocation(LocationType.ShopGood), out ArchipelagoItem archiItem))
            {
                IconReplacement.SetCustomSprite(archiItem, Sprite => SetSprite(__instance, Sprite));
            }
        }

        public static void SetSprite(ShopScrollObject __instance, Sprite sprite)
        {
            if (__instance.mBadgeImage != null)
            {
                var image = __instance.mBadgeImage;
                image.sprite = sprite;
            }
        }
    }

}

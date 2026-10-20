using HarmonyLib;
using Il2Cpp;
using Il2CppHnLib;
using Il2CppMaster;
using Il2CppUI.Report;
using Il2CppUI.Utility;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Il2CppTMPro.SpriteAssetUtilities.TexturePacker;
using static Il2CppUI.Battle.BattleResultBadgeController;
using static Il2CppUI.Report.UINoiseReport;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(DropInfo))]
    public static class DropInfoPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("UpdateParam", new Type[] { typeof(EnemyReport) })]
        public static void PreUpdateParam(EnemyReport __0, DropInfo __instance)
        {
            Initialization.InitializeNoiseDrops();
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdateParam", new Type[] { typeof(EnemyReport) })]
        public static void PostUpdateParam(EnemyReport __0, DropInfo __instance)
        {
            for (int i = 0; i < 5; i++)
            { 
                GameLocationID gameID = new(__0.mId + i * 100);
                ArchipelagoLocationID archiLocation = gameID.ToArchipelagoLocation(LocationType.NoiseDrop);

                //MelonLogger.Msg($"{archiLocation} can be? {DropRateCalc.DropRateReached(archiLocation)}");
                if (ModOptions.prevent_out_of_logic && !DropRateCalc.DropRateReached(archiLocation) && Core.save.TryGetArchipelagoItem(archiLocation, out ArchipelagoItem item))
                {
                    //MelonLogger.Msg("NO PIN IMAGE");

                    byte[] data = File.ReadAllBytes("Mods/NeoTwewyArchipelago/Resources/NO_pin.png");

                    Texture2D tex = new Texture2D(2, 2);
                    ImageConversion.LoadImage(tex, data);

                    Sprite sprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f)
                    );

                    DropInfo.DropBadge badge = __instance.mDropBadgeList[i];
                    if (Core.save.IsLocationChecked(archiLocation)) { badge.mBadgeNameText.SetText($"{item.name}"); }
                    badge.mBadgeImage.m_Sprite = sprite;
                    badge.mBadgeImage.enabled = true;
                    continue;
                }
                if (Core.save.IsLocationChecked(archiLocation) && Core.save.TryGetArchipelagoItem(archiLocation, out item))
                {
                    //MelonLogger.Msg(item.locationName);
                    DropInfo.DropBadge badge = __instance.mDropBadgeList[i];
                    badge.mBadgeNameText.SetText($"{item.name}");
                    badge.mBadgeNoText.SetText($"{item.player}");
                    
                    if (item.IsItemFromOurSlot() && item.itemType == ItemType.AllItems)
                    {
                        AllItems itemObject = MasterDataBase<AllItems>.Get((int)item.id);

                        string atlasPath = UIUtility.GetItemAtlasPath(itemObject);
                        string spriteName = UIUtility.GetItemImageName(itemObject);

                        //MelonLogger.Msg($"{atlasPath} atlas -> spriteName {spriteName}");

                        badge.mBadgeImage.m_Sprite = UISpriteBank.Instance.GetSprite(atlasPath, spriteName);
                    } //TODO: Update this for character nodes!!
                }
            }
        }
    }
}

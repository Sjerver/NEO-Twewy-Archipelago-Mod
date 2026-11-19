using HarmonyLib;
using Il2Cpp;
using Il2CppCriAtomDebugDetail;
using Il2CppCysharp.Text;
using Il2CppMaster;
using Il2CppSystem;
using Il2CppSystem.Linq;
using Il2CppUI.Utility;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{

    [HarmonyPatch(typeof(DialogGetUI))]
    public static class PatchDialogGetUI
    {
        //Item of the currently to open dialogGetUI
        public static ArchipelagoItem currentItem = null;

        [HarmonyPrefix]
        [HarmonyPatch("OpenItemGetDialog")]
        public static void PreItemGetDialog(AllItems.ELabel itemID, ref int num, Il2CppSystem.Action onUIEndEvent, Il2CppSystem.Action<DialogGetUI> onEnded)
        {
            if (num < 0) //&& (long)itemID == Core.ARCHIPELAGO_ITEM_ID
            {
                //MelonLogger.Msg($"Received location {-(num)} as Archipelago Item dialogue");  
                ArchipelagoLocationID locationID = new ArchipelagoLocationID(-(num));
                if (Core.save.TryGetArchipelagoItem(locationID, out ArchipelagoItem item))
                {
                    //MelonLogger.Msg($"Storing item {item.name} ({item.id}) [{item.count}] as current");
                    currentItem = item;
                }
                else
                {
                    currentItem = null;
                    
                }
                num = 1;
            }
            else
            {
                //MelonLogger.Msg($"{itemID} and {num} with {onUIEndEvent.Method.Name} and {onEnded.Method.Name}");
                //MelonLogger.Msg($"onUIEndEvent declaring type: {onUIEndEvent.Method.DeclaringType}");
                //MelonLogger.Msg($"onEnded declaring type: {onEnded.Method.DeclaringType}");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("Initialize")]
        public static void PreInitialize(DialogGetUI __instance)
        {
            if (currentItem != null && currentItem.IsItemFromOurSlot())
            {
                if(currentItem.itemType == ItemType.AllItems)
                {
                    __instance.ItemId = (AllItems.ELabel)currentItem.id;
                }
                
                if (currentItem.id != NEOTwewyDataManager.DIVE_FP_ID)
                {
                    //Non FP needs to custom set count! 
                    //MelonLogger.Msg($"Set Count for {currentItem.name} to {currentItem.count}");
                    __instance.NumItem = (int)currentItem.count;
                }
                
            }
        }

        public static void SetSprite(DialogGetUI __instance, Sprite sprite)
        {
            __instance.mItemImage.sprite = sprite;
            if (__instance.mBadge)
            {
                __instance.mBadge.mBadgeImage.sprite = sprite;
            }
            
        }

        [HarmonyPostfix]
        [HarmonyPatch("Initialize")]
        public static void PostInitialize(DialogGetUI __instance)
        {
            //MelonLogger.Msg($"Initializing for {(long)__instance.ItemId}"); 
            //if (__instance.ItemId != (AllItems.ELabel)Core.ARCHIPELAGO_ITEM_ID) return; 
            if (currentItem != null)
            {
                //MelonLogger.Msg($"Changing text for {currentItem.name}"); 
                __instance.SetMainText(currentItem.name);
                __instance.SetSubText($"A pin reminiscent of {currentItem.player}'s {currentItem.itemGame}.");

                //Special behaviour for ArchipelagoItems from our game!

                if (currentItem.IsItemFromOurSlot())
                {
                    IconReplacement.SetCustomSprite(currentItem, sprite => SetSprite(__instance, sprite));

                    //ItemType special SubText Handling?
                    if (currentItem.itemType == ItemType.AllItems)
                    {
                        AllItems allItem = MasterDataBase<AllItems>.Get((int)currentItem.id);
                        __instance.SetSubText(TextManager.GetText(allItem.Info));

                    }
                    else if (currentItem.itemType == ItemType.SocialNode)
                    { 
                        SkillTree skillTree = MasterDataBase<SkillTree>.Get((int)currentItem.id);
                        __instance.SetSubText($"Unlocks their respective node in the Social Network.");
                    }
                    else if (currentItem.itemType == ItemType.VIP)
                    {
                        __instance.SetSubText($"Increases VIP level for this brand by 1.");
                    }
                    else if (currentItem.itemType == ItemType.Skill)
                    {
                        Skill skill = MasterDataBase<Skill>.Get((int)currentItem.id);
                        __instance.SetSubText($"{TextManager.GetText(skill.Info)}");
                    }
                }

            }

            //MelonLogger.Msg($"No current item detected");


            currentItem = null;
        }
    }

}


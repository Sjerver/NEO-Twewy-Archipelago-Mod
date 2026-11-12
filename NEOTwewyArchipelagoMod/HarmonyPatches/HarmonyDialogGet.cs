using HarmonyLib;
using Il2Cpp;
using Il2CppCriAtomDebugDetail;
using Il2CppMaster;
using Il2CppSystem;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{

    [HarmonyPatch(typeof(DialogGetUI))]
    public static class PatchDialogGetUI  
    {
        //Item of the currently to open dialogGetUI
        private static ArchipelagoItem currentItem = null;

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
                    //MelonLogger.Msg($"Storing item {item.name} ({item.id}) as current"); 
                    currentItem = item; 
                } else { 
                    currentItem = null; 
                }
                num = 1;
            } else
            {
                //MelonLogger.Msg($"{itemID} and {num} with {onUIEndEvent.Method.Name} and {onEnded.Method.Name}");
                //MelonLogger.Msg($"onUIEndEvent declaring type: {onUIEndEvent.Method.DeclaringType}");
                //MelonLogger.Msg($"onEnded declaring type: {onEnded.Method.DeclaringType}");
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
            }

            //MelonLogger.Msg($"No current item detected");


            currentItem = null; 
        }
    }

}


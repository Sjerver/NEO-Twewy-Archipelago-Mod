using HarmonyLib;
using Il2Cpp;
using Il2CppComicEvent;
using Il2CppMaster;
using Il2CppScenario;
using Il2CppSteamworks;
using Il2CppUI;
using Il2CppUI.Shop;
using Il2CppUI.Title;
using Il2CppUI.Utility;
using MelonLoader;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Il2Cpp.SaveDataShop;


namespace NEOTwewyArchipelagoMod.HarmonyPatches
{

    [HarmonyPatch(typeof(TitleUI), nameof(TitleUI.OnDecide))]
    public static class PatchOnDecide
    {
        public static bool Prefix(Vector2Int index)
        {
            Initialization.InitializeAll();
            if (index.x == 0 && index.y == 1)
            {//New Game Button
                if (!Core.client.IsConnected)
                {//Not connected, disallow button
                    MelonLogger.Error("Require connection to Archipelago room to create new game.");
                    return false;
                }else if (Core.client.session.RoomState.Seed != Core.save.getSeed())
                {//Need to initialize a new save
                    MelonLogger.MsgDirect(MelonLoader.Logging.ColorARGB.Green,"Reset local archipelago save file!");
                    Core.save.Reset();
                    Core.save.setSeed(Core.client.session.RoomState.Seed);
                    _ = Core.GetArchipelagoData();
                    while (ArchipelagoData.DataLoaded)
                    {
                        //Add code to display something here maybe?
                    }
                    Core.syncState = SyncState.Ready;
                    Core.save.UpdateSlotData(Core.client.LoginInfo.SlotData); 
                }
                else
                {// Just start local save
                    Core.syncState = SyncState.Ready;
                }
            }

                return true;
        }
    }

    //Might be useful to find last battle IsLastBossBattleScenario

    [HarmonyPatch(typeof(FieldManager))]
    public static class FieldManagerPatch
    {
        [HarmonyPatch("DiveBattle")]
        [HarmonyPrefix]
        public static void DiveBattlePrefix()
        {
            Core.rewardSafety.assumeBattle = true;
            //MelonLogger.Msg("Assuming battle due to DiveBattle being called");
        }

        [HarmonyPatch("ScenarioBattle")]
        [HarmonyPrefix]
        public static void ScenarioBattlePrefix()
        {
            Core.rewardSafety.assumeBattle = true;
            //MelonLogger.Msg("Assuming battle due to ScenarioBattle being called");
        }
    }
}

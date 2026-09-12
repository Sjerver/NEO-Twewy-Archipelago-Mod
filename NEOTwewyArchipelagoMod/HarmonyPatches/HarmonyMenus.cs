using HarmonyLib;
using Il2Cpp;
using Il2CppComicEvent;
using Il2CppMaster;
using Il2CppUI.MainMenu;
using Il2CppUI.Option;
using Il2CppUI.Shop;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(MainMenuUI))]
    public static class PatchMainMenuUI
    {
        [HarmonyPatch("OnCloseMainMenu")]
        [HarmonyPostfix]
        public static void OnCloseMainMenu()
        {Core.rewardSafety.mainMenuOpen = false;}

        [HarmonyPatch("OnOpenMainMenu")]
        [HarmonyPostfix]
        public static void OnOpenMainMenu()
        {Core.rewardSafety.mainMenuOpen = true;}
    }

    [HarmonyPatch(typeof(ShopUI))]
    public static class PatchShopUI
    {
        [HarmonyPatch("OnAwake")]
        [HarmonyPostfix]
        public static void OnAwake()
        { Core.rewardSafety.shopMenuOpen = true;}

        [HarmonyPatch("OnDelete")]
        [HarmonyPostfix]
        public static void OnDelete()
        { Core.rewardSafety.shopMenuOpen = false;}
    }

    [HarmonyPatch(typeof(MainOptionUI))]
    public static class PatchMainOptionUI
    {
        [HarmonyPatch("OpenMainOption")]
        [HarmonyPostfix]
        public static void OpenMainOption() 
        { //MelonLogger.Msg("MainOptionUI opened");
            Core.rewardSafety.optionMenuOpen = true;}

        [HarmonyPatch("EndMainOption")]
        [HarmonyPostfix]
        public static void EndMainOption() 
        { //MelonLogger.Msg("MainOptionUI closed");
            Core.rewardSafety.optionMenuOpen = false;}
    }

    //TODO: Add replay menu
}

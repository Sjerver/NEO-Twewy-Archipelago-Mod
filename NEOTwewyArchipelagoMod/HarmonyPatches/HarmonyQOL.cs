using HarmonyLib;
using Il2Cpp;
using Il2CppComicEvent;
using Il2CppMaster;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(ComicEventManager), "UpdateSkipFlag")]
    public static class PatchSkipFlag
    {
        public static void Postfix(ComicEventManager __instance)
        {
            //MelonLogger.Msg($"Before: {__instance.m_IsSkipExecutable}");
            //Always make sure you can fastword in dialogue scenes
            __instance.m_IsSkipExecutable = true;

            //MelonLogger.Msg($"After: {__instance.m_IsSkipExecutable}");
        }
    }

    [HarmonyPatch(typeof(ScenarioMovieManager), nameof(ScenarioMovieManager.PlayMovie))]
    public static class PatchScenarioMovieManagerPlayMovie
    {
        public static void Prefix(ref bool inEnableSkip)
        {
            //MelonLogger.Msg("Movie starting, forcing skip enabled.");
            inEnableSkip = true;
        }
    }
}

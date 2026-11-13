using HarmonyLib;
using Il2Cpp;
using Il2CppComicEvent;
using Il2CppCustomPlayables;
using Il2CppMaster;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Il2CppNoise.Pig.AIPigDivision.Param;

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

    [HarmonyPatch]
    //Patch by https://github.com/aEnigmatic/NeoPlus/tree/main
    public static class PatchComicSpeed
    {
        private static void SetComicEventSpeed()
        {
            //MelonLogger.Msg($"Setting Comic Event Speed from {ComicEventManager.AUTO_SPEED_SKIP} to {SkipSpeed}");
            PlayableDirectorEx.DefaultSpeed = ModOptions.cutscene_skip_speed;
            ComicEventManager.AUTO_SPEED_SKIP = (float)ModOptions.cutscene_skip_speed;
            ComicEventManager.Instance.ApplyComicEventSpeed(ModOptions.cutscene_skip_speed);
        }

        private static void ResetComicEventSpeed()
        {
            PlayableDirectorEx.DefaultSpeed = 10.0f;
            ComicEventManager.AUTO_SPEED_SKIP = 10.0f;
            ComicEventManager.Instance.ApplyComicEventSpeed(10.0f);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ComicEventDirector), "OnPlayBegin")]
        public static void ComicEventDirector_OnPlayBegin(ComicEventDirector __instance)
        {
            //MelonLogger.Msg("ComicEventDirector OnPlayBegin, setting speed to " + SkipSpeed);
            SetComicEventSpeed();

            var root = __instance.GetMostParent() ?? __instance;

            root.CustomSpeed(ModOptions.cutscene_skip_speed);
            root.EnableAutomaticReproducing();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ComicEventDirector), nameof(ComicEventDirector.OnPlayEnd))]
        public static void ComicEventDirector_OnPlayEnd(ComicEventDirector __instance)
            => ResetComicEventSpeed();
    }
    
    //TODO: Overworld comic is fine, but instead try to make it so the speed increase only applies to the "Skip speed" not general stuff
    //Otherwise we get audio issues and stuff
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

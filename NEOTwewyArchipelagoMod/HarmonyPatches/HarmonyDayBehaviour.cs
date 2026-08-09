using Il2Cpp;
using Il2CppMaster;
using MelonLoader;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    /*
     * Behaviour when a regular day ends!
     */
    [HarmonyPatch(typeof(SaveDataField), "SetNextScenarioDateDay")]
    public static class PatchSetNextScenarioDateDay
    {
        public static void Prefix()
        {
            //MelonLogger.Msg($"Finished day {SaveLoadController.Get<SaveDataField>().GetNewestDateDay()}");
            if (Core.furthestDayReached > 0)
            {
                //We set to furthestDayReached -1 because this method increases that by 1 naturally
                SaveLoadController.Get<SaveDataField>().SetScenarioDateDay(Core.furthestDayReached - 1);
            }


        }

        public static void Postfix()
        {
            //MelonLogger.Msg($"Start day {SaveLoadController.Get<SaveDataField>().GetNewestDateDay()}");
            if (Core.furthestDayReached == 0 && SaveLoadController.Get<SaveDataField>().GetNewestDateDay() == 1)
            { // On day 0 manually increase since there is no custom end of chapter handling
                Core.furthestDayReached++;
            }
        }
    }

    [HarmonyPatch(typeof(SaveDataField), "SetScenarioFlag")]
    public static class PatchSetScenarioFlag
    {
        public static void Prefix(int __0, bool __1)
        {
            if (Core.DEBUG) { MelonLogger.Msg($"Set Scenario Flag {(Scenario.EName)__0} to {__1}"); }

            Core.CheckEndOfChapterReward((Scenario.EName)__0, __1);
        }
    }
    [HarmonyPatch(typeof(SaveDataField), "SetScenarioFlagData")]
    public static class PatchSetScenarioFlagData
    {
        public static void Prefix(int __0, bool __1)
        {
            if (Core.DEBUG) { MelonLogger.Msg($"Set ScenarioFlagData index {__0} name {ScenarioFlagList.flagNamesFromSaveIndex[__0]} to {__1}"); }

            Core.CheckEndOfChapterReward(ScenarioFlagList.flagNamesFromSaveIndex[__0], __1);

        }
    }
}

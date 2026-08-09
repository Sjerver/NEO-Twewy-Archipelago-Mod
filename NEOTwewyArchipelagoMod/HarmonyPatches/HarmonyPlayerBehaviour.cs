using HarmonyLib;
using Il2Cpp;
using Il2CppMaster;
using Il2CppScenario;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(SaveDataPlayerTeam), "JoinMember")]
    public static class PatchJoinMember
    {
        public static void Prefix(int __0, BattlePlayer.ELabel __1)
        {
            //Triggered for Minamimoto Day 3
            //Did not trigger revisiting w2d7, but did trigger on normal visit

            //MelonLogger.Msg($"Member joined index {__0} and Label {__1}");
            if (CustomEventData.memberToRewardID.TryGetValue(__1, out GameLocationID rewardID))
            {
                Core.queueCustomLocation(ArchipelagoData.GetArchipelagoLocation(rewardID, LocationType.ScenarioReward));
            }

        }
    }

    [HarmonyPatch(typeof(AddCharacterExtension), "ScenarioJoinCharacter")]
    class PatchScenarioJoinCharacter
    {
        public static void Prefix(BattlePlayer.ELabel __0, bool __1, ref bool __2)
        {
            //Setting it to false doesn't do anything to minamimoto joining
            //This Method does not trigger when replaying w2d7
            if (Core.DEBUG) { MelonLogger.Msg($"Prefix ScenarioJoinCharacter with playerID {__0} checkSystem {__1} and isNewestDateDay {__2}"); }
            ;
            //__2 = false;
        }

        public static void Postfix(BattlePlayer.ELabel __0, bool __1, ref bool __2)
        {

            if (Core.DEBUG)
            {
                MelonLogger.Msg($"Postfix ScenarioJoinCharacter with playerID {__0} checkSystem {__1} and isNewestDateDay {__2}");
            }
        }
    }
}

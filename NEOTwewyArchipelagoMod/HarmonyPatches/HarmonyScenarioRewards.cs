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
    [HarmonyPatch(typeof(FieldManager), "ReserveScenarioReward")]
    public static class PatchReserveScenarioReward
    {
        public static bool Prefix(ScenarioRewards.ELabel __0, int __1)
        {
            if ((long)__0 < ArchipelagoData.ARCHI_RECEIVE_START && !Core.save.getCheckedLocations().Contains((long)__0))
            {//The server needs to be alerted about the location
                if (Core.DEBUG) { MelonLogger.Msg($"Queue location {__0} to send to server"); }
                //Tell the server we got the location
                Core.save.enqueueLocation((long)__0);
                Core.save.addCheckedLocation((long)__0);

                return false; //Don't call actual method to give the actual reward
            }
            else if (Core.save.getCheckedLocations().Contains((long)__0))
            { // Location got already checked once
                return false;
            }

            return true; //Item received from server so give it normally!
        }


        public static void Postfix(ScenarioRewards.ELabel __0, int __1)
        {
            //MelonLogger.Msg( $"ReserveScenarioReward called: ID={__0}, Index={__1}");

            ScenarioRewards scenarioRewards = MasterDataBase<ScenarioRewards>.Get((int)__0);

            if (Core.DEBUG) { MelonLogger.Msg($"Gave Item {scenarioRewards.mReward1st} x {scenarioRewards.mReward1stCount}"); }

            //Auto Save that after we get an item
            FieldManager.Instance.CallReserveAutoSave();

        }
    }
}

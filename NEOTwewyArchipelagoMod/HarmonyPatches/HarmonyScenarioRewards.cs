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
    [HarmonyPatch(typeof(FieldManager), "ReserveScenarioReward")]
    public static class PatchReserveScenarioReward
    {
        public static bool Prefix(ScenarioRewards.ELabel __0, int __1)
        {
            GameLocationID rewardID = new GameLocationID((long)__0);
            ArchipelagoLocationID locationID = rewardID.ToArchipelagoLocation(LocationType.ScenarioReward);

            if ((long)__0 < ArchipelagoData.ARCHI_RECEIVE_START && !Core.save.IsLocationChecked(locationID))
            {//The server needs to be alerted about the location
                //if (Core.DEBUG) { MelonLogger.Msg($"Queue location {__0} to send to server"); }
                //Tell the server we got the location


                Core.save.enqueueLocation(locationID);


                //if (Core.save.TryGetArchipelagoItem(locationID, out ArchipelagoItem item))
                //{ 
                //    if (item.id == Core.ARCHIPELAGO_ITEM_ID)
                //    { //If the item is an archipelago item, we get from a vanilla reward. We want to still get a pop-up for what item we are sending
                //        Core.archiItemDisplayQueue.Enqueue(item);
                //    }
                //}

                return false; //Don't call actual method to give the actual reward
            }
            else if (Core.save.IsLocationChecked(locationID))
            { // Location got already checked once
                return false;
            }

            return true; //Item received from server so give it normally!
        }


        public static void Postfix(ScenarioRewards.ELabel __0, int __1)
        {
            //MelonLogger.Msg( $"ReserveScenarioReward called: ID={__0}, Index={__1}");

            ScenarioRewards scenarioRewards = MasterDataBase<ScenarioRewards>.Get((int)__0);

            //if (Core.DEBUG) { MelonLogger.Msg($"Gave Item {scenarioRewards.mReward1st} x {scenarioRewards.mReward1stCount}"); }

            //Auto Save that after we get an item
            FieldManager.Instance.CallReserveAutoSave();

        }
    }

}


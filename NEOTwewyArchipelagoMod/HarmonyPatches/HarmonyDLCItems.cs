using HarmonyLib;
using Il2Cpp;
using Il2CppMaster;
using Il2CppUI.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(DownLoadContentManeger))]
    public static class DownLoadContentManegerPatch
    {

        [HarmonyPrefix]
        [HarmonyPatch("AddContentItems")]
        public static bool AddContentItems(DLC.ELabel label)
        {
            if(ModOptions.dlc_options == DLCOption.Vanilla) { return true; }
            if(ModOptions.dlc_options == DLCOption.Disable) { return false; }

            long group = (long)(label - 1) * 10;

            for(int i = 1; i < 6; i++)
            {
                GameLocationID rewardID = new GameLocationID(group+i);
                ArchipelagoLocationID locationID = rewardID.ToArchipelagoLocation(LocationType.DLC);

                if(Core.save.TryGetArchipelagoItem(locationID, out ArchipelagoItem item) && !Core.save.IsLocationChecked(locationID))
                {
                    Core.save.enqueueLocation(locationID);
                    //Items are always send by server so just need this requirement
                    //if (item.id == Core.ARCHIPELAGO_ITEM_ID)
                    //{
                    //    Core.archiItemDisplayQueue.Enqueue(item);
                    //}
                }
            }

            return false;
        }
    }
}


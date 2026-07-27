using Archipelago.MultiClient.Net.Models;
using MelonLoader;

namespace NEOTwewyArchipelagoMod
{
    public class ArchipelagoData
    {
        public static long archiReceiveIDStart = 1000005;
        public static long archiReceiveID = 1000005;

        public static bool DataLoaded { get; set; } = false;
        
        //RewardID, ItemID
        public static Dictionary<long, long> ScenarioRewardsDict = new();
        //itemID, RewardID
        public static Dictionary<long, long> ReceivableRewards = new();

        public static void LoadArchipelagoLocations(Dictionary<long,ScoutedItemInfo> locationData)
        {
            foreach(KeyValuePair<long, ScoutedItemInfo> entry in locationData)
            {//For all locations in our game we received from the server
                if (entry.Value.ItemGame == Core.GAME_NAME)
                {//If the item is in our game, we can use the normal itemID in the normal reward
                    if (Core.DEBUG) { MelonLogger.Msg($"Location {entry.Value.LocationDisplayName} ({entry.Key}) has {entry.Value.ItemName} ({entry.Value.ItemId})"); }
                    ScenarioRewardsDict.Add(entry.Key, entry.Value.ItemId);
                }
                else
                {//If the item is not originally from our game we need to use the archipelago replacement item
                    if(Core.DEBUG) { MelonLogger.Msg($"Location {entry.Value.LocationDisplayName} ({entry.Key}) has {entry.Value.ItemName} ({entry.Value.ItemId})"); }
                    ScenarioRewardsDict.Add(entry.Key, Core.ARCHIPELAGO_ITEM_ID);
                }
                        
            }
        }

        public static void LoadStaticLocations()
        {
            
            foreach(KeyValuePair<string, NEOTwewyItemData> entry in NEOTwewyDataManager.item_data)
            {//Assemble a dictionary for all items we should be able to receive from archipelago
                ReceivableRewards.Add(entry.Value.id, archiReceiveID);
                archiReceiveID++;
            }

        }
    }
}

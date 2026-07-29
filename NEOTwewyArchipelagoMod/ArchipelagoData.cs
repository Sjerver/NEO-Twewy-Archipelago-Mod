using Archipelago.MultiClient.Net.Models;
using Harmony;
using MelonLoader;

namespace NEOTwewyArchipelagoMod
{
    public class ArchipelagoData
    {
        public static long archiReceiveIDStart = 1000005;
        public static long archiReceiveID = 1000005;

        public static bool DataLoaded { get; set; } = false;

        //itemID, RewardID
        public static Dictionary<long, NEOTwewyItemData> ReceivableRewards = new();

        public static void AssembleNewRewards()
        {

            foreach (KeyValuePair<string, NEOTwewyItemData> entry in NEOTwewyDataManager.item_data)
            {//Assemble a dictionary for all items we should be able to receive from archipelago
                entry.Value.reward_ID = (int)archiReceiveID;
                ReceivableRewards.Add(entry.Value.id, entry.Value);
                archiReceiveID++;
            }

        }
    }

    public class ArchipelagoItem
    {
        public long id { get; set; }
        public string name { get; set; }
        public long locationID { get; set; }
        public string locationName { get; set; }
        public long archipelagoID { get; set; }
        public string itemGame { get; set; }


        public ArchipelagoItem(long id, string name, long locationID, string locationName, long archipelagoID, string itemGame)
        {
            this.id = id;
            this.name = name;
            this.locationID = locationID;
            this.locationName = locationName;
            this.archipelagoID = archipelagoID;
            this.itemGame = itemGame;
        }    

    }
        

}

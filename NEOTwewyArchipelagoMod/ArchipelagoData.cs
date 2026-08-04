using Archipelago.MultiClient.Net.Models;
using Harmony;
using Il2CppMaster;
using MelonLoader;
using NEOTwewyArchipelagoMod;

namespace NEOTwewyArchipelagoMod
{
    public class ArchipelagoData
    {
        //The starting ID for the inserted rewards so we can receive items from archipelago, and the current ID we are at
        public static long ARCHI_RECEIVE_START = 9000000;
        private static long archiReceiveID = 9000000;

        public static bool DataLoaded { get; set; } = false;

        //itemID, RewardID
        public static Dictionary<long, NEOTwewyItemData> ReceivableRewards = new();
        /// <summary>
        /// Modifier applied additively to make ShopGoods ids unique in archipelago world
        /// </summary>
        public static long SHOP_LOCATION_MODIFIER = 200000;

        public static void AssembleNewRewards()
        {

            foreach (KeyValuePair<string, NEOTwewyItemData> entry in NEOTwewyDataManager.item_data)
            {//Assemble a dictionary for all items we should be able to receive from archipelago
                entry.Value.reward_ID = (int)archiReceiveID;
                ReceivableRewards.Add(entry.Value.arch_id, entry.Value);
                archiReceiveID++;
            }

        }
    }

    public class ArchipelagoItem
    {
        /// <summary>
        /// The id of the item received from archipelago in game
        /// May be the archipelago replacement id as well
        /// </summary>
        public long id { get; set; }
        public string name { get; set; }
        public long locationID { get; set; }
        public string locationName { get; set; }
        /// <summary>
        /// The id of the item in the archipelago system
        /// </summary>
        public long archipelagoID { get; set; }
        public string itemGame { get; set; }
        /// <summary>
        /// How many of the actual item this item represents
        /// Example: 5 FP is only 1 item in the archipelago world
        /// </summary>
        public long count { get; set; }
        public string player { get; set; }


        public ArchipelagoItem(long id, string name, long locationID, string locationName, long archipelagoID, string itemGame, long count, string player)
        {
            this.id = id;
            this.name = name;
            this.locationID = locationID;
            this.locationName = locationName;
            this.archipelagoID = archipelagoID;
            this.itemGame = itemGame;
            this.count = count;
            this.player = player;
        }    

    }
        

}

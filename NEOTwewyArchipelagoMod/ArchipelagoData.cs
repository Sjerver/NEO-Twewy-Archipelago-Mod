using Archipelago.MultiClient.Net.Models;
using Harmony;
using Il2Cpp;
using Il2CppMaster;
using Il2CppSteamworks;
using MelonLoader;
using NEOTwewyArchipelagoMod;
using Newtonsoft;
using Newtonsoft.Json;
using System.ComponentModel;
using UnityEngine.XR;
using static Il2Cpp.SaveDataDive;

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
        public static long SCENARIO_LOCATION_MODIFIER = 1000000;
        public static long DIVE_LOCATION_MODIFIER = 300000;
        public static long SHOP_LOCATION_MODIFIER = 200000;
        

        public static Dictionary<SaveDataDive.EPrizeStatus, long> DIVE_RANK_MODIFIER = new()
        {
            { SaveDataDive.EPrizeStatus.Gold, 0 },
            { SaveDataDive.EPrizeStatus.Silver, 10 },
            { SaveDataDive.EPrizeStatus.Bronze, 20 },
        };

        public static LocationType GetLocationType(ArchipelagoLocationID aId)
        {
            long id = aId.Value;
            if(id >= ARCHI_RECEIVE_START)
            {
                return LocationType.ArchipelagoReceive;
            }
            else if (id >= SCENARIO_LOCATION_MODIFIER)
            {
                return LocationType.ScenarioReward;
            }
            else if (id >= DIVE_LOCATION_MODIFIER)
            {
                return LocationType.DiveReward;
            }
            else if (id >= SHOP_LOCATION_MODIFIER)
            {
                return LocationType.ShopGood;
            }
            else
            {
                return LocationType.Vanilla;
            }
        }

        /// <summary>
        /// Returns the vanilla game id of an location from archipelago
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static GameLocationID GetGameLocation(ArchipelagoLocationID id)
        {
            long gameID = id.Value;
            LocationType type = GetLocationType(id);
            switch (type)
            {
                case LocationType.ArchipelagoReceive:
                    break; //These are the same!
                case LocationType.ScenarioReward:
                    gameID = gameID - SCENARIO_LOCATION_MODIFIER;
                    break;
                case LocationType.ShopGood:
                    gameID = gameID - SHOP_LOCATION_MODIFIER;
                    break;
                case LocationType.DiveReward:
                    gameID = gameID - DIVE_LOCATION_MODIFIER;
                    break;
                default:
                    break;
            }
            return new GameLocationID(gameID);
        }

        /// <summary>
        /// Returns the archipelago location id from a vanilla location id.
        /// </summary>
        /// <param name="rawID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ArchipelagoLocationID GetArchipelagoLocation(GameLocationID rawID, LocationType type)
        {
            long archiID = rawID.Value;
            switch (type)
            {
                case LocationType.ArchipelagoReceive:
                    break; //These are the same!
                case LocationType.ScenarioReward:
                    archiID = archiID + SCENARIO_LOCATION_MODIFIER;
                    break;
                case LocationType.ShopGood:
                    archiID = archiID + SHOP_LOCATION_MODIFIER;
                    break;
                case LocationType.DiveReward:
                    archiID = archiID + DIVE_LOCATION_MODIFIER;
                    break;
                default:
                    break;
            }
            return new ArchipelagoLocationID(archiID);
        }


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
        public ArchipelagoLocationID locationID { get; set; }
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


        public ArchipelagoItem(long id, string name, ArchipelagoLocationID locationID, string locationName, long archipelagoID, string itemGame, long count, string player)
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
        
        public bool IsItemFromOurSlot()
        {
            return itemGame == Core.GAME_NAME && player == Config.Data.slotName;
        }

    }

    public enum LocationType
    {
        Vanilla,
        ArchipelagoReceive,
        ScenarioReward,
        ShopGood,
        DiveReward
    }

    [TypeConverter(typeof(GameLocationIDTypeConverter))]
    [JsonConverter(typeof(GameLocationIDConverter))]
    public readonly record struct GameLocationID(long Value){

        public ArchipelagoLocationID ToArchipelagoLocation(LocationType type) => ArchipelagoData.GetArchipelagoLocation(this, type);
    }

    [TypeConverter(typeof(ArchipelagoLocationIDTypeConverter))]
    [JsonConverter(typeof(ArchipelagoLocationIDConverter))]
    public readonly record struct ArchipelagoLocationID(long Value){
        public GameLocationID ToGameLocation() => ArchipelagoData.GetGameLocation(this);

        public LocationType GetLocationType() => ArchipelagoData.GetLocationType(this);
    }
}

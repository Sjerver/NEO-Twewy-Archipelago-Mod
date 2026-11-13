using Archipelago.MultiClient.Net.Enums;
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
        public static long ARCHI_RECEIVE_START = 9_000_000;
        private static long archiReceiveID = 9_000_000;

        public static bool DataLoaded { get; set; } = false;

        //ArchipelagoItemID, RewardID
        public static Dictionary<long, NEOTwewyItemData> ReceivableRewards = new();

        public static long DLC_LOCATION_MODIFIER = 1_900_000;
        public static long SCENARIO_LOCATION_MODIFIER   = 1_000_000;
        public static long SKILL_LOCATION_MODIFIER      = 600_000;
        public static long SOCIAL_NODE_LOCATION_MODIFIER = 500_000;
        public static long PIG_DROP_MODIFIER            = 410_000;
        public static long NOISE_DROP_MODIFIER          = 400_000;
        public static long DIVE_LOCATION_MODIFIER       = 300_000;
        public static long VIP_LOCATION_MODIFIER        = 210_000;
        public static long SHOP_LOCATION_MODIFIER       = 200_000;
        

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
            else if (id >= DLC_LOCATION_MODIFIER)
            {
                return LocationType.DLC;
            }
            else if (id >= SCENARIO_LOCATION_MODIFIER)
            {
                return LocationType.ScenarioReward;
            }
            else if (id >= SKILL_LOCATION_MODIFIER)
            {
                return LocationType.Skill;
            }
            else if (id >= SOCIAL_NODE_LOCATION_MODIFIER)
            {
                return LocationType.SocialNode;
            }
            else if (id >= PIG_DROP_MODIFIER)
            {
                return LocationType.PigDrop;
            }
            else if (id >= NOISE_DROP_MODIFIER)
            {
                return LocationType.NoiseDrop;
            }
            else if (id >= DIVE_LOCATION_MODIFIER)
            {
                return LocationType.DiveReward;
            }
            else if (id >= VIP_LOCATION_MODIFIER)
            {
                return LocationType.VIP;
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
                case LocationType.DLC:
                    gameID = gameID - DLC_LOCATION_MODIFIER;
                    break;
                case LocationType.ScenarioReward:
                    gameID = gameID - SCENARIO_LOCATION_MODIFIER;
                    break;
                case LocationType.Skill:
                    gameID = gameID - SKILL_LOCATION_MODIFIER;
                    break;
                case LocationType.SocialNode:
                    gameID = gameID - SOCIAL_NODE_LOCATION_MODIFIER;
                    break;
                case LocationType.ShopGood:
                    gameID = gameID - SHOP_LOCATION_MODIFIER;
                    break;
                case LocationType.VIP:
                    gameID = gameID - VIP_LOCATION_MODIFIER;
                    break;
                case LocationType.DiveReward:
                    gameID = gameID - DIVE_LOCATION_MODIFIER;
                    break;
                case LocationType.NoiseDrop:
                    gameID = gameID - NOISE_DROP_MODIFIER;
                    break;
                case LocationType.PigDrop:
                    //gameID = gameID - PIG_DROP_MODIFIER;
                    if (PigLocator.TryGetPigDataId(id, out GameLocationID newGameID)) return newGameID;
                    throw new KeyNotFoundException( $"No game location mapping found for pig location {id.Value}");
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
                case LocationType.DLC:
                    archiID = archiID + DLC_LOCATION_MODIFIER;
                    break;
                case LocationType.ScenarioReward:
                    archiID = archiID + SCENARIO_LOCATION_MODIFIER;
                    break;
                case LocationType.Skill:
                    archiID += SKILL_LOCATION_MODIFIER;
                    break;
                case LocationType.SocialNode:
                    archiID += SOCIAL_NODE_LOCATION_MODIFIER;
                    break;
                case LocationType.ShopGood:
                    archiID = archiID + SHOP_LOCATION_MODIFIER;
                    break;
                case LocationType.VIP:
                    archiID = archiID + VIP_LOCATION_MODIFIER;
                    break;
                case LocationType.DiveReward:
                    archiID = archiID + DIVE_LOCATION_MODIFIER;
                    break;
                case LocationType.NoiseDrop:
                    archiID = archiID + NOISE_DROP_MODIFIER;
                    break;
                case LocationType.PigDrop:
                    //archiID = archiID - PIG_DROP_MODIFIER;
                    if (PigLocator.TryGetArchiId(rawID, out ArchipelagoLocationID newArchiID)) return newArchiID;
                    throw new KeyNotFoundException($"No game location mapping found for pig location {rawID.Value}");
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

        public ItemType itemType { get; set; }

        public ItemFlags flags { get; set; } = ItemFlags.None;

        [JsonConstructor]
        public ArchipelagoItem(long id, string name, ArchipelagoLocationID locationID, string locationName, long archipelagoID, string itemGame, long count, string player, ItemType type)
        {
            this.id = id;
            this.name = name;
            this.locationID = locationID;
            this.locationName = locationName;
            this.archipelagoID = archipelagoID;
            this.itemGame = itemGame;
            this.count = count;
            this.player = player;
            this.itemType = type;

        }

        public ArchipelagoItem(long id, ScoutedItemInfo info)
        {
            long itemCount = 1;
            ItemType itemType = ItemType.OtherGame;
            if (info.ItemGame == Core.GAME_NAME && info.Player.Name == Config.Data.slotName)
            {
                itemCount = NEOTwewyDataManager.item_data[info.ItemName].count;
                itemType = NEOTwewyDataManager.item_data[info.ItemName].type;
            }

            this.id = id;
            name = info.ItemName;
            locationID = new ArchipelagoLocationID(info.LocationId);
            locationName = info.LocationName;
            archipelagoID = info.ItemId;
            itemGame = info.ItemGame;
            count = itemCount;
            player = info.Player.Name;
            this.itemType = itemType;
            flags = info.Flags;
        }
        /* The difference between ItemInfo and ScoutedItemInfo is:
         * ItemInfo: Player is the one the item is LOCATED IN
         * ScoutedItemInfo: Player is the one the item is FOR
         */
        public ArchipelagoItem(PendingItem pendingItem)
        {
            ItemInfo info = pendingItem.Item;
            long id = ArchipelagoData.ReceivableRewards[pendingItem.Item.ItemId].id;

            long itemCount = 1;
            ItemType itemType = ItemType.OtherGame;
            //Pending items are always for our game
            itemCount = NEOTwewyDataManager.item_data[info.ItemName].count;
            itemType = NEOTwewyDataManager.item_data[info.ItemName].type;


            this.id = id;
            name = info.ItemName;
            locationID = new ArchipelagoLocationID(info.LocationId);
            locationName = info.LocationName;
            archipelagoID = info.ItemId;
            itemGame = info.ItemGame;
            count = itemCount;
            player = Config.Data.slotName;
            this.itemType = itemType;
            flags = info.Flags;
        }

        public override string ToString()
        {
            return $"{id}/{archipelagoID}: {name} Ct:{count} [{locationID} {locationName}] [{itemGame} {player}] {itemType}";
        }


        public bool IsItemFromOurSlot()
        {
            return itemGame == Core.GAME_NAME && player == Config.Data.slotName;
        }

    }

    public enum ItemType
    {
        OtherGame,
        AllItems,
        Skill,
        SocialNode,
        VIP
    }

    public enum LocationType
    {
        Vanilla,
        ArchipelagoReceive,
        ScenarioReward,
        ShopGood,
        DiveReward,
        NoiseDrop,
        Skill,
        PigDrop,
        DLC,
        SocialNode,
        VIP
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

        public bool TryGetItem(out ArchipelagoItem item)
        {
            return Core.save.TryGetArchipelagoItem(this, out item);
        }
    }
}

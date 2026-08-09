using Archipelago.MultiClient.Net.Models;
using Harmony;
using Il2CppHnLib;
using Il2CppSteamworks;
using Il2CppSystem.Linq;
using JetBrains.Annotations;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod
{
    public class ModSave
    {
        private string SavePath{ get; set; }

        private ModSaveData Data { get; set; }

        public ModSave ()
        {
            Data = new ModSaveData();
            SavePath = Path.Combine(MelonEnvironment.UserDataDirectory, "NEOTwewyArchipelagoSave.json");
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                try
                {
                    Data = JsonConvert.DeserializeObject<ModSaveData>(json);
                } catch (Exception e)
                {
                    MelonLogger.Error(e);
                    MelonLogger.MsgDirect(MelonLoader.Logging.ColorARGB.Red, "Save could not be loaded, save file is being reset");
                    Reset();
                }
               
            }
            Save();
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(SavePath, json);
        }

        public void Reset() {
            Data = new ModSaveData();
            Save();
        }

        public string getSeed() { return Data.Seed; }
        public void setSeed(string seed) { Data.Seed = seed; Save(); }

        public bool getGoalAchieved(){ return Data.goalAchieved;}
        public void setGoalAchieved(bool goalAchieved) { Data.goalAchieved = goalAchieved; Save(); }

        public long getLastItemIndex(){return Data.LastItemIndex;}
        public void setLastItemIndex(long LastItemIndex) { Data.LastItemIndex = LastItemIndex; Save(); }

        public HashSet<ArchipelagoLocationID> getCheckedLocations() { return Data.checkedLocations; }

        public bool TryGetArchipelagoItem(ArchipelagoLocationID locationID, out ArchipelagoItem item)
        {
            GameLocationID gameLocation = locationID.ToGameLocation();
            LocationType type = locationID.GetLocationType();
            
            if (type == LocationType.ShopGood)
            {
                return TryGetShopItem(gameLocation, out item);
            }
            else if (type == LocationType.ScenarioReward)
            {
                return TryGetScenarioRewardItem(gameLocation, out item);
            }

            item = null;
            return false;
        }
        
        public bool TryGetShopItem(GameLocationID locationID, out ArchipelagoItem item)
        {
            return Data.shopLocationsMapping.TryGetValue(locationID, out item);
        }

        public bool TryGetScenarioRewardItem(GameLocationID locationID, out ArchipelagoItem item)
        {
            return Data.scenarioLocationsMapping.TryGetValue(locationID, out item);
        }
        public int getPendingLocationSize()
        {
            return Data.pendingLocations.Count;
        }

        public int getRewardQueueSize()
        {
            return Data.rewardQueue.Count;
        }

        public  void enqueueLocation(ArchipelagoLocationID location)
        {
            if (Core.syncState == SyncState.WrongSeed) { return; }
            Data.pendingLocations.Enqueue(location);
            Save();
        }

        public void enqueueReward(QueuedReward reward)
        {
            if (Core.syncState == SyncState.WrongSeed) { return; }
            Data.rewardQueue.Enqueue(reward);
            Save();
        }

        public void addCheckedLocation(ArchipelagoLocationID location)
        {
            if (Core.syncState == SyncState.WrongSeed) { return; }
            Data.checkedLocations.Add(location);
            Save();
        }

        public bool IsLocationChecked(ArchipelagoLocationID location)
        {
            return Data.checkedLocations.Contains(location);
        }

        public ArchipelagoLocationID dequeueLocation() { return Data.pendingLocations.Dequeue(); }

        public QueuedReward dequeueReward() { return Data.rewardQueue.Dequeue(); }

        public void SaveArchipelagoLocations(Dictionary<long, ScoutedItemInfo> locationData)
        {
            foreach (KeyValuePair<long, ScoutedItemInfo> entry in locationData)
            {//For all locations in our game we received from the server
                try
                {
                    long itemID;
                    if (entry.Value.ItemGame == Core.GAME_NAME && entry.Value.Player.Name == Config.Data.slotName)
                    {//If the item is in our game, we can use the normal itemID in the normal reward
                        //Item also needs be in our slot!
                        if (Core.DEBUG) { MelonLogger.Msg($"Location {entry.Value.LocationDisplayName} ({entry.Key}) has {entry.Value.ItemName} ({entry.Value.ItemId})"); }
                        itemID = NEOTwewyDataManager.item_data[entry.Value.ItemName].id;
                    }
                    else
                    {//If the item is not originally from our game we need to use the archipelago replacement item
                        if (Core.DEBUG) { MelonLogger.Msg($"Location {entry.Value.LocationDisplayName} ({entry.Key}) has {entry.Value.ItemName} ({entry.Value.ItemId})"); }
                        itemID = Core.ARCHIPELAGO_ITEM_ID;
                        //Data.scenarioLocationsMapping.Add(entry.Key, FromScoutedItem( entry.Value));
                    }
                    ArchipelagoLocationID keyID = new ArchipelagoLocationID(entry.Key);
                    //MelonLogger.Msg($"Location {entry.Value.LocationDisplayName} ({entry.Key}) has {entry.Value.ItemName} ({entry.Value.ItemId})");
                    if(entry.Key > ArchipelagoData.SCENARIO_LOCATION_MODIFIER)
                    {
                        //if location is a normal scenario location, we need to save it in the scenario mapping
                        Data.scenarioLocationsMapping.Add(keyID.ToGameLocation(), FromScoutedItem(itemID, entry.Value));
                    }
                    else if (entry.Key > ArchipelagoData.DIVE_LOCATION_MODIFIER)
                    {
                        Data.diveLocationsMapping.Add(keyID.ToGameLocation(), FromScoutedItem(itemID, entry.Value));
                    }
                    else if (entry.Key > ArchipelagoData.SHOP_LOCATION_MODIFIER)
                    { //if location is a shop location, we need to save it in the shop mapping
                        Data.shopLocationsMapping.Add(keyID.ToGameLocation(), FromScoutedItem(itemID, entry.Value));

                    }
                    else
                    {  
                    }

                }
                catch (Exception e)
                {
                    MelonLogger.Msg($"Error saving location {entry.Value.LocationDisplayName} ({entry.Key}) with {entry.Value.ItemName} ({entry.Value.ItemId}): {e.Message}");
                }
            }
            Save();
            ArchipelagoData.DataLoaded = true;
        }

        public void CompareCheckedLocations(List<long> allCheckedLocations)
        {
            //TODO: Compare locations from server(param) to local saved locations
            //send ones missing at the server?
        }

        public static ArchipelagoItem FromScoutedItem(long id, ScoutedItemInfo info)
        {
            long itemCount = 1;
            if (info.ItemGame == Core.GAME_NAME && info.Player.Name == Config.Data.slotName)
            {
                itemCount = NEOTwewyDataManager.item_data[info.ItemName].count;
            }
            
            
            return new ArchipelagoItem(
                id,
                info.ItemName,
                new ArchipelagoLocationID(info.LocationId),
                info.LocationName,
                info.ItemId,
                info.ItemGame,
                itemCount,
                info.Player.Name
            );
        }
    }

    public class ModSaveData
    {
        public string Seed { get; set; }
        // We save the seed of the last connected room to indicate the save file

        public long LastItemIndex { get; set; } = -1;
        // Remember the highest index of an item we received from the server

        public HashSet<ArchipelagoLocationID> checkedLocations { get; set; } = new HashSet<ArchipelagoLocationID>();
        //Remember locations which the game save can't remember because they were added by the mod
        public Queue<ArchipelagoLocationID> pendingLocations { get; set; } = new Queue<ArchipelagoLocationID>();
        //Remember locations checked that we could not tell the server about
        public bool goalAchieved { get; set; } = false;
        //Remember we achieved or goal in case we could not tell the server as such

        public Queue<QueuedReward> rewardQueue { get; set; } = new Queue<QueuedReward>();
        //Remember what is currently in the reward queue when the game closed

        //Location ID, ItemID
        public Dictionary<GameLocationID, ArchipelagoItem> scenarioLocationsMapping { get; set; } = new Dictionary<GameLocationID, ArchipelagoItem>();
        public Dictionary<GameLocationID, ArchipelagoItem> shopLocationsMapping { get; set; } = new Dictionary<GameLocationID, ArchipelagoItem>();
        public Dictionary<GameLocationID, ArchipelagoItem> diveLocationsMapping { get; set; } = new Dictionary<GameLocationID, ArchipelagoItem>();

    }

    public enum SyncState
    {
        Offline,        // No connection
        Ready,          // Connected to matching room
        WrongSeed       // Connected, but save belongs to different room
    }


}

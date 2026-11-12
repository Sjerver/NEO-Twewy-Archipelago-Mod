using Archipelago.MultiClient.Net.Models;
using Harmony;
using Il2CppHnLib;
using Il2CppSteamworks;
using Il2CppSystem.Linq;
using JetBrains.Annotations;
using MelonLoader;
using MelonLoader.Utils;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod
{
    public class ModSave
    {
        private static string NOISE_INFO_PATH = "Mods/NeoTwewyArchipelago/Data/NoiseDropInfo.json";


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
                    if (Data.toggleOptions != null)
                    {
                        foreach(var optionEntry in Data.toggleOptions)
                        {
                            ModOptions.UpdateOption(optionEntry.Key, optionEntry.Value);
                        }
                    } else
                    {
                        MelonLogger.Error("Save file is missing options");
                    }
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

        public void UpdateSlotData(Dictionary<string, object> slotData)
        {//TODO: move slot data handling to ModOptions?
            //TODO: We can make our own SLOTData class to read/prase this beetter
            if (Core.syncState == SyncState.WrongSeed) { return; }
            foreach (KeyValuePair<string, object> entry in slotData)
            {
                if (entry.Key == "options")
                {
                    UpdateOptionsInSave(entry.Value);       
                }
                else
                {
                    MelonLogger.Error($"Unknown slot_data {entry.Key}");
                }
            }
            UpdateNoiseDropInfo();
            Save();
        }

        private void UpdateNoiseDropInfo()
        {
            if (File.Exists(NOISE_INFO_PATH))
            {
                string json = File.ReadAllText(NOISE_INFO_PATH);
                Dictionary<string, JObject> info;
                try
                {
                    info = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(json);
                }
                catch (Exception e)
                {
                    MelonLogger.Error(e);
                    MelonLogger.MsgDirect(MelonLoader.Logging.ColorARGB.Red, "File missing, please make sure you installed the mod correctly!");
                    return;
                }

                foreach (var infoEntry in info)
                {
                    if (infoEntry.Value is not JObject dropInfo)
                    {
                        MelonLogger.Error($"Invalid noise drop data for {infoEntry.Key}.");
                        continue;
                    }

                    double drop_chance = dropInfo["drop_rate"]!.Value<double>();

                    int minDropRate = Math.Max(1, (int)Math.Ceiling((ModOptions.min_drop_rate / 100) / drop_chance));
                    bool chainable = dropInfo["chainable"]!.Value<bool>();
                    bool blueNoise = dropInfo["blue_noise"]!.Value<bool>();

                    Data.noiseDropInformation[infoEntry.Key] =
                        new NoiseDropInfo(minDropRate, chainable, blueNoise);
                }
            }
        }

        private void UpdateOptionsInSave(object value)
        {
            MelonLogger.Msg($"options type: {value?.GetType()}");
            if (value is not JObject options)
            {
                MelonLogger.Error("Invalid options slot data.");
                return;
            }

            foreach (KeyValuePair<string,JToken> optionEntry in options)
            { //[21:55:30.795] [NEOTwewyArchipelagoMod] optionEntry: [prevent_out_of_logic, 1] / System.Collections.Generic.KeyValuePair`2[System.String,Newtonsoft.Json.Linq.JToken]
                //MelonLogger.Msg($"optionEntry: {optionEntry} / {optionEntry.GetType()}");
                Data.toggleOptions[optionEntry.Key] = optionEntry.Value;
                ModOptions.UpdateOption(optionEntry.Key, optionEntry.Value);
            }
        }

        public bool getGoalAchieved(){ return Data.goalAchieved;}
        public void setGoalAchieved(bool goalAchieved) { Data.goalAchieved = goalAchieved; Save(); }

        public long getLastItemIndex(){return Data.LastItemIndex;}
        public void setLastItemIndex(long LastItemIndex) { Data.LastItemIndex = LastItemIndex; Save(); }

        public HashSet<ArchipelagoLocationID> getCheckedLocations() { return Data.checkedLocations; }

        public bool TryGetNoiseDropInfo(string name, out NoiseDropInfo info)
        {
            return Data.noiseDropInformation.TryGetValue(name, out info);
        }

        public bool TryGetArchipelagoItem(ArchipelagoLocationID locationID, out ArchipelagoItem item)
        {
            GameLocationID gameLocation = locationID.ToGameLocation();
            LocationType type = locationID.GetLocationType();

            return Data.locationMappings[type].TryGetValue(gameLocation, out item);
        }

        public List<ArchipelagoItem> GetMissingLocations()
        {
            List<ArchipelagoItem> allItems = new List<ArchipelagoItem>();
            foreach (var typeMapping in Data.locationMappings)
            {
                foreach (var item in typeMapping.Value.Values)
                {
                    if (!Data.checkedLocations.Contains(item.locationID))
                    {
                        allItems.Add(item);
                    }
                        
                }
            }
            return allItems;
        }

        public int getPendingLocationSize()
        {
            return Data.pendingLocations.Count;
        }

        public int getRewardQueueSize()
        {
            return Data.rewardQueue.Count;
        }

        public void enqueueLocation(ArchipelagoLocationID location)
        {
            if (Core.syncState == SyncState.WrongSeed) { return; }

            if (IsLocationChecked(location)) { return; }

            if (TryGetArchipelagoItem(location, out ArchipelagoItem item))
            {//Get valid Archipelago Item
                bool archipelagoItemCheck = false;
                bool customDisplay = false;

                LocationType type = location.GetLocationType();
                switch (type)
                {
                    case LocationType.ScenarioReward:
                    case LocationType.DLC:
                    case LocationType.SocialNode:
                    case LocationType.VIP:
                        //Also CheckEndOfChapterReward
                        //"Normal Items" get stopped and handled via server
                        //Archipelago Items need display that they were found
                        archipelagoItemCheck = true;
                        break;

                    case LocationType.DiveReward:
                        //FP and 1x count items can be displayed normally
                        //Therfore we cant let everything be handled via receivedItems
                        archipelagoItemCheck = true;
                        if ((item.id != NEOTwewyDataManager.DIVE_FP_ID && item.count > 1) || item.itemType != ItemType.AllItems)
                        {
                            customDisplay = true;
                        }
                        break;

                    case LocationType.NoiseDrop:
                    case LocationType.PigDrop:
                        //Badges can be done normally, count 1 only
                        //Therfore we cant let everything be handled via receivedItems

                        archipelagoItemCheck = true;
                        break;

                    case LocationType.ShopGood:
                        //Shops can display archi items naturally
                        //Non-shop items like FP or non AllItems are handled via server
                        break;

                    case LocationType.Skill:
                        //Skills can display archi item naturally
                        //Everything but "container items" are handled via server
                        break;

                    case LocationType.Vanilla:
                    case LocationType.ArchipelagoReceive:
                        break;

                    default:
                        MelonLogger.Error($"No custom item handling for location type {type}");
                        break;
                    }

                if (archipelagoItemCheck && item.id == Core.ARCHIPELAGO_ITEM_ID) { customDisplay = true;}
                if (customDisplay)
                {
                    switch (item.itemType)
                    {
                        case ItemType.AllItems:
                        case ItemType.OtherGame:
                            Core.archiItemDisplayQueue.Enqueue(item);
                            break;

                        case ItemType.SocialNode:
                        case ItemType.VIP:
                            //MelonLogger.MsgDirect(MelonLoader.Logging.ColorARGB.Red,"CharacterNode item not implemented yet");
                            //Currently we dont need to do anything here, since always handled by server
                            break;

                        default:
                            MelonLogger.Error($"No custom item handling for item type {item.itemType}");
                            break;
                    }
                }
            }
        
                
            Data.pendingLocations.Enqueue(location);
            addCheckedLocation(location);
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

        public QueuedReward dequeueReward() { 
            if(Data.rewardQueue.TryDequeue(out QueuedReward reward))
            {
                return reward;
            } else {
                MelonLogger.Error("Reward queue is empty");
                return null;
            }
        }

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
                        //if (Core.DEBUG) { MelonLogger.Msg($"Location {entry.Value.LocationDisplayName} ({entry.Key}) has {entry.Value.ItemName} ({entry.Value.ItemId})"); }
                        itemID = NEOTwewyDataManager.item_data[entry.Value.ItemName].id;
                    }
                    else
                    {//If the item is not originally from our game we need to use the archipelago replacement item
                        //if (Core.DEBUG) { MelonLogger.Msg($"Location {entry.Value.LocationDisplayName} ({entry.Key}) has {entry.Value.ItemName} ({entry.Value.ItemId})"); }
                        itemID = Core.ARCHIPELAGO_ITEM_ID;
                        //Data.scenarioLocationsMapping.Add(entry.Key, FromScoutedItem( entry.Value));
                    }
                    ArchipelagoLocationID keyID = new ArchipelagoLocationID(entry.Key);
                    LocationType locType = keyID.GetLocationType();
                    //MelonLogger.Msg($"Location {entry.Value.LocationDisplayName} ({entry.Key}) has {entry.Value.ItemName} ({entry.Value.ItemId})");
                    if(locType != LocationType.Vanilla && locType != LocationType.ArchipelagoReceive)
                    {
                        Data.locationMappings[locType].Add(keyID.ToGameLocation(), new ArchipelagoItem (itemID, entry.Value));
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

    }

    public class ModSaveData
    {
        public string Seed { get; set; }
        // We save the seed of the last connected room to indicate the save file

        public Dictionary<string, object> toggleOptions { get; set; } = new();

        public long LastItemIndex { get; set; } = -1;
        // Remember the highest index of an item we received from the server

        public HashSet<ArchipelagoLocationID> checkedLocations { get; set; } = new HashSet<ArchipelagoLocationID>();
        //Remember locations which the game save can't remember because they were added by the mod
        public Queue<ArchipelagoLocationID> pendingLocations { get; set; } = new Queue<ArchipelagoLocationID>();
        //Remember locations checked that we could not tell the server about
        public bool goalAchieved { get; set; } = false;
        //Remember we achieved or goal in case we could not tell the server as such

        public ConcurrentQueue<QueuedReward> rewardQueue { get; set; } = new ConcurrentQueue<QueuedReward>();
        //Remember what is currently in the reward queue when the game closed

        //
        [JsonProperty(Order = 1000)]
        public Dictionary<string, NoiseDropInfo> noiseDropInformation = new Dictionary<string, NoiseDropInfo>();

        //Location ID, ItemID
        public Dictionary<LocationType, Dictionary<GameLocationID, ArchipelagoItem>> locationMappings { get; set; } = CreateLocationMappings();

        private static Dictionary<LocationType, Dictionary<GameLocationID, ArchipelagoItem>> CreateLocationMappings()
        {
            Dictionary<LocationType, Dictionary<GameLocationID, ArchipelagoItem>> new_dict = new();
            foreach (LocationType loc_type in Enum.GetValues<LocationType>())
            {
                Dictionary<GameLocationID, ArchipelagoItem> dict = new Dictionary<GameLocationID, ArchipelagoItem>();
                new_dict.Add(loc_type, dict);
            }

            return new_dict;
        }

    }

    public class NoiseDropInfo(int drop_multiplier, bool chainable, bool blue_noise)
    {
        public int min_drop_multiplier = drop_multiplier;
        public bool chainable = chainable;
        public bool blue_noise = blue_noise;
    }

    public enum SyncState
    {
        Offline,        // No connection
        Ready,          // Connected to matching room
        WrongSeed       // Connected, but save belongs to different room
    }


}

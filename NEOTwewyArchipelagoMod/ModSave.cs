using Archipelago.MultiClient.Net.Models;
using Il2CppHnLib;
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
                Data = JsonConvert.DeserializeObject<ModSaveData>(json);
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

        public HashSet<long> getCheckedLocations() { return Data.checkedLocations; }
        public int getPendingLocationSize()
        {
            return Data.pendingLocations.Count;
        }

        public int getRewardQueueSize()
        {
            return Data.rewardQueue.Count;
        }

        public  void enqueueLocation(long location)
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

        public void addCheckedLocation(long location)
        {
            if (Core.syncState == SyncState.WrongSeed) { return; }
            Data.checkedLocations.Add(location);
            Save();
        }

        public long dequeueLocation() { return Data.pendingLocations.Dequeue(); }

        public QueuedReward dequeueReward() { return Data.rewardQueue.Dequeue(); }

        public void SaveArchipelagoLocations(Dictionary<long, ScoutedItemInfo> locationData)
        {
            foreach (KeyValuePair<long, ScoutedItemInfo> entry in locationData)
            {//For all locations in our game we received from the server
                if (entry.Value.ItemGame == Core.GAME_NAME)
                {//If the item is in our game, we can use the normal itemID in the normal reward
                    if (Core.DEBUG) { MelonLogger.Msg($"Location {entry.Value.LocationDisplayName} ({entry.Key}) has {entry.Value.ItemName} ({entry.Value.ItemId})"); }
                    Data.archiLocationItemMapping.Add(entry.Key, FromScoutedItem(entry.Value.ItemId, entry.Value));
                }
                else
                {//If the item is not originally from our game we need to use the archipelago replacement item
                    if (Core.DEBUG) { MelonLogger.Msg($"Location {entry.Value.LocationDisplayName} ({entry.Key}) has {entry.Value.ItemName} ({entry.Value.ItemId})"); }
                    Data.archiLocationItemMapping.Add(entry.Key, FromScoutedItem(Core.ARCHIPELAGO_ITEM_ID, entry.Value));
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
            return new ArchipelagoItem(
                id,
                info.ItemName,
                info.LocationId,
                info.LocationName,
                info.ItemId,
                info.ItemGame
            );
        }
    }

    public class ModSaveData
    {
        public string Seed { get; set; }
        // We save the seed of the last connected room to indicate the save file

        public long LastItemIndex { get; set; } = -1;
        // Remember the highest index of an item we received from the server

        public HashSet<long> checkedLocations { get; set; } = new HashSet<long>();
        //Remember locations which the game save can't remember because they were added by the mod
        public Queue<long> pendingLocations { get; set; } = new Queue<long>();
        //Remember locations checked that we could not tell the server about
        public bool goalAchieved { get; set; } = false;
        //Remember we achieved or goal in case we could not tell the server as such

        public Queue<QueuedReward> rewardQueue { get; set; } = new Queue<QueuedReward>();
        //Remember what is currently in the reward queue when the game closed

        //Location ID, ItemID
        public Dictionary<long, ArchipelagoItem> archiLocationItemMapping { get; set; } = new Dictionary<long, ArchipelagoItem>();

    }

    public enum SyncState
    {
        Offline,        // No connection
        Ready,          // Connected to matching room
        WrongSeed       // Connected, but save belongs to different room
    }


}

using MelonLoader.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod
{
    public static class ModSave
    {
        private static string SavePath =>
            Path.Combine(MelonEnvironment.UserDataDirectory, "NEOTwewyArchipelagoSave.json");

        public static ModSaveData Data { get; private set; } = new();

        public static void Load(string seed)
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Data = JsonConvert.DeserializeObject<ModSaveData>(json);

                if (seed != Data.Seed)
                {
                    Data = new ModSaveData();
                    Data.Seed = seed;
                    Save();
                }
            }
            else
            {
                Data = new ModSaveData();
                Save();
            }
        }

        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(SavePath, json);
        }
    }

    public class ModSaveData
    {
        public string Seed { get; set; }
        // We save the seed of the last connected room to indicate the save file
        public long LastProcessedItemIndex { get; set; } = -1;
        // Remember the highest index of an item we received from the server

        public List<long> nonStandardLocations { get; set; } = new List<long>();
        //Remember locations which the game save can't remember because they were added by the mod
        public Queue<long> pendingLocations { get; set; } = new Queue<long>();
        //Remember locations checked that we could not tell the server about
        public bool goalAchieved { get; set; } = false;
        //Remember we achieved or goal in case we could not tell the server as such

        public Queue<QueuedReward> rewardQueue { get; set; } = new Queue<QueuedReward>();
        //Remember what is currently in the reward queue when the game closed

    }
}

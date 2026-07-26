using Archipelago.MultiClient.Net.Packets;
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
    public static class Config
    {
        private static string SavePath =>
            Path.Combine(MelonEnvironment.UserDataDirectory, "NEOTwewyArchipelagoConfig.json");

        public static ConfigData Data { get; private set; } = new();

        public static void Load()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                MelonLogger.Msg($"{json}");
                Data = JsonConvert.DeserializeObject<ConfigData>(json);
            }
            else
            {
                Data = new ConfigData();
                Save();
                MelonLogger.Msg("Created config file from scratch. Please edit the config.file before restarting the game.");

            }
        }

        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(SavePath, json);
        }
    }

    public class ConfigData
    {
        public string hostName = "localhost";
        public int port = 38281;
        public string slotName = "";
        public string password = "";
    }

}

using Archipelago.MultiClient.Net.Packets;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NEOTwewyArchipelagoMod
{
    public static class Config
    {
        private static string SavePath = "Mods/NeoTwewyArchipelago/NEOTwewyArchipelagoConfig.json";

        public static ConfigData Data { get; private set; } = new();

        public static void Load()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                //MelonLogger.Msg($"{json}");
                Data = JsonConvert.DeserializeObject<ConfigData>(json);
                Save();
            }
            else
            {
                Data = new ConfigData();
                Save();
                MelonLogger.Msg("Created config file from scratch. Please edit the config.file before restarting the game.");

            }

            bool validKey = Enum.TryParse<KeyCode>(Config.Data.skipDayButton, out var key);
            if (validKey)
            {
                Core.SKIP_DAY_BUTTON = key;
            } else
            {
                MelonLogger.Warning($"Invalid skipDayButton '{Data.skipDayButton}', using F5.");
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

        public string skipDayButton = "F5";
    }

}

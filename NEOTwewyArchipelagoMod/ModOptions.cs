using MelonLoader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod
{
    public static class ModOptions
    {
        public static bool prevent_out_of_logic = false;
        public static int min_drop_rate = 0;
        public static DLCOption dlc_options = DLCOption.Vanilla;
        public static bool soundsurf = false;
        public static bool sns = false;
        public static bool shops = false;
        public static double cutscene_skip_speed = 10.0;
        public static Dictionary<int, int> fp_costs = new Dictionary<int, int>();

        public static bool shop_hints = false;
        public static bool skill_hints = false;
        //Dictionary<ArchipelagoItemId, Tupley<PlayerId, LocationId, SphereNumber>>> 
        public static Dictionary<int, Tuple<int, int,int>> secret_report_hints = new Dictionary<int, Tuple<int, int,int>>();

        public static void UpdateOption(string key, object value)
        {
            string[] boolOptions = ["prevent_out_of_logic", "soundsurf", "sns", "shops", "shop_hints", "skill_hints" ];
            if (boolOptions.Contains(key))
            {
                typeof(ModOptions).GetField(key).SetValue(null, Convert.ToBoolean(value));
            }
            else if (key == "min_drop_rate")
            {
                typeof(ModOptions).GetField(key).SetValue(null, Convert.ToInt32(value));
            }
            else if (key == "cutscene_skip_speed")
            {
                typeof(ModOptions).GetField(key).SetValue(null, Convert.ToDouble(value));
            }
            else if (key == "dlc_options")
            {
                typeof(ModOptions).GetField(key).SetValue(null, (DLCOption)Convert.ToInt32(value));
            } else if (key == "fp_costs")
            {
                typeof(ModOptions).GetField(key).SetValue(null, ((JObject)value).ToObject<Dictionary<int,int>>());
            } else if (key == "secret_report_hints")
            {
                Dictionary<int, int[]> hints =  ((JObject)value).ToObject<Dictionary<int, int[]>>();
                secret_report_hints = hints.ToDictionary(
                    pair => pair.Key,
                    pair => Tuple.Create(pair.Value[0], pair.Value[1], pair.Value[2])
                );
                var sortedDict = from entry in secret_report_hints orderby entry.Value.Item3 ascending select entry;
                secret_report_hints = sortedDict.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            else {
                MelonLogger.Error($"Option {key} cannot be converted");
            }
            MelonLogger.Msg($"{key} = {typeof(ModOptions).GetField(key).GetValue(null)}");
        }
    }

    public enum DLCOption
    {
        Vanilla = 0,
        Disable = 1,
        Randomize = 2,
        Randomize_Empty = 3,
    }
}

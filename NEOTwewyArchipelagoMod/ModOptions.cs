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

        public static void UpdateOption(string key, object value)
        {
            if (key == "prevent_out_of_logic" || key == "soundsurf" || key =="sns" ||key == "shops")
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
            }
            else {
                MelonLogger.Error($"Option {key} cannot be converted");
            }
            MelonLogger.Msg($"{key} = {typeof(ModOptions).GetField(key).GetValue(null)}");
        }
    }
}

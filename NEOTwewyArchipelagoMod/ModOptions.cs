using MelonLoader;
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

        public static void UpdateOption(string key, object value)
        {
            if (key == "prevent_out_of_logic" || key == "soundsurf")
            {
                typeof(ModOptions).GetField(key).SetValue(null, Convert.ToBoolean(value));
            }
            else if (key == "min_drop_rate")
            {
                typeof(ModOptions).GetField(key).SetValue(null, Convert.ToInt32(value));
            }
            else if (key == "dlc_options")
            {
                typeof(ModOptions).GetField(key).SetValue(null, (DLCOption)Convert.ToInt32(value));
            }
            else {
                MelonLogger.Error($"Option {key} cannot be converted");
            }
            MelonLogger.Msg($"{key} = {typeof(ModOptions).GetField(key).GetValue(null)}");
        }
    }
}

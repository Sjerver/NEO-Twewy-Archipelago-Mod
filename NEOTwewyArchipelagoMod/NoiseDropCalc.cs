using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod
{
    public static class DropRateCalc
    {
        public static int currentMaxDropRate = 0;

        public static bool DropRateReached(ArchipelagoLocationID locationID)
        {

            if (Core.save.TryGetArchipelagoItem(locationID, out var item))
            {
                //Check difficulty:
                if (item.locationName.Contains("HARD Drop") && !SaveLoadController.Get<SaveDataSkill>().GetIsSkill(Il2CppMaster.Skill.ELabel.Difficulty_Hard))
                {
                    //MelonLogger.Msg($"{item.locationName} hard mode not unlocked");
                    return false;
                }
                else if (item.locationName.Contains("EASY Drop") && !SaveLoadController.Get<SaveDataSkill>().GetIsSkill(Il2CppMaster.Skill.ELabel.Difficulty_Easy))
                {
                    //MelonLogger.Msg($"{item.locationName} easy mode not unlocked");
                    return false;
                }
                else if (item.locationName.Contains("ULTIMATE Drop") && !SaveLoadController.Get<SaveDataSkill>().GetIsSkill(Il2CppMaster.Skill.ELabel.Difficulty_Ultimate))
                {
                    //MelonLogger.Msg($"{item.locationName} ultimate mode not unlocked");
                    return false;
                }


                if (Core.save.TryGetNoiseDropInfo(item.locationName, out NoiseDropInfo info))
                {
                    Recalculate(info);
                    //MelonLogger.Msg($"{item.locationName}: {info.min_drop_multiplier} >= {currentMaxDropRate} -> {info.min_drop_multiplier <= currentMaxDropRate} ");
                    if (info.min_drop_multiplier <= currentMaxDropRate)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            //These locations should not care about calculation?
            return true;
        }

        public static void Recalculate(NoiseDropInfo info)
        {
            double drop_multiplier = 1.0;

            double level_multiplier = 1.0;
            //TODO: Level_multiplier from inventory check


            drop_multiplier = level_multiplier * NEOTwewyDataManager.EXPECTED_LEVEL_PER_REGION[Core.furthestDayReached];

            //day 3 required for threads drop rate calc


            //Chain length TODO: Skill checks
            int chain_length = 1;
            if (info.chainable && !info.blue_noise) //TODO: Missing Bossy Noise check
            {//self.chainable and(not self.blue_noise or(self.blue_noise and inventory["Bossy Noise"] > 0)):
                chain_length = 5;
            }



            drop_multiplier *= chain_length;



            //killer remix

            currentMaxDropRate = (int)Math.Floor(drop_multiplier);
            //MelonLogger.Msg($"currentMaxDropRate {currentMaxDropRate}");

        }
    }
}

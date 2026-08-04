using HarmonyLib;
using Il2Cpp;
using Il2CppMaster;
using MelonLoader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(TextManager), "GetTextFromJson")]
    public static class PatchGetActualTextDataFromAssets
    {
        public static void Postfix(string json, string configJson, TextConstants.FileName inFileName, Il2CppSystem.Collections.Generic.Dictionary<string, TextManager.TextData> __result)
        {
            if (inFileName.ToString().Contains("ItemNameBDG"))
            {
                MelonLogger.Msg("Modify Pin Names");
                __result["ITM_BDG_Name_0313"].Content = "Archipelago";
            }
            else if (inFileName.ToString().Contains("ItemInfoBDG"))
            {
                MelonLogger.Msg("Modify Pin Descriptions");
                __result["ITM_BDG_Info_0313"].Content = "A pin reminiscent of a multiworld. Seems to have various effects... ";
            }

        }
    }


    /*
     * Get GameData from Assets 
     */
    [HarmonyPatch(typeof(MasterDataManager), "GetTextDataFromAssets")]
    public static class PatchGetTextDataFromAssets
    {
        public static void Postfix(string assetName, ref string __result)
        {
            // Inject ScenarioRewards
            if (assetName.Contains("ScenarioRewards"))
            {
                //MelonLogger.Msg("Injecting ScenarioRewards");

                JObject root = JObject.Parse(__result);

                JArray targets = (JArray)root["mTarget"];


                //MelonLogger.Msg($"Edit existing rewards");
                ////RewardID, ItemID
                //foreach (KeyValuePair<long, long> entry in ArchipelagoData.ScenarioRewardsDict)
                //{
                //    long scenarioId = entry.Key;
                //    long itemId = entry.Value;

                //    JObject target = targets.OfType<JObject>()
                //        .FirstOrDefault(t => (long)t["mId"] == scenarioId);

                //    if (target != null)
                //    {
                //        //MelonLogger.Msg($"{scenarioId}");
                //        target["mReward1st"] = itemId;
                //    }
                //    else
                //    {
                //        //MelonLogger.Msg($"{scenarioId}");
                //        targets.Add(new JObject
                //        {
                //            ["mId"] = scenarioId,
                //            ["mReward1st"] = itemId,
                //            ["mReward1stCount"] = 1,
                //            ["mReward2nd"] = -1,
                //            ["mReward2ndCount"] = 0,
                //            ["mSaveIndex"] = 251
                //        });
                //    }
                //}


                MelonLogger.Msg($"Add rewards to receive from Archipelago");
                //itemID, RewardID
                foreach (KeyValuePair<long, NEOTwewyItemData> entry in ArchipelagoData.ReceivableRewards)
                {
                    //MelonLogger.Msg($"{entry.Value}");
                    targets.Add(new JObject
                    {
                        ["mId"] = entry.Value.reward_ID,
                        ["mReward1st"] = entry.Value.id, //Secret Report 1
                        ["mReward1stCount"] = entry.Value.count,
                        ["mReward2nd"] = -1,
                        ["mReward2ndCount"] = 0,
                        ["mSaveIndex"] = 251 // From the TestReward
                    });
                }

                __result = root.ToString();
            }
            else if (assetName.Contains("EnemyData"))
            {
                MelonLogger.Msg("Edit Enemy Data");

                JObject root = JObject.Parse(__result);
                JArray targets = (JArray)root["mTarget"];


                //Edit enemies that drop 1Yen Pin to drop 5Yen Pin instead
                var dixieFrog = targets[12];
                dixieFrog["mDrop"][3] = 5001; // Change to 5 Yen Pin
                var pigW1D3Center = targets[213]; //W1D3 Center Street - Pig Noise
                pigW1D3Center["mDrop"][3] = 5001; // Change to 5 Yen Pin




                __result = root.ToString();
            }
            else if (assetName.Contains("BattleCharacter"))
            {
                MelonLogger.Msg("Edit BattleChar data");

                JObject root = JObject.Parse(__result);
                JArray targets = (JArray)root["mTarget"];

                if (Core.DEBUG)
                {
                    MelonLogger.Msg("Set Enemy HP to 1");
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i]["mId"] == null)
                        {
                            MelonLogger.Msg($"Missing mID at index {i}");
                            //MelonLogger.Msg(targets[i].ToString());
                            continue;
                        }

                        if ((int)targets[i]["mId"] > 100)
                        {
                            targets[i]["mHp"] = 1;
                            //MelonLogger.Msg(targets[i].ToString());
                        }
                    }
                }


                __result = root.ToString();
            }
            // Inject custom Item
            else if (assetName.Contains("Badge.txt"))
            {
                //MelonLogger.Msg("Editing Badges");

                //JObject root = JObject.Parse(__result);

                //JArray targets = (JArray)root["mTarget"];

                //targets[312]["mBadgeCategory"] = (int)ItemConst.EBadgeCategoryType.Material;

                //__result = root.ToString();

                //    MelonLogger.Msg($"Badge count: {targets.Count}");
            }
            else if (assetName.Contains("AllItems"))
            {
                //    MelonLogger.Msg("Injecting Items");

                //    JObject root = JObject.Parse(__result);

                //    JArray targets = (JArray)root["mTarget"];

                //    var copyObject = (JObject)targets[313].DeepClone(); // 1 Yen Pin
                //    MelonLogger.Msg(targets[313].ToString());

                //    copyObject["mID"] = 999999;
                //    copyObject["mSaveId"] = 801;
                //    targets.Add(copyObject);
                //    __result = root.ToString();
            }

            else if (assetName.Contains("Skill.txt"))
            {
                //    MelonLogger.Msg("Reading Skills"); //Technically worked to inject

                //    JObject root = JObject.Parse(__result);

                //    JArray targets = (JArray)root["mTarget"];

                //    MySkill test = new MySkill(99, 2, 3, 48);


                //    targets.Add(test.getJSON());
                //    __result = root.ToString();
            }

            else if (assetName.Contains("SkillTree"))
            {
                //    MelonLogger.Msg("Reading SkillTree");

                //    JObject root = JObject.Parse(__result);

                //    JArray targets = (JArray)root["mTarget"];

                //    targets[0]["mSkill"] = 99; //The game did not like displaying this one

                //    __result = root.ToString();
            }

            else if (assetName.Contains("Chapter"))
            {
                //    //TODO: Test recruiting character in replaying chapter: Shoka did not join in w2d5
                //    MelonLogger.Msg("Shuffling Chapter Order");

                //    JObject root = JObject.Parse(__result);
                //    JArray targets = (JArray)root["mTarget"];


                //    //This randomization method did not work
                //    int[] ids = [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 0, 1, 2];
                //    for (int i = 0; i < targets.Count; i++)
                //    {
                //        //MelonLogger.Msg(targets[i].ToString());
                //        targets[i]["mID"] = ids[i];
                //    }

                //    __result = root.ToString();
            }
            else if (assetName.Contains("Dive.txt"))
            {
                //    MelonLogger.Msg("Editing Dives");

                //    JObject root = JObject.Parse(__result);
                //    JArray targets = (JArray)root["mTarget"];

                //    for (int i = 0; i < targets.Count; i++)
                //    {
                //        MelonLogger.Msg(targets[i].ToString());
                //        targets[i]["mBronzeItem"] = (int)AllItems.ELabel.bad_99_00_07;
                //    }

                //    __result = root.ToString();
            }
            return;
        }
    }
}

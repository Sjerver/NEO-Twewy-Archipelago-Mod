using HarmonyLib;
using Il2Cpp;
using Il2CppMaster;
using Il2CppScenario;
using Il2CppUI.Utility;
using MelonLoader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NEOTwewyArchipelagoMod
{
    /*
     * Log when a ScenerioReward is reserved.
     */
    [HarmonyPatch(typeof(FieldManager), "ReserveScenarioReward")]
    public static class PatchReserveScenarioReward
    {
        public static void Postfix(ScenarioRewards.ELabel __0,int __1)
        {
            MelonLogger.Msg(
                $"ReserveScenarioReward called: ID={__0}, Index={__1}");

            ScenarioRewards scenarioRewards = MasterDataBase<ScenarioRewards>.Get((int)__0);

            MelonLogger.Msg($"Gave Item {scenarioRewards.mReward1st} x {scenarioRewards.mReward1stCount}");


            //Secret Report Handling
            AllItems allItems = MasterDataBase<AllItems>.Get((int)scenarioRewards.mReward1st);
            if (allItems.mItemType == ItemConst.EItemType.Book)
            {
                Book book = UIUtility.GetBook(allItems);
                if (book.IsSecretReport())
                {
                    Core.currentGameDay++;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TextManager), "GetTextFromJson")]
    public static class PatchGetActualTextDataFromAssets
    {
        public static void Postfix(string json,string configJson,TextConstants.FileName inFileName, Il2CppSystem.Collections.Generic.Dictionary<string, TextManager.TextData> __result)
        {
            if (inFileName.ToString().Contains("ItemNameBDG"))
            {
                MelonLogger.Msg("Modify Pin Names");
                __result["ITM_BDG_Name_0313"].Content = "Archipelago";
            }
            else if (inFileName.ToString().Contains("ItemInfoDG"))
            {
                MelonLogger.Msg("Modify Pin Descriptions");
                __result["ITM_BDG_Name_0313"].Content = "A pin reminiscent of a multiword. Seems to have various effects... ";
            }

        }
    }

    /*
     * Get TextData from Assets 
     */
    [HarmonyPatch(typeof(MasterDataManager), "GetTextDataFromAssets")]
    public static class PatchGetTextDataFromAssets
    {
        public static void Postfix(string assetName,ref string __result)
        {
            // Inject ScenarioRewards
            if (assetName.Contains("ScenarioRewards"))
            {
                MelonLogger.Msg("Injecting ScenarioRewards");

                JObject root = JObject.Parse(__result);

                JArray targets = (JArray)root["mTarget"];

                targets.Add(new JObject
                {
                    ["mId"] = 999999,
                    ["mReward1st"] = 31000, //Secret Report 1
                    ["mReward1stCount"] = 1,
                    ["mReward2nd"] = -1,
                    ["mReward2ndCount"] = 0,
                    ["mSaveIndex"] = 251 // From the TestReward
                });
                targets.Add(new JObject
                {
                    ["mId"] = 9999999,
                    ["mReward1st"] = 31001, //Secret Report 2
                    ["mReward1stCount"] = 1,
                    ["mReward2nd"] = -1,
                    ["mReward2ndCount"] = 0,
                    ["mSaveIndex"] = 251 // From the TestReward
                });

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

                __result = root.ToString();
            }
            // Inject custom Item
            else if (assetName.Contains("Badge.txt"))
            {
                //    MelonLogger.Msg("Injecting Badges");

                //    JObject root = JObject.Parse(__result);

                //    JArray targets = (JArray)root["mTarget"];

                //    var copyObject = (JObject)targets[313].DeepClone(); // 1 Yen Pin

                //    //MelonLogger.Msg(targets[313].ToString());

                //    copyObject["mID"] = 999999;
                //    //copyObject["mItemID"] = 999;

                //    targets.Add(copyObject);

                //    __result = root.ToString();

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
            return;
        }
    }

    /*
     * Behaviour when a regular day ends!
     */
    [HarmonyPatch(typeof(SaveDataField), "SetNextScenarioDateDay")]
    public static class PatchSetNextScenarioDateDay
    {
        public static void Prefix()
        {
            MelonLogger.Msg($"Finished day {SaveLoadController.Get<SaveDataField>().GetNewestDateDay()}");
            SaveLoadController.Get<SaveDataField>().SetScenarioDateDay(Core.currentGameDay - 1);
        }

        public static void Postfix()
        {
            MelonLogger.Msg($"Start day {SaveLoadController.Get<SaveDataField>().GetNewestDateDay()}");
        }
    }

    [HarmonyPatch(typeof(SaveDataPlayerTeam), "JoinMember")]
    public static class PatchJoinMember
    {
        public static void Prefix(int __0, BattlePlayer.ELabel __1)
        {
            //Triggered for Minamimoto Day 3
            //Did not trigger revisiting w2d7, but did trigger on normal visit

            //TODO: I think this is what prevents this from happening on revisit: ScenarioJoinCharacter

            MelonLogger.Msg($"Member joined index {__0} and Label {__1}");
        }
    }

    //SaveLoadController.Get<SaveDataField>().SetScenarioFlag(
    [HarmonyPatch(typeof(SaveDataField), "SetScenarioFlag")]
    public static class PatchSetScenarioFlag
    {
        public static void Prefix(int __0, bool __1)
        {
            //Also patch this one SetScenarioFlagData
            MelonLogger.Msg($"Set Scenario Flag {(Scenario.EName)__0} to {__1}");

            Core.CheckEndOfChapterReward((Scenario.EName)__0, __1);
        }
    }
    [HarmonyPatch(typeof(SaveDataField), "SetScenarioFlagData")]
    public static class PatchSetScenarioFlagData
    {
        public static void Prefix(int __0, bool __1)
        {
            MelonLogger.Msg($"Set ScenarioFlagData index {__0} name {ScenarioFlagList.flagNamesFromSaveIndex[__0]} to {__1}");

            Core.CheckEndOfChapterReward(ScenarioFlagList.flagNamesFromSaveIndex[__0], __1);
           
        }
    }

    [HarmonyPatch(typeof(AddCharacterExtension),"ScenarioJoinCharacter")]
    class PatchScenarioJoinCharacter
    {
        public static void Prefix(BattlePlayer.ELabel __0, bool __1, ref bool __2)
        {
            //Setting it to false doesn't do anything to minamimoto joining
            //This Method does not trigger when replaying w2d7

            MelonLogger.Msg($"Prefix ScenarioJoinCharacter with playerID {__0} checkSystem {__1} and isNewestDateDay {__2}");
            __2 = false;
        }

        public static void Postfix(BattlePlayer.ELabel __0, bool __1, ref bool __2)
        {

            MelonLogger.Msg($"Postfix ScenarioJoinCharacter with playerID {__0} checkSystem {__1} and isNewestDateDay {__2}");
        }
    }

    //Might be useful to find last battle IsLastBossBattleScenario
}

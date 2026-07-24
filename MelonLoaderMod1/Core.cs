using Il2Cpp;
using Il2CppMaster;
using MelonLoader;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using UnityEngine;
using static Il2CppCustomComponents.FixedSpriteParameterTable;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime;
using Il2CppUI.Report;

[assembly: MelonInfo(typeof(NEOTwewyArchipelagoMod.Core), "NEOTwewyArchipelagoMod", "1.0.0", "sjerver", null)]
[assembly: MelonGame("SQUARE ENIX", "NEO: The World Ends with You")]

namespace NEOTwewyArchipelagoMod
{
    public class Core : MelonMod
    {
        public bool initalized = false;
        
        public static bool activatedGameFunctions = false;
        
        public static Queue<int> rewardQueue = new Queue<int>();

        public static int currentGameDay = -1;

        public static List<int> daysBeaten = new List<int>();
        
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }


        public override void OnUpdate()
        {
            if (FieldManager.Instance.IsMoveStatus())
            {
                //MelonLogger.Msg("Player can move");

                
                // Initialize Values on Load/Start Save
                if(!initalized)
                {
                    initalized = true;

                    currentGameDay = SaveLoadController.Get<SaveDataField>().GetNewestDateDay();
                    MelonLogger.Msg($"Current Newest Day is {currentGameDay}");
                }

                // Activate Certain Game Functions
                //TODO: Instead of running once change to dynamic code that activates these flags if they arent otherwise active
                if (!activatedGameFunctions)
                {
                    activatedGameFunctions = true;
                    //Activates Drops
                    SaveLoadController.Get<SaveDataField>().SetScenarioFlag(Scenario.EName.System_Battle_DisableBadgeDrop, false);

                    //Activate Chapter Select + Noise Report + Social Network
                    SaveLoadController.Get<SaveDataField>().SetScenarioFlag(Scenario.EName.System_EnableMenu_Report_Chapter, true);
                    SaveLoadController.Get<SaveDataField>().SetScenarioFlag(Scenario.EName.System_EnableMenu_Report_Noise, true);
                    SaveLoadController.Get<SaveDataField>().SetScenarioFlag(Scenario.EName.System_EnableMenu_Analyze_Board, true);
                    SaveLoadController.Get<SaveDataField>().SetScenarioFlag(Scenario.EName.System_EnableMenu_Analyze_Memo, true);
                    SaveLoadController.Get<SaveDataField>().SetScenarioFlag(Scenario.EName.System_EnableMenu_Fashion, true);
                }


                // Run Reward Queue
                while (rewardQueue.Count > 0) {
                    int rewardToTrigger = rewardQueue.Dequeue();
                    MelonLogger.Msg(rewardToTrigger);
                    FieldManager.Instance.ReserveScenarioReward((ScenarioRewards.ELabel)rewardToTrigger, 1);
                }
                
                
            }
            
            if (Input.GetKeyDown(KeyCode.F8))
            {
                MelonLogger.Msg("F8 pressed");


                // Triggers Firestorm D1 Reward
                // Does not work in battles! So properly needs to be in a waiting list or something for archipelago
                // Only Slot 1 Works??
                rewardQueue.Enqueue((int)Il2CppMaster.ScenarioRewards.ELabel.Reward_1w1d_020);
                //FieldManager.Instance.ReserveScenarioReward(Il2CppMaster.ScenarioRewards.ELabel.Reward_1w1d_020, 1); //Firestorm
                //FieldManager.Instance.ReserveScenarioReward(Il2CppMaster.ScenarioRewards.ELabel.Reward_1w1d_010, 1); //Joli Becot
                rewardQueue.Enqueue((int)Il2CppMaster.ScenarioRewards.ELabel.Reward_Test_001);
                //FieldManager.Instance.ReserveScenarioReward(Il2CppMaster.ScenarioRewards.ELabel.Reward_Test_001, 1); // FP 3


                //Quietly adds a Pin
                //SaveLoadController.Get<SaveDataBadge>().AddMyBadge(Il2CppMaster.Badge.ELabel.Psi_03_01_02_00);

                //var badgeArray  = SaveLoadController.Get<SaveDataBadge>().GetMyBadgeListIdNotDuplicate();

                //for(int i = 0;  i < badgeArray.Count; i++)
                //{
                //    MelonLogger.Msg($"Badge: {nameDatabase.getPinName((int)badgeArray[i].Id)}");
                //}



                //Set Scenario Day
                //SaveLoadController.Get<SaveDataField>().SetScenarioDateDay(15);
                // SaveLoadController.Get<SaveDataField>().SetNextScenarioDateDay();
                //SetNextScenarioDateDay increases day by 1
                //At least setting the scenario directly changes current day! Also unlocks all previous chapters
                //Contrary to previous idea cannot manually end the current day with SaveLoadController.Get<SaveDataField>().SetNextScenarioDateDay();

                //IsProgressDay could maybe be modified to only show some chapters?
                //Idea might be that all chapters are available from start?
                //And then just hide all the ones that arent good? Might not work right?
                //Instead keep "unlocked chapter count" somewhere else and update in pre-hook for SetNextScenarioDateDay()?


                //SaveLoadController.Get<SaveDataSkill>().SetIsSkill(Skill.ELabel.Battle_HPGaugeDisp, true);


            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                MelonLogger.Msg("F5 pressed");

                //In combination with setting all flags for the day could work as a finish day thing!
                //Check here if you have beat current day at least once!!!
                int gameDay = SaveLoadController.Get<SaveDataField>().GetNewestDateDay();
                if (daysBeaten.Contains(gameDay))
                {
                    Scenario.EName[] flagList = ScenarioFlagList.flagsToBeatDay[gameDay];
                    for (int i = 0; i < flagList.Length; i++)
                    {
                        SaveLoadController.Get<SaveDataField>().SetScenarioFlag(flagList[i], true);
                    }

                    FieldManager.Instance.MainMenuToChapterSelect(currentGameDay);
                } else
                {
                    MelonLogger.Msg($"{gameDay} has not yeen beaten");
                }
                

            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                MelonLogger.Msg("F6 pressed");

                currentGameDay++;
                MelonLogger.Msg(currentGameDay);
                //SaveLoadController.Get<SaveDataField>().SetClearAnotherDay();

                //var rewards = MasterDataBase<ScenarioRewards>.Array;
                ////var rewards = ScenarioRewards.Array;

                //MelonLogger.Msg(rewards.GetType().FullName);
                //MelonLogger.Msg($"Reward count: {rewards.Length}");

                //foreach (var reward in rewards)
                //{
                //    MelonLogger.Msg(
                //    $"ID={reward.Id}, " +
                //    $"Item={reward.Reward1st}, " +
                //    $"Count={reward.Reward1stCount}");

                //    //if (reward.Id == 1000000) // Test Case
                //    //{
                //    //    reward.Reward1st = 
                //    //}


                //}

                //var reward = ScenarioRewards.Get(999999);

                //if (reward != null)
                //{
                //    MelonLogger.Msg("Injected reward exists!");
                //    MelonLogger.Msg(reward.Reward1st.ToString());
                //}
                //else
                //{
                //    MelonLogger.Msg("Injection failed");
                //}

                //// Unused Test Reward 1000000
                //rewardQueue.Enqueue(999999);
                //FieldManager.Instance.ReserveScenarioReward((ScenarioRewards.ELabel)999999, 1); // FP 3


                //SaveLoadController.Get<SaveDataBattle>().SetDifficulty(DifficultyLevel.ELabel.ULTIMATE);


            }
        }

        public static void CheckEndOfChapterReward(Scenario.EName scenarioFlag, bool value)
        {
            if (ScenarioFlagList.endOfDayFlag.ContainsKey(scenarioFlag) && value == true)
            {
                var (day, rewardID) = ScenarioFlagList.endOfDayFlag[scenarioFlag];
                Core.rewardQueue.Enqueue(rewardID);
                daysBeaten.Add(day);
            }
        }
    }
}
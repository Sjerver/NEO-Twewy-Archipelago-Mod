using Archipelago.MultiClient.Net.Models;
using Il2Cpp;
using Il2CppMaster;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(NEOTwewyArchipelagoMod.Core), "NEOTwewyArchipelagoMod", "1.0.0", "sjerver", null)]
[assembly: MelonGame("SQUARE ENIX", "NEO: The World Ends with You")]

namespace NEOTwewyArchipelagoMod
{
    public class Core : MelonMod
    {
        //Essentially static constants
        public static string GAME_NAME = "NEO: The World Ends with You";
        public static int ARCHIPELAGO_ITEM_ID = 5000;
        public static bool DEBUG = false;

        public static KeyCode SKIP_DAY_BUTTON = KeyCode.F5;

        //For stuff we call one time once we reach the field scene
        public bool initalized = false;
        
        //Lists of flags that should always have a certain value
        public static List<Scenario.EName> alwaysOnFlags = new List<Scenario.EName>()
        {
            Scenario.EName.System_EnableMenu_Report_Chapter,
            Scenario.EName.System_EnableMenu_Report_Noise,
            Scenario.EName.System_EnableMenu_Analyze_Board,
            Scenario.EName.System_EnableMenu_Analyze_Memo,
            Scenario.EName.System_EnableMenu_Fashion,
            Scenario.EName.System_EnableMenu_Badge, //Can't be true unless Rindo is initalized
            Scenario.EName.System_EnableMusicSync, //QOL but could also arguably be locked
        };

        public static List<Scenario.EName> alwaysOffFlags = new List<Scenario.EName>() 
        { 
            Scenario.EName.System_Battle_DisableBadgeDrop 
        };


        //Queue for Rewards that should be given to the player via ReserveScenarioReward()
        public static Queue<QueuedReward> rewardQueue = new Queue<QueuedReward>();

        //Local storage for current newest day in game
        public static int furthestDayReached = -1;

        //The actual client object
        public static ArchipelagoClient client = new ArchipelagoClient();

        //State for which "main" scene is currently loaded
        private static bool inBattle = false;
        private static bool inField = false;

        //Whether we are connecting to the server already
        private bool _connecting = false;
        //Storage if the goal has been achieved
        private static bool _goalAchieved = false;
        //Locations checked in game that need to be send to the server once possible
        public static Queue<long> pendingLocations = new();
        //
        public static string seed = string.Empty;


        public override void OnInitializeMelon()
        {
            Config.Load(); //Load config with hostname, port, slotname, and password


            if (!client.AttemptConnectionSync())
            {
                MelonLogger.Error("Failed to connect to the Archipelago server.");
                MelonLogger.Error("Please check your configuration and restart the game.");

                Application.Quit();
                return;
            }


            //Loading local save data and assigning values based on it
            seed = client.session.RoomState.Seed;
            ModSave.Load(seed);

            _goalAchieved = ModSave.Data.goalAchieved;
            while (ModSave.Data.pendingLocations.Count > 0)
            {
                pendingLocations.Enqueue(ModSave.Data.pendingLocations.Dequeue());
            }
            while (ModSave.Data.rewardQueue.Count > 0)
            {
                rewardQueue.Enqueue(ModSave.Data.rewardQueue.Dequeue());
            }

            ArchipelagoData.LoadStaticLocations();

            LoggerInstance.Msg("Initialized.");
        }

        public override void OnUpdate()
        {
            if (!client.IsConnected && !_connecting)
            {//Reconnect to the client if we are no longer connected
                _connecting = true;
                _ = TryReconnect();
            }
            if (client.IsConnected && !ArchipelagoData.DataLoaded)
            { // if we are connected get location data from the server, to edit game data
                ArchipelagoData.DataLoaded = true;
                LoadArchipelagoData();
            }
            if (client.IsConnected && pendingLocations.Count > 0)
            { //If we have locations we need to do send to the server
                FlushPendingLocations();
            }
            if (client.IsConnected && _goalAchieved)
            { //Inform the server that we reached our goal
                client.session.SetGoalAchieved();
            }

            if (!inBattle && inField)
            { //Only if the battle scene is not loaded and the field scene is loaded


                if (!initalized)
                {//Initialize Values on Load/ Start Save
                    initalized = true;

                    furthestDayReached = SaveLoadController.Get<SaveDataField>().GetNewestDateDay();
                    MelonLogger.Msg($"Current Newest Day is {furthestDayReached}");
                }

                if (SaveLoadController.Get<SaveDataField>().GetTipsFlag(Tips.ELabel.Tips_0003))
                {
                    for (int i = 0; i < alwaysOnFlags.Count; i++)
                    {
                        Scenario.EName flag = alwaysOnFlags[i];
                        bool currentValue = SaveLoadController.Get<SaveDataField>().GetScenarioFlag(flag);
                        if (currentValue != true)
                        {
                            SaveLoadController.Get<SaveDataField>().SetScenarioFlag(flag, true);
                            //MelonLogger.Msg($"Set {flag} to true");
                        }
                    }
                    for (int i = 0; i < alwaysOffFlags.Count; i++)
                    {
                        Scenario.EName flag = alwaysOffFlags[i];
                        bool currentValue = SaveLoadController.Get<SaveDataField>().GetScenarioFlag(flag);
                        if (currentValue != false)
                        {
                            SaveLoadController.Get<SaveDataField>().SetScenarioFlag(flag, false);
                            //MelonLogger.Msg($"Set {flag} to false");
                        }
                    }
                }
            
                


                //TODO: Move this out of field only
                while (client.pendingItems.Count > 0)
                { //If we are to receive items from the client, and have done the work to be able to receive them
                    
                    PendingItem receivedItem = client.pendingItems.Dequeue();
                    //MelonLogger.Msg($"Received Item: {receivedItem.Item.ItemName} at index {receivedItem.Index}");
                    if (receivedItem.Index <= ModSave.Data.LastProcessedItemIndex)
                    { // Only receive item with an index higher than the last item we received
                        continue;
                    }
                    long rewardID = ArchipelagoData.ReceivableRewards[receivedItem.Item.ItemId];
                    rewardQueue.Enqueue(new QueuedReward((int)rewardID, receivedItem.Index));
                }

                //Run Reward Queue
                while (rewardQueue.Count > 0 && FieldManager.Instance.IsPromptStatus())
                {//If rewards are queued and we can trigger a reward pop-up
                    QueuedReward rewardToTrigger = rewardQueue.Dequeue();
                    //MelonLogger.Msg(rewardToTrigger);
                    FieldManager.Instance.ReserveScenarioReward((ScenarioRewards.ELabel)rewardToTrigger.RewardID, 1);
                    ModSave.Data.LastProcessedItemIndex = Math.Max(ModSave.Data.LastProcessedItemIndex, rewardToTrigger.ItemIndex);
                    ModSave.Save();
                }
            }

           


            if (DEBUG && Input.GetKeyDown(KeyCode.F8))
            {
                MelonLogger.Msg("F8 pressed");


                // Triggers Firestorm D1 Reward
                // Does not work in battles! So properly needs to be in a waiting list or something for archipelago
                // Only Slot 1 Works??
                rewardQueue.Enqueue(new QueuedReward((int)Il2CppMaster.ScenarioRewards.ELabel.Reward_1w1d_020,-1));
                //FieldManager.Instance.ReserveScenarioReward(Il2CppMaster.ScenarioRewards.ELabel.Reward_1w1d_020, 1); //Firestorm
                //FieldManager.Instance.ReserveScenarioReward(Il2CppMaster.ScenarioRewards.ELabel.Reward_1w1d_010, 1); //Joli Becot
                rewardQueue.Enqueue(new QueuedReward((int)Il2CppMaster.ScenarioRewards.ELabel.Reward_Test_001,-1));
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

            
            if (Input.GetKeyDown(SKIP_DAY_BUTTON) && inField)
            {//This is the button that can be pressed to skip to the end of a already beaten day
                MelonLogger.Msg("F5 pressed");

                //Get clear location for the current game day
                int gameDay = SaveLoadController.Get<SaveDataField>().GetScenarioDateDay();
                int locationID = ScenarioFlagList.endOfDayFlag.FirstOrDefault(x => x.Value.Item1 == gameDay).Value.Item2;

                var checkedLocations = client.session.Locations.AllLocationsChecked;
                bool dayLocationReached = checkedLocations.Any(loc => loc == locationID);
                if (dayLocationReached)
                {//if we reached the clear location for a chapter/day
                    Scenario.EName[] flagList = ScenarioFlagList.flagsToBeatDay[gameDay];
                    for (int i = 0; i < flagList.Length; i++)
                    { // Set all flags for the day
                        SaveLoadController.Get<SaveDataField>().SetScenarioFlag(flagList[i], true);
                    }
                    //Go to the current newest day
                    FieldManager.Instance.MainMenuToChapterSelect(furthestDayReached);
                }
                else
                {
                    MelonLogger.Msg($"{gameDay} has not yeen beaten");
                }


            }

            if (DEBUG && Input.GetKeyDown(KeyCode.F6))
            {
                MelonLogger.Msg("F6 pressed");

                furthestDayReached++;
                MelonLogger.Msg(furthestDayReached);
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

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            try
            { //Change the current scene states
                if (DEBUG) { MelonLogger.Msg($"Scene {sceneName} was loaded."); }
                if (sceneName == "Battle")
                { inBattle = true; }
                else if (sceneName == "Field")
                { inField = true; }

            } catch (Exception e){MelonLogger.Msg($"Scene Load {e}");}
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasUnloaded(buildIndex, sceneName);
            try
            { //Change the current scene states
                if (DEBUG) { MelonLogger.Msg($"Scene {sceneName} was unloaded."); }
                if (sceneName == "Battle")
                { inBattle = false;}
                else if (sceneName == "Field")
                { inField = false;}
            } catch (Exception e) {  MelonLogger.Msg($"Scene Unload {e}"); }
        }

        public override void OnApplicationQuit()
        {
            if (pendingLocations.Count > 0)
            { //Before closing the game and we might need to store some information that could not be sent to server
                foreach (var location in pendingLocations)
                {
                    if (!ModSave.Data.pendingLocations.Contains(location))
                        ModSave.Data.pendingLocations.Enqueue(location);
                }
            }
            if(rewardQueue.Count > 0)
            {
                foreach(var reward in rewardQueue)
                {
                    if (!ModSave.Data.rewardQueue.Contains(reward))
                        ModSave.Data.rewardQueue.Enqueue(reward);
                }
            }
            ModSave.Data.goalAchieved = _goalAchieved;
            ModSave.Save();

        }

        public static void CheckEndOfChapterReward(Scenario.EName scenarioFlag, bool value)
        { //Based on certain scenarioFlags trigger the respective end of day location
            if (ScenarioFlagList.endOfDayFlag.ContainsKey(scenarioFlag) && value == true)
            {
                var (day, rewardID) = ScenarioFlagList.endOfDayFlag[scenarioFlag];
                Core.queueNonStandardReward(rewardID);

                var gameDay = SaveLoadController.Get<SaveDataField>().GetNewestDateDay();
                int collectedSecretReports = CountSecretReports();
                MelonLogger.Msg($"End Day {gameDay} with {collectedSecretReports} Reports and furthest day {furthestDayReached}");
                if(gameDay <= collectedSecretReports && gameDay == furthestDayReached)
                {
                    furthestDayReached++;
                }
                if (furthestDayReached == 3) { _goalAchieved = true; }
            }
        }

        public static int CountSecretReports()
        { // Count the currently collected secret reports
            int count = 0;
            SaveDataRecord record = SaveLoadController.Get<SaveDataRecord>();
            if (record == null)
                return 0;
            foreach (Book book in MasterDataBase<Book>.Array)
            {
                if (book != null && book.IsSecretReport())
                {
                    if (record.IsGetItem(book.mItemId))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private async void LoadArchipelagoData()
        {//Get Locations in our game and what items are in them
            long[] locations = client.session.Locations.AllLocations.ToArray();

            MelonLogger.Msg($"Requesting {locations.Length} locations");

            Dictionary<long, ScoutedItemInfo> items =
                await client.session.Locations.ScoutLocationsAsync(false, locations);

            MelonLogger.Msg($"Received {items.Count} locations");

            ArchipelagoData.LoadArchipelagoLocations(items);
        }

        public static void queueNonStandardReward(int rewardID)
        {//Queue a reward that can't be normally saved by the game
            bool nonStandardInSaveFile = ModSave.Data.nonStandardLocations.Contains((long)rewardID);
            bool nonStandardChecked = client.session.Locations.AllLocationsChecked.Contains((long)rewardID);


            if (!nonStandardInSaveFile)
            {// If the location has not been recorded in local mod save file
                Core.rewardQueue.Enqueue(new QueuedReward(rewardID, -1));
                ModSave.Data.nonStandardLocations.Add(rewardID);
            } else if (nonStandardInSaveFile && !nonStandardChecked)
            {//If the location is in local mod save file but not checked in archipelago
                pendingLocations.Enqueue((long)rewardID);
            }
        }

        private async Task TryReconnect()
        { //Try to Reconnect to the server asynchronously
            try
            {
                await client.AttemptConnectionAsync();
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
            }
            finally
            {
                _connecting = false;
            }
        }

        private static void FlushPendingLocations()
        {//Send locations checked in game to the server

            while (pendingLocations.Count > 0)
            {
                Core.client.session.Locations.CompleteLocationChecks([pendingLocations.Dequeue()]);
            }


            pendingLocations.Clear();
        }
    }

    public class QueuedReward
    {//This class is used for rewards received from the server
        public int RewardID { get; set; }
        public long ItemIndex { get; set; }
        public QueuedReward(int rewardID, long itemIndex)
        {
            RewardID = rewardID;
            ItemIndex = itemIndex;
        }

        public override string ToString()
        {
            return RewardID.ToString();
        }
    }
}
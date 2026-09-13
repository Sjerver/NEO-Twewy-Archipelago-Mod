using Archipelago.MultiClient.Net.Models;
using Il2Cpp;
using Il2CppCriMana;
using Il2CppInterop.Runtime;
using Il2CppMaster;
using MelonLoader;
using System.Diagnostics;
using UnityEngine;
using static Il2Cpp.ScenarioFlowComicComponent.Starter;
using static Il2Cpp.SortWindow;
using static Il2CppUI.Battle.BattlePauseSelectController;

[assembly: MelonInfo(typeof(NEOTwewyArchipelagoMod.Core), "NEOTwewyArchipelagoMod", "0.0.4", "sjerver", null)]
[assembly: MelonGame("SQUARE ENIX", "NEO: The World Ends with You")]

namespace NEOTwewyArchipelagoMod
{
    public class Core : MelonMod
    {
        //Essentially static constants
        public static string GAME_NAME = "NEO: The World Ends with You";
        public static long ARCHIPELAGO_ITEM_ID = 5000;

        public static bool DEBUG = false;
        public static bool ENEMY_HP_1 = false;

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
        };

        public static List<Scenario.EName> alwaysOffFlags = new List<Scenario.EName>()
        {
            Scenario.EName.System_Battle_DisableBadgeDrop
        };

        //Current save object
        public static ModSave save = new ModSave();

        //Queue to display pop-up for items we send that otherwise dont give any notification to our player
        public static Queue<ArchipelagoItem> archiItemDisplayQueue = new Queue<ArchipelagoItem>();
        //Are we already processing an item to display?
        public static bool processingArchiItem = false;

        //Local storage for current newest day in game
        public static int furthestDayReached = -1;

        //The actual client object
        public static ArchipelagoClient client = new ArchipelagoClient();

        //State for which "main" scene is currently loaded
        private static bool inBattle = false;
        private static bool inField = false;

        public static RewardSafety rewardSafety = new();
        public static bool lastRewardSafety = false;

        //Whether we are connecting to the server already
        private bool _connecting = false;

        //Current state on whether we can sync local save data
        public static SyncState syncState = SyncState.Offline;

        public override void OnInitializeMelon()
        {
            Config.Load(); //Load config with hostname, port, slotname, and password


            if (!client.AttemptConnectionSync())
            {
                MelonLogger.Error("Failed to connect to the Archipelago server.");
                MelonLogger.Error("Please check your configuration and restart the game.");
            }
            updateSyncState();

            //Assemble new rewards for every item so archipelago can hand them out
            ArchipelagoData.AssembleNewRewards();
            PigLocator.Initialize();

            LoggerInstance.Msg("Initialized.");
        }

        public override void OnUpdate()
        {
            if (!client.IsConnected && !_connecting)
            {//Reconnect to the client if we are no longer connected
                _connecting = true;
                MelonLogger.Msg("Attempting to reconnect to the Archipelago server...");
                _ = TryReconnect();
            }

            


            if (!inBattle && inField)
            { //Only if the battle scene is not loaded and the field scene is loaded
                if (syncState == SyncState.Ready && initalized)
                {
                    if (client.IsConnected && save.getPendingLocationSize() > 0)
                    {//If we have locations we need to do send to the server
                        Core.client.session.Locations.CompleteLocationChecks([save.dequeueLocation().Value]);
                    }

                    if (client.IsConnected && save.getGoalAchieved())
                    { //Inform the server that we reached our goal
                        client.session.SetGoalAchieved();
                    }
                    if (client.pendingItems.Count > 0)
                    { //If we are to receive items from the client, and have done the work to be able to receive them

                        ProcessPendingItems();
                    }
                }


                if (!initalized && syncState != SyncState.WrongSeed)
                {//Initialize Values on Load/ Start Save
                    initalized = true;

                    furthestDayReached = SaveLoadController.Get<SaveDataField>().GetNewestDateDay();

                    if (ModOptions.soundsurf)
                    {//Enable soundsurf if the option is set
                        alwaysOnFlags.Add(Scenario.EName.System_EnableMusicSync);
                    }

                }

                if (SaveLoadController.Get<SaveDataField>().GetTipsFlag(Tips.ELabel.Tips_0003))
                {//Only once we can ensure that we have at least some pins equipped
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

                rewardSafety.safeStatusCheck();
                if (save.getRewardQueueSize() > 0)
                {//Outside if needed to prevent checking this superflously
                    //Run Reward Queue
                    
                    if (rewardSafety && save.getRewardQueueSize() > 0 && syncState != SyncState.WrongSeed)
                    {//If rewards are queued and we can trigger a reward pop-up
                        QueuedReward rewardToTrigger = save.dequeueReward();
                        //MelonLogger.Msg(rewardToTrigger);
                        FieldManager.Instance.ReserveScenarioReward((ScenarioRewards.ELabel)rewardToTrigger.RewardID, 1);
                        save.Save();
                    }
                }
                if (archiItemDisplayQueue.Count > 0) {
                    if (rewardSafety && archiItemDisplayQueue.Count > 0 && !processingArchiItem && FieldManager.Instance.IsMoveStatus())
                    {
                        ProcessNextArchipelagoItem();
                    }
                }

            }
            if (DEBUG && Input.GetKeyDown(KeyCode.F8))
            {
                MelonLogger.Msg("F8 pressed");
                int rewardID = ArchipelagoData.ReceivableRewards[10205].reward_ID; //Joli Becot
                save.enqueueReward(new QueuedReward(rewardID, 1000));
                rewardID = ArchipelagoData.ReceivableRewards[31006].reward_ID;
                save.enqueueReward(new QueuedReward(rewardID, 1000));

            }


            if (Input.GetKeyDown(SKIP_DAY_BUTTON) && inField)
            {//This is the button that can be pressed to skip to the end of a already beaten day
                MelonLogger.Msg($"{SKIP_DAY_BUTTON} pressed");

                //Get clear location for the current game day
                int gameDay = SaveLoadController.Get<SaveDataField>().GetScenarioDateDay();
                GameLocationID locationID = ScenarioFlagList.endOfDayFlag.FirstOrDefault(x => x.Value.Item1 == gameDay).Value.Item2;
                ArchipelagoLocationID aLocationID = (ArchipelagoLocationID)ArchipelagoData.GetArchipelagoLocation(locationID, LocationType.ScenarioReward);
                bool dayLocationReached = save.IsLocationChecked(aLocationID);
                if (dayLocationReached)
                {
                    FieldManager.Instance.m_FieldSequenceComponent.ChangeState(new Action(FieldManager.Instance.m_FieldSequenceComponent.State_NextDayFadeout));
                }
                else
                {
                    MelonLogger.Msg($"{gameDay} has not yet been beaten");
                }


            }

            if (DEBUG && Input.GetKeyDown(KeyCode.F6))
            {
                MelonLogger.Msg("F6 pressed");
                int rewardID = ArchipelagoData.ReceivableRewards[31000 + (furthestDayReached - 1)].reward_ID;
                save.enqueueReward(new QueuedReward(rewardID, 1000));


            }
        }

        public void ProcessPendingItems()
        {
            PendingItem receivedItem = client.pendingItems.Dequeue();
            //MelonLogger.Msg($"Received Item: {receivedItem.Item.ItemName} at index {receivedItem.Index}");
            if (receivedItem.Index <= save.getLastItemIndex())
            { // Only receive item with an index higher than the last item we received
                return;
            }
            if (receivedItem.Item.LocationGame == GAME_NAME && receivedItem.Item.Player.Name == Config.Data.slotName)
            {//Make sure to only check our location if the received item is from one of ours
                ArchipelagoLocationID aLocationID = new ArchipelagoLocationID(receivedItem.Item.LocationId);
                LocationType type = ArchipelagoData.GetLocationType(aLocationID);
                GameLocationID gLocationID = ArchipelagoData.GetGameLocation(aLocationID);
                if (save.TryGetArchipelagoItem(aLocationID, out ArchipelagoItem archiItem))
                {
                    if (type == LocationType.ShopGood)
                    {//If the location is a shop location
                     //int goodID = (int)archiItem.locationID - (int)NEOConstants.shopLocationIDModifier;
                     //int purchaseCount = SaveLoadController.Get<SaveDataShop>().GetShopGoodsPurchases((ShopGoods.ELabel)goodID);

                        if (!NEOTwewyDataManager.NON_SHOP_ITEMS.Contains(archiItem.id) && save.IsLocationChecked(aLocationID))
                        {
                            //Don't give the item if the shop can give you the item normally and the location has been checked
                            return;
                        }
                    }

                    if ((type == LocationType.NoiseDrop || type == LocationType.PigDrop))
                    {
                        //MelonLogger.Msg($"{(int)archiItem.id} {archiItem.name} {archiItem.itemGame}");
                        AllItems itemObject = MasterDataBase<AllItems>.Get((int)archiItem.id);
                        ItemConst.EItemType itemType = itemObject.ItemType;
                        //MelonLogger.Msg($"{archiItem.name} at {archiItem.locationName} isBadge {itemType == ItemConst.EItemType.Badge} " +
                        //$"count {archiItem.count} archiBadge {archiItem.id != ARCHIPELAGO_ITEM_ID} locationChecked {save.IsLocationChecked(aLocationID)}");
                        if (itemType == ItemConst.EItemType.Badge && archiItem.id != ARCHIPELAGO_ITEM_ID && archiItem.count == 1 && save.IsLocationChecked(aLocationID))
                        {
                            //MelonLogger.Msg($"{archiItem.name} at {archiItem.locationName} has been gotten normally already");
                            //Don't give the item if it is a badge, not the archipelago badge, has count 1 and has been checked already
                            //In these cases noise / pig drops give you the item normally
                            return;
                        }
                    }
                    if (type == LocationType.DiveReward)
                    {
                        MelonLogger.Msg($"{archiItem.name} at {archiItem.locationName}" +
                            $"count {archiItem.count} archiBadge {archiItem.id != ARCHIPELAGO_ITEM_ID} locationChecked {save.IsLocationChecked(aLocationID)}");
                        if (((archiItem.id != ARCHIPELAGO_ITEM_ID && archiItem.count == 1) || archiItem.id == NEOTwewyDataManager.DIVE_FP_ID)
                            && save.IsLocationChecked(aLocationID))
                        {
                            //MelonLogger.Msg($"{archiItem.name} at {archiItem.locationName} has been gotten normally already");
                            //Don't give the item if its not an archipelago item and count is 1 or if its FP since dive reward can give you it
                            return;
                        }
                    }

                }

            }
            else
            {
                //MelonLogger.Msg($"Received item from {receivedItem.Item.LocationGame} {receivedItem.Item.ItemName}");
            }

            long rewardID = ArchipelagoData.ReceivableRewards[receivedItem.Item.ItemId].reward_ID;
            int itemCount = ArchipelagoData.ReceivableRewards[receivedItem.Item.ItemId].count;
            save.enqueueReward(new QueuedReward((int)rewardID, receivedItem.Index, itemCount));
            save.setLastItemIndex(Math.Max(save.getLastItemIndex(), receivedItem.Index));
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
                { 
                    inField = true;
                    //Assume we are leaving a battle here, since we are in the field scene now
                    rewardSafety.assumeBattle = false;
                    //MelonLogger.Msg($"Assuming battle is over since we are in the field scene now");
                }

            }
            catch (Exception e) { MelonLogger.Msg($"Scene Load {e}"); }
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasUnloaded(buildIndex, sceneName);
            try
            { //Change the current scene states
                if (DEBUG) { MelonLogger.Msg($"Scene {sceneName} was unloaded."); }
                if (sceneName == "Battle")
                { inBattle = false; }
                else if (sceneName == "Field")
                { inField = false;
                    rewardSafety.assumeBattle = true;
                    //MelonLogger.Msg($"Assuming battle since we are leaving the field scene now");
                }
            }
            catch (Exception e) { MelonLogger.Msg($"Scene Unload {e}"); }
        }

        public override void OnApplicationQuit()
        {
            if (syncState == SyncState.WrongSeed) { return; }
            save.Save();

        }

        public static bool CheckUnlockNewChapterEmpty()
        {
            return CheckUnlockNewChapter(0);
        }
        public static bool CheckUnlockNewChapter(int additionalSecretReports = 0)
        {
            if (syncState == SyncState.WrongSeed) { return false; }
            //MelonLogger.Msg($"CheckUnlockNewChapter with {additionalSecretReports} additional secret reports");

            int gameDay = furthestDayReached;
            GameLocationID locationID = ScenarioFlagList.endOfDayFlag.FirstOrDefault(x => x.Value.Item1 == gameDay).Value.Item2;
            ArchipelagoLocationID aLocationID = locationID.ToArchipelagoLocation(LocationType.ScenarioReward);
            bool dayLocationReached = save.IsLocationChecked(aLocationID);
            if (dayLocationReached)
            {
                int collectedSecretReports = CountSecretReports() + additionalSecretReports;
                /**
                 * The main reason the day needs to be finished is because certain things dont work on revisting a day like
                 * - character joins
                 * - vending machine?
                 * 
                 * But this way we can make sure it works because you are guaranteed to trigger the respective 
                 * scenario rewards at least once before finishing the day 1 time!
                **/
                MelonLogger.Msg($"End Day {gameDay} with {collectedSecretReports} Reports and furthest day {furthestDayReached}");
                if (gameDay <= collectedSecretReports)
                {
                    furthestDayReached++;
                    if (furthestDayReached == 5) { save.setGoalAchieved(true); }
                    return true;
                }
                
            }
            return false;
        }

        public static void CheckEndOfChapterReward(Scenario.EName scenarioFlag, bool value)
        { //Based on certain scenarioFlags trigger the respective end of day location
            if (syncState == SyncState.WrongSeed) { return; }
            if (ScenarioFlagList.endOfDayFlag.ContainsKey(scenarioFlag) && value == true)
            {
                (int day, GameLocationID rewardID) = ScenarioFlagList.endOfDayFlag[scenarioFlag];
                ArchipelagoLocationID archipelagoLocation = ArchipelagoData.GetArchipelagoLocation(rewardID, LocationType.ScenarioReward);
                //Check if custom location contains secret report, if so we want to count as one more day
                int additionalSecretReport = 0;
                //MelonLogger.Msg($"Location {archipelagoLocation} checked? {save.IsLocationChecked(archipelagoLocation)} -> Item? {save.TryGetScenarioRewardItem(rewardID, out ArchipelagoItem sd)} -> {sd.name}");
                if (!save.IsLocationChecked(archipelagoLocation) && save.TryGetArchipelagoItem(archipelagoLocation, out ArchipelagoItem item))
                {
                    //MelonLogger.Msg($"Reward 1st Time {item.IsItemFromOurSlot()} && {item.name} && {item.name.Contains("Secret Report")}");
                    if (item.IsItemFromOurSlot() && item.name.Contains("Secret Report No."))
                    {
                        additionalSecretReport += 1;
                        //MelonLogger.Msg("Manual secret report count in chapter end");
                    }
                    if (item.id == ARCHIPELAGO_ITEM_ID && !save.IsLocationChecked(archipelagoLocation))
                    {
                        archiItemDisplayQueue.Enqueue(item);
                    }

                }
                queueCustomLocation(archipelagoLocation);
                CheckUnlockNewChapter(additionalSecretReport);
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

        public static async Task GetArchipelagoData()
        {//Get Locations in our game and what items are in them
            long[] locations = client.session.Locations.AllLocations.ToArray();

            MelonLogger.Msg($"Requesting {locations.Length} locations");

            Dictionary<long, ScoutedItemInfo> items =
                await client.session.Locations.ScoutLocationsAsync(false, locations);

            MelonLogger.Msg($"Received {items.Count} locations");

            save.SaveArchipelagoLocations(items);
        }

        public static void queueCustomLocation(ArchipelagoLocationID locationID)
        {//Queue a reward that can't be normally saved by the game
            if (syncState == SyncState.WrongSeed) { return; }
            save.enqueueLocation(locationID);
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

                updateSyncState();
            }
        }

        public void updateSyncState()
        {
            if (!client.IsConnected)
            {
                //MelonLogger.Msg("Can#t get server seed");
                syncState = SyncState.Offline;
            }
            else if (save.getSeed() == client.session.RoomState.Seed)
            {
                //MelonLogger.Msg("Current Seed does match save file");
                syncState = SyncState.Ready;
            }
            else
            {
                syncState = SyncState.WrongSeed;
                if (save.getSeed() == null)
                {

                    MelonLogger.MsgDirect(MelonLoader.Logging.ColorARGB.Red, "Current Seed is null");
                }
                else
                {
                    MelonLogger.MsgDirect(MelonLoader.Logging.ColorARGB.Red, "Current Seed does not match save file");
                }



            }
        }

        private static void ProcessNextArchipelagoItem()
        {
            if (processingArchiItem)
                return;
            
            if (!FieldManager.Instance.IsPromptStatus() || !FieldManager.Instance.IsMoveStatus())
                return;

            if (archiItemDisplayQueue.Count == 0)
                return;

            processingArchiItem = true;

            ArchipelagoItem item = archiItemDisplayQueue.Dequeue();
            //MelonLogger.Msg($"Archipelago dialog open call for {item.name} {item.id} from {item.locationName}");
            FieldManager.Instance.m_FieldPlayer.SetStateFlag(FieldPlayer.EPlayerStateFlag.DisableInput, true);
            DialogGetUI.OpenItemGetDialog(
                (AllItems.ELabel)item.id,
                -((int)item.locationID.Value), //Gets passed as negative location value to distinguish it from "real" item
                DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(OnArchipelagoUIEnd),
                DelegateSupport.ConvertDelegate<Il2CppSystem.Action<DialogGetUI>>(OnArchipelagoDialogEnded)
                );
        }

        private static void OnArchipelagoUIEnd()
        {
            //MelonLogger.Msg("Archipelago UI ended");
            processingArchiItem = false;
            FieldManager.Instance.m_FieldPlayer.SetStateFlag(FieldPlayer.EPlayerStateFlag.DisableInput, false);
            ProcessNextArchipelagoItem();
        }

        private static void OnArchipelagoDialogEnded(DialogGetUI dialog)
        {
            //MelonLogger.Msg("Archipelago dialog ended");

        }

    }

    public class QueuedReward{//This class is used for rewards received from the server
        public int RewardID { get; set; }
        public long ItemIndex { get; set; }
        public int itemCount { get; set; }
        public QueuedReward(int rewardID, long itemIndex, int itemCount = 1)
        {
            RewardID = rewardID;
            ItemIndex = itemIndex;
            this.itemCount = itemCount;
        }

        public override string ToString()
        {
            return RewardID.ToString();
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
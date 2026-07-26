using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod
{
    public  class ArchipelagoClient
    {
        public ArchipelagoSession session;

        public LoginSuccessful LoginInfo;
        public bool IsConnected { get; private set; } = false;

        //Queue of items the client received from the server
        public Queue<PendingItem> pendingItems = new();

        public void AttemptConnectionSync()
        { //Attempt to connect synchronously at boot
            try
            {
                session = ArchipelagoSessionFactory.CreateSession(Config.Data.hostName, Config.Data.port);
            } catch (Exception e)
            {
                MelonLogger.Msg($"Failed to create session: {e.GetBaseException().Message}");
                return; // Did not connect, show the user the contents of `errorMessage`
            }
            SetupSubscriptions();

            Connect($"{Config.Data.hostName}:{Config.Data.port}", $"{Config.Data.slotName}", $"{Config.Data.password}");
        }

        public async Task AttemptConnectionAsync()
        {//Attempt to connect asynchronously whenever we need to reconnect
            try
            {
                session = ArchipelagoSessionFactory.CreateSession(Config.Data.hostName, Config.Data.port);
            }
            catch (Exception e)
            {
                MelonLogger.Msg($"Failed to create session: {e.GetBaseException().Message}");
                return; // Did not connect, show the user the contents of `errorMessage`
            }

            SetupSubscriptions();

            await ConnectAndLoginAsync($"{Config.Data.hostName}:{Config.Data.port}", $"{Config.Data.slotName}", $"{Config.Data.password}");
        }

        private async Task ConnectAndLoginAsync(string server, string user, string pass)
        {
            try
            {
                await session.ConnectAsync();

                var result = await session.LoginAsync(
                    Core.GAME_NAME,
                    user,
                    ItemsHandlingFlags.AllItems
                );

                if (result is LoginSuccessful)
                {
                    IsConnected = true;
                    MelonLogger.Msg("Connected!");
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                IsConnected = false;
            }
        }

        private void Connect(string server, string user, string pass)
        {
            LoginResult result;

            try
            {
                // handle TryConnectAndLogin attempt here and save the returned object to `result`
                result = session.TryConnectAndLogin(Core.GAME_NAME, user, ItemsHandlingFlags.AllItems);
            }
            catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (!result.Successful)
            {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"Failed to Connect to {server} as {user}:";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }
                MelonLogger.Msg($"{errorMessage}");
                return; // Did not connect, show the user the contents of `errorMessage`
            }

            // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be
            // used to interact with the server and the returned `LoginSuccessful` contains some useful information about the
            // initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
            LoginInfo = (LoginSuccessful)result;
            IsConnected = LoginInfo.Successful;
            MelonLogger.Msg("Connected!");
            MelonLogger.Msg($"Player: {LoginInfo.Slot}");
            MelonLogger.Msg($"Team: {LoginInfo.Team}");
        }

        public void SetupSubscriptions()
        {
            session.Items.ItemReceived += OnItemReceived;
            session.Socket.SocketClosed += OnSocketClosed;
            session.Socket.ErrorReceived += OnSocketError;
            session.MessageLog.OnMessageReceived += OnMessageReceived;
        }

        private void OnSocketClosed(string reason)
        {
            MelonLogger.Warning($"Disconnected from Archipelago: {reason}");

            IsConnected = false;
        }

        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            var item = helper.PeekItem();
            if(Core.DEBUG) { MelonLogger.Msg($"Queued {item.ItemName}"); }
            
            pendingItems.Enqueue(new PendingItem(helper.DequeueItem(),helper.Index));

        }

        private void OnSocketError(Exception e,string message)
        {
            MelonLogger.Error($"Archipelago socket error: {message}");
            IsConnected = false;
        }

        private void OnMessageReceived(LogMessage message)
        {
            MelonLogger.Msg($"[Archipelago] {message}");
        }
    }

    public class PendingItem(ItemInfo item, int index)
    {
        public ItemInfo Item = item;
        public int Index = index;
    }
}

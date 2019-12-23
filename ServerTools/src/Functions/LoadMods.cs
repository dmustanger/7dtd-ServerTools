using System.Data;

namespace ServerTools
{
    public class Mods
    {
        public static bool Startup = false;

        public static void Load()
        {
            Timers.TimerStart();
            Timers.Timer2Start();
            string _sql = "SELECT pollOpen FROM Polls WHERE pollOpen = 'true'";
            DataTable _result = SQL.TypeQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                PollConsole.Check();
            }
            _result.Dispose();
            if (!ClanManager.IsEnabled)
            {
                ClanManager.Clans.Clear();
                ClanManager.ClanMember.Clear();
            }
            if (!InfoTicker.IsEnabled && InfoTicker.IsRunning)
            {
                InfoTicker.Unload();
            }
            if (InfoTicker.IsEnabled && !InfoTicker.IsRunning)
            {
                InfoTicker.Load();
            }
            if (Gimme.IsRunning && !Gimme.IsEnabled)
            {
                Gimme.Unload();
            }
            if (!Gimme.IsRunning && Gimme.IsEnabled)
            {
                Gimme.Load();
            }
            if (Badwords.IsRunning && !Badwords.IsEnabled)
            {
                Badwords.Unload();
            }
            if (!Badwords.IsRunning && Badwords.IsEnabled)
            {
                Badwords.Load();
            }
            if (!LoginNotice.IsRunning && LoginNotice.IsEnabled)
            {
                LoginNotice.Load();
            }
            if (LoginNotice.IsRunning && !LoginNotice.IsEnabled)
            {
                LoginNotice.Unload();
            }
            if (!Zones.IsRunning && Zones.IsEnabled)
            {
                Zones.Load();
            }
            if (Zones.IsRunning && !Zones.IsEnabled)
            {
                Zones.Unload();
            }
            if (!VoteReward.IsRunning && VoteReward.IsEnabled)
            {
                VoteReward.Load();
            }
            if (VoteReward.IsRunning && !VoteReward.IsEnabled)
            {
                VoteReward.Unload();
            }
            if (!Watchlist.IsRunning && Watchlist.IsEnabled)
            {
                Watchlist.Load();
            }
            if (Watchlist.IsRunning && !Watchlist.IsEnabled)
            {
                Watchlist.Unload();
            }
            if (!ReservedSlots.IsRunning && ReservedSlots.IsEnabled)
            {
                ReservedSlots.Load();
            }
            if (ReservedSlots.IsRunning && !ReservedSlots.IsEnabled)
            {
                ReservedSlots.Unload();
            }
            if (!StartingItems.IsRunning && StartingItems.IsEnabled)
            {
                StartingItems.Load();
            }
            if (StartingItems.IsRunning && !StartingItems.IsEnabled)
            {
                StartingItems.Unload();
            }
            if (!Travel.IsRunning && Travel.IsEnabled)
            {
                Travel.Load();
            }
            if (Travel.IsRunning && !Travel.IsEnabled)
            {
                Travel.Unload();
            }
            if (!Shop.IsRunning && Shop.IsEnabled)
            {
                Shop.Load();
            }
            if (Shop.IsRunning && !Shop.IsEnabled)
            {
                Shop.Unload();
            }
            if (!Motd.IsRunning && Motd.IsEnabled)
            {
                Motd.Load();
            }
            if (Motd.IsRunning && !Motd.IsEnabled)
            {
                Motd.Unload();
            }
            if (InventoryCheck.IsRunning && !InventoryCheck.IsEnabled)
            {
                InventoryCheck.Unload();
            }
            if (!InventoryCheck.IsRunning && InventoryCheck.IsEnabled)
            {
                InventoryCheck.Load();
            }
            if (HighPingKicker.IsEnabled)
            {
                HighPingKicker.Load();
            }
            if (CredentialCheck.IsRunning && !CredentialCheck.IsEnabled)
            {
                CredentialCheck.Unload();
            }
            if (!CredentialCheck.IsRunning && CredentialCheck.IsEnabled)
            {
                CredentialCheck.Load();
            }
            if (CustomCommands.IsRunning && !CustomCommands.IsEnabled)
            {
                CustomCommands.Unload();
            }
            if (!CustomCommands.IsRunning && CustomCommands.IsEnabled)
            {
                CustomCommands.Load();
            }
            if (DupeLog.IsRunning && !DupeLog.IsEnabled)
            {
                DupeLog.Unload();
            }
            if (!DupeLog.IsRunning && DupeLog.IsEnabled)
            {
                DupeLog.Load();
            }
            if (ChatColorPrefix.IsRunning && !ChatColorPrefix.IsEnabled)
            {
                ChatColorPrefix.Unload();
            }
            if (!ChatColorPrefix.IsRunning && ChatColorPrefix.IsEnabled)
            {
                ChatColorPrefix.Load();
            }
            if (KillNotice.IsRunning && !KillNotice.IsEnabled)
            {
                KillNotice.Unload();
            }
            if (!KillNotice.IsRunning && KillNotice.IsEnabled)
            {
                KillNotice.Load();
            }
            if (!Prayer.IsRunning && Prayer.IsEnabled)
            {
                Prayer.Load();
            }
            if (Prayer.IsRunning && !Prayer.IsEnabled)
            {
                Prayer.Unload();
            }
            if (LoadTriggers.IsRunning)
            {
                LoadTriggers.Unload();
            }
            if (!LoadTriggers.IsRunning)
            {
                LoadTriggers.Load();
            }
            if (ProtectedSpace.IsRunning)
            {
                ProtectedSpace.Unload();
            }
            if (!ProtectedSpace.IsRunning)
            {
                ProtectedSpace.Load();
            }
            if (ClanManager.IsEnabled)
            {
                ClanManager.ClanList();
            }
            if (AuctionBox.IsEnabled)
            {
                AuctionBox.AuctionList();
            }
            if (Mute.IsEnabled)
            {
                Mute.MuteList();
            }
            if (Jail.IsEnabled)
            {
                Jail.JailList();
            }
            if (BattleLogger.IsEnabled && !BattleLogger.LogFound && !string.IsNullOrEmpty(Utils.GetApplicationScratchPath()))
            {
                if (!GamePrefs.GetString(EnumGamePrefs.ServerDisabledNetworkProtocols).ToLower().Contains("litenetlib"))
                {
                    BattleLogger.LogDirectory = Utils.GetApplicationScratchPath();
                    BattleLogger.ConfirmLog();
                }
                else
                {
                    Log.Out("--------------------------------------------------------------------------");
                    Log.Out("[SERVERTOOLS] Unable to verify log file. Battle_Loggers has been disabled.");
                    Log.Out("[SERVERTOOLS] Network protocol litenetlib is required for this tool.");
                    Log.Out("--------------------------------------------------------------------");
                }
            }
            PatchTools.ApplyPatches();
        }
    }
}
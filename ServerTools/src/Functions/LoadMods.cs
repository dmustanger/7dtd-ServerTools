using System.Data;

namespace ServerTools
{
    public class Mods
    {
        public static void Load()
        {
            Timers.TimerStart();
            if (TeleportCheck.IsEnabled)
            {
                TeleportCheck.DetectionLogsDir();
            }
            if (CountryBan.IsEnabled)
            {
                CountryBan.Load();
            }
            if (FlightCheck.IsEnabled)
            {
                FlightCheck.DetectionLogsDir();
            }
            if (HatchElevator.IsEnabled)
            {
                HatchElevator.DetectionLogsDir();
            }
            if (PlayerLogs.IsEnabled)
            {
                PlayerLogs.PlayerLogsDir();
            }
            if (Report.IsEnabled)
            {
                Report.ReportLogsDir();
            }
            if (PlayerStatCheck.IsEnabled)
            {
                PlayerStatCheck.DetectionLogsDir();
            }
            if (UndergroundCheck.IsEnabled)
            {
                UndergroundCheck.DetectionLogsDir();
            }
            if (Bank.IsEnabled)
            {
                Bank.CreateFolder();
            }
            if (AuctionBox.IsEnabled)
            {
                AuctionBox.CreateFolder();
            }
            if (Bounties.IsEnabled)
            {
                Players.CreateFolder();
            }
            if (CredentialCheck.IsEnabled)
            {
                CredentialCheck.CreateFolder();
            }
            if (DupeLog.IsEnabled)
            {
                DupeLog.CreateFolder();
            }
            PollConsole.CreateFolder();
            string _sql = "SELECT pollOpen FROM Polls WHERE pollOpen = 'true'";
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                PollConsole.Check();
            }
            _result.Dispose();
            if (ChatHook.Special_Player_Name_Coloring)
            {
                ChatHook.SpecialIdCheck();
            }
            if (ClanManager.IsEnabled)
            {
                ClanManager.GetClans();
                ClanManager.BuildList();
            }
            if (!ClanManager.IsEnabled)
            {
                ClanManager.clans.Clear();
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
            if (UndergroundCheck.IsRunning && !UndergroundCheck.IsEnabled)
            {
                UndergroundCheck.Unload();
            }
            if (!UndergroundCheck.IsRunning && UndergroundCheck.IsEnabled)
            {
                UndergroundCheck.Load();
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
            if (MutePlayer.IsEnabled)
            {
                MutePlayer.MuteList();
            }
            if (Jail.IsEnabled)
            {
                Jail.JailList();
            }
            if (Animals.IsEnabled)
            {
                Animals.AnimalList();
            }
            if (AutoShutdown.IsEnabled)
            {
                AutoShutdown.ShutdownList();
            }
        }
    }
}
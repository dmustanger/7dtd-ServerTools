using System.Data;

namespace ServerTools
{
    public class Mods
    {
        public static bool Startup = false;

        public static void Load()
        {
            Timers.TimerStart();
            if (CountryBan.IsEnabled)
            {
                CountryBan.Load();
            }
            string _sql = "SELECT pollOpen FROM Polls WHERE pollOpen = 'true'";
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                PollConsole.Check();
            }
            _result.Dispose();
            if (ClanManager.IsEnabled)
            {
                ClanManager.GetClans();
                ClanManager.BuildList();
            }
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
            if (LoadTriggers.IsRunning)
            {
                LoadTriggers.Unload();
            }
            if (!LoadTriggers.IsRunning)
            {
                LoadTriggers.Load();
            }
            if (MutePlayer.IsEnabled)
            {
                MutePlayer.MuteList();
            }
            if (Jail.IsEnabled)
            {
                Jail.JailList();
            }
            if (AutoShutdown.IsEnabled)
            {
                AutoShutdown.ShutdownTime();
            }
        }
    }
}
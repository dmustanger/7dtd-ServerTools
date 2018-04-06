namespace ServerTools
{
    public class Mods
    {
        public static void Load()
        {
            Timers.TimerStart();
            if (Animals.IsEnabled)
            {
                Animals.BuildList();
            }
            if (AutoShutdown.IsEnabled)
            {
                AutoShutdown.ShutdownList();
            }
            if (ChatHook.Special_Player_Name_Coloring)
            {
                ChatHook.SpecialIdCheck();
            }
            if (ClanManager.IsEnabled)
            {
                PersistentContainer.Instance.Players.GetClans();
                ClanManager.BuildList();
            }
            if (!ClanManager.IsEnabled)
            {
                PersistentContainer.Instance.Players.clans.Clear();
                ClanManager.ClanMember.Clear();
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
            if (AuctionBox.IsEnabled)
            {
                AuctionBox.BuildAuctionList();
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
            if (!ZoneProtection.IsRunning && ZoneProtection.IsEnabled)
            {
                ZoneProtection.DetectionLogsDir();
                ZoneProtection.Load();
            }
            if (ZoneProtection.IsRunning && !ZoneProtection.IsEnabled)
            {
                ZoneProtection.Unload();
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
            if (!ReservedSlots.IsRunning && ReservedSlots.Donator_Name_Coloring)
            {
                ReservedSlots.Load();
            }
            if (ReservedSlots.IsRunning && !ReservedSlots.Donator_Name_Coloring)
            {
                ReservedSlots.Unload();
            }
            if (!StartingItems.IsRunning && StartingItems.IsEnabled)
            {
                StartingItems.BuildList();
                StartingItems.Load();
            }
            if (StartingItems.IsRunning && !StartingItems.IsEnabled)
            {
                StartingItems.Unload();
            }
            if (TeleportCheck.IsEnabled)
            {
                TeleportCheck.DetectionLogsDir();
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
            if (HighPingKicker.IsRunning && !HighPingKicker.IsEnabled)
            {
                HighPingKicker.Unload();
            }
            if (!HighPingKicker.IsRunning && HighPingKicker.IsEnabled)
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
            if (Muted.IsEnabled)
            {
                Muted.MuteList();
            }
            if (Jail.IsEnabled)
            {
                Jail.JailList();
            }
        }
    }
}
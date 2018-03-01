namespace ServerTools
{
    public class Mods
    {
        public static void Load()
        {
            if (AutoShutdown.IsEnabled)
            {
                AutoShutdown.TimerStart();
            }
            if (!AutoShutdown.IsEnabled)
            {
                AutoShutdown.TimerStop();
            }
            if (!AutoSaveWorld.IsEnabled)
            {
                AutoSaveWorld.StopTimer();
            }
            if (AutoSaveWorld.IsEnabled)
            {
                AutoSaveWorld.StartTimer();
            }
            if (Badwords.IsRunning && !Badwords.IsEnabled)
            {
                Badwords.Unload();
            }
            if (!Badwords.IsRunning && Badwords.IsEnabled)
            {
                Badwords.Load();
            }
            if (!Bloodmoon.IsEnabled)
            {
                Bloodmoon.TimerStop();
            }
            if (Bloodmoon.IsEnabled)
            {
                Bloodmoon.TimerStart();
            }
            if (ChatHook.Special_Player_Name_Coloring)
            {
                ChatHook.SpecialIdCheck();
            }
            if (ClanManager.IsEnabled)
            {
                PersistentContainer.Instance.Players.GetClans();
            }
            if (!ClanManager.IsEnabled)
            {
                PersistentContainer.Instance.Players.clans.Clear();
            }
            if (CustomCommands.IsRunning && !CustomCommands.IsEnabled)
            {
                CustomCommands.Unload();
            }
            if (!CustomCommands.IsRunning && CustomCommands.IsEnabled)
            {
                CustomCommands.Load();
            }
            if (EntityUnderground.IsEnabled)
            {
                EntityUnderground.EntityUndergroundTimerStart();
            }
            if (!EntityUnderground.IsEnabled)
            {
                EntityUnderground.EntityUndergroundTimerStop();
            }
            if (FamilyShareAccount.IsRunning && !FamilyShareAccount.IsEnabled)
            {
                FamilyShareAccount.Unload();
            }
            if (!FamilyShareAccount.IsRunning && FamilyShareAccount.IsEnabled)
            {
                FamilyShareAccount.Load();
            }
            if (FlightCheck.IsEnabled)
            {
                FlightCheck.FlightTimerStart();
                FlightCheck.DetectionLogsDir();
            }
            if (!FlightCheck.IsEnabled)
            {
                FlightCheck.FlightTimerStop();
            }
            if (Gimme.IsRunning && !Gimme.IsEnabled)
            {
                Gimme.Unload();
            }
            if (!Gimme.IsRunning && Gimme.IsEnabled)
            {
                Gimme.Load();
            }
            if (HatchElevator.IsEnabled)
            {
                HatchElevator.HatchElevatorTimerStart();
                HatchElevator.DetectionLogsDir();
            }
            if (!HatchElevator.IsEnabled)
            {
                HatchElevator.HatchElevatorTimerStop();
            }
            if (HighPingKicker.IsRunning && !HighPingKicker.IsEnabled)
            {
                HighPingKicker.Unload();
            }
            if (!HighPingKicker.IsRunning && HighPingKicker.IsEnabled)
            {
                HighPingKicker.Load();
            }
            if (HowToSetup.IsEnabled)
            {
                HowToSetup.HowTo();
            }
            if (!InfoTicker.IsEnabled)
            {
                InfoTicker.Unload();
            }
            if (InfoTicker.IsEnabled)
            {
                InfoTicker.Load();
            }
            if (InventoryCheck.IsRunning && !InventoryCheck.IsEnabled)
            {
                InventoryCheck.Unload();
            }
            if (!InventoryCheck.IsRunning && InventoryCheck.IsEnabled)
            {
                InventoryCheck.Load();
            }
            if (Jail.IsEnabled)
            {
                Jail.TimerStart();
            }
            if (!Jail.IsEnabled)
            {
                Jail.TimerStop();
            }
            if (!Shop.IsRunning  && Shop.IsEnabled)
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
            if (PlayerLogs.IsEnabled)
            {
                PlayerLogs.PlayerLogsStart();
                PlayerLogs.PlayerLogsDir();
            }
            if (!PlayerLogs.IsEnabled)
            {
                PlayerLogs.PlayerLogsStop();
            }
            if (PlayerStatCheck.IsEnabled)
            {
                PlayerStatCheck.PlayerStat();
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
            if (UndergroundCheck.IsEnabled)
            {
                UndergroundCheck.UndergroundTimerStart();
                UndergroundCheck.DetectionLogsDir();
            }
            if (!UndergroundCheck.IsEnabled)
            {
                UndergroundCheck.UndergroundTimerStop();
            }
            if (!VoteReward.IsRunning && VoteReward.IsEnabled)
            {
                VoteReward.Load();
            }
            if (VoteReward.IsRunning && !VoteReward.IsEnabled)
            {
                VoteReward.Unload();
            }
            if (VoteReward.IsEnabled)
            {
                VoteReward.RandomList();
            }
            if (!Watchlist.IsRunning && Watchlist.IsEnabled)
            {
                Watchlist.Load();
            }
            if (Watchlist.IsRunning && !Watchlist.IsEnabled)
            {
                Watchlist.Unload();
            }
            /*if (WorldRadius.IsEnabled)
            {
                WorldRadius.WorldRadiusTimerStart();
            }
            if (!WorldRadius.IsEnabled)
            {
                WorldRadius.WorldRadiusTimerStop();
            }*/
            if (!ZoneProtection.IsRunning && ZoneProtection.IsEnabled)
            {
                ZoneProtection.DetectionLogsDir();
                ZoneProtection.ZoneProtectionTimerStart();
                ZoneProtection.Load();
            }
            if (ZoneProtection.IsRunning && !ZoneProtection.IsEnabled)
            {
                ZoneProtection.ZoneProtectionTimerStop();
                ZoneProtection.Unload();
            }
        }
    }
}
namespace ServerTools
{
    public class Mods
    {
        public static void Load()
        {
            if (AutoRestart.IsEnabled)
            {
                AutoRestart.TimerStart();
            }
            if (!AutoRestart.IsEnabled)
            {
                AutoRestart.TimerStop();
            }
            if (AutoSaveWorld.IsRunning && !AutoSaveWorld.IsEnabled)
            {
                AutoSaveWorld.Stop();
            }
            if (!AutoSaveWorld.IsRunning && AutoSaveWorld.IsEnabled)
            {
                AutoSaveWorld.Start();
            }
            if (Badwords.IsRunning && !Badwords.IsEnabled)
            {
                Badwords.Unload();
            }
            if (!Badwords.IsRunning && Badwords.IsEnabled)
            {
                Badwords.Load();
            }
            if (Bloodmoon.IsRunning && !Bloodmoon.IsEnabled)
            {
                Bloodmoon.Unload();
            }
            if (!Bloodmoon.IsRunning && Bloodmoon.IsEnabled)
            {
                Bloodmoon.Load();
            }
            if (ChatHook.SpecialPlayerNameColoring)
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
            if (FlightCheck.IsEnabled)
            {
                FlightCheck.FlightTimerStart();
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
            if (HighPingKicker.IsRunning && !HighPingKicker.IsEnabled)
            {
                HighPingKicker.Unload();
            }
            if (!HighPingKicker.IsRunning && HighPingKicker.IsEnabled)
            {
                HighPingKicker.Load();
            }
            if (InfoTicker.IsRunning && !InfoTicker.IsEnabled)
            {
                InfoTicker.Unload();
            }
            if (!InfoTicker.IsRunning && InfoTicker.IsEnabled)
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
            if (!Jail.IsRunning && Jail.IsEnabled)
            {
                Jail.Load();
            }
            if (Jail.IsRunning && !Jail.IsEnabled)
            {
                Jail.Unload();
            }
            if (PlayerPositionLogs.IsEnabled)
            {
                PlayerPositionLogs.PlayerPositionLogsStart();
                PlayerPositionLogs.PlayerPositionLogsDir();    
            }
            if (!PlayerPositionLogs.IsEnabled)
            {
                PlayerPositionLogs.PlayerPositionLogsStop();
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
            if (!ReservedSlots.IsRunning && ReservedSlots.DonatorNameColoring)
            {
                ReservedSlots.Load();
            }
            if (ReservedSlots.IsRunning && !ReservedSlots.DonatorNameColoring)
            {
                ReservedSlots.Unload();
            }            
            if (UndergroundCheck.IsEnabled)
            {
                UndergroundCheck.UndergroundTimerStart();
            }
            if (!UndergroundCheck.IsEnabled)
            {
                UndergroundCheck.UndergroundTimerStop();
            }
            if (!Watchlist.IsRunning && Watchlist.IsEnabled)
            {
                Watchlist.Load();
            }
            if (Watchlist.IsRunning && !Watchlist.IsEnabled)
            {
                Watchlist.Unload();
            }
        }
    }
}
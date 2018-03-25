namespace ServerTools
{
    public class Mods
    {
        public static void Load()
        {
            if (AutoShutdown.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
            }
            if (!AutoShutdown.IsEnabled)
            {
                Timers.TimerCheck(); 
            }
            if (AutoSaveWorld.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
            }
            if (!AutoSaveWorld.IsEnabled)
            {
                Timers.TimerCheck();
            }           
            if (Bloodmoon.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
            }
            if (!Bloodmoon.IsEnabled)
            {
                Timers.TimerCheck();
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
            if (EntityUnderground.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
            }
            if (!EntityUnderground.IsEnabled)
            {
                Timers.TimerCheck();
            }
            if (FlightCheck.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
                FlightCheck.DetectionLogsDir();
            }
            if (!FlightCheck.IsEnabled)
            {
                Timers.TimerCheck();
            }            
            if (HatchElevator.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
                HatchElevator.DetectionLogsDir();
            }
            if (!HatchElevator.IsEnabled)
            {
                Timers.TimerCheck();
            }
            if (InfoTicker.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
            }
            if (!InfoTicker.IsEnabled)
            {
                Timers.TimerCheck();
            }
            if (Jail.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
            }
            if (!Jail.IsEnabled)
            {
                Timers.TimerCheck();
            }
            if (PlayerLogs.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
                PlayerLogs.PlayerLogsDir();
            }
            if (!PlayerLogs.IsEnabled)
            {
                Timers.TimerCheck();
            }            
            if (PlayerStatCheck.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
                PlayerStatCheck.DetectionLogsDir();
            }
            if (!PlayerStatCheck.IsEnabled)
            {
                Timers.TimerCheck();
            }           
            if (UndergroundCheck.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
                UndergroundCheck.DetectionLogsDir();
            }
            if (!UndergroundCheck.IsEnabled)
            {
                Timers.TimerCheck();
            }
            if (ZoneProtection.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
                ZoneProtection.DetectionLogsDir();
            }
            if (!ZoneProtection.IsEnabled)
            {
                Timers.TimerCheck();
            }
            if (DeathSpot.IsEnabled)
            {
                if (!Timers.timer1Running)
                {
                    Timers.TimerStart1Second();
                }
            }
            if (!DeathSpot.IsEnabled)
            {
                Timers.TimerCheck();
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
            if (!ZoneProtection.IsRunning && ZoneProtection.IsEnabled)
            {
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
            if (HowToSetup.IsEnabled)
            {
                HowToSetup.Load();
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
            if (StartingItems.IsEnabled)
            {
                StartingItems.BuildList();
            }
        }
    }
}
using System.Timers;
using ServerTools.AntiCheat;
using ServerTools.Website;

namespace ServerTools
{
    class Timers
    {
        public static bool timer1Running = false, timer2Running = false;
        public static int _stopServerCount, _eventTime = 0, _autoShutdown, _autoShutdownBloodmoonOver;
        private static int timer1SecondInstanceCount, timer2Second, timer5Second, timer10Second, timer60Second, _watchList, _nightAlert, 
            _horde, _lottery, _breakTime, _invalidItems, _weatherVote, _bloodmoon, _playerLogs, _autoSaveWorld, _infoTicker, _restartVote, _stopServerCountDown, 
            _kickVote, _muteVote, _eventInvitation, _eventOpen, _zoneReminder, _tracking, _realWorldTime, _autoShutdownBloodmoon, _autoBackup;
        private static System.Timers.Timer t1 = new System.Timers.Timer();

        public static void TimerStart()
        {
            timer1SecondInstanceCount++;
            if (timer1SecondInstanceCount <= 1)
            {
                timer1Running = true;
                t1.Interval = 1000;
                t1.Start();
                t1.Elapsed += new ElapsedEventHandler(Init);
            }
        }

        public static void TimerStop()
        {
            if (timer1Running)
            {
                timer1Running = false;
                t1.Stop();
                t1.Close();
                timer1SecondInstanceCount = 0;
            }
        }

        public static void SingleUseTimer(int _delay, string _playerId, string _commands)
        {
            if (_delay > 120)
            {
                _delay = 120;
            }
            int _delayAdjusted = _delay * 1000;
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(_delayAdjusted);
            singleUseTimer.AutoReset = false;
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                Init3(sender, e, _playerId, _commands);
                singleUseTimer.Close();
            };
        }

        public static void NewPlayerExecTimer(ClientInfo _cInfo)
        {
            System.Timers.Timer newPlayerExecTimer = new System.Timers.Timer(3000);
            newPlayerExecTimer.AutoReset = false;
            newPlayerExecTimer.Start();
            newPlayerExecTimer.Elapsed += (sender, e) =>
            {
                Init4(sender, e, _cInfo);
                newPlayerExecTimer.Close();
            };
        }

        public static void NewPlayerStartingItemsTimer(ClientInfo _cInfo)
        {
            System.Timers.Timer newPlayerStartingItemsTimer = new System.Timers.Timer(2000);
            newPlayerStartingItemsTimer.AutoReset = false;
            newPlayerStartingItemsTimer.Start();
            newPlayerStartingItemsTimer.Elapsed += (sender, e) =>
            {
                Init5(sender, e, _cInfo);
                newPlayerStartingItemsTimer.Close();
            };
        }

        public static void DisconnectHardcorePlayer(ClientInfo _cInfo)
        {
            System.Timers.Timer hardcoreTimer = new System.Timers.Timer(20000);
            hardcoreTimer.AutoReset = false;
            hardcoreTimer.Start();
            hardcoreTimer.Elapsed += (sender, e) =>
            {
                Init6(sender, e, _cInfo);
                hardcoreTimer.Close();
            };
        }

        public static void BattleLogTool(string _id)
        {
            System.Timers.Timer exitTimer = new System.Timers.Timer(2000);
            exitTimer.AutoReset = false;
            exitTimer.Start();
            exitTimer.Elapsed += (sender, e) =>
            {
                Init7(sender, e, _id);
                exitTimer.Close();
            };
        }

        public static void BattleLogPlayerExit(string _id)
        {
            System.Timers.Timer playerExitTimer = new System.Timers.Timer(15000);
            playerExitTimer.AutoReset = false;
            playerExitTimer.Start();
            playerExitTimer.Elapsed += (sender, e) =>
            {
                Init8(sender, e, _id);
                playerExitTimer.Close();
            };
        }

        public static void LogAlert()
        {
            Log.Out("-------------------------------");
            Log.Out("[SERVERTOOLS] Anti-Cheat tools:");
            Log.Out("-------------------------------");
            if (CredentialCheck.IsEnabled)
            {
                Log.Out("Credential enabled");
            }
            if (ProcessDamage.Damage_Detector)
            {
                Log.Out("Damage detector enabled");
            }
            if (DupeLog.IsEnabled)
            {
                Log.Out("Dupe log enabled");
            }
            if (Flying.IsEnabled)
            {
                Log.Out("Flying detector enabled");
            }
            if (GodMode.IsEnabled)
            {
                Log.Out("God mode detector enabled");
            }
            if (InvalidItems.IsEnabled)
            {
                Log.Out("Invalid item detector enabled");
            }
            if (Jail.IsEnabled)
            {
                Log.Out("Jail enabled");
            }
            if (PlayerStatCheck.IsEnabled)
            {
                Log.Out("Player stat enabled");
            }
            if (PlayerLogs.IsEnabled)
            {
                Log.Out("Player logs enabled");
            }
            if (ProtectedSpaces.IsEnabled)
            {
                Log.Out("Protected spaces enabled");
            }
            if (TeleportCheck.IsEnabled)
            {
                Log.Out("Teleport enabled");
            }
            if (Watchlist.IsEnabled)
            {
                Log.Out("Watch list enabled");
            }
            if (WorldRadius.IsEnabled)
            {
                Log.Out("World radius enabled");
            }
            Log.Out("--------------------------------------");
            Log.Out("[SERVERTOOLS] Chat color-prefix tools:");
            Log.Out("--------------------------------------");
            if (ChatColorPrefix.IsEnabled)
            {
                Log.Out("Chat color and prefix enabled");
            }
            if (ChatHook.Normal_Player_Chat_Prefix)
            {
                Log.Out("Normal Player chat color and prefix enabled");
            }
            if (ChatHook.Message_Color_Enabled)
            {
                Log.Out("Message color is enabled");
            }
            if (VehicleTeleport.IsEnabled)
            {
                Log.Out("-------------------------------------");
                Log.Out("[SERVERTOOLS] Vehicle Teleport tools:");
                Log.Out("-------------------------------------");
                if (VehicleTeleport.Bike)
                {
                    Log.Out("Bike enabled");
                }
                if (VehicleTeleport.Mini_Bike)
                {
                    Log.Out("Mini bike enabled");
                }
                if (VehicleTeleport.Motor_Bike)
                {
                    Log.Out("Motor bike enabled");
                }
                if (VehicleTeleport.Jeep)
                {
                    Log.Out("Jeep enabled");
                }
                if (VehicleTeleport.Gyro)
                {
                    Log.Out("Gyro enabled");
                }
            }
            Log.Out("--------------------------");
            Log.Out("[SERVERTOOLS] Other tools:");
            Log.Out("--------------------------");
            if (AdminChat.IsEnabled)
            {
                Log.Out("Admin chat commands enabled");
            }
            if (AdminList.IsEnabled)
            {
                Log.Out("Admin list enabled");
            }
            if (Animals.IsEnabled)
            {
                Log.Out("Animal tracking enabled");
            }
            if (AuctionBox.IsEnabled)
            {
                Log.Out("Auction enabled");
            }
            if (AutoBackup.IsEnabled)
            {
                Log.Out("Auto backup enabled");
            }
            if (AutoSaveWorld.IsEnabled)
            {
                Log.Out("Auto save world enabled");
            }
            if (AutoShutdown.IsEnabled)
            {
                Log.Out("Auto shutdown enabled");
            }
            if (Badwords.IsEnabled)
            {
                Log.Out("Badword filter enabled");
            }
            if (Bank.IsEnabled)
            {
                Log.Out("Bank enabled");
            }
            if (EntityCleanup.BlockIsEnabled)
            {
                Log.Out("Block cleanup enabled");
            }
            if (Bloodmoon.IsEnabled)
            {
                Log.Out("Bloodmoon enabled");
            }
            if (BloodmoonWarrior.IsEnabled)
            {
                Log.Out("Bloodmoon warrior enabled");
            }
            if (Bounties.IsEnabled)
            {
                Log.Out("Bounties enabled");
            }
            if (BreakTime.IsEnabled)
            {
                Log.Out("Break time enabled");
            }
            if (ChatHook.ChatFlood)
            {
                Log.Out("Chat flood protection enabled");
            }
            if (ChatLog.IsEnabled)
            {
                Log.Out("Chat log enabled");
            }
            if (ClanManager.IsEnabled)
            {
                Log.Out("Clan manager enabled");
            }
            if (CommandLog.IsEnabled)
            {
                Log.Out("Command log enabled");
            }
            if (CustomCommands.IsEnabled)
            {
                Log.Out("Custom commands enabled");
            }
            if (Day7.IsEnabled)
            {
                Log.Out("Day 7 enabled");
            }
            if (DeathSpot.IsEnabled)
            {
                Log.Out("Death spot enabled");
            }
            if (EntityCleanup.IsEnabled)
            {
                Log.Out("Entity cleanup enabled");
            }
            if (EntityCleanup.Underground)
            {
                Log.Out("Entity cleanup underground enabled");
            }
            if (EntityCleanup.FallingTreeEnabled)
            {
                Log.Out("Falling tree cleanup enabled");
            }
            if (BattleLogger.IsEnabled)
            {
                Log.Out("Exit enabled");
            }
            if (FirstClaimBlock.IsEnabled)
            {
                Log.Out("First claim block enabled");
            }
            if (Fps.IsEnabled)
            {
                Log.Out("FPS enabled");
            }
            if (FriendTeleport.IsEnabled)
            {
                Log.Out("Friend teleport enabled");
            }
            if (Gimme.IsEnabled)
            {
                Log.Out("Gimme enabled");
            }
            if (HighPingKicker.IsEnabled)
            {
                Log.Out("High ping kicker enabled");
            }
            if (Hordes.IsEnabled)
            {
                Log.Out("Hordes enabled");
            }
            if (InfoTicker.IsEnabled)
            {
                Log.Out("Infoticker enabled");
            }
            if (KickVote.IsEnabled)
            {
                Log.Out("Kick vote enabled");
            }
            if (KillNotice.IsEnabled)
            {
                Log.Out("Kill notice enabled");
            }
            if (Lobby.IsEnabled)
            {
                Log.Out("Lobby enabled");
            }
            if (Loc.IsEnabled)
            {
                Log.Out("Location enabled");
            }
            if (LoginNotice.IsEnabled)
            {
                Log.Out("Login notice enabled");
            }
            if (Lottery.IsEnabled)
            {
                Log.Out("Lottery enabled");
            }
            if (Market.IsEnabled)
            {
                Log.Out("Market enabled");
            }
            if (Motd.IsEnabled)
            {
                Log.Out("Motd enabled");
            }
            if (Mute.IsEnabled)
            {
                Log.Out("Mute enabled");
            }
            if (MuteVote.IsEnabled)
            {
                Log.Out("Mute vote enabled");
            }
            if (NewSpawnTele.IsEnabled)
            {
                Log.Out("New spawn teleport enabled");
            }
            if (NightAlert.IsEnabled)
            {
                Log.Out("Night alert enabled");
            }
            if (Prayer.IsEnabled)
            {
                Log.Out("Prayer enabled");
            }
            if (Whisper.IsEnabled)
            {
                Log.Out("Private message enabled");
            }
            if (RealWorldTime.IsEnabled)
            {
                Log.Out("Real world time enabled");
            }
            if (ReservedSlots.IsEnabled)
            {
                Log.Out("Reserved slots enabled");
            }
            if (ScoutPlayer.IsEnabled)
            {
                Log.Out("Scout player enabled");
            }
            if (TeleportHome.IsEnabled)
            {
                Log.Out("Set home enabled");
            }
            if (Shop.IsEnabled)
            {
                Log.Out("Shop enabled");
            }
            if (StartingItems.IsEnabled)
            {
                Log.Out("Starting items enabled");
            }
            if (Suicide.IsEnabled)
            {
                Log.Out("Suicide enabled");
            }
            if (Tracking.IsEnabled)
            {
                Log.Out("Tracking enabled");
            }
            if (Travel.IsEnabled)
            {
                Log.Out("Travel enabled");
            }
            if (UnderWater.IsEnabled)
            {
                Log.Out("Under water enabled");
            }
            if (VoteReward.IsEnabled)
            {
                Log.Out("Vote reward enabled");
            }
            if (Wallet.IsEnabled)
            {
                Log.Out("Wallet enabled");
            }
            if (Waypoints.IsEnabled)
            {
                Log.Out("Waypoints enabled");
            }
            if (WebsiteServer.IsEnabled)
            {
                Log.Out("Website server enabled");
            }
            if (Zones.IsEnabled)
            {
                Log.Out("Zone enabled");
            }
        }

        public static void LoadAlert()
        {
            Log.Out("--------------------------------");
            Log.Out("[SERVERTOOLS] Tool load complete");
            Log.Out("--------------------------------");
        }

        private static void Init(object sender, ElapsedEventArgs e)
        {
            if (!StopServer.Shutdown)
            {
                PersistentOperations.PlayerCheck();
                if (Jail.IsEnabled)
                {
                    Jail.StatusCheck();
                }
                if (UnderWater.IsEnabled)
                {
                    UnderWater.Exec();
                }
                timer2Second++;
                if (timer2Second >= 2)
                {
                    if (WorldRadius.IsEnabled)
                    {
                        WorldRadius.Exec();
                    }
                    if (Flying.IsEnabled)
                    {
                        Flying.Exec();
                    }
                    timer2Second = 0;
                }
                timer5Second++;
                if (timer5Second >= 5)
                {
                    if (Zones.IsEnabled)
                    {
                        Zones.HostileCheck();
                    }
                    if (PlayerStatCheck.IsEnabled)
                    {
                        PlayerStatCheck.PlayerStat();
                    }
                    timer5Second = 0;
                }
                timer10Second++;
                if (timer10Second >= 10)
                {
                    if (EntityCleanup.IsEnabled)
                    {
                        EntityCleanup.EntityCheck();
                    }
                    timer10Second = 0;
                }
                timer60Second++;
                if (timer60Second >= 60)
                {
                    if (Jail.IsEnabled && Jail.Jailed.Count > 0)
                    {
                        Jail.Clear();
                    }
                    if (Mute.IsEnabled && Mute.Mutes.Count > 0)
                    {
                        Mute.Clear();
                    }
                    if (BloodmoonWarrior.IsEnabled)
                    {
                        BloodmoonWarrior.Exec();
                    }
                    timer60Second = 0;
                }
                if (WeatherVote.IsEnabled && WeatherVote.VoteOpen)
                {
                    _weatherVote++;
                    if (_weatherVote >= 60)
                    {
                        _weatherVote = 0;
                        WeatherVote.VoteOpen = false;
                        WeatherVote.ProcessWeatherVote();
                    }
                }
                if (RestartVote.IsEnabled && RestartVote.VoteOpen)
                {
                    _restartVote++;
                    if (_restartVote >= 60)
                    {
                        _restartVote = 0;
                        RestartVote.VoteOpen = false;
                        RestartVote.ProcessRestartVote();
                    }
                }
                if (MuteVote.IsEnabled && MuteVote.VoteOpen)
                {
                    _muteVote++;
                    if (_muteVote >= 60)
                    {
                        _muteVote = 0;
                        MuteVote.VoteOpen = false;
                        MuteVote.ProcessMuteVote();
                    }
                }
                if (KickVote.IsEnabled && KickVote.VoteOpen)
                {
                    _kickVote++;
                    if (_kickVote >= 60)
                    {
                        _kickVote = 0;
                        KickVote.VoteOpen = false;
                        KickVote.ProcessKickVote();
                    }
                }
                if (Lottery.IsEnabled && Lottery.OpenLotto)
                {
                    _lottery++;
                    if (_lottery == 3300)
                    {
                        Lottery.Alert();
                    }
                    if (_lottery >= 3600)
                    {
                        _lottery = 0;
                        Lottery.StartLotto();
                    }
                }
                else
                {
                    _lottery = 0;
                }
                if (Hordes.IsEnabled)
                {
                    _horde++;
                    if (_horde >= 1200)
                    {
                        _horde = 0;
                        Hordes.Exec();
                    }
                }
                else
                {
                    _horde = 0;
                }
                if (NightAlert.IsEnabled)
                {
                    _nightAlert++;
                    if (_nightAlert >= NightAlert.Delay * 60)
                    {
                        _nightAlert = 0;
                        NightAlert.Exec();
                    }
                }
                else
                {
                    _nightAlert = 0;
                }
                if (Watchlist.IsEnabled)
                {
                    _watchList++;
                    if (_watchList >= Watchlist.Delay * 60)
                    {
                        _watchList = 0;
                        Watchlist.CheckWatchlist();
                    }
                }
                else
                {
                    _watchList = 0;
                }
                if (Bloodmoon.IsEnabled & Bloodmoon.Auto_Show)
                {
                    if (Bloodmoon.Delay > 0)
                    {
                        _bloodmoon++;
                        if (_bloodmoon >= Bloodmoon.Delay * 60)
                        {
                            _bloodmoon = 0;
                            Bloodmoon.StatusCheck();
                        }
                    }
                }
                else
                {
                    _bloodmoon = 0;
                }
                if (PlayerLogs.IsEnabled & PlayerLogs.Delay > 0)
                {
                    _playerLogs++;
                    if (_playerLogs >= PlayerLogs.Delay)
                    {
                        _playerLogs = 0;
                        PlayerLogs.Exec();
                    }
                }
                else
                {
                    _playerLogs = 0;
                }
                if (StopServer.StopServerCountingDown)
                {
                    _stopServerCountDown++;
                    if (_stopServerCountDown == 60)
                    {
                        _stopServerCountDown = 0;
                        _stopServerCount--;
                    }
                    if (_stopServerCount == 0)
                    {
                        _stopServerCountDown = 0;
                        StopServer.StopServerCountingDown = false;
                        StopServer.Stop();
                    }
                    if (_stopServerCount == 1 && _stopServerCountDown == 0)
                    {
                        StopServer.StartShutdown3();
                    }
                    if (_stopServerCount > 1 && _stopServerCountDown == 0)
                    {
                        StopServer.StartShutdown2(_stopServerCount);
                    }
                    if (StopServer.Kick_30_Seconds)
                    {
                        if (_stopServerCount == 1 && _stopServerCountDown == 30)
                        {
                            StopServer.NoEntry = true;
                            StopServer.Kick30();
                        }
                    }
                    if (StopServer.Ten_Second_Countdown)
                    {
                        if (_stopServerCount == 1 && _stopServerCountDown == 50)
                        {
                            StopServer.StartShutdown4();
                        }
                        if (_stopServerCount == 1 && _stopServerCountDown == 55)
                        {
                            StopServer.StartShutdown5();
                        }
                        if (_stopServerCount == 1 && _stopServerCountDown == 56)
                        {
                            StopServer.StartShutdown6();
                        }
                        if (_stopServerCount == 1 && _stopServerCountDown == 57)
                        {
                            StopServer.StartShutdown7();
                        }
                        if (_stopServerCount == 1 && _stopServerCountDown == 58)
                        {
                            StopServer.StartShutdown8();
                        }
                        if (_stopServerCount == 1 && _stopServerCountDown == 59)
                        {
                            StopServer.StartShutdown9();
                        }
                    }
                }
                else
                {
                    _stopServerCountDown = 0;
                    _stopServerCount = 0;
                }
                if (AutoSaveWorld.IsEnabled & AutoSaveWorld.Delay > 0)
                {
                    _autoSaveWorld++;
                    if (_autoSaveWorld >= AutoSaveWorld.Delay * 60)
                    {
                        _autoSaveWorld = 0;
                        AutoSaveWorld.Save();
                    }
                }
                else
                {
                    _autoSaveWorld = 0;
                }
                if (AutoShutdown.IsEnabled && !AutoShutdown.Bloodmoon && !AutoShutdown.BloodmoonOver && !StopServer.StopServerCountingDown)
                {
                    _autoShutdown++;
                    if (!Event.Open && _autoShutdown >= AutoShutdown.Delay * 60)
                    {
                        _autoShutdown = 0;
                        AutoShutdown.BloodmoonCheck();
                    }
                }
                else
                {
                    _autoShutdown = 0;
                }
                if (AutoShutdown.Bloodmoon)
                {
                    _autoShutdownBloodmoon++;
                    if (_autoShutdownBloodmoon >= 150)
                    {
                        _autoShutdownBloodmoon = 0;
                        AutoShutdown.BloodmoonCheck();
                    }
                }
                if (AutoShutdown.BloodmoonOver && !Event.Open)
                {
                    _autoShutdownBloodmoonOver++;
                    if (_autoShutdownBloodmoonOver == 1)
                    {
                        AutoShutdown.BloodmoonOverAlert();
                    }
                    else if (_autoShutdownBloodmoonOver >= 900)
                    {
                        _autoShutdownBloodmoonOver = 0;
                        AutoShutdown.BloodmoonOver = false;
                        AutoShutdown.Shutdown();
                    }
                }
                if (InfoTicker.IsEnabled)
                {
                    _infoTicker++;
                    if (_infoTicker >= InfoTicker.Delay * 60)
                    {
                        _infoTicker = 0;
                        InfoTicker.StatusCheck();
                    }
                }
                else
                {
                    _infoTicker = 0;
                }
                if (Event.Invited)
                {
                    _eventInvitation++;
                    if (_eventInvitation >= 900)
                    {
                        _eventInvitation = 0;
                        Event.Invited = false;
                        Event.CheckOpen();
                    }
                }
                else
                {
                    _eventInvitation = 0;
                }
                if (Event.Open)
                {
                    _eventOpen++;
                    if (_eventOpen == _eventTime / 2)
                    {
                        Event.HalfTime();
                    }
                    if (_eventOpen == _eventTime - 300)
                    {
                        Event.FiveMin();
                    }
                    if (_eventOpen >= _eventTime)
                    {
                        _eventOpen = 0;
                        Event.EndEvent();
                    }
                }
                else
                {
                    _eventOpen = 0;
                }
                if (RestartVote.Startup)
                {
                    _restartVote++;
                    if (_restartVote >= 1800)
                    {
                        RestartVote.Startup = false;
                    }
                }
                else
                {
                    _restartVote = 0;
                }
                if (Zones.IsEnabled & Zones.Reminder.Count > 0)
                {
                    _zoneReminder++;
                    if (_zoneReminder >= Zones.Reminder_Delay * 60)
                    {
                        _zoneReminder = 0;
                        Zones.ReminderExec();
                    }
                }
                else
                {
                    _zoneReminder = 0;
                }
                if (AutoBackup.IsEnabled)
                {
                    _autoBackup++;
                    if (_autoBackup >= AutoBackup.Delay * 60)
                    {
                        _autoBackup = 0;
                        AutoBackup.Exec();
                    }
                }
                else
                {
                    _autoBackup = 0;
                }
                if (BreakTime.IsEnabled)
                {
                    _breakTime++;
                    if (_breakTime >= BreakTime.Break_Time * 60)
                    {
                        _breakTime = 0;
                        BreakTime.Exec();
                    }
                }
                else
                {
                    _breakTime = 0;
                }
                if (Tracking.IsEnabled)
                {
                    _tracking++;
                    if (_tracking >= Tracking.Rate)
                    {
                        _tracking = 0;
                        Tracking.Exec();
                    }
                }
                else
                {
                    _tracking = 0;
                }
                if (InvalidItems.IsEnabled && InvalidItems.Check_Storage)
                {
                    _invalidItems++;
                    if (_invalidItems >= 300)
                    {
                        _invalidItems = 0;
                        InvalidItems.CheckStorage();
                    }
                }
                else
                {
                    _invalidItems = 0;
                }
                if (RealWorldTime.IsEnabled)
                {
                    _realWorldTime++;
                    if (_realWorldTime >= RealWorldTime.Delay * 60)
                    {
                        _realWorldTime = 0;
                        RealWorldTime.Time();
                    }
                }
                else
                {
                    _realWorldTime = 0;
                }
            }
        }

        //init 2 available

        private static void Init3(object sender, ElapsedEventArgs e, string _playerId, string _commands)
        {
            CustomCommands.DelayedCommand(_playerId, _commands);
        }

        private static void Init4(object sender, ElapsedEventArgs e, ClientInfo _cInfo)
        {
            API.NewPlayerExec1(_cInfo);
        }

        private static void Init5(object sender, ElapsedEventArgs e, ClientInfo _cInfo)
        {
            API.NewPlayerExec2(_cInfo);
        }

        private static void Init6(object sender, ElapsedEventArgs e, ClientInfo _cInfo)
        {
            Hardcore.DisconnectHardcorePlayer(_cInfo);
        }

        private static void Init7(object sender, ElapsedEventArgs e, string _id)
        {
            BattleLogger.ScanLog(_id);
        }

        private static void Init8(object sender, ElapsedEventArgs e, string _id)
        {
            BattleLogger.PlayerExit(_id);
        }
    }
}

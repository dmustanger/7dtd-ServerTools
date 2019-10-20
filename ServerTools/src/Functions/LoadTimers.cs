using System.Timers;

namespace ServerTools
{
    class Timers
    {
        public static bool timer1Running = false, timer2Running = false;
        public static int Player_Log_Interval = 60, Auto_Show_Bloodmoon_Delay = 30, _tBS,
            Delay_Between_World_Saves = 15, Stop_Server_Time = 1, _newCount = 0,
            Shutdown_Delay = 60, Infoticker_Delay = 60, _sSC = 0, _sSCD = 0,
            Alert_Delay = 5, Real_Time_Delay = 60, Night_Time_Delay = 120, _sD = 0, _eventTime = 0;
        private static int timer1SecondInstanceCount, timerHalfSecondInstanceCount, _wV, _pSC, _b, _pL, _mC, _wSD, _iT, _rVS, _kV, _mV, _bT, _cC,
            _rS, _rV, _eC, _wL, _rWT, _rE, _aSB, _wR, _nA, _jR, _h, _l, _eI, _eO, _zR, _nP, _sC, _tP, _fC;
        private static System.Timers.Timer t1 = new System.Timers.Timer();
        private static System.Timers.Timer t2 = new System.Timers.Timer();

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
            }
            timer1SecondInstanceCount = 0;
        }

        public static void Timer2Start()
        {
            timerHalfSecondInstanceCount++;
            if (timerHalfSecondInstanceCount <= 1)
            {
                timer2Running = true;
                t2.Interval = 250;
                t2.Start();
                t2.Elapsed += new ElapsedEventHandler(Init2);
            }
        }

        public static void Timer2Stop()
        {
            if (timer2Running)
            {
                timer2Running = false;
                t2.Stop();
            }
            timerHalfSecondInstanceCount = 0;
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
            if (DupeLog.IsEnabled)
            {
                Log.Out("Dupe Log enabled");
            }
            if (Flying.IsEnabled)
            {
                Log.Out("Flying detector enabled");
            }
            if (GodMode.IsEnabled)
            {
                Log.Out("God mode detector enabled");
            }
            if (HatchElevator.IsEnabled)
            {
                Log.Out("Hatch elevator detector enabled");
            }
            if (InventoryCheck.IsEnabled)
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
            if (TeleportCheck.IsEnabled)
            {
                Log.Out("Teleport enabled");
            }
            if (Watchlist.IsEnabled)
            {
                Log.Out("Watchlist enabled");
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
            if (ChatHook.Normal_Player_Name_Coloring)
            {
                Log.Out("Normal Player chat color and prefix enabled");
            }
            if (VehicleTeleport.IsEnabled)
            {
                Log.Out("------------------------------------");
                Log.Out("[SERVERTOOLS] Vehicle Teleport tools:");
                Log.Out("------------------------------------");
                if (VehicleTeleport.Bike)
                {
                    Log.Out("Bike enabled");
                }
                if (VehicleTeleport.Mini_Bike)
                {
                    Log.Out("MiniBike enabled");
                }
                if (VehicleTeleport.Motor_Bike)
                {
                    Log.Out("MotorBike enabled");
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
            if (EntityCleanup.Underground)
            {
                Log.Out("Entity underground cleanup enabled");
            }
            if (EntityCleanup.FallingTreeEnabled)
            {
                Log.Out("Falling tree cleanup enabled");
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
            if (LobbyChat.IsEnabled)
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
            if (MarketChat.IsEnabled)
            {
                Log.Out("Market enabled");
            }
            if (Motd.IsEnabled)
            {
                Log.Out("Motd enabled");
            }
            if (MutePlayer.IsEnabled)
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
            if (RealWorldTime.IsEnabled)
            {
                Log.Out("Real world time enabled");
            }
            if (ReservedSlots.IsEnabled)
            {
                Log.Out("Reserved slots enabled");
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
            if (Waypoint.IsEnabled)
            {
                Log.Out("Waypoints enabled");
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

        public static void Init(object sender, ElapsedEventArgs e)
        {
            if (!StopServer.Shutdown)
            {
                if (HatchElevator.IsEnabled)
                {
                    HatchElevator.AutoHatchCheck();
                }
                if (Jail.IsEnabled)
                {
                    Jail.StatusCheck();
                }
                if (UnderWater.IsEnabled)
                {
                    UnderWater.Exec();
                }
                if (Jail.Jailed.Count > 0)
                {
                    _jR++;
                    if (_jR >= 60)
                    {
                        _jR = 0;
                        Jail.Clear();
                    }
                }
                else
                {
                    _jR = 0;
                }
                if (WeatherVote.IsEnabled)
                {
                    if (WeatherVote.VoteOpen)
                    {
                        _wV++;
                        if (_wV >= 60)
                        {
                            _wV = 0;
                            WeatherVote.CallForVote2();
                        }
                    }
                }
                else
                {
                    _wV = 0;
                }
                if (MutePlayer.IsEnabled && MutePlayer.Mutes.Count > 0)
                {
                    _mC++;
                    if (_mC >= 60)
                    {
                        _mC = 0;
                        MutePlayer.Clear();
                    }
                }
                else
                {
                    _mC = 0;
                }
                if (RealWorldTime.IsEnabled)
                {
                    _rWT++;
                    if (_rWT >= Real_Time_Delay * 60)
                    {
                        _rWT = 0;
                        RealWorldTime.Time();
                    }
                }
                else
                {
                    _rWT = 0;
                }
                if (Lottery.IsEnabled && Lottery.OpenLotto)
                {
                    _l++;
                    if (_l == 3300)
                    {
                        Lottery.Alert();
                    }
                    if (_l >= 3600)
                    {
                        _l = 0;
                        Lottery.StartLotto();
                    }
                }
                else
                {
                    _l = 0;
                }
                if (RestartVote.IsEnabled)
                {
                    if (RestartVote.VoteOpen)
                    {
                        _rV++;
                        if (_rV >= 60)
                        {
                            _rV = 0;
                            RestartVote.CallForVote2();
                        }
                    }
                }
                else
                {
                    _rV = 0;
                }
                if (MuteVote.IsEnabled)
                {
                    if (MuteVote.VoteOpen)
                    {
                        _mV++;
                        if (_mV >= 60)
                        {
                            _mV = 0;
                            MuteVote.VoteCount();
                        }
                    }
                }
                else
                {
                    _mV = 0;
                }
                if (KickVote.IsEnabled)
                {
                    if (KickVote.VoteOpen)
                    {
                        _kV++;
                        if (_kV >= 60)
                        {
                            _kV = 0;
                            KickVote.VoteCount();
                        }
                    }
                }
                else
                {
                    _kV = 0;
                }
                if (Hordes.IsEnabled)
                {
                    _h++;
                    if (_h >= 1200)
                    {
                        _h = 0;
                        Hordes.Exec();
                    }
                }
                else
                {
                    _h = 0;
                }
                if (EntityCleanup.IsEnabled && (EntityCleanup.BlockIsEnabled || EntityCleanup.FallingTreeEnabled || EntityCleanup.Underground || EntityCleanup.MiniBikes))
                {
                    _eC++;
                    if (_eC >= 10)
                    {
                        _eC = 0;
                        EntityCleanup.EntityCheck();
                    }
                }
                else
                {
                    _eC = 0;
                }
                if (Zones.IsEnabled)
                {
                    _rE++;
                    if (_rE >= 5)
                    {
                        _rE = 0;
                        EntityCleanup.ZombieCheck();
                    }
                }
                else
                {
                    _rE = 0;
                }
                if (NightAlert.IsEnabled)
                {
                    _nA++;
                    if (_nA >= Night_Time_Delay * 60)
                    {
                        _nA = 0;
                        NightAlert.Exec();
                    }
                }
                else
                {
                    _nA = 0;
                }
                if (Watchlist.IsEnabled)
                {
                    _wL++;
                    if (_wL >= Alert_Delay * 60)
                    {
                        _wL = 0;
                        Watchlist.CheckWatchlist();
                    }
                }
                else
                {
                    _wL = 0;
                }
                if (PlayerStatCheck.IsEnabled)
                {
                    _pSC++;
                    if (_pSC >= 5)
                    {
                        _pSC = 0;
                        PlayerStatCheck.PlayerStat();
                    }
                }
                else
                {
                    _pSC = 0;
                }
                if (ReservedSlots.IsEnabled)
                {
                    _rS++;
                    if (_rS >= 120)
                    {
                        _rS = 0;
                        int _playerCount = ConnectionManager.Instance.ClientCount();
                        if (_playerCount >= API.MaxPlayers - ReservedSlots.Admin_Slots)
                        {
                            ReservedSlots.OpenSlot();
                        }
                    }
                }
                else
                {
                    _rS = 0;
                }
                if (Bloodmoon.IsEnabled & Bloodmoon.Auto_Show)
                {
                    if (Auto_Show_Bloodmoon_Delay > 0)
                    {
                        _b++;
                        if (_b >= Auto_Show_Bloodmoon_Delay * 60)
                        {
                            _b = 0;
                            Bloodmoon.StatusCheck();
                        }
                    }
                }
                else
                {
                    _b = 0;
                }
                if (PlayerLogs.IsEnabled & Player_Log_Interval > 0)
                {
                    _pL++;
                    if (_pL >= Player_Log_Interval)
                    {
                        _pL = 0;
                        PlayerLogs.Exec();
                    }
                }
                else
                {
                    _pL = 0;
                }
                if (StopServer.stopServerCountingDown)
                {
                    _sSCD++;
                    if (_sSCD == 60)
                    {
                        _sSCD = 0;
                        _sSC--;
                    }
                    if (_sSC == 0)
                    {
                        _sSCD = 0;
                        StopServer.stopServerCountingDown = false;
                        StopServer.Stop();
                    }
                    if (_sSC == 1 && _sSCD == 0)
                    {
                        StopServer.StartShutdown3();
                    }
                    if (_sSC > 1 && _sSCD == 0)
                    {
                        StopServer.StartShutdown2(_sSC);
                    }
                    if (StopServer.Kick_30_Seconds)
                    {
                        if (_sSC == 1 && _sSCD == 30)
                        {
                            StopServer.Kick30();
                        }
                    }
                    if (StopServer.Ten_Second_Countdown)
                    {
                        if (_sSC == 1 && _sSCD == 50)
                        {
                            StopServer.StartShutdown4();
                        }
                        if (_sSC == 1 && _sSCD == 55)
                        {
                            StopServer.StartShutdown5();
                        }
                        if (_sSC == 1 && _sSCD == 56)
                        {
                            StopServer.StartShutdown6();
                        }
                        if (_sSC == 1 && _sSCD == 57)
                        {
                            StopServer.StartShutdown7();
                        }
                        if (_sSC == 1 && _sSCD == 58)
                        {
                            StopServer.StartShutdown8();
                        }
                        if (_sSC == 1 && _sSCD == 59)
                        {
                            StopServer.StartShutdown9();
                        }
                    }
                }
                else
                {
                    _sSCD = 0;
                    _sSC = 0;
                }
                if (AutoSaveWorld.IsEnabled & Delay_Between_World_Saves > 0)
                {
                    _wSD++;
                    if (_wSD >= Delay_Between_World_Saves * 60)
                    {
                        _wSD = 0;
                        AutoSaveWorld.Save();
                    }
                }
                else
                {
                    _wSD = 0;
                }
                if (AutoShutdown.IsEnabled && !AutoShutdown.Bloodmoon && !StopServer.stopServerCountingDown)
                {
                    _sD++;
                    if (!Event.Open && _sD >= Shutdown_Delay * 60)
                    {
                        _sD = 0;
                        AutoShutdown.CheckBloodmoon();
                    }
                }
                else
                {
                    _sD = 0;
                }
                if (AutoShutdown.Bloodmoon)
                {
                    _aSB++;
                    if (_aSB >= 900)
                    {
                        _aSB = 0;
                        AutoShutdown.CheckBloodmoon();
                    }
                }
                else
                {
                    _aSB = 0;
                }
                if (InfoTicker.IsEnabled)
                {
                    _iT++;
                    if (_iT >= Infoticker_Delay * 60)
                    {
                        _iT = 0;
                        InfoTicker.StatusCheck();
                    }
                }
                else
                {
                    _iT = 0;
                }
                if (WorldRadius.IsEnabled)
                {
                    _wR++;
                    if (_wR >= 2)
                    {
                        _wR = 0;
                        WorldRadius.Exec();
                    }
                }
                else
                {
                    _wR = 0;
                }
                if (Event.Invited)
                {
                    _eI++;
                    if (_eI >= 900)
                    {
                        _eI = 0;
                        Event.Invited = false;
                        Event.CheckOpen();
                    }
                }
                else
                {
                    _eI = 0;
                }
                if (Event.Open)
                {
                    _eO++;
                    if (_eO == _eventTime / 2)
                    {
                        Event.HalfTime();
                    }
                    if (_eO == _eventTime - 300)
                    {
                        Event.FiveMin();
                    }
                    if (_eO >= _eventTime)
                    {
                        _eO = 0;
                        Event.EndEvent();
                    }
                }
                else
                {
                    _eO = 0;
                }
                if (RestartVote.Startup)
                {
                    _rVS++;
                    if (_rVS >= 1800)
                    {
                        RestartVote.Startup = false;
                    }
                }
                else
                {
                    _rVS = 0;
                }
                if (Zones.IsEnabled & Zones.reminder.Count > 0)
                {
                    _zR++;
                    if (_zR >= Zones.Reminder_Delay * 60)
                    {
                        _zR = 0;
                        Zones.Reminder();
                    }
                }
                else
                {
                    _zR = 0;
                }
                if (AutoBackup.IsEnabled)
                {
                    _tBS++;
                    if (_tBS >= AutoBackup.Time_Between_Saves * 60)
                    {
                        _tBS = 0;
                        AutoBackup.Exec();
                    }
                }
                else
                {
                    _tBS = 0;
                }
                if (API.Que.Count > 0)
                {
                    _nP++;
                    if (_nP >= 5)
                    {
                        API.NewPlayerExec();
                        _nP = 0;
                    }
                }
                else
                {
                    _nP = 0;
                }
                if (BreakTime.IsEnabled)
                {
                    _bT++;
                    if (_bT >= BreakTime.Break_Time * 60)
                    {
                        _bT = 0;
                        BreakTime.Exec();
                    }
                }
                else
                {
                    _bT = 0;
                }
                if (Tracking.IsEnabled)
                {
                    _tP++;
                    if (_tP >= Tracking.Rate)
                    {
                        _tP = 0;
                        Tracking.Exec();
                    }
                }
                else
                {
                    _tP = 0;
                }
                if (InventoryCheck.IsEnabled && InventoryCheck.Chest_Checker)
                {
                    _cC++;
                    if (_cC >= 300)
                    {
                        _cC = 0;
                        InventoryCheck.ChestCheck();
                    }
                }
                else
                {
                    _cC = 0;
                }
                if (Flying.IsEnabled)
                {
                    _fC++;
                    if (_fC >= 2)
                    {
                        _fC = 0;
                        Flying.Exec();
                    }
                }
                else
                {
                    _fC = 0;
                }
            }
            if (StopServer.Shutdown)
            {
                _sC++;
                if (_sC >= 60)
                {
                    _sC = 0;
                    StopServer.FailSafe();
                }
            }
            else
            {
                _sC = 0;
            }
        }

        public static void Init2(object sender, ElapsedEventArgs e)
        {
            PlayerOperations.PlayerCheck();
        }
    }
}

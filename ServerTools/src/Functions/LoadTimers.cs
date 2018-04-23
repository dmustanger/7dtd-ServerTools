using System.Timers;

namespace ServerTools
{
    class Timers
    {
        public static bool timer1Running = false;
        public static int Player_Log_Interval = 60, Auto_Show_Bloodmoon_Delay = 30,
            Delay_Between_World_Saves = 15, Stop_Server_Time = 1, _newCount = 0, Weather_Vote_Delay = 30,
            Shutdown_Delay = 60, Infoticker_Delay = 60, Restart_Vote_Delay = 30, _sSC = 0, _sSCD = 0,
            Alert_Delay = 5, Real_Time_Delay = 60, Night_Time_Delay = 120, _sD = 0, _eventTime = 0;
        private static int timer1SecondInstanceCount, _wV, _wNV, _pSC, _b, _pL, _mC, _wSD, _iT,
            _rS, _rV, _rNV, _eC, _wL, _rWT, _rE, _aSB, _wR, _nA, _jR, _h, _l, _nV, _vR, _eS, _eI, _eO; 
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

        public static void LogAlert()
        {
            Log.Out("-------------------------------");
            Log.Out("[SERVERTOOLS] Anti-Cheat tools:");
            Log.Out("-------------------------------");
            if (CredentialCheck.IsEnabled)
            {
                Log.Out("Credential started");
            }
            if (DupeLog.IsEnabled)
            {
                Log.Out("Dupe Log enabled");
            }
            if (FlightCheck.IsEnabled)
            {
                Log.Out("Flight started");
            }
            if (HatchElevator.IsEnabled)
            {
                Log.Out("Hatch elevator started");
            }
            if (InventoryCheck.IsEnabled)
            {
                Log.Out("Invalid item kicker started");
            }
            if (UndergroundCheck.IsEnabled)
            {
                Log.Out("Underground flight started");
            }            
            if (Jail.IsEnabled)
            {
                Log.Out("Jail started");
            }
            if (PlayerStatCheck.IsEnabled)
            {
                Log.Out("Player stat started");
            }
            if (PlayerLogs.IsEnabled)
            {
                Log.Out("Player logs started");
            }
            if (TeleportCheck.IsEnabled)
            {
                Log.Out("Teleport started");
            }
            if (Watchlist.IsEnabled)
            {
                Log.Out("Watchlist started");
            }
            if (WorldRadius.IsEnabled)
            {
                Log.Out("World radius started");
            }
            Log.Out("--------------------------------------");
            Log.Out("[SERVERTOOLS] Chat prefix-color tools:");
            Log.Out("--------------------------------------");
            if (ChatHook.Admin_Name_Coloring)
            {
                Log.Out("Admin and moderator chat prefix-color enabled");
            }
            if (ChatHook.Donator_Name_Coloring)
            {
                Log.Out("Donator chat prefix-color enabled");
            }
            if (ChatHook.Normal_Player_Name_Coloring)
            {
                Log.Out("Normal Player chat prefix-color enabled");
            }
            if (ChatHook.Special_Player_Name_Coloring)
            {
                Log.Out("Special player chat prefix-color enabled");
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
            if (BikeReturn.IsEnabled)
            {
                Log.Out("Bike enabled");
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
            if (Players.Kill_Notice)
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
            if (NightVote.IsEnabled)
            {
                Log.Out("Night vote enabled");
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
            if (Travel.IsEnabled)
            {
                Log.Out("Travel enabled");
            }
            if (VoteReward.IsEnabled)
            {
                Log.Out("Vote reward enabled");
            }
            if (Zones.IsEnabled)
            {
                Log.Out("Zone protection enabled");
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
            if (FlightCheck.IsEnabled)
            {
                if ((int)GameManager.Instance.fps.Counter > 5)
                {
                    FlightCheck.AutoFlightCheck();
                }
            }
            if (HatchElevator.IsEnabled)
            {
                HatchElevator.AutoHatchCheck();
            }
            if (UndergroundCheck.IsEnabled)
            {
                if ((int)GameManager.Instance.fps.Counter > 5)
                {
                    UndergroundCheck.AutoUndergroundCheck();
                }
            }
            if (Jail.IsEnabled)
            {
                Jail.StatusCheck();
            }
            if (Bounties.IsEnabled || Players.Kill_Notice || DeathSpot.IsEnabled || Zones.IsEnabled)
            {
                Players.Exec();
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
                    if (_wV >= 30)
                    {
                        _wV = 0;
                        WeatherVote.VoteOpen = false;
                        WeatherVote.CallForVote2();
                    }
                }
                if (!WeatherVote.VoteNew)
                {
                    _wNV++;
                    if (_wNV >= Weather_Vote_Delay * 60)
                    {
                        _wNV = 0;
                        WeatherVote.VoteNew = true;
                        SdtdConsole.Instance.ExecuteSync("weather defaults", (ClientInfo)null);
                    }
                }
            }
            else
            {
                _wV = 0;
                _wNV = 0;
            }
            if (MutePlayer.Mutes.Count > 0)
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
                    if (_rV >= 30)
                    {
                        _rV = 0;
                        RestartVote.VoteNew = false;
                        RestartVote.CallForVote2();
                    }
                }
                if (!RestartVote.VoteNew)
                {
                    _rNV++;
                    if (_rNV >= Restart_Vote_Delay * 60)
                    {
                        _rNV = 0;
                        RestartVote.VoteNew = true;
                    }
                }
            }
            else
            {
                _rV = 0;
                _rNV = 0;
            }
            if (NightVote.IsEnabled)
            {
                if (NightVote.VoteOpen)
                {
                    _nV++;
                    if (_nV >= 30)
                    {
                        _nV = 0;
                        NightVote.VoteCount();
                    }
                }
            }
            else
            {
                _nV = 0;
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
            if (EntityCleanup.BlockIsEnabled || EntityCleanup.FallingTreeEnabled || EntityCleanup.Underground)
            {
                _eC++;
                if (_eC >= 15)
                {
                    _eC = 0;
                    EntityCleanup.EntityCheck();
                }
            }
            else
            {
                _eC = 0;
            }
            if (Zones.IsEnabled && Zones.No_Zombie)
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
                if (_pSC >= 2)
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
                    if (_playerCount == API.MaxPlayers)
                    {
                        ReservedSlots.OpenSlot();
                    }
                }
            }
            else
            {
                _rS = 0;
            }
            if (Bloodmoon.IsEnabled & Bloodmoon.Auto_Enabled)
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
                if (_aSB >= 1800)
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
            if (VoteReward.IsEnabled && VoteReward.QueOpen)
            {
                _vR++;
                if (_vR >= 60)
                {
                    _vR = 0;
                    VoteReward.que.Clear();
                    VoteReward.QueOpen = false;
                    VoteReward.RewardOpen = true;
                }
            }
            else
            {
                _vR = 0;
            }
            if (Event.Setup)
            {
                _eS++;
                if (_eS >= 900)
                {
                    _eS = 0;
                    Event.Setup = false;
                    Event.CheckOpen();
                }
            }
            else
            {
                _eS = 0;
            }
            if (Event.Invited)
            {
                _eI++;
                if (_eI >= 900)
                {
                    _eI = 0;
                    Event.Invited = false;
                    Event.CheckOpen2();
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
        }
    }
}

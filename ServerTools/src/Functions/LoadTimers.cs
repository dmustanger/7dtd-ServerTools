using System.Collections.Generic;
using System.Timers;

namespace ServerTools
{
    class Timers
    {
        public static bool timer1Running = false;
        public static int Player_Log_Interval = 60, Auto_Show_Bloodmoon_Delay = 30,
            Delay_Between_World_Saves = 15, Stop_Server_Time = 1, _newCount = 0, Weather_Vote_Delay = 30,
            Shutdown_Delay = 60, Infoticker_Delay = 60, Restart_Vote_Delay = 30, _sSC = 0, _sSCD = 0,
            Alert_Delay = 5;
        private static int timer1SecondInstanceCount = 0, _wV = 0, _wNV = 0, 
            _pSC = 0, _b = 0, _pL = 0, _wSD = 0, _sD = 0, _iD = 0, _eU = 0,
            _rS = 0, _rV = 0, _rNV = 0, _eC = 0, _wL = 0, _dS = 0; 
        private static System.Timers.Timer t1 = new System.Timers.Timer();

        public static void TimerStart1Second()
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

        public static void TimerStop1Second()
        {
            timer1Running = false;
            t1.Stop();
        }

        public static void LogAlert()
        {
            Log.Out("---------------------------------");
            Log.Out("[SERVERTOOLS] Beginning tool load");
            Log.Out("---------------------------------");
            Log.Out("-------------------------------");
            Log.Out("[SERVERTOOLS] Anti-Cheat tools:");
            Log.Out("-------------------------------");
            if (CredentialCheck.IsEnabled)
            {
                Log.Out("Credential started");
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
            if (Bloodmoon.IsEnabled)
            {
                Log.Out("Bloodmoon enabled");
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
            if (EntityCleanup.IsEnabled)
            {
                Log.Out("Entity cleanup enabled");
            }
            if (EntityUnderground.IsEnabled)
            {
                Log.Out("Entity underground enabled");
            }
            if (FirstClaimBlock.IsEnabled)
            {
                Log.Out("First claim block enabled");
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
            if (InfoTicker.IsEnabled)
            {
                Log.Out("Info ticker enabled");
            }
            if (KillMe.IsEnabled)
            {
                Log.Out("Kill me command enabled");
            }
            if (Motd.IsEnabled)
            {
                Log.Out("Motd enabled");
            }
            if (NewSpawnTele.IsEnabled)
            {
                Log.Out("New spawn teleport enabled");
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
            if (Travel.IsEnabled)
            {
                Log.Out("Travel enabled");
            }
            if (VoteReward.IsEnabled)
            {
                Log.Out("Vote reward enabled");
            }
            if (ZoneProtection.IsEnabled)
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
                FlightCheck.AutoFlightCheck();
            }
            if (HatchElevator.IsEnabled)
            {
                HatchElevator.AutoHatchCheck();
            }
            if (UndergroundCheck.IsEnabled)
            {
                UndergroundCheck.AutoUndergroundCheck();
            }
            if (ZoneProtection.IsEnabled)
            {
                ZoneProtection.Check();
            }
            if (Jail.IsEnabled)
            {
                Jail.StatusCheck();
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
            if (RestartVote.IsEnabled)
            {
                if (RestartVote.VoteOpen)
                {
                    _rV++;
                    if (_rV >= 30)
                    {
                        _rV = 0;
                        RestartVote.VoteOpen = false;
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
            if (EntityCleanup.IsEnabled)
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
            if (DeathSpot.IsEnabled)
            {
                _dS++;
                if (_dS >= 5)
                {
                    _dS = 0;
                    DeathSpot.CheckDead();
                }
            }
            else
            {
                _dS = 0;
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
            //if (WorldRadius.IsEnabled)
            //{
            //    _wR++;
            //    if (_wR >= 2)
            //    {
            //        _wR = 0;
            //        WorldRadius.WorldRad();
            //    }
            //}
            //else
            //{
            //    _wR = 0;
            //}
            if (EntityUnderground.IsEnabled)
            {
                _eU++;
                if (_eU >= 10)
                {
                    _eU = 0;
                    EntityUnderground.AutoEntityUnderground();
                }
            }
            else
            {
                _eU = 0;
            }
            if (ReservedSlots.IsEnabled)
            {
                _rS++;
                if (_rS >= 300)
                {
                    _rS = 0;
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                    foreach (var _cInfo in _cInfoList)
                    {
                        ReservedSlots.CheckReservedSlot(_cInfo);
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
                    if (PlayerLogs.Position)
                    {
                        PlayerLogs.Move_Log();
                    }
                    if (PlayerLogs.Inventory)
                    {
                        PlayerLogs.Player_InvLog();
                    }
                    if (PlayerLogs.P_Data)
                    {
                        PlayerLogs.Player_Data();
                    }
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
            if (AutoShutdown.IsEnabled)
            {
                if (AutoShutdown.IsEnabled & _sD == 0)
                {
                    AutoShutdown.ShutdownList();
                }
                _sD++;
                if (_sD >= Shutdown_Delay * 60)
                {
                    _sD = 0;
                    AutoShutdown.ShutdownList();
                    AutoShutdown.Auto_Shutdown();
                }
            }
            else
            {
                _sD = 0;
            }
            if (InfoTicker.IsEnabled)
            {
                _iD++;
                if (_iD >= Infoticker_Delay * 60)
                {
                    _iD = 0;
                    InfoTicker.StatusCheck();
                }
            }
            else
            {
                _iD = 0;
            }
        }

        public static void TimerCheck()
        {
            if (!FlightCheck.IsEnabled & !HatchElevator.IsEnabled & !UndergroundCheck.IsEnabled & !ZoneProtection.IsEnabled 
                & !WeatherVote.IsEnabled & !PlayerStatCheck.IsEnabled & !Bloodmoon.IsEnabled & !WorldRadius.IsEnabled & !PlayerLogs.IsEnabled
                & !AutoSaveWorld.IsEnabled & !StopServer.stopServerCountingDown & !Jail.IsEnabled & !AutoShutdown.IsEnabled
                & !InfoTicker.IsEnabled & !EntityUnderground.IsEnabled)
            {
                TimerStop1Second();
            }
        }
    }
}

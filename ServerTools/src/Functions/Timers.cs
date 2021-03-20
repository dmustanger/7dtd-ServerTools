using System.Timers;
using ServerTools.AntiCheat;
using ServerTools.Website;

namespace ServerTools
{
    class Timers
    {
        public static bool IsRunning = false;
        public static int StopServerMinutes, _eventTime;
        private static int CoreCount = 0, _twoSecondTick, _fiveSecondTick, _tenSecondTick, _twentySecondTick, _oneMinTick, _fiveMinTick, _stopServerSeconds, _eventInvitation,
            _eventOpen, _horde, _kickVote, _lottery, _muteVote, _restartVoteCycle, _restartVote, _weatherVote;
        private static System.Timers.Timer Core = new System.Timers.Timer();

        public static void TimerStart()
        {
            if (CoreCount < 1)
            {
                CoreCount++;
                IsRunning = true;
                Core.Interval = 1000;
                Core.Start();
                Core.Elapsed += new ElapsedEventHandler(Tick);
            }
        }

        public static void TimerStop()
        {
            CoreCount = 0;
            IsRunning = false;
            Core.Stop();
            Core.Close();
        }

        private static void Tick(object sender, ElapsedEventArgs e)
        {
            _twoSecondTick++;
            _fiveSecondTick++;
            _tenSecondTick++;
            _twentySecondTick++;
            _oneMinTick++;
            _fiveMinTick++;
            Exec();
        }

        public static void SingleUseTimer(int _delay, string _playerId, string _commands)
        {
            if (_delay > 120)
            {
                _delay = 120;
            }
            int _delayAdjusted = _delay * 1000;
            System.Timers.Timer _singleUseTimer = new System.Timers.Timer(_delayAdjusted);
            _singleUseTimer.AutoReset = false;
            _singleUseTimer.Start();
            _singleUseTimer.Elapsed += (sender, e) =>
            {
                Init1(sender, e, _playerId, _commands);
                _singleUseTimer.Close();
            };
        }

        public static void NewPlayerTimer(ClientInfo _cInfo)
        {
            System.Timers.Timer _newPlayerExecTimer = new System.Timers.Timer(5000);
            _newPlayerExecTimer.AutoReset = false;
            _newPlayerExecTimer.Start();
            _newPlayerExecTimer.Elapsed += (sender, e) =>
            {
                Init2(sender, e, _cInfo);
                _newPlayerExecTimer.Close();
            };
        }

        public static void StartingItemsTimer(ClientInfo _cInfo)
        {
            System.Timers.Timer _newPlayerStartingItemsTimer = new System.Timers.Timer(3000);
            _newPlayerStartingItemsTimer.AutoReset = false;
            _newPlayerStartingItemsTimer.Start();
            _newPlayerStartingItemsTimer.Elapsed += (sender, e) =>
            {
                Init3(sender, e, _cInfo);
                _newPlayerStartingItemsTimer.Close();
            };
        }

        public static void DisconnectHardcorePlayer(ClientInfo _cInfo)
        {
            System.Timers.Timer _hardcoreTimer = new System.Timers.Timer(20000);
            _hardcoreTimer.AutoReset = false;
            _hardcoreTimer.Start();
            _hardcoreTimer.Elapsed += (sender, e) =>
            {
                Init4(sender, e, _cInfo);
                _hardcoreTimer.Close();
            };
        }

        public static void BattleLogPlayerExit(string _id, int _time)
        {
            System.Timers.Timer _exitTimer = new System.Timers.Timer(_time * 1000);
            _exitTimer.AutoReset = false;
            _exitTimer.Start();
            _exitTimer.Elapsed += (sender, e) =>
            {
                Init5(sender, e, _id);
                _exitTimer.Close();
            };
        }

        public static void BattleLogDelay(ClientInfo _cInfo, string _ip)
        {
            System.Timers.Timer _exitDelay = new System.Timers.Timer(2500);
            _exitDelay.AutoReset = false;
            _exitDelay.Start();
            _exitDelay.Elapsed += (sender, e) =>
            {
                Init6(sender, e, _cInfo, _ip);
                _exitDelay.Close();
            };
        }

        public static void SaveDelay()
        {
            System.Timers.Timer _saveDelay = new System.Timers.Timer(1000);
            _saveDelay.AutoReset = false;
            _saveDelay.Start();
            _saveDelay.Elapsed += (sender, e) =>
            {
                PersistentContainer.Instance.Save();
                _saveDelay.Close();
            };
        }

        public static void ShutdownFailsafe()
        {
            System.Timers.Timer _shutdownFailsafe = new System.Timers.Timer(60000);
            _shutdownFailsafe.AutoReset = false;
            _shutdownFailsafe.Start();
            _shutdownFailsafe.Elapsed += (sender, e) =>
            {
                StopServer.FailSafe(sender, e);
                _shutdownFailsafe.Close();
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
            if (PlayerChecks.FlyEnabled)
            {
                Log.Out("Flying detector enabled");
            }
            if (PlayerChecks.GodEnabled)
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
            if (PlayerStats.IsEnabled)
            {
                Log.Out("Player stats enabled");
            }
            if (PlayerLogs.IsEnabled)
            {
                Log.Out("Player logs enabled");
            }
            if (ProtectedSpaces.IsEnabled)
            {
                Log.Out("Protected spaces enabled");
            }
            if (PlayerChecks.SpectatorEnabled)
            {
                Log.Out("Spectator detector enabled");
            }
            //if (TeleportCheck.IsEnabled)
            //{
            //    Log.Out("Teleport enabled");
            //}
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
            if (ChatHook.Normal_Player_Color_Prefix)
            {
                Log.Out("Normal player chat color and prefix enabled");
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
            if (Auction.IsEnabled)
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
            if (Badwords.IsEnabled)
            {
                Log.Out("Badword filter enabled");
            }
            if (Bank.IsEnabled)
            {
                Log.Out("Bank enabled");
            }
            if (BattleLogger.IsEnabled)
            {
                Log.Out("Battle logger enabled");
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
            if (ChatCommandLog.IsEnabled)
            {
                Log.Out("Chat command log enabled");
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
            if (ConsoleCommandLog.IsEnabled)
            {
                Log.Out("Console command log enabled");
            }
            if (CountryBan.IsEnabled)
            {
                Log.Out("Country ban enabled");
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
                Log.Out("Entity falling tree cleanup enabled");
            }
            if (FallingBlocks.IsEnabled)
            {
                Log.Out("Falling blocks remover enabled");
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
                Log.Out("Info ticker enabled");
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
            if (POIProtection.IsEnabled)
            {
                Log.Out("POI protection enabled");
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
            if (Shutdown.IsEnabled)
            {
                Log.Out("Shutdown enabled");
            }
            if (Home.IsEnabled)
            {
                Log.Out("Set home enabled");
            }
            if (Shop.IsEnabled)
            {
                Log.Out("Shop enabled");
            }
            if (SleeperRespawn.IsEnabled)
            {
                Log.Out("Sleeper respawn enabled");
            }
            if (StartingItems.IsEnabled)
            {
                Log.Out("Starting items enabled");
            }
            if (Suicide.IsEnabled)
            {
                Log.Out("Suicide enabled");
            }
            if (Track.IsEnabled)
            {
                Log.Out("Tracking enabled");
            }
            if (Travel.IsEnabled)
            {
                Log.Out("Travel enabled");
            }
            if (PlayerChecks.WaterEnabled)
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
            if (WebPanel.IsEnabled)
            {
                Log.Out("Web panel enabled");
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

        private static void Exec()
        {
            PersistentOperations.PlayerCheck();
            if (Jail.IsEnabled)
            {
                Jail.StatusCheck();
            }
            if (_twoSecondTick >= 2)
            {
                _twoSecondTick = 0;
                if (WorldRadius.IsEnabled)
                {
                    WorldRadius.Exec();
                }
                if (PlayerChecks.GodEnabled || PlayerChecks.FlyEnabled || PlayerChecks.SpectatorEnabled || PlayerChecks.WaterEnabled)
                {
                    PlayerChecks.Exec();
                }
            }
            if (_fiveSecondTick >= 5)
            {
                _fiveSecondTick = 0;
                if (Zones.IsEnabled)
                {
                    Zones.HostileCheck();
                }
                if (PlayerStats.IsEnabled)
                {
                    PlayerStats.Exec();
                }
            }
            if (_tenSecondTick >= 10)
            {
                _tenSecondTick = 0;
                if (EntityCleanup.IsEnabled)
                {
                    EntityCleanup.EntityCheck();
                }
                EventSchedule.Exec();
            }
            if (_twentySecondTick >= 20)
            {
                _twentySecondTick = 0;
                if (Track.IsEnabled)
                {
                    Track.Exec();
                }
            }
            if (_oneMinTick >= 60)
            {
                _oneMinTick = 0;
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
            }
            if (_fiveMinTick >= 300)
            {
                _fiveMinTick = 0;
                StateManager.Save();
                if (InvalidItems.Check_Storage)
                {
                    InvalidItems.CheckStorage();
                }
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
            if (StopServer.ShuttingDown)
            {
                _stopServerSeconds++;
                if (_stopServerSeconds >= 60)
                {
                    _stopServerSeconds = 0;
                    StopServerMinutes--;
                    if (StopServerMinutes > 1)
                    {
                        StopServer.TimeRemaining(StopServerMinutes);
                    }
                    else if (StopServerMinutes == 1)
                    {
                        StopServer.OneMinuteRemains();
                    }
                    else if (StopServerMinutes == 0)
                    {
                        StopServer.ShuttingDown = false;
                        StopServer.Stop();
                    }
                }
                if (_stopServerSeconds == 30 && StopServerMinutes == 1)
                {
                    StopServer.Kick30();
                }
            }
            else
            {
                _stopServerSeconds = 0;
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
            if (RestartVote.Cycle)
            {
                _restartVoteCycle++;
                if (_restartVoteCycle >= 1800)
                {
                    RestartVote.Cycle = false;
                }
            }
        }

        private static void Init1(object sender, ElapsedEventArgs e, string _playerId, string _commands)
        {
            CustomCommands.DelayedCommand(_playerId, _commands);
        }

        private static void Init2(object sender, ElapsedEventArgs e, ClientInfo _cInfo)
        {
            API.NewPlayerExec(_cInfo);
        }

        private static void Init3(object sender, ElapsedEventArgs e, ClientInfo _cInfo)
        {
            StartingItems.Exec(_cInfo);
        }

        private static void Init4(object sender, ElapsedEventArgs e, ClientInfo _cInfo)
        {
            Hardcore.KickPlayer(_cInfo);
        }

        private static void Init5(object sender, ElapsedEventArgs e, string _id)
        {
            BattleLogger.PlayerExit(_id);
        }

        private static void Init6(object sender, ElapsedEventArgs e, ClientInfo _cInfo, string _ip)
        {
            BattleLogger.BattleLog(_cInfo, _ip);
        }
    }
}

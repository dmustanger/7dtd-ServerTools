using System.Collections.Generic;
using System.Timers;

namespace ServerTools
{
    class Timers
    {
        public static bool IsRunning = false;
        public static int StopServerMinutes, _eventTime;
        private static int CoreCount = 0, _twoSecondTick, _fiveSecondTick, _tenSecondTick, _twentySecondTick, _oneMinTick, _fiveMinTick, _stopServerSeconds, _eventInvitation,
            _eventOpen, _horde, _kickVote, _lottery, _muteVote, _restartVoteCycle, _restartVote, _weatherVote;
        private static readonly System.Timers.Timer Core = new System.Timers.Timer();

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
            Core.Dispose();
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

        public static void Custom_SingleUseTimer(int _delay, string _playerId, List<string> _commands)
        {
            if (_delay > 180)
            {
                _delay = 180;
            }
            int _delayAdjusted = _delay * 1000;
            System.Timers.Timer _singleUseTimer = new System.Timers.Timer(_delayAdjusted)
            {
                AutoReset = false
            };
            _singleUseTimer.Start();
            _singleUseTimer.Elapsed += (sender, e) =>
            {
                Init1(_playerId, _commands);
                _singleUseTimer.Stop();
                _singleUseTimer.Close();
                _singleUseTimer.Dispose();
            };
        }

        public static void Zone_SingleUseTimer(int _delay, string _playerId, List<string> _commands)
        {
            if (_delay > 180)
            {
                _delay = 180;
            }
            int _delayAdjusted = _delay * 1000;
            System.Timers.Timer _singleUseTimer = new System.Timers.Timer(_delayAdjusted)
            {
                AutoReset = false
            };
            _singleUseTimer.Start();
            _singleUseTimer.Elapsed += (sender, e) =>
            {
                Init7(_playerId, _commands);
                _singleUseTimer.Stop();
                _singleUseTimer.Close();
                _singleUseTimer.Dispose();
            };
        }

        public static void Level_SingleUseTimer(int _delay, string _playerId, List<string> _commands)
        {
            if (_delay > 180)
            {
                _delay = 180;
            }
            int _delayAdjusted = _delay * 1000;
            System.Timers.Timer _singleUseTimer = new System.Timers.Timer(_delayAdjusted)
            {
                AutoReset = false
            };
            _singleUseTimer.Start();
            _singleUseTimer.Elapsed += (sender, e) =>
            {
                Init8(_playerId, _commands);
                _singleUseTimer.Stop();
                _singleUseTimer.Close();
                _singleUseTimer.Dispose();
            };
        }

        public static void NewPlayerTimer(ClientInfo _cInfo)
        {
            System.Timers.Timer _newPlayerExecTimer = new System.Timers.Timer(5000)
            {
                AutoReset = false
            };
            _newPlayerExecTimer.Start();
            _newPlayerExecTimer.Elapsed += (sender, e) =>
            {
                Init2(_cInfo);
                _newPlayerExecTimer.Stop();
                _newPlayerExecTimer.Close();
                _newPlayerExecTimer.Dispose();
            };
        }

        public static void StartingItemsTimer(ClientInfo _cInfo)
        {
            System.Timers.Timer _newPlayerStartingItemsTimer = new System.Timers.Timer(3000)
            {
                AutoReset = false
            };
            _newPlayerStartingItemsTimer.Start();
            _newPlayerStartingItemsTimer.Elapsed += (sender, e) =>
            {
                Init3(_cInfo);
                _newPlayerStartingItemsTimer.Stop();
                _newPlayerStartingItemsTimer.Close();
                _newPlayerStartingItemsTimer.Dispose();
            };
        }

        public static void DisconnectHardcorePlayer(ClientInfo _cInfo)
        {
            System.Timers.Timer _hardcoreTimer = new System.Timers.Timer(20000)
            {
                AutoReset = false
            };
            _hardcoreTimer.Start();
            _hardcoreTimer.Elapsed += (sender, e) =>
            {
                Init4(_cInfo);
                _hardcoreTimer.Stop();
                _hardcoreTimer.Close();
                _hardcoreTimer.Dispose();
            };
        }

        public static void ExitWithCommand(int _id, int _time)
        {
            System.Timers.Timer _exitCommand = new System.Timers.Timer(_time * 1000)
            {
                AutoReset = false
            };
            _exitCommand.Start();
            _exitCommand.Elapsed += (sender, e) =>
            {
                Init5(_id);
                _exitCommand.Stop();
                _exitCommand.Close();
                _exitCommand.Dispose();
            };
        }

        public static void ExitWithoutCommand(ClientInfo _cInfo, string _ip)
        {
            System.Timers.Timer _exitDelay = new System.Timers.Timer(1500)
            {
                AutoReset = false
            };
            _exitDelay.Start();
            _exitDelay.Elapsed += (sender, e) =>
            {
                Init6(_cInfo, _ip);
                _exitDelay.Stop();
                _exitDelay.Close();
                _exitDelay.Dispose();
            };
        }

        public static void PersistentDataSave()
        {
            System.Timers.Timer _saveDelay = new System.Timers.Timer(15000)
            {
                AutoReset = true
            };
            _saveDelay.Start();
            _saveDelay.Elapsed += (sender, e) =>
            {
                PersistentContainer.Instance.Save();
            };
        }

        private static void Exec()
        {
            PersistentOperations.PlayerCheck();
            if (Jail.IsEnabled)
            {
                Jail.StatusCheck();
            }
            if (DiscordBot.IsEnabled && DiscordBot.Queue.Count > 0)
            {
                DiscordBot.WebHook();
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
                if (Fps.IsEnabled)
                {
                    Fps.LowFPS();
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

        private static void Init1(string _playerId, List<string> _commands)
        {
            CustomCommands.CustomCommandDelayed(_playerId, _commands);
        }

        private static void Init2(ClientInfo _cInfo)
        {
            API.NewPlayerExec(_cInfo);
        }

        private static void Init3(ClientInfo _cInfo)
        {
            StartingItems.Exec(_cInfo);
        }

        private static void Init4(ClientInfo _cInfo)
        {
            Hardcore.KickPlayer(_cInfo);
        }

        private static void Init5(int _id)
        {
            ExitCommand.ExitWithCommand(_id);
        }

        private static void Init6(ClientInfo _cInfo, string _ip)
        {
            ExitCommand.ExitWithoutCommand(_cInfo, _ip);
        }

        private static void Init7(string _playerId, List<string> _commands)
        {
            Zones.ZoneCommandDelayed(_playerId, _commands);
        }

        private static void Init8(string _playerId, List<string> _commands)
        {
            LevelUp.LevelCommandDelayed(_playerId, _commands);
        }
    }
}

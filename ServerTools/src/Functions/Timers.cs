using System.Collections.Generic;
using System.Timers;

namespace ServerTools
{
    class Timers
    {
        public static bool IsRunning = false;
        public static int StopServerMinutes = 0, eventTime = 0;
        private static int CoreCount = 0, twoSecondTick, fiveSecondTick, tenSecondTick, twentySecondTick, oneMinTick, fiveMinTick, stopServerSeconds, eventInvitation,
            eventOpen, horde, kickVote, lottery, muteVote, newPlayer, restartVoteCycle, restartVote, weatherVote;
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
            twoSecondTick++;
            fiveSecondTick++;
            tenSecondTick++;
            twentySecondTick++;
            oneMinTick++;
            fiveMinTick++;
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
            System.Timers.Timer _saveDelay = new System.Timers.Timer(120000)
            {
                AutoReset = true
            };
            _saveDelay.Start();
            _saveDelay.Elapsed += (sender, e) =>
            {
                PersistentContainer.Instance.Save();
            };
        }

        public static void Delayed_Web_API()
        {
            System.Timers.Timer _singleUseTimer = new System.Timers.Timer(30000)
            {
                AutoReset = false
            };
            _singleUseTimer.Start();
            _singleUseTimer.Elapsed += (sender, e) =>
            {
                Init9();
                _singleUseTimer.Stop();
                _singleUseTimer.Close();
                _singleUseTimer.Dispose();
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
            if (twoSecondTick >= 2)
            {
                twoSecondTick = 0;
                if (WorldRadius.IsEnabled)
                {
                    WorldRadius.Exec();
                }
                if (PlayerChecks.GodEnabled || PlayerChecks.FlyEnabled || PlayerChecks.SpectatorEnabled || PlayerChecks.WaterEnabled)
                {
                    PlayerChecks.Exec();
                }
            }
            if (fiveSecondTick >= 5)
            {
                fiveSecondTick = 0;
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
            if (tenSecondTick >= 10)
            {
                tenSecondTick = 0;
                if (EntityCleanup.IsEnabled)
                {
                    EntityCleanup.EntityCheck();
                }
            }
            if (twentySecondTick >= 20)
            {
                twentySecondTick = 0;
                if (Track.IsEnabled)
                {
                    Track.Exec();
                }
                EventSchedule.Exec();
            }
            if (oneMinTick >= 60)
            {
                oneMinTick = 0;
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
            if (fiveMinTick >= 300)
            {
                fiveMinTick = 0;
                StateManager.Save();
                if (InvalidItems.Check_Storage)
                {
                    InvalidItems.CheckStorage();
                }
            }
            if (PersistentOperations.NewPlayerQue.Count > 0)
            {
                newPlayer++;
                if (newPlayer >= 5)
                {
                    ClientInfo _cInfo = PersistentOperations.NewPlayerQue[0];
                    PersistentOperations.NewPlayerQue.RemoveAt(0);
                    API.NewPlayerExec(_cInfo);
                }
            }
            if (WeatherVote.IsEnabled && WeatherVote.VoteOpen)
            {
                weatherVote++;
                if (weatherVote >= 60)
                {
                    weatherVote = 0;
                    WeatherVote.VoteOpen = false;
                    WeatherVote.ProcessWeatherVote();
                }
            }
            if (RestartVote.IsEnabled && RestartVote.VoteOpen)
            {
                restartVote++;
                if (restartVote >= 60)
                {
                    restartVote = 0;
                    RestartVote.VoteOpen = false;
                    RestartVote.ProcessRestartVote();
                }
            }
            if (MuteVote.IsEnabled && MuteVote.VoteOpen)
            {
                muteVote++;
                if (muteVote >= 60)
                {
                    muteVote = 0;
                    MuteVote.VoteOpen = false;
                    MuteVote.ProcessMuteVote();
                }
            }
            if (KickVote.IsEnabled && KickVote.VoteOpen)
            {
                kickVote++;
                if (kickVote >= 60)
                {
                    kickVote = 0;
                    KickVote.VoteOpen = false;
                    KickVote.ProcessKickVote();
                }
            }
            if (Lottery.IsEnabled && Lottery.OpenLotto)
            {
                lottery++;
                if (lottery == 3300)
                {
                    Lottery.Alert();
                }
                if (lottery >= 3600)
                {
                    lottery = 0;
                    Lottery.StartLotto();
                }
            }
            else
            {
                lottery = 0;
            }
            if (Hordes.IsEnabled)
            {
                horde++;
                if (horde >= 1200)
                {
                    horde = 0;
                    Hordes.Exec();
                }
            }
            else
            {
                horde = 0;
            }
            if (Shutdown.ShuttingDown)
            {
                stopServerSeconds++;
                if (stopServerSeconds >= 60)
                {
                    stopServerSeconds = 0;
                    StopServerMinutes--;
                    if (StopServerMinutes > 1)
                    {
                        Shutdown.TimeRemaining(StopServerMinutes);
                    }
                    else if (StopServerMinutes == 1)
                    {
                        Shutdown.OneMinute();
                    }
                    else if (StopServerMinutes == 0)
                    {
                        Shutdown.ShuttingDown = false;
                        Shutdown.Close();
                    }
                }
                if (StopServerMinutes == 1)
                {
                    if (Shutdown.UI_Lock && stopServerSeconds == 15)
                    {
                        Shutdown.Lock();
                    }
                    else if (stopServerSeconds == 30)
                    {
                        Shutdown.Kick();
                    }
                }
            }
            else
            {
                stopServerSeconds = 0;
            }
            if (Event.Invited)
            {
                eventInvitation++;
                if (eventInvitation >= 900)
                {
                    eventInvitation = 0;
                    Event.Invited = false;
                    Event.CheckOpen();
                }
            }
            if (Event.Open)
            {
                eventOpen++;
                if (eventOpen == eventTime / 2)
                {
                    Event.HalfTime();
                }
                if (eventOpen == eventTime - 300)
                {
                    Event.FiveMin();
                }
                if (eventOpen >= eventTime)
                {
                    eventOpen = 0;
                    Event.EndEvent();
                }
            }
            else
            {
                eventOpen = 0;
            }
            if (RestartVote.Cycle)
            {
                restartVoteCycle++;
                if (restartVoteCycle >= 1800)
                {
                    RestartVote.Cycle = false;
                }
            }
        }

        private static void Init1(string _playerId, List<string> _commands)
        {
            CustomCommands.CustomCommandDelayed(_playerId, _commands);
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

        private static void Init9()
        {
            if (WebAPI.IsEnabled && !WebAPI.IsRunning)
            {
                WebAPI.Load();
            }
        }
    }
}

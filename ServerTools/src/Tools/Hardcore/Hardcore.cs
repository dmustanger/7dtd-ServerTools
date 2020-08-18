using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class Hardcore
    {
        public static bool IsEnabled = false, Optional = false;
        public static int Max_Deaths = 9, Max_Extra_Lives = 3, Life_Price = 2000;
        public static string Command11 = "top3", Command12 = "score", Command101 = "buy life", Command127 = "hardcore", Command128 = "hardcore on";

        public static void Check(ClientInfo _cInfo, EntityPlayer _player)
        {
            string[] _stats = PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreStats;
            if (int.TryParse(_stats[1], out int _deaths))
            {
                if (int.TryParse(_stats[2], out int _extraLives))
                {
                    int _lives = Max_Deaths - (XUiM_Player.GetDeaths(_player) - _deaths) + _extraLives;
                    if (_lives > 0)
                    {
                        string _phrase949;
                        if (!Phrases.Dict.TryGetValue(949, out _phrase949))
                        {
                            _phrase949 = "Hardcore mode is enabled! You have {Lives} lives remaining...";
                        }
                        _phrase949 = _phrase949.Replace("{Lives}", _lives.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase949 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        EndGame(_cInfo, _player, _stats);
                    }
                }
            }
        }

        public static void EndGame(ClientInfo _cInfo, EntityPlayer _player, string[] _stats)
        {
            PersistentPlayer _p = PersistentContainer.Instance.Players[_cInfo.playerId];
            if (_p != null)
            {
                PersistentOperations.Session.TryGetValue(_cInfo.playerId, out DateTime _time);
                TimeSpan varTime = DateTime.Now - _time;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                int _oldSession = PersistentContainer.Instance.Players[_cInfo.playerId].SessionTime;
                int _newSession = _oldSession + _timepassed;
                int.TryParse(_stats[2], out int _extraLives);
                string[] _newStats = { _cInfo.playerName, _newSession.ToString(), _player.KilledPlayers.ToString(), _player.KilledZombies.ToString(), Max_Deaths + _extraLives.ToString(), _player.Score.ToString() };
                if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreSavedStats != null && PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreSavedStats.Count > 0)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreSavedStats.Add(_newStats);
                }
                else
                {
                    List<string[]> SavedStats = new List<string[]>();
                    SavedStats.Add(_newStats);
                    PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreSavedStats = SavedStats;
                }
                _p.Auction = new Dictionary<int, ItemDataSerializable>();
                _p.AuctionReturn = new Dictionary<int, ItemDataSerializable>();
                _p.Bank = 0;
                _p.BikeId = 0;
                _p.Bounty = 0;
                _p.BountyHunter = 0;
                _p.ClanInvite = "";
                _p.ClanName = "";
                _p.ClanOfficer = false;
                _p.ClanOwner = false;
                _p.ClanRequestToJoin = new Dictionary<string, string>();
                _p.CountryBanImmune = false;
                _p.CustomCommand1 = new DateTime();
                _p.CustomCommand2 = new DateTime();
                _p.CustomCommand3 = new DateTime();
                _p.CustomCommand4 = new DateTime();
                _p.CustomCommand5 = new DateTime();
                _p.CustomCommand6 = new DateTime();
                _p.CustomCommand7 = new DateTime();
                _p.CustomCommand8 = new DateTime();
                _p.CustomCommand9 = new DateTime();
                _p.CustomCommand10 = new DateTime();
                _p.CustomCommand11 = new DateTime();
                _p.CustomCommand12 = new DateTime();
                _p.CustomCommand13 = new DateTime();
                _p.CustomCommand14 = new DateTime();
                _p.CustomCommand15 = new DateTime();
                _p.CustomCommand16 = new DateTime();
                _p.CustomCommand17 = new DateTime();
                _p.CustomCommand18 = new DateTime();
                _p.CustomCommand19 = new DateTime();
                _p.CustomCommand20 = new DateTime();
                _p.FirstClaimBlock = false;
                _p.GyroId = 0;
                _p.HardcoreEnabled = false;
                _p.HardcoreStats = new string[0];
                _p.HighPingImmune = false;
                _p.HighPingImmuneName = "";
                _p.HomePosition1 = "";
                _p.HomePosition2 = "";
                _p.JailDate = new DateTime();
                _p.JailName = "";
                _p.JailTime = 0;
                _p.JeepId = 0;
                _p.LastAnimal = new DateTime();
                _p.LastBike = new DateTime();
                _p.LastDied = new DateTime();
                _p.LastFriendTele = new DateTime();
                _p.LastGimme = new DateTime();
                _p.LastGyro = new DateTime();
                _p.LastHome1 = new DateTime();
                _p.LastHome2 = new DateTime();
                _p.LastJeep = new DateTime();
                _p.LastJoined = new DateTime();
                _p.LastKillMe = new DateTime();
                _p.LastLobby = new DateTime();
                _p.LastLog = new DateTime();
                _p.LastMarket = new DateTime();
                _p.LastMiniBike = new DateTime();
                _p.LastMotorBike = new DateTime();
                _p.LastStuck = new DateTime();
                _p.LastTravel = new DateTime();
                _p.LastVote = new DateTime();
                _p.LastVoteWeek = new DateTime();
                _p.LastWhisper = "";
                _p.LobbyReturnPos = "";
                _p.MarketReturnPos = "";
                _p.MiniBikeId = 0;
                _p.MotorBikeId = 0;
                _p.MuteDate = new DateTime();
                _p.MuteName = "";
                _p.MuteTime = 0;
                _p.NewSpawn = false;
                _p.NewSpawnPosition = "";
                _p.OldPlayer = false;
                _p.PlayerName = "";
                _p.PlayerWallet = 0;
                _p.SessionTime = 0;
                _p.StartingItems = false;
                _p.TotalTimePlayed = 0;
                _p.VoteWeekCount = 0;
                _p.WP = "";
                _p.ZoneDeathTime = new DateTime();
                PersistentContainer.Instance.Save();
                Hardcore.Disconnect(_cInfo, _newStats);
            }
        }

        public static void TopThree(ClientInfo _cInfo)
        {
            int _topSession1 = 0, _topSession2 = 0, _topSession3 = 0, _topScore1 = 0, _topScore2 = 0, _topScore3 = 0;
            string _sessionName1 = "", _sessionName2 = "", _sessionName3 = "", _ScoreName1 = "", _ScoreName2 = "", _ScoreName3 = "";
            List<string> _persistentPlayers = PersistentContainer.Instance.Players.SteamIDs;
            for (int i = 0; i < _persistentPlayers.Count; i++)
            {
                if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreSavedStats != null && PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreSavedStats.Count > 0)
                {
                    List<string[]> _hardcoreSavedStats = PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreSavedStats;
                    for (int j = 0; j < _hardcoreSavedStats.Count; j++)
                    {
                        string[] _stats = _hardcoreSavedStats[j];
                        int.TryParse(_stats[1], out int _sessionTime);
                        if (_sessionTime > _topSession1)
                        {
                            _topSession3 = _topSession2;
                            _sessionName3 = _sessionName2;
                            _topSession2 = _topSession1;
                            _sessionName2 = _sessionName1;
                            _topSession1 = _sessionTime;
                            _sessionName1 = _stats[0];
                        }
                        else if (_sessionTime > _topSession2)
                        {
                            _topSession3 = _topSession2;
                            _sessionName3 = _sessionName2;
                            _topSession2 = _sessionTime;
                            _sessionName2 = _stats[0];
                        }
                        else if (_sessionTime > _topSession3)
                        {
                            _topSession3 = _sessionTime;
                            _sessionName3 = _stats[0];
                        }
                        int.TryParse(_stats[4], out int _score);
                        if (_score > _topScore1)
                        {
                            _topScore3 = _topScore2;
                            _ScoreName3 = _ScoreName2;
                            _topScore2 = _topScore1;
                            _ScoreName2 = _ScoreName1;
                            _topScore1 = _score;
                            _ScoreName1 = _stats[0];
                        }
                        else if (_score > _topScore2)
                        {
                            _topScore3 = _topScore2;
                            _ScoreName3 = _ScoreName2;
                            _topScore2 = _score;
                            _ScoreName2 = _stats[0];
                        }
                        else if (_score > _topScore3)
                        {
                            _topScore3 = _score;
                            _ScoreName3 = _stats[0];
                        }
                    }
                }
            }
            if (_sessionName1 != "" || _ScoreName1 != null)
            {
                string _phrase945;
                if (!Phrases.Dict.TryGetValue(945, out _phrase945))
                {
                    _phrase945 = "Hardcore Top Players";
                }
                string _phrase946;
                if (!Phrases.Dict.TryGetValue(946, out _phrase946))
                {
                    _phrase946 = "Playtime 1 {Name1}, {Session1}. Playtime 2 {Name2}, {Session3}. Playtime 3 {Name3}, {Session3}";
                }
                _phrase946 = _phrase946.Replace("{Name1}", _sessionName1);
                _phrase946 = _phrase946.Replace("{Session1}", _topSession1.ToString());
                _phrase946 = _phrase946.Replace("{Name2}", _sessionName2);
                _phrase946 = _phrase946.Replace("{Session2}", _topSession2.ToString());
                _phrase946 = _phrase946.Replace("{Name3}", _sessionName3);
                _phrase946 = _phrase946.Replace("{Session3}", _topSession3.ToString());
                string _phrase947;
                if (!Phrases.Dict.TryGetValue(947, out _phrase947))
                {
                    _phrase947 = "Score 1 {Name1}, {Score1}. Score 2 {Name2}, {Score2}. Score 3 {Name3}, {Score3}";
                }
                _phrase947 = _phrase947.Replace("{Name1}", _ScoreName1);
                _phrase947 = _phrase947.Replace("{Score1}", _topScore1.ToString());
                _phrase947 = _phrase947.Replace("{Name2}", _ScoreName2);
                _phrase947 = _phrase947.Replace("{Score2}", _topScore2.ToString());
                _phrase947 = _phrase947.Replace("{Name3}", _ScoreName3);
                _phrase947 = _phrase947.Replace("{Score3}", _topScore3.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase945 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase946 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase947 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "There are no hardcore records" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
        }

        public static void Score(ClientInfo _cInfo)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreSavedStats != null && PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreSavedStats.Count > 0)
                {
                    List<string[]> _hardcoreSavedStats = PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreSavedStats;
                    for (int i = 0; i < _hardcoreSavedStats.Count; i++)
                    {
                        string[] _stats = _hardcoreSavedStats[i];
                        string _phrase948;
                        if (!Phrases.Dict.TryGetValue(948, out _phrase948))
                        {
                            _phrase948 = "Hardcore stats: Name {PlayerName}, Playtime {PlayTime}, Player Kills {PlayerKills}, Zombie Kills {ZombieKills}, Deaths {Deaths}, Score {Score}";
                        }
                        _phrase948 = _phrase948.Replace("{PlayerName}", _stats[0]);
                        _phrase948 = _phrase948.Replace("{PlayTime}", _stats[1]);
                        _phrase948 = _phrase948.Replace("{PlayerKills}", _stats[2]);
                        _phrase948 = _phrase948.Replace("{ZombieKills}", _stats[3]);
                        _phrase948 = _phrase948.Replace("{Deaths}", _stats[4]);
                        _phrase948 = _phrase948.Replace("{Score}", _stats[5]);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase948 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have no hardcore records" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.Score: {0}", e.Message));
            }
        }

        public static void BuyLife(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    if (Max_Extra_Lives > 0)
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                        {
                            string[] _stats = PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreStats;
                            int.TryParse(_stats[2], out int _extraLives);
                            if (_extraLives < Max_Extra_Lives)
                            {
                                if (Life_Price < 0)
                                {
                                    Life_Price = 0;
                                }
                                int _cost = Life_Price * _extraLives;
                                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= _cost)
                                {
                                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _cost);
                                    _stats[2] = (_extraLives + 1).ToString();
                                    PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreStats = _stats;
                                    PersistentContainer.Instance.Save();
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have bought one extra life." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You need a total of " + _cost.ToString() + " " + Wallet.Coin_Name + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are at the maximum extra lives." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have not turned on hardcore mode." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are at the maximum extra lives." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.BuyLife: {0}", e.Message));
            }
        }

        public static void ResetHardcoreProfile(ClientInfo _cInfo)
        {
            try
            {
                string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                if (!File.Exists(_filepath))
                {
                    SdtdConsole.Instance.Output(string.Format("Could not find file {0}.map", _cInfo.playerId));
                }
                else
                {
                    File.Delete(_filepath);
                }
                if (!File.Exists(_filepath1))
                {
                    SdtdConsole.Instance.Output(string.Format("Could not find file {0}.ttp", _cInfo.playerId));
                }
                else
                {
                    File.Delete(_filepath1);
                }
                RemovePlayerData(_cInfo);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.ResetHardcoreProfile: {0}", e.Message));
            }
        }

        public static void RemovePlayerData(ClientInfo _cInfo)
        {
            try
            {
                PersistentPlayer p = PersistentContainer.Instance.Players[_cInfo.playerId];
                if (p != null)
                {
                    p.Auction = new Dictionary<int, ItemDataSerializable>();
                    p.AuctionReturn = new Dictionary<int, ItemDataSerializable>();
                    p.Bank = 0;
                    p.BikeId = 0;
                    p.Bounty = 0;
                    p.BountyHunter = 0;
                    p.ClanInvite = "";
                    p.ClanName = "";
                    p.ClanOfficer = false;
                    p.ClanOwner = false;
                    p.ClanRequestToJoin = new Dictionary<string, string>();
                    p.CountryBanImmune = false;
                    p.CustomCommand1 = new DateTime();
                    p.CustomCommand2 = new DateTime();
                    p.CustomCommand3 = new DateTime();
                    p.CustomCommand4 = new DateTime();
                    p.CustomCommand5 = new DateTime();
                    p.CustomCommand6 = new DateTime();
                    p.CustomCommand7 = new DateTime();
                    p.CustomCommand8 = new DateTime();
                    p.CustomCommand9 = new DateTime();
                    p.CustomCommand10 = new DateTime();
                    p.CustomCommand11 = new DateTime();
                    p.CustomCommand12 = new DateTime();
                    p.CustomCommand13 = new DateTime();
                    p.CustomCommand14 = new DateTime();
                    p.CustomCommand15 = new DateTime();
                    p.CustomCommand16 = new DateTime();
                    p.CustomCommand17 = new DateTime();
                    p.CustomCommand18 = new DateTime();
                    p.CustomCommand19 = new DateTime();
                    p.CustomCommand20 = new DateTime();
                    p.FirstClaimBlock = false;
                    p.GyroId = 0;
                    p.HardcoreEnabled = false;
                    p.HardcoreSavedStats = new List<string[]>();
                    p.HardcoreStats = new string[0];
                    p.HighPingImmune = false;
                    p.HighPingImmuneName = "";
                    p.HomePosition1 = "";
                    p.HomePosition2 = "";
                    p.JailDate = new DateTime();
                    p.JailName = "";
                    p.JailTime = 0;
                    p.JeepId = 0;
                    p.LastAnimal = new DateTime();
                    p.LastBike = new DateTime();
                    p.LastDied = new DateTime();
                    p.LastFriendTele = new DateTime();
                    p.LastGimme = new DateTime();
                    p.LastGyro = new DateTime();
                    p.LastHome1 = new DateTime();
                    p.LastHome2 = new DateTime();
                    p.LastJeep = new DateTime();
                    p.LastJoined = new DateTime();
                    p.LastKillMe = new DateTime();
                    p.LastLobby = new DateTime();
                    p.LastLog = new DateTime();
                    p.LastMarket = new DateTime();
                    p.LastMiniBike = new DateTime();
                    p.LastMotorBike = new DateTime();
                    p.LastStuck = new DateTime();
                    p.LastTravel = new DateTime();
                    p.LastVote = new DateTime();
                    p.LastVoteWeek = new DateTime();
                    p.LastWhisper = "";
                    p.LobbyReturnPos = "";
                    p.MarketReturnPos = "";
                    p.MiniBikeId = 0;
                    p.MotorBikeId = 0;
                    p.MuteDate = new DateTime();
                    p.MuteName = "";
                    p.MuteTime = 0;
                    p.NewSpawn = false;
                    p.NewSpawnPosition = "";
                    p.OldPlayer = false;
                    p.PlayerName = "";
                    p.PlayerWallet = 0;
                    p.SessionTime = 0;
                    p.StartingItems = false;
                    p.TotalTimePlayed = 0;
                    p.VoteWeekCount = 0;
                    p.Waypoints = new Dictionary<string, string>();
                    p.WP = "";
                    p.ZoneDeathTime = new DateTime();
                    PersistentContainer.Instance.Save();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.RemovePlayerData: {0}", e.Message));
            }
        }

        public static void Disconnect(ClientInfo _cInfo, string[] _newStats)
        {
            try
            {
                if (_cInfo != null)
                {
                    string _phrase951;
                    if (!Phrases.Dict.TryGetValue(951, out _phrase951))
                    {
                        _phrase951 = "Hardcore Game Over: Player {PlayerName}, Playtime {Playtime}, Player Kills {PlayerKills}, Zombie Kills {ZombieKills}, Deaths {Deaths}, Score {Score}";
                    }
                    _phrase951 = _phrase951.Replace("{PlayerName}", _newStats[0]);
                    _phrase951 = _phrase951.Replace("{Playtime}", _newStats[1]);
                    _phrase951 = _phrase951.Replace("{PlayerKills}", _newStats[2]);
                    _phrase951 = _phrase951.Replace("{ZombieKills}", _newStats[3]);
                    _phrase951 = _phrase951.Replace("{Deaths}", _newStats[4]);
                    _phrase951 = _phrase951.Replace("{Score}", _newStats[5]);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase951 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    Timers.DisconnectHardcorePlayer(_cInfo);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.Disconnect: {0}", e.Message));
            }
        }

        public static void KickPlayer(ClientInfo _cInfo)
        {
            try
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto kicked at end of hardcore session\"", _cInfo.playerId), (ClientInfo)null);
                EntityPlayer entityPlayer = (EntityPlayer)GameManager.Instance.World.GetEntity(_cInfo.entityId);
                PersistentOperations.SavePersistentPlayerDataXML();
                PersistentOperations.RemoveAllClaims(_cInfo.playerId);
                PersistentOperations.RemovePersistentPlayerData(_cInfo.playerId);
                PersistentOperations.RemoveAllACL(_cInfo.playerId);
                Hardcore.ResetHardcoreProfile(_cInfo);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.DisconnectHardcorePlayer: {0}", e.Message));
            }
        }
    }
}

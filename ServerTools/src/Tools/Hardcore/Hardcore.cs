using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class Hardcore
    {
        public static bool IsEnabled = false, Optional = true;
        public static int Max_Deaths = 9, Max_Extra_Lives = 3, Life_Price = 2000;
        public static string Command_top3 = "top3", Command_score = "score", Command_buy_life = "buy life", Command_hardcore = "hardcore", Command_hardcore_on = "hardcore on";

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
                        Phrases.Dict.TryGetValue("Hardcore5", out string _phrase);
                        _phrase = _phrase.Replace("{Value}", _lives.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                _p.Bounty = 0;
                _p.BountyHunter = 0;
                _p.ClanInvite = "";
                _p.ClanName = "";
                _p.ClanOfficer = false;
                _p.ClanOwner = false;
                _p.ClanRequestToJoin = new Dictionary<string, string>();
                _p.CountryBanImmune = false;
                _p.CustomCommandDelays = new Dictionary<string, DateTime>();
                _p.FirstClaimBlock = false;
                _p.HardcoreEnabled = false;
                _p.HardcoreStats = new string[0];
                _p.HighPingImmune = false;
                _p.HighPingImmuneName = "";
                _p.Homes = new Dictionary<string, string>();
                _p.JailDate = new DateTime();
                _p.JailName = "";
                _p.JailTime = 0;
                _p.LastAnimal = new DateTime();
                _p.LastDied = new DateTime();
                _p.LastFriendTele = new DateTime();
                _p.LastGimme = new DateTime();
                _p.LastHome = new DateTime();
                _p.LastJoined = new DateTime();
                _p.LastKillMe = new DateTime();
                _p.LastLobby = new DateTime();
                _p.LastLog = new DateTime();
                _p.LastMarket = new DateTime();
                _p.LastStuck = new DateTime();
                _p.LastTravel = new DateTime();
                _p.LastVote = new DateTime();
                _p.LastVoteWeek = new DateTime();
                _p.LastWhisper = "";
                _p.LobbyReturnPos = "";
                _p.MarketReturnPos = "";
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
                _p.Vehicles = new Dictionary<int, string[]>();
                _p.VoteWeekCount = 0;
                _p.WebPass = "";
                _p.ZoneDeathTime = new DateTime();
                PersistentContainer.DataChange = true;
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
                Phrases.Dict.TryGetValue("Hardcore1", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                Phrases.Dict.TryGetValue("Hardcore2", out _phrase);
                _phrase = _phrase.Replace("{Name1}", _sessionName1);
                _phrase = _phrase.Replace("{Session1}", _topSession1.ToString());
                _phrase = _phrase.Replace("{Name2}", _sessionName2);
                _phrase = _phrase.Replace("{Session2}", _topSession2.ToString());
                _phrase = _phrase.Replace("{Name3}", _sessionName3);
                _phrase = _phrase.Replace("{Session3}", _topSession3.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                Phrases.Dict.TryGetValue("Hardcore3", out _phrase);
                _phrase = _phrase.Replace("{Name1}", _ScoreName1);
                _phrase = _phrase.Replace("{Score1}", _topScore1.ToString());
                _phrase = _phrase.Replace("{Name2}", _ScoreName2);
                _phrase = _phrase.Replace("{Score2}", _topScore2.ToString());
                _phrase = _phrase.Replace("{Name3}", _ScoreName3);
                _phrase = _phrase.Replace("{Score3}", _topScore3.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Hardcore6", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                        Phrases.Dict.TryGetValue("Hardcore4", out string _phrase);
                        _phrase = _phrase.Replace("{PlayerName}", _stats[0]);
                        _phrase = _phrase.Replace("{PlayTime}", _stats[1]);
                        _phrase = _phrase.Replace("{PlayerKills}", _stats[2]);
                        _phrase = _phrase.Replace("{ZombieKills}", _stats[3]);
                        _phrase = _phrase.Replace("{Deaths}", _stats[4]);
                        _phrase = _phrase.Replace("{Score}", _stats[5]);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Hardcore6", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                int _cost = Life_Price * _extraLives++;
                                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= _cost)
                                {
                                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _cost);
                                    _stats[2] = (_extraLives + 1).ToString();
                                    PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreStats = _stats;
                                    PersistentContainer.DataChange = true;
                                    Phrases.Dict.TryGetValue("Hardcore7", out string _phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Hardcore8", out string _phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Hardcore9", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Hardcore10", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Hardcore9", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    p.Bounty = 0;
                    p.BountyHunter = 0;
                    p.ClanInvite = "";
                    p.ClanName = "";
                    p.ClanOfficer = false;
                    p.ClanOwner = false;
                    p.ClanRequestToJoin = new Dictionary<string, string>();
                    p.CountryBanImmune = false;
                    p.CustomCommandDelays = new Dictionary<string, DateTime>();
                    p.FirstClaimBlock = false;
                    p.HardcoreEnabled = false;
                    p.HardcoreSavedStats = new List<string[]>();
                    p.HardcoreStats = new string[0];
                    p.HighPingImmune = false;
                    p.HighPingImmuneName = "";
                    p.Homes = new Dictionary<string, string>();
                    p.JailDate = new DateTime();
                    p.JailName = "";
                    p.JailTime = 0;
                    p.LastAnimal = new DateTime();
                    p.LastDied = new DateTime();
                    p.LastFriendTele = new DateTime();
                    p.LastGimme = new DateTime();
                    p.LastHome = new DateTime();
                    p.LastJoined = new DateTime();
                    p.LastKillMe = new DateTime();
                    p.LastLobby = new DateTime();
                    p.LastLog = new DateTime();
                    p.LastMarket = new DateTime();
                    p.LastStuck = new DateTime();
                    p.LastTravel = new DateTime();
                    p.LastVote = new DateTime();
                    p.LastVoteWeek = new DateTime();
                    p.LastWhisper = "";
                    p.LobbyReturnPos = "";
                    p.MarketReturnPos = "";
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
                    p.Vehicles = new Dictionary<int, string[]>();
                    p.VoteWeekCount = 0;
                    p.Waypoints = new Dictionary<string, string>();
                    p.WebPass = "";
                    p.ZoneDeathTime = new DateTime();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.RemovePlayerData: {0}", e.Message));
            }
        }

        public static void Disconnect(ClientInfo _cInfo, string[] _stats)
        {
            try
            {
                if (_cInfo != null)
                {
                    Phrases.Dict.TryGetValue("Hardcore4", out string _phrase);
                    _phrase = _phrase.Replace("{PlayerName}", _stats[0]);
                    _phrase = _phrase.Replace("{PlayTime}", _stats[1]);
                    _phrase = _phrase.Replace("{PlayerKills}", _stats[2]);
                    _phrase = _phrase.Replace("{ZombieKills}", _stats[3]);
                    _phrase = _phrase.Replace("{Deaths}", _stats[4]);
                    _phrase = _phrase.Replace("{Score}", _stats[5]);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.KickPlayer: {0}", e.Message));
            }
        }
    }
}

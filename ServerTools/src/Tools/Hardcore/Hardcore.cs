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
            string[] stats = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreStats;
            if (int.TryParse(stats[1], out int deaths))
            {
                if (int.TryParse(stats[2], out int extraLives))
                {
                    int lives = Max_Deaths - (XUiM_Player.GetDeaths(_player) - deaths) + extraLives;
                    if (lives > 0)
                    {
                        Phrases.Dict.TryGetValue("Hardcore5", out string phrase);
                        phrase = phrase.Replace("{Value}", lives.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        EndGame(_cInfo, _player, stats);
                    }
                }
            }
        }

        public static void EndGame(ClientInfo _cInfo, EntityPlayer _player, string[] _stats)
        {
            PersistentPlayer p = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString];
            if (p != null)
            {
                PersistentOperations.Session.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime time);
                TimeSpan varTime = DateTime.Now - time;
                double fractionalMinutes = varTime.TotalMinutes;
                int timepassed = (int)fractionalMinutes;
                int oldSession = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].SessionTime;
                int newSession = oldSession + timepassed;
                int.TryParse(_stats[2], out int _extraLives);
                string[] _newStats = { _cInfo.playerName, newSession.ToString(), _player.KilledPlayers.ToString(), _player.KilledZombies.ToString(), Max_Deaths + _extraLives.ToString(), _player.Score.ToString() };
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats.Count > 0)
                {
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats.Add(_newStats);
                }
                else
                {
                    List<string[]> SavedStats = new List<string[]>();
                    SavedStats.Add(_newStats);
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats = SavedStats;
                }
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
                p.WebPass = "";
                p.ZoneDeathTime = new DateTime();
                PersistentContainer.DataChange = true;
                Hardcore.Disconnect(_cInfo, _newStats);
            }
        }

        public static void TopThree(ClientInfo _cInfo)
        {
            int _topSession1 = 0, _topSession2 = 0, _topSession3 = 0, _topScore1 = 0, _topScore2 = 0, _topScore3 = 0;
            string _sessionName1 = "", _sessionName2 = "", _sessionName3 = "", _ScoreName1 = "", _ScoreName2 = "", _ScoreName3 = "";
            List<string> _persistentPlayers = PersistentContainer.Instance.Players.IDs;
            for (int i = 0; i < _persistentPlayers.Count; i++)
            {
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats.Count > 0)
                {
                    List<string[]> _hardcoreSavedStats = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats;
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
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats.Count > 0)
                {
                    List<string[]> _hardcoreSavedStats = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats;
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
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (Max_Extra_Lives > 0)
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreEnabled)
                        {
                            string[] stats = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreStats;
                            int.TryParse(stats[2], out int extraLives);
                            if (extraLives < Max_Extra_Lives)
                            {
                                if (Life_Price < 0)
                                {
                                    Life_Price = 0;
                                }
                                int cost = Life_Price * extraLives++;
                                if (Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString) >= cost)
                                {
                                    Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                                    stats[2] = (extraLives + 1).ToString();
                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreStats = stats;
                                    PersistentContainer.DataChange = true;
                                    Phrases.Dict.TryGetValue("Hardcore7", out string phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Hardcore8", out string phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Hardcore9", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                string filepath1 = string.Format("{0}/Player/{1}.map", GameIO.GetSaveGameDir(), _cInfo.CrossplatformId.CombinedString);
                string filepath2 = string.Format("{0}/Player/{1}.ttp", GameIO.GetSaveGameDir(), _cInfo.CrossplatformId.CombinedString);
                if (!File.Exists(filepath1))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Could not find file {0}.map", _cInfo.CrossplatformId.CombinedString));
                }
                else
                {
                    File.Delete(filepath1);
                }
                if (!File.Exists(filepath2))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Could not find file {0}.ttp", _cInfo.CrossplatformId.CombinedString));
                }
                else
                {
                    File.Delete(filepath2);
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
                PersistentPlayer p = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString];
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
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"Auto kicked at end of hardcore session\"", _cInfo.CrossplatformId.CombinedString), null);
                EntityPlayer entityPlayer = (EntityPlayer)GameManager.Instance.World.GetEntity(_cInfo.entityId);
                PersistentOperations.SavePersistentPlayerDataXML();
                PersistentOperations.RemoveAllClaims(_cInfo.CrossplatformId.CombinedString);
                PersistentOperations.RemovePersistentPlayerData(_cInfo.CrossplatformId.CombinedString);
                PersistentOperations.RemoveAllACL(_cInfo.CrossplatformId.CombinedString);
                Hardcore.ResetHardcoreProfile(_cInfo);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.KickPlayer: {0}", e.Message));
            }
        }
    }
}

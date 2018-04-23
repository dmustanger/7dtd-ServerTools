using System;
using System.IO;

namespace ServerTools
{
    class Hardcore
    {
        public static bool IsEnabled = false;
        public static int Max_Deaths = 9;

        public static void Announce(ClientInfo _cInfo)
        {
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, hardcore mode is enabled! You have {2} lives remaining...[-]", Config.Chat_Response_Color, _cInfo.playerName, Max_Deaths), Config.Server_Response_Name, false, "ServerTools", false));
        }

        public static void Check(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player != null)
            {
                if (_player.IsAlive())
                {
                    int _deaths = XUiM_Player.GetDeaths(_player);
                    if (_deaths < Max_Deaths)
                    {
                        int _lives = Max_Deaths - _deaths;
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Hardcore: Zombie Kills {1}, Player Kills {2}, Score {3}, Lives remaining {4}...[-]", Config.Chat_Response_Color, _player.KilledZombies, _player.KilledPlayers, _player.Score, _lives), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        EndGame(_cInfo, _player);
                    }
                }
            }
        }

        public static void EndGame(ClientInfo _cInfo, EntityPlayer _player)
        {
            DateTime _time;
            Players.Session.TryGetValue(_cInfo.playerId, out _time);
            TimeSpan varTime = DateTime.Now - _time;
            double fractionalMinutes = varTime.TotalMinutes;
            int _timepassed = (int)fractionalMinutes;
            int _deaths = XUiM_Player.GetDeaths(_player);
            int _oldSession = PersistentContainer.Instance.Players[_cInfo.playerId, true].SessionTime;
            int _newSession = _oldSession + _timepassed;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].HardcoreSessionTime = _newSession;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].HardcoreZKills = _player.KilledZombies;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].HardcoreDeaths = _deaths;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].HardcoreKills = _player.KilledPlayers;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].HardcoreScore = _player.Score;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].HardcoreName = _cInfo.playerName;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].BikeId = 0;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = 0;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].SessionTime = 0;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionData = 0;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].StartingItems = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].FirstClaim = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].Poll = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOwner = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOfficer = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsMuted = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsJailed = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastStuck = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastLobby = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastStuck = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CancelTime = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].SellDate = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].Log = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].JailDate = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].MuteDate = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastBike = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastBackpack = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastStuck = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastFriendTele = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand1 = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand2 = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand3 = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand4 = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand5 = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand6 = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand7 = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand8 = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand9 = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand10 = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].RespawnTime = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastTravel = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastAnimals = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastVoteReward = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastGimme = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastKillme = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastSetHome = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].RespawnTime = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LobbyReturn = null;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].NewTeleSpawn = null;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].JailName = null;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].MuteName = null;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition = null;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition2 = null;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].ClanName = null;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].InvitedToClan = null;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastWhisper = null;
            PersistentContainer.Instance.Save();
            if (ClanManager.ClanMember.Contains(_cInfo.playerId))
            {
                ClanManager.ClanMember.Remove(_cInfo.playerId);
            }
            string _session = string.Format("0:00", _newSession % 60);
            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Hardcore Game Over: Zombie Kills {1}, Player Kills {2}, Deaths {3}, Score {4}, Playtime {5}\"", _cInfo.playerId, _player.KilledZombies, _player.KilledPlayers, _deaths, _player.Score, _newSession), (ClientInfo)null);
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null)
            {
                string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                if (File.Exists(_filepath))
                {
                    File.Delete(_filepath);
                }
                if (File.Exists(_filepath1))
                {
                    File.Delete(_filepath1);
                }
            }
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}Hardcore Game Over: Player {1}, Zombie Kills {2}, Player Kills {3}, Deaths {4}, Score {5}, Playtime {6}[-]", Config.Chat_Response_Color, _cInfo.playerName, _player.KilledZombies, _player.KilledPlayers, _deaths, _player.Score, _session), Config.Server_Response_Name, false, "ServerTools", false);
        }

        public static void TopThree(ClientInfo _cInfo, bool _announce)
        {
            int _sessionTime, _score, _topSession1 = 0, _topSession2 = 0, _topSession3 = 0, _topScore1 = 0, _topScore2 = 0, _topScore3 = 0;
            string _sessionName1 = "", _sessionName2 = "", _sessionName3 = "", _ScoreName1 = "", _ScoreName2 = "", _ScoreName3 = "";
            for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
            {
                string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                Player p = PersistentContainer.Instance.Players[_id, false];
                {
                    if (p != null)
                    {
                        _sessionTime = p.HardcoreSessionTime;
                        if (_sessionTime > _topSession1)
                        {
                            _topSession3 = _topSession2;
                            _sessionName3 = _sessionName2;
                            _topSession2 = _topSession1;
                            _sessionName2 = _sessionName1;
                            _topSession1 = _sessionTime;
                            _sessionName1 = p.HardcoreName;
                        }
                        else
                        {
                            if (_sessionTime > _topSession2)
                            {
                                _topSession3 = _topSession2;
                                _sessionName3 = _sessionName2;
                                _topSession2 = _sessionTime;
                                _sessionName2 = p.HardcoreName;
                            }
                            else
                            {
                                if (_sessionTime > _topSession3)
                                {
                                    _topSession3 = _sessionTime;
                                    _sessionName3 = p.HardcoreName;
                                }
                            }
                        }
                        _score = p.HardcoreScore;
                        if (_score > _topScore1)
                        {
                            _topScore3 = _topScore2;
                            _ScoreName3 = _ScoreName2;
                            _topScore2 = _topScore1;
                            _ScoreName2 = _ScoreName1;
                            _topScore1 = _score;
                            _ScoreName1 = p.HardcoreName;
                        }
                        else
                        {
                            if (_score > _topScore2)
                            {
                                _topScore3 = _topScore2;
                                _ScoreName3 = _ScoreName2;
                                _topScore2 = _score;
                                _ScoreName2 = p.HardcoreName;
                            }
                            else
                            {
                                if (_score > _topScore3)
                                {
                                    _topScore3 = _score;
                                    _ScoreName3 = p.HardcoreName;
                                }
                            }
                        }
                    }
                }
            }
            if (_announce)
            {
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}Hardcore Top Players[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false);
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}Playtime #1 {1}, {2} Playtime #2 {3}, {4} Playtime #3 {5}, {6}[-]", Config.Chat_Response_Color, _sessionName1, _topSession1, _sessionName2, _topSession2, _sessionName3, _topSession3), Config.Server_Response_Name, false, "ServerTools", false);
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}Score #1 {1}, {2} Score #2 {3}, {4} Score #3 {5}, {6}[-]", Config.Chat_Response_Color, _ScoreName1, _topScore1, _ScoreName2, _topScore2, _ScoreName3, _topScore3), Config.Server_Response_Name, false, "ServerTools", false);
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Hardcore Top Players[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Playtime #1 {1}, {2} Playtime #2 {3}, {4} Playtime #3 {5}, {6}[-]", Config.Chat_Response_Color, _sessionName1, _topSession1, _sessionName2, _topSession2, _sessionName3, _topSession3), Config.Server_Response_Name, false, "ServerTools", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Score #1 {1}, {2} Score #2 {3}, {4} Score #3 {5}, {6}[-]", Config.Chat_Response_Color, _ScoreName1, _topScore1, _ScoreName2, _topScore2, _ScoreName3, _topScore3), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Score(ClientInfo _cInfo, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            {
                if (p != null)
                {
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} your last hardcore stats: Name {2} Zombie Kills {3}, Player Kills {4}, Deaths {5}, Score {6}, Playtime {7}[-]", Config.Chat_Response_Color, _cInfo.playerName, p.HardcoreName, p.HardcoreZKills, p.HardcoreKills, p.HardcoreDeaths, p.HardcoreScore, p.HardcoreSessionTime), Config.Server_Response_Name, false, "ServerTools", false);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your last hardcore stats: Name {2} Zombie Kills {3}, Player Kills {4}, Deaths {5}, Score {6}, Playtime {7}[-]", Config.Chat_Response_Color, _cInfo.playerName, p.HardcoreName, p.HardcoreZKills, p.HardcoreKills, p.HardcoreDeaths, p.HardcoreScore, p.HardcoreSessionTime), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
        }
    }
}

using System;
using System.Data;
using System.IO;

namespace ServerTools
{
    class Hardcore
    {
        public static bool IsEnabled = false;
        public static int Max_Deaths = 9;

        public static void Announce(ClientInfo _cInfo)
        {
            string _phrase949;
            if (!Phrases.Dict.TryGetValue(949, out _phrase949))
            {
                _phrase949 = "{PlayerName}, hardcore mode is enabled! You have {Lives} lives remaining...";
            }
            _phrase949 = _phrase949.Replace("{PlayerName}", _cInfo.playerName);
            _phrase949 = _phrase949.Replace("{Lives}", Max_Deaths.ToString());
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase949), Config.Server_Response_Name, false, "ServerTools", false));
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
                        string _phrase950;
                        if (!Phrases.Dict.TryGetValue(950, out _phrase950))
                        {
                            _phrase950 = "Hardcore: Zombie Kills {ZombieKills}, Player Kills {PlayerKills}, Score {Score}, Lives remaining {Lives}...";
                        }
                        _phrase950 = _phrase950.Replace("{ZombieKills}", _player.KilledZombies.ToString());
                        _phrase950 = _phrase950.Replace("{PlayerKills}", _player.KilledPlayers.ToString());
                        _phrase950 = _phrase950.Replace("{Score}", _player.Score.ToString());
                        _phrase950 = _phrase950.Replace("{Lives}", _lives.ToString());
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase950), Config.Server_Response_Name, false, "ServerTools", false));
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
            string _sql = string.Format("SELECT sessionTime FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            int _oldSession;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _oldSession);
            _result.Dispose();
            int _newSession = _oldSession + _timepassed;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionData = 0;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].StartingItems = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].FirstClaim = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOwner = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOfficer = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsMuted = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsJailed = false;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].CancelTime = DateTime.Now.AddDays(-5);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].SellDate = DateTime.Now.AddDays(-5);
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
            PersistentContainer.Instance.Players[_cInfo.playerId, true].ClanName = null;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].InvitedToClan = null;
            PersistentContainer.Instance.Save();
            _sql = string.Format("SELECT last_gimme FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result1 = SQL.TQuery(_sql);
            if (_result1.Rows.Count != 0)
            {
                string _name = SQL.EscapeString(_cInfo.playerName);
                _sql = string.Format("UPDATE Players SET " +
                    "playername = 'Unknown', " +
                    "last_gimme = '10/29/2000 7:30:00 AM', " +
                    "lastkillme = '10/29/2000 7:30:00 AM', " +
                    "playerSpentCoins = 0, " +
                    "hardcoreSessionTime = {0}, " +
                    "hardcoreKills = {1}, " +
                    "hardcoreZKills = {2}, " +
                    "hardcoreScore = {3}, " +
                    "hardcoreDeaths = {4}, " +
                    "hardcoreName = '{5}', " +
                    "sessionTime = 0, " +
                    "bikeId = 0, " +
                    "lastBike = '10/29/2000 7:30:00 AM', " +
                    "jailName = 'Unknown', " +
                    "jailDate = '10/29/2000 7:30:00 AM', " +
                    "muteName = 'Unknown', " +
                    "muteDate = '10/29/2000 7:30:00 AM', " +
                    "lobbyReturn = 'Unknown', " +
                    "newTeleSpawn = 'Unknown', " +
                    "homeposition = 'Unknown', " +
                    "homeposition2 = 'Unknown', " +
                    "lastsethome = '10/29/2000 7:30:00 AM', " +
                    "lastwhisper = 'Unknown', " +
                    "lastStuck = '10/29/2000 7:30:00 AM', " +
                    "lastLobby = '10/29/2000 7:30:00 AM', " +
                    "lastLog = '10/29/2000 7:30:00 AM', " +
                    "lastDied = '10/29/2000 7:30:00 AM', " +
                    "lastFriendTele = '10/29/2000 7:30:00 AM', " +
                    "respawnTime = '10/29/2000 7:30:00 AM', " +
                    "lastTravel = '10/29/2000 7:30:00 AM', " +
                    "lastAnimals = '10/29/2000 7:30:00 AM', " +
                    "lastVoteReward = '10/29/2000 7:30:00 AM' " +
                    "WHERE steamid = '{6}'", _newSession, _player.KilledPlayers, _player.KilledZombies, _player.Score, _deaths, _name, _cInfo.playerId);
                SQL.FastQuery(_sql);
            }
            _result1.Dispose();
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
            string _phrase951;
            if (!Phrases.Dict.TryGetValue(951, out _phrase951))
            {
                _phrase951 = "Hardcore Game Over: Player {PlayerName}, Zombie Kills {ZombieKills}, Player Kills {PlayerKills}, Deaths {Deaths}, Score {Score}, Playtime {Playtime}";
            }
            _phrase951 = _phrase951.Replace("{PlayerName}", _cInfo.playerName);
            _phrase951 = _phrase951.Replace("{ZombieKills}", _player.KilledZombies.ToString());
            _phrase951 = _phrase951.Replace("{PlayerKills}", _player.KilledPlayers.ToString());
            _phrase951 = _phrase951.Replace("{Deaths}", _deaths.ToString());
            _phrase951 = _phrase951.Replace("{Score}", _player.Score.ToString());
            _phrase951 = _phrase951.Replace("{Playtime}", _session);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase951), Config.Server_Response_Name, false, "ServerTools", false);
        }

        public static void TopThree(ClientInfo _cInfo, bool _announce)
        {
            int _sessionTime, _score, _topSession1 = 0, _topSession2 = 0, _topSession3 = 0, _topScore1 = 0, _topScore2 = 0, _topScore3 = 0;
            string _sessionName1 = "", _sessionName2 = "", _sessionName3 = "", _ScoreName1 = "", _ScoreName2 = "", _ScoreName3 = "";
            string _sql = "SELECT hardcoreSessionTime, hardcoreScore, hardcoreName From Players";
            DataTable _result = SQL.TQuery(_sql);
            foreach (DataRow row in _result.Rows)
            {
                int.TryParse(row[0].ToString(), out _sessionTime);
                if (_sessionTime > _topSession1)
                {
                    _topSession3 = _topSession2;
                    _sessionName3 = _sessionName2;
                    _topSession2 = _topSession1;
                    _sessionName2 = _sessionName1;
                    _topSession1 = _sessionTime;
                    _sessionName1 = row[2].ToString();
                }
                else
                {
                    if (_sessionTime > _topSession2)
                    {
                        _topSession3 = _topSession2;
                        _sessionName3 = _sessionName2;
                        _topSession2 = _sessionTime;
                        _sessionName2 = row[2].ToString();
                    }
                    else
                    {
                        if (_sessionTime > _topSession3)
                        {
                            _topSession3 = _sessionTime;
                            _sessionName3 = row[2].ToString();
                        }
                    }
                }
                int.TryParse(row[1].ToString(), out _score);
                if (_score > _topScore1)
                {
                    _topScore3 = _topScore2;
                    _ScoreName3 = _ScoreName2;
                    _topScore2 = _topScore1;
                    _ScoreName2 = _ScoreName1;
                    _topScore1 = _score;
                    _ScoreName1 = row[2].ToString();
                }
                else
                {
                    if (_score > _topScore2)
                    {
                        _topScore3 = _topScore2;
                        _ScoreName3 = _ScoreName2;
                        _topScore2 = _score;
                        _ScoreName2 = row[2].ToString();
                    }
                    else
                    {
                        if (_score > _topScore3)
                        {
                            _topScore3 = _score;
                            _ScoreName3 = row[2].ToString();
                        }
                    }
                }
            }
            _result.Dispose();
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
            if (_announce)
            {
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase945), Config.Server_Response_Name, false, "ServerTools", false);
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase946), Config.Server_Response_Name, false, "ServerTools", false);
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase947), Config.Server_Response_Name, false, "ServerTools", false);
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase945), Config.Server_Response_Name, false, "ServerTools", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase946), Config.Server_Response_Name, false, "ServerTools", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase947), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Score(ClientInfo _cInfo, bool _announce)
        {
            string _sql = string.Format("SELECT hardcoreSessionTime, hardcoreKills, hardcoreZKills, hardcoreScore, hardcoreDeaths, hardcoreName From Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _phrase948;
            if (!Phrases.Dict.TryGetValue(948, out _phrase948))
            {
                _phrase948 = "{PlayerName} your last hardcore stats: Name {LastName} Zombie Kills {ZombieKills}, Player Kills {PlayerKills}, Deaths {Deaths}, Score {Score}, Playtime {Playtime} Minutes";
            }
            _phrase948 = _phrase948.Replace("{PlayerName}", _cInfo.playerName);
            _phrase948 = _phrase948.Replace("{LastName}", _result.Rows[0].ItemArray.GetValue(5).ToString());
            _phrase948 = _phrase948.Replace("{ZombieKills}", _result.Rows[0].ItemArray.GetValue(2).ToString());
            _phrase948 = _phrase948.Replace("{PlayerKills}", _result.Rows[0].ItemArray.GetValue(1).ToString());
            _phrase948 = _phrase948.Replace("{Deaths}", _result.Rows[0].ItemArray.GetValue(4).ToString());
            _phrase948 = _phrase948.Replace("{Score}", _result.Rows[0].ItemArray.GetValue(3).ToString());
            _phrase948 = _phrase948.Replace("{Playtime}", _result.Rows[0].ItemArray.GetValue(0).ToString());
            _result.Dispose();
            if (_announce)
            {
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase948), Config.Server_Response_Name, false, "ServerTools", false);
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase948), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}

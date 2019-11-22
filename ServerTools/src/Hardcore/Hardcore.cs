using System;
using System.Data;

namespace ServerTools
{
    class Hardcore
    {
        public static bool IsEnabled = false, Optional = false;
        public static int Max_Deaths = 9, Max_Extra_Lives = 3, Life_Price = 2000;
        public static string Command11 = "top3", Command12 = "score", Command101 = "buy life", Command127 = "hardcore", Command128 = "hardcore on";

        public static void Alert(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player != null)
            {
                string _sql = string.Format("SELECT deaths, extraLives FROM Hardcore WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                if (_result.Rows.Count > 0)
                {
                    int _deaths, _extraLives;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _deaths);
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _extraLives);
                    if (_deaths - _extraLives < Max_Deaths)
                    {
                        int _lives = Max_Deaths - _deaths + _extraLives;
                        string _phrase949;
                        if (!Phrases.Dict.TryGetValue(949, out _phrase949))
                        {
                            _phrase949 = "Hardcore mode is enabled! You have {Lives} lives remaining...";
                        }
                        _phrase949 = _phrase949.Replace("{Lives}", _lives.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase949 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
        }

        public static void Check(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player != null)
            {
                string _sql = string.Format("SELECT deaths, extraLives FROM Hardcore WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                if (_result.Rows.Count > 0)
                {
                    int _deaths, _extraLives;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _deaths);
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _extraLives);
                    if (_deaths + 1 - _extraLives < Max_Deaths)
                    {
                        _sql = string.Format("UPDATE Hardcore SET deaths = {0} WHERE steamid = '{1}'", _deaths + 1, _cInfo.playerId);
                        SQL.FastQuery(_sql, "Hardcore");
                        int _lives = Max_Deaths - _deaths - 1 + _extraLives;
                        string _phrase950;
                        if (!Phrases.Dict.TryGetValue(950, out _phrase950))
                        {
                            _phrase950 = "Hardcore: Zombie Kills {ZombieKills}, Player Kills {PlayerKills}, Score {Score}, Lives remaining {Lives}...";
                        }
                        _phrase950 = _phrase950.Replace("{ZombieKills}", _player.KilledZombies.ToString());
                        _phrase950 = _phrase950.Replace("{PlayerKills}", _player.KilledPlayers.ToString());
                        _phrase950 = _phrase950.Replace("{Score}", _player.Score.ToString());
                        _phrase950 = _phrase950.Replace("{Lives}", _lives.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase950 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        _sql = string.Format("UPDATE Hardcore SET deaths = {0} WHERE steamid = '{1}'", _deaths + 1, _cInfo.playerId);
                        SQL.FastQuery(_sql, "Hardcore");
                        EndGame(_cInfo, _player);
                    }
                }
                _result.Dispose();
            }
        }

        public static void EndGame(ClientInfo _cInfo, EntityPlayer _player)
        {
            string _sql = string.Format("SELECT deaths FROM Hardcore WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                int _deaths;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _deaths);
                DateTime _time;
                PlayerOperations.Session.TryGetValue(_cInfo.playerId, out _time);
                TimeSpan varTime = DateTime.Now - _time;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                int _oldSession = PersistentContainer.Instance.Players[_cInfo.playerId].SessionTime;
                int _newSession = _oldSession + _timepassed;
                int _playerKills = _player.KilledPlayers, _zKills = _player.KilledZombies, _score = _player.Score;
                string _session = string.Format("{0:00}", _newSession % 60);
                _sql = string.Format("UPDATE Hardcore SET sessionTime = {0}, kills = {1}, zKills = {2}, score = {3}, deaths = {4}, playerName = {5}, extraLives = {6} WHERE steamid = '{7}'",
                    _newSession, _playerKills, _zKills, _score, _deaths, _cInfo.playerName, 0, _cInfo.playerId);
                SQL.FastQuery(_sql, "Hardcore");
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Hardcore Game Over: Zombie Kills {1}, Player Kills {2}, Deaths {3}, Score {4}, Playtime {5}\"",
                    _cInfo.playerId, _zKills, _playerKills, _deaths, _score, _newSession), (ClientInfo)null);
                SdtdConsole.Instance.ExecuteSync(string.Format("resetplayerprofile {0}", _cInfo.playerId), (ClientInfo)null);
                string _phrase951;
                if (!Phrases.Dict.TryGetValue(951, out _phrase951))
                {
                    _phrase951 = "Hardcore Game Over: Player {PlayerName}, Zombie Kills {ZombieKills}, Player Kills {PlayerKills}, Deaths {Deaths}, Score {Score}, Playtime {Playtime}";
                }
                _phrase951 = _phrase951.Replace("{PlayerName}", _cInfo.playerName);
                _phrase951 = _phrase951.Replace("{ZombieKills}", _zKills.ToString());
                _phrase951 = _phrase951.Replace("{PlayerKills}", _playerKills.ToString());
                _phrase951 = _phrase951.Replace("{Deaths}", _deaths.ToString());
                _phrase951 = _phrase951.Replace("{Score}", _score.ToString());
                _phrase951 = _phrase951.Replace("{Playtime}", _session);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase951 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
            _result.Dispose();
        }

        public static void TopThree(ClientInfo _cInfo, bool _announce)
        {
            int _sessionTime, _score, _topSession1 = 0, _topSession2 = 0, _topSession3 = 0, _topScore1 = 0, _topScore2 = 0, _topScore3 = 0;
            string _sessionName1 = "", _sessionName2 = "", _sessionName3 = "", _ScoreName1 = "", _ScoreName2 = "", _ScoreName3 = "";
            string _sql = "SELECT sessionTime, score, playerName From Hardcore";
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
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
                    else if (_sessionTime > _topSession2)
                    {
                        _topSession3 = _topSession2;
                        _sessionName3 = _sessionName2;
                        _topSession2 = _sessionTime;
                        _sessionName2 = row[2].ToString();
                    }
                    else if (_sessionTime > _topSession3)
                    {
                        _topSession3 = _sessionTime;
                        _sessionName3 = row[2].ToString();
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
                    else if (_score > _topScore2)
                    {
                        _topScore3 = _topScore2;
                        _ScoreName3 = _ScoreName2;
                        _topScore2 = _score;
                        _ScoreName2 = row[2].ToString();
                    }
                    else if (_score > _topScore3)
                    {
                        _topScore3 = _score;
                        _ScoreName3 = row[2].ToString();
                    }
                }
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
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase945 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase946 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase947 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase945 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase946 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase947 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "There are no hardcore records" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
            _result.Dispose();
            
        }

        public static void Score(ClientInfo _cInfo, bool _announce)
        {
            string _sql = string.Format("SELECT sessionTime, kills, zKills, score, deaths, playerName From Hardcore WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
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
                if (_announce)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase948 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase948 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have no hardcore records." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            _result.Dispose();
        }

        public static void BuyLife(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player != null)
            {
                int _deaths = XUiM_Player.GetDeaths(_player);
                string _sql = string.Format("SELECT extraLives FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                if (_result.Rows.Count > 0)
                {
                    int _extraLives;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _extraLives);
                    _result.Dispose();
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
                            _sql = string.Format("UPDATE Players SET extraLives = {0} WHERE steamid = '{1}'", _extraLives + 1, _cInfo.playerId);
                            SQL.FastQuery(_sql, "Hardcore");
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have bought one extra life." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you need a total of " + _cost.ToString() + " " + Wallet.Coin_Name + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are at the maximum extra lives." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
        }
    }
}

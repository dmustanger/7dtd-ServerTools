using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    [Serializable]
    public class Players
    {
        public static int Bonus = 0;
        public Dictionary<string, Player> players = new Dictionary<string, Player>();
        public Dictionary<string, string> clans = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static Dictionary<int, DateTime> DeathTime = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> LastDeathPos = new Dictionary<int, string>();
        public static Dictionary<int, int> KillStreak = new Dictionary<int, int>();
        public static Dictionary<int, string> Victim = new Dictionary<int, string>();
        public static Dictionary<int, int> Forgive = new Dictionary<int, int>();
        public static Dictionary<int, string> ZoneExit = new Dictionary<int, string>();
        public static List<int> ZonePvE = new List<int>();
        public static List<int> Dead = new List<int>();
        public static List<int> NoFlight = new List<int>();
        private static Dictionary<int, string> Friends = new Dictionary<int, string>();
        private static string file = string.Format("Bounty_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Bounties/{1}", API.GamePath, file);
        private static int _counter = 0;
        private static bool DP = false;

        public static void CreateFolder()
        {
            if (!Directory.Exists(API.GamePath + "/Bounties"))
            {
                Directory.CreateDirectory(API.GamePath + "/Bounties");
            }
        }

        public List<string> SteamIDs
        {
            get
            {
                return new List<string>(players.Keys);
            }
        }

        public Player this[string steamId, bool create]
        {
            get
            {
                if (string.IsNullOrEmpty(steamId))
                {
                    return null;
                }
                else if (players.ContainsKey(steamId))
                {
                    return players[steamId];
                }
                else
                {
                    if (create && steamId != null && steamId.Length == 17)
                    {
                        Player p = new Player(steamId);
                        players.Add(steamId, p);
                        return p;
                    }
                    return null;
                }
            }
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            Session.Add(_cInfo.playerId, DateTime.Now);
        }

        public static void FriendList(ClientInfo _cInfo)
        {
            if (!Friends.ContainsKey(_cInfo.entityId) && GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
            {
                EntityPlayer ent1 = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                string _friends = "";
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = _cInfoList[i];
                    if (_cInfo2 != null)
                    {
                        EntityPlayer ent2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                        if (ent1.IsFriendsWith(ent2))
                        {
                            _friends = string.Format("{0} {1}", _friends, _cInfo2.playerId);
                        }
                    }
                }
                if (_friends != "")
                {
                    Friends.Add(_cInfo.entityId, _friends);
                }
            }
        }

        public static void Exec()
        {
            DP = false;
            List<EntityPlayer> _playerList = GameManager.Instance.World.Players.list;
            for (int i = 0; i < _playerList.Count; i++)
            {
                EntityPlayer _player = _playerList[i];
                if (_player != null)
                {
                    if (_player.IsDead())
                    {
                        DP = true;
                        if (!Dead.Contains(_player.entityId))
                        {
                            Dead.Add(_player.entityId);
                            if (!DeathTime.ContainsKey(_player.entityId))
                            {
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _dposition = x + "," + y + "," + z;
                                DeathTime.Add(_player.entityId, DateTime.Now);
                                LastDeathPos.Add(_player.entityId, _dposition);
                            }
                            else
                            {
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _dposition = x + "," + y + "," + z;
                                DeathTime[_player.entityId] = DateTime.Now;
                                LastDeathPos[_player.entityId] = _dposition;
                            }
                            for (int j = 0; j < _playerList.Count; j++)
                            {
                                EntityPlayer _player2 = _playerList[j];
                                Entity _target = _player2.GetDamagedTarget();
                                if (_target == _player && _player != _player2)
                                {
                                    _player2.ClearDamagedTarget();
                                    ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_player.entityId);
                                    ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForEntityId(_player2.entityId);
                                    if (_cInfo != null && _cInfo2 != null)
                                    {
                                        if (KillNotice.IsEnabled)
                                        {
                                            string _holdingItem = _player2.inventory.holdingItem.Name;
                                            ItemValue _itemValue = ItemClass.GetItem(_holdingItem, true);
                                            if (_itemValue.type != ItemValue.None.type)
                                            {
                                                _holdingItem = _itemValue.ItemClass.localizedName ?? _itemValue.ItemClass.Name;
                                            }
                                            KillNotice.Notice(_cInfo, _cInfo2, _holdingItem);
                                        }
                                        if (Bounties.IsEnabled)
                                        {
                                            if (!_player.IsFriendsWith(_player2) || !_player2.IsFriendsWith(_player))
                                            {
                                                if (Friends.ContainsKey(_player.entityId))
                                                {
                                                    string _friends;
                                                    if (Friends.TryGetValue(_player.entityId, out _friends))
                                                    {
                                                        string[] _friendList = _friends.Split(' ').ToArray();
                                                        for (int k = 0; k < _friendList.Length; k++)
                                                        {
                                                            string _friend = _friendList[k];
                                                            if (_friend == _cInfo2.playerId)
                                                            {
                                                                return;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (Friends.ContainsKey(_player2.entityId))
                                                {
                                                    string _friends;
                                                    if (Friends.TryGetValue(_player2.entityId, out _friends))
                                                    {
                                                        string[] _friendList = _friends.Split(' ').ToArray();
                                                        for (int k = 0; k < _friendList.Length; k++)
                                                        {
                                                            string _friend = _friendList[k];
                                                            if (_friend == _cInfo.playerId)
                                                            {
                                                                return;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (ClanManager.IsEnabled)
                                                {
                                                    if (ClanManager.ClanMember.Contains(_cInfo.playerId) && ClanManager.ClanMember.Contains(_cInfo2.playerId))
                                                    {
                                                        string _sql1 = string.Format("SELECT clanname FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                                        DataTable _result1 = SQL.TQuery(_sql1);
                                                        string _clanName = _result1.Rows[0].ItemArray.GetValue(0).ToString();
                                                        _result1.Dispose();
                                                        _sql1 = string.Format("SELECT clanname FROM Players WHERE steamid = '{0}'", _cInfo2.playerId);
                                                        DataTable _result2 = SQL.TQuery(_sql1);
                                                        string _clanName2 = _result2.Rows[0].ItemArray.GetValue(0).ToString();
                                                        _result2.Dispose();
                                                        Player p2 = PersistentContainer.Instance.Players[_cInfo2.playerId, false];
                                                        if (_clanName != "Unknown" && _clanName2 != "Unknown")
                                                        {
                                                            if (_clanName == _clanName2)
                                                            {
                                                                return;
                                                            }
                                                        }
                                                    }
                                                }
                                                string _sql = string.Format("SELECT bounty, bountyHunter FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                                DataTable _result = SQL.TQuery(_sql);
                                                int _bounty;
                                                int _hunterCountVictim;
                                                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bounty);
                                                int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _hunterCountVictim);
                                                _result.Dispose();
                                                if (_bounty > 0)
                                                {
                                                    _sql = string.Format("SELECT playerSpentCoins, bountyHunter FROM Players WHERE steamid = '{0}'", _cInfo2.playerId);
                                                    DataTable _result2 = SQL.TQuery(_sql);
                                                    int _playerSpentCoins;
                                                    int _hunterCountKiller;
                                                    int.TryParse(_result2.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
                                                    int.TryParse(_result2.Rows[0].ItemArray.GetValue(1).ToString(), out _hunterCountKiller);
                                                    _result2.Dispose();
                                                    if (Bonus > 0 && _hunterCountVictim >= Bonus)
                                                    {
                                                        _sql = string.Format("UPDATE Players SET playerSpentCoins = {0}, bountyHunter = {1} WHERE steamid = '{2}'", _playerSpentCoins + _bounty + Bonus, _hunterCountKiller + 1, _cInfo2.playerId);
                                                        SQL.FastQuery(_sql);
                                                    }
                                                    else
                                                    {
                                                        _sql = string.Format("UPDATE Players SET playerSpentCoins = {0}, bountyHunter = {1} WHERE steamid = '{2}'", _playerSpentCoins + _bounty, _hunterCountKiller + 1, _cInfo2.playerId);
                                                        SQL.FastQuery(_sql);
                                                    }
                                                    _sql = string.Format("UPDATE Players SET bounty = 0, bountyHunter = 0 WHERE steamid = '{0}'", _cInfo.playerId);
                                                    SQL.FastQuery(_sql);
                                                    string _phrase912;
                                                    if (!Phrases.Dict.TryGetValue(912, out _phrase912))
                                                    {
                                                        _phrase912 = "{PlayerName} is a bounty hunter! {Victim} was snuffed out.";
                                                    }
                                                    _phrase912 = _phrase912.Replace("{PlayerName}", _cInfo2.playerName);
                                                    _phrase912 = _phrase912.Replace("{Victim}", _cInfo.playerName);
                                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase912), Config.Server_Response_Name, false, "ServerTools", false);
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} is a bounty hunter! {2} was snuffed out. Bounty was worth {3}", DateTime.Now, _cInfo2.playerName, _cInfo.playerName, _bounty));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                                if (Bounties.Kill_Streak > 0)
                                                {
                                                    if (KillStreak.ContainsKey(_cInfo.entityId))
                                                    {
                                                        KillStreak.Remove(_cInfo.entityId);
                                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                                        {
                                                            sw.WriteLine(string.Format("{0}: Player {1} kill streak has come to an end by {2}.", DateTime.Now, _cInfo.playerName, _cInfo2.playerName));
                                                            sw.WriteLine();
                                                            sw.Flush();
                                                            sw.Close();
                                                        }
                                                    }
                                                    if (KillStreak.ContainsKey(_cInfo2.entityId))
                                                    {
                                                        int _value;
                                                        if (KillStreak.TryGetValue(_cInfo2.entityId, out _value))
                                                        {
                                                            int _newValue = _value + 1;
                                                            KillStreak[_cInfo2.entityId] = _newValue;
                                                            if (_newValue == Bounties.Kill_Streak)
                                                            {
                                                                string _phrase913;
                                                                if (!Phrases.Dict.TryGetValue(913, out _phrase913))
                                                                {
                                                                    _phrase913 = "{PlayerName} is on a kill streak! Their bounty has increased.";
                                                                }
                                                                _phrase913 = _phrase913.Replace("{PlayerName}", _cInfo2.playerName);
                                                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase913), Config.Server_Response_Name, false, "ServerTools", false);
                                                            }
                                                            if (_newValue >= Bounties.Kill_Streak)
                                                            {
                                                                _sql = string.Format("SELECT bounty FROM Players WHERE steamid = '{0}'", _cInfo2.playerId);
                                                                DataTable _result3 = SQL.TQuery(_sql);
                                                                int _oldBounty;
                                                                int.TryParse(_result3.Rows[0].ItemArray.GetValue(0).ToString(), out _oldBounty);
                                                                _result3.Dispose();
                                                                _sql = string.Format("UPDATE Players SET bounty = {0} WHERE steamid = '{1}'", _oldBounty + (_player2.Level * Bounties.Bounty), _cInfo.playerId);
                                                                SQL.FastQuery(_sql);
                                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                                {
                                                                    sw.WriteLine(string.Format("{0}: {1} is on a kill streak of {2}. Their bounty has increased.", DateTime.Now, _cInfo2.playerName, _newValue));
                                                                    sw.WriteLine();
                                                                    sw.Flush();
                                                                    sw.Close();
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        KillStreak.Add(_cInfo2.entityId, 1);
                                                    }
                                                }
                                            }
                                        }
                                        if (Zones.IsEnabled)
                                        {
                                            Zones.Check(_cInfo2, _cInfo);
                                        }
                                    }
                                }
                            }
                            if (Wallet.IsEnabled && Wallet.Lose_On_Death)
                            {
                                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_player.entityId);
                                if (_cInfo != null)
                                {
                                    World world = GameManager.Instance.World;
                                    string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                    DataTable _result = SQL.TQuery(_sql);
                                    int _playerSpentCoins;
                                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
                                    _result.Dispose();
                                    int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                                    if (_currentCoins >= 1)
                                    {
                                        _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins - _currentCoins, _cInfo.playerId);
                                        SQL.FastQuery(_sql);
                                    }
                                }
                            }
                            if (Event.Open && Event.Players.Contains(_player.entityId) && !Event.SpawnList.Contains(_player.entityId))
                            {
                                Event.SpawnList.Add(_player.entityId);
                            }
                        }
                    }
                    else
                    {
                        if (Zones.IsEnabled)
                        {
                            ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_player.entityId);
                            if (_cInfo != null)
                            {
                                ZoneCheck(_cInfo, _player);
                            }
                        }
                    }
                }
            }
            if (!DP)
            {
                _counter++;
                if (_counter >= 6)
                {
                    _counter = 0;
                    for (int i = 0; i < _playerList.Count; i++)
                    {
                        EntityPlayer _player = _playerList[i];
                        if (_player != null)
                        {
                            _player.ClearDamagedTarget();
                        }
                    }
                }
            }
            else
            {
                _counter = 0;
            }
        }

        public static void ZoneCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (Zones.Box1.Count > 0)
            {
                int _flagCount = 0;
                int _X = (int)_player.position.x;
                int _Y = (int)_player.position.y;
                int _Z = (int)_player.position.z;
                for (int i = 0; i < Zones.Box1.Count; i++)
                {
                    string[] _box = Zones.Box1[i];
                    bool[] _box2 = Zones.Box2[i];
                    if (Zones.BoxCheck(_box, _X, _Y, _Z, _box2))
                    {
                        if (!ZoneExit.ContainsKey(_player.entityId))
                        {
                            for (int j = 0; j < Zones.Box1.Count; j++)
                            {
                                string[] _box3 = Zones.Box1[j];
                                bool[] _box4 = Zones.Box2[j];
                                if (Zones.BoxCheck(_box3, _X, _Y, _Z, _box4))
                                {
                                    ZoneExit.Add(_player.entityId, _box3[3]);
                                    if (_box4[1])
                                    {
                                        ZonePvE.Add(_player.entityId);
                                        if (_box4[2])
                                        {
                                            EntityAlive entityAlive = GameManager.Instance.World.GetEntity(_player.entityId) as EntityAlive;
                                            entityAlive.IsGodMode.Value = true;
                                        }
                                    }
                                    if (Zones.Zone_Message)
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _box3[2]), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                    if (_box3[4] != "")
                                    {
                                        Zones.Response(_cInfo, _box3[4]);
                                    }
                                    Zones.reminder.Add(_player.entityId, DateTime.Now);
                                    Zones.reminderMsg.Add(_player.entityId, _box3[5]);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            string _exitMsg;
                            if (ZoneExit.TryGetValue(_player.entityId, out _exitMsg))
                            {
                                if (_exitMsg != _box[3])
                                {
                                    for (int j = 0; j < Zones.Box1.Count; j++)
                                    {
                                        string[] _box3 = Zones.Box1[j];
                                        bool[] _box4 = Zones.Box2[j];
                                        if (Zones.BoxCheck(_box3, _X, _Y, _Z, _box4))
                                        {
                                            ZoneExit[_player.entityId] = _box3[3];
                                            EntityAlive entityAlive = GameManager.Instance.World.GetEntity(_player.entityId) as EntityAlive;
                                            if (_box4[1])
                                            {
                                                if (!ZonePvE.Contains(_player.entityId))
                                                {
                                                    ZonePvE.Add(_player.entityId);
                                                }
                                                if (_box4[2])
                                                {
                                                    entityAlive.IsGodMode.Value = true;
                                                }
                                            }
                                            else
                                            {
                                                if (ZonePvE.Contains(_player.entityId))
                                                {
                                                    ZonePvE.Remove(_player.entityId);
                                                }
                                                entityAlive.IsGodMode.Value = false;
                                            }
                                            if (Zones.Zone_Message)
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _box3[2]), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                            if (_box3[4] != "")
                                            {
                                                Zones.Response(_cInfo, _box3[4]);
                                            }
                                            Zones.reminder[_player.entityId] = DateTime.Now;
                                            Zones.reminderMsg[_player.entityId] = _box3[5];
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        _flagCount++;
                        if (_flagCount == Zones.Box1.Count && ZoneExit.ContainsKey(_player.entityId))
                        {
                            if (Zones.Zone_Message)
                            {
                                string _msg;
                                if (ZoneExit.TryGetValue(_player.entityId, out _msg))
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _msg), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            ZoneExit.Remove(_player.entityId);
                            if (ZonePvE.Contains(_player.entityId))
                            {
                                ZonePvE.Remove(_player.entityId);
                            }
                            Zones.reminder.Remove(_player.entityId);
                            Zones.reminderMsg.Remove(_player.entityId);
                        }
                    }
                }
            }
        }
    }
}
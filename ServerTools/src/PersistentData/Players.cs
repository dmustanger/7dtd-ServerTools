using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    [Serializable]
    public class Players
    {
        public static bool Kill_Notice = false;
        public Dictionary<string, Player> players = new Dictionary<string, Player>();
        public Dictionary<string, string> clans = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static Dictionary<int, DateTime> DeathTime = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> LastDeathPos = new Dictionary<int, string>();
        public static Dictionary<int, int> KillStreak = new Dictionary<int, int>();
        public static Dictionary<string, string[]> Box = new Dictionary<string, string[]>();
        public static Dictionary<int, string> Victim = new Dictionary<int, string>();
        public static Dictionary<int, int> Forgive = new Dictionary<int, int>();
        public static Dictionary<int, string> ZoneFlag = new Dictionary<int, string>();
        private static Dictionary<int, int> Flag = new Dictionary<int, int>();
        public static List<int> ZonePvE = new List<int>();
        public static List<int> Dead = new List<int>();
        public static List<int> NoFlight = new List<int>();
        private static Dictionary<int, string> Friends = new Dictionary<int, string>();
        private static string file = string.Format("Bounty_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Bounties/{1}", API.GamePath, file);
        private static int _counter = 0;
        private static int _xMinCheck = 0, _yMinCheck = 0, _zMinCheck = 0, _xMaxCheck = 0, _yMaxCheck = 0, _zMaxCheck = 0;
        private static bool DP = false;

        public static void CreateFolder()
        {
            if (!Directory.Exists(API.GamePath + "/Bounties"))
            {
                Directory.CreateDirectory(API.GamePath + "/Bounties");
            }
        }

        public List<string> ClanList
        {
            get
            {
                return new List<string>(clans.Keys);
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

        public void GetClans()
        {
            foreach (string _id in PersistentContainer.Instance.Players.SteamIDs)
            {
                Player p = PersistentContainer.Instance.Players[_id, false];
                if (p.IsClanOwner)
                {
                    if (!clans.ContainsKey(p.ClanName))
                    {
                        if (p.ClanName != null)
                        {
                            clans.Add(p.ClanName, _id);
                        }
                    }
                }
            }
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            Session.Add(_cInfo.playerId, DateTime.Now);
        }

        public static void FriendList(ClientInfo _cInfo)
        {
            if (!Friends.ContainsKey(_cInfo.entityId))
            {
                string _friends = "";
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _player = _cInfoList[i];
                    EntityPlayer ent1 = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    EntityPlayer ent2 = GameManager.Instance.World.Players.dict[_player.entityId];
                    if (ent1 != null && ent2 != null)
                    {
                        if (ent1.IsFriendsWith(ent2))
                        {
                            _friends = string.Format("{0} {1}", _friends, _player.playerId);
                        }
                    }
                }
                if (_friends.Length != 0)
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
                                        if (Kill_Notice)
                                        {
                                            string _holdingItem = _player2.inventory.holdingItem.Name;
                                            if (_holdingItem == "handPlayer")
                                            {
                                                _holdingItem = "Fists of Fury";
                                            }
                                            ItemValue _itemValue = ItemClass.GetItem(_holdingItem, true);
                                            if (_itemValue.type != ItemValue.None.type)
                                            {
                                                _holdingItem = _itemValue.ItemClass.localizedName ?? _itemValue.ItemClass.Name;
                                            }
                                            string _phrase915;
                                            if (!Phrases.Dict.TryGetValue(915, out _phrase915))
                                            {
                                                _phrase915 = "{PlayerName} has killed {Victim} with {Item}.";
                                            }
                                            _phrase915 = _phrase915.Replace("{PlayerName}", _cInfo2.playerName);
                                            _phrase915 = _phrase915.Replace("{Victim}", _cInfo.playerName);
                                            _phrase915 = _phrase915.Replace("{Item}", _holdingItem);
                                            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase915), Config.Server_Response_Name, false, "ServerTools", false);
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
                                                        Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                                                        Player p2 = PersistentContainer.Instance.Players[_cInfo2.playerId, false];
                                                        if (p != null && p2 != null)
                                                        {
                                                            if (p.ClanName != null && p2.ClanName != null)
                                                            {
                                                                if (p.ClanName == p2.ClanName)
                                                                {
                                                                    return;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                int _bounty = PersistentContainer.Instance.Players[_cInfo.playerId, true].Bounty;
                                                if (_bounty > 0)
                                                {
                                                    int _oldCoins = PersistentContainer.Instance.Players[_cInfo2.playerId, true].PlayerSpentCoins;
                                                    PersistentContainer.Instance.Players[_cInfo2.playerId, true].PlayerSpentCoins = _oldCoins + _bounty;
                                                    PersistentContainer.Instance.Players[_cInfo.playerId, true].Bounty = 0;
                                                    PersistentContainer.Instance.Save();
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
                                                                int _oldBounty = PersistentContainer.Instance.Players[_cInfo2.playerId, true].Bounty;
                                                                PersistentContainer.Instance.Players[_cInfo.playerId, true].Bounty = _oldBounty + (_player2.Level * Bounties.Bounty);
                                                                PersistentContainer.Instance.Save();
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
                            int _X = (int)_player.position.x;
                            int _Y = (int)_player.position.y;
                            int _Z = (int)_player.position.z;
                            if (Box.Count > 0)
                            {
                                Flag.Remove(_player.entityId);
                                foreach (KeyValuePair<string, string[]> kvpCorners in Box)
                                {
                                    int xMin, yMin, zMin;
                                    string[] _corner1 = kvpCorners.Value[0].Split(',');
                                    int.TryParse(_corner1[0], out xMin);
                                    int.TryParse(_corner1[1], out yMin);
                                    int.TryParse(_corner1[2], out zMin);
                                    int xMax, yMax, zMax;
                                    string[] _corner2 = kvpCorners.Value[1].Split(',');
                                    int.TryParse(_corner2[0], out xMax);
                                    int.TryParse(_corner2[1], out yMax);
                                    int.TryParse(_corner2[2], out zMax);
                                    if (xMin >= 0 && xMax >= 0)
                                    {
                                        if (xMin < xMax)
                                        {
                                            if (_X >= xMin)
                                            {
                                                _xMinCheck = 1;
                                            }
                                            else
                                            {
                                                _xMinCheck = 0;
                                            }
                                            if (_X <= xMax)
                                            {
                                                _xMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _xMaxCheck = 0;
                                            }
                                        }
                                        else
                                        {
                                            if (_X <= xMin)
                                            {
                                                _xMinCheck = 1;
                                            }
                                            else
                                            {
                                                _xMinCheck = 0;
                                            }
                                            if (_X >= xMax)
                                            {
                                                _xMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _xMaxCheck = 0;
                                            }
                                        }
                                    }
                                    else if (xMin <= 0 && xMax <= 0)
                                    {
                                        if (xMin < xMax)
                                        {
                                            if (_X >= xMin)
                                            {
                                                _xMinCheck = 1;
                                            }
                                            else
                                            {
                                                _xMinCheck = 0;
                                            }
                                            if (_X <= xMax)
                                            {
                                                _xMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _xMaxCheck = 0;
                                            }
                                        }
                                        else
                                        {
                                            if (_X <= xMin)
                                            {
                                                _xMinCheck = 1;
                                            }
                                            else
                                            {
                                                _xMinCheck = 0;
                                            }
                                            if (_X >= xMax)
                                            {
                                                _xMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _xMaxCheck = 0;
                                            }
                                        }
                                    }
                                    else if (xMin <= 0 && xMax >= 0)
                                    {
                                        if (_X >= xMin)
                                        {
                                            _xMinCheck = 1;
                                        }
                                        else
                                        {
                                            _xMinCheck = 0;
                                        }
                                        if (_X <= xMax)
                                        {
                                            _xMaxCheck = 1;
                                        }
                                        else
                                        {
                                            _xMaxCheck = 0;
                                        }
                                    }
                                    else if (xMin >= 0 && xMax <= 0)
                                    {
                                        if (_X <= xMin)
                                        {
                                            _xMinCheck = 1;
                                        }
                                        else
                                        {
                                            _xMinCheck = 0;
                                        }
                                        if (_X >= xMax)
                                        {
                                            _xMaxCheck = 1;
                                        }
                                        else
                                        {
                                            _xMaxCheck = 0;
                                        }
                                    }

                                    if (yMin >= 0 && yMax >= 0)
                                    {
                                        if (yMin < yMax)
                                        {
                                            if (_Y >= yMin)
                                            {
                                                _yMinCheck = 1;
                                            }
                                            else
                                            {
                                                _yMinCheck = 0;
                                            }
                                            if (_Y <= yMax)
                                            {
                                                _yMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _yMaxCheck = 0;
                                            }
                                        }
                                        else
                                        {
                                            if (_Y <= yMin)
                                            {
                                                _yMinCheck = 1;
                                            }
                                            else
                                            {
                                                _yMinCheck = 0;
                                            }
                                            if (_Y >= yMax)
                                            {
                                                _yMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _yMaxCheck = 0;
                                            }
                                        }
                                    }
                                    else if (yMin <= 0 && yMax <= 0)
                                    {
                                        if (yMin < yMax)
                                        {
                                            if (_Y >= yMin)
                                            {
                                                _yMinCheck = 1;
                                            }
                                            else
                                            {
                                                _yMinCheck = 0;
                                            }
                                            if (_Y <= yMax)
                                            {
                                                _yMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _yMaxCheck = 0;
                                            }
                                        }
                                        else
                                        {
                                            if (_Y <= yMin)
                                            {
                                                _yMinCheck = 1;
                                            }
                                            else
                                            {
                                                _yMinCheck = 0;
                                            }
                                            if (_Y >= yMax)
                                            {
                                                _yMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _yMaxCheck = 0;
                                            }
                                        }
                                    }
                                    else if (yMin <= 0 && yMax >= 0)
                                    {
                                        if (_Y >= yMin)
                                        {
                                            _yMinCheck = 1;
                                        }
                                        else
                                        {
                                            _yMinCheck = 0;
                                        }
                                        if (_Y <= yMax)
                                        {
                                            _yMaxCheck = 1;
                                        }
                                        else
                                        {
                                            _yMaxCheck = 0;
                                        }
                                    }
                                    else if (yMin >= 0 && yMax <= 0)
                                    {
                                        if (_Y <= yMin)
                                        {
                                            _yMinCheck = 1;
                                        }
                                        else
                                        {
                                            _yMinCheck = 0;
                                        }
                                        if (_Y >= yMax)
                                        {
                                            _yMaxCheck = 1;
                                        }
                                        else
                                        {
                                            _yMaxCheck = 0;
                                        }
                                    }

                                    if (zMin >= 0 && zMax >= 0)
                                    {
                                        if (zMin < zMax)
                                        {
                                            if (_Z >= zMin)
                                            {
                                                _zMinCheck = 1;
                                            }
                                            else
                                            {
                                                _zMinCheck = 0;
                                            }
                                            if (_Z <= zMax)
                                            {
                                                _zMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _zMaxCheck = 0;
                                            }
                                        }
                                        else
                                        {
                                            if (_Z <= zMin)
                                            {
                                                _zMinCheck = 1;
                                            }
                                            else
                                            {
                                                _zMinCheck = 0;
                                            }
                                            if (_Z >= zMax)
                                            {
                                                _zMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _zMaxCheck = 0;
                                            }
                                        }
                                    }
                                    else if (zMin <= 0 && zMax <= 0)
                                    {
                                        if (zMin < zMax)
                                        {
                                            if (_Z >= zMin)
                                            {
                                                _zMinCheck = 1;
                                            }
                                            else
                                            {
                                                _zMinCheck = 0;
                                            }
                                            if (_Z <= zMax)
                                            {
                                                _zMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _zMaxCheck = 0;
                                            }
                                        }
                                        else
                                        {
                                            if (_Z <= zMin)
                                            {
                                                _zMinCheck = 1;
                                            }
                                            else
                                            {
                                                _zMinCheck = 0;
                                            }
                                            if (_Z >= zMax)
                                            {
                                                _zMaxCheck = 1;
                                            }
                                            else
                                            {
                                                _zMaxCheck = 0;
                                            }
                                        }
                                    }
                                    else if (zMin <= 0 && zMax >= 0)
                                    {
                                        if (_Z >= zMin)
                                        {
                                            _zMinCheck = 1;
                                        }
                                        else
                                        {
                                            _zMinCheck = 0;
                                        }
                                        if (_Z <= zMax)
                                        {
                                            _zMaxCheck = 1;
                                        }
                                        else
                                        {
                                            _zMaxCheck = 0;
                                        }
                                    }
                                    else if (zMin >= 0 && zMax <= 0)
                                    {
                                        if (_Z <= zMin)
                                        {
                                            _zMinCheck = 1;
                                        }
                                        else
                                        {
                                            _zMinCheck = 0;
                                        }
                                        if (_Z >= zMax)
                                        {
                                            _zMaxCheck = 1;
                                        }
                                        else
                                        {
                                            _zMaxCheck = 0;
                                        }
                                    }
                                    if (_xMinCheck == 1 && _yMinCheck == 1 && _zMinCheck == 1 && _xMaxCheck == 1 && _yMaxCheck == 1 && _zMaxCheck == 1)
                                    {
                                        if (!ZoneFlag.ContainsKey(_player.entityId))
                                        {
                                            ZoneFlag.Add(_player.entityId, kvpCorners.Value[3]);
                                            if (kvpCorners.Value[5] == "true")
                                            {
                                                ZonePvE.Add(_player.entityId);
                                            }
                                            ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_player.entityId);
                                            if (Zones.Zone_Message)
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, kvpCorners.Value[2]), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                            if (kvpCorners.Value[4] != null)
                                            {
                                                Zones.Response(_cInfo, kvpCorners.Value[4]);
                                            }
                                        }
                                        else
                                        {
                                            if (kvpCorners.Value[5] == "true")
                                            {
                                                ZonePvE[_player.entityId] = _player.entityId;
                                            }
                                            ZoneFlag[_player.entityId] = kvpCorners.Value[3];
                                        }
                                    }
                                    else
                                    {
                                        if (ZoneFlag.ContainsKey(_player.entityId))
                                        {
                                            if (Flag.ContainsKey(_player.entityId))
                                            {
                                                Flag[_player.entityId] += 1;
                                                int _flag;
                                                Flag.TryGetValue(_player.entityId, out _flag);
                                                {
                                                    if (_flag == Box.Count)
                                                    {
                                                        if (Zones.Zone_Message)
                                                        {
                                                            string _msg;
                                                            if (ZoneFlag.TryGetValue(_player.entityId, out _msg))
                                                            {
                                                                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_player.entityId);
                                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _msg), Config.Server_Response_Name, false, "ServerTools", false));
                                                            }
                                                        }
                                                        ZoneFlag.Remove(_player.entityId);
                                                        ZonePvE.Remove(_player.entityId);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Flag[_player.entityId] = 1;
                                                if (Box.Count == 1)
                                                {
                                                    if (Zones.Zone_Message)
                                                    {
                                                        string _msg;
                                                        if (ZoneFlag.TryGetValue(_player.entityId, out _msg))
                                                        {
                                                            ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_player.entityId);
                                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _msg), Config.Server_Response_Name, false, "ServerTools", false));
                                                        }
                                                    }
                                                    ZoneFlag.Remove(_player.entityId);
                                                    ZonePvE.Remove(_player.entityId);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (!DP)
            {
                _counter++;
                if (_counter >= 10)
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
    }
}
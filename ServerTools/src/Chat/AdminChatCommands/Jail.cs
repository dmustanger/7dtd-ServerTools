using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ServerTools
{
    public class Jail
    {
        public static bool IsRunning = false;
        public static bool IsEnabled = false;
        public static int JailSize = 8;
        private static string[] _cmd = { "jail" };
        public static string JailPosition = "0,0,0";
        private static Thread th;
        public static SortedDictionary<string, string> Dict = new SortedDictionary<string, string>();
        public static SortedDictionary<string, string> Dict1 = new SortedDictionary<string, string>();

        private static List<string> List
        {
            get { return new List<string>(Dict.Keys); }
        }

        public static void Load()
        {
            Start();
            IsRunning = true;
        }

        public static void Unload()
        {
            th.Abort();
            IsRunning = false;
        }

        public static void SetJail(ClientInfo _cInfo)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                string x = _position.x.ToString();
                string y = _position.y.ToString();
                string z = _position.z.ToString();
                string _sposition = x + "," + y + "," + z;
                JailPosition = _sposition;
                string _phrase502;
                if (!Phrases.Dict.TryGetValue(502, out _phrase502))
                {
                    _phrase502 = "{PlayerName} you have set the jail position as {JailPosition}.";
                }
                _phrase502 = _phrase502.Replace("{PlayerName}", _cInfo.playerName);
                _phrase502 = _phrase502.Replace("{JailPosition}", JailPosition);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase502), "Server", false, "", false));
                Config.UpdateXml();
            }
        }

        public static void PutInJail(ClientInfo _cInfo, string _playerName)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
            }
            else
            {
                if (JailPosition == "0,0,0")
                {
                    string _phrase503;
                    if (!Phrases.Dict.TryGetValue(503, out _phrase503))
                    {
                        _phrase503 = "{PlayerName} the jail position jas not been set.";
                    }
                    _phrase503 = _phrase503.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase503), "Server", false, "", false));
                }
                else
                {
                    _playerName = _playerName.Replace("jail ", "");
                    ClientInfo _PlayertoJail = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoJail == null)
                    {
                        string _phrase201;
                        if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                        {
                            _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                        }
                        _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                        _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        Player p = PersistentContainer.Instance.Players[_PlayertoJail.playerId, false];
                        if (p == null)
                        {
                            PutPlayerInJail(_cInfo, _PlayertoJail);
                        }
                        else
                        {
                            if (p.IsJailed)
                            {
                                string _phrase504;
                                if (!Phrases.Dict.TryGetValue(504, out _phrase504))
                                {
                                    _phrase504 = "{AdminPlayerName} player {PlayerName} is already in jail.";
                                }
                                _phrase504 = _phrase504.Replace("{AdminPlayerName}", _cInfo.playerName);
                                _phrase504 = _phrase504.Replace("{PlayerName}", _playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase504, CustomCommands.ChatColor), "Server", false, "", false));
                            }
                            else
                            {
                                PutPlayerInJail(_cInfo, _PlayertoJail);
                            }
                        }
                    }
                }
            }
        }

        private static void PutPlayerInJail(ClientInfo _cInfo, ClientInfo _PlayertoJail)
        {
            float xf;
            float yf;
            float zf;
            string[] _cords = JailPosition.Split(',');
            float.TryParse(_cords[0], out xf);
            float.TryParse(_cords[1], out yf);
            float.TryParse(_cords[2], out zf);
            int x = (int)xf;
            int y = (int)yf;
            int z = (int)zf;
            SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _PlayertoJail.entityId, x, y, z), _cInfo);
            if (!Dict.ContainsKey(_PlayertoJail.playerId))
            {
                Dict.Add(_PlayertoJail.playerId, null);
                Dict1.Add(_PlayertoJail.playerId, _PlayertoJail.playerName);

            }
            PersistentContainer.Instance.Players[_PlayertoJail.playerId, true].IsJailed = true;
            PersistentContainer.Instance.Players[_PlayertoJail.playerId, true].IsRemovedFromJail = false;
            PersistentContainer.Instance.Save();
            string _phrase500;
            if (!Phrases.Dict.TryGetValue(500, out _phrase500))
            {
                _phrase500 = "{PlayerName} you have been sent to jail.";
            }
            _phrase500 = _phrase500.Replace("{PlayerName}", _PlayertoJail.playerName);
            _PlayertoJail.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase500), "Server", false, "", false));
            string _phrase505;
            if (!Phrases.Dict.TryGetValue(505, out _phrase505))
            {
                _phrase505 = "{AdminPlayerName} you have put {PlayerName} in jail.";
            }
            _phrase505 = _phrase505.Replace("{AdminPlayerName}", _cInfo.playerName);
            _phrase505 = _phrase505.Replace("{PlayerName}", _PlayertoJail.playerName);
            _PlayertoJail.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase505), "Server", false, "", false));
        }

        public static void RemoveFromJail(ClientInfo _cInfo, string _playerName)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
            }
            else
            {
                _playerName = _playerName.Replace("unjail ", "");
                ClientInfo _PlayertoUnJail = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_PlayertoUnJail == null)
                {
                    string _phrase201;
                    if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                    {
                        _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                    }
                    _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                    _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    Player p = PersistentContainer.Instance.Players[_PlayertoUnJail.playerId, false];
                    if (p == null)
                    {
                        string _phrase506;
                        if (!Phrases.Dict.TryGetValue(506, out _phrase506))
                        {
                            _phrase506 = "{AdminPlayerName} player {PlayerName} is not in jail.";
                        }
                        _phrase506 = _phrase506.Replace("{AdminPlayerName}", _cInfo.playerName);
                        _phrase506 = _phrase506.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase506, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (!p.IsJailed)
                        {
                            string _phrase506;
                            if (!Phrases.Dict.TryGetValue(506, out _phrase506))
                            {
                                _phrase506 = "{AdminPlayerName} player {PlayerName} is not in jail.";
                            }
                            _phrase506 = _phrase506.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase506 = _phrase506.Replace("{PlayerName}", _playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase506, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            if (Dict.ContainsKey(_PlayertoUnJail.playerId))
                            {
                                Dict.Remove(_PlayertoUnJail.playerId);
                                Dict1.Remove(_PlayertoUnJail.playerId);
                            }
                            PersistentContainer.Instance.Players[_PlayertoUnJail.playerId, false].IsJailed = false;
                            PersistentContainer.Instance.Players[_PlayertoUnJail.playerId, false].IsRemovedFromJail = true;
                            PersistentContainer.Instance.Save();
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_PlayertoUnJail.entityId];
                            EntityBedrollPositionList _position = _player.SpawnPoints;
                            if (_position.Count > 0)
                            {
                                SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _PlayertoUnJail.entityId, _position[0].x, _position[0].y, _position[0].z), _cInfo);
                            }
                            else
                            {
                                Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _PlayertoUnJail.entityId, _pos[0].x, _pos[0].y, _pos[0].z), _cInfo);
                            }
                            string _phrase501;
                            if (!Phrases.Dict.TryGetValue(501, out _phrase501))
                            {
                                _phrase501 = "{PlayerName} you have been released from jail.";
                            }
                            _phrase501 = _phrase501.Replace("{PlayerName}", _PlayertoUnJail.playerName);
                            _PlayertoUnJail.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase501), "Server", false, "", false));
                        }
                    }
                }
            }
        }

        public static void CheckPlayer(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null)
            {
                if (p.IsJailed)
                {
                    if (!Dict.ContainsKey(_cInfo.playerId))
                    {
                        Dict.Add(_cInfo.playerId, null);
                        Dict1.Add(_cInfo.playerId, _cInfo.playerName);
                    }
                }
                else
                {
                    if (Dict.ContainsKey(_cInfo.playerId))
                    {
                        Dict.Remove(_cInfo.playerId);
                        Dict1.Remove(_cInfo.playerId);
                    }
                    if (!p.IsRemovedFromJail)
                    {
                        EntityBedrollPositionList _position = _player.SpawnPoints;
                        if (_position.Count > 0)
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, _position[0].x, _position[0].y, _position[0].z), _cInfo);
                        }
                        else
                        {
                            Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                            SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, _pos[0].x, _pos[0].y, _pos[0].z), _cInfo);
                        }
                        string _phrase501;
                        if (!Phrases.Dict.TryGetValue(501, out _phrase501))
                        {
                            _phrase501 = "{PlayerName} you have been released from jail.";
                        }
                        _phrase501 = _phrase501.Replace("{PlayerName}", _cInfo.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase501), "Server", false, "", false));
                        PersistentContainer.Instance.Players[_cInfo.playerId, false].IsRemovedFromJail = true;
                        PersistentContainer.Instance.Save();
                    }
                }
            }
        }

        private static void Start()
        {
            th = new Thread(new ThreadStart(StatusCheck));
            th.IsBackground = true;
            th.Start();
        }

        private static void StatusCheck()
        {
            while (IsEnabled)
            {
                if (Dict.Count > 0)
                {
                    foreach (string _steamid in List)
                    {
                        float xf;
                        float yf;
                        float zf;
                        string[] _cords = JailPosition.Split(',');
                        float.TryParse(_cords[0], out xf);
                        float.TryParse(_cords[1], out yf);
                        float.TryParse(_cords[2], out zf);
                        int x = (int)xf;
                        int y = (int)yf;
                        int z = (int)zf;
                        ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_steamid);
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (_player.Spawned)
                        {
                            float _distance = _player.GetDistanceSq(new Vector3(xf, yf, zf));
                            int _dis = (int)_distance;
                            if (_dis > JailSize)
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _dis), "Server", false, "", false));
                                SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, x, y, z), _cInfo);
                            }
                        } 
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
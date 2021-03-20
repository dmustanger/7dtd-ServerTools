using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Event
    {
        public static bool Open = false, Invited = false, Cancel = false, Extend = false, Return = false, OperatorWarned = false;
        public static string Command100 = "join";
        public static Dictionary<string, int> Teams = new Dictionary<string, int>();
        public static string Operator = "", EventName = "";
        public static int TeamCount = 0, PlayerCount = 0, TeamSetup = 0, Time = 0;

        public static void CheckOpen()
        {
            if (!Open)
            {
                Invited = false;
                Teams.Clear();
                EventName = "";
                TeamCount = 0;
                PlayerCount = 0;
                TeamSetup = 0;
                Time = 0;
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(Operator);
                Operator = "";
                if (_cInfo != null)
                {
                    Phrases.Dict.TryGetValue(771, out string _phrase771);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase771 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                Phrases.Dict.TryGetValue(772, out string _phrase772);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase772 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
        }

        public static void AddPlayer(ClientInfo _cInfo)
        {
            if (!Teams.ContainsKey(_cInfo.playerId))
            {
                if (Teams.Count >= PlayerCount)
                {
                    Phrases.Dict.TryGetValue(773, out string _phrase773);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase773 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    Vector3 _position = _player.GetPosition();
                    int x = (int)_position.x;
                    int y = (int)_position.y;
                    int z = (int)_position.z;
                    string _sposition = x + "," + y + "," + z;
                    PersistentContainer.Instance.Players[_cInfo.playerId].EventReturnPosition = _sposition;
                    Phrases.Dict.TryGetValue(774, out string _phrase774);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase774 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                EventCommandsConsole.Setup.TryGetValue(Operator, out List<string> _setup);
                Teams.Add(_cInfo.playerId, TeamSetup);
                Phrases.Dict.TryGetValue(775, out string _phrase775);
                _phrase775 = _phrase775.Replace("{Value}", TeamSetup.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase775 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                TeamSetup--;
                if (TeamSetup == 0)
                {
                    TeamSetup = TeamCount;
                }
                if (Teams.Count == PlayerCount)
                {
                    Invited = false;
                    foreach (var _teamPlayer in Teams)
                    {
                        ClientInfo _eventClientInfo = ConnectionManager.Instance.Clients.ForPlayerId(_teamPlayer.Key);
                        if (_eventClientInfo != null)
                        {
                            EntityPlayer _eventPlayer = GameManager.Instance.World.Players.dict[_eventClientInfo.entityId];
                            if (_eventPlayer != null && _eventPlayer.IsAlive())
                            {
                                Teams.TryGetValue(_teamPlayer.Key, out int _teamNumber);
                                string _spawnPosition = _setup[_teamNumber + 4];
                                int _x, _y, _z;
                                string[] _cords = _spawnPosition.Split(',');
                                int.TryParse(_cords[0], out _x);
                                int.TryParse(_cords[1], out _y);
                                int.TryParse(_cords[2], out _z);
                                _eventClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_eventClientInfo.playerId].EventSpawn = true;
                            }
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_eventClientInfo.playerId].EventSpawn = true;
                        }
                    }
                    int _eventTime = Time * 60;
                    Timers._eventTime = _eventTime;
                    Open = true;
                    Phrases.Dict.TryGetValue(776, out string _phrase776);
                    _phrase776 = _phrase776.Replace("{EventName}", TeamSetup.ToString());
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase776 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(777, out string _phrase777);
                    _phrase777 = _phrase777.Replace("{EventName}", TeamSetup.ToString());
                    _phrase777 = _phrase777.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase777 = _phrase777.Replace("{Command100}", Command100);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase777 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    Phrases.Dict.TryGetValue(778, out string _phrase778);
                    _phrase778 = _phrase778.Replace("{Value}", Teams.Count.ToString());
                    _phrase778 = _phrase778.Replace("{PlayerTotal}", PlayerCount.ToString());
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase778 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(779, out string _phrase779);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase779 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void HalfTime()
        {
            Phrases.Dict.TryGetValue(780, out string _phrase780);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase780 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void FiveMin()
        {
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.GetForPlayerName(Operator);
            if (_cInfo != null)
            {
                Phrases.Dict.TryGetValue(782, out string _phrase782);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase782 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            Phrases.Dict.TryGetValue(781, out string _phrase781);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase781 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void EndEvent()
        {
            foreach (var _eventPlayer in Teams)
            {
                ClientInfo _eventClientInfo = ConnectionManager.Instance.Clients.ForPlayerId(_eventPlayer.Key);
                if (_eventClientInfo != null)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_eventClientInfo.entityId];
                    if (_player.IsSpawned())
                    {
                        string _returnPosition = PersistentContainer.Instance.Players[_eventClientInfo.playerId].EventReturnPosition;
                        string[] _cords = _returnPosition.Split(',');
                        int.TryParse(_cords[0], out int _x);
                        int.TryParse(_cords[1], out int _y);
                        int.TryParse(_cords[2], out int _z);
                        _eventClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                        Teams.Remove(_eventPlayer.Key);
                    }
                }
                else
                {
                    PersistentContainer.Instance.Players[_eventPlayer.Key].EventOver = true;
                    Teams.Remove(_eventPlayer.Key);
                }
            }
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.GetForPlayerName(Operator);
            Open = false;
            Teams.Clear();
            Operator = "";
            EventName = "";
            TeamCount = 0;
            PlayerCount = 0;
            TeamSetup = 0;
            Time = 0;
            Phrases.Dict.TryGetValue(783, out string _phrase783);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase783 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void EventOver(ClientInfo _cInfo)
        {
            string _returnPosition = PersistentContainer.Instance.Players[_cInfo.playerId].EventReturnPosition;
            string[] _cords = _returnPosition.Split(',');
            int.TryParse(_cords[0], out int _x);
            int.TryParse(_cords[1], out int _y);
            int.TryParse(_cords[2], out int _z);
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
            Phrases.Dict.TryGetValue(784, out string _phrase784);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase784 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void Spawn(ClientInfo _cInfo)
        {
            if (Teams.TryGetValue(_cInfo.playerId, out int _team))
            {
                if (EventCommandsConsole.Setup.TryGetValue(Operator, out List<string> _setup))
                {
                    string _spawnPosition = _setup[_team + 4];
                    string[] _cords = _spawnPosition.Split(',');
                    int.TryParse(_cords[0], out int _x);
                    int.TryParse(_cords[1], out int _y);
                    int.TryParse(_cords[2], out int _z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                    PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn = false;
                    Phrases.Dict.TryGetValue(785, out string _phrase785);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase785 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn = false;
                Phrases.Dict.TryGetValue(786, out string _phrase786);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase786 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Respawn(ClientInfo _cInfo)
        {
            if (Teams.TryGetValue(_cInfo.playerId, out int _team))
            {
                if (EventCommandsConsole.Setup.TryGetValue(Operator, out List<string> _setup))
                {
                    _setup.RemoveRange(0, 4 + _team);
                    string _respawnPosition = _setup[_team - 1];
                    string[] _cords = _respawnPosition.Split(',');
                    int.TryParse(_cords[0], out int _x);
                    int.TryParse(_cords[1], out int _y);
                    int.TryParse(_cords[2], out int _z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                }
            }
        }
    }
}

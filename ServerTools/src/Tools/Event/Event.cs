using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Event
    {
        public static bool Open = false, Invited = false, Cancel = false, Extend = false, Return = false, OperatorWarned = false;
        public static string Command_join = "join";
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
                    Phrases.Dict.TryGetValue("Event1", out string _phrase);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                Phrases.Dict.TryGetValue("Event2", out string _phrase1);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
        }

        public static void AddPlayer(ClientInfo _cInfo)
        {
            if (!Teams.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                if (Teams.Count >= PlayerCount)
                {
                    Phrases.Dict.TryGetValue("Event3", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                EntityPlayer player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (player != null)
                {
                    Vector3 _position = player.GetPosition();
                    int x = (int)_position.x;
                    int y = (int)_position.y;
                    int z = (int)_position.z;
                    string sposition = x + "," + y + "," + z;
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].EventReturnPosition = sposition;
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("Event4", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                EventCommandsConsole.Setup.TryGetValue(Operator, out List<string> setup);
                Teams.Add(_cInfo.CrossplatformId.CombinedString, TeamSetup);
                Phrases.Dict.TryGetValue("Event5", out string phrase1);
                phrase1 = phrase1.Replace("{Value}", TeamSetup.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                TeamSetup--;
                if (TeamSetup == 0)
                {
                    TeamSetup = TeamCount;
                }
                if (Teams.Count == PlayerCount)
                {
                    Invited = false;
                    foreach (var teamPlayer in Teams)
                    {
                        ClientInfo eventClientInfo = GeneralOperations.GetClientInfoFromNameOrId(teamPlayer.Key);
                        if (eventClientInfo != null)
                        {
                            EntityPlayer eventPlayer = GameManager.Instance.World.Players.dict[eventClientInfo.entityId];
                            if (eventPlayer != null && eventPlayer.IsAlive())
                            {
                                Teams.TryGetValue(teamPlayer.Key, out int _teamNumber);
                                string spawnPosition = setup[_teamNumber + 4];
                                string[] cords = spawnPosition.Split(',');
                                int.TryParse(cords[0], out int x);
                                int.TryParse(cords[1], out int y);
                                int.TryParse(cords[2], out int z);
                                eventClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[eventClientInfo.CrossplatformId.CombinedString].EventSpawn = true;
                                PersistentContainer.DataChange = true;
                            }
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[eventClientInfo.CrossplatformId.CombinedString].EventSpawn = true;
                            PersistentContainer.DataChange = true;
                        }
                    }
                    int eventTime = Time * 60;
                    Timers.eventTime = eventTime;
                    Open = true;
                    Phrases.Dict.TryGetValue("Event6", out string _phrase);
                    _phrase = _phrase.Replace("{EventName}", TeamSetup.ToString());
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Event7", out string _phrase);
                    _phrase = _phrase.Replace("{EventName}", TeamSetup.ToString());
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_join}", Command_join);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    Phrases.Dict.TryGetValue("Event8", out _phrase);
                    _phrase = _phrase.Replace("{Value}", Teams.Count.ToString());
                    _phrase = _phrase.Replace("{PlayerTotal}", PlayerCount.ToString());
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Event9", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void HalfTime()
        {
            Phrases.Dict.TryGetValue("Event10", out string _phrase);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void FiveMin()
        {
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.GetForPlayerName(Operator);
            if (_cInfo != null)
            {
                Phrases.Dict.TryGetValue("Event12", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            Phrases.Dict.TryGetValue("Event11", out string _phrase1);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void EndEvent()
        {
            foreach (var eventPlayer in Teams)
            {
                ClientInfo eventClientInfo = GeneralOperations.GetClientInfoFromNameOrId(eventPlayer.Key);
                if (eventClientInfo != null)
                {
                    EntityPlayer player = GameManager.Instance.World.Players.dict[eventClientInfo.entityId];
                    if (player.IsSpawned())
                    {
                        string returnPosition = PersistentContainer.Instance.Players[eventClientInfo.CrossplatformId.CombinedString].EventReturnPosition;
                        string[] cords = returnPosition.Split(',');
                        int.TryParse(cords[0], out int x);
                        int.TryParse(cords[1], out int y);
                        int.TryParse(cords[2], out int z);
                        eventClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        Teams.Remove(eventPlayer.Key);
                    }
                }
                else
                {
                    PersistentContainer.Instance.Players[eventPlayer.Key].EventOver = true;
                    Teams.Remove(eventPlayer.Key);
                }
            }
            Open = false;
            Teams.Clear();
            Operator = "";
            EventName = "";
            TeamCount = 0;
            PlayerCount = 0;
            TeamSetup = 0;
            Time = 0;
            Phrases.Dict.TryGetValue("Event13", out string phrase);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void EventOver(ClientInfo _cInfo)
        {
            string returnPosition = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].EventReturnPosition;
            string[] cords = returnPosition.Split(',');
            int.TryParse(cords[0], out int x);
            int.TryParse(cords[1], out int y);
            int.TryParse(cords[2], out int z);
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
            Phrases.Dict.TryGetValue("Event14", out string phrase);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void Spawn(ClientInfo _cInfo)
        {
            if (Teams.TryGetValue(_cInfo.CrossplatformId.CombinedString, out int _team))
            {
                if (EventCommandsConsole.Setup.TryGetValue(Operator, out List<string> _setup))
                {
                    string spawnPosition = _setup[_team + 4];
                    string[] cords = spawnPosition.Split(',');
                    int.TryParse(cords[0], out int x);
                    int.TryParse(cords[1], out int y);
                    int.TryParse(cords[2], out int z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].EventSpawn = false;
                    Phrases.Dict.TryGetValue("Event15", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].EventSpawn = false;
                Phrases.Dict.TryGetValue("Event16", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Respawn(ClientInfo _cInfo)
        {
            if (Teams.TryGetValue(_cInfo.CrossplatformId.CombinedString, out int _team))
            {
                if (EventCommandsConsole.Setup.TryGetValue(Operator, out List<string> _setup))
                {
                    _setup.RemoveRange(0, 4 + _team);
                    string respawnPosition = _setup[_team - 1];
                    string[] cords = respawnPosition.Split(',');
                    int.TryParse(cords[0], out int x);
                    int.TryParse(cords[1], out int y);
                    int.TryParse(cords[2], out int z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                }
            }
        }
    }
}

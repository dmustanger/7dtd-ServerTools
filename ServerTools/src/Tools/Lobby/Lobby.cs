using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Lobby
    {
        public static bool IsEnabled = false, Return = false, Reserved_Only = false, PvE = true, Damage_Z = true, Bloodmoon = false,
            Player_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 5, Lobby_Size = 25, Command_Cost = 0, Player_Killing_Mode = 0;
        public static string Lobby_Position = "0,0,0", Command_lobbyback = "lobbyback", Command_lback = "lback", 
            Command_set = "setlobby", Command_lobby = "lobby";

        public static List<int> LobbyPlayers = new List<int>();
        public static Bounds LobbyBounds = new Bounds();

        public static void SetBounds(string _position)
        {
            if (_position != "0,0,0" && _position != "0 0 0" && _position != "" && _position.Contains(","))
            {
                string[] cords = _position.Split(',').ToArray();
                if (int.TryParse(cords[0], out int x))
                {
                    if (int.TryParse(cords[1], out int y))
                    {
                        if (int.TryParse(cords[2], out int z))
                        {
                            Lobby_Position = _position;
                            LobbyBounds.center = new Vector3(x, y, z);
                            int size = Lobby_Size * 2;
                            LobbyBounds.size = new Vector3(size, size, size);
                        }
                    }
                }
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (!Bloodmoon && GeneralFunction.IsBloodmoon())
            {
                Phrases.Dict.TryGetValue("Lobby13", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (Reserved_Only && ReservedSlots.IsEnabled && !ReservedSlots.ReservedCheck(_cInfo.PlatformId, _cInfo.CrossplatformId))
            {
                Phrases.Dict.TryGetValue("Lobby9", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (Delay_Between_Uses < 1)
            {
                if (Command_Cost >= 1 && Wallet.IsEnabled)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    LobbyTele(_cInfo);
                }
            }
            else
            {
                DateTime lastLobby = DateTime.Now;
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastLobby != null)
                {
                    lastLobby = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastLobby;
                }
                TimeSpan varTime = DateTime.Now - lastLobby;
                double fractionalMinutes = varTime.TotalMinutes;
                int timepassed = (int)fractionalMinutes;
                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                {
                    if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                    {
                        if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Time(_cInfo, timepassed, delay);
                                return;
                            }
                        }
                        else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Time(_cInfo, timepassed, delay);
                                return;
                            }
                        }
                    }
                }
                Time(_cInfo, timepassed, Delay_Between_Uses);
            }
        }

        private static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Command_Cost >= 1 && Wallet.IsEnabled)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    LobbyTele(_cInfo);
                }
            }
            else
            {
                int timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue("Lobby1", out string phrase);
                phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{Command_lobby}", Command_lobby);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void CommandCost(ClientInfo _cInfo)
        {
            if (Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString) >= Command_Cost)
            {
                LobbyTele(_cInfo);
            }
            else
            {
                Phrases.Dict.TryGetValue("Lobby4", out string phrase);
                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void LobbyTele(ClientInfo _cInfo)
        {
            if (Lobby_Position != "0,0,0" && Lobby_Position != "0 0 0" && Lobby_Position != "")
            {
                EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (!LobbyPlayers.Contains(_cInfo.entityId))
                    {
                        if (Player_Check)
                        {
                            if (Teleportation.PCheck(_cInfo, player))
                            {
                                return;
                            }
                        }
                        if (Zombie_Check)
                        {
                            if (Teleportation.ZCheck(_cInfo, player))
                            {
                                return;
                            }
                        }
                        string[] cords = Lobby_Position.Split(',').ToArray();
                        if (int.TryParse(cords[0], out int i))
                        {
                            if (int.TryParse(cords[1], out int j))
                            {
                                if (int.TryParse(cords[2], out int k))
                                {
                                    LobbyPlayers.Add(_cInfo.entityId);
                                    if (Return)
                                    {
                                        Vector3 position = player.GetPosition();
                                        int x = (int)position.x;
                                        int y = (int)position.y;
                                        int z = (int)position.z;
                                        string pposition = x + "," + y + "," + z;
                                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LobbyReturnPos = pposition;
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(i, j, k), null, false));
                                        Phrases.Dict.TryGetValue("Lobby3", out string phrase);
                                        phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                        phrase = phrase.Replace("{Command_lobbyback}", Command_lobbyback);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(i, j, k), null, false));
                                    }
                                    if (Command_Cost >= 1 && Wallet.IsEnabled)
                                    {
                                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                                    }
                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastLobby = DateTime.Now;
                                    PersistentContainer.DataChange = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Lobby10", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Lobby5", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SendBack(ClientInfo _cInfo)
        {
            if (LobbyPlayers.Contains(_cInfo.entityId))
            {
                EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    LobbyPlayers.Remove(_cInfo.entityId);
                    string lastPos = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LobbyReturnPos;
                    if (lastPos != "")
                    {
                        string[] returnCoords = lastPos.Split(',');
                        int.TryParse(returnCoords[0], out int x);
                        int.TryParse(returnCoords[1], out int y);
                        int.TryParse(returnCoords[2], out int z);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LobbyReturnPos = "";
                        PersistentContainer.DataChange = true;
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Lobby6", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Lobby6", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void InsideLobby(ClientInfo _cInfo, EntityAlive _player, List<Entity> _entityList)
        {
            if (!IsLobby(_player.position))
            {
                LobbyPlayers.Remove(_cInfo.entityId);
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LobbyReturnPos = "";
                PersistentContainer.DataChange = true;
                Phrases.Dict.TryGetValue("Lobby7", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                if (_entityList != null && _entityList.Count > 0)
                {
                    for (int i = 0; i < _entityList.Count; i++)
                    {
                        if (IsLobby(_entityList[i].position))
                        {
                            GameManager.Instance.World.RemoveEntity(_entityList[i].entityId, EnumRemoveEntityReason.Despawned);
                            Log.Out(string.Format("[SERVERTOOLS] Removed a hostile from the lobby @ '{0}'", _entityList[i].position));
                        }
                    }
                }
            }
        }

        public static bool IsLobby(Vector3 _position)
        {
            if (LobbyBounds.Contains(_position))
            {
                return true;
            }
            return false;
        }

        public static bool PvEViolation(ClientInfo _cInfo2)
        {
            try
            {
                Phrases.Dict.TryGetValue("Lobby11", out string phrase);
                ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                if (GeneralFunction.PvEViolations.ContainsKey(_cInfo2.entityId))
                {
                    GeneralFunction.PvEViolations.TryGetValue(_cInfo2.entityId, out int _violations);
                    _violations++;
                    GeneralFunction.PvEViolations[_cInfo2.entityId] = _violations;
                    if (GeneralFunction.Jail_Violation > 0 && _violations == GeneralFunction.Jail_Violation)
                    {
                        GeneralFunction.JailPlayer(_cInfo2);
                    }
                    if (GeneralFunction.Kill_Violation > 0 && _violations == GeneralFunction.Kill_Violation)
                    {
                        GeneralFunction.KillPlayer(_cInfo2);
                    }
                    if (GeneralFunction.Kick_Violation > 0 && _violations == GeneralFunction.Kick_Violation)
                    {
                        GeneralFunction.KickPlayer(_cInfo2);
                    }
                    else if (GeneralFunction.Ban_Violation > 0 && _violations == GeneralFunction.Ban_Violation)
                    {
                        GeneralFunction.BanPlayer(_cInfo2);
                    }
                }
                else
                {
                    GeneralFunction.PvEViolations.Add(_cInfo2.entityId, 1);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Lobby.PvEViolation: {0}", e.Message));
            }
            return true;
        }

        public static void SetPosition(string _position)
        {
            if (_position != "0,0,0" && _position != "0 0 0" && _position != "" && _position.Contains(","))
            {
                string[] lobbyPosition = _position.Split(',');
                int.TryParse(lobbyPosition[0], out int x);
                int.TryParse(lobbyPosition[1], out int y);
                int.TryParse(lobbyPosition[2], out int z);
                LobbyBounds.center = new Vector3(x, y, z);
                int size = Lobby_Size * 2;
                LobbyBounds.size = new Vector3(size, size, size);
                Lobby_Position = _position;
            }
        }
    }
}

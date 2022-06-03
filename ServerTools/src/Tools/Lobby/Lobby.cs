using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Lobby
    {
        public static bool IsEnabled = false, Return = false, Player_Check = false, Zombie_Check = false, Reserved_Only = false, PvE = false;
        public static int Delay_Between_Uses = 5, Lobby_Size = 25, Command_Cost = 0, Player_Killing_Mode = 0;
        public static string Lobby_Position = "0,0,0", Command_lobbyback = "lobbyback", Command_lback = "lback", Command_set = "setlobby", Command_lobby = "lobby";
        public static List<int> LobbyPlayers = new List<int>();

        public static void Set(ClientInfo _cInfo)
        {
            string[] command = { Command_set };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(command, _cInfo))
            {
                Phrases.Dict.TryGetValue("Lobby8", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    Vector3 position = player.GetPosition();
                    int x = (int)position.x;
                    int y = (int)position.y;
                    int z = (int)position.z;
                    string lposition = x + "," + y + "," + z;
                    Lobby_Position = lposition;
                    Config.WriteXml();
                    Phrases.Dict.TryGetValue("Lobby2", out string phrase);
                    phrase = phrase.Replace("{LobbyPosition}", Lobby_Position);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (Reserved_Only && ReservedSlots.IsEnabled && !ReservedSlots.ReservedCheck(_cInfo.PlatformId, _cInfo.CrossplatformId))
            {
                Phrases.Dict.TryGetValue("Lobby9", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (Delay_Between_Uses < 1)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
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
                if (Wallet.IsEnabled && Command_Cost >= 1)
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
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
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
                        if (!Teleportation.Teleporting.Contains(_cInfo.entityId))
                        {
                            Teleportation.Teleporting.Add(_cInfo.entityId);
                        }
                        int x, y, z;
                        if (Return)
                        {
                            Vector3 position = player.GetPosition();
                            x = (int)position.x;
                            y = (int)position.y;
                            z = (int)position.z;
                            string pposition = x + "," + y + "," + z;
                            LobbyPlayers.Add(_cInfo.entityId);
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LobbyReturnPos = pposition;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Lobby3", out string phrase);
                            phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            phrase = phrase.Replace("{Command_lobbyback}", Command_lobbyback);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        string[] cords = Lobby_Position.Split(',').ToArray();
                        if (int.TryParse(cords[0], out int i))
                        {
                            if (int.TryParse(cords[1], out int j))
                            {
                                if (int.TryParse(cords[2], out int k))
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(i, j, k), null, false));
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
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
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    string lastPos = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LobbyReturnPos;
                    if (lastPos != "")
                    {
                        string[] returnCoords = lastPos.Split(',');
                        int.TryParse(returnCoords[0], out int x);
                        int.TryParse(returnCoords[1], out int y);
                        int.TryParse(returnCoords[2], out int z);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        LobbyPlayers.Remove(_cInfo.entityId);
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LobbyReturnPos = "";
                        PersistentContainer.DataChange = true;
                    }
                    else
                    {
                        LobbyPlayers.Remove(_cInfo.entityId);
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

        public static void InsideLobby(ClientInfo _cInfo, EntityAlive _player)
        {
            if (!InsideLobby(_player.position.x, _player.position.z))
            {
                LobbyPlayers.Remove(_cInfo.entityId);
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LobbyReturnPos = "";
                PersistentContainer.DataChange = true;
                Phrases.Dict.TryGetValue("Lobby7", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static bool InsideLobby(float _x, float _z)
        {
            string[] cords = Lobby_Position.Split(',').ToArray();
            int.TryParse(cords[0], out int x);
            int.TryParse(cords[2], out int z);
            if ((x - _x) * (x - _x) + (z - _z) * (z - _z) <= Lobby_Size * Lobby_Size)
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
                if (PersistentOperations.PvEViolations.ContainsKey(_cInfo2.entityId))
                {
                    PersistentOperations.PvEViolations.TryGetValue(_cInfo2.entityId, out int _violations);
                    _violations++;
                    PersistentOperations.PvEViolations[_cInfo2.entityId] = _violations;
                    if (PersistentOperations.Jail_Violation > 0 && _violations == PersistentOperations.Jail_Violation)
                    {
                        PersistentOperations.JailPlayer(_cInfo2);
                    }
                    if (PersistentOperations.Kill_Violation > 0 && _violations == PersistentOperations.Kill_Violation)
                    {
                        PersistentOperations.KillPlayer(_cInfo2);
                    }
                    if (PersistentOperations.Kick_Violation > 0 && _violations == PersistentOperations.Kick_Violation)
                    {
                        PersistentOperations.KickPlayer(_cInfo2);
                    }
                    else if (PersistentOperations.Ban_Violation > 0 && _violations == PersistentOperations.Ban_Violation)
                    {
                        PersistentOperations.BanPlayer(_cInfo2);
                    }
                }
                else
                {
                    PersistentOperations.PvEViolations.Add(_cInfo2.entityId, 1);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Lobby.PvEViolation: {0}", e.Message));
            }
            return true;
        }
    }
}

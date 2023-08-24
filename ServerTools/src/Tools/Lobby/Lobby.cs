using System;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Lobby
    {
        public static bool IsEnabled = false, Return = false, Reserved_Only = false, PvE = true, Bloodmoon = false,
            Player_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 5, Lobby_Size = 25, Command_Cost = 0, Player_Killing_Mode = 0;
        public static string Lobby_Position = "0,0,0", Command_lobbyback = "lobbyback", Command_lback = "lback", 
            Command_set = "setlobby", Command_lobby = "lobby";
        public static float[] LobbyBounds = new float[6];

        public static void SetBounds(string _position)
        {
            if (_position != "0,0,0" && _position != "" && _position.Contains(","))
            {
                string[] cords = _position.Split(',').ToArray();
                if (int.TryParse(cords[0], out int x))
                {
                    if (int.TryParse(cords[1], out int y))
                    {
                        if (int.TryParse(cords[2], out int z))
                        {
                            Lobby_Position = _position;
                            Bounds bounds = new Bounds();
                            bounds.center = new Vector3(x, y, z);
                            int size = Lobby_Size * 2;
                            bounds.size = new Vector3(size, size, size);
                            LobbyBounds[0] = bounds.min.x;
                            LobbyBounds[1] = bounds.min.y;
                            LobbyBounds[2] = bounds.min.z;
                            LobbyBounds[3] = bounds.max.x;
                            LobbyBounds[4] = bounds.max.y;
                            LobbyBounds[5] = bounds.max.z;
                            Log.Out(string.Format("[SERVERTOOLS] Lobby has been set to position '{0}'", _position));
                            return;
                        }
                    }
                }
            }
            Log.Out(string.Format("[SERVERTOOLS] Unable to set lobby bounds using position '{0}'", _position));
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (!Bloodmoon && GeneralOperations.IsBloodmoon())
            {
                Phrases.Dict.TryGetValue("Lobby13", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (Reserved_Only && ReservedSlots.IsEnabled && !ReservedSlots.IsReserved(_cInfo))
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
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                {
                    int delay = Delay_Between_Uses / 2;
                    Time(_cInfo, timepassed, delay);
                    return;
                }
                Time(_cInfo, timepassed, Delay_Between_Uses);
            }
        }

        private static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Command_Cost > 0)
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
            int currency = 0, bankCurrency = 0, cost = Command_Cost;
            if (Wallet.IsEnabled)
            {
                currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
            }
            if (Bank.IsEnabled && Bank.Direct_Payment)
            {
                bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
            }
            if (currency + bankCurrency >= cost)
            {
                if (currency > 0)
                {
                    if (currency < cost)
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                        cost -= currency;
                        Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                    }
                    else
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                    }
                }
                else
                {
                    Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                }
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
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (IsLobby(player.position))
                    {
                        Phrases.Dict.TryGetValue("Lobby10", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
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
                                if (Return)
                                {
                                    Vector3 position = player.GetPosition();
                                    Phrases.Dict.TryGetValue("Lobby3", out string phrase);
                                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    phrase = phrase.Replace("{Command_lback}", Command_lback);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    int x = (int)position.x;
                                    int y = (int)position.y;
                                    int z = (int)position.z;
                                    string pposition = x + "," + y + "," + z;
                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LobbyReturnPos = pposition;
                                }
                                if (!TeleportDetector.Omissions.Contains(_cInfo.entityId))
                                {
                                    TeleportDetector.Omissions.Add(_cInfo.entityId);
                                }
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(i, j, k), null, false));
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastLobby = DateTime.Now;
                                PersistentContainer.DataChange = true;
                            }
                        }
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
            EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
            if (player != null)
            {
                if (IsLobby(player.position))
                {
                    string lastPos = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LobbyReturnPos;
                    if (lastPos != null && lastPos != "")
                    {
                        string[] returnPos = lastPos.Split(',');
                        int.TryParse(returnPos[0], out int x);
                        int.TryParse(returnPos[1], out int y);
                        int.TryParse(returnPos[2], out int z);
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
                else
                {
                    Phrases.Dict.TryGetValue("Lobby7", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static bool IsLobby(Vector3 _position)
        {
            if (_position.x >= LobbyBounds[0] && _position.y >= LobbyBounds[1] && _position.z >= LobbyBounds[2] &&
                _position.x <= LobbyBounds[3] && _position.y <= LobbyBounds[4] && _position.z <= LobbyBounds[5])
            {
                return true;
            }
            return false;
        }

        public static void PvEViolation(ClientInfo _cInfo)
        {
            try
            {
                if (GeneralOperations.PvEViolations.ContainsKey(_cInfo.entityId))
                {
                    GeneralOperations.PvEViolations.TryGetValue(_cInfo.entityId, out int _violations);
                    GeneralOperations.PvEViolations[_cInfo.entityId] += 1;
                    if (TooManyViolations(_cInfo, _violations + 1))
                    {
                        return;
                    }
                }
                else
                {
                    GeneralOperations.PvEViolations.Add(_cInfo.entityId, 1);
                    if (TooManyViolations(_cInfo, 1))
                    {
                        return;
                    }
                }
                Phrases.Dict.TryGetValue("Lobby11", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Lobby.PvEViolation: {0}", e.Message);
            }
        }

        public static bool TooManyViolations(ClientInfo _cInfo, int _violations)
        {
            if (GeneralOperations.Jail_Violation > 0 && _violations == GeneralOperations.Jail_Violation)
            {
                GeneralOperations.PvEViolations.Remove(_cInfo.entityId);
                GeneralOperations.JailPlayer(_cInfo);
                return true;
            }
            if (GeneralOperations.Kill_Violation > 0 && _violations == GeneralOperations.Kill_Violation)
            {
                GeneralOperations.PvEViolations.Remove(_cInfo.entityId);
                GeneralOperations.KillPlayer(_cInfo, 1);
                return true;
            }
            if (GeneralOperations.Kick_Violation > 0 && _violations == GeneralOperations.Kick_Violation)
            {
                GeneralOperations.PvEViolations.Remove(_cInfo.entityId);
                Phrases.Dict.TryGetValue("Lobby15", out string phrase);
                GeneralOperations.KickPlayer(_cInfo, phrase);
                return true;
            }
            else if (GeneralOperations.Ban_Violation > 0 && _violations == GeneralOperations.Ban_Violation)
            {
                GeneralOperations.PvEViolations.Remove(_cInfo.entityId);

                GeneralOperations.BanPlayer(_cInfo, "");
                return true;
            }
            return false;
        }
    }
}

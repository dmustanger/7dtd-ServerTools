using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class Homes
    {
        public static bool IsEnabled = false, Player_Check = false, Zombie_Check = false, Vehicle_Check = false, Return = false;
        public static int Delay_Between_Uses = 0, Max_Homes = 2, Reserved_Max_Homes = 4, Command_Cost = 0;
        public static string Command_home = "home", Command_fhome = "fhome", Command_save = "home save", Command_delete = "home del", Command_go = "go home",
            Command_set = "sethome";
        public static Dictionary<int, DateTime> Invite = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> FriendPosition = new Dictionary<int, string>();

        public static void List(ClientInfo _cInfo)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes != null && PersistentContainer.Instance.Players[_cInfo.playerId].Homes.Count > 0)
                {
                    if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                    {
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                        if (DateTime.Now < _dt)
                        {
                            ListResult(_cInfo, Reserved_Max_Homes);
                            return;
                        }
                    }
                }
                ListResult(_cInfo, Max_Homes);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.List: {0}", e.Message));
            }
        }

        public static void ListResult(ClientInfo _cInfo, int _homeLimit)
        {
            try
            {
                Dictionary<string, string> _homes = new Dictionary<string, string>();
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes != null)
                {
                    _homes = PersistentContainer.Instance.Players[_cInfo.playerId].Homes;
                }
                int _count = 0;
                if (_homes.Count > 0)
                {
                    foreach (var _home in _homes)
                    {
                        _count += 1;
                        if (_count <= _homeLimit)
                        {
                            Phrases.Dict.TryGetValue("Homes2", out string _phrase);
                            _phrase = _phrase.Replace("{Name}", _home.Key);
                            _phrase = _phrase.Replace("{Position}", _home.Value);
                            _phrase = _phrase.Replace("{Cost}", Command_Cost.ToString());
                            _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes1", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.ListResult: {0}", e.Message));
            }
        }

        public static void TeleDelay(ClientInfo _cInfo, string _home, bool _friends)
        {
            try
            {
                if (!Event.Teams.ContainsKey(_cInfo.playerId))
                {
                    if (Delay_Between_Uses < 1)
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes != null && PersistentContainer.Instance.Players[_cInfo.playerId].Homes.Count > 0)
                        {
                            Checks(_cInfo, _home, _friends);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Homes1", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].LastHome != null)
                        {
                            DateTime _lastWaypoint = PersistentContainer.Instance.Players[_cInfo.playerId].LastHome;
                            TimeSpan varTime = DateTime.Now - _lastWaypoint;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        int _delay = Delay_Between_Uses / 2;
                                        Time(_cInfo, _home, _timepassed, _delay, _friends);
                                        return;
                                    }
                                }
                            }
                            Time(_cInfo, _home, _timepassed, Delay_Between_Uses, _friends);
                        }
                        else
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes != null && PersistentContainer.Instance.Players[_cInfo.playerId].Homes.Count > 0)
                            {
                                Checks(_cInfo, _home, false);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Homes1", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes3", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.TeleDelay: {0}", e.Message));
            }
        }

        private static void Time(ClientInfo _cInfo, string _homeName, int _timepassed, int _delay, bool _friends)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    Checks(_cInfo, _homeName, _friends);
                }
                else
                {
                    int _timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Homes4", out string _phrase);
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    _phrase = _phrase.Replace("{Value}", _timeleft.ToString());
                    _phrase = _phrase.Replace("{Command_home}", Command_home);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.Time: {0}", e.Message));
            }
        }

        private static void Checks(ClientInfo _cInfo, string _homeName, bool _friends)
        {
            try
            {
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (_player != null)
                {
                    if (Vehicle_Check)
                    {
                        Entity _attachedEntity = _player.AttachedToEntity;
                        if (_attachedEntity != null)
                        {
                            Phrases.Dict.TryGetValue("Teleport3", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                    if (Player_Check)
                    {
                        if (Teleportation.PCheck(_cInfo, _player))
                        {
                            return;
                        }
                    }
                    if (Zombie_Check)
                    {
                        if (Teleportation.ZCheck(_cInfo, _player))
                        {
                            return;
                        }
                    }
                    CommandCost(_cInfo, _homeName, _player.GetPosition(), _friends);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.Checks: {0}", e.Message));
            }
        }

        private static void CommandCost(ClientInfo _cInfo, string _homeName, Vector3 _position, bool _friends)
        {
            try
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    if (Wallet.GetCurrency(_cInfo.playerId) >= Command_Cost)
                    {
                        Exec(_cInfo, _homeName, _position, _friends);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Homes6", out string _phrase);
                        _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Exec(_cInfo, _homeName, _position, _friends);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.CommandCost: {0}", e.Message));
            }
        }

        private static void Exec(ClientInfo _cInfo, string _homeName, Vector3 _position, bool _friends)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes != null && PersistentContainer.Instance.Players[_cInfo.playerId].Homes.ContainsKey(_homeName))
                {
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes.TryGetValue(_homeName, out string _homePos))
                    {
                        string[] _cords = _homePos.Split(',');
                        int.TryParse(_cords[0], out int _x);
                        int.TryParse(_cords[1], out int _y);
                        int.TryParse(_cords[2], out int _z);
                        if (_friends)
                        {
                            FriendInvite(_cInfo, _position, _homePos);
                        }
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastHome = DateTime.Now;
                        PersistentContainer.DataChange = true;
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            Wallet.RemoveCurrency(_cInfo.playerId, Command_Cost);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Homes7", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes7", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.Exec: {0}", e.Message));
            }
        }

        public static void SaveClaimCheck(ClientInfo _cInfo, string _home)
        {
            try
            {
                if (!Event.Teams.ContainsKey(_cInfo.playerId))
                {
                    World world = GameManager.Instance.World;
                    EntityPlayer _player = world.Players.dict[_cInfo.entityId];
                    if (_player != null)
                    {
                        Vector3 _position = _player.GetPosition();
                        if (PersistentOperations.ClaimedByAllyOrSelf(_cInfo.playerId, new Vector3i(_position.x, _position.y, _position.z)))
                        {
                            ReservedCheck(_cInfo, _home);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Homes8", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes3", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.SaveClaimCheck: {0}", e.Message));
            }
        }

        private static void ReservedCheck(ClientInfo _cInfo, string _home)
        {
            try
            {
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime dt);
                    if (DateTime.Now < dt)
                    {
                        SaveHome(_cInfo, _home, Reserved_Max_Homes);
                        return;
                    }
                }
                SaveHome(_cInfo, _home, Max_Homes);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.ReservedCheck: {0}", e.Message));
            }
        }

        private static void SaveHome(ClientInfo _cInfo, string _homeName, int _homeTotal)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_homeName))
                {
                    Phrases.Dict.TryGetValue("Homes11", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes != null && PersistentContainer.Instance.Players[_cInfo.playerId].Homes.Count > 0)
                {
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes.Count < _homeTotal)
                    {
                        EntityPlayer player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (player != null)
                        {
                            Vector3 position = player.GetPosition();
                            int x = (int)position.x;
                            int y = (int)position.y;
                            int z = (int)position.z;
                            string _wposition = x + "," + y + "," + z;
                            if (!PersistentContainer.Instance.Players[_cInfo.playerId].Homes.ContainsKey(_homeName))
                            {
                                PersistentContainer.Instance.Players[_cInfo.playerId].Homes.Add(_homeName, _wposition);
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Homes9", out string _phrase);
                                _phrase = _phrase.Replace("{Name}", _homeName);
                                _phrase = _phrase.Replace("{Position}", _wposition);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Homes10", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Homes11", out string _phrase);
                        _phrase = _phrase.Replace("{Value}", _homeTotal.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else if (_homeTotal > 0)
                {
                    EntityPlayer player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (player != null)
                    {
                        Dictionary<string, string> homes = new Dictionary<string, string>();
                        Vector3 position = player.GetPosition();
                        int x = (int)position.x;
                        int y = (int)position.y;
                        int z = (int)position.z;
                        string wposition = x + "," + y + "," + z;
                        homes.Add(_homeName, wposition);
                        PersistentContainer.Instance.Players[_cInfo.playerId].Homes = homes;
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("Homes12", out string phrase);
                        phrase = phrase.Replace("{Name}", _homeName);
                        phrase = phrase.Replace("{Position}", wposition);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes11", out string phrase);
                    phrase = phrase.Replace("{Value}", _homeTotal.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.SaveHome: {0}", e.Message));
            }
        }

        public static void DelHome(ClientInfo _cInfo, string _homeName)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes != null && PersistentContainer.Instance.Players[_cInfo.playerId].Homes.ContainsKey(_homeName))
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].Homes.Remove(_homeName);
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("Homes13", out string phrase);
                    phrase = phrase.Replace("{Name}", _homeName);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes7", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.DelHome: {0}", e.Message));
            }
        }

        public static void FriendInvite(ClientInfo _cInfo, Vector3 _position, string _destination)
        {
            try
            {
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (player != null)
                {
                    List<ClientInfo> clientList = PersistentOperations.ClientList();
                    if (clientList != null)
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            EntityPlayer player2 = GameManager.Instance.World.Players.dict[cInfo2.entityId];
                            if (player2 != null)
                            {
                                if (player.IsFriendsWith(player2))
                                {
                                    if ((x - (int)player2.position.x) * (x - (int)player2.position.x) + (z - (int)player2.position.z) * (z - (int)player2.position.z) <= 20 * 20)
                                    {
                                        Phrases.Dict.TryGetValue("Homes14", out string phrase);
                                        phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                                        phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                        phrase = phrase.Replace("{Command_go}", Command_go);
                                        ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        Phrases.Dict.TryGetValue("Homes15", out phrase);
                                        phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        if (Invite.ContainsKey(cInfo2.entityId))
                                        {
                                            Invite.Remove(cInfo2.entityId);
                                            FriendPosition.Remove(cInfo2.entityId);
                                        }
                                        Invite.Add(cInfo2.entityId, DateTime.Now);
                                        FriendPosition.Add(cInfo2.entityId, _destination);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.FriendInvite: {0}", e.Message));
            }
        }

        public static void FriendHome(ClientInfo _cInfo)
        {
            try
            {
                Invite.TryGetValue(_cInfo.entityId, out DateTime dt);
                {
                    TimeSpan varTime = DateTime.Now - dt;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timepassed = (int)fractionalMinutes;
                    if (timepassed <= 2)
                    {
                        FriendPosition.TryGetValue(_cInfo.entityId, out string pos);
                        {
                            string[] cords = pos.Split(',');
                            int.TryParse(cords[0], out int x);
                            int.TryParse(cords[1], out int y);
                            int.TryParse(cords[2], out int z);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                            Invite.Remove(_cInfo.entityId);
                            FriendPosition.Remove(_cInfo.entityId);
                        }
                    }
                    else
                    {
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                        Phrases.Dict.TryGetValue("Homes16", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.FriendHome: {0}", e.Message));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ServerTools
{
    public class Homes
    {
        public static bool IsEnabled = false, PvP_Check = false, Zombie_Check = false, Vehicle_Check = false, Return = false;
        public static int Delay_Between_Uses = 0, Max_Homes = 2, Reserved_Max_Homes = 4, Command_Cost = 0;
        public static string Command1 = "home", Command2 = "fhome", Command3 = "home save", Command4 = "home del", Command5 = "go home",
            Command6 = "sethome";
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
                        DateTime _dt;
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                        if (DateTime.Now < _dt)
                        {
                            ListResult(_cInfo, Reserved_Max_Homes);
                            return;
                        }
                    }
                    ListResult(_cInfo, Max_Homes);
                }
                else
                {
                    Phrases.Dict.TryGetValue(731, out string _phrase731);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase731 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
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
                Dictionary<string, string> _homes = PersistentContainer.Instance.Players[_cInfo.playerId].Homes;
                int _count = 1;
                foreach (var _home in _homes)
                {
                    if (_count <= _homeLimit)
                    {
                        string[] _cords = _home.Value.Split(',');
                        int.TryParse(_cords[0], out int _x);
                        int.TryParse(_cords[1], out int _y);
                        int.TryParse(_cords[2], out int _z);
                        _count++;
                        Phrases.Dict.TryGetValue(732, out string _phrase732);
                        _phrase732 = _phrase732.Replace("{Name}", _home.Key);
                        _phrase732 = _phrase732.Replace("{Value}", _x.ToString());
                        _phrase732 = _phrase732.Replace("{Value2}", _y.ToString());
                        _phrase732 = _phrase732.Replace("{Value3}", _z.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase732 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
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
                            Phrases.Dict.TryGetValue(731, out string _phrase731);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase731 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
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
                                Phrases.Dict.TryGetValue(731, out string _phrase731);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase731 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(733, out string _phrase733);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase733 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    Phrases.Dict.TryGetValue(734, out string _phrase734);
                    _phrase734 = _phrase734.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase734 = _phrase734.Replace("{DelayBetweenUses}", _delay.ToString());
                    _phrase734 = _phrase734.Replace("{Value}", _timeleft.ToString());
                    _phrase734 = _phrase734.Replace("{Command1}", Command1);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase734 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            Phrases.Dict.TryGetValue(853, out string _phrase853);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase853 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                    if (PvP_Check)
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
                    if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                    {
                        Exec(_cInfo, _homeName, _position, _friends);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(736, out string _phrase736);
                        _phrase736 = _phrase736.Replace("{CoinName}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase736 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(737, out string _phrase737);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase737 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(737, out string _phrase737);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase737 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            Phrases.Dict.TryGetValue(738, out string _phrase738);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase738 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(733, out string _phrase733);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase733 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                    if (DateTime.Now < _dt)
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
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes != null && PersistentContainer.Instance.Players[_cInfo.playerId].Homes.Count > 0)
                {
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].Homes.Count < _homeTotal)
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (_player != null)
                        {
                            Vector3 _position = _player.GetPosition();
                            int _x = (int)_position.x;
                            int _y = (int)_position.y;
                            int _z = (int)_position.z;
                            string _wposition = _x + "," + _y + "," + _z;
                            if (!PersistentContainer.Instance.Players[_cInfo.playerId].Homes.ContainsKey(_homeName))
                            {
                                PersistentContainer.Instance.Players[_cInfo.playerId].Homes.Add(_homeName, _wposition);
                                Phrases.Dict.TryGetValue(739, out string _phrase739);
                                _phrase739 = _phrase739.Replace("{Name}", _homeName);
                                _phrase739 = _phrase739.Replace("{Position}", _wposition);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase739 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue(740, out string _phrase740);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase740 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(741, out string _phrase741);
                        _phrase741 = _phrase741.Replace("{Value}", _homeTotal.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase741 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player != null)
                    {
                        Dictionary<string, string> _homes = new Dictionary<string, string>();
                        Vector3 _position = _player.GetPosition();
                        int _x = (int)_position.x;
                        int _y = (int)_position.y;
                        int _z = (int)_position.z;
                        string _wposition = _x + "," + _y + "," + _z;
                        _homes.Add(_homeName, _wposition);
                        PersistentContainer.Instance.Players[_cInfo.playerId].Homes = _homes;
                        Phrases.Dict.TryGetValue(742, out string _phrase742);
                        _phrase742 = _phrase742.Replace("{Name}", _homeName);
                        _phrase742 = _phrase742.Replace("{Position}", _wposition);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase742 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
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
                    Phrases.Dict.TryGetValue(743, out string _phrase743);
                    _phrase743 = _phrase743.Replace("{Name}", _homeName);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase743 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(737, out string _phrase737);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase737 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = _cInfoList[i];
                    EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                    if (_player2 != null)
                    {
                        if (_player.IsFriendsWith(_player2))
                        {
                            if ((x - (int)_player2.position.x) * (x - (int)_player2.position.x) + (z - (int)_player2.position.z) * (z - (int)_player2.position.z) <= 10 * 10)
                            {
                                Phrases.Dict.TryGetValue(744, out string _phrase744);
                                _phrase744 = _phrase744.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase744 = _phrase744.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                _phrase744 = _phrase744.Replace("{Command5}", Command5);
                                ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase744 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue(745, out string _phrase745);
                                _phrase745 = _phrase745.Replace("{PlayerName}", _cInfo2.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase745 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                if (Invite.ContainsKey(_cInfo2.entityId))
                                {
                                    Invite.Remove(_cInfo2.entityId);
                                    FriendPosition.Remove(_cInfo2.entityId);
                                }
                                Invite.Add(_cInfo2.entityId, DateTime.Now);
                                FriendPosition.Add(_cInfo2.entityId, _destination);
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
                Invite.TryGetValue(_cInfo.entityId, out DateTime _dt);
                {
                    TimeSpan varTime = DateTime.Now - _dt;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed <= 2)
                    {
                        FriendPosition.TryGetValue(_cInfo.entityId, out string _pos);
                        {
                            string[] _cords = _pos.Split(',');
                            int.TryParse(_cords[0], out int _x);
                            int.TryParse(_cords[1], out int _y);
                            int.TryParse(_cords[2], out int _z);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                            Invite.Remove(_cInfo.entityId);
                            FriendPosition.Remove(_cInfo.entityId);
                        }
                    }
                    else
                    {
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                        Phrases.Dict.TryGetValue(746, out string _phrase746);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase746 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
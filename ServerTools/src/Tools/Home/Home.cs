using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class Home
    {
        public static bool IsEnabled = false, Home2_Enabled = false, Home2_Reserved_Only = false, Home2_Delay = false, Player_Check = false, 
            Zombie_Check = false, Vehicle_Check = false, Return = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static string Command1 = "sethome", Command2 = "home", Command3 = "fhome", Command4 = "delhome", Command5 = "sethome2", 
            Command6 = "home2", Command7 = "fhome2", Command8 = "delhome2", Command9 = "go";
        public static Dictionary<int, DateTime> Invite = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> FriendPosition = new Dictionary<int, string>();

        public static void SetHome1(ClientInfo _cInfo)
        {
            if (!Event.Teams.ContainsKey(_cInfo.playerId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    Vector3 _position = _player.GetPosition();
                    int x = (int)_position.x;
                    int y = (int)_position.y;
                    int z = (int)_position.z;
                    Vector3i _vec3i = new Vector3i(x, y, z);
                    if (PersistentOperations.ClaimedByAllyOrSelf(_cInfo.playerId, _vec3i))
                    {
                        string _sposition = x + "," + y + "," + z;
                        PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1 = _sposition;
                        Phrases.Dict.TryGetValue(732, out string _phrase732);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase732 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(739, out string _phrase739);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase739 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(740, out string _phrase740);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase740 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec1(ClientInfo _cInfo)
        {
            if (!Event.Teams.ContainsKey(_cInfo.playerId))
            {
                if (string.IsNullOrEmpty(PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1))
                {
                    Phrases.Dict.TryGetValue(733, out string _phrase733);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase733 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _homePos = PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Checks(_cInfo))
                        {
                            Cost1(_cInfo, _homePos);
                        }
                    }
                    else
                    {
                        DateTime _lastsethome;
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1 == null)
                        {
                            _lastsethome = DateTime.Now;
                        }
                        else
                        {
                            _lastsethome = PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1;
                        }
                        TimeSpan varTime = DateTime.Now - _lastsethome;
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
                                    Time1(_cInfo, _homePos, _timepassed, _delay);
                                    return;
                                }
                            }
                        }
                        Time1(_cInfo, _homePos, _timepassed, Delay_Between_Uses);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(740, out string _phrase740);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase740 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void Time1(ClientInfo _cInfo, string _pos, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Checks(_cInfo))
                {
                    Cost1(_cInfo, _pos);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue(735, out string _phrase735);
                _phrase735 = _phrase735.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase735 = _phrase735.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase735 = _phrase735.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase735 = _phrase735.Replace("{Command2}", Command2);
                _phrase735 = _phrase735.Replace("{Command6}", Command6);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase735 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void Cost1(ClientInfo _cInfo, string _pos)
        {
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                {
                    Home1(_cInfo, _pos);
                }
                else
                {
                    Phrases.Dict.TryGetValue(741, out string _phrase741);
                    _phrase741 = _phrase741.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase741 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Home1(_cInfo, _pos);
            }
        }

        private static void Home1(ClientInfo _cInfo, string _pos)
        {
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out int x);
            int.TryParse(_cords[1], out int y);
            int.TryParse(_cords[2], out int z);
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            }
            PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1 = DateTime.Now;
        }

        public static void DelHome1(ClientInfo _cInfo)
        {
            if (!string.IsNullOrEmpty(PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1))
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1 = "";
                Phrases.Dict.TryGetValue(742, out string _phrase742);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase742 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue(743, out string _phrase743);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase743 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SetHome2(ClientInfo _cInfo)
        {
            if (!Event.Teams.ContainsKey(_cInfo.playerId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                Vector3i _vec3i = new Vector3i(x, y, z);
                if (PersistentOperations.ClaimedByAllyOrSelf(_cInfo.playerId, _vec3i))
                {
                    string _sposition = x + "," + y + "," + z;
                    PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2 = _sposition;
                    Phrases.Dict.TryGetValue(736, out string _phrase736);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase736 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(739, out string _phrase739);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase739 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(740, out string _phrase740);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase740 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec2(ClientInfo _cInfo)
        {
            if (!Event.Teams.ContainsKey(_cInfo.playerId))
            {
                if (string.IsNullOrEmpty(PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2))
                {
                    Phrases.Dict.TryGetValue(737, out string _phrase737);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase737 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _homePos2 = PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Checks(_cInfo))
                        {
                            Cost2(_cInfo, _homePos2);
                        }
                    }
                    else
                    {
                        DateTime _lastsethome;
                        if (Home2_Delay)
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.playerId].LastHome2 != null)
                            {
                                _lastsethome = PersistentContainer.Instance.Players[_cInfo.playerId].LastHome2;
                            }
                            else
                            {
                                _lastsethome = DateTime.Now;
                            }
                        }
                        else
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1 != null)
                            {
                                _lastsethome = PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1;
                            }
                            else
                            {
                                _lastsethome = DateTime.Now;
                            }
                        }
                        TimeSpan varTime = DateTime.Now - _lastsethome;
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
                                    Time2(_cInfo, _homePos2, _timepassed, _delay);
                                    return;
                                }
                            }
                        }
                        Time2(_cInfo, _homePos2, _timepassed, Delay_Between_Uses);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(740, out string _phrase740);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase740 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Time2(ClientInfo _cInfo, string _pos, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Checks(_cInfo))
                {
                    Cost2(_cInfo, _pos);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue(735, out string _phrase735);
                _phrase735 = _phrase735.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase735 = _phrase735.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase735 = _phrase735.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase735 = _phrase735.Replace("{Command2}", Command2);
                _phrase735 = _phrase735.Replace("{Command6}", Command6);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase735 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void Cost2(ClientInfo _cInfo, string _pos)
        {
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                {
                    Home2(_cInfo, _pos);
                }
                else
                {
                    Phrases.Dict.TryGetValue(741, out string _phrase741);
                    _phrase741 = _phrase741.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase741 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Home2(_cInfo, _pos);
            }
        }

        private static void Home2(ClientInfo _cInfo, string _pos)
        {
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out int _x);
            int.TryParse(_cords[1], out int _y);
            int.TryParse(_cords[2], out int _z);
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            }
            PersistentContainer.Instance.Players[_cInfo.playerId].LastHome2 = DateTime.Now;
        }

        public static void DelHome2(ClientInfo _cInfo)
        {
            if (!string.IsNullOrEmpty(PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2))
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2 = "";
                Phrases.Dict.TryGetValue(738, out string _phrase738);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase738 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue(737, out string _phrase737);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase737 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FExec1(ClientInfo _cInfo)
        {
            if (!Event.Teams.ContainsKey(_cInfo.playerId))
            {
                if (string.IsNullOrEmpty(PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1))
                {
                    Phrases.Dict.TryGetValue(733, out string _phrase733);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase733 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _homePos1 = PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Checks(_cInfo))
                        {
                            FCost1(_cInfo, _homePos1);
                        }
                    }
                    else
                    {
                        DateTime _lastsethome;
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1 == null)
                        {
                            _lastsethome = DateTime.Now;
                        }
                        else
                        {
                            _lastsethome = PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1;
                        }
                        TimeSpan varTime = DateTime.Now - _lastsethome;
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

                                    FTime1(_cInfo, _homePos1, _timepassed, _delay);
                                    return;
                                }
                            }
                        }
                        FTime1(_cInfo, _homePos1, _timepassed, Delay_Between_Uses);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(740, out string _phrase740);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase740 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FTime1(ClientInfo _cInfo, string _pos, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Checks(_cInfo))
                {
                    FCost1(_cInfo, _pos);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue(735, out string _phrase735);
                _phrase735 = _phrase735.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase735 = _phrase735.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase735 = _phrase735.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase735 = _phrase735.Replace("{Command2}", Command2);
                _phrase735 = _phrase735.Replace("{Command6}", Command6);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase735 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FCost1(ClientInfo _cInfo, string _pos)
        {
            if (Wallet.IsEnabled && Command_Cost > 0)
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                {
                    FHome1(_cInfo, _pos);
                }
                else
                {
                    Phrases.Dict.TryGetValue(741, out string _phrase741);
                    _phrase741 = _phrase741.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase741 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                FHome1(_cInfo, _pos);
            }
        }

        private static void FHome1(ClientInfo _cInfo, string _pos)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player != null)
            {
                FriendInvite(_cInfo, _player.position, _pos);
                string[] _cords = _pos.Split(',');
                int.TryParse(_cords[0], out int _x);
                int.TryParse(_cords[1], out int _y);
                int.TryParse(_cords[2], out int _z);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1 = DateTime.Now;
            }
        }

        public static void FExec2(ClientInfo _cInfo)
        {
            if (!Event.Teams.ContainsKey(_cInfo.playerId))
            {
                if (string.IsNullOrEmpty(PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2))
                {
                    Phrases.Dict.TryGetValue(737, out string _phrase737);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase737 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _homePos2 = PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Checks(_cInfo))
                        {
                            FCost2(_cInfo, _homePos2);
                        }
                    }
                    else
                    {
                        DateTime _lastsethome;
                        if (Home2_Delay)
                        {
                            _lastsethome = PersistentContainer.Instance.Players[_cInfo.playerId].LastHome2;
                        }
                        else
                        {
                            _lastsethome = PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1;
                        }
                        TimeSpan varTime = DateTime.Now - _lastsethome;
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
                                    FTime2(_cInfo, _homePos2, _timepassed, _delay);
                                    return;
                                }
                            }
                        }
                        FTime2(_cInfo, _homePos2, _timepassed, Delay_Between_Uses);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(740, out string _phrase740);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase740 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FTime2(ClientInfo _cInfo, string _pos, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Checks(_cInfo))
                {
                    FCost2(_cInfo, _pos);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue(735, out string _phrase735);
                _phrase735 = _phrase735.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase735 = _phrase735.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase735 = _phrase735.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase735 = _phrase735.Replace("{Command2}", Command2);
                _phrase735 = _phrase735.Replace("{Command6}", Command6);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase735 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FCost2(ClientInfo _cInfo, string _pos)
        {
            if (Wallet.IsEnabled && Command_Cost > 0)
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                {
                    FHome2(_cInfo, _pos);
                }
                else
                {
                    Phrases.Dict.TryGetValue(741, out string _phrase741);
                    _phrase741 = _phrase741.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase741 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                FHome2(_cInfo, _pos);
            }
        }

        private static void FHome2(ClientInfo _cInfo, string _pos)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player != null)
            {
                FriendInvite(_cInfo, _player.position, _pos);
                string[] _cords = _pos.Split(',');
                int.TryParse(_cords[0], out int _x);
                int.TryParse(_cords[1], out int _y);
                int.TryParse(_cords[2], out int _z);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                if (Home2_Delay)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].LastHome2 = DateTime.Now;
                }
                else
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1 = DateTime.Now;
                }
            }
        }

        public static void FriendInvite(ClientInfo _cInfo, Vector3 _position, string _destination)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = world.Players.dict[_cInfo.entityId];
            if (_player != null)
            {
                List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = _cInfoList[i];
                    EntityPlayer _player2 = world.Players.dict[_cInfo2.entityId];
                    if (_player2 != null)
                    {
                        if (_player.IsFriendsWith(_player2))
                        {
                            if ((_position.x - _player2.position.x) * (_position.x - _player2.position.x) + (_position.z - _player2.position.z) * (_position.z - _player2.position.z) <= 10f * 10f)
                            {
                                if (Invite.ContainsKey(_cInfo2.entityId))
                                {
                                    Invite.Remove(_cInfo2.entityId);
                                    FriendPosition.Remove(_cInfo2.entityId);
                                }
                                Invite.Add(_cInfo2.entityId, DateTime.Now);
                                FriendPosition.Add(_cInfo2.entityId, _destination);
                                Phrases.Dict.TryGetValue(744, out string _phrase744);
                                _phrase744 = _phrase744.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase744 = _phrase744.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                _phrase744 = _phrase744.Replace("{Command9}", Command9);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase744 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue(745, out string _phrase745);
                                _phrase745 = _phrase745.Replace("{PlayerName}", _cInfo2.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase745 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
        }

        public static void FriendHome(ClientInfo _cInfo)
        {
            DateTime _dt;
            Invite.TryGetValue(_cInfo.entityId, out _dt);
            {
                TimeSpan varTime = DateTime.Now - _dt;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (_timepassed <= 2)
                {
                    string _pos;
                    FriendPosition.TryGetValue(_cInfo.entityId, out _pos);
                    {
                        int x, y, z;
                        string[] _cords = _pos.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
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

        private static bool Checks(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player != null)
            {
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                if (Teleportation.VehicleCheck(_cInfo))
                {
                    return false;
                }
                if (Player_Check)
                {
                    if (Teleportation.PCheck(_cInfo, _player))
                    {
                        return false;
                    }
                }
                if (Zombie_Check)
                {
                    if (Teleportation.ZCheck(_cInfo, _player))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
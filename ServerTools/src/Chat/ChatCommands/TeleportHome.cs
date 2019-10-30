using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    public class TeleportHome
    {
        public static bool IsEnabled = false, Set_Home2_Enabled = false, Set_Home2_Reserved_Only = false, Home2_Delay = false, PvP_Check = false, Zombie_Check = false, Vehicle = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static string Command1 = "sethome", Command2 = "home", Command3 = "fhome", Command4 = "delhome", Command5 = "sethome2", 
            Command6 = "home2", Command7 = "fhome2", Command8 = "delhome2", Command9 = "go";
        public static Dictionary<int, DateTime> Invite = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> FriendPosition = new Dictionary<int, string>();

        public static void SetHome1(ClientInfo _cInfo)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = world.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                Vector3i _vec3i = new Vector3i(x, y, z);
                PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(_player.entityId);
                EnumLandClaimOwner _owner = world.GetLandClaimOwner(_vec3i, _persistentPlayerData);
                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                {
                    string _sposition = x + "," + y + "," + z;
                    PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1 = _sposition;
                    PersistentContainer.Instance.Save();
                    string _phrase10;
                    if (!Phrases.Dict.TryGetValue(10, out _phrase10))
                    {
                        _phrase10 = " your home has been saved.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase10 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _phrase817;
                    if (!Phrases.Dict.TryGetValue(817, out _phrase817))
                    {
                        _phrase817 = " you are not inside your own or a friend's claimed space. You can not save this as your home.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase817 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec1(ClientInfo _cInfo)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                if (Vehicle)
                {
                    Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player.AttachedToEntity != null)
                    {
                        string _phrase826;
                        if (!Phrases.Dict.TryGetValue(826, out _phrase826))
                        {
                            _phrase826 = " you can not teleport home with a vehicle.";
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase826 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        return;
                    }
                }
                string _homePos = PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1;
                if (string.IsNullOrEmpty(_homePos))
                {
                    string _phrase11;
                    if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                    {
                        _phrase11 = " you do not have a home saved.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase11 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            CommandCost1(_cInfo, _homePos);
                        }
                        else
                        {
                            TeleHome1(_cInfo, _homePos);
                        }
                    }
                    else
                    {
                        DateTime _lastsethome = PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1;
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
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Time1(ClientInfo _cInfo, string _pos, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost1(_cInfo, _pos);
                }
                else
                {
                    TeleHome1(_cInfo, _pos);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                string _phrase13;
                if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                {
                    _phrase13 = " you can only use {CommandPrivate}{Command2} or {CommandPrivate}{Command6} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase13 = _phrase13.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase13 = _phrase13.Replace("{Command2}", Command2);
                _phrase13 = _phrase13.Replace("{Command6}", Command6);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost1(ClientInfo _cInfo, string _pos)
        {
            int _currentCoins = Wallet.GetCurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                TeleHome1(_cInfo, _pos);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void TeleHome1(ClientInfo _cInfo, string _pos)
        {
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
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
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            }
            PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1 = DateTime.Now;
            PersistentContainer.Instance.Save();
        }

        public static void DelHome1(ClientInfo _cInfo)
        {
            string _homePos = PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1;
            if (!string.IsNullOrEmpty(_homePos))
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " deleted saved home position.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1 = "";
                PersistentContainer.Instance.Save();
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " no home to delete.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SetHome2(ClientInfo _cInfo)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = world.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                Vector3i _vec3i = new Vector3i(x, y, z);
                PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(_player.entityId);
                EnumLandClaimOwner _owner = world.GetLandClaimOwner(_vec3i, _persistentPlayerData);
                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                {
                    string _sposition = x + "," + y + "," + z;
                    PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2 = _sposition;
                    PersistentContainer.Instance.Save();
                    string _phrase607;
                    if (!Phrases.Dict.TryGetValue(607, out _phrase607))
                    {
                        _phrase607 = " your home2 has been saved.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase607 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _phrase817;
                    if (!Phrases.Dict.TryGetValue(817, out _phrase817))
                    {
                        _phrase817 = " you are not inside your own or a friend's claimed space. You can not save this as your home.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase817 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec2(ClientInfo _cInfo)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                if (Vehicle)
                {
                    Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player.AttachedToEntity != null)
                    {
                        string _phrase826;
                        if (!Phrases.Dict.TryGetValue(826, out _phrase826))
                        {
                            _phrase826 = " you can not teleport home with a vehicle.";
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase826 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        return;
                    }
                }
                string _homePos2 = PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2;
                if (string.IsNullOrEmpty(_homePos2))
                {
                    string _phrase11;
                    if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                    {
                        _phrase11 = " you do not have a home saved.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase11 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            CommandCost2(_cInfo, _homePos2);
                        }
                        else
                        {
                            Home2(_cInfo, _homePos2);
                        }
                    }
                    else
                    {
                        DateTime _lastsethome = new DateTime();
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
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Time2(ClientInfo _cInfo, string _pos, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost2(_cInfo, _pos);
                }
                else
                {
                    Home2(_cInfo, _pos);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                string _phrase13;
                if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                {
                    _phrase13 = " you can only use {CommandPrivate}{Command2} or {CommandPrivate}{Command6} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase13 = _phrase13.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase13 = _phrase13.Replace("{Command2}", Command2);
                _phrase13 = _phrase13.Replace("{Command6}", Command6);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost2(ClientInfo _cInfo, string _pos)
        {
            int currentCoins = Wallet.GetCurrentCoins(_cInfo);
            if (currentCoins >= Command_Cost)
            {
                Home2(_cInfo, _pos);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void Home2(ClientInfo _cInfo, string _pos)
        {
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
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
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            PersistentContainer.Instance.Players[_cInfo.playerId].LastHome2 = DateTime.Now;
            PersistentContainer.Instance.Save();
        }

        public static void DelHome2(ClientInfo _cInfo)
        {
            string _homePos2 = PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2;
            if (!string.IsNullOrEmpty(_homePos2))
            {
                string _phrase609;
                if (!Phrases.Dict.TryGetValue(609, out _phrase609))
                {
                    _phrase609 = " your home2 has been removed.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase609 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2 = "";
                PersistentContainer.Instance.Save();
            }
            else
            {
                string _phrase608;
                if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                {
                    _phrase608 = " you do not have a home2 saved.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase608 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FExec1(ClientInfo _cInfo)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                if (Vehicle)
                {
                    Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player.AttachedToEntity != null)
                    {
                        string _phrase826;
                        if (!Phrases.Dict.TryGetValue(826, out _phrase826))
                        {
                            _phrase826 = " you can not teleport home with a vehicle.";
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase826 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        return;
                    }
                }
                string _homePos1 = PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition1;
                if (string.IsNullOrEmpty(_homePos1))
                {
                    string _phrase11;
                    if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                    {
                        _phrase11 = " you do not have a home saved.";
                    }
                    _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase11 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            FCommandCost1(_cInfo, _homePos1);
                        }
                        else
                        {
                            FHome1(_cInfo, _homePos1);
                        }
                    }
                    else
                    {
                        DateTime _lastsethome = PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1;
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
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FTime1(ClientInfo _cInfo, string _pos, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    FCommandCost1(_cInfo, _pos);
                }
                else
                {
                    FHome1(_cInfo, _pos);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                string _phrase13;
                if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                {
                    _phrase13 = " you can only use {CommandPrivate}{Command2} or {CommandPrivate}{Command6} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase13 = _phrase13.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase13 = _phrase13.Replace("{Command2}", Command2);
                _phrase13 = _phrase13.Replace("{Command6}", Command6);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FCommandCost1(ClientInfo _cInfo, string _pos)
        {
            int _currentCoins = Wallet.GetCurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                FHome1(_cInfo, _pos);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void FHome1(ClientInfo _cInfo, string _pos)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
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
            FriendInvite(_cInfo, _player.position, _pos);
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1 = DateTime.Now;
            PersistentContainer.Instance.Save();
            string _phrase818;
            if (!Phrases.Dict.TryGetValue(818, out _phrase818))
            {
                _phrase818 = " you are traveling home.";
            }
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase818 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void FExec2(ClientInfo _cInfo)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                if (Vehicle)
                {
                    Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    Entity _attachedEntity = _player.AttachedToEntity;
                    if (_attachedEntity != null)
                    {
                        string _phrase826;
                        if (!Phrases.Dict.TryGetValue(826, out _phrase826))
                        {
                            _phrase826 = " you can not teleport home with a vehicle.";
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase826 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        return;
                    }
                }
                string _homePos2 = PersistentContainer.Instance.Players[_cInfo.playerId].HomePosition2;
                if (string.IsNullOrEmpty(_homePos2))
                {
                    string _phrase608;
                    if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                    {
                        _phrase608 = " you do not have a home2 saved.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase608 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            FCommandCost2(_cInfo, _homePos2);
                        }
                        else
                        {
                            FHome2(_cInfo, _homePos2);
                        }
                    }
                    else
                    {
                        DateTime _lastsethome = new DateTime();
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
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FTime2(ClientInfo _cInfo, string _pos, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    FCommandCost2(_cInfo, _pos);
                }
                else
                {
                    FHome2(_cInfo, _pos);
                }
            }
            else
            {

                int _timeleft = _delay - _timepassed;
                string _phrase815;
                if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                {
                    _phrase815 = " you can only use {CommandPrivate}{Command3} or {CommandPrivate}{Command7} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase815 = _phrase815.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase815 = _phrase815.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase815 = _phrase815.Replace("{Command3}", Command3);
                _phrase815 = _phrase815.Replace("{Command7}", Command7);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase815 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FCommandCost2(ClientInfo _cInfo, string _pos)
        {
            int _currentCoins = Wallet.GetCurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                FHome2(_cInfo, _pos);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void FHome2(ClientInfo _cInfo, string _pos)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            FriendInvite(_cInfo, _player.position, _pos);
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
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
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            if (Home2_Delay)
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].LastHome1 = DateTime.Now;
            }
            else
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].LastHome2 = DateTime.Now;
            }
            PersistentContainer.Instance.Save();
            string _phrase818;
            if (!Phrases.Dict.TryGetValue(818, out _phrase818))
            {
                _phrase818 = " you are traveling home.";
            }
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase818 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void FriendInvite(ClientInfo _cInfo, Vector3 _position, string _destination)
        {
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            World world = GameManager.Instance.World;
            EntityPlayer _player = world.Players.dict[_cInfo.entityId];
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo2 = _cInfoList[i];
                EntityPlayer _player2 = world.Players.dict[_cInfo2.entityId];
                if (_player2 != null)
                {
                    if (_player.IsFriendsWith(_player2))
                    {
                        if ((x - (int)_player2.position.x) * (x - (int)_player2.position.x) + (z - (int)_player2.position.z) * (z - (int)_player2.position.z) <= 10 * 10)
                        {
                            string _response1 = " your friend {PlayerName} has invited you to their saved home. Type {CommandPrivate}{Command9} to accept the request.";
                            _response1 = _response1.Replace("{PlayerName}", _cInfo.playerName);
                            _response1 = _response1.Replace("{CommandPrivate}", ChatHook.Command_Private);
                            _response1 = _response1.Replace("{Command9}", Command9);
                            string _response2 = " invited your friend {PlayerName} to your saved home.[-]";
                            _response2 = _response2.Replace("{PlayerName}", _cInfo2.playerName);
                            ChatHook.ChatMessage(_cInfo2, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _response1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _response2 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " sending you to your friend's home.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Invite.Remove(_cInfo.entityId);
                    FriendPosition.Remove(_cInfo.entityId);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have run out of time to accept your friend's home invitation.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }
    }
}
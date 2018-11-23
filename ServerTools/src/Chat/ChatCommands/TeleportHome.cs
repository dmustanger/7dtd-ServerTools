using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    public class TeleportHome
    {
        public static bool IsEnabled = false, Set_Home2_Enabled = false, Set_Home2_Donor_Only = false, PvP_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static Dictionary<int, DateTime> Invite = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> FriendPosition = new Dictionary<int, string>();

        public static void SetHome(ClientInfo _cInfo, string _playerName, bool _announce)
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
                    string _sql = string.Format("UPDATE Players SET homeposition = '{0}' WHERE steamid = '{1}'", _sposition, _cInfo.playerId);
                    SQL.FastQuery(_sql);
                    string _phrase10;
                    if (!Phrases.Dict.TryGetValue(10, out _phrase10))
                    {
                        _phrase10 = "your home has been saved.";
                    }
                    if (_announce)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase10 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase10 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    string _phrase817;
                    if (!Phrases.Dict.TryGetValue(817, out _phrase817))
                    {
                        _phrase817 = "you are not inside your own or a friend's claimed space. You can not save this as your home.";
                    }
                    if (_announce)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase817 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase817 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Check(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                string _sql = string.Format("SELECT homeposition, lastsethome FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                DateTime _lastsethome;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _lastsethome);
                string _pos = _result.Rows[0].ItemArray.GetValue(0).ToString();
                _result.Dispose();
                if (_pos == "Unknown")
                {
                    string _phrase11;
                    if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                    {
                        _phrase11 = "you do not have a home saved.";
                    }
                    if (_announce)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase11 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase11 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            CommandCost(_cInfo, _pos, _announce);
                        }
                        else
                        {
                            Home(_cInfo, _pos, _announce);
                        }
                    }
                    else
                    {
                        if (_lastsethome.ToString() == "10/29/2000 7:30:00 AM")
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                CommandCost(_cInfo, _pos, _announce);
                            }
                            else
                            {
                                Home(_cInfo, _pos, _announce);
                            }
                        }
                        else
                        {
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
                                        _donator = true;
                                        int _newDelay = Delay_Between_Uses / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            if (Wallet.IsEnabled && Command_Cost >= 1)
                                            {
                                                CommandCost(_cInfo, _pos, _announce);
                                            }
                                            else
                                            {
                                                Home(_cInfo, _pos, _announce);
                                            }
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase13;
                                            if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                            {
                                                _phrase13 = "you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            }
                                            else
                                            {
                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        CommandCost(_cInfo, _pos, _announce);
                                    }
                                    else
                                    {
                                        Home(_cInfo, _pos, _announce);
                                    }
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase13;
                                    if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                    {
                                        _phrase13 = "you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase13 = _phrase13.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo, string _pos, bool _announce)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                Home(_cInfo, _pos, _announce);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = "you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void Home(ClientInfo _cInfo, string _pos, bool _announce)
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
            Players.NoFlight.Add(_cInfo.entityId);
            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
            string _sql;
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            }
            _sql = string.Format("UPDATE Players SET lastsethome = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
            SQL.FastQuery(_sql);
        }

        public static void DelHome(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            string _sql = string.Format("SELECT homeposition FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _pos = _result.Rows[0].ItemArray.GetValue(0).ToString();
            _result.Dispose();
            if (_pos != "Unknown")
            {
                if (_announce)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", deleted home.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", deleted home.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                _sql = string.Format("UPDATE Players SET homeposition = 'Unknown' WHERE steamid = '{0}'", _cInfo.playerId);
                SQL.FastQuery(_sql);
            }
            else
            {
                if (_announce)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", no home to delete.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", no home to delete.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void SetHome2(ClientInfo _cInfo, string _playerName, bool _announce)
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
                    string _sql = string.Format("UPDATE Players SET homeposition2 = '{0}' WHERE steamid = '{1}'", _sposition, _cInfo.playerId);
                    SQL.FastQuery(_sql);
                    string _phrase607;
                    if (!Phrases.Dict.TryGetValue(607, out _phrase607))
                    {
                        _phrase607 = "your home2 has been saved.";
                    }
                    if (_announce)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase607 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase607 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                if (_owner == EnumLandClaimOwner.None)
                {
                    string _phrase817;
                    if (!Phrases.Dict.TryGetValue(817, out _phrase817))
                    {
                        _phrase817 = "you are not inside your own or a friend's claimed space. You can not save this as your home.";
                    }
                    if (_announce)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase817 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase817 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Check2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                string _sql = string.Format("SELECT homeposition2, lastsethome FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                string _pos = _result.Rows[0].ItemArray.GetValue(0).ToString();
                DateTime _lastsethome;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _lastsethome);
                _result.Dispose();
                if (_pos == "Unknown")
                {
                    string _phrase11;
                    if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                    {
                        _phrase11 = "you do not have a home saved.";
                    }
                    if (_announce)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase11 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase11 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            CommandCost2(_cInfo, _pos, _announce);
                        }
                        else
                        {
                            Home2(_cInfo, _pos, _announce);
                        }
                    }
                    else
                    {
                        if (_lastsethome.ToString() == "10/29/2000 7:30:00 AM")
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                CommandCost2(_cInfo, _pos, _announce);
                            }
                            else
                            {
                                Home2(_cInfo, _pos, _announce);
                            }
                        }
                        else
                        {
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
                                        _donator = true;
                                        int _newDelay = Delay_Between_Uses / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            if (Wallet.IsEnabled && Command_Cost >= 1)
                                            {
                                                CommandCost2(_cInfo, _pos, _announce);
                                            }
                                            else
                                            {
                                                Home2(_cInfo, _pos, _announce);
                                            }
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase13;
                                            if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                            {
                                                _phrase13 = "you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            }
                                            else
                                            {
                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        CommandCost2(_cInfo, _pos, _announce);
                                    }
                                    else
                                    {
                                        Home2(_cInfo, _pos, _announce);
                                    }
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase13;
                                    if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                    {
                                        _phrase13 = "you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase13 = _phrase13.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost2(ClientInfo _cInfo, string _pos, bool _announce)
        {
            int currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (currentCoins >= Command_Cost)
            {
                Home2(_cInfo, _pos, _announce);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = "you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void Home2(ClientInfo _cInfo, string _pos, bool _announce)
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
            Players.NoFlight.Add(_cInfo.entityId);
            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            string _sql = string.Format("UPDATE Players SET lastsethome = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
            SQL.FastQuery(_sql);
        }

        public static void DelHome2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            string _sql = string.Format("SELECT homeposition2 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _pos = _result.Rows[0].ItemArray.GetValue(0).ToString();
            _result.Dispose();
            if (_pos != "Unknown")
            {
                if (_announce)
                {
                    string _phrase609;
                    if (!Phrases.Dict.TryGetValue(609, out _phrase609))
                    {
                        _phrase609 = "your home2 has been removed.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase609 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    string _phrase609;
                    if (!Phrases.Dict.TryGetValue(609, out _phrase609))
                    {
                        _phrase609 = "your home2 has been removed.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase609 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                _sql = string.Format("UPDATE Players SET homeposition2 = 'Unknown' WHERE steamid = '{0}'", _cInfo.playerId);
                SQL.FastQuery(_sql);
            }
            else
            {
                if (_announce)
                {
                    string _phrase608;
                    if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                    {
                        _phrase608 = "you do not have a home2 saved.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase608 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    string _phrase608;
                    if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                    {
                        _phrase608 = "you do not have a home2 saved.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase608 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void FCheck(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                string _sql = string.Format("SELECT homeposition, lastsethome FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                string _pos = _result.Rows[0].ItemArray.GetValue(0).ToString();
                DateTime _lastsethome;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _lastsethome);
                _result.Dispose();
                if (_pos == "Unknown")
                {
                    string _phrase11;
                    if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                    {
                        _phrase11 = "you do not have a home saved.";
                    }
                    _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
                    if (_announce)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase11 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase11 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            FCommandCost(_cInfo, _pos, _announce);
                        }
                        else
                        {
                            FHome(_cInfo, _pos, _announce);
                        }
                    }
                    else
                    {
                        if (_lastsethome.ToString() == "10/29/2000 7:30:00 AM")
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                FCommandCost(_cInfo, _pos, _announce);
                            }
                            else
                            {
                                FHome(_cInfo, _pos, _announce);
                            }
                        }
                        else
                        {
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
                                        _donator = true;
                                        int _newDelay = Delay_Between_Uses / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            if (Wallet.IsEnabled && Command_Cost >= 1)
                                            {
                                                FCommandCost(_cInfo, _pos, _announce);
                                            }
                                            else
                                            {
                                                FHome(_cInfo, _pos, _announce);
                                            }
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase13;
                                            if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                            {
                                                _phrase13 = "you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            }
                                            else
                                            {
                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase13 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        FCommandCost(_cInfo, _pos, _announce);
                                    }
                                    else
                                    {
                                        FHome(_cInfo, _pos, _announce);
                                    }
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase815;
                                    if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                                    {
                                        _phrase815 = "you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase815 = _phrase815.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase815 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase815 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FCommandCost(ClientInfo _cInfo, string _pos, bool _announce)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                FHome(_cInfo, _pos, _announce);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = "you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void FHome(ClientInfo _cInfo, string _pos, bool _announce)
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
            Players.NoFlight.Add(_cInfo.entityId);
            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            string _sql = string.Format("UPDATE Players SET lastsethome = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
            SQL.FastQuery(_sql);
            string _phrase818;
            if (!Phrases.Dict.TryGetValue(818, out _phrase818))
            {
                _phrase818 = "you are traveling home.";
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase818 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void FCheck2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                string _sql = string.Format("SELECT homeposition2, lastsethome FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                string _pos = _result.Rows[0].ItemArray.GetValue(0).ToString();
                DateTime _lastsethome;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _lastsethome);
                _result.Dispose();
                if (_pos == "Unknown")
                {
                    string _phrase608;
                    if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                    {
                        _phrase608 = "you do not have a home2 saved.";
                    }
                    if (_announce)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase608 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase608 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            FCommandCost2(_cInfo, _pos, _announce);
                        }
                        else
                        {
                            FHome2(_cInfo, _pos, _announce);
                        }
                    }
                    else
                    {
                        if (_lastsethome.ToString() == "10/29/2000 7:30:00 AM")
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                FCommandCost2(_cInfo, _pos, _announce);
                            }
                            else
                            {
                                FHome2(_cInfo, _pos, _announce);
                            }
                        }
                        else
                        {
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
                                        _donator = true;
                                        int _newDelay = Delay_Between_Uses / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            if (Wallet.IsEnabled && Command_Cost >= 1)
                                            {
                                                FCommandCost2(_cInfo, _pos, _announce);
                                            }
                                            else
                                            {
                                                FHome2(_cInfo, _pos, _announce);
                                            }
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase815;
                                            if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                                            {
                                                _phrase815 = "you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase815 = _phrase815.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase815 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            }
                                            else
                                            {
                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase815 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        FCommandCost2(_cInfo, _pos, _announce);
                                    }
                                    else
                                    {
                                        FHome2(_cInfo, _pos, _announce);
                                    }
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase815;
                                    if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                                    {
                                        _phrase815 = "you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase815 = _phrase815.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase815 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase815 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you can not use home commands while signed up for or inside an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void FCommandCost2(ClientInfo _cInfo, string _pos, bool _announce)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                FHome2(_cInfo, _pos, _announce);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = "you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void FHome2(ClientInfo _cInfo, string _pos, bool _announce)
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
            Players.NoFlight.Add(_cInfo.entityId);
            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            string _sql = string.Format("UPDATE Players SET lastsethome = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
            SQL.FastQuery(_sql);
            string _phrase818;
            if (!Phrases.Dict.TryGetValue(818, out _phrase818))
            {
                _phrase818 = "you are traveling home.";
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase818 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            string _response1 = "your friend {PlayerName} has invited you to their saved home. Type /go to accept the request.[-]";
                            _response1 = _response1.Replace("{PlayerName}", _cInfo.playerName);
                            string _response2 = "invited your friend {PlayerName} to your saved home.[-]";
                            _response2 = _response2.Replace("{PlayerName}", _cInfo2.playerName);
                            ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _response1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _response2 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        Players.NoFlight.Add(_cInfo.entityId);
                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", sending you to your friend's home.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Invite.Remove(_cInfo.entityId);
                    FriendPosition.Remove(_cInfo.entityId);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you have run out of time to accept your friend's home invitation.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }
    }
}
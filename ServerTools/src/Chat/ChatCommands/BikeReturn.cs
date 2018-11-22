using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class BikeReturn
    {
        public static bool IsEnabled = false, Inside_Claim = true;
        public static int Delay_Between_Uses = 120, Command_Cost = 0;

        public static void BikeDelay(ClientInfo _cInfo, string _playerName)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player.AttachedMainEntity == null)
            {
                bool _donator = false;
                if (Delay_Between_Uses < 1)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo, _player);
                    }
                    else
                    {
                        Exec2(_cInfo, _player);
                    }
                }
                else
                {
                    string _sql = string.Format("SELECT lastBike FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    DateTime _lastBike;
                    DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastBike);
                    _result.Dispose();
                    TimeSpan varTime = DateTime.Now - _lastBike;
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
                                        CommandCost(_cInfo, _player);
                                    }
                                    else
                                    {
                                        Exec2(_cInfo, _player);
                                    }
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase786;
                                    if (!Phrases.Dict.TryGetValue(786, out _phrase786))
                                    {
                                        _phrase786 = "you can only use /bike once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase786 = _phrase786.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase786 = _phrase786.Replace("{TimeRemaining}", _timeleft.ToString());
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase786 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
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
                                CommandCost(_cInfo, _player);
                            }
                            else
                            {
                                Exec2(_cInfo, _player);
                            }
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase786;
                            if (!Phrases.Dict.TryGetValue(786, out _phrase786))
                            {
                                _phrase786 = "{PlayerName} you can only use /bike once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase786 = _phrase786.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase786 = _phrase786.Replace("{TimeRemaining}", _timeleft.ToString());
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase786 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                        }
                    }
                }
            }
            else
            {
                Exec1(_cInfo, _player);
            }
        }

        public static void CommandCost(ClientInfo _cInfo, EntityPlayer _player)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                Exec2(_cInfo, _player);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = "you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
            }
        }

        public static void Exec1(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (Inside_Claim)
            {
                World world = GameManager.Instance.World;
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
                    string _phrase781;
                    if (!Phrases.Dict.TryGetValue(781, out _phrase781))
                    {
                        _phrase781 = "saved your current bike for retrieval.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase781 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                    string _sql = string.Format("UPDATE Players SET bikeId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                    SQL.FastQuery(_sql);
                }
                else
                {
                    string _phrase780;
                    if (!Phrases.Dict.TryGetValue(780, out _phrase780))
                    {
                        _phrase780 = "you have not claimed this space or a friend. You can only save your bike inside a claimed space.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase780 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                }
            }
            else
            {
                string _phrase781;
                if (!Phrases.Dict.TryGetValue(781, out _phrase781))
                {
                    _phrase781 = "saved your current bike for retrieval.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase781 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                string _sql = string.Format("UPDATE Players SET bikeId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                SQL.FastQuery(_sql);
            }
        }

        public static void Exec2(ClientInfo _cInfo, EntityPlayer _player)
        {
            string _sql = string.Format("SELECT bikeId FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            int _bikeId;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bikeId);
            _result.Dispose();
            if (_bikeId != 0)
            {
                List<Entity> Entities = GameManager.Instance.World.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    string _name = EntityClass.list[_entity.entityClass].entityClassName;
                    Type _class = EntityClass.list[_entity.entityClass].classname.BaseType;
                    Log.Out(string.Format("entity name = {0}/ class base type = {1}", _name, _class));
                    if (_name == "minibike")
                    {
                        if ((_player.position.x - _entity.position.x) * (_player.position.x - _entity.position.x) + (_player.position.z - _entity.position.z) * (_player.position.z - _entity.position.z) <= 50 * 50)
                        {
                            if (_entity.entityId == _bikeId)
                            {
                                if (_entity.AttachedToEntity == false)
                                {
                                    _entity.SetPosition(_player.position);
                                    string _phrase782;
                                    if (!Phrases.Dict.TryGetValue(782, out _phrase782))
                                    {
                                        _phrase782 = "found your bike and sent it to you.";
                                    }
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase782 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                                    }
                                    _sql = string.Format("UPDATE Players SET lastBike = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                                    SQL.FastQuery(_sql);
                                    return;
                                }
                                else
                                {
                                    string _phrase785;
                                    if (!Phrases.Dict.TryGetValue(785, out _phrase785))
                                    {
                                        _phrase785 = "found your bike but someone else is on it.";
                                    }
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase785 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                                    return;
                                }
                            }
                        }
                    }
                }
                string _phrase784;
                if (!Phrases.Dict.TryGetValue(784, out _phrase784))
                {
                    _phrase784 = "could not find your bike near by.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase784 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
            }
            else
            {
                string _phrase783;
                if (!Phrases.Dict.TryGetValue(783, out _phrase783))
                {
                    _phrase783 = "you do not have a bike saved.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase783 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
            }
        }
    }
}

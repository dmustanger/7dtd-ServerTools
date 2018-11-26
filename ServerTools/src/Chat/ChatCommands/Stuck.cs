using System;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class Stuck
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60;

        public static void Delay(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                CheckLP(_cInfo);
            }
            else
            {
                string _sql = string.Format("SELECT lastMarket FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                DateTime _lastStuck;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastStuck);
                _result.Dispose();
                if (_lastStuck.ToString() == "10/29/2000 7:30:00 AM")
                {
                    CheckLP(_cInfo);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - _lastStuck;
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
                                    CheckLP(_cInfo);
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase920;
                                    if (!Phrases.Dict.TryGetValue(920, out _phrase920))
                                    {
                                        _phrase920 = "you can only use /stuck once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase920 = _phrase920.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase920 = _phrase920.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase920 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase920 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= Delay_Between_Uses)
                        {
                            CheckLP(_cInfo);
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase920;
                            if (!Phrases.Dict.TryGetValue(920, out _phrase920))
                            {
                                _phrase920 = "you can only use /stuck once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase920 = _phrase920.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase920 = _phrase920.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase920 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase920 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
        }

        public static void CheckLP(ClientInfo _cInfo)
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
                if (CheckStuck(_player))
                {
                    Exec(_cInfo, _player);
                }
                else
                {
                    string _phrase923;
                    if (!Phrases.Dict.TryGetValue(923, out _phrase923))
                    {
                        _phrase923 = "you do not seem to be stuck.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase923 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                string _phrase921;
                if (!Phrases.Dict.TryGetValue(921, out _phrase921))
                {
                    _phrase921 = "you are outside of your claimed space or a friends. Command is unavailable.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase921 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static bool CheckStuck(EntityPlayer _player)
        {
            int x = (int)_player.position.x;
            int y = (int)_player.position.y;
            int z = (int)_player.position.z;
            for (int i = x - 1; i <= (x + 1); i++)
            {
                for (int j = z - 1; j <= (z + 1); j++)
                {
                    for (int k = y - 2; k <= (y + 1); k++)
                    {
                        BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        string _blockName = Block.Block.GetBlockName();
                        if (Block.type == BlockValue.Air.type || _player.IsInElevator() || _player.IsInWater())
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static void Exec(ClientInfo _cInfo, EntityPlayer _player)
        {
            Players.NoFlight.Add(_cInfo.entityId);
            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3((int)_player.position.x, -1, (int)_player.position.z), null, false));
            string _phrase922;
            if (!Phrases.Dict.TryGetValue(922, out _phrase922))
            {
                _phrase922 = "sending you to the world surface. If you are still stuck, contact an administrator.";
            }
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase922 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            string _sql = string.Format("UPDATE Players SET lastStuck = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
            SQL.FastQuery(_sql);
        }
    }
}

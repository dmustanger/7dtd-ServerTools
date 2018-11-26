using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    public class Suicide
    {
        public static bool IsEnabled = false, PvP_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 60;

        public static void CheckPlayer(ClientInfo _cInfo, bool _announce)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                Kill(_cInfo);
            }
            else
            {
                string _sql = string.Format("SELECT lastkillme FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                DateTime _lastkillme;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastkillme);
                _result.Dispose();
                TimeSpan varTime = DateTime.Now - _lastkillme;
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
                                Kill(_cInfo);
                            }
                            else
                            {
                                int _timeleft = _newDelay - _timepassed;
                                string _phrase8;
                                if (!Phrases.Dict.TryGetValue(8, out _phrase8))
                                {
                                    _phrase8 = "you can only use /killme, /wrist, /hang, or /suicide once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                }
                                _phrase8 = _phrase8.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                _phrase8 = _phrase8.Replace("{TimeRemaining}", _timeleft.ToString());
                                if (_announce)
                                {
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase8 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase8 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
                if (!_donator)
                {
                    if (_timepassed >= Delay_Between_Uses)
                    {
                        Kill(_cInfo);
                    }
                    else
                    {
                        int _timeleft = Delay_Between_Uses - _timepassed;
                        string _phrase8;
                        if (!Phrases.Dict.TryGetValue(8, out _phrase8))
                        {
                            _phrase8 = "you can only use /killme, /wrist, /hang, or /suicide once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                        }
                        _phrase8 = _phrase8.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                        _phrase8 = _phrase8.Replace("{TimeRemaining}", _timeleft.ToString());
                        if (_announce)
                        {
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase8 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase8 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        private static void Kill(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (PvP_Check)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = _cInfoList[i];
                    if (_cInfo2 != null)
                    {
                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                        if (_player2 != null)
                        {
                            Vector3 _pos2 = _player2.GetPosition();
                            if (((int)_player.position.x - (int)_pos2.x) * ((int)_player.position.x - (int)_pos2.x) + ((int)_player.position.z - (int)_pos2.z) * ((int)_player.position.z - (int)_pos2.z) <= 25 * 25)
                            {
                                if (!_player.IsFriendsWith(_player2))
                                {
                                    string _phrase819;
                                    if (!Phrases.Dict.TryGetValue(819, out _phrase819))
                                    {
                                        _phrase819 = "you are too close to a player that is not a friend. Command unavailable.";
                                    }
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase819 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (Zombie_Check)
            {
                World world = GameManager.Instance.World;
                List<Entity> Entities = world.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    if (_entity != null)
                    {
                        EntityType _type = _entity.entityType;
                        if (_type == EntityType.Zombie)
                        {
                            Vector3 _pos2 = _entity.GetPosition();
                            if (((int)_player.position.x - (int)_pos2.x) * ((int)_player.position.x - (int)_pos2.x) + ((int)_player.position.z - (int)_pos2.z) * ((int)_player.position.z - (int)_pos2.z) <= 10 * 10)
                            {
                                string _phrase820;
                                if (!Phrases.Dict.TryGetValue(820, out _phrase820))
                                {
                                    _phrase820 = "you are too close to a zombie. Command unavailable.";
                                }
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase820 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                        }
                    }
                }
            }
            SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), (ClientInfo)null);
            string _sql = string.Format("UPDATE Players SET lastkillme = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
            SQL.FastQuery(_sql);
        }
    }
}
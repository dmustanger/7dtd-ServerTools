using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class Suicide
    {
        public static bool IsEnabled = false, PvP_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 60;
        public static string Command20 = "killme", Command21 = "wrist", Command22 = "hang", Command23 = "suicide";

        public static void Exec(ClientInfo _cInfo)
        {
            if (Delay_Between_Uses < 1)
            {
                Kill(_cInfo);
            }
            else
            {
                DateTime _lastkillme = PersistentContainer.Instance.Players[_cInfo.playerId].LastKillMe;
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
                            int _delay = Delay_Between_Uses / 2;
                            Time(_cInfo, _timepassed, _delay);
                            return;
                        }
                    }
                }
                Time(_cInfo, _timepassed, Delay_Between_Uses);
            }
        }

        private static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                Kill(_cInfo);
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                string _phrase8;
                if (!Phrases.Dict.TryGetValue(8, out _phrase8))
                {
                    _phrase8 = "You can only use {CommandPrivate}killme, {CommandPrivate}{Command21}, {CommandPrivate}{Command22}, or {CommandPrivate}{Command23} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase8 = _phrase8.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase8 = _phrase8.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase8 = _phrase8.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase8 = _phrase8.Replace("{Command20}", Command20);
                _phrase8 = _phrase8.Replace("{Command21}", Command21);
                _phrase8 = _phrase8.Replace("{Command22}", Command22);
                _phrase8 = _phrase8.Replace("{Command23}", Command23);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase8 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }


        private static void Kill(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (PvP_Check)
            {
                List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
                for (int i = 0; i < ClientInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = ClientInfoList[i];
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
                                        _phrase819 = "You are too close to a player that is not a friend. Command unavailable.";
                                    }
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase819 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (Zombie_Check)
            {
                List<Entity> Entities = GameManager.Instance.World.Entities.list;
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
                                    _phrase820 = "You are too close to a zombie. Command unavailable.";
                                }
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase820 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                        }
                    }
                }
            }
            SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), (ClientInfo)null);
            PersistentContainer.Instance.Players[_cInfo.playerId].LastKillMe = DateTime.Now;
            PersistentContainer.Instance.Save();
        }
    }
}
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
                Phrases.Dict.TryGetValue(31, out string _phrase31);
                _phrase31 = _phrase31.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase31 = _phrase31.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase31 = _phrase31.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase31 = _phrase31.Replace("{Command20}", Command20);
                _phrase31 = _phrase31.Replace("{Command21}", Command21);
                _phrase31 = _phrase31.Replace("{Command22}", Command22);
                _phrase31 = _phrase31.Replace("{Command23}", Command23);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase31 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    Phrases.Dict.TryGetValue(32, out string _phrase32);
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase32 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                Phrases.Dict.TryGetValue(33, out string _phrase33);
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase33 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                        }
                    }
                }
            }
            SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), (ClientInfo)null);
            PersistentContainer.Instance.Players[_cInfo.playerId].LastKillMe = DateTime.Now;
        }
    }
}
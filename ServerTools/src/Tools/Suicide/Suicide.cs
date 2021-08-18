using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class Suicide
    {
        public static bool IsEnabled = false, Player_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 60;
        public static string Command_killme = "killme", Command_wrist = "wrist", Command_hang = "hang", Command_suicide = "suicide";

        public static void Exec(ClientInfo _cInfo)
        {
            try
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
                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Suicide.Exec: {0}", e.Message));
            }
        }

        private static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    Kill(_cInfo);
                }
                else
                {
                    int _timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Suicide1", out string _phrase1);
                    _phrase1 = _phrase1.Replace("{DelayBetweenUses}", _delay.ToString());
                    _phrase1 = _phrase1.Replace("{TimeRemaining}", _timeleft.ToString());
                    _phrase1 = _phrase1.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase1 = _phrase1.Replace("{Command_killme}", Command_killme);
                    _phrase1 = _phrase1.Replace("{Command_wrist}", Command_wrist);
                    _phrase1 = _phrase1.Replace("{Command_hang}", Command_hang);
                    _phrase1 = _phrase1.Replace("{Command_suicide}", Command_suicide);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Suicide.Time: {0}", e.Message));
            }
        }


        private static void Kill(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (Player_Check)
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
                                        Phrases.Dict.TryGetValue("Suicide2", out string _phrase2);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                    Phrases.Dict.TryGetValue("Suicide3", out string _phrase3);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase3 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                            }
                        }
                    }
                }
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), null);
                PersistentContainer.Instance.Players[_cInfo.playerId].LastKillMe = DateTime.Now;
                PersistentContainer.DataChange = true;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Suicide.Kill: {0}", e.Message));
            }
        }
    }
}
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
                    DateTime lastkillme = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastKillMe;
                    TimeSpan varTime = DateTime.Now - lastkillme;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                            {
                                if (DateTime.Now < dt)
                                {
                                    int delay = Delay_Between_Uses / 2;
                                    Time(_cInfo, timepassed, delay);
                                    return;
                                }
                            }
                            else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                            {
                                if (DateTime.Now < dt)
                                {
                                    int delay = Delay_Between_Uses / 2;
                                    Time(_cInfo, timepassed, delay);
                                    return;
                                }
                            }
                        }
                    }
                    Time(_cInfo, timepassed, Delay_Between_Uses);
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
                    int timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Suicide1", out string phrase1);
                    phrase1 = phrase1.Replace("{DelayBetweenUses}", _delay.ToString());
                    phrase1 = phrase1.Replace("{TimeRemaining}", timeleft.ToString());
                    phrase1 = phrase1.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase1 = phrase1.Replace("{Command_killme}", Command_killme);
                    phrase1 = phrase1.Replace("{Command_wrist}", Command_wrist);
                    phrase1 = phrase1.Replace("{Command_hang}", Command_hang);
                    phrase1 = phrase1.Replace("{Command_suicide}", Command_suicide);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (Player_Check)
                    {
                        List<ClientInfo> clientList = GeneralOperations.ClientList();
                        if (clientList != null)
                        {
                            for (int i = 0; i < clientList.Count; i++)
                            {
                                ClientInfo cInfo2 = clientList[i];
                                if (cInfo2 != null)
                                {
                                    EntityPlayer player2 = GeneralOperations.GetEntityPlayer(cInfo2.entityId);
                                    if (player2 != null)
                                    {
                                        Vector3 pos2 = player2.GetPosition();
                                        if (((int)player.position.x - (int)pos2.x) * ((int)player.position.x - (int)pos2.x) + ((int)player.position.z - (int)pos2.z) * ((int)player.position.z - (int)pos2.z) <= 30 * 30)
                                        {
                                            if (!player.IsFriendsWith(player2))
                                            {
                                                Phrases.Dict.TryGetValue("Suicide2", out string phrase);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Zombie_Check)
                    {
                        List<Entity> entities = GameManager.Instance.World.Entities.list;
                        for (int i = 0; i < entities.Count; i++)
                        {
                            Entity entity = entities[i];
                            if (entity != null)
                            {
                                EntityType type = entity.entityType;
                                if (type == EntityType.Zombie)
                                {
                                    Vector3 pos2 = entity.GetPosition();
                                    if (((int)player.position.x - (int)pos2.x) * ((int)player.position.x - (int)pos2.x) + ((int)player.position.z - (int)pos2.z) * ((int)player.position.z - (int)pos2.z) <= 20 * 20)
                                    {
                                        Phrases.Dict.TryGetValue("Suicide3", out string phrase);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.CrossplatformId.CombinedString), null);
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastKillMe = DateTime.Now;
                    PersistentContainer.DataChange = true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Suicide.Kill: {0}", e.Message));
            }
        }
    }
}
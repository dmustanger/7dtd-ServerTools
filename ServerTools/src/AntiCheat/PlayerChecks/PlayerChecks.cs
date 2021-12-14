using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class PlayerChecks
    {
        public static bool GodEnabled = false, SpectatorEnabled = false, HalfSecondRunning = false;
        public static int Godmode_Admin_Level = 0, Spectator_Admin_Level = 0;

        public static Dictionary<int, Vector3> TwoSecondMovement = new Dictionary<int, Vector3>();

        private static string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static void TwoSecondExec()
        {
            try
            {
                List<ClientInfo> clientList = PersistentOperations.ClientList();
                if (clientList != null)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo = clientList[i];
                        if (cInfo != null && cInfo.playerId != null)
                        {
                            EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.playerId);
                            if (player != null)
                            {
                                int userPermissionLevel = GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo);
                                if (SpectatorEnabled && userPermissionLevel > Spectator_Admin_Level)
                                {
                                    if (player.IsSpectator)
                                    {
                                        Phrases.Dict.TryGetValue("Spectator2", out string phrase);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", cInfo.playerId, phrase), null);
                                        using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("Detected \"'{0}'\" with steam id '{1}' using spectator mode @ {2}", cInfo.playerName, cInfo.playerId, new Vector3i(player.position)));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        Log.Warning("[SERVERTOOLS] Detected '{0}' with steam id '{1}' using spectator mode @ {2}", cInfo.playerName, cInfo.playerId, new Vector3i(player.position));
                                        Phrases.Dict.TryGetValue("Spectator1", out phrase);
                                        phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                        continue;
                                    }
                                }
                                if (GodEnabled && userPermissionLevel > Godmode_Admin_Level)
                                {
                                    if (player.Buffs.HasBuff("god"))
                                    {
                                        Phrases.Dict.TryGetValue("Godemode2", out string phrase);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", cInfo.playerId, phrase), null);
                                        using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("Detected '{0}' with steam id '{1}' using godmode @ {2}", cInfo.playerName, cInfo.playerId, new Vector3i (player.position)));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        Log.Warning("[SERVERTOOLS] Detected \"'{0}'\" with steam id '{1}' using godmode @ {2}", cInfo.playerName, cInfo.playerId, new Vector3i(player.position));
                                        Phrases.Dict.TryGetValue("Godemode1", out phrase);
                                        phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                        continue;
                                    }
                                }
                                if (TwoSecondMovement.ContainsKey(cInfo.entityId))
                                {
                                    TwoSecondMovement.TryGetValue(cInfo.entityId, out Vector3 oldPosition);
                                    if (oldPosition != player.position)
                                    {
                                        TwoSecondMovement[cInfo.entityId] = player.position;
                                        if (oldPosition.y - player.position.y >= 4)
                                        {
                                            TwoSecondMovement.Remove(cInfo.entityId);
                                            continue;
                                        }
                                        if (player.AttachedToEntity != null && player.AttachedToEntity is EntityVehicle)
                                        {
                                            Entity entity = GameManager.Instance.World.GetEntity(player.AttachedToEntity.entityId);
                                            if (entity != null && entity is EntityVehicle)
                                            {
                                                continue;
                                            }
                                        }
                                        if (SpeedDetector.IsEnabled && userPermissionLevel > SpeedDetector.Speed_Admin_Level && SpeedDetector.TravelTooFar(oldPosition, player.position))
                                        {
                                            SpeedDetector.Detected(cInfo);
                                        }
                                        if (FlyingDetector.IsEnabled && userPermissionLevel > FlyingDetector.Flying_Admin_Level && cInfo.ping < 350)
                                        {
                                            
                                            if (FlyingDetector.IsFlying(player.position))
                                            {
                                                FlyingDetector.Detected(cInfo, player);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    TwoSecondMovement.Add(cInfo.entityId, player.position);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerChecks.TwoSecondExec: {0}", e.Message));
            }
        }

        public static void HalfSecondExec()
        {
            try
            {
                HalfSecondRunning = true;
                List<ClientInfo> clientList = PersistentOperations.ClientList();
                if (clientList != null)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo = clientList[i];
                        if (cInfo != null && cInfo.playerId != null)
                        {
                            if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo) > XRayDetector.Admin_Level)
                            {
                                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.playerId);
                                if (player != null)
                                {
                                    XRayDetector.IsInsideBlocks(cInfo, player);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerChecks.HalfSecondExec: {0}", e.Message));
            }
            HalfSecondRunning = false;
        }
    }
}

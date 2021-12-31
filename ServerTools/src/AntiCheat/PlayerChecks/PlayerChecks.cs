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
                        if (cInfo != null)
                        {
                            EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                            if (player != null)
                            {
                                int userPlatformPermissionLevel = GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId);
                                int userCrossplatformPermissionLevel = GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId);
                                if (SpectatorEnabled && (userPlatformPermissionLevel > Spectator_Admin_Level || userCrossplatformPermissionLevel > Spectator_Admin_Level))
                                {
                                    if (player.IsSpectator)
                                    {
                                        Phrases.Dict.TryGetValue("Spectator2", out string phrase);
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                                        using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' using spectator mode @ '{2}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, player.position));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        Log.Warning(string.Format("[SERVERTOOLS] Detected id '{0}' '{1}' named '{2}' using spectator mode @ '{2}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, player.position));
                                        Phrases.Dict.TryGetValue("Spectator1", out phrase);
                                        phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                        continue;
                                    }
                                }
                                if (GodEnabled && (userPlatformPermissionLevel > Godmode_Admin_Level || userCrossplatformPermissionLevel > Godmode_Admin_Level))
                                {
                                    if (player.Buffs.HasBuff("god"))
                                    {
                                        Phrases.Dict.TryGetValue("Godemode2", out string phrase);
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                                        using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' using god mode @ '{2}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, player.position));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        Log.Warning(string.Format("[SERVERTOOLS] Detected id '{0}' '{1}' named '{2}' using god mode @ '{2}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, player.position));
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
                                        if (oldPosition.y - player.position.y >= 3)
                                        {
                                            TwoSecondMovement.Remove(cInfo.entityId);
                                            continue;
                                        }
                                        if (player.AttachedToEntity != null && player.AttachedToEntity is EntityVehicle)
                                        {
                                            Entity entity = PersistentOperations.GetEntity(player.AttachedToEntity.entityId);
                                            if (entity != null && entity is EntityVehicle)
                                            {
                                                continue;
                                            }
                                        }
                                        if (cInfo.ping < 350)
                                        {
                                            if (SpeedDetector.IsEnabled && (userPlatformPermissionLevel > SpeedDetector.Speed_Admin_Level || userCrossplatformPermissionLevel > SpeedDetector.Speed_Admin_Level))
                                            {
                                                if (SpeedDetector.TravelTooFar(oldPosition, player.position))
                                                {
                                                    SpeedDetector.Detected(cInfo);
                                                }
                                                else if (SpeedDetector.Flags.ContainsKey(cInfo.entityId))
                                                {
                                                    SpeedDetector.Flags.Remove(cInfo.entityId);
                                                }
                                            }
                                            if (FlyingDetector.IsEnabled && (userPlatformPermissionLevel > FlyingDetector.Flying_Admin_Level || userCrossplatformPermissionLevel > FlyingDetector.Flying_Admin_Level))
                                            {
                                                if (FlyingDetector.IsFlying(player.position))
                                                {
                                                    FlyingDetector.Detected(cInfo, player);
                                                }
                                                else if (FlyingDetector.Flags.ContainsKey(cInfo.entityId))
                                                {
                                                    FlyingDetector.Flags.Remove(cInfo.entityId);
                                                }
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
                        if (cInfo != null)
                        {
                            if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > XRayDetector.Admin_Level ||
                                GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > XRayDetector.Admin_Level)
                            {
                                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
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

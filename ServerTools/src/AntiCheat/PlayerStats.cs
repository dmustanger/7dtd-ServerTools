using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools.AntiCheat
{
    class PlayerStats
    {
        public static bool IsEnabled = false, Kick_Enabled = false, Ban_Enabled = false;
        public static int Admin_Level = 0, Days_Before_Log_Delete = 5, Max_Speed = 28;
        private static string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);

        public static void Exec()
        {
            try
            {
                if (GameManager.Instance.World.Players.dict.Count > 0)
                {
                    List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
                    for (int i = 0; i < ClientInfoList.Count; i++)
                    {
                        ClientInfo _cInfo = ClientInfoList[i];
                        GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                        AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                        if (Admin.PermissionLevel > Admin_Level)
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                            if (_player.IsSpawned())
                            {
                                var p_speedForward = _player.speedForward;
                                var p_Health = _player.Stats.Health.Value;
                                var p_Stamina = _player.Stats.Stamina.Value;
                                var p_jumpStrength = _player.jumpStrength;
                                var p_height = _player.height;
                                if (p_Health > 250)
                                {
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("{0} {1} steamId {2} was detected with an illegal health value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_Health));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    Log.Warning("Detected player {0} steamId {1} with health @ {2}. 250 is maximum", _cInfo.playerName, _cInfo.playerId, p_Health);
                                    if (Kick_Enabled)
                                    {

                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " was detected and kicked for illegal player stat health" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat health\"", _cInfo.playerId), (ClientInfo)null);
                                    }
                                    if (Ban_Enabled)
                                    {
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " was detected and banned for illegal player stat health" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat health\"", _cInfo.playerId), (ClientInfo)null);
                                    }
                                }
                                if (p_Stamina > 250)
                                {
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("{0} {1} steamId {2} was detected with an illegal stamina value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_Stamina));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    Log.Warning("Detected player {0} steamId {1} with stamina @ {2}. 250 is maximum", _cInfo.playerName, _cInfo.playerId, p_Stamina);
                                    if (Kick_Enabled)
                                    {
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " was detected and kicked for illegal player stat stamina" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat stamina\"", _cInfo.playerId), (ClientInfo)null);
                                    }
                                    if (Ban_Enabled)
                                    {
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " was detected and banned for illegal player stat stamina" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat stamina\"", _cInfo.playerId), (ClientInfo)null);
                                    }
                                }
                                if (p_jumpStrength >= 1.5)
                                {
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("{0} {1} steamId {2} was detected with an illegal jump strength value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_jumpStrength));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    Log.Warning("Detected player {0} steamId {1} at jump strength {2}. 1.1 is default", _cInfo.playerName, _cInfo.playerId, p_jumpStrength);
                                    if (Kick_Enabled)
                                    {
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " was detected and kicked for illegal player jump ability" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat jump\"", _cInfo.playerId), (ClientInfo)null);
                                    }
                                    if (Ban_Enabled)
                                    {
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " was detected and banned for illegal player jump ability" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat jump\"", _cInfo.playerId), (ClientInfo)null);
                                    }
                                }
                                if (p_height > 1.8 || p_height < 1.7)
                                {
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("{0} {1} steamId {2} was detected with an illegal player height value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_height));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    Log.Warning("Detected player {0} steamId {1} with player height @ {2}", _cInfo.playerName, _cInfo.playerId, p_height);
                                    if (Kick_Enabled)
                                    {
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " was detected and kicked for illegal player height" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat height\"", _cInfo.playerId), (ClientInfo)null);
                                    }
                                    if (Ban_Enabled)
                                    {
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " was detected and banned for illegal player height" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat height\"", _cInfo.playerId), (ClientInfo)null);
                                    }
                                }
                                if (p_speedForward > Max_Speed)
                                {
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("{0} {1} steamId {2} was detected with an illegal run speed value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_speedForward));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    Log.Warning("Detected player {0} steamId {1} speed hacking", _cInfo.playerName, _cInfo.playerId);
                                    if (Kick_Enabled)
                                    {
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " was detected and kicked for illegal player speed" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat speed\"", _cInfo.playerId), (ClientInfo)null);
                                    }
                                    if (Ban_Enabled)
                                    {
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " was detected and banned for illegal player speed" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat speed\"", _cInfo.playerId), (ClientInfo)null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerStats.Exec: {0}", e.Message));
            }
        }
    }
}

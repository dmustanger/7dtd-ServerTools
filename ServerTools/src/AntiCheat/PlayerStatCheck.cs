using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class PlayerStatCheck
    {
        public static bool IsEnabled = false, Kick_Enabled = false, Ban_Enabled = false;
        public static int Admin_Level = 0, Days_Before_Log_Delete = 5;
        private static string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/DetectionLogs/{1}", API.GamePath, _file);

        public static void DetectionLogsDir()
        {
            if (!Directory.Exists(API.GamePath + "/DetectionLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/DetectionLogs");
            }

            string[] files = Directory.GetFiles(API.GamePath + "/DetectionLogs");
            int _daysBeforeDeleted = (Days_Before_Log_Delete * -1);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                {
                    fi.Delete();
                }
            }
        }

        public static void PlayerStat()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                    AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                    if (Admin.PermissionLevel > Admin_Level)
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (_player.IsSpawned())
                        {
                            int pointsPer = Progression.SkillPointsPerLevel;
                            int maxPlayerLevel = Progression.MaxLevel;
                            var p_speedForward = _player.speedForward;
                            var p_Health = _player.Stats.Health.Value;
                            var p_Stamina = _player.Stats.Stamina.Value;
                            var p_ExpToNextLevel = _player.ExpToNextLevel;
                            var p_jumpStrength = _player.jumpStrength;
                            var p_Level = _player.Level;
                            var p_SkillPoints = _player.SkillPoints;
                            var p_height = _player.height;
                            var maxPts = (p_Level * pointsPer + 42);

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
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat health[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat health\"", _cInfo.playerId), (ClientInfo)null);
                                }
                                if (Ban_Enabled)
                                {
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and banned for illegal player stat health[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
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
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat stamina[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat stamina\"", _cInfo.playerId), (ClientInfo)null);
                                }
                                if (Ban_Enabled)
                                {
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat stamina[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat stamina\"", _cInfo.playerId), (ClientInfo)null);
                                }
                            }
                            if (p_jumpStrength >= 1.2)
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
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player jump ability[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat jump\"", _cInfo.playerId), (ClientInfo)null);
                                }
                                if (Ban_Enabled)
                                {
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player jump ability[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat jump\"", _cInfo.playerId), (ClientInfo)null);
                                }
                            }
                            if (p_Level > maxPlayerLevel)
                            {                               
                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0} {1} steamId {2} was detected with an illegal player level value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, maxPlayerLevel));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                Log.Warning("Detected player {0} steamId {1} at level {2}. Maximum level set is {3}", _cInfo.playerName, _cInfo.playerId, p_Level, maxPlayerLevel);
                                if (Kick_Enabled)
                                {
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat level[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat level\"", _cInfo.playerId), (ClientInfo)null);
                                }
                                if (Ban_Enabled)
                                {
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat level[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat level\"", _cInfo.playerId), (ClientInfo)null);
                                }
                            }
                            if (p_SkillPoints > maxPts)
                            {
                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0} {1} steamId {2} was detected with an illegal skill point value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_SkillPoints));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                Log.Warning("Detected player {0} steamId {1} with {2} skill points available, while only level {3}. Player can only have {4} skill points at this level.", _cInfo.playerName, _cInfo.playerId, p_SkillPoints, p_Level, maxPts);
                                if (Kick_Enabled)
                                {
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat skill points available[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat skill points\"", _cInfo.playerId), (ClientInfo)null);
                                }
                                if (Ban_Enabled)
                                {
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat skill points available[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat skill points\"", _cInfo.playerId), (ClientInfo)null);
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
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player height[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat height\"", _cInfo.playerId), (ClientInfo)null);
                                }
                                if (Ban_Enabled)
                                {
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player height[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat height\"", _cInfo.playerId), (ClientInfo)null);
                                }
                            }
                            if (p_speedForward > 14)
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
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player speed[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat speed\"", _cInfo.playerId), (ClientInfo)null);
                                }
                                if (Ban_Enabled)
                                {
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player speed[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat speed\"", _cInfo.playerId), (ClientInfo)null);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

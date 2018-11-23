using System;
using System.IO;

namespace ServerTools
{
    class TeleportCheck
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0;
        public static bool Announce = false;
        public static bool Kill_Enabled = false;
        public static bool Jail_Enabled = false;
        public static bool Kick_Enabled = false;
        public static bool Ban_Enabled = false;
        public static int Days_Before_Log_Delete = 5;

        public static void TeleportCheckValid(ClientInfo _cInfo)
        {
            GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
            AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
            if (Admin.PermissionLevel > Admin_Level)
            {
                Penalty(_cInfo);
            }
        }

        public static void Penalty(ClientInfo _cInfo)
        {
            if (Jail_Enabled)
            {
                if (Announce)
                {
                    ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " has been jailed for teleporting." + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0}", _cInfo.playerId), (ClientInfo)null);
            }
            if (Kill_Enabled)
            {
                if (Announce)
                {
                    ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " has been executed for teleporting." + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), (ClientInfo)null);
            }
            if (Kick_Enabled)
            {
                if (Announce)
                {
                    ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " has been kicked for teleporting." + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for teleporting.\"", _cInfo.playerId), (ClientInfo)null);
            }
            if (Ban_Enabled)
            {
                if (Announce)
                {
                    ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + " has been banned for teleporting." + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for teleporting\"", _cInfo.playerId), (ClientInfo)null);
            }
            string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
            string _filepath = string.Format("{0}/DetectionLogs/{1}", API.GamePath, _file);
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, Entity Id {2} teleporting.", _cInfo.playerName, _cInfo.steamId, _cInfo.entityId));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }

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
    }
}

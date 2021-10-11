using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class TeleportCheck
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0;
        public static bool Kill_Enabled = false;
        public static bool Jail_Enabled = false;
        public static bool Kick_Enabled = false;
        public static bool Ban_Enabled = false;

        public static void TeleportCheckValid(ClientInfo _cInfo)
        {
            if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
            {
                Penalty(_cInfo);
            }
        }

        public static void Penalty(ClientInfo _cInfo)
        {
            if (Jail_Enabled)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0}", _cInfo.playerId), null);

            }
            if (Kill_Enabled)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), null);

            }
            if (Kick_Enabled)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for teleporting.\"", _cInfo.playerId), null);

            }
            if (Ban_Enabled)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for teleporting\"", _cInfo.playerId), null);
            }
            string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
            string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
            using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("Detected {0} Steam Id {1}, teleporting without permission.", _cInfo.playerName, _cInfo.playerId, _cInfo.entityId));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }
    }
}

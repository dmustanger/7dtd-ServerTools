using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class TeleportCheck
    {
        public static bool IsEnabled = false, Kill_Enabled = false, Jail_Enabled = false, Kick_Enabled = false, Ban_Enabled = false;
        public static int Admin_Level = 0;
        public static string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static void TeleportCheckValid(ClientInfo _cInfo)
        {
            if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.PlatformId) > Admin_Level ||
                GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.CrossplatformId) > Admin_Level)
            {
                Penalty(_cInfo);
            }
        }

        public static void Penalty(ClientInfo _cInfo)
        {
            if (Jail_Enabled)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("jail add {0}", _cInfo.CrossplatformId.CombinedString), null);

            }
            if (Kill_Enabled)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.CrossplatformId.CombinedString), null);

            }
            if (Kick_Enabled)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for teleporting\"", _cInfo.CrossplatformId.CombinedString), null);

            }
            if (Ban_Enabled)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for teleporting\"", _cInfo.CrossplatformId.CombinedString), null);
            }
            
            using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' teleporting without permission", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _cInfo.entityId));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }
    }
}

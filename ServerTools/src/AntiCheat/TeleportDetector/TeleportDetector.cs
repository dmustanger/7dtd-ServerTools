using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class TeleportDetector
    {
        public static bool IsEnabled = false, Kill = false, Jail = false, Kick = false, Ban = false;
        public static int Admin_Level = 0;
        public static string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string Filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static List<int> Ommissions = new List<int>();

        public static void Exec(ClientInfo _cInfo)
        {
            if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.PlatformId) > Admin_Level &&
                GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.CrossplatformId) > Admin_Level)
            {
                if (!Ommissions.Contains(_cInfo.entityId))
                {
                    if (Jail)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("st-Jail add {0} 120", _cInfo.CrossplatformId.CombinedString), null);
                        Phrases.Dict.TryGetValue("Teleport4", out string phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    if (Kill)
                    {
                        Phrases.Dict.TryGetValue("Teleport5", out string phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.CrossplatformId.CombinedString), null);

                    }
                    if (Kick)
                    {
                        Phrases.Dict.TryGetValue("Teleport6", out string phrase);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);

                    }
                    if (Ban)
                    {
                        Phrases.Dict.TryGetValue("Teleport7", out string phrase);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);

                    }
                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(string.Format("Detected Id '{0}' '{1}' named '{2}' teleporting illegally", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
                else
                {
                    Ommissions.Remove(_cInfo.entityId);
                }
            }
        }
    }
}

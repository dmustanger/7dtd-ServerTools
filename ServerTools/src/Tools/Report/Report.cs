using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class Report
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Delay = 60, Length = 150, Days_Before_Log_Delete = 5;
        public static string Command_report = "report";

        private static string file = string.Format("Report_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string FilePath = string.Format("{0}/Logs/PlayerReports/{1}", API.ConfigPath, file);

        public static void Check(ClientInfo _cInfo, string _message)
        {
            DateTime lastLog = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastLog;
            TimeSpan varTime = DateTime.Now - lastLog;
            double fractionalMinutes = varTime.TotalMinutes;
            int timepassed = (int)fractionalMinutes;
            if (timepassed >= Delay)
            {
                Exec(_cInfo, _message);
            }
            else
            {
                int timeleft = Delay - timepassed;
                Phrases.Dict.TryGetValue("Report1", out string phrase);
                phrase = phrase.Replace("{DelayBetweenUses}", Delay.ToString());
                phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _message)
        {
            if (_message.Length > Length)
            {
                Phrases.Dict.TryGetValue("Report4", out string phrase);
                phrase = phrase.Replace("{Length}", Length.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            Vector3 _pos = GameManager.Instance.World.Players.dict[_cInfo.entityId].position;
            List<ClientInfo> clientList = PersistentOperations.ClientList();
            if (clientList != null)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    ClientInfo cInfoAdmin = clientList[i];
                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfoAdmin.PlatformId) <= Admin_Level &&
                        GameManager.Instance.adminTools.GetUserPermissionLevel(cInfoAdmin.CrossplatformId) <= Admin_Level)
                    {
                        Phrases.Dict.TryGetValue("Report2", out string phrase);
                        phrase = phrase.Replace("{Position}", _pos.ToString());
                        phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                        phrase = phrase.Replace("{Message}", _message);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("{0}: Location '{1}' Id '{2}' '{3}' named '{4}'. '{5}'", DateTime.Now, _pos, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _message));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastLog = DateTime.Now;
            PersistentContainer.DataChange = true;
            Phrases.Dict.TryGetValue("Report5", out string phrase1);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Log.Out(string.Format("[SERVERTOOLS] Report sent by Id '{0}' '{1}' named '{2}' and saved to the report logs", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName));
        }
    }
}

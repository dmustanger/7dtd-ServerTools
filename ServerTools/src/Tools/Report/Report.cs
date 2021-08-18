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
            DateTime _lastLog = PersistentContainer.Instance.Players[_cInfo.playerId].LastLog;
            TimeSpan varTime = DateTime.Now - _lastLog;
            double fractionalMinutes = varTime.TotalMinutes;
            int _timepassed = (int)fractionalMinutes;
            if (_timepassed >= Delay)
            {
                Exec(_cInfo, _message);
            }
            else
            {
                int _timeleft = Delay - _timepassed;
                Phrases.Dict.TryGetValue("Report1", out string _phrase);
                _phrase = _phrase.Replace("{DelayBetweenUses}", Delay.ToString());
                _phrase = _phrase.Replace("{TimeRemaining}", _timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _message)
        {
            _message = _message.Replace(Command_report + " ", "");
            if (_message.Length > Length)
            {
                Phrases.Dict.TryGetValue("Report4", out string _phrase);
                _phrase = _phrase.Replace("{Length}", Length.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            Vector3 _pos = GameManager.Instance.World.Players.dict[_cInfo.entityId].position;
            List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
            if (ClientInfoList != null && ClientInfoList.Count > 0)
            {
                for (int i = 0; i < ClientInfoList.Count; i++)
                {
                    ClientInfo _cInfoAdmin = ClientInfoList[i];
                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfoAdmin) <= Admin_Level)
                    {
                        Phrases.Dict.TryGetValue("Report2", out string _phrase);
                        _phrase = _phrase.Replace("{Position}", _pos.ToString());
                        _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase = _phrase.Replace("{Message}", _message);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("{0}: Location: {1} {2} {3}. Player {4} {5} reports: {6}.", DateTime.Now, (int)_pos.x, (int)_pos.y, (int)_pos.z, _cInfo.playerName, _cInfo.playerId, _message));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            PersistentContainer.Instance.Players[_cInfo.playerId].LastLog = DateTime.Now;
            PersistentContainer.DataChange = true;
            Phrases.Dict.TryGetValue("Report5", out string _phrase1);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Log.Out(string.Format("[SERVERTOOLS] Report sent by player {0} {1} and saved to the report logs", _cInfo.playerName, _cInfo.playerId));
        }
    }
}

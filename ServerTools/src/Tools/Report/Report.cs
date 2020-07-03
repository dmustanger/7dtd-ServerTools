using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ServerTools
{
    class Report
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Delay = 60, Days_Before_Log_Delete = 5;
        public static string Command82 = "report";
        private static string _file = string.Format("Report_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/Logs/PlayerReports/{1}", API.ConfigPath, _file);

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
                string _phrase795;
                if (!Phrases.Dict.TryGetValue(795, out _phrase795))
                {
                    _phrase795 = "You can only make a report once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase795 = _phrase795.Replace("{DelayBetweenUses}", Delay.ToString());
                _phrase795 = _phrase795.Replace("{TimeRemaining}", _timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase795 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _message)
        {
            _message = _message.Replace(Command82 + " ", "");
            if (_message.Length > 150)
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your report is too long. Please reduce it to 150 characters." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            Vector3 _pos = GameManager.Instance.World.Players.dict[_cInfo.entityId].position;
            List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
            for (int i = 0; i < ClientInfoList.Count; i++)
            {
                ClientInfo _cInfoAdmins = ClientInfoList[i];
                if (GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfoAdmins.playerId).PermissionLevel <= Admin_Level)
                {
                    string _phrase796;
                    if (!Phrases.Dict.TryGetValue(796, out _phrase796))
                    {
                        _phrase796 = "Report @ position {Position} from {PlayerName}: {Message}";
                    }
                    _phrase796 = _phrase796.Replace("{Position}", _pos.ToString());
                    _phrase796 = _phrase796.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase796 = _phrase796.Replace("{Message}", _message);
                    ChatHook.ChatMessage(_cInfoAdmins, LoadConfig.Chat_Response_Color + _phrase796 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("{0}: Location: {1} {2} {3}. Player {4} reports: {5}.", DateTime.Now, (int)_pos.x, (int)_pos.y, (int)_pos.z, _cInfo.playerName, _message));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            string _phrase797;
            if (!Phrases.Dict.TryGetValue(797, out _phrase797))
            {
                _phrase797 = "Your report has been sent to online administrators and logged.";
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase797 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            PersistentContainer.Instance.Players[_cInfo.playerId].LastLog = DateTime.Now;
            PersistentContainer.Instance.Save();
            Log.Out(string.Format("[SERVERTOOLS] Report sent by player name {0} and saved to the report logs", _cInfo.playerName));
        }
    }
}

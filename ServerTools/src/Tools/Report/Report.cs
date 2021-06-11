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
                Phrases.Dict.TryGetValue(521, out string _phrase521);
                _phrase521 = _phrase521.Replace("{DelayBetweenUses}", Delay.ToString());
                _phrase521 = _phrase521.Replace("{TimeRemaining}", _timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase521 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _message)
        {
            _message = _message.Replace(Command82 + " ", "");
            if (_message.Length > Length)
            {
                Phrases.Dict.TryGetValue(524, out string _phrase524);
                _phrase524 = _phrase524.Replace("{Length}", Length.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase524 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            Vector3 _pos = GameManager.Instance.World.Players.dict[_cInfo.entityId].position;
            List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
            if (ClientInfoList != null && ClientInfoList.Count > 0)
            {
                for (int i = 0; i < ClientInfoList.Count; i++)
                {
                    ClientInfo _cInfoAdmins = ClientInfoList[i];
                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= Admin_Level)
                    {
                        Phrases.Dict.TryGetValue(522, out string _phrase522);
                        _phrase522 = _phrase522.Replace("{Position}", _pos.ToString());
                        _phrase522 = _phrase522.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase522 = _phrase522.Replace("{Message}", _message);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase522 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            using (StreamWriter sw = new StreamWriter(_filepath, false, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("{0}: Location: {1} {2} {3}. Player {4} {5} reports: {6}.", DateTime.Now, (int)_pos.x, (int)_pos.y, (int)_pos.z, _cInfo.playerName, _cInfo.playerId, _message));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            PersistentContainer.Instance.Players[_cInfo.playerId].LastLog = DateTime.Now;
            PersistentContainer.DataChange = true;
            Phrases.Dict.TryGetValue(525, out string _phrase525);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase525 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Log.Out(string.Format("[SERVERTOOLS] Report sent by player {0} {1} and saved to the report logs", _cInfo.playerName, _cInfo.playerId));
        }
    }
}

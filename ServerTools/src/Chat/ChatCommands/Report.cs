using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

namespace ServerTools
{
    class Report
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Delay = 60, Days_Before_Log_Delete = 5;
        private static string _file = string.Format("Report_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/Reports/{1}", API.GamePath, _file);

        public static void ReportLogsDir()
        {
            if (!Directory.Exists(API.GamePath + "/Reports"))
            {
                Directory.CreateDirectory(API.GamePath + "/Reports");
            }

            string[] files = Directory.GetFiles(API.GamePath + "/Reports");
            int _daysBeforeDeleted = (Days_Before_Log_Delete * -1);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.CreationTime <= DateTime.Now.AddDays(_daysBeforeDeleted))
                {
                    fi.Delete();
                }
            }
        }

        public static void Check(ClientInfo _cInfo, string _message)
        {
            string _sql = string.Format("SELECT lastLog FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            DateTime _lastLog;
            DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastLog);
            _result.Dispose();
            if (_lastLog.ToString() != "10/29/2000 7:30:00 AM")
            {
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
                        _phrase795 = "{PlayerName} you can only use /report once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                    }
                    _phrase795 = _phrase795.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase795 = _phrase795.Replace("{DelayBetweenUses}", Delay.ToString());
                    _phrase795 = _phrase795.Replace("{TimeRemaining}", _timeleft.ToString());
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase795), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                Exec(_cInfo, _message);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _message)
        {
            _message = _message.Replace("report ", "");
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 _pos = _player.position;
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfoAdmins = _cInfoList[i];
                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfoAdmins.playerId);
                if (Admin.PermissionLevel <= Admin_Level)
                {
                    string _phrase796;
                    if (!Phrases.Dict.TryGetValue(796, out _phrase796))
                    {
                        _phrase796 = "Report from {PlayerName}: {Message}";
                    }
                    _phrase796 = _phrase796.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase796 = _phrase796.Replace("{Message}", _message);
                    _cInfoAdmins.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase796), Config.Server_Response_Name, false, "ServerTools", false));
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
                _phrase797 = "{PlayerName} your report has been sent to online administrators and logged.";
            }
            _phrase797 = _phrase797.Replace("{PlayerName}", _cInfo.playerName);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase797), Config.Server_Response_Name, false, "ServerTools", false));
            string _sql = string.Format("UPDATE Players SET lastLog = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
            SQL.FastQuery(_sql);
            Log.Out(string.Format("[SERVERTOOLS] Report sent by player name {0}", _cInfo.playerName));
        }
    }
}

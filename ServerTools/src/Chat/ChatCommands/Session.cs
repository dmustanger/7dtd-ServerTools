using System;
using System.Data;

namespace ServerTools
{
    class Session
    {
        public static bool IsEnabled = false;

        public static void Exec(ClientInfo _cInfo)
        {
            DateTime _time;
            if (Players.Session.TryGetValue(_cInfo.playerId, out _time))
            {
                TimeSpan varTime = DateTime.Now - _time;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                string _sql = string.Format("SELECT sessionTime FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                int _sessionTime;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _sessionTime);
                _result.Dispose();
                _sessionTime = _sessionTime + _timepassed;
                string _phrase570;
                if (!Phrases.Dict.TryGetValue(570, out _phrase570))
                {
                    _phrase570 = "your current session is at {TimePassed} minutes. Your total session time is at {TotalTimePassed} minutes.";
                }
                _phrase570 = _phrase570.Replace("{TimePassed}", _timepassed.ToString());
                _phrase570 = _phrase570.Replace("{TotalTimePassed}", _sessionTime.ToString());
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase570 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
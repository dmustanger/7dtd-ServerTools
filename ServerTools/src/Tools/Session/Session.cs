using System;

namespace ServerTools
{
    class Session
    {
        public static bool IsEnabled = false;
        public static string Command105 = "session";

        public static void Exec(ClientInfo _cInfo)
        {
            DateTime _time;
            if (PersistentOperations.Session.TryGetValue(_cInfo.playerId, out _time))
            {
                TimeSpan varTime = DateTime.Now - _time;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                int _sessionTime = PersistentContainer.Instance.Players[_cInfo.playerId].SessionTime;
                _sessionTime = _sessionTime + _timepassed;
                Phrases.Dict.TryGetValue(791, out string _phrase791);
                _phrase791 = _phrase791.Replace("{TimePassed}", _timepassed.ToString());
                _phrase791 = _phrase791.Replace("{TotalTimePassed}", _sessionTime.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase791 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
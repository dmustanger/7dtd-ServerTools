using System;

namespace ServerTools
{
    class Session
    {
        public static bool IsEnabled = false;
        public static string Command_session = "session";

        public static void Exec(ClientInfo _cInfo)
        {
            if (GeneralFunction.Session.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime _time))
            {
                TimeSpan varTime = DateTime.Now - _time;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                int _sessionTime = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].SessionTime;
                _sessionTime = _sessionTime + _timepassed;
                Phrases.Dict.TryGetValue("Session1", out string _phrase);
                _phrase = _phrase.Replace("{TimePassed}", _timepassed.ToString());
                _phrase = _phrase.Replace("{TotalTimePassed}", _sessionTime.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
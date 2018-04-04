using System;

namespace ServerTools
{
    class RealWorldTime
    {
        public static bool IsEnabled = false;
        public static string Time_Zone = "UTC";

        public static void Time()
        {
            DateTime _time = DateTime.Now;
            string _phrase765;
            if (!Phrases.Dict.TryGetValue(765, out _phrase765))
            {
                _phrase765 = "The real world time is {Time} {TimeZone}.";
            }
            _phrase765 = _phrase765.Replace("{Time}", _time.ToShortTimeString());
            _phrase765 = _phrase765.Replace("{TimeZone}", Time_Zone);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase765), Config.Server_Response_Name, false, "", false);
        }
    }
}

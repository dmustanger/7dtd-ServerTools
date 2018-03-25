using System;

namespace ServerTools
{
    class RealWorldTime
    {
        public static bool IsEnabled = false;

        public static void Time()
        {
            DateTime _now = DateTime.Now;
            string _phrase765;
            if (!Phrases.Dict.TryGetValue(765, out _phrase765))
            {
                _phrase765 = "The real world time is {Time}.";
            }
            _phrase765 = _phrase765.Replace("{Time}", _now.ToShortTimeString());
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase765), "Server", false, "", false);
        }
    }
}

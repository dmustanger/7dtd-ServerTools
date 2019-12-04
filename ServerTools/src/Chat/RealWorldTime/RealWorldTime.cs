using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class RealWorldTime
    {
        public static bool IsEnabled = false;
        public static string Time_Zone = "UTC";
        public static int Delay = 60, Adjustment = 0;

        public static void Time()
        {
            DateTime _time = DateTime.Now;
            if (Adjustment != 0)
            {
                _time = DateTime.Now.AddHours(Adjustment);
            }
            string _phrase765;
            if (!Phrases.Dict.TryGetValue(765, out _phrase765))
            {
                _phrase765 = "The real world time is {Time} {TimeZone}.";
            }
            _phrase765 = _phrase765.Replace("{Time}", _time.ToShortTimeString());
            _phrase765 = _phrase765.Replace("{TimeZone}", Time_Zone);
            List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo = _cInfoList[i];
                if (_cInfo != null)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase765 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }
    }
}

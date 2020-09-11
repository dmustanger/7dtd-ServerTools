using System;
using System.Collections.Generic;

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
            Phrases.Dict.TryGetValue(823, out string _phrase823);
            _phrase823 = _phrase823.Replace("{Time}", _time.ToShortTimeString());
            _phrase823 = _phrase823.Replace("{TimeZone}", Time_Zone);
            List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
            if (_cInfoList != null && _cInfoList.Count > 0)
            {
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    if (_cInfo != null)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase823 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
        }
    }
}

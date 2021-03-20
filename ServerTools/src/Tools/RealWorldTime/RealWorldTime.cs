using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RealWorldTime
    {
        public static bool IsEnabled = false;
        public static string Time_Zone = "UTC";
        public static int Delay = 60, Adjustment = 0;

        public static void Exec()
        {
            DateTime _time = DateTime.Now;
            if (Adjustment != 0)
            {
                _time = DateTime.Now.AddHours(Adjustment);
            }
            Phrases.Dict.TryGetValue(471, out string _phrase471);
            _phrase471 = _phrase471.Replace("{Time}", _time.ToShortTimeString());
            _phrase471 = _phrase471.Replace("{TimeZone}", Time_Zone);
            List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
            if (_cInfoList != null && _cInfoList.Count > 0)
            {
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    if (_cInfo != null)
                    {
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase471 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
        }
    }
}

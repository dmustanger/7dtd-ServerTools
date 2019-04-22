using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class BreakTime
    {
        public static bool IsEnabled = false;
        public static int Break_Time = 60;
        public static string Break_Message = "It has been {Time} minutes since the last break reminder. Stretch and get some water.";

        public static void Exec()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                Break_Message = Break_Message.Replace("{Time}", Break_Time.ToString());
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + Break_Message + "[-]", 33, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}

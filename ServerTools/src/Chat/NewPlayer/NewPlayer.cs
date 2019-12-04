
namespace ServerTools
{
    class NewPlayer
    {
        public static bool IsEnabled = false;
        public static string Entry_Message = "*The stench of young flesh bares itself upon the world*...{PlayerName} has entered the world for the first time.";

        public static void Exec(ClientInfo _cInfo)
        {
            string Entry_Message2 = Entry_Message;
            Entry_Message2 = Entry_Message2.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + Entry_Message2 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }
    }
}


namespace ServerTools
{
    class NewPlayer
    {
        public static bool IsEnabled = false;
        public static string Entry_Message = "*The stench of young flesh bares itself upon the world*...{PlayerName} has entered the world for the first time.";

        public static void Exec(ClientInfo _cInfo)
        {
            Entry_Message = Entry_Message.Replace("{PlayerName}", _cInfo.playerName);
            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, Entry_Message), Config.Server_Response_Name, false, "ServerTools", true);
        }
    }
}

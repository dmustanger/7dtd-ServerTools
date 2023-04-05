using System.Collections.Generic;

namespace ServerTools
{
    class PlayerList
    {
        public static bool IsEnabled = false;
        public static string Command_playerlist = "playerlist", Command_plist = "plist";

        public static void Exec(ClientInfo _cInfo)
        {
            List<ClientInfo> clientList = GeneralOperations.ClientList();
            if (clientList != null)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    ClientInfo cInfo2 = clientList[i];
                    if (_cInfo.entityId != cInfo2.entityId)
                    {
                        Phrases.Dict.TryGetValue("PlayerList1", out string phrase);
                        phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                        phrase = phrase.Replace("{EntityId}", cInfo2.entityId.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("PlayerList2", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

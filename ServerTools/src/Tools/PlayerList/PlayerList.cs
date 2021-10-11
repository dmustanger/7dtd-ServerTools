using System.Collections.Generic;

namespace ServerTools
{
    class PlayerList
    {
        public static bool IsEnabled = false;
        public static string Command_playerlist = "playerlist";

        public static void Exec(ClientInfo _cInfo)
        {
            List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
            if (ClientInfoList != null && ClientInfoList.Count > 1)
            {
                for (int i = 0; i < ClientInfoList.Count; i++)
                {
                    ClientInfo _cInfo1 = ClientInfoList[i];
                    if (_cInfo.entityId != _cInfo1.entityId)
                    {
                        Phrases.Dict.TryGetValue("PlayerList1", out string _phrase);
                        _phrase = _phrase.Replace("{PlayerName}", _cInfo1.playerName);
                        _phrase = _phrase.Replace("{EntityId}", _cInfo1.entityId.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("PlayerList2", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

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
            if (ClientInfoList != null && ClientInfoList.Count > 0)
            {
                for (int i = 0; i < ClientInfoList.Count; i++)
                {
                    ClientInfo _cInfo1 = ClientInfoList[i];
                    if (_cInfo.entityId != _cInfo1.entityId)
                    {
                        Phrases.Dict.TryGetValue(811, out string _phrase811);
                        _phrase811 = _phrase811.Replace("{PlayerName}", _cInfo1.playerName);
                        _phrase811 = _phrase811.Replace("{EntityId}", _cInfo1.entityId.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase811 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(812, out string _phrase812);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase812 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

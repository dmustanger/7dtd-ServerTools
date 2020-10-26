using System;
using System.IO;

namespace ServerTools
{
    class Packages
    {
        private static string _file = string.Format("PacketManipulationLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/Logs/PacketManipulationLogs/{1}", API.ConfigPath, _file);

        public static void Ban(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue(1031, out string _phrase1031);
            if (_cInfo.ownerId != _cInfo.playerId)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 50 years \"{1}\"", _cInfo.ownerId, _phrase1031), null);
            }
            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 50 years \"{1}\"", _cInfo.playerId, _phrase1031), null);
            Phrases.Dict.TryGetValue(1032, out string _phrase1032);
            _phrase1032 = _phrase1032.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase1032 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        public static void Writer(ClientInfo _cInfo, string _action)
        {
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("{0}: {1} {2} {3} Net package manipulation: {4}", DateTime.Now, _cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, _action));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }
    }
}

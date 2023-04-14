using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class GodMode
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0;

        private static string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static void CheckForGodMode(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (_player.Buffs.HasBuff("god"))
            {
                Phrases.Dict.TryGetValue("Godmode2", out string phrase);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' using god mode @ '{3}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _player.position));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Log.Warning(string.Format("[SERVERTOOLS] Detected id '{0}' '{1}' named '{2}' using god mode @ '{3}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _player.position));
                Phrases.Dict.TryGetValue("Godmode1", out phrase);
                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}

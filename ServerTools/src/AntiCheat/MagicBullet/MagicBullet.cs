using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class MagicBullet
    {
        public static bool IsEnabled = false;
        public static List<int> Kill = new List<int>();

        private static string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static bool IsValid(EntityAlive killer)
        {
            try
            {
                if (!Kill.Contains(killer.entityId))
                {
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(killer.entityId);
                    if (cInfo != null)
                    {
                        Phrases.Dict.TryGetValue("MagicBullet1", out string phrase);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                        using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("Detected Id '{0}' '{1}' named '{2}' manipulating net packages", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Log.Warning("[SERVERTOOLS] Detected Id '{0}' '{1}' named '{2}' manipulating net packages. They have been banned", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName);
                        Phrases.Dict.TryGetValue("MagicBullet2", out phrase);
                        phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MagicBullet.IsValid: {0}", e.Message));
            }
            return true;
        }
    }
}

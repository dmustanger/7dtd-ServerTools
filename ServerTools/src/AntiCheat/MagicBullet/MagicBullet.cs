using HarmonyLib;
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

        public static AccessTools.FieldRef<NetPackageEntityAddScoreClient, int> entityId = AccessTools.FieldRefAccess<NetPackageEntityAddScoreClient, int>("entityId");
        public static AccessTools.FieldRef<NetPackageEntityAddScoreClient, int> playerKills = AccessTools.FieldRefAccess<NetPackageEntityAddScoreClient, int>("playerKills");

        private static string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string Filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static bool Exec(NetPackageEntityAddScoreServer _instance)
        {
            try
            {
                if (_instance.Sender.entityId == entityId(_instance) && playerKills(_instance) == 1 && !Kill.Contains(entityId(_instance)))
                {
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId(_instance));
                    if (cInfo != null)
                    {
                        Phrases.Dict.TryGetValue("MagicBullet1", out string phrase);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("Detected Id '{0}' '{1}' named '{2}' running magic bullet hack", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Log.Warning("[SERVERTOOLS] Detected Id '{0}' '{1}' named '{2}' running magic bullet hack. They have been banned", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName);
                        Phrases.Dict.TryGetValue("MagicBullet2", out phrase);
                        phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MagicBullet.Exec: {0}", e.Message));
            }
            return false;
        }
    }
}

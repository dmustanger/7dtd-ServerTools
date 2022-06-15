using HarmonyLib;
using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class PlayerDataPackage
    {
        private static AccessTools.FieldRef<NetPackagePlayerData, PlayerDataFile> playerDataFile = AccessTools.FieldRefAccess<NetPackagePlayerData, PlayerDataFile>("playerDataFile");
        private static readonly string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static bool IsValid(NetPackagePlayerData __instance)
        {
            try
            {
                if (__instance.Sender.entityId != playerDataFile(__instance).ecd.clientEntityId)
                {
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(playerDataFile(__instance).ecd.clientEntityId);
                    if (cInfo != null)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for packet manipulation\"", __instance.Sender.CrossplatformId.CombinedString), null);
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("Detected invalid player data update by '{0}' '{1}' named '{2}' against '{3}' '{4}' named '{5}'", __instance.Sender.PlatformId.CombinedString, __instance.Sender.CrossplatformId.CombinedString, __instance.Sender.playerName, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Log.Out(string.Format("[SERVERTOOLS] Detected invalid player data update by '{0}' '{1}' named '{2}' against '{3}' '{4}' named '{5}'. Client has been banned", __instance.Sender.PlatformId.CombinedString, __instance.Sender.CrossplatformId.CombinedString, __instance.Sender.playerName, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerDataPackage.IsValid: {0}", e.Message));
            }
            return true;
        }
    }
}

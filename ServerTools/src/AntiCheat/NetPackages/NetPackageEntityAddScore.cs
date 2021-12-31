using HarmonyLib;
using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class EntityAddScoreClientPackage
    {

        private static AccessTools.FieldRef<NetPackageEntityAddScoreClient, int> entityId = AccessTools.FieldRefAccess<NetPackageEntityAddScoreClient, int>("entityId");
        private static readonly string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static bool IsValid(NetPackageEntityAddScoreClient __instance)
        {
            try
            {
                if (__instance.Sender.entityId != entityId(__instance))
                {
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId(__instance));
                    if (cInfo != null)
                    {
                        GameUtils.KickPlayerForClientInfo(__instance.Sender, new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, 0, DateTime.Now.AddDays(1095), "Auto detection has banned you for packet manipulation"));
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("Detected invalid player score update by '{0}' '{1}' named '{2}' against '{3}' '{4}' named '{5}'", __instance.Sender.PlatformId.CombinedString, __instance.Sender.CrossplatformId.CombinedString, __instance.Sender.playerName, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Log.Out(string.Format("[SERVERTOOLS] Detected invalid player score update by '{0}' '{1}' named '{2}' against '{3}' '{4}' named '{5}'. Client has been banned", __instance.Sender.PlatformId.CombinedString, __instance.Sender.CrossplatformId.CombinedString, __instance.Sender.playerName, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityAddScorePackage.IsValid: {0}", e.Message));
            }
            return true;
        }
    }
}

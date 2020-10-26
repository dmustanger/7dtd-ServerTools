using System;

namespace ServerTools
{
    class PackageChunk
    {
        public static bool PackageChunk_ProcessPackage_Prefix(NetPackageChunk __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                if (!GameManager.Instance.adminTools.IsAdmin(_cInfo))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageChunk uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted sending chunk data without permission", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName));
                    Packages.Ban(_cInfo);
                    Packages.Writer(_cInfo, string.Format("Attempted sending chunk data without permission"));
                    return false;
                }
            }
            return true;
        }
    }
}

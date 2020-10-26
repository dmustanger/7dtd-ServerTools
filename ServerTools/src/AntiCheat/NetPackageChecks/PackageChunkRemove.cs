using System;

namespace ServerTools
{
    class PackageChunkRemove
    {
        public static bool PackageChunkRemove_ProcessPackage_Prefix(NetPackageChunkRemove __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                if (!GameManager.Instance.adminTools.IsAdmin(_cInfo))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageChunkRemove uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted removing a chunk from the world without permission", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName));
                    Packages.Ban(_cInfo);
                    Packages.Writer(_cInfo, string.Format("Attempted removing a chunk from the world without permission"));
                    return false;
                }
            }
            return true;
        }
    }
}

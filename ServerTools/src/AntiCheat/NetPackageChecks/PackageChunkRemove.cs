using System;

namespace ServerTools
{
    class PackageChunkRemove
    {
        public static bool PackageChunkRemove_ProcessPackage_Prefix(NetPackageChunkRemove __instance, World _world)
        {
            try
            {
                if (__instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    if (!GameManager.Instance.adminTools.IsAdmin(_cInfo))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageChunkRemove uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted removing a chunk from the world without permission", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted removing a chunk from the world without permission"));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PackagePersistentPlayerState.PackagePersistentPlayerState_ProcessPackage_Prefix: {0}", e.Message));
            }
            return true;
        }
    }
}

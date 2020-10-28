using System;

namespace ServerTools
{
    class PackageChunk
    {
        public static bool PackageChunk_ProcessPackage_Prefix(NetPackageChunk __instance, World _world)
        {
            try
            {
                if (__instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    if (!GameManager.Instance.adminTools.IsAdmin(_cInfo))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageChunk uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted sending chunk data without permission", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted sending chunk data without permission"));
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

using System;

namespace ServerTools
{
    class PackageWorldTime
    {
        public static bool PackageWorldTime_ProcessPackage_Prefix(NetPackageWorldTime __instance, World _world)
        {
            try
            {
                if (Packages.IsEnabled && __instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    if (!GameManager.Instance.adminTools.IsAdmin(_cInfo))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageWorldTime uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted to change the world time without permission", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted to change the world time without permission"));
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

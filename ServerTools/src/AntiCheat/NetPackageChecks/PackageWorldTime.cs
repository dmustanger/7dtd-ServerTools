using System;

namespace ServerTools
{
    class PackageWorldTime
    {
        public static bool PackageWorldTime_ProcessPackage_Prefix(NetPackageWorldTime __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                if (!GameManager.Instance.adminTools.IsAdmin(_cInfo))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageWorldTime uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted to change the world time without permission", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName));
                    Packages.Ban(_cInfo);
                    Packages.Writer(_cInfo, string.Format("Attempted to change the world time without permission"));
                    return false;
                }
            }
            return true;
        }
    }
}

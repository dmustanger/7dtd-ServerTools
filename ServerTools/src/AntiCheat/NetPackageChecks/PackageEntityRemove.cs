using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageEntityRemove
    {
        static AccessTools.FieldRef<NetPackageEntityRemove, int> _entityId = AccessTools.FieldRefAccess<NetPackageEntityRemove, int>("entityId");

        public static bool PackageEntityRemove_ProcessPackage_Prefix(NetPackageEntityRemove __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                if (!GameManager.Instance.adminTools.IsAdmin(_cInfo))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageEntityRemove uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempting to remove entity id {4} without permission", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityId(__instance)));
                    Packages.Ban(_cInfo);
                    Packages.Writer(_cInfo, string.Format("Attempted to remove entity id {0} without permission", _entityId(__instance)));
                    return false;
                }
            }
            return true;
        }
    }
}

using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageItemReload
    {
        static AccessTools.FieldRef<NetPackageItemReload, int> _entityId = AccessTools.FieldRefAccess<NetPackageItemReload, int>("entityId");

        public static bool PackageItemReload_ProcessPackage_Prefix(NetPackageItemReload __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                if (_cInfo.entityId != _entityId(__instance))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageItemReload uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted triggering item reload with mismatched entity id target {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityId(__instance)));
                    Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                    Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted triggering item reload with mismatched entity id target {0}", _entityId(__instance)));
                    return false;
                }
            }
            return true;
        }
    }
}

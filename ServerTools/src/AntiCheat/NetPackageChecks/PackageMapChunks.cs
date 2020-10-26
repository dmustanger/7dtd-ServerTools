using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageMapChunks
    {
        static AccessTools.FieldRef<NetPackageMapChunks, int> _entityId = AccessTools.FieldRefAccess<NetPackageMapChunks, int>("entityId");

        public static bool PackageMapChunks_ProcessPackage_Prefix(NetPackageMapChunks __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                if (_cInfo.entityId != _entityId(__instance))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageMapChunks uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying their entity id to {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityId(__instance)));
                    Packages.Ban(_cInfo);
                    Packages.Writer(_cInfo, string.Format("Attempted modifying their entity id to {0}", _entityId(__instance)));
                    return false;
                }
            }
            return true;
        }
    }
}

using System;

namespace ServerTools
{
    class PackageChunkRemoveAll
    {
        public static bool PackageChunkRemoveAll_ProcessPackage_Prefix(NetPackageChunkRemoveAll __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                if (!GameManager.Instance.adminTools.IsAdmin(_cInfo))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageChunkRemoveAll uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted to remove all chunks without perission", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName));
                    Packages.Ban(_cInfo);
                    Packages.Writer(_cInfo, string.Format("Attempted to remove all chunks without perission"));
                    return false;
                }
            }
            return true;
        }
    }
}

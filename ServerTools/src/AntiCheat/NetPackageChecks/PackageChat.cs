using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageChat
    {
        static AccessTools.FieldRef<NetPackageChat, int> _entityId = AccessTools.FieldRefAccess<NetPackageChat, int>("senderEntityId");
        static AccessTools.FieldRef<NetPackageChat, string> _mainName = AccessTools.FieldRefAccess<NetPackageChat, string>("mainName");

        public static bool PackageChat_ProcessPackage_Prefix(NetPackageChat __instance, World _world)
        {
            try
            {
                if (__instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    if (_cInfo.entityId != _entityId(__instance))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageChat uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Modifying their entity id to {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityId(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Modifying their entity id to {0}", _entityId(__instance)));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        return false;
                    }
                    if (_cInfo.playerName != _mainName(__instance))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageChat uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Modifying their user name to {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _mainName(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Modifying their user name to {0}", _mainName(__instance)));
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

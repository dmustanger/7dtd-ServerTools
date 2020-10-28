using HarmonyLib;
using System;

namespace ServerTools
{
    class PackagePlayerId
    {
        static AccessTools.FieldRef<NetPackagePlayerId, int> _entityId = AccessTools.FieldRefAccess<NetPackagePlayerId, int>("id");
        static AccessTools.FieldRef<NetPackagePlayerId, PlayerDataFile> _playerDataFile = AccessTools.FieldRefAccess<NetPackagePlayerId, PlayerDataFile>("playerDataFile");

        public static bool PackagePlayerId_ProcessPackage_Prefix(NetPackagePlayerId __instance, World _world)
        {
            try
            {
                if (Packages.IsEnabled && __instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    if (_cInfo.entityId != _entityId(__instance))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePlayerId uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying their entity id to {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityId(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted modifying their entity id to {0}", _entityId(__instance)));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        return false;
                    }
                    if (_cInfo.entityId != _playerDataFile(__instance).ecd.id)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePlayerId uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying mismatched player data file entity id {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _playerDataFile(__instance).ecd.id));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted modifying mismatched player data file entity id {0}", _playerDataFile(__instance).ecd.id));
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

using HarmonyLib;
using System;

namespace ServerTools
{
    class PackagePlayerData
    {
        static AccessTools.FieldRef<NetPackagePlayerData, PlayerDataFile> _playerDataFile = AccessTools.FieldRefAccess<NetPackagePlayerData, PlayerDataFile>("playerDataFile");

        public static bool PackagePlayerData_ProcessPackage_Prefix(NetPackagePlayerData __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                EntityAlive _entityAlive = _world.GetEntity(_playerDataFile(__instance).ecd.id) as EntityAlive;
                if (_entityAlive != null)
                {
                    if (_cInfo.entityId != _entityAlive.entityId)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePlayerData uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying their entity id to {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityAlive.entityId));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted modifying their entity id to {0}", _entityAlive.entityId));
                        return false;
                    }
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePlayerData uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying their entity id to a non existent entity with id {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _playerDataFile(__instance).ecd.id));
                    Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                    Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted modifying their entity id to a non existent entity with id {0}", _playerDataFile(__instance).ecd.id));
                    return false;
                }
            }
            return true;
        }
    }
}

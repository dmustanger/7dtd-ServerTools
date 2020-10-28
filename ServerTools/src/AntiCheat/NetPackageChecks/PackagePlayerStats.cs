using HarmonyLib;
using System;

namespace ServerTools
{
    class PackagePlayerStats
    {
        static AccessTools.FieldRef<NetPackagePlayerStats, int> _entityId = AccessTools.FieldRefAccess<NetPackagePlayerStats, int>("entityId");
        static AccessTools.FieldRef<NetPackagePlayerStats, string> _entityName = AccessTools.FieldRefAccess<NetPackagePlayerStats, string>("entityName");

        public static bool PackagePlayerStats_ProcessPackage_Prefix(NetPackagePlayerStats __instance, World _world)
        {
            try
            {
                if (Packages.IsEnabled && __instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    EntityAlive _entityAlive = _world.GetEntity(_entityId(__instance)) as EntityAlive;
                    if (_entityAlive != null)
                    {
                        if (_cInfo.entityId != _entityAlive.entityId)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePlayerStats uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying their entity id to {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityId(__instance)));
                            Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted modifying their entity id to {0}", _entityId(__instance)));
                            Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                            return false;
                        }
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePlayerStats uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying their entity id to a non existent entity with id {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityName(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted modifying their entity id to a non existent entity with id {0}", _entityId(__instance)));
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

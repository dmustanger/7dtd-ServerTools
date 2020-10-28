using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageEntityStatsBuff
    {
        static AccessTools.FieldRef<NetPackageEntityStatsBuff, int> _entityId = AccessTools.FieldRefAccess<NetPackageEntityStatsBuff, int>("m_entityId");

        public static bool PackageEntityStatsBuff_ProcessPackage_Prefix(NetPackageEntityStatsBuff __instance, World _world)
        {
            try
            {
                if (Packages.IsEnabled && __instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    EntityAlive _sendingEntity = _world.GetEntity(_cInfo.entityId) as EntityAlive;
                    if (_sendingEntity != null && _sendingEntity.AttachedToEntity == null)
                    {
                        EntityAlive _entityAlive = _world.GetEntity(_entityId(__instance)) as EntityAlive;
                        if (_entityAlive != null)
                        {
                            if (_cInfo.entityId != _entityAlive.entityId)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageEntityStatsBuff uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying their entity id to {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityAlive.entityId));
                                Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted modifying their entity id to {0}", _entityAlive.entityId));
                                Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                                return false;
                            }
                        }
                        else
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageEntityStatsBuff uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying their entity id to a non existent entity with id {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityId(__instance)));
                            Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted modifying their entity id to a non existent entity with id {0}", _entityId(__instance)));
                            Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                            return false;
                        }
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

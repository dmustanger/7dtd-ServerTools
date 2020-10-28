using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageAddRemoveBuff
    {
        static AccessTools.FieldRef<NetPackageAddRemoveBuff, int> _entityId = AccessTools.FieldRefAccess<NetPackageAddRemoveBuff, int>("m_entityId");
        static AccessTools.FieldRef<NetPackageAddRemoveBuff, string> _buffName = AccessTools.FieldRefAccess<NetPackageAddRemoveBuff, string>("buffName");

        public static bool PackageAddRemoveBuff_ProcessPackage_Prefix(NetPackageAddRemoveBuff __instance, World _world)
        {
            try
            {
                if (Packages.IsEnabled && __instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    EntityAlive _entityAlive = _world.GetEntity(_entityId(__instance)) as EntityAlive;
                    if (_entityAlive != null)
                    {
                        if (!GameManager.Instance.adminTools.IsAdmin(_cInfo))
                        {
                            string _buff = _buffName(__instance).ToLower();
                            if (Packages.Dict.Contains(_buff))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageAddRemoveBuff uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempting to apply or remove '{4}' buff without permission to entity id {5}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _buff, _entityAlive.entityId));
                                Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempting to apply or remove '{0}' buff without permission to entity id {1}", _buff, _entityAlive.entityId));
                                Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                                return false;
                            }
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

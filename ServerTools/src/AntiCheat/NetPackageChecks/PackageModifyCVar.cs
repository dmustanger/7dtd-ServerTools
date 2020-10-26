using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageModifyCVar
    {
        static AccessTools.FieldRef<NetPackageModifyCVar, int> _entityId = AccessTools.FieldRefAccess<NetPackageModifyCVar, int>("m_entityId");
        static AccessTools.FieldRef<NetPackageModifyCVar, string> _cvarName = AccessTools.FieldRefAccess<NetPackageModifyCVar, string>("cvarName");
        static AccessTools.FieldRef<NetPackageModifyCVar, float> _value = AccessTools.FieldRefAccess<NetPackageModifyCVar, float>("value");

        public static bool PackageModifyCVar_ProcessPackage_Prefix(NetPackageModifyCVar __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                EntityAlive _entityAlive = _world.GetEntity(_entityId(__instance)) as EntityAlive;
                if (_entityAlive != null && _entityAlive.IsClientControlled())
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageModifyCVar uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying entity id {4} cvar named {5} to value {6}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityId(__instance), _cvarName(__instance), _value(__instance)));
                    //Packages.Ban(_cInfo);
                    //Packages.Writer(_cInfo, string.Format("Attempted modifying their entity id to {0}", _entityId(__instance)));
                    //return false;
                }
            }
            return true;
        }
    }
}

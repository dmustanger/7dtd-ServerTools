using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageDamageEntity
    {
        static AccessTools.FieldRef<NetPackageDamageEntity, int> _targetId = AccessTools.FieldRefAccess<NetPackageDamageEntity, int>("entityId");
        static AccessTools.FieldRef<NetPackageDamageEntity, int> _attackerId = AccessTools.FieldRefAccess<NetPackageDamageEntity, int>("attackerEntityId");
        static AccessTools.FieldRef<NetPackageDamageEntity, EnumDamageSource> _damageSource = AccessTools.FieldRefAccess<NetPackageDamageEntity, EnumDamageSource>("damageSrc");
        static AccessTools.FieldRef<NetPackageDamageEntity, EnumDamageTypes> _damageType = AccessTools.FieldRefAccess<NetPackageDamageEntity, EnumDamageTypes>("damageTyp");
        static AccessTools.FieldRef<NetPackageDamageEntity, ushort> _strength = AccessTools.FieldRefAccess<NetPackageDamageEntity, ushort>("strength");

        public static bool PackageDamageEntity_ProcessPackage_Prefix(NetPackageDamageEntity __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                //Log.Out(string.Format("[SERVERTOOLS] NetPackageDamageEntity uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. _targetId = {4} _attackerId {5} _damageSource {6} _damageType {7} _strength {8}", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName, _targetId(__instance), _attackerId(__instance), _damageSource(__instance), _damageType(__instance), _strength(__instance)));
                EntityAlive _entityAlive = _world.GetEntity(_targetId(__instance)) as EntityAlive;
                if (_entityAlive != null)
                {
                    if (_damageType(__instance) == EnumDamageTypes.Suicide && __instance.Sender.entityId != _entityAlive.entityId && !GameManager.Instance.adminTools.CommandAllowedFor(new string[] { "kill" }, _cInfo))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageDamageEntity uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted running suicide on entity id {4} without permission", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName, _targetId(__instance)));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted running suicide on entity id {0} without permission", _targetId(__instance)));
                        return false;
                    }
                    else if (_strength(__instance) >= 3000 && !GameManager.Instance.adminTools.IsAdmin(_cInfo))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageDamageEntity uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted running excessive damage of {4} on entity id {5} and is not an admin", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName, _strength(__instance), _targetId(__instance)));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted running excessive damage of {0} on entity id {1} and is not an admin", _strength(__instance), _targetId(__instance)));
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

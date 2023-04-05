using HarmonyLib;
using System;

namespace ServerTools
{
    class NoVehicleDrone
    {
        public static bool IsEnabled = false;

        public static AccessTools.FieldRef<NetPackageEntityAttach, int> riderId = AccessTools.FieldRefAccess<NetPackageEntityAttach, int>("riderId");
        private static AccessTools.FieldRef<NetPackageEntityAttach, int> vehicleId = AccessTools.FieldRefAccess<NetPackageEntityAttach, int>("vehicleId");

        public static bool Exec(NetPackageEntityAttach __instance, EntityPlayer _player)
        {
            Entity entity = GeneralOperations.GetEntity(vehicleId(__instance));
            if (entity != null)//if null, they are most likely dettaching
            {
                OwnedEntityData[] ownedEntities = _player.GetOwnedEntities();
                if (ownedEntities != null && ownedEntities.Length > 0)
                {
                    for (int i = 0; i < ownedEntities.Length; i++)
                    {
                        OwnedEntityData entityData = ownedEntities[i];
                        Entity ownedEntity = GeneralOperations.GetEntity(entityData.Id);
                        if (ownedEntity != null && ownedEntity is EntityDrone && entity is EntityVehicle)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}

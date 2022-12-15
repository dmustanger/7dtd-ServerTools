using System;
using System.Collections.Generic;

namespace ServerTools
{
    class DroppedBagProtection
    {
        public static bool IsEnabled = false, Friend_Access = false;

        public static bool IsAllowed(int _entityIdThatOpenedIt, TileEntityLootContainer lootContainer)
        {
            Entity entity = GameManager.Instance.World.GetEntity(lootContainer.entityId);
            if (entity == null && entity is EntityBackpack)
            {
                EntityBackpack backpack = entity as EntityBackpack;
                if (backpack.RefPlayerId == _entityIdThatOpenedIt)
                {
                    return true;
                }
                else if (Friend_Access)
                {
                    PersistentPlayerData ppdAccess = GeneralFunction.GetPersistentPlayerDataFromEntityId(_entityIdThatOpenedIt);
                    PersistentPlayerData ppdOwner = GeneralFunction.GetPersistentPlayerDataFromEntityId(backpack.RefPlayerId);
                    if (ppdAccess != null && ppdOwner != null)
                    {
                        if (ppdAccess.ACL.Contains(ppdAccess.UserIdentifier) && ppdOwner.ACL.Contains(ppdOwner.UserIdentifier))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

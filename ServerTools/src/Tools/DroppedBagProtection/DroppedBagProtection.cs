using System;
using System.Collections.Generic;

namespace ServerTools
{
    class DroppedBagProtection
    {
        public static bool IsEnabled = false, Friend_Access = false;
        public static Dictionary<int, int> Backpacks = new Dictionary<int, int>();

        public static void BuildList()
        {
            if (PersistentContainer.Instance.Backpacks != null && PersistentContainer.Instance.Backpacks.Count > 0)
            {
                Backpacks = PersistentContainer.Instance.Backpacks;
            }
        }

        public static bool IsAllowed(int _entityIdThatOpenedIt, TileEntityLootContainer lootContainer)
        {
            if (Backpacks.ContainsKey(lootContainer.entityId))
            {
                Backpacks.TryGetValue(lootContainer.entityId, out int ownerId);
                if (_entityIdThatOpenedIt == ownerId)
                {
                    Backpacks.Remove(lootContainer.entityId);
                    PersistentContainer.Instance.Backpacks.Remove(lootContainer.entityId);
                    PersistentContainer.DataChange = true;
                    return true;
                }
                else if (Friend_Access)
                {
                    PersistentPlayerData ppdAccess = GeneralFunction.GetPersistentPlayerDataFromEntityId(_entityIdThatOpenedIt);
                    PersistentPlayerData ppdOwner = GeneralFunction.GetPersistentPlayerDataFromEntityId(ownerId);
                    if (ppdAccess != null && ppdOwner != null)
                    {
                        if (ppdAccess.ACL.Contains(ppdAccess.UserIdentifier) && ppdOwner.ACL.Contains(ppdOwner.UserIdentifier))
                        {
                            Backpacks.Remove(lootContainer.entityId);
                            PersistentContainer.Instance.Backpacks.Remove(lootContainer.entityId);
                            PersistentContainer.DataChange = true;
                            return true;
                        }
                    }
                }
                return false;
            }
            return true;
        }
    }
}

using UnityEngine;

namespace ServerTools
{
    public class FirstClaimBlock
    {
        public static bool IsEnabled = false;

        public static void firstClaim(ClientInfo _cInfo)
        {
            World world = GameManager.Instance.World;
            int worldSeed = world.Seed;
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p == null || p.WorldSeedFirstClaim != worldSeed)
            {
                string claimBlock = "keystoneBlock";
                ItemValue itemValue;
                itemValue = new ItemValue(ItemClass.GetItem(claimBlock).type, 1, 1, true);

                if (Equals(itemValue, ItemValue.None))
                {
                    SdtdConsole.Instance.Output(string.Format("Unable to find item {0} for /claim command", claimBlock));
                    return;
                }
                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                {
                    entityClass = EntityClass.FromString("item"),
                    id = EntityFactory.nextEntityID++,
                    itemStack = new ItemStack(itemValue, 1),
                    pos = world.Players.dict[_cInfo.entityId].position,
                    rot = new Vector3(20f, 0f, 20f),
                    lifetime = 60f,
                    belongsPlayerId = _cInfo.entityId
                });
                world.SpawnEntityInWorld(entityItem);
                _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Claim block has been added to your inventory or if inventory is full, it dropped at your feet.[-]", Config.ChatColor), "Server", false, "", false));
                PersistentContainer.Instance.Players[_cInfo.playerId, true].WorldSeedFirstClaim = worldSeed;
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have already received your first claim block. Contact an administrator if you require help.[-]", Config.ChatColor), "Server", false, "", false));
            }
        }
    }
}

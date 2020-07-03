using UnityEngine;

namespace ServerTools
{
    public class FirstClaimBlock
    {
        public static bool IsEnabled = false;
        public static string Command32 = "claim";

        public static void firstClaim(ClientInfo _cInfo)
        {
            bool _firstClaim = PersistentContainer.Instance.Players[_cInfo.playerId].FirstClaimBlock;
            if (!_firstClaim)
            {
                World world = GameManager.Instance.World;
                string claimBlock = "keystoneBlock";
                ItemValue itemValue;
                itemValue = new ItemValue(ItemClass.GetItem(claimBlock).type, 1, 1, true);

                if (Equals(itemValue, ItemValue.None))
                {
                    SdtdConsole.Instance.Output(string.Format("Unable to find block {0} for /{1} command", claimBlock, Command32));
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
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Claim block has been added to your inventory or if inventory is full, it dropped at your feet.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                PersistentContainer.Instance.Players[_cInfo.playerId].FirstClaimBlock = true;
                PersistentContainer.Instance.Save();
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have already received your first claim block. Contact an administrator if you require help.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

using UnityEngine;

namespace ServerTools
{
    public class FirstClaimBlock
    {
        public static bool IsEnabled = false;
        public static string Command_claim = "claim";

        public static void FirstClaim(ClientInfo _cInfo)
        {
            bool _firstClaim = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].FirstClaimBlock;
            if (!_firstClaim)
            {
                World world = GameManager.Instance.World;
                string claimBlock = "keystoneBlock";
                ItemValue itemValue;
                itemValue = new ItemValue(ItemClass.GetItem(claimBlock).type, 1, 1, true);

                if (Equals(itemValue, ItemValue.None))
                {
                    SdtdConsole.Instance.Output(string.Format("Unable to find block {0} for /{1} command", claimBlock, Command_claim));
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
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].FirstClaimBlock = true;
                PersistentContainer.DataChange = true;
                Phrases.Dict.TryGetValue("FirstClaimBlock1", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("FirstClaimBlock2", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

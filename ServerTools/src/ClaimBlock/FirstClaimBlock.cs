using System.Data;
using UnityEngine;

namespace ServerTools
{
    public class FirstClaimBlock
    {
        public static bool IsEnabled = false;

        public static void firstClaim(ClientInfo _cInfo)
        {
            
            string _sql = string.Format("SELECT firstClaim FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            bool _firstClaim;
            bool.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _firstClaim);
            _result.Dispose();
            if (!_firstClaim)
            {
                World world = GameManager.Instance.World;
                string claimBlock = "keystoneBlock";
                ItemValue itemValue;
                itemValue = new ItemValue(ItemClass.GetItem(claimBlock).type, 1, 1, true);

                if (Equals(itemValue, ItemValue.None))
                {
                    SdtdConsole.Instance.Output(string.Format("Unable to find block {0} for /claim command", claimBlock));
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
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", claim block has been added to your inventory or if inventory is full, it dropped at your feet.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                _sql = string.Format("UPDATE Players SET firstClaim = 'true' WHERE steamid = '{0}'", _cInfo.playerId);
                SQL.FastQuery(_sql);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you have already received your first claim block. Contact an administrator if you require help.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}

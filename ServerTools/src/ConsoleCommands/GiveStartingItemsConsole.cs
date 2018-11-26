using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class GiveStartingItemsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Gives the starting items from the list directly to a players inventory. Drops to the ground if full.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. givestartingitems <steamId/entityId>\n" +
                "1. Gives a player the item(s) from the StatingItems.xml to their inventory unless full. Drops to the ground when full.\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-GiveStartingItems", "givestartingitems", "gsi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (StartingItems.IsEnabled)
            {
                try
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    if (_params[0].Length < 1 || _params[0].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not give starting items to Id: Invalid Id {0}", _params[0]));
                        return;
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (_cInfo != null)
                        {
                            Send(_cInfo);
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with Id {0} does not exist", _params[0]));
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in GiveStartingItemsConsole.Run: {0}.", e));
                }
            }
        }

        public static void Send(ClientInfo _cInfo)
        {
            if (StartingItems.startItemList.Count > 0)
            {
                World world = GameManager.Instance.World;
                foreach (KeyValuePair<string, int[]> kvp in StartingItems.startItemList)
                {
                    ItemValue _itemValue = new ItemValue(ItemClass.GetItem(kvp.Key).type, kvp.Value[1], kvp.Value[1], true);
                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                    {
                        entityClass = EntityClass.FromString("item"),
                        id = EntityFactory.nextEntityID++,
                        itemStack = new ItemStack(_itemValue, kvp.Value[0]),
                        pos = world.Players.dict[_cInfo.entityId].position,
                        rot = new Vector3(20f, 0f, 20f),
                        lifetime = 60f,
                        belongsPlayerId = _cInfo.entityId
                    });
                    world.SpawnEntityInWorld(entityItem);
                    _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                    SdtdConsole.Instance.Output(string.Format("Spawned starting item {0} for {1}.", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name, _cInfo.playerName));
                    Log.Out(string.Format("[SERVERTOOLS] Spawned starting item {0} for {1}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name, _cInfo.playerName));
                }
                string _phrase806;
                if (!Phrases.Dict.TryGetValue(806, out _phrase806))
                {
                    _phrase806 = "you have received the starting items. Check your inventory. If full, check the ground.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase806 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

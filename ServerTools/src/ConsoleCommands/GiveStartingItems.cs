using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class GiveStartingItems : ConsoleCmdAbstract
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
                            if (StartingItems.startItemList.Count > 0)
                            {
                                World world = GameManager.Instance.World;
                                foreach (KeyValuePair<string, int[]> kvp in StartingItems.startItemList)
                                {
                                    ItemValue itemValue;
                                    var itemId = 4096;
                                    int _itemId;
                                    if (int.TryParse(kvp.Key, out _itemId))
                                    {
                                        int calc = (_itemId + 4096);
                                        itemId = calc;
                                        itemValue = ItemClass.list[itemId] == null ? ItemValue.None : new ItemValue(itemId, kvp.Value[1], kvp.Value[1], true);
                                    }
                                    else
                                    {
                                        if (!ItemClass.ItemNames.Contains(kvp.Key))
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Unable to find item {0}", kvp.Key));
                                        }

                                        itemValue = new ItemValue(ItemClass.GetItem(kvp.Key).type, kvp.Value[1], kvp.Value[1], true);
                                    }

                                    if (Equals(itemValue, ItemValue.None))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Unable to find item {0}", kvp.Key));
                                    }
                                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                    {
                                        entityClass = EntityClass.FromString("item"),
                                        id = EntityFactory.nextEntityID++,
                                        itemStack = new ItemStack(itemValue, kvp.Value[0]),
                                        pos = world.Players.dict[_cInfo.entityId].position,
                                        rot = new Vector3(20f, 0f, 20f),
                                        lifetime = 60f,
                                        belongsPlayerId = _cInfo.entityId
                                    });
                                    world.SpawnEntityInWorld(entityItem);
                                    _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                                    SdtdConsole.Instance.Output(string.Format("Spawned starting item {0} for {1}.", itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name, _cInfo.playerName));
                                    Log.Out(string.Format("[SERVERTOOLS] Spawned starting item {0} for {1}", itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name, _cInfo.playerName));
                                }
                                string _phrase806;
                                if (!Phrases.Dict.TryGetValue(806, out _phrase806))
                                {
                                    _phrase806 = "You have received your starting items.";
                                }
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase806), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with Id {0} does not exist", _params[0]));
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in GiveStartingItems.Run: {0}.", e));
                }
            }
        }
    }
}

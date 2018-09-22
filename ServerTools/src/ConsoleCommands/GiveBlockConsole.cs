using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class GiveBlockConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Gives a block directly to a players inventory. Drops to the ground if full.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. giveblock <steamId/entityId> <blockId or name> <count>\n" +
                "  2. giveblock all <blockId or name> <count>\n" +
                "1. Gives a player the block(s) to their inventory unless full. Drops to the ground when full.\n"+
                "2. Gives all players the block(s) to their inventory unless full. Drops to the ground when full.\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-GiveBlock", "giveblock", "gb" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 3)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0].Length < 1 || _params[0].Length > 17)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give block to all players or player with Id: Invalid \"all\" or Id {0}", _params[0]));
                    return;
                }
                if (_params[1].Length < 1 || _params[0].Length > 4)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give block: Invalid blockId {0}", _params[1]));
                    return;
                }
                if (_params[2].Length < 1 || _params[2].Length > 4)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give block: Invalid count {0}", _params[2]));
                    return;
                }
                else
                {
                    if (_params[0] == "all" || _params[0] == "ALL" || _params[0] == "All")
                    {
                        List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                        foreach (var _cInfo in _cInfoList)
                        {
                            int count = 1;
                            int _count;
                            if (int.TryParse(_params[2], out _count))
                            {
                                if (_count > 0 & _count < 10000)
                                {
                                    count = _count;
                                }
                            }

                            ItemValue itemValue;
                            var itemId = 1;
                            int _itemId;
                            if (int.TryParse(_params[1], out _itemId))
                            {
                                itemValue = ItemClass.list[itemId] == null ? ItemValue.None : new ItemValue(_itemId, 1, 1, true);
                            }
                            else
                            {
                                if (!ItemClass.ItemNames.Contains(_params[1]))
                                {
                                    SdtdConsole.Instance.Output(string.Format("Unable to find item {0}", _params[1]));
                                    return;
                                }

                                itemValue = new ItemValue(ItemClass.GetItem(_params[1]).type, 1, 1, true);
                            }

                            World world = GameManager.Instance.World;
                            if (world.Players.dict[_cInfo.entityId].IsSpawned())
                            {
                                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                {
                                    entityClass = EntityClass.FromString("item"),
                                    id = EntityFactory.nextEntityID++,
                                    itemStack = new ItemStack(itemValue, count),
                                    pos = world.Players.dict[_cInfo.entityId].position,
                                    rot = new Vector3(20f, 0f, 20f),
                                    lifetime = 60f,
                                    belongsPlayerId = _cInfo.entityId
                                });
                                world.SpawnEntityInWorld(entityItem);
                                _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                                SdtdConsole.Instance.Output(string.Format("Gave {0} to {1}.", itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name, _cInfo.playerName));
                                string _phrase804;
                                if (!Phrases.Dict.TryGetValue(804, out _phrase804))
                                {
                                    _phrase804 = "{Count} {ItemName} was sent to your inventory. If your bag is full, check the ground.";
                                }
                                _phrase804 = _phrase804.Replace("{Count}", count.ToString());
                                _phrase804 = _phrase804.Replace("{ItemName}", itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase804), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with steamdId {0} is not spawned", _params[0]));
                            }
                        }
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (_cInfo != null)
                        {
                            int count = 1;
                            int _count;
                            if (int.TryParse(_params[2], out _count))
                            {
                                if (_count > 0 & _count < 10000)
                                {
                                    count = _count;
                                }
                            }

                            ItemValue itemValue;
                            var itemId = 1;
                            int _itemId;
                            if (int.TryParse(_params[1], out _itemId))
                            {
                                itemValue = ItemClass.list[itemId] == null ? ItemValue.None : new ItemValue(_itemId, 1, 1, true);
                            }
                            else
                            {
                                if (!ItemClass.ItemNames.Contains(_params[1]))
                                {
                                    SdtdConsole.Instance.Output(string.Format("Unable to find item {0}", _params[1]));
                                    return;
                                }

                                itemValue = new ItemValue(ItemClass.GetItem(_params[1]).type, 1, 1, true);
                            }

                            if (Equals(itemValue, ItemValue.None))
                            {
                                SdtdConsole.Instance.Output(string.Format("Unable to find item {0}", _params[1]));
                                return;
                            }

                            World world = GameManager.Instance.World;
                            if (world.Players.dict[_cInfo.entityId].IsSpawned())
                            {
                                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                {
                                    entityClass = EntityClass.FromString("item"),
                                    id = EntityFactory.nextEntityID++,
                                    itemStack = new ItemStack(itemValue, count),
                                    pos = world.Players.dict[_cInfo.entityId].position,
                                    rot = new Vector3(20f, 0f, 20f),
                                    lifetime = 60f,
                                    belongsPlayerId = _cInfo.entityId
                                });
                                world.SpawnEntityInWorld(entityItem);
                                _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                                string _phrase804;
                                if (!Phrases.Dict.TryGetValue(804, out _phrase804))
                                {
                                    _phrase804 = "{Count} {ItemName} was sent to your inventory. If your bag is full, check the ground.";
                                }
                                _phrase804 = _phrase804.Replace("{Count}", count.ToString());
                                _phrase804 = _phrase804.Replace("{ItemName}", itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase804), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with Id {0} is not spawned", _params[0]));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with steamdId {0} does not exist", _params[0]));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GiveItemDirectConsole.Run: {0}.", e));
            }
        }
    }
}

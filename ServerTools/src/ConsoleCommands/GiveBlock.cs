using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class GiveBlock : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Gives a block directly to a players inventory. Drops to the ground if full.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. giveblock <steamId> <blockId> <count>\n" +
                "  2. giveblock all <blockId> <count>\n" +
                "1. Gives a player the block(s) in their inventory unless full. Drops to the ground when full.\n"+
                "2. Gives all players the block(s) in their inventory unless full. Drops to the ground when full.\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "giveblock", "gb" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 4)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 4, found {0}", _params.Count));
                    return;
                }
                if (_params[0].Length != 3 || _params[0].Length != 17)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give block to SteamId: Invalid SteamId {0}", _params[0]));
                    return;
                }
                if (_params[1].Length < 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give block: Invalid itemId {0}", _params[1]));
                    return;
                }
                if (_params[2].Length < 1 || _params[2].Length > 4)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give block: Invalid count {0}", _params[2]));
                    return;
                }
                else
                {
                    string all = _params[0];
                    if (all == "all")
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
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2} was sent to your inventory by an admin. If your bag is full, check the ground.[-]", Config.ChatColor, count, itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name), "Server", false, "", false));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with steamdId {0} is not spawned", _params[1]));
                            }
                        }
                    }
                    else if (_params[0].Length == 17)
                    {
                        ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_params[0]);
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
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2} was sent to your inventory by an admin. If your bag is full, check the ground.[-]", Config.ChatColor, count, itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name), "Server", false, "", false));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with steamdId {0} is not spawned", _params[1]));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with steamdId {0} does not exist", _params[1]));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GiveItemDirect.Run: {0}.", e));
            }
        }
    }
}

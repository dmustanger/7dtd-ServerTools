using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class GiveItem : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]-Gives a item directly to a players inventory. Drops to the ground if full.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. giveitem <steamId> <itemId or name> <count> <quality>\n" +
                "  2. giveitem all <itemId or name> <count> <quality>\n " +
                "1. Gives a player the item(s) to their inventory unless full. Drops to the ground when full.\n" +
                "2. Gives all players the item(s) to their inventory unless full. Drops to the ground when full.\n" +
                "*Note Item(s) with no quality require a 1*\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-GiveItem", "giveitem", "gi" };
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
                if (_params[0].Length != 3)
                {
                    if (_params[0].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not give item to SteamId: Invalid SteamId {0}", _params[0]));
                        return;
                    }
                }
                if (_params[1].Length < 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give item: Invalid itemId {0}", _params[1]));
                    return;
                }
                if (_params[2].Length < 1 || _params[2].Length > 4)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give item: Invalid count {0}", _params[2]));
                    return;
                }
                if (_params[3].Length < 1 || _params[3].Length > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give item: Invalid quality {0}", _params[3]));
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
                                if (_count > 0 & _count < 10001)
                                {
                                    count = _count;
                                }
                            }

                            int min = 1;
                            int max = 1;
                            int quality;

                            if (int.TryParse(_params[3], out quality))
                            {
                                if (quality > 0 & quality < 601)
                                {
                                    min = quality;
                                    max = quality;
                                }
                            }

                            ItemValue itemValue;
                            var itemId = 4096;
                            int _itemId;
                            if (int.TryParse(_params[1], out _itemId))
                            {
                                int calc = (_itemId + 4096);
                                itemId = calc;
                                itemValue = ItemClass.list[itemId] == null ? ItemValue.None : new ItemValue(itemId, true);
                            }
                            else
                            {
                                ItemValue _itemValue = ItemClass.GetItem(_params[1], true);
                                if (_itemValue.type == ItemValue.None.type)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Unable to find item {0}", _params[1]));
                                    return;
                                }
                                else
                                {
                                    itemValue = new ItemValue(ItemClass.GetItem(_params[1]).type, true);
                                }
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
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} {2} was sent to your inventory by an admin. If your bag is full, check the ground.[-]", Config.ChatResponseColor, count, itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name), "Server", false, "", false);
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with steamdId {0} is not spawned", _params[1]));
                            }

                        }
                    }
                    else
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

                            int min = 1;
                            int max = 1;
                            int quality;

                            if (int.TryParse(_params[3], out quality))
                            {
                                if (quality > 0 & quality < 601)
                                {
                                    min = quality;
                                    max = quality;
                                }
                            }

                            ItemValue itemValue;
                            var itemId = 4096;
                            int _itemId;
                            if (int.TryParse(_params[1], out _itemId))
                            {
                                int calc = (_itemId + 4096);
                                itemId = calc;
                                itemValue = ItemClass.list[itemId] == null ? ItemValue.None : new ItemValue(itemId, min, max, true);
                            }
                            else
                            {
                                ItemValue _itemValue = ItemClass.GetItem(_params[1], true);
                                if (_itemValue.type == ItemValue.None.type)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Unable to find item {0}", _params[1]));
                                    return;
                                }
                                else
                                {
                                    itemValue = new ItemValue(ItemClass.GetItem(_params[1]).type, min, max, true);
                                }
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
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2} was sent to your inventory by an admin. If your bag is full, check the ground.[-]", Config.ChatResponseColor, count, itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name), "Server", false, "", false));
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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    public class GiveItemConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]-Gives a item directly to a players inventory. Drops to the ground if full.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. giveitem <steamId/entityId> <itemId or name> <count> <quality>\n" +
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
                if (_params.Count != 4)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 4, found {0}", _params.Count));
                    return;
                }
                if (_params[0].Length < 1 || _params[0].Length > 17)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give item to SteamId: Invalid SteamId {0}", _params[0]));
                    return;
                }
                if (_params[1].Length < 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give item: Invalid itemId or name {0}", _params[1]));
                    return;
                }
                if (_params[2].Length < 1 || _params[2].Length > 5)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give item: Invalid count {0}", _params[2]));
                    return;
                }
                if (_params[3].Length < 1 || _params[3].Length > 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not give item: Invalid quality {0}", _params[3]));
                    return;
                }
                else
                {
                    World world = GameManager.Instance.World;
                    int count = 1;
                    int _count;
                    if (int.TryParse(_params[2], out _count))
                    {
                        if (_count > 0 && _count < 100000)
                        {
                            count = _count;
                        }
                    }
                    int min = 1;
                    int max = 1;
                    int quality;
                    if (int.TryParse(_params[3], out quality))
                    {
                        if (quality > 0 && quality < 7)
                        {
                            min = quality;
                            max = quality;
                        }
                    }
                    ItemValue _itemValue;
                    ItemClass _class;
                    Block _block;
                    int _id;
                    if (int.TryParse(_params[1], out _id))
                    {
                        _class = ItemClass.GetForId(_id);
                        _block = Block.GetBlockByName(_params[1], true);
                    }
                    else
                    {
                        _class = ItemClass.GetItemClass(_params[1], true);
                        _block = Block.GetBlockByName(_params[1], true);
                    }
                    if (_class == null && _block == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("Unable to find item {0}", _params[1]));
                        return;
                    }
                    else
                    {
                        _itemValue = new ItemValue(ItemClass.GetItem(_params[1]).type, min, max, true);
                    }
                    if (_params[0].ToLower() == "all")
                    {
                        List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                        foreach (var _cInfo in _cInfoList)
                        {
                            if (_cInfo != null)
                            {
                                if (world.Players.dict[_cInfo.entityId].IsSpawned())
                                {
                                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                    {
                                        entityClass = EntityClass.FromString("item"),
                                        id = EntityFactory.nextEntityID++,
                                        itemStack = new ItemStack(_itemValue, count),
                                        pos = world.Players.dict[_cInfo.entityId].position,
                                        rot = new Vector3(20f, 0f, 20f),
                                        lifetime = 60f,
                                        belongsPlayerId = _cInfo.entityId
                                    });
                                    world.SpawnEntityInWorld(entityItem);
                                    _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                                    SdtdConsole.Instance.Output(string.Format("Gave {0} to {1}.", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name, _cInfo.playerName));
                                    string _phrase804;
                                    if (!Phrases.Dict.TryGetValue(804, out _phrase804))
                                    {
                                        _phrase804 = "{Count} {ItemName} was sent to your inventory. If your bag is full, check the ground.";
                                    }
                                    _phrase804 = _phrase804.Replace("{Count}", count.ToString());
                                    _phrase804 = _phrase804.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase804 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("Player with steamdId {0} is not spawned", _params[0]));
                                }
                            }
                        }
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (_cInfo != null)
                        {
                            if (world.Players.dict[_cInfo.entityId].IsSpawned())
                            {
                                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                {
                                    entityClass = EntityClass.FromString("item"),
                                    id = EntityFactory.nextEntityID++,
                                    itemStack = new ItemStack(_itemValue, count),
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
                                _phrase804 = _phrase804.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase804 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with Id {0} is not spawned", _params[0]));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with Id {0} does not exist", _params[0]));
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

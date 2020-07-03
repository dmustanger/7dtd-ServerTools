using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class GiveItemConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]- Gives a item directly to a player's inventory. Drops to the ground if inventory and bag is full.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. GiveItem <SteamId/EntityId> <Name> <Count> <Quality>\n" +
                "  2. GiveItem all <Name> <Count> <Quality>\n " +
                "1. Gives a player the item(s) to their inventory unless full. Drops to the ground when full.\n" +
                "2. Gives all players the item(s) to their inventory unless full. Drops to the ground when full.\n" +
                "*Note Item(s) with no quality require using 1*\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-GiveItem", "GiveItem", "giveitem", "gi" };
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
                        _itemValue = new ItemValue(ItemClass.GetItem(_params[1]).type, min, max, false, null, 1);
                    }
                    if (_params[0].ToLower() == "all")
                    {
                        List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                        if (_cInfoList != null && _cInfoList.Count > 0)
                        {
                            for (int i = 0; i < _cInfoList.Count; i++)
                            {
                                ClientInfo _cInfo = _cInfoList[i];
                                if (_cInfo != null && world.Players.dict.ContainsKey(_cInfo.entityId))
                                {
                                    EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                                    if (_player != null && _player.IsSpawned() && _player.IsDead())
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
                                        SdtdConsole.Instance.Output(string.Format("Gave {0} to {1}.", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name, _cInfo.playerName));
                                        if (_player.bag.CanTakeItem(new ItemStack(_itemValue, count)) || _player.inventory.CanTakeItem(new ItemStack(_itemValue, count)))
                                        {
                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                            world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                            string _phrase804;
                                            if (!Phrases.Dict.TryGetValue(804, out _phrase804))
                                            {
                                                _phrase804 = "{Count} {ItemName} was sent to your inventory. If your bag is full, check the ground.";
                                            }
                                            _phrase804 = _phrase804.Replace("{Count}", count.ToString());
                                            _phrase804 = _phrase804.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase804 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                        else
                                        {
                                            string _phrase828;
                                            if (!Phrases.Dict.TryGetValue(828, out _phrase828))
                                            {
                                                _phrase828 = "{Count} {ItemName} was sent to you but your bag is full. Check the ground.";
                                            }
                                            _phrase828 = _phrase828.Replace("{Count}", count.ToString());
                                            _phrase828 = _phrase828.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase828 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("Player with steamd Id {0} has not spawned. Unable to give item", _cInfo.playerId));
                                }
                            }
                        }
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (_cInfo != null && world.Players.dict.ContainsKey(_cInfo.entityId))
                        {
                            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                            if (_player != null && _player.IsSpawned() && _player.IsDead())
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
                                SdtdConsole.Instance.Output(string.Format("Gave {0} to {1}.", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name, _cInfo.playerName));
                                if (_player.bag.CanTakeItem(new ItemStack(_itemValue, count)) || _player.inventory.CanTakeItem(new ItemStack(_itemValue, count)))
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                    string _phrase804;
                                    if (!Phrases.Dict.TryGetValue(804, out _phrase804))
                                    {
                                        _phrase804 = "{Count} {ItemName} was sent to your inventory.";
                                    }
                                    _phrase804 = _phrase804.Replace("{Count}", count.ToString());
                                    _phrase804 = _phrase804.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase804 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    string _phrase828;
                                    if (!Phrases.Dict.TryGetValue(828, out _phrase828))
                                    {
                                        _phrase828 = "{Count} {ItemName} was sent to you but your bag is full. Check the ground.";
                                    }
                                    _phrase828 = _phrase828.Replace("{Count}", count.ToString());
                                    _phrase828 = _phrase828.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase828 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with steamd id {0} is not logged on or loaded in yet", _params[0]));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GiveItemDirectConsole.Execute: {0}", e));
            }
        }
    }
}

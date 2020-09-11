using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class GiveItemConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Gives a item directly to a player's inventory. Drops to the ground if inventory is full.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. GiveItem <SteamId/EntityId> <Name> <Count> <Quality> <Durability>\n" +
                "  2. GiveItem <SteamId/EntityId> <Name> <Count> <Quality>\n" +
                "  3. GiveItem <SteamId/EntityId> <Name> <Count>\n" +
                "  4. GiveItem <SteamId/EntityId> <Name>\n" +
                "  5. GiveItem all <Name> <Count> <Quality> <Durability>\n " +
                "  6. GiveItem all <Name> <Count> <Quality>\n " +
                "  7. GiveItem all <Name> <Count>\n " +
                "  8. GiveItem all <Name>\n " +
                "1. Gives a player the item with specific count, quality and durability\n" +
                "2. Gives a player the item with specific count, quality and 100 percent durability\n" +
                "3. Gives a player the item with specific count, 1 quality and 100 percent durability\n" +
                "4. Gives a player the item with 1 count 1 quality and 100 percent durability\n" +
                "5. Gives all players the item with specific count, quality and durability\n" +
                "6. Gives all players the item with specific count, quality and 100 percent durability\n" +
                "7. Gives all players the item with specific count, 1 quality and 100 percent durability\n" +
                "8. Gives all players the item with 1 count 1 quality and 100 percent durability\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-GiveItem", "gi", "st-gi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 2 && _params.Count > 5)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 to 5, found {0}", _params.Count));
                    return;
                }
                if (_params[0].Length < 3 || _params[0].Length > 17)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not give item: Invalid id {0}", _params[0]));
                    return;
                }
                if (_params[1].Length < 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not give item: Invalid name {0}", _params[1]));
                    return;
                }
                else
                {
                    World world = GameManager.Instance.World;
                    int _itemCount = 1;
                    if (_params.Count > 2 && int.TryParse(_params[2], out int _count))
                    {
                        if (_count > 0 && _count < 1000000)
                        {
                            _itemCount = _count;
                        }
                    }
                    int _min = 1;
                    int _max = 1;
                    int _itemQuality;
                    if (_params.Count > 3 && int.TryParse(_params[3], out _itemQuality))
                    {
                        if (_itemQuality > 0 && _itemQuality < 7)
                        {
                            _min = _itemQuality;
                            _max = _itemQuality;
                        }
                    }
                    ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_params[1]).type, _min, _max, true, null, 1);
                    if (_itemValue != null)
                    {
                        if (_params.Count > 4 && float.TryParse(_params[4], out float _durability))
                        {
                            if (_durability > 0 && _durability < 101)
                            {
                                float _newDurability = _itemValue.MaxUseTimes - (_durability / 100 * _itemValue.MaxUseTimes);
                                _itemValue.UseTimes = _newDurability;
                            }
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
                                        if (_player != null && _player.IsSpawned() && !_player.IsDead())
                                        {
                                            var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                            {
                                                entityClass = EntityClass.FromString("item"),
                                                id = EntityFactory.nextEntityID++,
                                                itemStack = new ItemStack(_itemValue, _itemCount),
                                                pos = world.Players.dict[_cInfo.entityId].position,
                                                rot = new Vector3(20f, 0f, 20f),
                                                lifetime = 60f,
                                                belongsPlayerId = _cInfo.entityId
                                            });
                                            world.SpawnEntityInWorld(entityItem);
                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Gave {0} to {1}.", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name, _cInfo.playerName));
                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                            world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                            Phrases.Dict.TryGetValue(901, out string _phrase901);
                                            _phrase901 = _phrase901.Replace("{Value}", _itemCount.ToString());
                                            _phrase901 = _phrase901.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase901 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player with steamd Id {0} has not spawned. Unable to give item", _cInfo.playerId));
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
                                if (_player != null && _player.IsSpawned())
                                {
                                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                    {
                                        entityClass = EntityClass.FromString("item"),
                                        id = EntityFactory.nextEntityID++,
                                        itemStack = new ItemStack(_itemValue, _itemCount),
                                        pos = world.Players.dict[_cInfo.entityId].position,
                                        rot = new Vector3(20f, 0f, 20f),
                                        lifetime = 60f,
                                        belongsPlayerId = _cInfo.entityId
                                    });
                                    world.SpawnEntityInWorld(entityItem);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Gave {0} to {1}.", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name, _cInfo.playerName));
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                    Phrases.Dict.TryGetValue(901, out string _phrase901);
                                    _phrase901 = _phrase901.Replace("{Value}", _itemCount.ToString());
                                    _phrase901 = _phrase901.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase901 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player with steamd id {0} is not logged on or loaded in yet", _params[0]));
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to find item {0}", _params[1]));
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GiveItemConsole.Execute: {0}", e.Message));
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class GiveItemConsole : ConsoleCmdAbstract
    {

        protected override string getDescription()
        {
            return "[ServerTools] - Gives a item directly to a player's inventory. Drops to the ground if inventory is full.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. st-gi <EOS/EntityId/PlayerName> <Item> <Count> <Quality> <Durability>\n" +
                "  2. st-gi <EOS/EntityId/PlayerName> <Item> <Count> <Quality>\n" +
                "  3. st-gi <EOS/EntityId/PlayerName> <Item> <Count>\n" +
                "  4. st-gi <EOS/EntityId/PlayerName> <Item>\n" +
                "  5. st-gi all <Item> <Count> <Quality> <Durability>\n " +
                "  6. st-gi all <Item> <Count> <Quality>\n " +
                "  7. st-gi all <Item> <Count>\n " +
                "  8. st-gi all <Item>\n " +
                "1. Gives a player the item with specific count, quality and durability\n" +
                "2. Gives a player the item with specific count, quality and 100 percent durability\n" +
                "3. Gives a player the item with specific count, 1 quality and 100 percent durability\n" +
                "4. Gives a player the item with 1 count 1 quality and 100 percent durability\n" +
                "5. Gives all players the item with specific count, quality and durability\n" +
                "6. Gives all players the item with specific count, quality and 100 percent durability\n" +
                "7. Gives all players the item with specific count, 1 quality and 100 percent durability\n" +
                "8. Gives all players the item with 1 count 1 quality and 100 percent durability\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-GiveItem", "gi", "st-gi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 2 && _params.Count > 5)
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Wrong number of arguments, expected 2 to 5, found '{0}'", _params.Count);
                    return;
                }
                if (_params[1].Length < 1)
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid item name '{0}'", _params[1]);
                    return;
                }
                else
                {
                    World world = GameManager.Instance.World;
                    int itemCount = 1;
                    if (_params.Count > 2 && int.TryParse(_params[2], out int count))
                    {
                        if (count > 0 && count < 1000000)
                        {
                            itemCount = count;
                        }
                    }
                    ItemValue itemValue = new ItemValue(ItemClass.GetItem(_params[1], false).type, 1, 1, false, null);
                    if (itemValue != null)
                    {
                        if (itemValue.HasQuality)
                        {
                            if (_params.Count > 3 && int.TryParse(_params[3], out int itemQuality) && itemQuality > 0)
                            {
                                itemValue.Quality = itemQuality;
                            }
                        }
                        if (_params.Count > 4 && float.TryParse(_params[4], out float durability))
                        {
                            if (durability > 0 && durability < 101)
                            {
                                float newDurability = itemValue.MaxUseTimes - (durability / 100 * itemValue.MaxUseTimes);
                                itemValue.UseTimes = newDurability;
                            }
                        }
                        if (_params[0].ToLower() == "all")
                        {
                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                            {
                                ThreadManager.StartCoroutine(SpawnItems(SdtdConsole.Instance, itemValue, itemCount, world));
                            }, null);
                        }
                        else
                        {
                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                            {
                                ThreadManager.StartCoroutine(SpawnItem(SdtdConsole.Instance, itemValue, itemCount, world, _params[0]));
                            }, null);
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Unable to find item '{0}'", _params[1]);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in GiveItemConsole.Execute: {0}", e.Message);
            }
        }

        public static IEnumerator SpawnItems(SdtdConsole _stdt, ItemValue _itemValue, int _itemCount, World _world)
        {
            try
            {
                if (_itemValue == null || _world == null)
                {
                    yield break;
                }
                ClientInfo cInfo;
                EntityPlayer player;
                EntityItem entityItem;
                List<ClientInfo> cInfoList = GeneralOperations.ClientList();
                if (cInfoList == null || cInfoList.Count == 0)
                {
                    yield break;
                }
                for (int i = 0; i < cInfoList.Count; i++)
                {
                    cInfo = cInfoList[i];
                    if (cInfo == null)
                    {
                        continue;
                    }
                    player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                    if (player != null && player.IsSpawned() && !player.IsDead())
                    {
                        entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                        {
                            entityClass = EntityClass.FromString("item"),
                            id = EntityFactory.nextEntityID++,
                            itemStack = new ItemStack(_itemValue, _itemCount),
                            pos = _world.Players.dict[cInfo.entityId].position,
                            rot = new Vector3(20f, 0f, 20f),
                            lifetime = 60f,
                            belongsPlayerId = cInfo.entityId
                        });
                        _world.SpawnEntityInWorld(entityItem);
                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, cInfo.entityId));
                        _world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                        Phrases.Dict.TryGetValue("GiveItem1", out string phrase);
                        phrase = phrase.Replace("{Value}", _itemCount.ToString());
                        phrase = phrase.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        _stdt.Output("[SERVERTOOLS] Player '{0}' '{1}' is not spawned or is dead. Unable to give item", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in GiveItemConsole.SpawnItems: {0}", e.StackTrace);
            }
            yield break;
        }

        public static IEnumerator SpawnItem(SdtdConsole _stdt, ItemValue _itemValue, int _itemCount, World _world, string _target)
        {
            try
            {
                if (_itemValue == null || _world == null)
                {
                    yield break;
                }
                EntityPlayer player;
                EntityItem entityItem;
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_target);
                if (cInfo != null)
                {
                    player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                    if (player != null && player.IsSpawned() && !player.IsDead())
                    {
                        entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                        {
                            entityClass = EntityClass.FromString("item"),
                            id = EntityFactory.nextEntityID++,
                            itemStack = new ItemStack(_itemValue, _itemCount),
                            pos = _world.Players.dict[cInfo.entityId].position,
                            rot = new Vector3(20f, 0f, 20f),
                            lifetime = 60f,
                            belongsPlayerId = cInfo.entityId
                        });
                        _world.SpawnEntityInWorld(entityItem);
                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, cInfo.entityId));
                        _world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                        Phrases.Dict.TryGetValue("GiveItem1", out string phrase);
                        phrase = phrase.Replace("{Value}", _itemCount.ToString());
                        phrase = phrase.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        _stdt.Output("[SERVERTOOLS] Player '{0}' is not spawned or is dead. Unable to give item", _target);
                    }
                }
                else
                {
                    _stdt.Output("[SERVERTOOLS] Player '{0}' not found", _target);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in GiveItemConsole.SpawnItem: {0}", e.StackTrace);
            }
            yield break;
        }
    }
}

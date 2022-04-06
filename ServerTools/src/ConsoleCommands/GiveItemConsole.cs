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
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 to 5, found '{0}'", _params.Count));
                    return;
                }
                if (_params[1].Length < 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid item name '{0}'", _params[1]));
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
                    ItemValue itemValue = new ItemValue(ItemClass.GetItem(_params[1]).type);
                    if (itemValue != null)
                    {
                        if (itemValue.HasQuality)
                        {
                            itemValue.Quality = 1;
                            if (_params.Count > 3 && int.TryParse(_params[3], out int itemQuality))
                            {
                                if (itemQuality > 0)
                                {
                                    itemValue.Quality = itemQuality;
                                }
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
                            List<ClientInfo> cInfoList = PersistentOperations.ClientList();
                            if (cInfoList != null)
                            {
                                for (int i = 0; i < cInfoList.Count; i++)
                                {
                                    ClientInfo cInfo = cInfoList[i];
                                    if (cInfo != null)
                                    {
                                        EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                                        if (player != null && player.IsSpawned() && !player.IsDead())
                                        {
                                            EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                            {
                                                entityClass = EntityClass.FromString("item"),
                                                id = EntityFactory.nextEntityID++,
                                                itemStack = new ItemStack(itemValue, itemCount),
                                                pos = world.Players.dict[cInfo.entityId].position,
                                                rot = new Vector3(20f, 0f, 20f),
                                                lifetime = 60f,
                                                belongsPlayerId = cInfo.entityId
                                            });
                                            world.SpawnEntityInWorld(entityItem);
                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, cInfo.entityId));
                                            world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                            Phrases.Dict.TryGetValue("GiveItem1", out string phrase);
                                            phrase = phrase.Replace("{Value}", itemCount.ToString());
                                            phrase = phrase.Replace("{ItemName}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name);
                                            ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                        else
                                        {
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player Id '{0' has not spawned or is dead. Unable to give item at this time", cInfo.PlatformId.ReadablePlatformUserIdentifier));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                            if (cInfo != null)
                            {
                                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                                if (player != null && player.IsSpawned() && !player.IsDead())
                                {
                                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                    {
                                        entityClass = EntityClass.FromString("item"),
                                        id = EntityFactory.nextEntityID++,
                                        itemStack = new ItemStack(itemValue, itemCount),
                                        pos = world.Players.dict[cInfo.entityId].position,
                                        rot = new Vector3(20f, 0f, 20f),
                                        lifetime = 60f,
                                        belongsPlayerId = cInfo.entityId
                                    });
                                    world.SpawnEntityInWorld(entityItem);
                                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, cInfo.entityId));
                                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                    Phrases.Dict.TryGetValue("GiveItem1", out string _phrase);
                                    _phrase = _phrase.Replace("{Value}", itemCount.ToString());
                                    _phrase = _phrase.Replace("{ItemName}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player Id '{0}' has not spawned or is dead. Unable to give item at this time", _params[0]));
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player Id '{0}' not found", _params[0]));
                            }
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find item '{0}'", _params[1]));
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

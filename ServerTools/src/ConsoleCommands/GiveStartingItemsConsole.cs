using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using System.Collections;

namespace ServerTools
{
    public class GiveStartingItemsConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Gives the starting items from the xml list";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. st-st-gsi <EOS/EntityId/PlayerName>\n" +
                "1. Gives a player the item(s) from the StatingItems.xml to their inventory unless full. Drops to the ground when full\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-GiveStartingItems", "gsi", "st-gsi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (StartingItems.IsEnabled)
            {
                try
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    else
                    {
                        ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                        if (cInfo != null)
                        {
                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                            {
                                ThreadManager.StartCoroutine(SpawnItems(cInfo));
                            }, null);
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player with id '{0}' is not online", _params[0]));
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in GiveStartingItemsConsole.Execute: {0}", e.Message));
                }
            }
        }

        public static IEnumerator SpawnItems(ClientInfo _cInfo)
        {
            try
            {
                if (StartingItems.Dict.Count > 0)
                {
                    World world = GameManager.Instance.World;
                    if (world.Players.dict.ContainsKey(_cInfo.entityId))
                    {
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                        if (player != null && player.IsSpawned() && !player.IsDead())
                        {
                            ItemValue itemValue;
                            EntityItem entityItem;
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].StartingItems = true;
                            PersistentContainer.DataChange = true;
                            List<string> itemList = StartingItems.Dict.Keys.ToList();
                            for (int i = 0; i < itemList.Count; i++)
                            {
                                string item = itemList[i];
                                StartingItems.Dict.TryGetValue(item, out int[] itemData);
                                itemValue = new ItemValue(ItemClass.GetItem(item, false).type, false);
                                if (itemValue.HasQuality && itemData[1] > 0)
                                {
                                    itemValue.Quality = itemData[1];
                                }
                                entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                {
                                    entityClass = EntityClass.FromString("item"),
                                    id = EntityFactory.nextEntityID++,
                                    itemStack = new ItemStack(itemValue, itemData[0]),
                                    pos = world.Players.dict[_cInfo.entityId].position,
                                    rot = new Vector3(20f, 0f, 20f),
                                    lifetime = 60f,
                                    belongsPlayerId = _cInfo.entityId
                                });
                                world.SpawnEntityInWorld(entityItem);
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                Thread.Sleep(TimeSpan.FromSeconds(1));
                            }
                            Log.Out(string.Format("[SERVERTOOLS] '{0}' with id '{1}' received their starting items", _cInfo.playerName, _cInfo.CrossplatformId.CombinedString));
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] '{0}' with id '{1}' received their starting items", _cInfo.playerName, _cInfo.CrossplatformId.CombinedString));
                            Phrases.Dict.TryGetValue("StartingItems1", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player with id '{0}' has not spawned. Unable to give starting items", _cInfo.CrossplatformId.CombinedString));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GiveStartingItemsConsole.SpawnItems: {0}", e.Message));
            }
            yield break;
        }
    }
}

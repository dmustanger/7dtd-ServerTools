using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;

namespace ServerTools
{
    public class GiveStartingItemsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Gives the starting items from the xml list";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-st-gsi <EOS/EntityId/PlayerName>\n" +
                "1. Gives a player the item(s) from the StatingItems.xml to their inventory unless full. Drops to the ground when full\n";
        }

        public override string[] GetCommands()
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
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    else
                    {
                        ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(_params[0]);
                        if (cInfo != null)
                        {
                            SpawnItems(cInfo);
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player with id '{0}' is not online", _params[0]));
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in GiveStartingItemsConsole.Execute: {0}", e.Message));
                }
            }
        }

        public static void SpawnItems(ClientInfo _cInfo)
        {
            try
            {
                if (StartingItems.Dict.Count > 0)
                {
                    World world = GameManager.Instance.World;
                    if (world.Players.dict.ContainsKey(_cInfo.entityId))
                    {
                        EntityPlayer _player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
                        if (_player != null && _player.IsSpawned() && !_player.IsDead())
                        {
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].StartingItems = true;
                            PersistentContainer.DataChange = true;
                            List<string> _itemList = StartingItems.Dict.Keys.ToList();
                            for (int i = 0; i < _itemList.Count; i++)
                            {
                                string _item = _itemList[i];
                                StartingItems.Dict.TryGetValue(_item, out int[]  _itemData);
                                ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_item, false).type, false);
                                if (_itemValue.HasQuality && _itemData[1] > 0)
                                {
                                    _itemValue.Quality = _itemData[1];
                                }
                                EntityItem entityItem = new EntityItem();
                                entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                {
                                    entityClass = EntityClass.FromString("item"),
                                    id = EntityFactory.nextEntityID++,
                                    itemStack = new ItemStack(_itemValue, _itemData[0]),
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
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] '{0}' with id '{1}' received their starting items", _cInfo.playerName, _cInfo.CrossplatformId.CombinedString));
                            Phrases.Dict.TryGetValue("StartingItems1", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player with id '{0}' has not spawned. Unable to give starting items", _cInfo.CrossplatformId.CombinedString));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GiveStartingItemsConsole.SpawnItems: {0}", e.Message));
            }
        }
    }
}

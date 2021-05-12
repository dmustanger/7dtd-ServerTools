using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class AuctionConsole : ConsoleCmdAbstract
    {
        private static string file = string.Format("Auction_{0}.txt", DateTime.Today.ToString("M-d-yyyy")), filepath = string.Format("{0}/Logs/AuctionLogs/{1}", API.ConfigPath, file);

        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable auction. Cancel, clear or list the auction items.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-auc off\n" +
                   "  2. st-auc on\n" +
                   "  3. st-auc cancel <Id>\n" +
                   "  4. st-auc clear <Id>\n" +
                   "  5. st-auc list\n" +
                   "1. Turn off the auction\n" +
                   "2. Turn on the auction\n" +
                   "3. Cancel the auction Id and return it to the owner\n" +
                   "4. Clear the auction Id from the list. It will not return to the owner\n" +
                   "5. Show the auction list\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Auction", "auc", "st-auc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 && _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Auction.IsEnabled)
                    {
                        Auction.IsEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Auction has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Auction is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Auction.IsEnabled)
                    {
                        Auction.IsEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Auction has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Auction is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("cancel"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    int _id;
                    if (int.TryParse(_params[1], out _id))
                    {
                        if (Auction.AuctionItems.ContainsKey(_id))
                        {
                            Auction.AuctionItems.TryGetValue(_id, out string _playerId);
                            if (PersistentContainer.Instance.Players[_playerId].Auction != null && PersistentContainer.Instance.Players[_playerId].Auction.Count > 0)
                            {
                                if (PersistentContainer.Instance.Players[_playerId].Auction.ContainsKey(_id))
                                {
                                    if (PersistentContainer.Instance.Players[_playerId].Auction.TryGetValue(_id, out ItemDataSerializable _itemData))
                                    {
                                        ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_playerId);
                                        if (_cInfo != null)
                                        {
                                            ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_itemData.name, false).type, false);
                                            if (_itemValue != null)
                                            {
                                                _itemValue.UseTimes = _itemData.useTimes;
                                                _itemValue.Quality = _itemData.quality;
                                                World world = GameManager.Instance.World;
                                                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                                {
                                                    entityClass = EntityClass.FromString("item"),
                                                    id = EntityFactory.nextEntityID++,
                                                    itemStack = new ItemStack(_itemValue, _itemData.count),
                                                    pos = world.Players.dict[_cInfo.entityId].position,
                                                    rot = new UnityEngine.Vector3(20f, 0f, 20f),
                                                    lifetime = 60f,
                                                    belongsPlayerId = _cInfo.entityId
                                                });
                                                world.SpawnEntityInWorld(entityItem);
                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                                Auction.AuctionItems.Remove(_id);
                                                PersistentContainer.Instance.Players[_playerId].Auction.Remove(_id);
                                                PersistentContainer.Instance.AuctionPrices.Remove(_id);
                                                using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} {2} had their auction entry # {3} cancelled via console by {4}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _id, _senderInfo.RemoteClientInfo.playerId));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "Your auction item has returned to you.[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                            else
                                            {
                                                Auction.AuctionItems.Remove(_id);
                                                PersistentContainer.Instance.Players[_playerId].Auction.Remove(_id);
                                                PersistentContainer.Instance.AuctionPrices.Remove(_id);
                                            }
                                        }
                                        else
                                        {
                                            if (PersistentContainer.Instance.Players[_playerId].AuctionReturn != null && PersistentContainer.Instance.Players[_playerId].AuctionReturn.Count > 0)
                                            {
                                                PersistentContainer.Instance.Players[_playerId].AuctionReturn.Add(_id, _itemData);
                                            }
                                            else
                                            {
                                                Dictionary<int, ItemDataSerializable> _auctionReturn = new Dictionary<int, ItemDataSerializable>();
                                                _auctionReturn.Add(_id, _itemData);
                                                PersistentContainer.Instance.Players[_playerId].AuctionReturn = _auctionReturn;
                                            }
                                            Auction.AuctionItems.Remove(_id);
                                            PersistentContainer.Instance.Players[_playerId].Auction.Remove(_id);
                                            PersistentContainer.Instance.AuctionPrices.Remove(_id);
                                        }
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id {0} has been removed from the auction list", _id));
                                    }
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Could not find this id listed in the auction. Unable to cancel.[-]");
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer {0}", _id));
                    }
                    return;
                }
                else if (_params[0].ToLower().Equals("clear"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                    }
                    if (int.TryParse(_params[1], out int _id))
                    {
                        if (Auction.AuctionItems.ContainsKey(_id))
                        {
                            Auction.AuctionItems.TryGetValue(_id, out string _playerId);
                            Auction.AuctionItems.Remove(_id);
                            if (PersistentContainer.Instance.Players[_playerId].Auction != null && PersistentContainer.Instance.Players[_playerId].Auction.Count > 0)
                            {
                                PersistentContainer.Instance.Players[_playerId].Auction.Remove(_id);
                            }
                            if (PersistentContainer.Instance.AuctionPrices != null && PersistentContainer.Instance.AuctionPrices.Count > 0)
                            {
                                PersistentContainer.Instance.AuctionPrices.Remove(_id);
                            }
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id {0} has been removed from the auction", _id));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Auction does not contain id {0}", _id));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer {0}", _id));
                    }
                    return;
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    if (Auction.AuctionItems.Count > 0)
                    {
                        if (PersistentContainer.Instance.Players.SteamIDs.Count > 0)
                        {
                            List<string> playerlist = PersistentContainer.Instance.Players.SteamIDs;
                            Dictionary<int, int> _auctionPrices = PersistentContainer.Instance.AuctionPrices;
                            for (int i = 0; i < playerlist.Count; i++)
                            {
                                string _steamId = playerlist[i];
                                if (PersistentContainer.Instance.Players[_steamId].Auction != null && PersistentContainer.Instance.Players[_steamId].Auction.Count > 0)
                                {
                                    foreach (var _auctionItem in PersistentContainer.Instance.Players[_steamId].Auction)
                                    {
                                        _auctionPrices.TryGetValue(_auctionItem.Key, out int _price);
                                        string _message = "# {Id}: {Count} {Item} at {Quality} quality, {Durability} durability for {Price} {Name}";
                                        _message = _message.Replace("{Id}", _auctionItem.Key.ToString());
                                        _message = _message.Replace("{Count}", _auctionItem.Value.count.ToString());
                                        _message = _message.Replace("{Item}", _auctionItem.Value.name);
                                        _message = _message.Replace("{Quality}", _auctionItem.Value.quality.ToString());
                                        _message = _message.Replace("{Durability}", (100 - _auctionItem.Value.useTimes).ToString());
                                        _message = _message.Replace("{Price}", _price.ToString());
                                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                                        SdtdConsole.Instance.Output(_message);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] No items are listed in the auction");
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AuctionConsole.Execute: {0}", e.Message));
            }
        }
    }
}
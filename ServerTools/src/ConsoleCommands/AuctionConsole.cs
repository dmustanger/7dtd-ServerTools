using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AuctionConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable auction. Cancel, clear or list the auction items.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. auc off\n" +
                   "  2. auc on\n" +
                   "  3. auc cancel <Id>\n" +
                   "  4. auc clear <Id>\n" +
                   "  5. auc list\n" +
                   "1. Turn off the auction\n" +
                   "2. Turn on the auction\n" +
                   "3. Cancel the auction Id and return it to the owner\n" +
                   "4. Clear the auction Id from the list\n" +
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
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (AuctionBox.IsEnabled)
                    {
                        AuctionBox.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Auction has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Auction is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!AuctionBox.IsEnabled)
                    {
                        AuctionBox.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Auction has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Auction is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("cancel"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    int _id;
                    if (int.TryParse(_params[1], out _id))
                    {
                        if (AuctionBox.AuctionItems.ContainsKey(_id))
                        {
                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_id);
                            if (_cInfo != null)
                            {
                                string _auctionName = PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemName;
                                int _auctionCount = PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemCount;
                                int _auctionQuality = PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemQuality;
                                int _auctionPrice = PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemPrice;
                                ItemClass _class = ItemClass.GetItemClass(_auctionName, false);
                                Block _block = Block.GetBlockByName(_auctionName, false);
                                if (_class == null && _block == null)
                                {
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Could not complete the auction cancel. Unable to find item {0} in the item.xml list. Contact an administrator.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    Log.Out(string.Format("Could not complete the auction cancel. Unable to find item {0} in the item.xml list", _auctionName));
                                    return;
                                }
                                ItemValue itemValue = new ItemValue(ItemClass.GetItem(_auctionName).type, _auctionQuality, _auctionQuality, false, null, 1);
                                World world = GameManager.Instance.World;
                                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                {
                                    entityClass = EntityClass.FromString("item"),
                                    id = EntityFactory.nextEntityID++,
                                    itemStack = new ItemStack(itemValue, _auctionCount),
                                    pos = world.Players.dict[_cInfo.entityId].position,
                                    rot = new UnityEngine.Vector3(20f, 0f, 20f),
                                    lifetime = 60f,
                                    belongsPlayerId = _cInfo.entityId
                                });
                                world.SpawnEntityInWorld(entityItem);
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                                AuctionBox.AuctionItems.Remove(_id);
                                PersistentContainer.Instance.Players[_cInfo.playerId].AuctionId = 0;
                                PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemName = "";
                                PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemCount = 0;
                                PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemQuality = 0;
                                PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemPrice = 0;
                                PersistentContainer.Instance.Players[_cInfo.playerId].AuctionCancelTime = DateTime.Now;
                                PersistentContainer.Instance.Save();
                            }
                            else
                            {
                                AuctionBox.AuctionItems.Remove(_id);
                                PersistentContainer.Instance.Players[_cInfo.playerId].AuctionReturn = true;
                                PersistentContainer.Instance.Save();
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Auction does not contain id {0}", _id));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid integer {0}", _id));
                    }
                    return;
                }
                else if (_params[0].ToLower().Equals("clear"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                    }
                    int _id;
                    if (int.TryParse(_params[1], out _id))
                    {
                        if (AuctionBox.AuctionItems.ContainsKey(_id))
                        {
                            AuctionBox.AuctionItems.Remove(_id);
                            SdtdConsole.Instance.Output(string.Format("Id {0} has been removed from the auction", _id));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Auction does not contain id {0}", _id));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid integer {0}", _id));
                    }
                    return;
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    bool _auctionItemsFound = false;
                    List<string> playerlist = PersistentContainer.Instance.Players.SteamIDs;
                    for (int i = 0; i < playerlist.Count; i++)
                    {
                        string _steamId = playerlist[i];
                        int _auctionId = PersistentContainer.Instance.Players[_steamId].AuctionId;
                        if (_auctionId > 0 && AuctionBox.AuctionItems.ContainsKey(_auctionId))
                        {
                            _auctionItemsFound = true;
                            int _auctionCount = PersistentContainer.Instance.Players[_steamId].AuctionItemCount;
                            string _auctionName = PersistentContainer.Instance.Players[_steamId].AuctionItemName;
                            int _auctionQuality = PersistentContainer.Instance.Players[_steamId].AuctionItemQuality;
                            int _auctionPrice = PersistentContainer.Instance.Players[_steamId].AuctionItemPrice;
                            if (_auctionQuality > 1)
                            {
                                string _message = "# {Id}: {Count} {Item} at {Quality} quality, for {Price} {Name}";
                                _message = _message.Replace("{Id}", _auctionId.ToString());
                                _message = _message.Replace("{Count}", _auctionCount.ToString());
                                _message = _message.Replace("{Item}", _auctionName);
                                _message = _message.Replace("{Quality}", _auctionQuality.ToString());
                                _message = _message.Replace("{Price}", _auctionPrice.ToString());
                                _message = _message.Replace("{Name}", Wallet.Coin_Name);
                                SdtdConsole.Instance.Output(_message);
                            }
                            else
                            {
                                string _message = "# {Id}: {Count} {Item} for {Price} {Name}";
                                _message = _message.Replace("{Id}", _auctionId.ToString());
                                _message = _message.Replace("{Count}", _auctionCount.ToString());
                                _message = _message.Replace("{Item}", _auctionName);
                                _message = _message.Replace("{Price}", _auctionPrice.ToString());
                                _message = _message.Replace("{Name}", Wallet.Coin_Name);
                                SdtdConsole.Instance.Output(_message);
                            }
                        }
                    }
                    if (!_auctionItemsFound)
                    {
                        SdtdConsole.Instance.Output("No items are currently for sale");
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AuctionConsole.Execute: {0}", e));
            }
        }
    }
}
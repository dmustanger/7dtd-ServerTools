using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ServerTools
{
    class AuctionBox
    {
        public static bool IsEnabled = false, IsRunning = false, No_Admins = false;
        public static int Delay_Between_Uses = 24, Admin_Level = 0, Cancel_Time = 15;
        public static string Command71 = "auction", Command72 = "auction cancel", Command73 = "auction buy", Command74 = "auction sell";
        public static Dictionary<int, string> AuctionItems = new Dictionary<int, string>();
        private static DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
        private static LinkedList<Chunk> chunkArray = new LinkedList<Chunk>();
        private static string file = string.Format("Auction_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/AuctionLog/{1}", API.ConfigPath, file);

        public static void Delay(ClientInfo _cInfo, string _price)
        {
            if (Delay_Between_Uses < 1)
            {
                CheckBox(_cInfo, _price);
            }
            else
            {
                DateTime _sellDate = PersistentContainer.Instance.Players[_cInfo.playerId].AuctionSellDate;
                TimeSpan varTime = DateTime.Now - _sellDate;
                double fractionalHours = varTime.TotalHours;
                int _timepassed = (int)fractionalHours;
                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                {
                    if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                    {
                        DateTime _dt;
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                        if (DateTime.Now < _dt)
                        {
                            int _newDelay = Delay_Between_Uses / 2;
                            Time(_cInfo, _timepassed, _newDelay, _price);
                            return;
                        }
                    }
                }
                Time(_cInfo, _timepassed, Delay_Between_Uses, _price);
            }
        }

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay, string _price)
        {
            if (_timepassed >= _delay)
            {
                CheckBox(_cInfo, _price);
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                string _phrase900;
                if (!Phrases.Dict.TryGetValue(900, out _phrase900))
                {
                    _phrase900 = " you can only use {CommandPrivate}{Command74} {DelayBetweenUses} hours after a sale. Time remaining: {TimeRemaining} hours.";
                }
                _phrase900 = _phrase900.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase900 = _phrase900.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase900 = _phrase900.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase900 = _phrase900.Replace("{Command74}", Command74);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase900 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }


        public static void CheckBox(ClientInfo _cInfo, string _price)
        {
            if (AuctionItems.ContainsKey(_cInfo.entityId))
            {
                string _message = " you have auction item # {Value} in the auction already. Wait for it to sell or cancel it with {CommandPrivate}{Command72}.";
                _message = _message.Replace("{Value}", _cInfo.entityId.ToString());
                _message = _message.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _message = _message.Replace("{Command72}", Command72);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                int _p;
                if (int.TryParse(_price, out _p))
                {
                    if (_p > 0)
                    {
                        ChunkClusterList chunklist = GameManager.Instance.World.ChunkClusters;
                        for (int i = 0; i < chunklist.Count; i++)
                        {
                            ChunkCluster chunk = chunklist[i];
                            chunkArray = chunk.GetChunkArray();
                            foreach (Chunk _c in chunkArray)
                            {
                                tiles = _c.GetTileEntities();
                                foreach (TileEntity tile in tiles.dict.Values)
                                {
                                    TileEntityType type = tile.GetTileEntityType();
                                    if (type.ToString().Equals("SecureLoot"))
                                    {
                                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tile;
                                        Vector3i vec3i = SecureLoot.ToWorldPos();
                                        if ((vec3i.x - _player.position.x) * (vec3i.x - _player.position.x) + (vec3i.z - _player.position.z) * (vec3i.z - _player.position.z) <= 2.5 * 2.5)
                                        {
                                            if (SecureLoot.IsUserAllowed(_cInfo.playerId) && !SecureLoot.IsUserAccessing())
                                            {
                                                ItemStack[] items = SecureLoot.items;
                                                ItemStack _item = items[0];
                                                if (_item != null && !_item.IsEmpty())
                                                {
                                                    ItemClass _itemClass = ItemClass.list[_item.itemValue.type];
                                                    string _itemName = _item.itemValue.ItemClass.GetLocalizedItemName() ?? _item.itemValue.ItemClass.GetItemName();
                                                    SecureLoot.UpdateSlot(0, ItemStack.Empty.Clone());
                                                    AuctionItems.Add(_cInfo.entityId, _cInfo.playerId);
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionId = _cInfo.entityId;
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemName = _itemName;
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemCount = _item.count;
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemQuality = _item.itemValue.Quality;
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemPrice = _p;
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionCancelTime = DateTime.Now;
                                                    PersistentContainer.Instance.Save();
                                                    string _message = " your auction item {Name} has been removed from the secure loot and added to the auction.";
                                                    _message = _message.Replace("{Name}", _itemName);
                                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                    Log.Out(string.Format("{0} has added {1} {2}, {3} quality to the auction for {4} {5}.", _cInfo.playerName, _item.count, _itemName, _item.itemValue.Quality, _price, Wallet.Coin_Name));
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} {3}, {4} quality to the auction for {5} {6}.", DateTime.Now, _cInfo.playerName, _item.count, _itemName, _item.itemValue.Quality, _price, Wallet.Coin_Name));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " no item was found in the first slot of the secure chest near you." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you need to input a price greater than zero. This is not a transfer system.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your sell price must be an integer and greater than zero.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void AuctionList(ClientInfo _cInfo)
        {
            bool _auctionItemsFound = false;
            List<string> playerlist = PersistentContainer.Instance.Players.SteamIDs;
            for (int i = 0; i < playerlist.Count; i++)
            {
                string _steamId = playerlist[i];
                int _auctionId = PersistentContainer.Instance.Players[_steamId].AuctionId;
                if (_auctionId > 0)
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
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _message = "# {Id}: {Count} {Item} for {Price} {Name}";
                        _message = _message.Replace("{Id}", _auctionId.ToString());
                        _message = _message.Replace("{Count}", _auctionCount.ToString());
                        _message = _message.Replace("{Item}", _auctionName);
                        _message = _message.Replace("{Price}", _auctionPrice.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            if (!_auctionItemsFound)
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " no items are currently for sale.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
            

        public static void WalletCheck(ClientInfo _cInfo, int _purchase)
        {
            if (AuctionItems.ContainsKey(_purchase))
            {
                string _steamId;
                if (AuctionItems.TryGetValue(_purchase, out _steamId))
                {
                    int _itemPrice = PersistentContainer.Instance.Players[_steamId].AuctionItemPrice;
                    int _currentCoins = Wallet.GetCurrentCoins(_cInfo);
                    if (_currentCoins >= _itemPrice)
                    {
                        BuyAuction(_cInfo, _purchase);
                    }
                    else
                    {
                        int _missing = _itemPrice - _currentCoins;
                        string _message = " you can not make this purchase. You need {Value} more {Name}.";
                        _message = _message.Replace("{Value}", _missing.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " this # could not be found. Please check the auction list by typing " + ChatHook.Command_Private + Command71 + ".[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void BuyAuction(ClientInfo _cInfo, int _purchase)
        {
            string _steamId;
            if (AuctionItems.TryGetValue(_purchase, out _steamId))
            {
                int _auctionCount = PersistentContainer.Instance.Players[_steamId].AuctionItemCount;
                string _auctionName = PersistentContainer.Instance.Players[_steamId].AuctionItemName;
                int _auctionQuality = PersistentContainer.Instance.Players[_steamId].AuctionItemQuality;
                int _auctionPrice = PersistentContainer.Instance.Players[_steamId].AuctionItemPrice;
                ItemValue itemValue = ItemClass.GetItem(_auctionName, false);
                if (itemValue.type == ItemValue.None.type)
                {
                    Log.Out(string.Format("Could not find itemValue for {0}", _auctionName));
                    return;
                }
                else
                {
                    itemValue = new ItemValue(ItemClass.GetItem(_auctionName).type, _auctionQuality, _auctionQuality, false, null, 0);
                }
                World world = GameManager.Instance.World;
                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                {
                    entityClass = EntityClass.FromString("item"),
                    id = EntityFactory.nextEntityID++,
                    itemStack = new ItemStack(itemValue, _auctionCount),
                    pos = world.Players.dict[_cInfo.entityId].position,
                    rot = new Vector3(20f, 0f, 20f),
                    lifetime = 60f,
                    belongsPlayerId = _cInfo.entityId
                });
                world.SpawnEntityInWorld(entityItem);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);

                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _auctionPrice);
                string _message = " you have purchased {Count} {ItemName} from the auction for {Value} {CoinName}.";
                _message = _message.Replace("{Count}", _auctionCount.ToString());
                _message = _message.Replace("{ItemName}", _auctionName);
                _message = _message.Replace("{Value}", _auctionPrice.ToString());
                _message = _message.Replace("{CoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                double _percent = _auctionPrice * 0.05;
                int _newCoin2 = _auctionPrice - (int)_percent;
                AuctionItems.Remove(_purchase);
                PersistentContainer.Instance.Players[_steamId].AuctionId = 0;
                PersistentContainer.Instance.Players[_steamId].AuctionItemName = "";
                PersistentContainer.Instance.Players[_steamId].AuctionItemCount = 0;
                PersistentContainer.Instance.Players[_steamId].AuctionItemQuality = 0;
                PersistentContainer.Instance.Players[_steamId].AuctionItemPrice = 0;
                PersistentContainer.Instance.Players[_steamId].AuctionSellDate = DateTime.Now;
                PersistentContainer.Instance.Save();
                Wallet.AddCoinsToWallet(_steamId, _newCoin2);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " seller has received the funds in their wallet.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_steamId);
                if (_cInfo2 != null)
                {
                    ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo2.playerName + LoadConfig.Chat_Response_Color + " your auction item was purchased and the value placed in your wallet.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                using (StreamWriter sw = new StreamWriter(filepath, true))
                {
                    sw.WriteLine(string.Format("{0}: {1} has purchased auction entry {2}, profits went to steam id {3}", DateTime.Now, _cInfo.playerName, _purchase, _steamId));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static void CancelAuction(ClientInfo _cInfo)
        {
            if (AuctionItems.ContainsKey(_cInfo.entityId))
            {
                DateTime _auctionCancelTime = PersistentContainer.Instance.Players[_cInfo.playerId].AuctionCancelTime;
                TimeSpan varTime = DateTime.Now - _auctionCancelTime;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (_timepassed >= Cancel_Time)
                {
                    int _auctionCount = PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemCount;
                    string _auctionName = PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemName;
                    int _auctionQuality = PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemQuality;
                    int _auctionPrice = PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemPrice;
                    ItemValue itemValue = ItemClass.GetItem(_auctionName, false);
                    if (itemValue.type == ItemValue.None.type)
                    {
                        Log.Out(string.Format("Could not find itemValue {0}", itemValue));
                        return;
                    }
                    else
                    {
                        itemValue = new ItemValue(ItemClass.GetItem(_auctionName).type, _auctionQuality, _auctionQuality, false, null, 1);
                    }
                    World world = GameManager.Instance.World;
                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                    {
                        entityClass = EntityClass.FromString("item"),
                        id = EntityFactory.nextEntityID++,
                        itemStack = new ItemStack(itemValue, _auctionCount),
                        pos = world.Players.dict[_cInfo.entityId].position,
                        rot = new Vector3(20f, 0f, 20f),
                        lifetime = 60f,
                        belongsPlayerId = _cInfo.entityId
                    });
                    world.SpawnEntityInWorld(entityItem);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                    AuctionItems.Remove(_cInfo.entityId);
                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionId = 0;
                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemName = "";
                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemCount = 0;
                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemQuality = 0;
                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionItemPrice = 0;
                    PersistentContainer.Instance.Players[_cInfo.playerId].AuctionCancelTime = DateTime.Now;
                    PersistentContainer.Instance.Save();
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your auction item has returned to you.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    using (StreamWriter sw = new StreamWriter(filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}: {1} has cancelled their auction entry # {2}.", DateTime.Now, _cInfo.playerName, _cInfo.entityId));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
                else
                {
                    int _timeleft = Cancel_Time - _timepassed;
                    string _message = " you must wait {Time1} minutes before you can cancel your auction item. Be careful what you sell. Time remaining: {Time2} minutes.[-]";
                    _message = _message.Replace("{Time1}", Cancel_Time.ToString());
                    _message = _message.Replace("{Time2}", _timeleft.ToString());
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have no items in the auction.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void AuctionList()
        {
            List<string> playerlist = PersistentContainer.Instance.Players.SteamIDs;
            for (int i = 0; i < playerlist.Count; i++)
            {
                string _steamId = playerlist[i];
                PersistentPlayer p = PersistentContainer.Instance.Players[_steamId];
                if (p != null)
                {
                    if (p.AuctionId != 0)
                    {
                        AuctionItems.Add(p.AuctionId, _steamId);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class AuctionBox
    {
        public static bool IsEnabled = false, IsRunning = false, No_Admins = false;
        public static int Admin_Level = 0, Total_Items = 1, Tax;
        public static string Command71 = "auction", Command72 = "auction cancel", Command73 = "auction buy", Command74 = "auction sell";
        public static Dictionary<int, string> AuctionItems = new Dictionary<int, string>();
        private static string file = string.Format("Auction_{0}.txt", DateTime.Today.ToString("M-d-yyyy")), filepath = string.Format("{0}/Logs/AuctionLogs/{1}", API.ConfigPath, file);
        private static System.Random random = new System.Random();

        public static void CheckBox(ClientInfo _cInfo, string _price)
        {
            try
            {
                if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player != null)
                    {
                        if (int.TryParse(_price, out int _auctionPrice))
                        {
                            if (_auctionPrice > 0)
                            {
                                if (PersistentContainer.Instance.Players[_cInfo.playerId].Auction == null)
                                {
                                    Dictionary<int, ItemDataSerializable> _auctionItems = new Dictionary<int, ItemDataSerializable>();
                                    PersistentContainer.Instance.Players[_cInfo.playerId].Auction = _auctionItems;
                                    PersistentContainer.Instance.Save();
                                }
                                else if (PersistentContainer.Instance.Players[_cInfo.playerId].Auction.Count < Total_Items)
                                {
                                    LinkedList<Chunk> _chunkArray = new LinkedList<Chunk>();
                                    DictionaryList<Vector3i, TileEntity> _tiles = new DictionaryList<Vector3i, TileEntity>();
                                    ChunkClusterList _chunklist = GameManager.Instance.World.ChunkClusters;
                                    for (int i = 0; i < _chunklist.Count; i++)
                                    {
                                        ChunkCluster _chunk = _chunklist[i];
                                        _chunkArray = _chunk.GetChunkArray();
                                        foreach (Chunk _c in _chunkArray)
                                        {
                                            _tiles = _c.GetTileEntities();
                                            foreach (TileEntity _tile in _tiles.dict.Values)
                                            {
                                                TileEntityType _type = _tile.GetTileEntityType();
                                                if (_type.ToString().Equals("SecureLoot"))
                                                {
                                                    TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)_tile;
                                                    Vector3i vec3i = SecureLoot.ToWorldPos();
                                                    if ((vec3i.x - _player.position.x) * (vec3i.x - _player.position.x) + (vec3i.z - _player.position.z) * (vec3i.z - _player.position.z) <= 3 * 3)
                                                    {
                                                        if (vec3i.y >= (int)_player.position.y - 3 && vec3i.y <= (int)_player.position.y + 3)
                                                        {
                                                            if (SecureLoot.IsUserAllowed(_cInfo.playerId) && !SecureLoot.IsUserAccessing())
                                                            {
                                                                ItemStack[] items = SecureLoot.items;
                                                                ItemStack _item = items[0];
                                                                if (_item != null && !_item.IsEmpty())
                                                                {
                                                                    int _id = GenerateAuctionId();
                                                                    if (_id > 0)
                                                                    {
                                                                        AuctionItems.Add(_id, _cInfo.playerId);
                                                                        items[0] = ItemStack.Empty.Clone();
                                                                        if (PersistentContainer.Instance.Players[_cInfo.playerId].Auction != null && PersistentContainer.Instance.Players[_cInfo.playerId].Auction.Count > 0)
                                                                        {
                                                                            ItemDataSerializable _serializedItemStack = new ItemDataSerializable();
                                                                            {
                                                                                _serializedItemStack.name = _item.itemValue.ItemClass.GetItemName();
                                                                                _serializedItemStack.count = _item.count;
                                                                                _serializedItemStack.useTimes = _item.itemValue.UseTimes;
                                                                                _serializedItemStack.quality = _item.itemValue.Quality;
                                                                            }
                                                                            PersistentContainer.Instance.Players[_cInfo.playerId].Auction.Add(_id, _serializedItemStack);
                                                                        }
                                                                        else
                                                                        {
                                                                            ItemDataSerializable _serializedItemStack = new ItemDataSerializable();
                                                                            {
                                                                                _serializedItemStack.name = _item.itemValue.ItemClass.GetItemName();
                                                                                _serializedItemStack.count = _item.count;
                                                                                _serializedItemStack.useTimes = _item.itemValue.UseTimes;
                                                                                _serializedItemStack.quality = _item.itemValue.Quality;
                                                                            }
                                                                            Dictionary<int, ItemDataSerializable> _auctionItems = new Dictionary<int, ItemDataSerializable>();
                                                                            _auctionItems.Add(_id, _serializedItemStack);
                                                                            PersistentContainer.Instance.Players[_cInfo.playerId].Auction = _auctionItems;
                                                                        }
                                                                        if (PersistentContainer.Instance.AuctionPrices != null && PersistentContainer.Instance.AuctionPrices.Count > 0)
                                                                        {
                                                                            PersistentContainer.Instance.AuctionPrices.Add(_id, _auctionPrice);
                                                                        }
                                                                        else
                                                                        {
                                                                            Dictionary<int, int> _auctionPrices = new Dictionary<int, int>();
                                                                            _auctionPrices.Add(_id, _auctionPrice);
                                                                            PersistentContainer.Instance.AuctionPrices = _auctionPrices;
                                                                        }
                                                                        PersistentContainer.Instance.Save();
                                                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                                                        {
                                                                            sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4}, {5} quality, {6} percent durability for {7} {8}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _item.count, _item.itemValue.ItemClass.GetItemName(), _item.itemValue.Quality, _item.itemValue.UseTimes / _item.itemValue.MaxUseTimes * 100, _price, Wallet.Coin_Name));
                                                                            sw.WriteLine();
                                                                            sw.Flush();
                                                                            sw.Close();
                                                                        }
                                                                        string _message = "Your auction item {Name} has been removed from the secure loot and added to the auction.";
                                                                        _message = _message.Replace("{Name}", _item.itemValue.ItemClass.GetLocalizedItemName() ?? _item.itemValue.ItemClass.GetItemName());
                                                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                                        _tile.SetModified();
                                                                        return;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    string _message = "You have the max auction items already listed. Wait for one to sell or cancel it with {CommandPrivate}{Command72} #.";
                                    _message = _message.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                    _message = _message.Replace("{Command72}", Command72);
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You need to input a price greater than zero. This is not a transfer system.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your sell price must be an integer and greater than zero.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AuctionBox.CheckBox: {0}", e.Message));
            }
        }

        public static void AuctionList(ClientInfo _cInfo)
        {
            try
            {
                if (AuctionItems.Count > 0)
                {
                    if (PersistentContainer.Instance.AuctionPrices != null)
                    {
                        Dictionary<int, int> _auctionPrices = PersistentContainer.Instance.AuctionPrices;
                        List<string> playerlist = AuctionItems.Values.ToList();
                        if (playerlist != null && playerlist.Count > 0)
                        {
                            for (int i = 0; i < playerlist.Count; i++)
                            {
                                string _steamId = playerlist[i];
                                if (PersistentContainer.Instance.Players[_steamId].Auction != null && PersistentContainer.Instance.Players[_steamId].Auction.Count > 0)
                                {
                                    Dictionary<int, ItemDataSerializable> _auctionItems = PersistentContainer.Instance.Players[_steamId].Auction;
                                    foreach (var _item in _auctionItems)
                                    {
                                        ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_item.Value.name, false).type, false);
                                        if (_itemValue != null)
                                        {
                                            if (_auctionPrices.TryGetValue(_item.Key, out int _price))
                                            {
                                                string _message = "# {Id}: {Count} {Item} at {Quality} quality, {Durability} percent durability for {Price} {Coin}";
                                                _message = _message.Replace("{Id}", _item.Key.ToString());
                                                _message = _message.Replace("{Count}", _item.Value.count.ToString());
                                                _message = _message.Replace("{Item}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName());
                                                _message = _message.Replace("{Quality}", _item.Value.quality.ToString());
                                                _message = _message.Replace("{Durability}", (_item.Value.useTimes / _itemValue.MaxUseTimes * 100).ToString());
                                                _message = _message.Replace("{Price}", _price.ToString());
                                                _message = _message.Replace("{Coin}", Wallet.Coin_Name);
                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "No items are currently for sale.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AuctionBox.AuctionList: {0}", e.Message));
            }
        }     

        public static void WalletCheck(ClientInfo _cInfo, int _purchase)
        {
            if (AuctionItems.ContainsKey(_purchase))
            {
                if (PersistentContainer.Instance.AuctionPrices != null)
                {
                    Dictionary<int, int> _auctionPrices = PersistentContainer.Instance.AuctionPrices;
                    int _price = 0;
                    if (_auctionPrices.ContainsKey(_purchase))
                    {
                        _auctionPrices.TryGetValue(_purchase, out _price);
                    }
                    int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
                    if (_currentCoins >= _price)
                    {
                        BuyAuction(_cInfo, _purchase, _price);
                    }
                    else
                    {
                        int _missing = _price - _currentCoins;
                        string _message = "You can not make this purchase. You need {Value} more {Name}.";
                        _message = _message.Replace("{Value}", _missing.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "This # could not be found. Please check the auction list by typing " + ChatHook.Command_Private + Command71 + ".[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void BuyAuction(ClientInfo _cInfo, int _purchase, int _price)
        {
            if (AuctionItems.TryGetValue(_purchase, out string _steamId))
            {
                if (PersistentContainer.Instance.Players[_steamId].Auction != null && PersistentContainer.Instance.Players[_steamId].Auction.Count > 0)
                {
                    PersistentContainer.Instance.Players[_steamId].Auction.TryGetValue(_purchase, out ItemDataSerializable _itemData);
                    ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_itemData.name, false).type, false);
                    if (_itemValue != null)
                    {
                        _itemValue.UseTimes = _itemData.useTimes;
                        _itemValue.Quality = _itemData.quality;
                        World world = GameManager.Instance.World;
                        EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                        {
                            entityClass = EntityClass.FromString("item"),
                            id = EntityFactory.nextEntityID++,
                            itemStack = new ItemStack(_itemValue, _itemData.count),
                            pos = world.Players.dict[_cInfo.entityId].position,
                            rot = new Vector3(20f, 0f, 20f),
                            lifetime = 60f,
                            belongsPlayerId = _cInfo.entityId
                        });
                        world.SpawnEntityInWorld(entityItem);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                        world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                        AuctionItems.Remove(_purchase);
                        PersistentContainer.Instance.Players[_steamId].Auction.Remove(_purchase);
                        PersistentContainer.Instance.AuctionPrices.Remove(_purchase);
                        PersistentContainer.Instance.Save();
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _price);
                        float _fee = _price * ((float)Tax / 100);
                        int _adjustedPrice = _price - (int)_fee;
                        Wallet.AddCoinsToWallet(_steamId, _adjustedPrice);
                        string _playerName = PersistentOperations.GetPlayerDataFileFromSteamId(_steamId).ecd.entityName;
                        using (StreamWriter sw = new StreamWriter(filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: {1} {2} has purchased auction entry {3}, profits went to steam id {4} {5}", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _purchase, _steamId, _playerName));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        string _message = "You have purchased {Count} {ItemName} from the auction for {Value} {CoinName}.";
                        _message = _message.Replace("{Count}", _itemData.count.ToString());
                        _message = _message.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName());
                        _message = _message.Replace("{Value}", _price.ToString());
                        _message = _message.Replace("{CoinName}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_steamId);
                        if (_cInfo2 != null)
                        {
                            ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + "Your auction item was purchased and the value placed in your wallet.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        public static void CancelAuction(ClientInfo _cInfo, string _auctionId)
        {
            if (int.TryParse(_auctionId, out int _id))
            {
                if (AuctionItems.ContainsKey(_id))
                {
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].Auction != null && PersistentContainer.Instance.Players[_cInfo.playerId].Auction.Count > 0)
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].Auction.TryGetValue(_id, out ItemDataSerializable _itemData))
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
                                    rot = new Vector3(20f, 0f, 20f),
                                    lifetime = 60f,
                                    belongsPlayerId = _cInfo.entityId
                                });
                                world.SpawnEntityInWorld(entityItem);
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                AuctionItems.Remove(_id);
                                PersistentContainer.Instance.Players[_cInfo.playerId].Auction.Remove(_id);
                                PersistentContainer.Instance.AuctionPrices.Remove(_id);
                                PersistentContainer.Instance.Save();
                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}: {1} {2} has cancelled their auction entry # {3}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _cInfo.entityId));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your auction item has returned to you.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Could not find this id listed in the auction. Unable to cancel.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
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
                    if (p.Auction != null && p.Auction.Count > 0)
                    {
                        foreach (var _auctionItem in p.Auction)
                        {
                            AuctionItems.Add(_auctionItem.Key, _steamId);
                        }
                    }
                }
            }
        }

        private static int GenerateAuctionId()
        {
            int _id = random.Next(1000, 5001);
            if (AuctionItems.ContainsKey(_id))
            {
                _id = random.Next(1000, 5001);
                if (AuctionItems.ContainsKey(_id))
                {
                    _id = random.Next(1000, 5001);
                    if (AuctionItems.ContainsKey(_id))
                    {
                        _id = random.Next(1000, 5001);
                        return 0;
                    }
                    else
                    {
                        return _id;
                    }
                }
                else
                {
                    return _id;
                }
            }
            else
            {
                return _id;
            }
        }
    }
}

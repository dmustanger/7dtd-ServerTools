using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class Auction
    {
        public static bool IsEnabled = false, IsRunning = false, No_Admins = false;
        public static int Admin_Level = 0, Total_Items = 1, Tax;
        public static string Command_auction = "auction", Command_auction_cancel = "auction cancel", Command_auction_buy = "auction buy", Command_auction_sell = "auction sell";
        public static Dictionary<int, string> AuctionItems = new Dictionary<int, string>();

        private static readonly string file = string.Format("Auction_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/AuctionLogs/{1}", API.ConfigPath, file);
        private static readonly System.Random Random = new System.Random();

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
                                    PersistentContainer.DataChange = true;
                                }
                                else if (PersistentContainer.Instance.Players[_cInfo.playerId].Auction.Count < Total_Items)
                                {
                                    List<Chunk> _chunks = new List<Chunk>();
                                    DictionaryList<Vector3i, TileEntity> _tiles = new DictionaryList<Vector3i, TileEntity>();
                                    Vector3 _position = _player.position;
                                    Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)_position.x - 5, (int)_position.y, (int)_position.z);
                                    if (_chunk != null && !_chunks.Contains(_chunk))
                                    {
                                        _chunks.Add(_chunk);
                                    }
                                    _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)_position.x + 5, (int)_position.y, (int)_position.z);
                                    if (_chunk != null && !_chunks.Contains(_chunk))
                                    {
                                        _chunks.Add(_chunk);
                                    }
                                    _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)_position.x, (int)_position.y, (int)_position.z - 5);
                                    if (_chunk != null && !_chunks.Contains(_chunk))
                                    {
                                        _chunks.Add(_chunk);
                                    }
                                    _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)_position.x + 5, (int)_position.y, (int)_position.z + 5);
                                    if (_chunk != null && !_chunks.Contains(_chunk))
                                    {
                                        _chunks.Add(_chunk);
                                    }
                                    for (int i = 0; i < _chunks.Count; i++)
                                    {
                                        _tiles = _chunks[i].GetTileEntities();
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
                                                                if (_item.itemValue.Modifications.Length > 0)
                                                                {
                                                                    for (int j = 0; j < _item.itemValue.Modifications.Length; j++)
                                                                    {
                                                                        if (!_item.itemValue.Modifications[j].IsEmpty())
                                                                        {
                                                                            Phrases.Dict.TryGetValue("Auction18", out string _phrase);
                                                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                            return;
                                                                        }
                                                                    }
                                                                }
                                                                if (_item.itemValue.CosmeticMods.Length > 0)
                                                                {
                                                                    for (int j = 0; j < _item.itemValue.CosmeticMods.Length; j++)
                                                                    {
                                                                        if (!_item.itemValue.CosmeticMods[j].IsEmpty())
                                                                        {
                                                                            Phrases.Dict.TryGetValue("Auction18", out string _phrase);
                                                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                            return;
                                                                        }
                                                                    }
                                                                }
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
                                                                            if (_item.itemValue.CosmeticMods.Length > 0)
                                                                            {
                                                                                _serializedItemStack.modSlots = _item.itemValue.Modifications.Length;
                                                                            }
                                                                            else
                                                                            {
                                                                                _serializedItemStack.modSlots = 0;
                                                                            }
                                                                            if (_item.itemValue.CosmeticMods.Length > 0)
                                                                            {
                                                                                _serializedItemStack.cosmeticSlots = _item.itemValue.CosmeticMods.Length;
                                                                            }
                                                                            else
                                                                            {
                                                                                _serializedItemStack.cosmeticSlots = 0;
                                                                            }
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
                                                                            if (_item.itemValue.Modifications.Length > 0)
                                                                            {
                                                                                _serializedItemStack.modSlots = _item.itemValue.Modifications.Length;
                                                                            }
                                                                            else
                                                                            {
                                                                                _serializedItemStack.modSlots = 0;
                                                                            }
                                                                            if (_item.itemValue.CosmeticMods.Length > 0)
                                                                            {
                                                                                _serializedItemStack.cosmeticSlots = _item.itemValue.CosmeticMods.Length;
                                                                            }
                                                                            else
                                                                            {
                                                                                _serializedItemStack.cosmeticSlots = 0;
                                                                            }
                                                                        }
                                                                        Dictionary<int, ItemDataSerializable> _auctionItems = new Dictionary<int, ItemDataSerializable>
                                                                            {
                                                                                { _id, _serializedItemStack }
                                                                            };
                                                                        PersistentContainer.Instance.Players[_cInfo.playerId].Auction = _auctionItems;
                                                                    }
                                                                    if (PersistentContainer.Instance.AuctionPrices != null && PersistentContainer.Instance.AuctionPrices.Count > 0)
                                                                    {
                                                                        PersistentContainer.Instance.AuctionPrices.Add(_id, _auctionPrice);
                                                                    }
                                                                    else
                                                                    {
                                                                        Dictionary<int, int> _auctionPrices = new Dictionary<int, int>
                                                                            {
                                                                                { _id, _auctionPrice }
                                                                            };
                                                                        PersistentContainer.Instance.AuctionPrices = _auctionPrices;
                                                                    }
                                                                    _tile.SetModified();
                                                                    PersistentContainer.DataChange = true;
                                                                    if (_item.itemValue.HasQuality)
                                                                    {
                                                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                                        {
                                                                            sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4}, {5} quality, {6} percent durability for {7} {8}", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _item.count, _item.itemValue.ItemClass.GetItemName(), _item.itemValue.Quality, _item.itemValue.UseTimes / _item.itemValue.MaxUseTimes * 100, _price, Wallet.Coin_Name));
                                                                            sw.WriteLine();
                                                                            sw.Flush();
                                                                            sw.Close();
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                                        {
                                                                            sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4}, for {5} {6}", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _item.count, _item.itemValue.ItemClass.GetItemName(), _price, Wallet.Coin_Name));
                                                                            sw.WriteLine();
                                                                            sw.Flush();
                                                                            sw.Close();
                                                                        }
                                                                    }
                                                                    Phrases.Dict.TryGetValue("Auction1", out string _phrase);
                                                                    _phrase = _phrase.Replace("{Name}", _item.itemValue.ItemClass.GetLocalizedItemName() ?? _item.itemValue.ItemClass.GetItemName());
                                                                    _phrase = _phrase.Replace("{Value}", _id.ToString());
                                                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                    return;
                                                                }
                                                                else
                                                                {
                                                                    Phrases.Dict.TryGetValue("Auction16", out string _phrase);
                                                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                    Phrases.Dict.TryGetValue("Auction2", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_auction_cancel}", Command_auction_cancel);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Auction3", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Auction4", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Auction.CheckBox: {0}", e.Message));
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
                                                if (_itemValue.HasQuality)
                                                {
                                                    Phrases.Dict.TryGetValue("Auction5", out string _phrase);
                                                    _phrase = _phrase.Replace("{Id}", _item.Key.ToString());
                                                    _phrase = _phrase.Replace("{Count}", _item.Value.count.ToString());
                                                    _phrase = _phrase.Replace("{Item}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName());
                                                    _phrase = _phrase.Replace("{Quality}", _item.Value.quality.ToString());
                                                    _phrase = _phrase.Replace("{Durability}", (_item.Value.useTimes / _itemValue.MaxUseTimes * 100).ToString());
                                                    _phrase = _phrase.Replace("{Price}", _price.ToString());
                                                    _phrase = _phrase.Replace("{Coin}", Wallet.Coin_Name);
                                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                }
                                                else
                                                {
                                                    Phrases.Dict.TryGetValue("Auction17", out string _phrase);
                                                    _phrase = _phrase.Replace("{Id}", _item.Key.ToString());
                                                    _phrase = _phrase.Replace("{Count}", _item.Value.count.ToString());
                                                    _phrase = _phrase.Replace("{Item}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName());
                                                    _phrase = _phrase.Replace("{Price}", _price.ToString());
                                                    _phrase = _phrase.Replace("{Coin}", Wallet.Coin_Name);
                                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    Phrases.Dict.TryGetValue("Auction6", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Auction.AuctionList: {0}", e.Message));
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
                        Phrases.Dict.TryGetValue("Auction7", out string _phrase);
                        _phrase = _phrase.Replace("{Value}", _missing.ToString());
                        _phrase = _phrase.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Auction8", out string _phrase);
                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase = _phrase.Replace("{Command_auction}", Command_auction);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void BuyAuction(ClientInfo _cInfo, int _purchase, int _price)
        {
            if (AuctionItems.TryGetValue(_purchase, out string _steamId))
            {
                if (PersistentContainer.Instance.Players[_steamId].Auction != null && PersistentContainer.Instance.Players[_steamId].Auction.Count > 0)
                {
                    PersistentContainer.Instance.Players[_steamId].Auction.TryGetValue(_purchase, out ItemDataSerializable _itemData);
                    ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_itemData.name, false).type, _itemData.quality, _itemData.quality, false);
                    if (_itemValue != null)
                    {
                        _itemValue.UseTimes = _itemData.useTimes;
                        if (_itemData.modSlots > 0)
                        {
                            _itemValue.Modifications = new ItemValue[_itemData.modSlots];
                        }
                        if (_itemData.cosmeticSlots > 0)
                        {
                            _itemValue.CosmeticMods = new ItemValue[_itemData.cosmeticSlots];
                        }
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
                        PersistentContainer.DataChange = true;
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _price);
                        float _fee = _price * ((float)Tax / 100);
                        int _adjustedPrice = _price - (int)_fee;
                        Wallet.AddCoinsToWallet(_steamId, _adjustedPrice);
                        string _playerName = PersistentOperations.GetPlayerDataFileFromSteamId(_steamId).ecd.entityName;
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("{0}: {1} {2} has purchased auction entry {3}, profits went to steam id {4} {5}", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _purchase, _steamId, _playerName));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Phrases.Dict.TryGetValue("Auction9", out string _phrase);
                        _phrase = _phrase.Replace("{Count}", _itemData.count.ToString());
                        _phrase = _phrase.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName());
                        _phrase = _phrase.Replace("{Value}", _price.ToString());
                        _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_steamId);
                        if (_cInfo2 != null)
                        {
                            Phrases.Dict.TryGetValue("Auction10", out string _phrase1);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_itemData.name, false).type, _itemData.quality, _itemData.quality, false);
                            if (_itemValue != null)
                            {
                                _itemValue.UseTimes = _itemData.useTimes;
                                if (_itemData.modSlots > 0)
                                {
                                    _itemValue.Modifications = new ItemValue[_itemData.modSlots];
                                }
                                if (_itemData.cosmeticSlots > 0)
                                {
                                    _itemValue.CosmeticMods = new ItemValue[_itemData.cosmeticSlots];
                                }
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
                                PersistentContainer.DataChange = true;
                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: {1} {2} has cancelled their auction entry # {3}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _cInfo.entityId));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                Phrases.Dict.TryGetValue("Auction11", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Auction12", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
            int _id = 0;
            for (int i = 0; i < 20; i++)
            {
                _id = Random.Next(1000, 5001);
                if (!AuctionItems.ContainsKey(_id))
                {
                    break;
                }
            }
            return _id;
        }
    }
}

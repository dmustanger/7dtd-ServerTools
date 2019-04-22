using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class AuctionBox
    {
        public static bool IsEnabled = false, IsRunning = false, No_Admins = false;
        public static int Delay_Between_Uses = 24, Admin_Level = 0;
        public static string Command71 = "auction", Command72 = "auction cancel", Command73 = "auction buy", Command74 = "auction sell";
        private static DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
        private static LinkedList<Chunk> chunkArray = new LinkedList<Chunk>();
        private static string file = string.Format("Auction_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/AuctionLog/{1}", API.ConfigPath, file);

        public static void Delay(ClientInfo _cInfo, string _price)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                CheckBox(_cInfo, _price);
            }
            else
            {
                string _sql = string.Format("SELECT sellDate FROM Auction WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                if (_result.Rows.Count == 0)
                {
                    CheckBox(_cInfo, _price);
                }
                else
                {
                    DateTime _sellDate;
                    DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _sellDate);
                    if (_sellDate.ToString() == "10/29/2000 7:30:00 AM")
                    {
                        CheckBox(_cInfo, _price);
                    }
                    else
                    {
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
                                    _donator = true;
                                    int _newDelay = Delay_Between_Uses / 2;
                                    if (_timepassed >= _newDelay)
                                    {
                                        CheckBox(_cInfo, _price);
                                    }
                                    else
                                    {
                                        int _timeleft = _newDelay - _timepassed;
                                        string _phrase900;
                                        if (!Phrases.Dict.TryGetValue(900, out _phrase900))
                                        {
                                            _phrase900 = " you can only use {CommandPrivate}{Command74} {DelayBetweenUses} hours after a sale. Time remaining: {TimeRemaining} hours.";
                                        }
                                        _phrase900 = _phrase900.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                        _phrase900 = _phrase900.Replace("{TimeRemaining}", _timeleft.ToString());
                                        _phrase900 = _phrase900.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                        _phrase900 = _phrase900.Replace("{Command74}", Command74);
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase900 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                        if (!_donator)
                        {
                            if (_timepassed >= Delay_Between_Uses)
                            {
                                CheckBox(_cInfo, _price);
                            }
                            else
                            {
                                int _timeleft = Delay_Between_Uses - _timepassed;
                                string _phrase900;
                                if (!Phrases.Dict.TryGetValue(900, out _phrase900))
                                {
                                    _phrase900 = " you can only use {CommandPrivate}{Command74} {DelayBetweenUses} hours after a sale. Time remaining: {TimeRemaining} hours.";
                                }
                                _phrase900 = _phrase900.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                _phrase900 = _phrase900.Replace("{TimeRemaining}", _timeleft.ToString());
                                _phrase900 = _phrase900.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                _phrase900 = _phrase900.Replace("{Command74}", Command74);
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase900 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                _result.Dispose();
            }
        }

        public static void CheckBox(ClientInfo _cInfo, string _price)
        {
            string _sql = string.Format("SELECT auctionid, steamid FROM Auction WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                int _auctionid;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _auctionid);
                string _message = " you have auction item # {Value} in the auction already. Wait for it to sell or cancel it with {CommandPrivate}{Command72}.";
                _message = _message.Replace("{Value}", _auctionid.ToString());
                _message = _message.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _message = _message.Replace("{Command72}", Command72);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                        if ((vec3i.x - _player.position.x) * (vec3i.x - _player.position.x) + (vec3i.z - _player.position.z) * (vec3i.z - _player.position.z) <= 1.5 * 1.5)
                                        {
                                            int _playerCount = ConnectionManager.Instance.ClientCount();
                                            if (_playerCount > 1)
                                            {
                                                List<ClientInfo> ClientInfoList = ConnectionManager.Instance.Clients.List.ToList();
                                                for (int k = 0; k < ClientInfoList.Count; k++)
                                                {
                                                    ClientInfo _cInfo2 = ClientInfoList[k];
                                                    if (_cInfo != _cInfo2)
                                                    {
                                                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                                                        if ((vec3i.x - _player2.position.x) * (vec3i.x - _player2.position.x) + (vec3i.z - _player2.position.z) * (vec3i.z - _player2.position.z) <= 8 * 8)
                                                        {
                                                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are too close to another player to use auction. Tell them to back off and get their own moldy sandwich.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                            List<string> boxUsers = SecureLoot.GetUsers();
                                            if (!boxUsers.Contains(_cInfo.playerId) && !SecureLoot.GetOwner().Equals(_cInfo.playerId))
                                            {
                                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", the local secure loot is not owned by you or a friend. You can only auction an item through a secure loot you own.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                return;
                                            }
                                            ItemStack[] items = SecureLoot.items;
                                            int count = 0;
                                            int slotNumber = 0;
                                            foreach (ItemStack item in items)
                                            {
                                                if (!item.IsEmpty())
                                                {
                                                    if (count < 1)
                                                    {
                                                        ItemClass _itemClass = ItemClass.list[item.itemValue.type];
                                                        string _itemName = _itemClass.GetItemName();
                                                        SecureLoot.UpdateSlot(slotNumber, ItemStack.Empty);
                                                        _sql = string.Format("INSERT INTO Auction (steamid, itemName, itemCount, itemQuality, itemPrice, cancelTime, sellDate) VALUES ('{0}', '{1}', {2}, {3}, {4}, '{5}', '{6}')", _cInfo.playerId, _itemName, item.count, item.itemValue.Quality, _price, DateTime.Now, DateTime.Now);
                                                        SQL.FastQuery(_sql, "AuctionBox");
                                                        string _message = "your auction item {Name} has been removed from the secure loot and added to the auction.";
                                                        _message = _message.Replace("{Name}", _itemName);
                                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                                        {
                                                            sw.WriteLine(string.Format("{0}: {1} has added {2} {3}, {4} quality to the auction for {5} {6}.", DateTime.Now, _cInfo.playerName, item.count, _itemName, item.itemValue.Quality, _price, Wallet.Coin_Name));
                                                            sw.WriteLine();
                                                            sw.Flush();
                                                            sw.Close();
                                                        }
                                                        count++;
                                                    }
                                                }
                                                slotNumber++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you need to input a price greater than zero. This is not a transfer system.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your sell price must be an integer and greater than zero.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            _result.Dispose();
        }

        public static void AuctionList(ClientInfo _cInfo)
        {
            string _sql = "SELECT * FROM Auction";
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                foreach (DataRow row in _result.Rows)
                {
                    int _id;
                    int.TryParse(row[0].ToString(), out _id);
                    string _itemName = row[2].ToString();
                    int _itemCount;
                    int.TryParse(row[3].ToString(), out _itemCount);
                    int _itemQuality;
                    int.TryParse(row[4].ToString(), out _itemQuality);
                    int _itemPrice;
                    int.TryParse(row[5].ToString(), out _itemPrice);

                    if (_itemQuality > 1)
                    {
                        string _message = "# {Id}: {Count} {Item} at {Quality} quality, for {Price} {Name}";
                         _message = _message.Replace("{Id}", _id.ToString());
                        _message = _message.Replace("{Count}", _itemCount.ToString());
                        _message = _message.Replace("{Item}", _itemName);
                        _message = _message.Replace("{Quality}", _itemQuality.ToString());
                        _message = _message.Replace("{Price}", _itemPrice.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _message = "# {Id}: {Count} {Item} for {Price} {Name}";
                        _message = _message.Replace("{Id}", _id.ToString());
                        _message = _message.Replace("{Count}", _itemCount.ToString());
                        _message = _message.Replace("{Item}", _itemName);
                        _message = _message.Replace("{Price}", _itemPrice.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", no items are currently for sale.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            _result.Dispose();
        }

        public static void WalletCheck(ClientInfo _cInfo, int _purchase)
        {
            string _sql = string.Format("SELECT itemPrice FROM Auction WHERE auctionid = {0}", _purchase);
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                int _coinValue;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _coinValue);
                int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                if (_currentCoins >= _coinValue)
                {
                    BuyAuction(_cInfo, _purchase);
                }
                else
                {
                    int _missing;
                    if (_currentCoins >= 0)
                    {
                        _missing = _coinValue - _currentCoins;
                    }
                    else
                    {
                        _missing = _coinValue + _currentCoins;
                    }
                    string _message = " you can not make this purchase. You need {Value} more {Name}.";
                    _message = _message.Replace("{Value}", _missing.ToString());
                    _message = _message.Replace("{Name}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", this # could not be found. Please check the auction list by typing " + ChatHook.Command_Private + Command71 + ".[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            _result.Dispose();
        }

        public static void BuyAuction(ClientInfo _cInfo, int _purchase)
        {
            string _sql = string.Format("SELECT * FROM Auction WHERE auctionid = {0}", _purchase);
            DataTable _result = SQL.TQuery(_sql);
            string _steamid = _result.Rows[0].ItemArray.GetValue(1).ToString();
            string _itemName = _result.Rows[0].ItemArray.GetValue(2).ToString();
            int _itemCount;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(3).ToString(), out _itemCount);
            int _quality;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(4).ToString(), out _quality);
            int _itemPrice;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(5).ToString(), out _itemPrice);
            ItemValue itemValue = ItemClass.GetItem(_itemName, false);
            if (itemValue.type == ItemValue.None.type)
            {
                Log.Out(string.Format("Could not find itemValue for {0}", _itemName));
                return;
            }
            else
            {
                itemValue = new ItemValue(ItemClass.GetItem(_itemName).type, _quality, _quality, false, default(FastTags), 1);
            }
            World world = GameManager.Instance.World;
            var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
            {
                entityClass = EntityClass.FromString("item"),
                id = EntityFactory.nextEntityID++,
                itemStack = new ItemStack(itemValue, _itemCount),
                pos = world.Players.dict[_cInfo.entityId].position,
                rot = new Vector3(20f, 0f, 20f),
                lifetime = 60f,
                belongsPlayerId = _cInfo.entityId
            });
            world.SpawnEntityInWorld(entityItem);
            _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
            world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _itemPrice);
            string _message = " you have purchased {Count} {ItemName} from the auction for {Value} {CoinName}.";
            _message = _message.Replace("{Count}", _itemCount.ToString());
            _message = _message.Replace("{ItemName}", _itemName);
            _message = _message.Replace("{Value}", _itemPrice.ToString());
            _message = _message.Replace("{CoinName}", Wallet.Coin_Name);
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            double _percent = _itemPrice * 0.05;
            int _newCoin2 = _itemPrice - (int)_percent;
            _sql = string.Format("DELETE FROM Auction WHERE auctionid = {0}", _purchase);
            SQL.FastQuery(_sql, "AuctionBox");
            Wallet.AddCoinsToWallet(_steamid, _newCoin2);
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", seller has received the funds in their wallet.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            ClientInfo _cInfo1 = ConnectionManager.Instance.Clients.ForPlayerId(_steamid);
            if (_cInfo1 != null)
            {
                ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _cInfo1.playerName + LoadConfig.Chat_Response_Color + ", your auction item was purchased and the value placed in your wallet.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            using (StreamWriter sw = new StreamWriter(filepath, true))
            {
                sw.WriteLine(string.Format("{0}: {1} has purchased auction entry {2}, profits went to steam id {3}", DateTime.Now, _cInfo.playerName, _purchase, _steamid));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }

        public static void CancelAuction(ClientInfo _cInfo)
        {
            string _sql = string.Format("SELECT * FROM Auction WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                DateTime _cancelTime;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(6).ToString(), out _cancelTime);
                TimeSpan varTime = DateTime.Now - _cancelTime;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (_timepassed >= 15)
                {
                    string _itemName = _result.Rows[0].ItemArray.GetValue(2).ToString();
                    ItemValue itemValue = ItemClass.GetItem(_itemName, false);
                    if (itemValue.type == ItemValue.None.type)
                    {
                        Log.Out(string.Format("Could not find itemValue {0}", itemValue));
                        return;
                    }
                    else
                    {
                        int _quality;
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(4).ToString(), out _quality);
                        itemValue = new ItemValue(ItemClass.GetItem(_itemName).type, _quality, _quality, false, default(FastTags), 1);
                    }
                    int _itemCount;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(3).ToString(), out _itemCount);
                    World world = GameManager.Instance.World;
                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                    {
                        entityClass = EntityClass.FromString("item"),
                        id = EntityFactory.nextEntityID++,
                        itemStack = new ItemStack(itemValue, _itemCount),
                        pos = world.Players.dict[_cInfo.entityId].position,
                        rot = new Vector3(20f, 0f, 20f),
                        lifetime = 60f,
                        belongsPlayerId = _cInfo.entityId
                    });
                    world.SpawnEntityInWorld(entityItem);
                    _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                    _sql = string.Format("DELETE FROM Auction WHERE steamid = '{0}'", _cInfo.playerId);
                    SQL.FastQuery(_sql, "AuctionBox");
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your auction item has returned to you.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    int _timeleft = 15 - _timepassed;
                    string _message = " you must wait 15 minutes before you can cancel your auction item. Be careful what you sell. Time remaining: {Time} minutes.[-]";
                    _message = _message.Replace("{Time}", _timeleft.ToString());
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Log.Out("No auction item found for this player");
            }
            _result.Dispose();
        }
    }
}

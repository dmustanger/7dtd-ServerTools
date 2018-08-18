using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

namespace ServerTools
{
    class AuctionBox
    {
        public static bool IsEnabled = false, IsRunning = false, No_Admins = false;
        public static int Delay_Between_Uses = 24, Admin_Level = 0;
        private static DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
        private static List<Chunk> chunkArray = new List<Chunk>();
        private static string file = string.Format("Auction_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/AuctionLog/{1}", API.GamePath, file);

        public static void CreateFolder()
        {
            if (!Directory.Exists(API.GamePath + "/AuctionLog"))
            {
                Directory.CreateDirectory(API.GamePath + "/AuctionLog");
            }
        }

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
                                            _phrase900 = "{PlayerName} you can only use /auction sell {DelayBetweenUses} hours after a sale. Time remaining: {TimeRemaining} hours.";
                                        }
                                        _phrase900 = _phrase900.Replace("{PlayerName}", _cInfo.playerName);
                                        _phrase900 = _phrase900.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                        _phrase900 = _phrase900.Replace("{TimeRemaining}", _timeleft.ToString());
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase900), Config.Server_Response_Name, false, "ServerTools", false));
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
                                    _phrase900 = "{PlayerName} you can only use /auction sell {DelayBetweenUses} hours after a sale. Time remaining: {TimeRemaining} hours.";
                                }
                                _phrase900 = _phrase900.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase900 = _phrase900.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                _phrase900 = _phrase900.Replace("{TimeRemaining}", _timeleft.ToString());
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase900), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have auction item # {2} in the auction already. Wait for it to sell or cancel it with /auction cancel.[-]", Config.Chat_Response_Color, _cInfo.playerName, _auctionid), Config.Server_Response_Name, false, "ServerTools", false));
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
                            for (int j = 0; j < chunkArray.Count; j++)
                            {
                                Chunk _c = chunkArray[j];
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
                                                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                                                for (int k = 0; k < _cInfoList.Count; k++)
                                                {
                                                    ClientInfo _cInfo2 = _cInfoList[k];
                                                    if (_cInfo != _cInfo2)
                                                    {
                                                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                                                        if ((vec3i.x - _player2.position.x) * (vec3i.x - _player2.position.x) + (vec3i.z - _player2.position.z) * (vec3i.z - _player2.position.z) <= 8 * 8)
                                                        {
                                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you are too close to another player to use auction. Tell them to back off and get their own moldy sandwich.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                            List<string> boxUsers = SecureLoot.GetUsers();
                                            if (!boxUsers.Contains(_cInfo.playerId) && !SecureLoot.GetOwner().Equals(_cInfo.playerId))
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the local secure loot is not owned by you or a friend. You can only auction an item through a secure loot you own.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
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
                                                        _sql = string.Format("INSERT INTO Auction (steamid, itemName, itemCount, itemQuality, itemPrice, cancelTime) VALUES ('{0}', '{1}', {2}, {3}, {4}, '{5}')", _cInfo.playerId, _itemName, item.count, item.itemValue.Quality, _price, DateTime.Now);
                                                        SQL.FastQuery(_sql);
                                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your auction item {2} has been removed from the secure loot and added to the auction.[-]", Config.Chat_Response_Color, _cInfo.playerName, _itemName), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you need to input a price greater than zero. This is not a transfer system.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your sell price must be an integer and greater than zero.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}# {1}: {2} {3} at {4} quality, for {5} {6}[-]", Config.Chat_Response_Color, _id, _itemCount, _itemName, _itemQuality, _itemPrice, Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}# {1}: {2} {3} for {4} {5}[-]", Config.Chat_Response_Color, _id, _itemCount, _itemName, _itemPrice, Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} no items are currently for sale.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you can not make this purchase. You need {2} more {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _missing, Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} this # could not be found. Please check the auction list by typing /auction.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
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
            ItemValue itemValue = ItemClass.GetItem(_itemName, true);
            if (itemValue.type == ItemValue.None.type)
            {
                Log.Out(string.Format("Could not find itemValue for {0}", _itemName));
                return;
            }
            else
            {
                itemValue = new ItemValue(ItemClass.GetItem(_itemName).type, _quality, _quality, true);
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
            _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result1 = SQL.TQuery(_sql);
            int _playerSpentCoins;
            int.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
            _result1.Dispose();
            _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins - _itemPrice, _cInfo.playerId);
            SQL.FastQuery(_sql);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have purchased {2} {3} from the auction for {4} {5}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _itemCount, _itemName, _itemPrice, Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
            double _percent = _itemPrice * 0.05;
            int _newCoin2 = _itemPrice - (int)_percent;
            _sql = string.Format("DELETE FROM Auction WHERE auctionid = {0}", _purchase);
            SQL.FastQuery(_sql);
            _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _steamid);
            DataTable _result2 = SQL.TQuery(_sql);
            int _playerSpentCoins1;
            int.TryParse(_result2.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins1);
            _result2.Dispose();
            _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins1 + _newCoin2, _steamid);
            SQL.FastQuery(_sql);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} seller has received the funds in their wallet.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForPlayerId(_steamid);
            if (_cInfo1 != null)
            {
                _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your auction item was purchased and the value placed in your wallet.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
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
                    ItemValue itemValue = ItemClass.GetItem(_itemName, true);
                    if (itemValue.type == ItemValue.None.type)
                    {
                        Log.Out(string.Format("Could not find itemValue {0}", itemValue));
                        return;
                    }
                    else
                    {
                        int _quality;
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(4).ToString(), out _quality);
                        itemValue = new ItemValue(ItemClass.GetItem(_itemName).type, _quality, _quality, true);
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
                    SQL.FastQuery(_sql);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your auction item has returned to you.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you must wait 15 minutes before you can cancel your auction item. Be careful what you sell. Time remaining: {2} minutes[-]", Config.Chat_Response_Color, _cInfo.playerName, _timeleft), Config.Server_Response_Name, false, "ServerTools", false));
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

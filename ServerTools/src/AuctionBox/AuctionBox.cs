using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class AuctionBox
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Delay_Between_Uses = 24;
        public static Dictionary<int, string[]> AuctionItems = new Dictionary<int, string[]>();
        private static DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
        private static List<Chunk> chunkArray = new List<Chunk>();

        public static void Delay(ClientInfo _cInfo, string _price)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                CheckBox(_cInfo, _price);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.SellDate == null)
                {
                    CheckBox(_cInfo, _price);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.SellDate;
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
                            string _phrase810;
                            if (!Phrases.Dict.TryGetValue(810, out _phrase810))
                            {
                                _phrase810 = "{PlayerName} you can only use /auction sell {DelayBetweenUses} hours after a sale. Time remaining: {TimeRemaining} hours.";
                            }
                            _phrase810 = _phrase810.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase810 = _phrase810.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase810 = _phrase810.Replace("{TimeRemaining}", _timeleft.ToString());
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase810), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
            }
        }

        public static void CheckBox(ClientInfo _cInfo, string _price)
        {
            int _p;
            if (int.TryParse(_price, out _p))
            {
                if (_p > 0)
                {
                    if (!AuctionItems.ContainsKey(_cInfo.entityId))
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
                                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You are too close to another player to use auction. Tell them to back off and get their own moldy sandwich.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                            List<string> boxUsers = SecureLoot.GetUsers();
                                            if (!boxUsers.Contains(_cInfo.playerId) && !SecureLoot.GetOwner().Equals(_cInfo.playerId))
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Local secure loot is not owned by you or a friend. You can only auction an item through a secure loot you own.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                                                        string[] _auctionItem = { item.count.ToString(), _itemName, item.itemValue.Quality.ToString(), _price };
                                                        SecureLoot.UpdateSlot(slotNumber, ItemStack.Empty);
                                                        AuctionItems.Add(_cInfo.entityId, _auctionItem);
                                                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CancelTime = DateTime.Now;
                                                        PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionData = _cInfo.entityId;
                                                        PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionItem = _auctionItem;
                                                        PersistentContainer.Instance.Save();
                                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your auction item {1} has been removed from the secure loot and added to the auction.[-]", Config.Chat_Response_Color, _itemName), Config.Server_Response_Name, false, "ServerTools", false));
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
                        if (!AuctionItems.ContainsKey(_cInfo.entityId))
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Auction sell failed. No items were found in a secure chest you own near by.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Place an item in a chest and stand very close to it, then use /auction sell #.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have auction item # {1} in the auction already. Wait for it to sell or cancel it with /auction cancel.[-]", Config.Chat_Response_Color, _cInfo.entityId), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You need to input a price greater than zero. This is not a transfer system.[-]", Config.Chat_Response_Color, _cInfo.entityId), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your sell price must be an interger and greater than zero.[-]", Config.Chat_Response_Color, _cInfo.entityId), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void AuctionList()
        {
            List<string> playerlist = PersistentContainer.Instance.Players.SteamIDs;
            for (int i = 0; i < playerlist.Count; i++)
            {
                string _steamId = playerlist[i];
                Player p = PersistentContainer.Instance.Players[_steamId, false];
                if (p != null)
                {
                    if (p.AuctionItem != null)
                    {
                        AuctionItems.Add(p.AuctionData, p.AuctionItem);
                    }
                }
            }
        }

        public static void AuctionList(ClientInfo _cInfo)
        {
            if (AuctionItems.Count > 0)
            {
                foreach (var _auctionItem in AuctionItems)
                {
                    int _quality;
                    int.TryParse(_auctionItem.Value[2], out _quality);
                    if (_quality > 1)
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}# {1}: {2} {3} at {4} quality, for {5} {6}[-]", Config.Chat_Response_Color, _auctionItem.Key, _auctionItem.Value[0], _auctionItem.Value[1], _auctionItem.Value[2], _auctionItem.Value[3], Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}# {1}: {2} {3} for {4} {5}[-]", Config.Chat_Response_Color, _auctionItem.Key, _auctionItem.Value[0], _auctionItem.Value[1], _auctionItem.Value[3], Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}No items are currently for sale.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void WalletCheck(ClientInfo _cInfo, int _purchase)
        {
            if (AuctionItems.ContainsKey(_purchase))
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p != null)
                {
                    int spentCoins = p.PlayerSpentCoins;
                    int currentCoins = 0;
                    int gameMode = world.GetGameMode();
                    if (gameMode == 7)
                    {
                        currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                        if (!Wallet.Negative_Wallet)
                        {
                            if (currentCoins < 0)
                            {
                                currentCoins = 0;
                            }
                        }
                    }
                    else
                    {
                        currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                        if (!Wallet.Negative_Wallet)
                        {
                            if (currentCoins < 0)
                            {
                                currentCoins = 0;
                            }
                        }
                    }
                    List<string> playerlist = PersistentContainer.Instance.Players.SteamIDs;
                    for (int i = 0; i < playerlist.Count; i++)
                    {
                        string _steamId = playerlist[i];
                        Player _seller = PersistentContainer.Instance.Players[_steamId, false];
                        if (_seller.AuctionData == _purchase)
                        {
                            string[] _value;
                            if (AuctionItems.TryGetValue(_purchase, out _value))
                            {
                                int _coinValue;
                                int.TryParse(_value[3], out _coinValue);
                                if (currentCoins >= _coinValue)
                                {
                                    BuyAuction(_cInfo, _purchase, _value, _steamId);
                                }
                                else
                                {
                                    int _missing;
                                    if (currentCoins >= 0)
                                    {
                                        _missing = _coinValue - currentCoins;
                                    }
                                    else
                                    {
                                        _missing = _coinValue + currentCoins;
                                    }
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You can not make this purchase. You need {1} more {2}.[-]", Config.Chat_Response_Color, _missing, Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                        }
                    }
                }
                else
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = 0;
                    PersistentContainer.Instance.Save();
                    WalletCheck(_cInfo, _purchase);
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}This # could not be found. Please check the auction list by typing /auction.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void BuyAuction(ClientInfo _cInfo, int _purchase, string[] _value, string _steamId)
        {
            int _quality;
            int.TryParse(_value[2], out _quality);
            ItemValue itemValue = ItemClass.GetItem(_value[1], true);
            if (itemValue.type == ItemValue.None.type)
            {
                Log.Out(string.Format("Could not find itemValue {0}", itemValue));
                return;
            }
            else
            {
                itemValue = new ItemValue(ItemClass.GetItem(_value[1]).type, _quality, _quality, true);
            }
            int _count;
            int.TryParse(_value[0], out _count);
            World world = GameManager.Instance.World;
            var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
            {
                entityClass = EntityClass.FromString("item"),
                id = EntityFactory.nextEntityID++,
                itemStack = new ItemStack(itemValue, _count),
                pos = world.Players.dict[_cInfo.entityId].position,
                rot = new Vector3(20f, 0f, 20f),
                lifetime = 60f,
                belongsPlayerId = _cInfo.entityId
            });
            world.SpawnEntityInWorld(entityItem);
            _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
            world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);

            int _price;
            int.TryParse(_value[3], out _price);
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            int _newCoin = p.PlayerSpentCoins - _price;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _newCoin;
            PersistentContainer.Instance.Save();
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have purchased {1} {2} from the auction for {3} {4}.[-]", Config.Chat_Response_Color, _value[0], _value[1], _value[3], Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
            PersistentContainer.Instance.Players[_steamId, true].SellDate = DateTime.Now;
            PersistentContainer.Instance.Players[_steamId, true].PlayerSpentCoins = PersistentContainer.Instance.Players[_steamId, false].PlayerSpentCoins + (_price - (_price % 5));
            PersistentContainer.Instance.Players[_steamId, true].AuctionData = 0;
            PersistentContainer.Instance.Players[_steamId, true].AuctionItem = null;
            PersistentContainer.Instance.Save();
            AuctionItems.Remove(_purchase);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Seller has received the funds in their wallet.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForPlayerId(_steamId);
            if (_cInfo1 != null)
            {
                _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your auction item was purchased and the value placed in your wallet.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void CancelAuction(ClientInfo _cInfo)
        {
            if (AuctionItems.ContainsKey(_cInfo.entityId))
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p != null)
                {
                    TimeSpan varTime = DateTime.Now - p.CancelTime;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed >= 15)
                    {
                        string[] _value;
                        if (AuctionItems.TryGetValue(_cInfo.entityId, out _value))
                        {
                            ItemValue itemValue = ItemClass.GetItem(_value[1], true);
                            if (itemValue.type == ItemValue.None.type)
                            {
                                Log.Out(string.Format("Could not find itemValue {0}", itemValue));
                                return;
                            }
                            else
                            {
                                int _quality;
                                int.TryParse(_value[2], out _quality);
                                itemValue = new ItemValue(ItemClass.GetItem(_value[1]).type, _quality, _quality, true);
                            }
                            int _count;
                            int.TryParse(_value[0], out _count);
                            World world = GameManager.Instance.World;
                            var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                            {
                                entityClass = EntityClass.FromString("item"),
                                id = EntityFactory.nextEntityID++,
                                itemStack = new ItemStack(itemValue, _count),
                                pos = world.Players.dict[_cInfo.entityId].position,
                                rot = new Vector3(20f, 0f, 20f),
                                lifetime = 60f,
                                belongsPlayerId = _cInfo.entityId
                            });
                            world.SpawnEntityInWorld(entityItem);
                            _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                            world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                            AuctionItems.Remove(_cInfo.entityId);
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionData = 0;
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionItem = null;
                            PersistentContainer.Instance.Save();
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your auction item has returned to you.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        int _timeleft = 15 - _timepassed;
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You must wait 15 minutes before you can cancel your auction item. Be careful what you sell. Time remaining: {1} minutes[-]", Config.Chat_Response_Color, _timeleft), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            else
            {
                Log.Out("No auction item found for this player");
            }
        }
    }
}

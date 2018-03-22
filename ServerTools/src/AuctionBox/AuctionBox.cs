using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class AuctionBox
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static SortedDictionary<int, string[]> AuctionItems = new SortedDictionary<int, string[]>();
        private static DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
        private static List<Chunk> chunkArray = new List<Chunk>();

        public static void CheckBox(ClientInfo _cInfo, string _price)
        {
            if (!AuctionItems.ContainsKey(_cInfo.entityId))
            {
                float _distance = 0;
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
                                Vector3 _vec3 = vec3i.ToVector3();
                                _distance = _player.GetDistanceSq(_vec3);
                                if (_distance <= 1.3)
                                {
                                    int _playerCount = ConnectionManager.Instance.ClientCount();
                                    if (_playerCount > 1)
                                    {
                                        float _distancePlayer;
                                        List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                                        for (int j = 0; i < _cInfoList.Count; j++)
                                        {
                                            ClientInfo _cInfo2 = _cInfoList[j];
                                            if (_cInfo != _cInfo2)
                                            {
                                                EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                                                Vector3 _player2pos = _player2.position;
                                                _distancePlayer = _player.GetDistanceSq(_player2pos);
                                                if (_distancePlayer < 8)
                                                {
                                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You are too close to another player to use auction. Tell them to back off and get their own moldy sandwich.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                    List<string> boxUsers = SecureLoot.GetUsers();
                                    if (!boxUsers.Contains(_cInfo.playerId) && !SecureLoot.GetOwner().Equals(_cInfo.playerId))
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Local secure loot is not owned by you or a friend. You can only auction an item through a secure loot you own.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                                        return;
                                    }
                                    else if (SecureLoot.IsUserAccessing())
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Local box is currently open. Close the chest first.[-]", Config.Chat_Response_Color), "Server", false, "", false));
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
                                                PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionData = _cInfo.entityId;
                                                PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionItem = _auctionItem;
                                                PersistentContainer.Instance.Save();
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your auction item {1} has been removed from the secure loot and added to the auction.[-]", Config.Chat_Response_Color, _itemName), "Server", false, "", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Auction add failed. No items were found in a secure chest you own near by.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Place an item in a chest and stand very close to it, then use /auction sell #.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have auction item # {1} in the auction already. Wait for it to sell or cancel it with /auction cancel.[-]", Config.Chat_Response_Color, _cInfo.entityId), "Server", false, "", false));
            }
        }

        public static void BuildAuctionList()
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
            Log.Out("---------------------------------");
            Log.Out("[SERVERTOOLS] Auction list loaded");
            Log.Out("---------------------------------");
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}# {1}: {2} {3} at {4} quality, for {5} {6}[-]", Config.Chat_Response_Color, _auctionItem.Key, _auctionItem.Value[0], _auctionItem.Value[1], _auctionItem.Value[2], _auctionItem.Value[3], Wallet.Coin_Name), "Server", false, "", false));
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}# {1}: {2} {3} for {4} {5}[-]", Config.Chat_Response_Color, _auctionItem.Key, _auctionItem.Value[0], _auctionItem.Value[1], _auctionItem.Value[3], Wallet.Coin_Name), "Server", false, "", false));
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}No items are currently for sale.[-]", Config.Chat_Response_Color), "Server", false, "", false));
            }
        }

        public static void WalletCheck(ClientInfo _cInfo, int _purchase)
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
                }
                else
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                if (!Wallet.Negative_Wallet)
                {
                    if (currentCoins < 0)
                    {
                        currentCoins = 0;
                    }
                }
                string[] _value;
                List<string> playerlist = PersistentContainer.Instance.Players.SteamIDs;
                int _count = 0;
                int _entId = 0;
                foreach (string _steamId in playerlist)
                {
                    Player pData = PersistentContainer.Instance.Players[_steamId, false];
                    if (pData.AuctionData != 0)
                    {
                        _entId = pData.AuctionData;
                        if (AuctionItems.TryGetValue(_entId, out _value))
                        {
                            int _coinValue;
                            int.TryParse(_value[3], out _coinValue);
                            if (currentCoins >= _coinValue)
                            {
                                BuyAuction(_cInfo, _purchase, _value);
                            }
                            else
                            {
                                int _missing = 0;
                                if (currentCoins >= 0)
                                {
                                    _missing = _coinValue - currentCoins;
                                }
                                else
                                {
                                    _missing = _coinValue + currentCoins;
                                }
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You can not make this purchase. You need {1} more {2}.[-]", Config.Chat_Response_Color, _missing, Wallet.Coin_Name), "Server", false, "", false));
                            }
                        }
                        else
                        {
                            _count++;
                            if (_count == playerlist.Count)
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}This # could not be found. Please check the auction list by typing /auction.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                            }
                            else
                            {
                                continue;
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

        public static void BuyAuction(ClientInfo _cInfo, int _purchase, string[] _value)
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
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have purchased {1} {2} from the auction for {3} {4}.[-]", Config.Chat_Response_Color, _value[0], _value[1], _value[3], Wallet.Coin_Name), "Server", false, "", false));

            int count = 0;
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            int _infoCount = _cInfoList.Count;
            for (int i = 0; i < _infoCount; i++)
            {
                ClientInfo _cInfo1 = _cInfoList[i];
                Player _p = PersistentContainer.Instance.Players[_cInfo1.playerId, false];
                _newCoin = _p.PlayerSpentCoins + _price;
                if (_cInfo1.entityId == _purchase)
                {
                    PersistentContainer.Instance.Players[_cInfo1.playerId, true].PlayerSpentCoins = _newCoin;
                    PersistentContainer.Instance.Players[_cInfo1.playerId, true].AuctionData = 0;
                    PersistentContainer.Instance.Players[_cInfo1.playerId, true].AuctionItem = null;                    
                    PersistentContainer.Instance.Save();
                    AuctionItems.Remove(_cInfo1.entityId);
                    _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your auction item was purchased and the value placed in your wallet.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Seller has received the funds in their wallet.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                }
                else
                {
                    count++;
                }
                if (count == _infoCount)
                {
                    PersistentContainer.Instance.Players[_cInfo1.playerId, true].PlayerSpentCoins = _newCoin;                    
                    PersistentContainer.Instance.Players[_cInfo1.playerId, true].AuctionData = 0;
                    PersistentContainer.Instance.Players[_cInfo1.playerId, true].AuctionItem = null;
                    PersistentContainer.Instance.Save();
                    AuctionItems.Remove(_cInfo1.entityId);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Seller has received the funds in their wallet.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                }
            }
            
        }

        public static void CancelAuction(ClientInfo _cInfo)
        {
            if (AuctionItems.ContainsKey(_cInfo.entityId))
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
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionData = 0;
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionItem = null;
                    PersistentContainer.Instance.Save();
                    AuctionItems.Remove(_cInfo.entityId);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your auction item has returned to you.[-]", Config.Chat_Response_Color), "Server", false, "", false));                   
                }
            }
            else
            {
                Log.Out("No auction item found for this player");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Bank
    {
        public static bool IsEnabled = false;
        public static string Ingame_Coin = "casinoCoin";
        public static int Limit = 50000;
        public static Dictionary<string, int> TransferId = new Dictionary<string, int>();
        private static DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
        private static List<Chunk> chunkArray = new List<Chunk>();
        private static string file = string.Format("Bank_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Banking/{1}", API.GamePath, file);
        private static System.Random random = new System.Random();

        public static void Check(ClientInfo _cInfo)
        {
            string _sql = string.Format("SELECT bank FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            int _bank;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bank);
            _result.Dispose();
            if (TransferId.ContainsKey(_cInfo.playerId))
            {
                int _id;
                TransferId.TryGetValue(_cInfo.playerId, out _id);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your bank account is worth {2}. Transfer Id is {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _bank, _id), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                AddId(_cInfo);
            }
        }

        public static void AddId(ClientInfo _cInfo)
        {
            int _rndId = random.Next(1000, 5001);
            TransferId.Add(_cInfo.playerId, _rndId);
            string _sql = string.Format("SELECT bank FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            int _bank;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bank);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your bank account is worth {2}. Transfer Id is {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _bank, _rndId), Config.Server_Response_Name, false, "ServerTools", false));
            _result.Dispose();
        }

        public static void CreateFolder()
        {
            if (!Directory.Exists(API.GamePath + "/Banking"))
            {
                Directory.CreateDirectory(API.GamePath + "/Banking");
            }
        }

        public static void CheckLocation(ClientInfo _cInfo, string _amount, int _exec)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = world.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.GetPosition();
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            Vector3i _vec3i = new Vector3i(x, y, z);
            PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
            PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(_player.entityId);
            EnumLandClaimOwner _owner = world.GetLandClaimOwner(_vec3i, _persistentPlayerData);
            if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
            {
                if (_exec == 1)
                {
                    Deposit(_cInfo, _amount);
                }
                if (_exec == 2)
                {
                    Withdraw(_cInfo, _amount);
                }
                if (_exec == 3)
                {
                    WalletDeposit(_cInfo, _amount);
                }
                if (_exec == 4)
                {
                    WalletWithdraw(_cInfo, _amount);
                }

            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You can not use this command here. Stand in your own or a friend's claimed space.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Deposit(ClientInfo _cInfo, string _amount)
        {
            bool Found = false;
            int _coinAmount;
            if (int.TryParse(_amount, out _coinAmount))
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
                                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you are too close to another player to deposit from your chest in to the bank.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                    List<string> boxUsers = SecureLoot.GetUsers();
                                    if (!boxUsers.Contains(_cInfo.playerId) && !SecureLoot.GetOwner().Equals(_cInfo.playerId))
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the local secure loot is not owned by you. You can only deposit to the bank through a secure loot you own.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                        return;
                                    }
                                    ItemStack[] items = SecureLoot.items;
                                    int slotNumber = 0;
                                    foreach (ItemStack item in items)
                                    {
                                        if (!item.IsEmpty())
                                        {
                                            ItemClass _itemClass = ItemClass.list[item.itemValue.type];
                                            string _itemName = _itemClass.GetItemName();
                                            if (_itemName == Ingame_Coin)
                                            {
                                                if (item.count >= _coinAmount)
                                                {
                                                    string _sql = string.Format("SELECT bank FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                                    DataTable _result = SQL.TQuery(_sql);
                                                    int _bank;
                                                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bank);
                                                    _result.Dispose();
                                                    double _percent = _coinAmount * 0.05;
                                                    int _newCoin = _coinAmount - (int)_percent;
                                                    double _newLimit = Limit + (Limit * 0.05);
                                                    if (_bank + _coinAmount <= (int)_newLimit)
                                                    {
                                                        Found = true;
                                                        int _newCount = item.count - _coinAmount;
                                                        ItemValue _itemValue = ItemClass.GetItem(Ingame_Coin, true);
                                                        if (_itemValue.type == ItemValue.None.type)
                                                        {
                                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the bank coin is not setup correctly, contact the server Admin.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                                            Log.Out(string.Format("[SERVERTOOLS] Unable to find item {0}", Ingame_Coin));
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            _itemValue = new ItemValue(ItemClass.GetItem(Ingame_Coin).type, 1, 1, true);
                                                        }
                                                        ItemStack itemStack = new ItemStack(_itemValue, _newCount);
                                                        SecureLoot.UpdateSlot(slotNumber, itemStack);
                                                        _sql = string.Format("UPDATE Players SET bank = {0} WHERE steamid = '{1}'", _bank + _newCoin, _cInfo.playerId);
                                                        SQL.FastQuery(_sql);
                                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                                        {
                                                            sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account.", DateTime.Now, _cInfo.playerName, _newCoin));
                                                            sw.WriteLine();
                                                            sw.Flush();
                                                            sw.Close();
                                                        }
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your bank can not hold this much. The bank can hold {2} total. You currently have {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, Limit, _bank), Config.Server_Response_Name, false, "ServerTools", false));
                                                    }
                                                }
                                                else
                                                {
                                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} there is not enough {2} in the secure loot to deposit this value.[-]", Config.Chat_Response_Color, _cInfo.playerName, Ingame_Coin), Config.Server_Response_Name, false, "ServerTools", false));
                                                }
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you input an invalid integer. Type /deposit #.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
            if (Found)
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} deposited {2} into your bank account from the secure loot. 5% fee was applied.[-]", Config.Chat_Response_Color, _cInfo.playerName, Ingame_Coin), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you don't have enough in the secure loot to deposit that much into your bank.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Withdraw(ClientInfo _cInfo, string _amount)
        {
            int _coinAmount;
            if (int.TryParse(_amount, out _coinAmount))
            {
                string _sql = string.Format("SELECT bank FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                int _bank;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bank);
                _result.Dispose();
                if (_bank >= _coinAmount)
                {
                    ItemValue _itemValue = ItemClass.GetItem(Ingame_Coin, true);
                    if (_itemValue.type == ItemValue.None.type)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Unable to find item {0}", Ingame_Coin));
                        return;
                    }
                    else
                    {
                        _itemValue = new ItemValue(ItemClass.GetItem(Ingame_Coin).type, 1, 1, true);
                    }
                    int _maxAllowed = ItemClass.list[_itemValue.type].Stacknumber.Value;
                    if (_coinAmount <= _maxAllowed)
                    {
                        World world = GameManager.Instance.World;
                        if (world.Players.dict[_cInfo.entityId].IsSpawned())
                        {
                            var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                            {
                                entityClass = EntityClass.FromString("item"),
                                id = EntityFactory.nextEntityID++,
                                itemStack = new ItemStack(_itemValue, _coinAmount),
                                pos = world.Players.dict[_cInfo.entityId].position,
                                rot = new Vector3(20f, 0f, 20f),
                                lifetime = 60f,
                                belongsPlayerId = _cInfo.entityId
                            });
                            world.SpawnEntityInWorld(entityItem);

                            _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                            world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                            _sql = string.Format("UPDATE Players SET bank = {0} WHERE steamid = '{1}'", _bank - _coinAmount, _cInfo.playerId);
                            SQL.FastQuery(_sql);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have received your {2}. If your inventory is full, check the ground.[-]", Config.Chat_Response_Color, _cInfo.playerName, Ingame_Coin), Config.Server_Response_Name, false, "ServerTools", false));
                            using (StreamWriter sw = new StreamWriter(filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}: {1} has removed {2} from their bank account as {3}.", DateTime.Now, _cInfo.playerName, _coinAmount, Ingame_Coin));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                        }
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you can only withdraw a full stack at a time. The maximum stack size is {2}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _maxAllowed), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your bank account does not have enough to withdraw that value.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you input an invalid integer. Type /withdraw #.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void WalletDeposit(ClientInfo _cInfo, string _amount)
        {
            int _coinAmount;
            if (int.TryParse(_amount, out _coinAmount))
            {
                int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                if (_currentCoins >= _coinAmount)
                {
                    string _sql = string.Format("SELECT bank, playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    int _bank;
                    int _playerSpentCoins;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bank);
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _playerSpentCoins);
                    _result.Dispose();
                    double _percent = _coinAmount * 0.05;
                    int _newCoin = _coinAmount - (int)_percent;
                    double _newLimit = Limit + (Limit * 0.05);
                    if (_bank + _coinAmount <= (int)_newLimit)
                    {
                        _sql = string.Format("UPDATE Players SET bank = {0}, playerSpentCoins = {1} WHERE steamid = '{2}'", _bank + _newCoin, _playerSpentCoins - _coinAmount, _cInfo.playerId);
                        SQL.FastQuery(_sql);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} deposited {2} {3} from your wallet to your bank account. 5% fee was applied.[-]", Config.Chat_Response_Color, _cInfo.playerName, _coinAmount, Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                        using (StreamWriter sw = new StreamWriter(filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account from their wallet.", DateTime.Now, _cInfo.playerName, _newCoin));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your bank can not hold this much. The bank can hold {2} total. You currently have {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, Limit, _bank), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your wallet does not have enough to deposit that value.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you input an invalid integer. Type /wallet deposit #.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void WalletWithdraw(ClientInfo _cInfo, string _amount)
        {
            int _coinAmount;
            if (int.TryParse(_amount, out _coinAmount))
            {
                string _sql = string.Format("SELECT bank, playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                int _bank;
                int _playerSpentCoins;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bank);
                int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _playerSpentCoins);
                _result.Dispose();
                if (_bank >= _coinAmount)
                {
                    _sql = string.Format("UPDATE Players SET bank = {0}, playerSpentCoins = {1} WHERE steamid = '{2}'", _bank - _coinAmount, _playerSpentCoins + _coinAmount, _cInfo.playerId);
                    SQL.FastQuery(_sql);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have received your {2}. It has gone to your wallet.[-]", Config.Chat_Response_Color, _cInfo.playerName, Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                    using (StreamWriter sw = new StreamWriter(filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}: {1} has removed {2} from their bank account into their wallet.", DateTime.Now, _cInfo.playerName, _coinAmount));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your bank account does not have enough to withdraw that value.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you input an invalid integer. Type /withdraw #.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Transfer(ClientInfo _cInfo, string _transferIdAndAmount)
        {
            string[] _idAndAmount = { };
            if (_transferIdAndAmount.Contains(" "))
            {
                _idAndAmount = _transferIdAndAmount.Split(' ').ToArray();
                int _id;
                if (int.TryParse(_idAndAmount[0], out _id))
                {
                    int _amount;
                    if (int.TryParse(_idAndAmount[1], out _amount))
                    {
                        if (TransferId.ContainsValue(_id))
                        {
                            string _sql = string.Format("SELECT bank FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                            DataTable _result = SQL.TQuery(_sql);
                            int _bank;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bank);
                            _result.Dispose();
                            if (_bank >= _amount)
                            {
                                foreach (KeyValuePair<string, int> _accountInfo in TransferId)
                                {
                                    if (_accountInfo.Value == _id)
                                    {
                                        ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForPlayerId(_accountInfo.Key);
                                        if (_cInfo1 != null)
                                        {
                                            TransferId.Remove(_cInfo1.playerId);
                                            _sql = string.Format("UPDATE Players SET bank = {0} WHERE steamid = '{1}'", _bank - _amount, _cInfo.playerId);
                                            SQL.FastQuery(_sql);
                                            _sql = string.Format("SELECT bank FROM Players WHERE steamid = '{0}'", _cInfo1.playerId);
                                            DataTable _result1 = SQL.TQuery(_sql);
                                            int _bank1;
                                            int.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _bank1);
                                            _result1.Dispose();
                                            _sql = string.Format("UPDATE Players SET bank = {0} WHERE steamid = '{1}'", _bank1 + _amount, _cInfo1.playerId);
                                            SQL.FastQuery(_sql);
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have sent {2} from your bank to player {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _amount, _cInfo1.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                            _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have received {2} in to your bank from player {3}.[-]", Config.Chat_Response_Color, _cInfo1.playerName, _amount, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you do not have enough in your bank account to make this transfer.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you input an invalid transfer Id. Ask for the transfer Id from the player you want to transfer to.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you input an invalid transfer. Type /transfer Id #. Get the Id from the player you are transferring to.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you input an invalid transfer. Type /transfer Id #. Get the Id from the player you are transferring to.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you input an invalid transfer. Type /transfer Id #. Get the Id from the player you are transferring to.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}

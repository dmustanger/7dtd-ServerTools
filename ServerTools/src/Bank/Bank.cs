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
        public static bool IsEnabled = false, Inside_Claim = true;
        public static string Ingame_Coin = "casinoCoin";
        public static int Limit = 50000;
        public static Dictionary<string, int> TransferId = new Dictionary<string, int>();
        private static DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
        private static LinkedList<Chunk> chunkArray = new LinkedList<Chunk>();
        private static string file = string.Format("Bank_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/BankLogs/{1}", API.GamePath, file);
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
                string _message = "your bank account is worth {Value}. Transfer Id is {Id}.";
                _message = _message.Replace("{Value}", _bank.ToString());
                _message = _message.Replace("{Id}", _id.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
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
            _result.Dispose();
            string _message = "your bank account is worth {Value}. Transfer Id is {Id}.";
            _message = _message.Replace("{Value}", _bank.ToString());
            _message = _message.Replace("{Id}", _rndId.ToString());
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        public static void CreateFolder()
        {
            if (!Directory.Exists(API.GamePath + "/BankLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/BankLogs");
            }
        }

        public static void CheckLocation(ClientInfo _cInfo, string _amount, int _exec)
        {
            if (Inside_Claim)
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
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + " you can not use this command here.Stand in your own or a friend's claimed space.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
            else
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
                    foreach (Chunk _c in chunkArray)
                    {
                        tiles = _c.GetTileEntities();
                        foreach (TileEntity tile in tiles.dict.Values)
                        {
                            if (!Found)
                            {
                                TileEntityType type = tile.GetTileEntityType();
                                Log.Out(string.Format("tile entity type = {0}", type.ToString()));
                                if (type.ToString().Equals("SecureLoot"))
                                {
                                    TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tile;
                                    if (SecureLoot.IsUserAllowed(_cInfo.playerId))
                                    {
                                        Vector3i vec3i = SecureLoot.ToWorldPos();
                                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        if ((vec3i.x - _player.position.x) * (vec3i.x - _player.position.x) + (vec3i.z - _player.position.z) * (vec3i.z - _player.position.z) <= 1.5 * 1.5)
                                        {
                                            int _playerCount = ConnectionManager.Instance.ClientCount();
                                            if (_playerCount > 1)
                                            {
                                                List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                                                for (int k = 0; k < _cInfoList.Count; k++)
                                                {
                                                    ClientInfo _cInfo2 = _cInfoList[k];
                                                    if (_cInfo != _cInfo2)
                                                    {
                                                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                                                        if ((vec3i.x - _player2.position.x) * (vec3i.x - _player2.position.x) + (vec3i.z - _player2.position.z) * (vec3i.z - _player2.position.z) <= 8 * 8)
                                                        {
                                                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + " you are too close to another player to deposit from your chest in to the bank.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                            return;
                                                        }
                                                    }
                                                }
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
                                                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + " the bank coin is not setup correctly, contact the server Admin.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                                                return;
                                                            }
                                                            else
                                                            {
                                                                string _message = "your bank can not hold this much. The bank can hold {Limit} total. You currently have {Value}.";
                                                                _message = _message.Replace("{Limit}", Limit.ToString());
                                                                _message = _message.Replace("{Value}", _bank.ToString());
                                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            string _message = "there is not enough {Name} in the secure loot to deposit this value.";
                                                            _message = _message.Replace("{Name}", Ingame_Coin);
                                                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you input an invalid integer. Type /deposit #.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            if (Found)
            {
                string _message = "there is not enough {Value} in the secure loot to deposit this value.";
                _message = _message.Replace("{Name}", Ingame_Coin);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you do not have enough in the secure loot to deposit that much into your bank.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    ItemClass _class = ItemClass.GetItemClass(Ingame_Coin, true);
                    if (_class == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Unable to find item {0}", Ingame_Coin));
                        return;
                    }
                    ItemValue _item = new ItemValue(ItemClass.GetItem(Ingame_Coin).type, 1, 1, true);
                    int _maxAllowed = ItemClass.list[_item.type].Stacknumber.Value;
                    if (_coinAmount <= _maxAllowed)
                    {
                        World world = GameManager.Instance.World;
                        if (world.Players.dict[_cInfo.entityId].IsSpawned())
                        {
                            var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                            {
                                entityClass = EntityClass.FromString("item"),
                                id = EntityFactory.nextEntityID++,
                                itemStack = new ItemStack(_item, _coinAmount),
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
                            string _message = "you have received your {Name}. If your inventory is full, check the ground.";
                            _message = _message.Replace("{Name}", Ingame_Coin);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        string _message = "you can only withdraw a full stack at a time. The maximum stack size is {Max}.";
                        _message = _message.Replace("{Max}", _maxAllowed.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", your bank account does not have enough to withdraw that value.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you input an invalid integer. Type /withdraw #.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        string _message = "deposited {Value} {Name} from your wallet to your bank account. 5% fee was applied.";
                        _message = _message.Replace("{Value}", _coinAmount.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        string _message = "your bank can not hold this much. The bank can hold {Limit} total. You currently have {Value}.";
                        _message = _message.Replace("{Limit}", Limit.ToString());
                        _message = _message.Replace("{Value}", _bank.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", your wallet does not have enough to deposit that value.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you input an invalid integer. Type /wallet deposit #.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    string _message = "you have received your {Name}. It has gone to your wallet.";
                    _message = _message.Replace("{Name}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", your bank account does not have enough to withdraw that value.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you input an invalid integer. Type /withdraw #.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                        ClientInfo _cInfo1 = ConnectionManager.Instance.Clients.ForPlayerId(_accountInfo.Key);
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
                                            string _message = "you have sent {Value} from your bank to player {PlayerName}.";
                                            _message = _message.Replace("{Value}", _amount.ToString());
                                            _message = _message.Replace("{PlayerName}", _cInfo1.playerName);
                                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            _message = "you have received {Value} in to your bank from player {PlayerName}.";
                                            _message = _message.Replace("{Value}", _amount.ToString());
                                            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                                            ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you do not have enough in your bank account to make this transfer.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you input an invalid transfer Id. Ask for the transfer Id from the player you want to transfer to.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you input an invalid transfer. Type /transfer Id #. Get the Id from the player you are transferring to.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you input an invalid transfer. Type /transfer Id #. Get the Id from the player you are transferring to.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", you input an invalid transfer. Type /transfer Id #. Get the Id from the player you are transferring to.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

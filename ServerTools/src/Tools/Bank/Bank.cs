using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Bank
    {
        public static bool IsEnabled = false, Inside_Claim = false, Player_Transfers = false;
        public static string Ingame_Coin = "casinoCoin", Command94 = "bank", Command95 = "deposit", Command96 = "withdraw", Command97 = "wallet deposit", Command98 = "wallet withdraw", Command99 = "transfer";
        public static int Deposit_Fee_Percent = 5;
        public static Dictionary<string, int> TransferId = new Dictionary<string, int>();
        private static string file = string.Format("Bank_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/BankLogs/{1}", API.ConfigPath, file);
        private static System.Random random = new System.Random();

        public static int GetCurrentBank(string _steamid)
        {
            int _bankValue = PersistentContainer.Instance.Players[_steamid].Bank;
            if (_bankValue < 0)
            {
                PersistentContainer.Instance.Players[_steamid].Bank = 0;
                PersistentContainer.Instance.Save();
                _bankValue = 0;
            }
            return _bankValue;
        }

        public static void AddCoinsToBank(string _steamid, int _amount)
        {
            int _bankValue = PersistentContainer.Instance.Players[_steamid].Bank;
            PersistentContainer.Instance.Players[_steamid].Bank = _bankValue + _amount;
            PersistentContainer.Instance.Save();
        }

        public static void SubtractCoinsFromBank(string _steamid, int _amount)
        {
            int _newValue = PersistentContainer.Instance.Players[_steamid].Bank - _amount;
            if (_newValue < 0)
            {
                _newValue = 0;
            }
            PersistentContainer.Instance.Players[_steamid].Bank = _newValue;
            PersistentContainer.Instance.Save();
        }

        public static void ClearBank(ClientInfo _cInfo)
        {
            PersistentContainer.Instance.Players[_cInfo.playerId].Bank = 0;
            PersistentContainer.Instance.Save();
        }

        public static void CurrentBankAndId(ClientInfo _cInfo)
        {
            int _bank = GetCurrentBank(_cInfo.playerId);
            if (TransferId.ContainsKey(_cInfo.playerId))
            {
                int _id;
                TransferId.TryGetValue(_cInfo.playerId, out _id);
                string _message = "Your bank account is worth {Value}. Transfer Id is {Id}.";
                _message = _message.Replace("{Value}", _bank.ToString());
                _message = _message.Replace("{Id}", _id.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                AddId(_cInfo);
                CurrentBankAndId(_cInfo);
            }
        }

        public static void AddId(ClientInfo _cInfo)
        {
            int _tranferId = GenerateTransferId();
            if (_tranferId > 0)
            {
                TransferId.Add(_cInfo.playerId, _tranferId);
            }
        }

        private static int GenerateTransferId()
        {
            int _id = random.Next(1000, 5001);
            if (TransferId.ContainsValue(_id))
            {
                _id = random.Next(1000, 5001);
                if (TransferId.ContainsValue(_id))
                {
                    _id = random.Next(1000, 5001);
                    if (TransferId.ContainsValue(_id))
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

        public static void CheckLocation(ClientInfo _cInfo, string _amount, int _exec)
        {
            if (Inside_Claim)
            {
                if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player != null)
                    {
                        Vector3 _position = _player.GetPosition();
                        Vector3i _vec3i = new Vector3i((int)_position.x, (int)_position.y, (int)_position.z);
                        if (!PersistentOperations.ClaimedByAllyOrSelf(_cInfo.playerId, _vec3i))
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You can not use this command here. Stand in your own or a friend's claimed space.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                }
            }
            if (_exec == 1)
            {
                ChestToBankDeposit(_cInfo, _amount);
            }
            else if (_exec == 2)
            {
                BankToPlayerWithdraw(_cInfo, _amount);
            }
            else if (_exec == 3)
            {
                WalletToBankDeposit(_cInfo, _amount);
            }
            else if (_exec == 4)
            {
                BankToWalletWithdraw(_cInfo, _amount);
            }
        }

        public static void ChestToBankDeposit(ClientInfo _cInfo, string _amount)
        {
            if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    ItemValue _itemValue = new ItemValue(ItemClass.GetItem(Ingame_Coin).type, 1, 1, false, null, 1);
                    if (_itemValue != null)
                    {
                        int _coinId = _itemValue.GetItemOrBlockId();
                        if (int.TryParse(_amount, out int _value))
                        {
                            if (_value > 0)
                            {
                                int _currencyToRemove = _value;
                                int _currencyRemoved = 0;
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
                                            TileEntityType type = _tile.GetTileEntityType();
                                            if (type.ToString().Equals("SecureLoot"))
                                            {
                                                TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)_tile;
                                                if (SecureLoot.IsUserAllowed(_cInfo.playerId) && !SecureLoot.IsUserAccessing())
                                                {
                                                    Vector3i vec3i = SecureLoot.ToWorldPos();
                                                    if ((vec3i.x - _player.position.x) * (vec3i.x - _player.position.x) + (vec3i.z - _player.position.z) * (vec3i.z - _player.position.z) <= 3 * 3)
                                                    {
                                                        if (vec3i.y >= (int)_player.position.y - 3 && vec3i.y <= (int)_player.position.y + 3)
                                                        {
                                                            ItemStack[] _items = SecureLoot.items;
                                                            if (_items != null && _items.Length > 0)
                                                            {
                                                                for (int j = 0; j < _items.Length; j++)
                                                                {
                                                                    if (_currencyToRemove > 0)
                                                                    {
                                                                        if (!_items[j].IsEmpty() && _items[j].itemValue.GetItemOrBlockId() == _coinId)
                                                                        {
                                                                            if (_items[j].count <= _currencyToRemove)
                                                                            {
                                                                                int _newCount = _currencyToRemove - _items[j].count;
                                                                                int _newCount2 = _currencyRemoved + _items[j].count;
                                                                                _currencyToRemove = _newCount;
                                                                                _currencyRemoved = _newCount2;
                                                                                _items[j] = ItemStack.Empty.Clone();
                                                                            }
                                                                            else
                                                                            {
                                                                                int _newCount = _currencyRemoved + _currencyToRemove;
                                                                                int _newStackCount = _items[j].count - _currencyToRemove;
                                                                                _currencyToRemove = 0;
                                                                                _currencyRemoved = _newCount;
                                                                                _items[j] = new ItemStack(_itemValue, _newStackCount);
                                                                            }
                                                                            _tile.SetModified();
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
                                }
                                if (_currencyRemoved > 0)
                                {
                                    if (Deposit_Fee_Percent > 0)
                                    {
                                        float _fee = _currencyRemoved * ((float)Deposit_Fee_Percent / 100);
                                        int _adjustedDeposit = _currencyRemoved - (int)_fee;
                                        AddCoinsToBank(_cInfo.playerId, _adjustedDeposit);
                                        string _message = "Deposited {Value} {Name} from the secure loot to your bank account. " + "{Percent}" + "% deposit fee was applied.";
                                        _message = _message.Replace("{Value}", _adjustedDeposit.ToString());
                                        _message = _message.Replace("{Name}", Ingame_Coin);
                                        _message = _message.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                        {
                                            sw.WriteLine(string.Format("{0}: {1} {2} has added {3} to their bank account from a secure loot.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _adjustedDeposit));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                    }
                                    else
                                    {
                                        AddCoinsToBank(_cInfo.playerId, _currencyRemoved);
                                        string _message = "Deposited {Value} {Name} from the secure loot to your bank account.";
                                        _message = _message.Replace("{Value}", _currencyRemoved.ToString());
                                        _message = _message.Replace("{Name}", Ingame_Coin);
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                        {
                                            sw.WriteLine(string.Format("{0}: {1} {2} has added {3} to their bank account from a secure loot.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _currencyRemoved));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                    }
                                }
                                else
                                {
                                    string _message = "Could not find any {CoinName} in a near by secure loot.";
                                    _message = _message.Replace("{CoinName}", Ingame_Coin);
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You input an invalid integer. Type " + ChatHook.Command_Private + Command95 + " #[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You input an invalid integer. Type " + ChatHook.Command_Private + Command95 + " #[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The bank coin is not setup correctly, contact the server Admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        Log.Out(string.Format("[SERVERTOOLS] Bank operation failed. Unable to find item {0}. Check the default game currency from your items.xml", Ingame_Coin));
                    }
                }
            }
        }

        public static void BankToPlayerWithdraw(ClientInfo _cInfo, string _amount)
        {
            if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null && _player.IsSpawned())
                {
                    ItemValue _itemValue = ItemClass.GetItem(Ingame_Coin, false);
                    if (_itemValue != null)
                    {
                        int _value;
                        if (int.TryParse(_amount, out _value))
                        {
                            int _bank = GetCurrentBank(_cInfo.playerId);
                            if (_bank >= _value)
                            {
                                int _maxAllowed = _itemValue.ItemClass.Stacknumber.Value;
                                if (_value <= _maxAllowed)
                                {
                                    ItemStack itemStack = new ItemStack(_itemValue, _value);
                                    if (itemStack != null)
                                    {
                                        if (_player.bag.CanTakeItem(itemStack))
                                        {
                                            string _message = "Your bag can not take all of the {Name}. Check on the ground by your feet.";
                                            _message = _message.Replace("{Name}", Ingame_Coin);
                                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                        else
                                        {
                                            string _message = "You have received the {Name} in your bag.";
                                            _message = _message.Replace("{Name}", Ingame_Coin);
                                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                        SubtractCoinsFromBank(_cInfo.playerId, _value);
                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                        {
                                            sw.WriteLine(string.Format("{0}: {1} has removed {2} from their bank account as {3}.", DateTime.Now, _cInfo.playerName, _value, Ingame_Coin));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                        {
                                            entityClass = EntityClass.FromString("item"),
                                            id = EntityFactory.nextEntityID++,
                                            itemStack = itemStack,
                                            pos = _player.position,
                                            rot = new Vector3(20f, 0f, 20f),
                                            lifetime = 60f,
                                            belongsPlayerId = _cInfo.entityId
                                        });
                                        GameManager.Instance.World.SpawnEntityInWorld(entityItem);
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                        GameManager.Instance.World.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                    }
                                }
                                else
                                {
                                    string _message = "You can only withdraw a full stack at a time. The maximum stack size is {Max}.";
                                    _message = _message.Replace("{Max}", _maxAllowed.ToString());
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                string _message = "Your bank account does not have enough to withdraw that value. Bank account is currently {Total}";
                                _message = _message.Replace("{Total}", _bank.ToString());
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You input an invalid integer. Type " + ChatHook.Command_Private + Command96 + " #.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The bank coin is not setup correctly, contact the server Admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        Log.Out(string.Format("[SERVERTOOLS] Bank operation failed. Unable to find item {0}. Check the default game currency from your items.xml", Ingame_Coin));
                    }
                }
            }
        }

        public static void WalletToBankDeposit(ClientInfo _cInfo, string _amount)
        {
            int _value;
            if (int.TryParse(_amount, out _value))
            {
                int _walletTotal = Wallet.GetCurrentCoins(_cInfo.playerId);
                if (_walletTotal >= _value)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _value);
                    if (Deposit_Fee_Percent > 0)
                    {
                        int _depositFeePercent = Deposit_Fee_Percent / 100;
                        int _depositFee = (int)_value * _depositFeePercent;
                        int _adjustedDeposit = _value - _depositFee;
                        AddCoinsToBank(_cInfo.playerId, _adjustedDeposit);
                        string _message = "Deposited {Value} {Name} from your wallet to your bank account." + " {Percent}" + "% fee was applied.";
                        _message = _message.Replace("{Value}", _value.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        _message = _message.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        using (StreamWriter sw = new StreamWriter(filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account from their wallet.", DateTime.Now, _cInfo.playerName, _adjustedDeposit));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    else
                    {
                        AddCoinsToBank(_cInfo.playerId, _value);
                        string _message = "Deposited {Value} {Name} from your wallet to your bank account.";
                        _message = _message.Replace("{Value}", _value.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        using (StreamWriter sw = new StreamWriter(filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account from their wallet.", DateTime.Now, _cInfo.playerName, _value));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your wallet does not have enough to deposit that value.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You input an invalid integer. Type " + ChatHook.Command_Private + Command97 + " #.", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void BankToWalletWithdraw(ClientInfo _cInfo, string _amount)
        {
            int _value;
            if (int.TryParse(_amount, out _value))
            {
                int _bank = GetCurrentBank(_cInfo.playerId);
                if (_bank >= _value)
                {
                    Wallet.AddCoinsToWallet(_cInfo.playerId, _value);
                    SubtractCoinsFromBank(_cInfo.playerId, _value);
                    string _message = "You have received the {Name} from your bank. It has gone to your wallet.";
                    _message = _message.Replace("{Name}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    using (StreamWriter sw = new StreamWriter(filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}: {1} has removed {2} from their bank account and it has been placed into their wallet.", DateTime.Now, _cInfo.playerName, _value));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your bank account does not have enough to withdraw that value.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You input an invalid integer. Type " + ChatHook.Command_Private + Command98 + " #.", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    int _transferValue;
                    if (int.TryParse(_idAndAmount[1], out _transferValue))
                    {
                        if (TransferId.ContainsValue(_id))
                        {
                            int _bank = GetCurrentBank(_cInfo.playerId);
                            if (_bank >= _transferValue)
                            {
                                foreach (KeyValuePair<string, int> _accountInfo in TransferId)
                                {
                                    if (_accountInfo.Value == _id)
                                    {
                                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_accountInfo.Key);
                                        if (_cInfo2 != null)
                                        {
                                            TransferId.Remove(_cInfo2.playerId);
                                            SubtractCoinsFromBank(_cInfo.playerId, _transferValue);
                                            AddCoinsToBank(_cInfo2.playerId, _transferValue);
                                            string _message = "You have made a bank transfer of {Value} to player {PlayerName}.";
                                            _message = _message.Replace("{Value}", _transferValue.ToString());
                                            _message = _message.Replace("{PlayerName}", _cInfo2.playerName);
                                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            _message = "You have received a bank transfer of {Value} from player {PlayerName}.";
                                            _message = _message.Replace("{Value}", _transferValue.ToString());
                                            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                                            ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            return;
                                        }
                                        else
                                        {
                                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The player is offline. They must be online to transfer.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You do not have enough in your bank account to make this transfer.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You input an invalid transfer Id. Ask for the transfer Id from the player you want to transfer to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You input an invalid transfer. Type " + ChatHook.Command_Private + Command99 + " Id #. Get the Id from the player you are transferring to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You input an invalid transfer. Type " + ChatHook.Command_Private + Command99 + " Id #. Get the Id from the player you are transferring to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You input an invalid transfer. Type " + ChatHook.Command_Private + Command99 + " Id #. Get the Id from the player you are transferring to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

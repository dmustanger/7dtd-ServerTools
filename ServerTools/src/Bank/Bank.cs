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
        private static DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
        private static LinkedList<Chunk> chunkArray = new LinkedList<Chunk>();
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
                string _message = " your bank account is worth {Value}. Transfer Id is {Id}.";
                _message = _message.Replace("{Value}", _bank.ToString());
                _message = _message.Replace("{Id}", _id.ToString());
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
            int _bank = PersistentContainer.Instance.Players[_cInfo.playerId].Bank;
            string _message = " your bank account is worth {Value}. Transfer Id is {Id}.";
            _message = _message.Replace("{Value}", _bank.ToString());
            _message = _message.Replace("{Id}", _rndId.ToString());
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void CheckLocation(ClientInfo _cInfo, string _amount, int _exec)
        {
            if (Inside_Claim)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                Vector3i _vec3i = new Vector3i(x, y, z);
                PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(_player.entityId);
                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(_vec3i, _persistentPlayerData);
                if (_owner != EnumLandClaimOwner.Self || _owner != EnumLandClaimOwner.Ally)
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use this command here. Stand in your own or a friend's claimed space.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    return;
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
            ItemValue _itemValue = ItemClass.GetItem(Ingame_Coin, false);
            if (CoinCheck(_cInfo, _itemValue))
            {
                int _value;
                if (int.TryParse(_amount, out _value))
                {
                    _itemValue = new ItemValue(ItemClass.GetItem(Ingame_Coin).type, 1, 1, false, null, 1);
                    int _currencyToRemove = _value;
                    int _currencyRemoved = 0;
                    ChunkClusterList chunklist = GameManager.Instance.World.ChunkClusters;
                    for (int i = 0; i < chunklist.Count; i++)
                    {
                        if (_currencyToRemove > 0)
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
                                        TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tile;
                                        if (SecureLoot.IsUserAllowed(_cInfo.playerId) && !SecureLoot.IsUserAccessing())
                                        {
                                            Vector3i vec3i = SecureLoot.ToWorldPos();
                                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                            if ((vec3i.x - _player.position.x) * (vec3i.x - _player.position.x) + (vec3i.z - _player.position.z) * (vec3i.z - _player.position.z) <= 2.5 * 2.5)
                                            {
                                                ItemStack[] _items = SecureLoot.items;
                                                for (int j = 0; j < _items.Length; j++)
                                                {
                                                    if (_currencyToRemove > 0)
                                                    {
                                                        if (!_items[j].IsEmpty() && _items[j].itemValue.GetItemOrBlockId() == _itemValue.GetItemOrBlockId())
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
                                                                break;
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
                            int _depositFeePercent = Deposit_Fee_Percent / 100;
                            int _depositFee = (int)_currencyRemoved * _depositFeePercent;
                            int _adjustedDeposit = _currencyRemoved - _depositFee;
                            AddCoinsToBank(_cInfo.playerId, _adjustedDeposit);
                            string _message = " deposited {Value} {Name} from the secure loot to your bank account. " + " {Percent}" + "% fee was applied.";
                            _message = _message.Replace("{Value}", _value.ToString());
                            _message = _message.Replace("{Name}", Ingame_Coin);
                            _message = _message.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            using (StreamWriter sw = new StreamWriter(filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account from a secure loot.", DateTime.Now, _cInfo.playerName, _currencyRemoved));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            using (StreamWriter sw = new StreamWriter(filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account from a secure loot.", DateTime.Now, _cInfo.playerName, _adjustedDeposit));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                        }
                        else
                        {
                            AddCoinsToBank(_cInfo.playerId, _currencyRemoved);
                            string _message = "Removed {Total} {Name} and deposited it to your bank. ";
                            _message = _message.Replace("{Total}", _currencyRemoved.ToString());
                            _message = _message.Replace("{Name}", Ingame_Coin);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            using (StreamWriter sw = new StreamWriter(filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account from a secure loot.", DateTime.Now, _cInfo.playerName, _currencyRemoved));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                        }
                    }
                    else
                    {
                        string _message = "Could not find any {Name} in a near by secure loot.";
                        _message = _message.Replace("{Name}", Ingame_Coin);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you input an invalid integer. Type " + ChatHook.Command_Private + Command95 + " #.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void BankToPlayerWithdraw(ClientInfo _cInfo, string _amount)
        {
            ItemValue _itemValue = ItemClass.GetItem(Ingame_Coin, false);
            if (CoinCheck(_cInfo, _itemValue))
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
                            EntityAlive _player = GameManager.Instance.World.Players.dict[_cInfo.entityId] as EntityAlive;
                            ItemStack itemStack = new ItemStack(_itemValue, _value);
                            if (_player.IsSpawned() && _player.bag.CanTakeItem(itemStack))
                            {
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
                                GameManager.Instance.World.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                                SubtractCoinsFromBank(_cInfo.playerId, _value);
                                string _message = " you have received the {Name} in your bag.";
                                _message = _message.Replace("{Name}", Ingame_Coin);
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}: {1} has removed {2} from their bank account as {3}.", DateTime.Now, _cInfo.playerName, _value, Ingame_Coin));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                            else
                            {
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
                                SubtractCoinsFromBank(_cInfo.playerId, _value);
                                string _message = " your bag could not take all of the {Name}. Dropped the stack on the ground by your feet.";
                                _message = _message.Replace("{Name}", Ingame_Coin);
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            
                        }
                        else
                        {
                            string _message = " you can only withdraw a full stack at a time. The maximum stack size is {Max}.";
                            _message = _message.Replace("{Max}", _maxAllowed.ToString());
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        string _message = " your bank account does not have enough to withdraw that value. Bank account is currently {Total}";
                        _message = _message.Replace("{Total}", _bank.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you input an invalid integer. Type " + ChatHook.Command_Private + Command96 + " #.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        string _message = " deposited {Value} {Name} from your wallet to your bank account." + " {Percent}" + "% fee was applied.";
                        _message = _message.Replace("{Value}", _value.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        _message = _message.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        string _message = " deposited {Value} {Name} from your wallet to your bank account.";
                        _message = _message.Replace("{Value}", _value.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your wallet does not have enough to deposit that value.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you input an invalid integer. Type " + ChatHook.Command_Private + Command97 + " #.", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    string _message = " you have received the {Name} from your bank. It has gone to your wallet.";
                    _message = _message.Replace("{Name}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your bank account does not have enough to withdraw that value.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you input an invalid integer. Type " + ChatHook.Command_Private + Command98 + " #.", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                            string _message = " you have made a bank transfer of {Value} to player {PlayerName}.";
                                            _message = _message.Replace("{Value}", _transferValue.ToString());
                                            _message = _message.Replace("{PlayerName}", _cInfo2.playerName);
                                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            _message = " you have received a bank transfer of {Value} from player {PlayerName}.";
                                            _message = _message.Replace("{Value}", _transferValue.ToString());
                                            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                                            ChatHook.ChatMessage(_cInfo2, ChatHook.Player_Name_Color + _cInfo2.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            return;
                                        }
                                        else
                                        {
                                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " the player is offline. They must be online to transfer.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you do not have enough in your bank account to make this transfer.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you input an invalid transfer Id. Ask for the transfer Id from the player you want to transfer to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you input an invalid transfer. Type " + ChatHook.Command_Private + Command99 + " Id #. Get the Id from the player you are transferring to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you input an invalid transfer. Type " + ChatHook.Command_Private + Command99 + " Id #. Get the Id from the player you are transferring to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you input an invalid transfer. Type " + ChatHook.Command_Private + Command99 + " Id #. Get the Id from the player you are transferring to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static bool CoinCheck(ClientInfo _cInfo, ItemValue _itemValue)
        {
            if (_itemValue.type == ItemValue.None.type)
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " the bank coin is not setup correctly, contact the server Admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                Log.Out(string.Format("[SERVERTOOLS] Unable to find item {0}", Ingame_Coin));
                return false;
            }
            return true;
        }
    }
}

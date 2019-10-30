using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Bank
    {
        public static bool IsEnabled = false, Inside_Claim = false, Account_Transfers = false;
        public static string Ingame_Coin = "casinoCoin", Command94 = "bank", Command95 = "deposit", Command96 = "withdraw", Command97 = "wallet deposit", Command98 = "wallet withdraw", Command99 = "transfer";
        public static int Limit = 50000, Deposit_Fee_Percent = 5;
        public static Dictionary<string, int> TransferId = new Dictionary<string, int>();
        private static DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
        private static LinkedList<Chunk> chunkArray = new LinkedList<Chunk>();
        private static string file = string.Format("Bank_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/BankLogs/{1}", API.ConfigPath, file);
        private static System.Random random = new System.Random();

        public static void Check(ClientInfo _cInfo)
        {
            int _bank = PersistentContainer.Instance.Players[_cInfo.playerId].Bank;
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
                    else if (_exec == 2)
                    {
                        Withdraw(_cInfo, _amount);
                    }
                    else if (_exec == 3)
                    {
                        WalletDeposit(_cInfo, _amount);
                    }
                    else if (_exec == 4)
                    {
                        WalletWithdraw(_cInfo, _amount);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use this command here.Stand in your own or a friend's claimed space.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                if (_exec == 1)
                {
                    Deposit(_cInfo, _amount);
                }
                else if (_exec == 2)
                {
                    Withdraw(_cInfo, _amount);
                }
                else if (_exec == 3)
                {
                    WalletDeposit(_cInfo, _amount);
                }
                else if (_exec == 4)
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
                        if (!Found)
                        {
                            tiles = _c.GetTileEntities();
                            foreach (TileEntity tile in tiles.dict.Values)
                            {

                                TileEntityType type = tile.GetTileEntityType();
                                if (type.ToString().Equals("SecureLoot"))
                                {
                                    TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tile;
                                    if (SecureLoot.IsUserAllowed(_cInfo.playerId))
                                    {
                                        Vector3i vec3i = SecureLoot.ToWorldPos();
                                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        if ((vec3i.x - _player.position.x) * (vec3i.x - _player.position.x) + (vec3i.z - _player.position.z) * (vec3i.z - _player.position.z) <= 2 * 2)
                                        {
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
                                                            int _bank = PersistentContainer.Instance.Players[_cInfo.playerId].Bank;
                                                            int _percent = Deposit_Fee_Percent / 100;
                                                            double _fee = _coinAmount * _percent;
                                                            int _newCoin = _coinAmount - (int)_fee;
                                                            double _newLimit = Limit + (Limit * _percent);
                                                            if (_bank + _coinAmount <= (int)_newLimit)
                                                            {
                                                                Found = true;
                                                                int _newCount = item.count - _coinAmount;
                                                                ItemValue _itemValue = ItemClass.GetItem(Ingame_Coin, false);
                                                                if (_itemValue.type == ItemValue.None.type)
                                                                {
                                                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " the bank coin is not setup correctly, contact the server Admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                                    Log.Out(string.Format("[SERVERTOOLS] Unable to find item {0}", Ingame_Coin));
                                                                    return;
                                                                }
                                                                else
                                                                {
                                                                    _itemValue = new ItemValue(ItemClass.GetItem(Ingame_Coin).type, 1, 1, false, null, 1);
                                                                }
                                                                ItemStack itemStack = new ItemStack(_itemValue, _newCount);
                                                                SecureLoot.UpdateSlot(slotNumber, itemStack);
                                                                PersistentContainer.Instance.Players[_cInfo.playerId].Bank = _bank + _newCoin;
                                                                PersistentContainer.Instance.Save();
                                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                                {
                                                                    sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account.", DateTime.Now, _cInfo.playerName, _newCoin));
                                                                    sw.WriteLine();
                                                                    sw.Flush();
                                                                    sw.Close();
                                                                }
                                                                string _message = " deposited {Value} in to your bank minus the transfer fee of {Percent} percent.";
                                                                _message = _message.Replace("{Value}", _coinAmount.ToString());
                                                                _message = _message.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                                                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                                return;
                                                            }
                                                            else
                                                            {
                                                                string _message = " your bank can not hold this much. The bank can hold {Limit} total. You currently have {Value}.";
                                                                _message = _message.Replace("{Limit}", Limit.ToString());
                                                                _message = _message.Replace("{Value}", _bank.ToString());
                                                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                                return;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            string _message = " there is not enough {Name} in the secure loot to deposit this value.";
                                                            _message = _message.Replace("{Name}", Ingame_Coin);
                                                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                            return;
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
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you input an invalid integer. Type " + ChatHook.Command_Private + Command95 + " #.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            if (Found)
            {
                string _message = " there is not enough {Name} in the secure loot to deposit this value.";
                _message = _message.Replace("{Name}", Ingame_Coin);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you do not have enough in the secure loot to deposit that much into your bank.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Withdraw(ClientInfo _cInfo, string _amount)
        {
            int _coinAmount;
            if (int.TryParse(_amount, out _coinAmount))
            {
                int _bank = PersistentContainer.Instance.Players[_cInfo.playerId].Bank;
                if (_bank >= _coinAmount)
                {
                    ItemClass _class = ItemClass.GetItemClass(Ingame_Coin, false);
                    if (_class == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Unable to find item {0}", Ingame_Coin));
                        return;
                    }
                    ItemValue _item = new ItemValue(ItemClass.GetItem(Ingame_Coin).type, 1, 1, false, null, 1);
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
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                            world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                            PersistentContainer.Instance.Players[_cInfo.playerId].Bank = _bank - _coinAmount;
                            PersistentContainer.Instance.Save();
                            string _message = " you have received your {Name}. If your inventory is full, check the ground.";
                            _message = _message.Replace("{Name}", Ingame_Coin);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        string _message = " you can only withdraw a full stack at a time. The maximum stack size is {Max}.";
                        _message = _message.Replace("{Max}", _maxAllowed.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your bank account does not have enough to withdraw that value.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you input an invalid integer. Type " + ChatHook.Command_Private + Command96 + " #.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void WalletDeposit(ClientInfo _cInfo, string _amount)
        {
            int _depositValue;
            if (int.TryParse(_amount, out _depositValue))
            {
                int _walletTotal = Wallet.GetCurrentCoins(_cInfo);
                if (_walletTotal >= _depositValue)
                {
                    double _bankCharge = _depositValue * Deposit_Fee_Percent;
                    int _adjustedValue = _depositValue - (int)_bankCharge;
                    int _bank = PersistentContainer.Instance.Players[_cInfo.playerId].Bank;
                    int _newBankTotal = _bank + _adjustedValue;
                    if (_newBankTotal <= Limit)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].Bank = _newBankTotal;
                        PersistentContainer.Instance.Save();
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _depositValue);
                        string _message = " deposited {Value} {Name} from your wallet to your bank account. " + "{Percent}" + "% fee was applied.";
                        _message = _message.Replace("{Value}", _depositValue.ToString());
                        _message = _message.Replace("{Name}", Wallet.Coin_Name);
                        _message = _message.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        using (StreamWriter sw = new StreamWriter(filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account from their wallet.", DateTime.Now, _cInfo.playerName, _newBankTotal));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    else
                    {
                        string _message = " your bank can not hold this much. The bank can hold {Limit} total. You currently have {Value}.";
                        _message = _message.Replace("{Limit}", Limit.ToString());
                        _message = _message.Replace("{Value}", _bank.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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

        public static void WalletWithdraw(ClientInfo _cInfo, string _amount)
        {
            int _withdrawValue;
            if (int.TryParse(_amount, out _withdrawValue))
            {
                int _bank = PersistentContainer.Instance.Players[_cInfo.playerId].Bank;
                if (_bank >= _withdrawValue)
                {
                    int _newBankTotal = _bank - _withdrawValue;
                    PersistentContainer.Instance.Players[_cInfo.playerId].Bank = _newBankTotal;
                    PersistentContainer.Instance.Save();
                    Wallet.AddCoinsToWallet(_cInfo.playerId, _withdrawValue);
                    string _message = " you have received your {Name} from the bank. It has gone to your wallet.";
                    _message = _message.Replace("{Name}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    using (StreamWriter sw = new StreamWriter(filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}: {1} has removed {2} from their bank account and it has been placed into their wallet.", DateTime.Now, _cInfo.playerName, _withdrawValue));
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

        public static void Transfer(ClientInfo _cInfo1, string _transferIdAndAmount)
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
                            int _bank1 = PersistentContainer.Instance.Players[_cInfo1.playerId].Bank;
                            if (_bank1 >= _transferValue)
                            {
                                foreach (KeyValuePair<string, int> _accountInfo in TransferId)
                                {
                                    if (_accountInfo.Value == _id)
                                    {
                                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_accountInfo.Key);
                                        if (_cInfo2 != null)
                                        {
                                            TransferId.Remove(_cInfo2.playerId);
                                            int _bank2 = PersistentContainer.Instance.Players[_cInfo2.playerId].Bank;
                                            PersistentContainer.Instance.Players[_cInfo2.playerId].Bank = _bank2 + _transferValue;
                                            PersistentContainer.Instance.Players[_cInfo1.playerId].Bank = _bank1 - _transferValue;
                                            PersistentContainer.Instance.Save();
                                            string _message = " you have made a bank transfer of {Value} to player {PlayerName}.";
                                            _message = _message.Replace("{Value}", _transferValue.ToString());
                                            _message = _message.Replace("{PlayerName}", _cInfo2.playerName);
                                            ChatHook.ChatMessage(_cInfo1, ChatHook.Player_Name_Color + _cInfo1.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            _message = " you have received a bank transfer of {Value} from player {PlayerName}.";
                                            _message = _message.Replace("{Value}", _transferValue.ToString());
                                            _message = _message.Replace("{PlayerName}", _cInfo1.playerName);
                                            ChatHook.ChatMessage(_cInfo2, ChatHook.Player_Name_Color + _cInfo1.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo1, ChatHook.Player_Name_Color + _cInfo1.playerName + LoadConfig.Chat_Response_Color + " you do not have enough in your bank account to make this transfer.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo1, ChatHook.Player_Name_Color + _cInfo1.playerName + LoadConfig.Chat_Response_Color + " you input an invalid transfer Id. Ask for the transfer Id from the player you want to transfer to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo1, ChatHook.Player_Name_Color + _cInfo1.playerName + LoadConfig.Chat_Response_Color + " you input an invalid transfer. Type " + ChatHook.Command_Private + Command99 + " Id #. Get the Id from the player you are transferring to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo1, ChatHook.Player_Name_Color + _cInfo1.playerName + LoadConfig.Chat_Response_Color + " you input an invalid transfer. Type " + ChatHook.Command_Private + Command99 + " Id #. Get the Id from the player you are transferring to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo1, ChatHook.Player_Name_Color + _cInfo1.playerName + LoadConfig.Chat_Response_Color + " you input an invalid transfer. Type " + ChatHook.Command_Private + Command99 + " Id #. Get the Id from the player you are transferring to.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class Bank
    {
        public static bool IsEnabled = false, Inside_Claim = false, Player_Transfers = false;
        public static string Ingame_Coin = "casinoCoin", Command_bank = "bank", Command_deposit = "deposit", Command_withdraw = "withdraw",
            Command_wallet_deposit = "wallet deposit", Command_wallet_withdraw = "wallet withdraw", Command_transfer = "transfer";
        public static int Deposit_Fee_Percent = 5;
        public static Dictionary<string, int> TransferId = new Dictionary<string, int>();

        private static readonly string file = string.Format("Bank_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/BankLogs/{1}", API.ConfigPath, file);
        private static readonly System.Random Random = new System.Random();

        public static int GetCurrentBank(string _steamid)
        {
            int _bankValue = PersistentContainer.Instance.Players[_steamid].Bank;
            if (_bankValue < 0)
            {
                PersistentContainer.Instance.Players[_steamid].Bank = 0;
                PersistentContainer.DataChange = true;
                _bankValue = 0;
            }
            return _bankValue;
        }

        public static void AddCoinsToBank(string _steamid, int _amount)
        {
            int _bankValue = PersistentContainer.Instance.Players[_steamid].Bank;
            PersistentContainer.Instance.Players[_steamid].Bank = _bankValue + _amount;
            PersistentContainer.DataChange = true;
        }

        public static void SubtractCoinsFromBank(string _steamid, int _amount)
        {
            int _newValue = PersistentContainer.Instance.Players[_steamid].Bank - _amount;
            if (_newValue < 0)
            {
                _newValue = 0;
            }
            PersistentContainer.Instance.Players[_steamid].Bank = _newValue;
            PersistentContainer.DataChange = true;
        }

        public static void ClearBank(ClientInfo _cInfo)
        {
            PersistentContainer.Instance.Players[_cInfo.playerId].Bank = 0;
            PersistentContainer.DataChange = true;
        }

        public static void CurrentBankAndId(ClientInfo _cInfo)
        {
            try
            {
                int _bank = GetCurrentBank(_cInfo.playerId);
                if (TransferId.ContainsKey(_cInfo.playerId))
                {
                    TransferId.TryGetValue(_cInfo.playerId, out int _id);
                    Phrases.Dict.TryGetValue("Bank1", out string _phrase);
                    _phrase = _phrase.Replace("{Value}", _bank.ToString());
                    _phrase = _phrase.Replace("{Id}", _id.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    AddId(_cInfo);
                    CurrentBankAndId(_cInfo);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.CurrentBankAndId: {0}", e.Message));
            }
        }

        public static void AddId(ClientInfo _cInfo)
        {
            try
            {
                int _tranferId = GenerateTransferId();
                if (_tranferId > 0)
                {
                    TransferId.Add(_cInfo.playerId, _tranferId);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.AddId: {0}", e.Message));
            }
        }

        private static int GenerateTransferId()
        {
            int _id = Random.Next(1000, 5001);
            if (TransferId.ContainsValue(_id))
            {
                _id = Random.Next(1000, 5001);
                if (TransferId.ContainsValue(_id))
                {
                    _id = Random.Next(1000, 5001);
                    if (TransferId.ContainsValue(_id))
                    {
                        _id = Random.Next(1000, 5001);
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
            try
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
                                Phrases.Dict.TryGetValue("Bank2", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.CheckLocation: {0}", e.Message));
            }
        }

        public static void ChestToBankDeposit(ClientInfo _cInfo, string _amount)
        {
            try
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
                                        ChunkCluster _chunkCluster = _chunklist[i];
                                        _chunkArray = _chunkCluster.GetChunkArray();
                                        foreach (Chunk _chunk in _chunkArray)
                                        {
                                            _tiles = _chunk.GetTileEntities();
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
                                            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                            {
                                                sw.WriteLine(string.Format("{0}: {1} {2} has added {3} to their bank account from a secure loot.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _adjustedDeposit));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            Phrases.Dict.TryGetValue("Bank3", out string _phrase);
                                            _phrase = _phrase.Replace("{Value}", _adjustedDeposit.ToString());
                                            _phrase = _phrase.Replace("{CoinName}", Ingame_Coin);
                                            _phrase = _phrase.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                        else
                                        {
                                            AddCoinsToBank(_cInfo.playerId, _currencyRemoved);
                                            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                            {
                                                sw.WriteLine(string.Format("{0}: {1} {2} has added {3} to their bank account from a secure loot.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _currencyRemoved));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            Phrases.Dict.TryGetValue("Bank4", out string _phrase);
                                            _phrase = _phrase.Replace("{Value}", _currencyRemoved.ToString());
                                            _phrase = _phrase.Replace("{CoinName}", Ingame_Coin);
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue("Bank5", out string _phrase);
                                        _phrase = _phrase.Replace("{CoinName}", Ingame_Coin);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Bank6", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_deposit}", Command_deposit);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Bank6", out string _phrase);
                                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                _phrase = _phrase.Replace("{Command_deposit}", Command_deposit);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Bank7", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            Log.Out(string.Format("[SERVERTOOLS] Bank operation failed. Unable to find item {0}. Check the default game currency from your items.xml", Ingame_Coin));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.ChestToBankDeposit: {0}", e.Message));
            }
        }

        public static void BankToPlayerWithdraw(ClientInfo _cInfo, string _amount)
        {
            try
            {
                if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player != null && _player.IsSpawned())
                    {
                        ItemValue _itemValue = ItemClass.GetItem(Ingame_Coin, false);
                        if (_itemValue != null)
                        {
                            if (int.TryParse(_amount, out int _value))
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
                                            SubtractCoinsFromBank(_cInfo.playerId, _value);
                                            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                            {
                                                sw.WriteLine(string.Format("{0}: {1} has removed {2} from their bank account as {3}.", DateTime.Now, _cInfo.playerName, _value, Ingame_Coin));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            if (!_player.bag.CanTakeItem(itemStack))
                                            {
                                                Phrases.Dict.TryGetValue("Bank8", out string _phrase);
                                                _phrase = _phrase.Replace("{CoinName}", Ingame_Coin);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                            else
                                            {
                                                Phrases.Dict.TryGetValue("Bank9", out string _phrase);
                                                _phrase = _phrase.Replace("{CoinName}", Ingame_Coin);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                        Phrases.Dict.TryGetValue("Bank10", out string _phrase);
                                        _phrase = _phrase.Replace("{Max}", _maxAllowed.ToString());
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Bank11", out string _phrase);
                                    _phrase = _phrase.Replace("{Total}", _bank.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Bank12", out string _phrase);
                                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                _phrase = _phrase.Replace("{Command_withdraw}", Command_withdraw);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Bank7", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            Log.Out(string.Format("[SERVERTOOLS] Bank operation failed. Unable to find item {0}. Check the default game currency from your items.xml", Ingame_Coin));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.BankToPlayerWithdraw: {0}", e.Message));
            }
        }

        public static void WalletToBankDeposit(ClientInfo _cInfo, string _amount)
        {
            try
            {
                if (int.TryParse(_amount, out int _value))
                {
                    int _walletTotal = Wallet.GetCurrentCoins(_cInfo.playerId);
                    if (_walletTotal >= _value)
                    {
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _value);
                        if (Deposit_Fee_Percent > 0)
                        {
                            float _fee = _value * ((float)Deposit_Fee_Percent / 100);
                            int _adjustedDeposit = _value - (int)_fee;
                            AddCoinsToBank(_cInfo.playerId, _adjustedDeposit);
                            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                            {
                                sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account from their wallet.", DateTime.Now, _cInfo.playerName, _adjustedDeposit));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            Phrases.Dict.TryGetValue("Bank13", out string _phrase);
                            _phrase = _phrase.Replace("{Value}", _value.ToString());
                            _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                            _phrase = _phrase.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            AddCoinsToBank(_cInfo.playerId, _value);
                            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                            {
                                sw.WriteLine(string.Format("{0}: {1} {2} has added {3} to their bank account from their wallet.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _value));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            Phrases.Dict.TryGetValue("Bank14", out string _phrase);
                            _phrase = _phrase.Replace("{Value}", _value.ToString());
                            _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Bank15", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Bank16", out string _phrase);
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_wallet_deposit}", Command_wallet_deposit);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.WalletToBankDeposit: {0}", e.Message));
            }
        }

        public static void BankToWalletWithdraw(ClientInfo _cInfo, string _amount)
        {
            try
            {
                if (int.TryParse(_amount, out int _value))
                {
                    int _bank = GetCurrentBank(_cInfo.playerId);
                    if (_bank >= _value)
                    {
                        Wallet.AddCoinsToWallet(_cInfo.playerId, _value);
                        SubtractCoinsFromBank(_cInfo.playerId, _value);
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("{0}: {1} has removed {2} from their bank account and it has been placed into their wallet.", DateTime.Now, _cInfo.playerName, _value));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Phrases.Dict.TryGetValue("Bank17", out string _phrase);
                        _phrase = _phrase.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Bank18", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Bank19", out string _phrase);
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_wallet_withdraw}", Command_wallet_withdraw);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.BankToWalletWithdraw: {0}", e.Message));
            }
        }

        public static void Transfer(ClientInfo _cInfo, string _transferIdAndAmount)
        {
            try
            {
                string[] _idAndAmount = { };
                if (_transferIdAndAmount.Contains(" "))
                {
                    _idAndAmount = _transferIdAndAmount.Split(' ').ToArray();
                    if (int.TryParse(_idAndAmount[0], out int _id))
                    {
                        if (int.TryParse(_idAndAmount[1], out int _transferValue))
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
                                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} {2} has made a bank transfer of {3} to {4} {5}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _transferValue, _cInfo2.playerId, _cInfo2.playerName));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Phrases.Dict.TryGetValue("Bank20", out string _phrase);
                                                _phrase = _phrase.Replace("{Value}", _transferValue.ToString());
                                                _phrase = _phrase.Replace("{PlayerName}", _cInfo2.playerName);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                Phrases.Dict.TryGetValue("Bank21", out _phrase);
                                                _phrase = _phrase.Replace("{Value}", _transferValue.ToString());
                                                _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return;
                                            }
                                            else
                                            {
                                                Phrases.Dict.TryGetValue("Bank22", out string _phrase);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Bank11", out string _phrase);
                                    _phrase = _phrase.Replace("{Total}", _bank.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Bank23", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Bank24", out string _phrase);
                            _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            _phrase = _phrase.Replace("{Command_transfer}", Command_transfer);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Bank25", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Bank25", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.Transfer: {0}", e.Message));
            }
        }
    }
}

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
                _bankValue = 0;
            }
            return _bankValue;
        }

        public static void AddCoinsToBank(string _steamid, int _amount)
        {
            int _bankValue = PersistentContainer.Instance.Players[_steamid].Bank;
            PersistentContainer.Instance.Players[_steamid].Bank = _bankValue + _amount;
        }

        public static void SubtractCoinsFromBank(string _steamid, int _amount)
        {
            int _newValue = PersistentContainer.Instance.Players[_steamid].Bank - _amount;
            if (_newValue < 0)
            {
                _newValue = 0;
            }
            PersistentContainer.Instance.Players[_steamid].Bank = _newValue;
        }

        public static void ClearBank(ClientInfo _cInfo)
        {
            PersistentContainer.Instance.Players[_cInfo.playerId].Bank = 0;
        }

        public static void CurrentBankAndId(ClientInfo _cInfo)
        {
            try
            {
                int _bank = GetCurrentBank(_cInfo.playerId);
                if (TransferId.ContainsKey(_cInfo.playerId))
                {
                    int _id;
                    TransferId.TryGetValue(_cInfo.playerId, out _id);
                    Phrases.Dict.TryGetValue(641, out string _phrase641);
                    _phrase641 = _phrase641.Replace("{Value}", _bank.ToString());
                    _phrase641 = _phrase641.Replace("{Id}", _id.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase641 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                Phrases.Dict.TryGetValue(642, out string _phrase642);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase642 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                            using (StreamWriter sw = new StreamWriter(filepath, true))
                                            {
                                                sw.WriteLine(string.Format("{0}: {1} {2} has added {3} to their bank account from a secure loot.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _adjustedDeposit));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            Phrases.Dict.TryGetValue(643, out string _phrase643);
                                            _phrase643 = _phrase643.Replace("{Value}", _adjustedDeposit.ToString());
                                            _phrase643 = _phrase643.Replace("{CoinName}", Ingame_Coin);
                                            _phrase643 = _phrase643.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase643 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                        else
                                        {
                                            AddCoinsToBank(_cInfo.playerId, _currencyRemoved);
                                            using (StreamWriter sw = new StreamWriter(filepath, true))
                                            {
                                                sw.WriteLine(string.Format("{0}: {1} {2} has added {3} to their bank account from a secure loot.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _currencyRemoved));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            Phrases.Dict.TryGetValue(644, out string _phrase644);
                                            _phrase644 = _phrase644.Replace("{Value}", _currencyRemoved.ToString());
                                            _phrase644 = _phrase644.Replace("{CoinName}", Ingame_Coin);
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase644 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(645, out string _phrase645);
                                        _phrase645 = _phrase645.Replace("{CoinName}", Ingame_Coin);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase645 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(646, out string _phrase646);
                                    _phrase646 = _phrase646.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                    _phrase646 = _phrase646.Replace("{Command95}", Command95);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase646 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue(646, out string _phrase646);
                                _phrase646 = _phrase646.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                _phrase646 = _phrase646.Replace("{Command95}", Command95);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase646 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(647, out string _phrase647);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase647 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                            SubtractCoinsFromBank(_cInfo.playerId, _value);
                                            using (StreamWriter sw = new StreamWriter(filepath, true))
                                            {
                                                sw.WriteLine(string.Format("{0}: {1} has removed {2} from their bank account as {3}.", DateTime.Now, _cInfo.playerName, _value, Ingame_Coin));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            if (!_player.bag.CanTakeItem(itemStack))
                                            {
                                                Phrases.Dict.TryGetValue(648, out string _phrase648);
                                                _phrase648 = _phrase648.Replace("{CoinName}", Ingame_Coin);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase648 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                            else
                                            {
                                                Phrases.Dict.TryGetValue(649, out string _phrase649);
                                                _phrase649 = _phrase649.Replace("{CoinName}", Ingame_Coin);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase649 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                        Phrases.Dict.TryGetValue(650, out string _phrase650);
                                        _phrase650 = _phrase650.Replace("{Max}", _maxAllowed.ToString());
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase650 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(651, out string _phrase651);
                                    _phrase651 = _phrase651.Replace("{Total}", _bank.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase651 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue(652, out string _phrase652);
                                _phrase652 = _phrase652.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                _phrase652 = _phrase652.Replace("{Command96}", Command96);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase652 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(647, out string _phrase647);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase647 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                int _value;
                if (int.TryParse(_amount, out _value))
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
                            using (StreamWriter sw = new StreamWriter(filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}: {1} has added {2} to their bank account from their wallet.", DateTime.Now, _cInfo.playerName, _adjustedDeposit));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            Phrases.Dict.TryGetValue(653, out string _phrase653);
                            _phrase653 = _phrase653.Replace("{Value}", _value.ToString());
                            _phrase653 = _phrase653.Replace("{CoinName}", Wallet.Coin_Name);
                            _phrase653 = _phrase653.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase653 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            AddCoinsToBank(_cInfo.playerId, _value);
                            using (StreamWriter sw = new StreamWriter(filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}: {1} {2} has added {3} to their bank account from their wallet.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _value));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            Phrases.Dict.TryGetValue(654, out string _phrase654);
                            _phrase654 = _phrase654.Replace("{Value}", _value.ToString());
                            _phrase654 = _phrase654.Replace("{CoinName}", Wallet.Coin_Name);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase654 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(655, out string _phrase655);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase655 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(656, out string _phrase656);
                    _phrase656 = _phrase656.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase656 = _phrase656.Replace("{Command97}", Command97);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase656 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                int _value;
                if (int.TryParse(_amount, out _value))
                {
                    int _bank = GetCurrentBank(_cInfo.playerId);
                    if (_bank >= _value)
                    {
                        Wallet.AddCoinsToWallet(_cInfo.playerId, _value);
                        SubtractCoinsFromBank(_cInfo.playerId, _value);
                        using (StreamWriter sw = new StreamWriter(filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: {1} has removed {2} from their bank account and it has been placed into their wallet.", DateTime.Now, _cInfo.playerName, _value));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Phrases.Dict.TryGetValue(657, out string _phrase657);
                        _phrase657 = _phrase657.Replace("{Name}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase657 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(658, out string _phrase658);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase658 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(659, out string _phrase659);
                    _phrase659 = _phrase659.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase659 = _phrase659.Replace("{Command98}", Command98);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase659 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} {2} has made a bank transfer of {3} to {4} {5}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _transferValue, _cInfo2.playerId, _cInfo2.playerName));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Phrases.Dict.TryGetValue(660, out string _phrase660);
                                                _phrase660 = _phrase660.Replace("{Value}", _transferValue.ToString());
                                                _phrase660 = _phrase660.Replace("{PlayerName}", _cInfo2.playerName);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase660 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                Phrases.Dict.TryGetValue(661, out string _phrase661);
                                                _phrase661 = _phrase661.Replace("{Value}", _transferValue.ToString());
                                                _phrase661 = _phrase661.Replace("{PlayerName}", _cInfo.playerName);
                                                ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase661 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return;
                                            }
                                            else
                                            {
                                                Phrases.Dict.TryGetValue(662, out string _phrase662);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase662 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(651, out string _phrase651);
                                    _phrase651 = _phrase651.Replace("{Total}", _bank.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase651 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue(663, out string _phrase663);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase663 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(664, out string _phrase664);
                            _phrase664 = _phrase664.Replace("{CommandPrivate}", ChatHook.Command_Private);
                            _phrase664 = _phrase664.Replace("{Command99}", Command99);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase664 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(665, out string _phrase665);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase665 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(665, out string _phrase665);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase665 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.Transfer: {0}", e.Message));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class DupeLog
    {
        public static bool IsEnabled = false;
        private static Dictionary<int, ItemStack[]> Bag = new Dictionary<int, ItemStack[]>();
        private static Dictionary<int, ItemStack[]> Inventory = new Dictionary<int, ItemStack[]>();
        private static Dictionary<int, int> Crafted = new Dictionary<int, int>();
        private static string file = string.Format("DupeLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/DupeLogs/{1}", API.GamePath, file);

        public static void CreateFolder()
        {
            if (!Directory.Exists(API.GamePath + "/DupeLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/DupeLogs");
            }
        }

        public static void Exec(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            ItemStack[] _bag = _playerDataFile.bag;
            ItemStack[] _inventory = _playerDataFile.inventory;

            int _craftCount;
            if (Crafted.TryGetValue(_cInfo.entityId, out _craftCount))
            {
                if ((int)_playerDataFile.totalItemsCrafted != _craftCount)
                {
                    Crafted[_cInfo.entityId] = (int)_playerDataFile.totalItemsCrafted;
                    Bag[_cInfo.entityId] = _bag;
                    Inventory[_cInfo.entityId] = _inventory;
                    return;
                }
            }
            else
            {
                Crafted.Add(_cInfo.entityId, (int)_playerDataFile.totalItemsCrafted);
                Bag.Add(_cInfo.entityId, _bag);
                Inventory.Add(_cInfo.entityId, _inventory);
                return;
            }

            List<int> BagSlot = new List<int>();
            List<int> InvSlot = new List<int>();
            ItemStack[] _bagStacks, _invStacks;
            ItemStack _bagStackOld, _bagStackNew, _invStackOld, _invStackNew, _CompareBagOld, _CompareBagNew,
            _CompareInvOld, _CompareInvNew;

            Bag.TryGetValue(_cInfo.entityId, out _bagStacks);
            {
                Inventory.TryGetValue(_cInfo.entityId, out _invStacks);
                {
                    int _bagSize = _bag.Length;
                    int _invSize = _inventory.Length;
                    for (int i = 0; i < _bagSize; i++)
                    {
                        _bagStackOld = _bagStacks[i];
                        _bagStackNew = _bag[i];
                        bool BagNext = true;
                        int _oldTotal = 0, _newTotal = 0, _newCount;
                        if (!_bagStackNew.Equals(_bagStackOld) && !_bagStackNew.IsEmpty())
                        {
                            string _name = _bagStackNew.itemValue.ItemClass.Name;
                            for (int j = 0; j < _bagSize; j++)
                            {
                                _CompareBagOld = _bagStacks[j];
                                if (!_CompareBagOld.IsEmpty() && _name == _CompareBagOld.itemValue.ItemClass.Name)
                                {
                                    _newCount = _oldTotal + _CompareBagOld.count;
                                    _oldTotal = _newCount;
                                }
                            }
                            for (int j = 0; j < _invSize; j++)
                            {
                                _CompareInvOld = _invStacks[j];
                                if (!_CompareInvOld.IsEmpty() && _name == _CompareInvOld.itemValue.ItemClass.Name)
                                {
                                    _newCount = _oldTotal + _CompareInvOld.count;
                                    _oldTotal = _newCount;
                                }
                            }

                            for (int j = 0; j < _bagSize; j++)
                            {
                                _CompareBagNew = _bag[j];
                                if (!_CompareBagNew.IsEmpty() && _name == _CompareBagNew.itemValue.ItemClass.Name)
                                {
                                    _newCount = _newTotal + _CompareBagNew.count;
                                    _newTotal = _newCount;
                                }
                            }
                            for (int j = 0; j < _invSize; j++)
                            {
                                _CompareInvNew = _inventory[j];
                                if (!_CompareInvNew.IsEmpty() && _name == _CompareInvNew.itemValue.ItemClass.Name)
                                {
                                    _newCount = _newTotal + _CompareInvNew.count;
                                    _newTotal = _newCount;
                                }
                            }
                            if (_oldTotal == _newTotal)
                            {
                                BagNext = false;
                            }
                            if (BagNext)
                            {
                                if (_bagStackNew.count == 1)
                                {
                                    int _counter1 = 0, _counter2 = 0;
                                    for (int j = 0; j < _bagSize; j++)
                                    {
                                        _CompareBagNew = _bag[j];
                                        if (!_CompareBagNew.IsEmpty() && _name == _CompareBagNew.itemValue.ItemClass.Name && _CompareBagNew.count == 1)
                                        {
                                            _counter1++;
                                        }
                                    }
                                    for (int j = 0; j < _invSize; j++)
                                    {
                                        _CompareInvNew = _inventory[j];
                                        if (!_CompareInvNew.IsEmpty() && _name == _CompareInvNew.itemValue.ItemClass.Name && _CompareInvNew.count == 1)
                                        {
                                            _counter2++;
                                        }
                                    }
                                    if (_counter1 + _counter2 > 1)
                                    {
                                        for (int j = 0; j < _bagSize; j++)
                                        {
                                            _CompareBagOld = _bagStacks[j];
                                            if (!_CompareBagOld.IsEmpty() && _name == _CompareBagOld.itemValue.ItemClass.Name && _CompareBagOld.count >= _counter1)
                                            {
                                                BagNext = false;
                                            }
                                        }
                                        if (BagNext)
                                        {
                                            for (int j = 0; j < _invSize; j++)
                                            {
                                                _CompareInvOld = _invStacks[j];
                                                if (!_CompareInvOld.IsEmpty() && _name == _CompareInvOld.itemValue.ItemClass.Name && _CompareInvOld.count >= _counter1)
                                                {
                                                    BagNext = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (BagNext)
                            {
                                if (_bagStackNew.itemValue.ItemClass.HasParts)
                                {
                                    for (int j = 0; j < _bagSize; j++)
                                    {
                                        _CompareBagNew = _bag[j];
                                        if (!_CompareBagNew.IsEmpty() && _bagStackNew.Equals(_CompareBagNew) && i != j && !BagSlot.Contains(j))
                                        {
                                            BagSlot.Add(i);
                                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                            PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                            EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                            if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                            {
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality and parts to their bag inside their own or ally claimed space @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                            }
                                            else
                                            {
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality and parts to their bag, standing @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                            }
                                            BagNext = false;
                                        }
                                    }
                                    for (int j = 0; j < _invSize; j++)
                                    {
                                        _CompareInvNew = _inventory[j];
                                        if (!_CompareInvNew.IsEmpty() && _bagStackNew.Equals(_CompareInvNew) && !InvSlot.Contains(j))
                                        {
                                            InvSlot.Add(i);
                                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                            PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                            EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                            if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                            {
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality and parts to their bag inside their own or ally claimed space @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                            }
                                            else
                                            {
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality and parts to their bag, standing @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                            }
                                            BagNext = false;
                                        }
                                    }
                                }
                                else if (_bagStackNew.itemValue.HasQuality)
                                {
                                    for (int j = 0; j < _bagSize; j++)
                                    {
                                        _CompareBagNew = _bag[j];
                                        if (!_CompareBagNew.IsEmpty() && _bagStackNew.Equals(_CompareBagNew) && i != j && !BagSlot.Contains(j))
                                        {
                                            BagSlot.Add(i);
                                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                            PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                            EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                            if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                            {
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality to their bag inside their own or ally claimed space @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                            }
                                            else
                                            {
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality to their bag, standing  @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                            }
                                        }
                                    }
                                    for (int j = 0; j < _invSize; j++)
                                    {
                                        _CompareInvNew = _inventory[j];
                                        if (!_CompareInvNew.IsEmpty() && _bagStackNew.Equals(_CompareInvNew) && !InvSlot.Contains(j))
                                        {
                                            InvSlot.Add(i);
                                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                            PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                            EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                            if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                            {
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality to their bag inside their own or ally claimed space @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                            }
                                            else
                                            {
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality to their bag, standing @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (_bagStackNew.count > 1)
                                    {
                                        for (int j = 0; j < _bagSize; j++)
                                        {
                                            _CompareBagNew = _bag[j];
                                            if (!_CompareBagNew.IsEmpty() && _bagStackNew.Equals(_CompareBagNew) && i != j && !BagSlot.Contains(j))
                                            {
                                                BagSlot.Add(i);
                                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} {3} to their bag, identical to another stack, inside their own or ally claimed space @ {4} {5} {6}.", DateTime.Now, _cInfo.playerName, _bagStackNew.count, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                                else
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} {3} to their bag, identical to another stack, standing @ {4} {5} {6}.", DateTime.Now, _cInfo.playerName, _bagStackNew.count, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                            }
                                        }
                                        for (int j = 0; j < _invSize; j++)
                                        {
                                            _CompareInvNew = _inventory[j];
                                            if (!_CompareInvNew.IsEmpty() && _bagStackNew.Equals(_CompareInvNew) && !InvSlot.Contains(j))
                                            {
                                                InvSlot.Add(i);
                                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} {3} to their bag, identical to another stack, inside their own or ally claimed space @ {4} {5} {6}.", DateTime.Now, _cInfo.playerName, _bagStackNew.count, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                                else
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} {3} to their bag, identical to another stack, standing at {4} {5} {6}.", DateTime.Now, _cInfo.playerName, _bagStackNew.count, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < _invSize; i++)
                    {
                        bool InvNext = true;
                        _invStackOld = _invStacks[i];
                        _invStackNew = _inventory[i];
                        if (!_invStackNew.Equals(_invStackOld))
                        {
                            if (!_invStackNew.IsEmpty())
                            {
                                string _invName = _invStackNew.itemValue.ItemClass.Name;
                                int _oldTotal = 0, _newTotal = 0, _newCount;
                                for (int j = 0; j < _invSize; j++)
                                {
                                    _CompareInvOld = _invStacks[j];
                                    if (!_CompareInvOld.IsEmpty() && _invName == _CompareInvOld.itemValue.ItemClass.Name)
                                    {
                                        _newCount = _oldTotal + _CompareInvOld.count;
                                        _oldTotal = _newCount;
                                    }
                                }
                                for (int j = 0; j < _bagSize; j++)
                                {
                                    _CompareBagOld = _bagStacks[j];
                                    if (!_CompareBagOld.IsEmpty() && _invName == _CompareBagOld.itemValue.ItemClass.Name)
                                    {
                                        _newCount = _oldTotal + _CompareBagOld.count;
                                        _oldTotal = _newCount;
                                    }
                                }
                                for (int j = 0; j < _invSize; j++)
                                {
                                    _CompareInvNew = _inventory[j];
                                    if (!_CompareInvNew.IsEmpty() && _invName == _CompareInvNew.itemValue.ItemClass.Name)
                                    {
                                        _newCount = _newTotal + _CompareInvNew.count;
                                        _newTotal = _newCount;
                                    }
                                }
                                for (int j = 0; j < _bagSize; j++)
                                {
                                    _CompareBagNew = _bag[j];
                                    if (!_CompareBagNew.IsEmpty() && _invName == _CompareBagNew.itemValue.ItemClass.Name)
                                    {
                                        _newCount = _newTotal + _CompareBagNew.count;
                                        _newTotal = _newCount;
                                    }
                                }
                                if (_oldTotal == _newTotal)
                                {
                                    InvNext = false;
                                }
                                if (InvNext)
                                {
                                    if (_invStackNew.count == 1)
                                    {
                                        int _counter1 = 0, _counter2 = 0;
                                        for (int j = 0; j < _invSize; j++)
                                        {
                                            _CompareInvNew = _inventory[j];
                                            if (!_CompareInvNew.IsEmpty() && _invName == _CompareInvNew.itemValue.ItemClass.Name && _CompareInvNew.count == 1)
                                            {
                                                _counter1++;
                                            }
                                        }
                                        for (int j = 0; j < _bagSize; j++)
                                        {
                                            _CompareBagNew = _bag[j];
                                            if (!_CompareBagNew.IsEmpty() && _invName == _CompareBagNew.itemValue.ItemClass.Name && _CompareBagNew.count == 1)
                                            {
                                                _counter2++;
                                            }
                                        }
                                        if (_counter1 + _counter2 > 1)
                                        {
                                            for (int j = 0; j < _invSize; j++)
                                            {
                                                _CompareInvOld = _invStacks[j];
                                                if (!_CompareInvOld.IsEmpty() && _invName == _CompareInvOld.itemValue.ItemClass.Name && _CompareInvOld.count >= _counter1)
                                                {
                                                    InvNext = false;
                                                }
                                            }
                                            if (InvNext)
                                            {
                                                for (int j = 0; j < _bagSize; j++)
                                                {
                                                    _CompareBagOld = _bagStacks[j];
                                                    if (!_CompareBagOld.IsEmpty() && _invName == _CompareBagOld.itemValue.ItemClass.Name && _CompareBagOld.count >= _counter1)
                                                    {
                                                        InvNext = false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (InvNext)
                                {
                                    if (_invStackNew.itemValue.ItemClass.HasParts)
                                    {
                                        for (int j = 0; j < _invSize; j++)
                                        {
                                            _CompareInvNew = _inventory[j];
                                            if (!_CompareInvNew.IsEmpty() && _invStackNew.Equals(_CompareInvNew) && i != j && !InvSlot.Contains(j))
                                            {
                                                InvSlot.Add(i);
                                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality and parts to their inventory inside their own or ally claimed space @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                                else
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality and parts to their inventory, standing @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                            }
                                        }
                                        for (int j = 0; j < _bagSize; j++)
                                        {
                                            _CompareBagNew = _bag[j];
                                            if (!_CompareBagNew.IsEmpty() && (_invStackNew.Equals(_CompareBagNew)) && !InvSlot.Contains(j))
                                            {
                                                InvSlot.Add(i);
                                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality and parts to their inventory inside their own or ally claimed space @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                                else
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality and parts to their inventory, standing @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (_invStackNew.itemValue.HasQuality)
                                    {
                                        for (int j = 0; j < _invSize; j++)
                                        {
                                            _CompareInvNew = _inventory[j];
                                            if (!_CompareInvNew.IsEmpty() && _invStackNew.Equals(_CompareInvNew) && i != j && !InvSlot.Contains(j))
                                            {
                                                InvSlot.Add(i);
                                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality to their inventory inside their own or ally claimed space @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                                else
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality to their inventory, standing @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                            }
                                        }
                                        for (int j = 0; j < _bagSize; j++)
                                        {
                                            _CompareBagNew = _bag[j];
                                            if (!_CompareBagNew.IsEmpty() && _invStackNew.Equals(_CompareBagNew) && !InvSlot.Contains(j))
                                            {
                                                InvSlot.Add(i);
                                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality to their inventory inside their own or ally claimed space @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                                else
                                                {
                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0}: {1} has added {2} with identical quality to their inventory, standing @ {3} {4} {5}.", DateTime.Now, _cInfo.playerName, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (_invStackNew.count > 1)
                                        {
                                            for (int j = 0; j < _invSize; j++)
                                            {
                                                _CompareInvNew = _inventory[j];
                                                if (!_CompareInvNew.IsEmpty() && _invStackNew.Equals(_CompareInvNew) && i != j && !InvSlot.Contains(j))
                                                {
                                                    InvSlot.Add(i);
                                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                    PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                    EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                    if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                    {
                                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                                        {
                                                            sw.WriteLine(string.Format("{0}: {1} has added {2} {3} to their inventory, identical to another stack, inside their own or ally claimed space @ {4} {5} {6}.", DateTime.Now, _cInfo.playerName, _invStackNew.count, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                            sw.WriteLine();
                                                            sw.Flush();
                                                            sw.Close();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                                        {
                                                            sw.WriteLine(string.Format("{0}: {1} has added {2} {3} to their inventory, identical to another stack, standing at {4} {5} {6}.", DateTime.Now, _cInfo.playerName, _invStackNew.count, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                            sw.WriteLine();
                                                            sw.Flush();
                                                            sw.Close();
                                                        }
                                                    }
                                                }
                                            }
                                            for (int j = 0; j < _bagSize; j++)
                                            {
                                                _CompareBagNew = _bag[j];
                                                if (!_CompareBagNew.IsEmpty() && _invStackNew.Equals(_CompareBagNew) && !InvSlot.Contains(j))
                                                {
                                                    InvSlot.Add(i);
                                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                    PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                    EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                    if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                    {
                                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                                        {
                                                            sw.WriteLine(string.Format("{0}: {1} has added {2} {3} to their inventory, identical to another stack, inside their own or ally claimed space @ {4} {5} {6}.", DateTime.Now, _cInfo.playerName, _invStackNew.count, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                            sw.WriteLine();
                                                            sw.Flush();
                                                            sw.Close();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                                        {
                                                            sw.WriteLine(string.Format("{0}: {1} has added {2} {3} to their inventory, identical to another stack, standing @ {4} {5} {6}.", DateTime.Now, _cInfo.playerName, _invStackNew.count, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                            sw.WriteLine();
                                                            sw.Flush();
                                                            sw.Close();
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
                    Bag[_cInfo.entityId] = _bag;
                    Inventory[_cInfo.entityId] = _inventory;
                }
            }
        }
    }
}

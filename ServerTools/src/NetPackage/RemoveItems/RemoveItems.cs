using System;
using System.Linq;

namespace ServerTools
{
    public class NetPackageRemoveBagItem
    {
        public static void Exec(ClientInfo _cInfo, string _itemNameAndCount)
        {
            Log.Out("[SERVERTOOLS] Test 1");
            if (_cInfo != null && !string.IsNullOrEmpty(_itemNameAndCount))
            {
                try
                {
                    string _itemName = "";
                    int _count = 1;
                    if (_itemNameAndCount.Contains(" "))
                    {
                        string _amount = _itemNameAndCount.Split(' ').Last();
                        if (int.TryParse(_amount, out _count))
                        {
                            _itemName = _itemNameAndCount.Replace(" " + _amount, "");
                            Log.Out(string.Format("[SERVERTOOLS] _itemName: {0}", _itemName));
                            ItemClass _class = ItemClass.GetItemClass(_itemName, true);
                            Block _block = Block.GetBlockByName(_itemName, true);
                            if (_class == null && _block == null)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] invalid item to remove. Item not found: {0}", _itemName));
                                return;
                            }
                            else
                            {
                                Log.Out("[SERVERTOOLS] Test 2. Building local player");
                                EntityPlayerLocal _entityPlayerLocal = BuildLocalPlayer(_cInfo);
                                LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_entityPlayerLocal);
                                if (uiforPlayer != null)
                                {
                                    Log.Out("[SERVERTOOLS] Test 3. uiforPlayer is not null");
                                }
                                EntityPlayerLocal _localPlayerFromID = GameManager.Instance.World.GetLocalPlayerFromID(_cInfo.entityId);
                                if (_localPlayerFromID != null)
                                {
                                    Log.Out("[SERVERTOOLS] Test 3.5. _localPlayerFromID is not null");
                                }
                                Log.Out("[SERVERTOOLS] Test 4. Adjusting local player bag");
                                EntityPlayerLocal _entityPlayerLocalAdjusted = Adjust(_entityPlayerLocal, _itemName, _count);
                                Log.Out("[SERVERTOOLS] Test 5 Sending package");

                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerInventory>().Setup(_entityPlayerLocalAdjusted, false, true, false));
                                Log.Out("[SERVERTOOLS] Test 6 Operation complete");
                            }
                        }
                        else
                        {
                            Log.Out(string.Format("[SERVERTOOLS] you must input a valid item count. {0}", _amount));
                            return;
                        }
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] you must input a valid item name and count. {0}", _itemNameAndCount));
                        return;
                    }
                    
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[ServerTools] Failed to run ", e.Message));
                }
            }
        }

        public static EntityPlayerLocal BuildLocalPlayer(ClientInfo _cInfo)
        {
            Log.Out("[SERVERTOOLS] Building local player");
            EntityPlayer _entityPlayer = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            EntityPlayerLocal _entityPlayerLocal = new EntityPlayerLocal();
            _entityPlayerLocal.clientEntityId = _entityPlayer.clientEntityId;
            _entityPlayerLocal.belongsPlayerId = _entityPlayer.belongsPlayerId;
            _entityPlayerLocal.inventory = _entityPlayer.inventory;
            _entityPlayerLocal.bag = _entityPlayer.bag;
            _entityPlayerLocal.equipment = _entityPlayer.equipment;
            _entityPlayerLocal.persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_entityPlayer.entityId);
            return _entityPlayerLocal;
        }

        public static EntityPlayerLocal Adjust(EntityPlayerLocal _entityPlayerLocal, string _itemName, int _itemCount)
        {
            Log.Out("[SERVERTOOLS] Adjusting local player bag");
            Log.Out(string.Format("[ST] _itemName = {0}, _itemCount = {1}", _itemName, _itemCount));
            ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_itemName).type, 1, 1, false, null, 1);
            if (_itemValue != null)
            {
                Log.Out("[SERVERTOOLS] _itemValue not null");
                int _count = _itemCount;
                ItemStack[] bagSlots = _entityPlayerLocal.bag.GetSlots();
                for (int i = 0; i < bagSlots.Length; i++)
                {
                    Log.Out(string.Format("[ST] bagSlots[i].itemValue.GetItemOrBlockId() = {0}, _itemValue.GetItemOrBlockId() = {1}", bagSlots[i].itemValue.GetItemOrBlockId(), _itemValue.GetItemOrBlockId()));
                    if (_count != 0 && !bagSlots[i].IsEmpty() && bagSlots[i].itemValue.GetItemOrBlockId() == _itemValue.GetItemOrBlockId())
                    {
                        if (bagSlots[i].count <= _count)
                        {
                            int _newCount = _count - bagSlots[i].count;
                            Log.Out(string.Format("[ST] _count = {0}, bagSlots[i].count = {1}, _newCount = {2}", _count, bagSlots[i].count, _newCount));
                            _count = _newCount;
                            bagSlots[i] = ItemStack.Empty.Clone();
                        }
                        else
                        {
                            int _newCount = _count - bagSlots[i].count;
                            Log.Out(string.Format("[ST] _count = {0}, bagSlots[i].count = {1}, _newCount = {2}", _count, bagSlots[i].count, _newCount));
                            _count = 0;
                            ItemStack _stack = new ItemStack(bagSlots[i].itemValue, _newCount);
                            bagSlots[i] = _stack.Clone();
                        }
                    }
                }
                _entityPlayerLocal.bag.SetSlots(bagSlots);
                Log.Out("[SERVERTOOLS] Completed adjustment");
            }
            return _entityPlayerLocal;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace ServerTools
{
    class PlayerLogs
    {
        private static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        public static bool Position = false;
        public static bool Inventory = false;
        public static int Interval = 60;
        private static System.Timers.Timer timerLogs = new System.Timers.Timer();

        public static void PlayerLogsDir()
        {
            if (!Directory.Exists(API.GamePath + "/PlayerLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/PlayerLogs");
            }
        }

        public static void PlayerLogsStart()
        {
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {
                int d = (Interval * 1000);
                timerLogs.Interval = d;
                timerLogs.Start();
                timerLogs.Elapsed += new ElapsedEventHandler(Move_Log);
                timerLogs.Elapsed += new ElapsedEventHandler(PlayerInvLog);
                timerLogs.Elapsed += new ElapsedEventHandler(Move_Log_PlayerInvLog);
            }
        }

        public static void PlayerLogsStop()
        {
            timerLogs.Stop();
        }

        public static void Move_Log(object sender, ElapsedEventArgs e)
        {
            if (Position && !Inventory)
            {
                if (ConnectionManager.Instance.ClientCount() > 0)
                {
                    World world = GameManager.Instance.World;
                    var enumerator = world.Entities.list;
                    foreach (var ent in enumerator)
                    {
                        if (ent.IsClientControlled())
                        {
                            var x = (int)ent.position.x;
                            var y = (int)ent.position.y;
                            var z = (int)ent.position.z;

                            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                            foreach (var _cInfo in _cInfoList)
                            {
                                if (ent.entityId == _cInfo.entityId)
                                {
                                    string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                    string _filepath = string.Format("{0}/PlayerLogs/{1}", API.GamePath, _file);

                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("{0} {1} steamId {2} Position: {3} X {4} Y {5} Z", DateTime.Now, _cInfo.playerName, _cInfo.playerId, x, y, z));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    PersistentPlayerData lpRelative = null;
                                    ent.world.GetLandClaimOwner(new Vector3i(x, y, z), lpRelative);
                                    }
                            }
                        }
                    }
                }
            }
        }

        public static void PlayerInvLog(object sender, ElapsedEventArgs e)
        {
            if (Inventory && !Position)
            {
                if (ConnectionManager.Instance.ClientCount() > 0)
                {
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                    foreach (var _cInfo in _cInfoList)
                    {
                        AllocsFixes.PersistentData.Player p = AllocsFixes.PersistentData.PersistentContainer.Instance.Players[_cInfo.playerId, false];
                        AllocsFixes.PersistentData.Inventory inv = p.Inventory;
                        string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                        string _filepath = string.Format("{0}/PlayerLogs/{1}", API.GamePath, _file);
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: " + "Inventory of " + p.Name + " steamId {1}", DateTime.Now, _cInfo.playerId));
                            sw.WriteLine("Belt:");
                            sw.Flush();
                            sw.Close();
                        }
                        PrintInv(inv.belt, p.EntityID, "belt");
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine("Bagpack:");
                            sw.Flush();
                            sw.Close();
                        }
                        PrintInv(inv.bag, p.EntityID, "backpack");
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine("Equipment:");
                            sw.Flush();
                            sw.Close();
                        }
                        PrintEquipment(inv.equipment, p.EntityID, "equipment");
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine("End of inventory");
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
            }
        }

        private static void PrintInv(List<AllocsFixes.PersistentData.InvItem> _inv, int _entityId, string _location)
        {
            for (int i = 0; i < _inv.Count; i++)
            {
                if (_inv[i] != null)
                {
                    if (_inv[i].quality < 0)
                    {
                        string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                        string _filepath = string.Format("{0}/PlayerLogs/{1}", API.GamePath, _file);
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine(string.Format("    Slot {0}: {1:000} * {2}", i, _inv[i].count, _inv[i].itemName));
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    else
                    {
                        string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                        string _filepath = string.Format("{0}/PlayerLogs/{1}", API.GamePath, _file);
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine(string.Format("    Slot {0}: {1:000} * {2} - quality: {3}", i, _inv[i].count, _inv[i].itemName, _inv[i].quality));
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    DoParts(_inv[i].parts, 1, null);
                }
            }
        }

        private static void PrintEquipment(AllocsFixes.PersistentData.InvItem[] _equipment, int _entityId, string _location)
        {
            AddEquipment("head", _equipment, EquipmentSlots.Headgear, _entityId, _location);
            AddEquipment("eyes", _equipment, EquipmentSlots.Eyewear, _entityId, _location);
            AddEquipment("face", _equipment, EquipmentSlots.Face, _entityId, _location);

            AddEquipment("armor", _equipment, EquipmentSlots.ChestArmor, _entityId, _location);
            AddEquipment("jacket", _equipment, EquipmentSlots.Jacket, _entityId, _location);
            AddEquipment("shirt", _equipment, EquipmentSlots.Shirt, _entityId, _location);

            AddEquipment("legarmor", _equipment, EquipmentSlots.LegArmor, _entityId, _location);
            AddEquipment("pants", _equipment, EquipmentSlots.Legs, _entityId, _location);
            AddEquipment("boots", _equipment, EquipmentSlots.Feet, _entityId, _location);

            AddEquipment("gloves", _equipment, EquipmentSlots.Hands, _entityId, _location);
        }

        private static void AddEquipment(string _slotname, AllocsFixes.PersistentData.InvItem[] _items, EquipmentSlots _slot, int _entityId, string _location)
        {
            int[] slotindices = XUiM_PlayerEquipment.GetSlotIndicesByEquipmentSlot(_slot);

            for (int i = 0; i < slotindices.Length; i++)
            {
                if (_items != null && _items[slotindices[i]] != null)
                {
                    AllocsFixes.PersistentData.InvItem item = _items[slotindices[i]];
                    if (item.quality < 0)
                    {
                        string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                        string _filepath = string.Format("{0}/PlayerLogs/{1}", API.GamePath, _file);
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine(string.Format("    Slot {0:8}: {1:000}", _slotname, item.itemName));
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    else
                    {
                        string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                        string _filepath = string.Format("{0}/PlayerLogs/{1}", API.GamePath, _file);
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine(string.Format("    Slot {0:8}: {1:000} - quality: {2}", _slotname, item.itemName, item.quality));
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    DoParts(_items[slotindices[i]].parts, 1, null);
                    return;
                }
            }
        }

        private static string DoParts(AllocsFixes.PersistentData.InvItem[] _parts, int _indent, string _currentMessage)
        {
            if (_parts != null && _parts.Length > 0)
            {
                string indenter = new string(' ', _indent * 4);
                for (int i = 0; i < _parts.Length; i++)
                {
                    if (_parts[i] != null)
                    {
                        if (_currentMessage == null)
                        { // no currentMessage given -> readable output
                            if (_parts[i].quality < 0)
                            {
                                string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                string _filepath = string.Format("{0}/PlayerLogs/{1}", API.GamePath, _file);
                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}         - {1}", indenter, _parts[i].itemName));
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                            else
                            {
                                string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                string _filepath = string.Format("{0}/PlayerLogs/{1}", API.GamePath, _file);
                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}         - {1} - quality: {2}", indenter, _parts[i].itemName, _parts[i].quality));
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                            DoParts(_parts[i].parts, _indent + 1, _currentMessage);
                        }
                        else
                        { // currentMessage given -> parseable output
                            if (_currentMessage.Length > 0)
                            {
                                _currentMessage += ",";
                            }
                            _currentMessage += _parts[i].itemName + "@" + _parts[i].quality;
                            _currentMessage = DoParts(_parts[i].parts, _indent + 1, _currentMessage);
                        }
                    }
                }
            }
            return _currentMessage;
        }

        public static void Move_Log_PlayerInvLog(object sender, ElapsedEventArgs e)
        {
            if (Position && Inventory)
            {
                if (ConnectionManager.Instance.ClientCount() > 0)
                {
                    World world = GameManager.Instance.World;
                    var enumerator = world.Entities.list;
                    foreach (var ent in enumerator)
                    {
                        if (ent.IsClientControlled())
                        {
                            var x = (int)ent.position.x;
                            var y = (int)ent.position.y;
                            var z = (int)ent.position.z;

                            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                            foreach (var _cInfo in _cInfoList)
                            {
                                if (ent.entityId == _cInfo.entityId)
                                {
                                    string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                    string _filepath = string.Format("{0}/PlayerLogs/{1}", API.GamePath, _file);
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("{0} {1} steamId {2} Position: {3} X {4} Y {5} Z", DateTime.Now, _cInfo.playerName, _cInfo.playerId, x, y, z));
                                        sw.Flush();
                                        sw.Close();
                                    }

                                    AllocsFixes.PersistentData.Player p = AllocsFixes.PersistentData.PersistentContainer.Instance.Players[_cInfo.playerId, false];
                                    AllocsFixes.PersistentData.Inventory inv = p.Inventory;

                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("{0}: " + "Inventory of " + p.Name, DateTime.Now));
                                        sw.WriteLine("Belt:");
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    PrintInv(inv.belt, p.EntityID, "belt");
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine("Bagpack:");
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    PrintInv(inv.bag, p.EntityID, "backpack");
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine("Equipment:");
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    PrintEquipment(inv.equipment, p.EntityID, "equipment");
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine("End of inventory");
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

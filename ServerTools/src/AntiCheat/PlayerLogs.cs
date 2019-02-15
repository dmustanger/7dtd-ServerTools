using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class PlayerLogs
    {
        public static bool IsEnabled = false, Position = false, Inventory = false, P_Data = false;
        public static int Days_Before_Log_Delete = 5;
        private static string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/Logs/PlayerLogs/{1}", API.ConfigPath, _file);

        public static void Exec()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                World world = GameManager.Instance.World;
                List<EntityPlayer> PlayerList = world.Players.list;
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    EntityPlayer _player = PlayerList[i];
                    if (_player != null)
                    {
                        ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_player.entityId);
                        if (_cInfo != null)
                        {
                            if (Position)
                            {
                                var x = (int)_player.position.x;
                                var y = (int)_player.position.y;
                                var z = (int)_player.position.z;
                                var region_x = x / 512;
                                if (x < 0)
                                {
                                    region_x--;
                                }
                                var region_z = z / 512;
                                if (z < 0)
                                {
                                    region_z--;
                                }
                                string _ip = _cInfo.ip;
                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}  {1} steamId {2} IP Address {3} at Position: {4} X {5} Y {6} Z in RegionFile: r.{7}.{8}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, _ip, x, y, z, region_x, region_z));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                            if (Inventory)
                            {
                                AllocsFixes.PersistentData.Player p = AllocsFixes.PersistentData.PersistentContainer.Instance.Players[_cInfo.playerId, false];
                                AllocsFixes.PersistentData.Inventory inv = p.Inventory;
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
                                    sw.WriteLine("Backpack:");
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
                            if (P_Data)
                            {
                                if (_player.IsDead())
                                {
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("{0}: " + "SteamId {1}. {2} is currently dead", DateTime.Now, _cInfo.playerId, _cInfo.playerName));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                }
                                if (_player.IsSpawned())
                                {
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("{0}: " + "SteamId {1}. {2} stats: Health= {3} Stamina= {4} ZombieKills= {5} PlayerKills= {6}", DateTime.Now,
                                            _cInfo.playerId, _cInfo.playerName, _player.Stats.Health.Value, _player.Stats.Stamina.Value, _player.KilledZombies, _player.KilledPlayers));
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

        private static void PrintInv(List<AllocsFixes.PersistentData.InvItem> _inv, int _entityId, string _location)
        {
            for (int i = 0; i < _inv.Count; i++)
            {
                if (_inv[i] != null)
                {
                    if (_inv[i].quality < 0)
                    {
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine(string.Format("    Slot {0}: {1:000} * {2},", i, _inv[i].count, _inv[i].itemName));
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine(string.Format("    Slot {0}: {1:000} * {2} - quality: {3},", i, _inv[i].count, _inv[i].itemName, _inv[i].quality));
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
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine(string.Format("    Slot {0:8}: {1:000},", _slotname, item.itemName));
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine(string.Format("    Slot {0:8}: {1:000} - quality: {2},", _slotname, item.itemName, item.quality));
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
                        {
                            if (_parts[i].quality < 0)
                            {
                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}         - {1},", indenter, _parts[i].itemName));
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                            else
                            {
                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}         - {1} - quality: {2},", indenter, _parts[i].itemName, _parts[i].quality));
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                            DoParts(_parts[i].parts, _indent + 1, _currentMessage);
                        }
                        else
                        {
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
    }
}

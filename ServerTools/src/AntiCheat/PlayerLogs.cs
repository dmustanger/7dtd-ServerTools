using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class PlayerLogs
    {
        public static bool IsEnabled = false, Vehicle = false;
        public static int Delay = 60, Days_Before_Log_Delete = 5;
        private static readonly string file = string.Format("PlayerLog_{0}.xml", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string FilePath = string.Format("{0}/Logs/PlayerLogs/{1}", API.ConfigPath, file);

        public static void Exec()
        {
            try
            {
                if (GameManager.Instance.World != null && GameManager.Instance.World.Players.Count > 0)
                {
                    if (!Utils.FileExists(FilePath))
                    {
                        using (StreamWriter sw = new StreamWriter(FilePath, true, Encoding.UTF8))
                        {
                            sw.WriteLine("<PlayerLogs/>");
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    if (Utils.FileExists(FilePath))
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        try
                        {
                            xmlDoc.Load(FilePath);
                        }
                        catch (XmlException e)
                        {
                            Log.Error(string.Format("[SERVERTOOLS] Player logs failed loading {0}: {1}", file, e.Message));
                            return;
                        }
                        string _dt = DateTime.Now.ToString("HH:mm:ss");
                        PlayerDataFile _playerDataFile = new PlayerDataFile();
                        List<EntityPlayer> _playerList = GameManager.Instance.World.Players.list;
                        for (int i = 0; i < _playerList.Count; i++)
                        {
                            EntityPlayer _player = _playerList[i];
                            if (_player != null)
                            {
                                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_player.entityId);
                                if (_cInfo != null)
                                {
                                    _playerDataFile.Load(GameUtils.GetPlayerDataDir(), _cInfo.playerId);
                                    if (_playerDataFile != null)
                                    {
                                        XmlNode _playerNode = null;
                                        XmlNodeList _playerNodeList = xmlDoc.GetElementsByTagName("Player", "SteamId " + _cInfo.playerId);
                                        if (_playerNodeList == null || _playerNodeList.Count == 0)
                                        {
                                            _playerNode = xmlDoc.CreateNode(XmlNodeType.Element, "Player", "SteamId " + _cInfo.playerId);
                                            xmlDoc.DocumentElement.AppendChild(_playerNode);
                                        }
                                        else
                                        {
                                            _playerNode = _playerNodeList[0];
                                        }
                                        if (_playerNode != null)
                                        {
                                            if (_player.IsSpawned())
                                            {
                                                var _x = (int)_player.position.x;
                                                var _y = (int)_player.position.y;
                                                var _z = (int)_player.position.z;
                                                double _regionX, _regionZ;
                                                if (_player.position.x < 0)
                                                {
                                                    _regionX = Math.Truncate(_player.position.x / 512) - 1;
                                                }
                                                else
                                                {
                                                    _regionX = Math.Truncate(_player.position.x / 512);
                                                }
                                                if (_player.position.z < 0)
                                                {
                                                    _regionZ = Math.Truncate(_player.position.z / 512) - 1;
                                                }
                                                else
                                                {
                                                    _regionZ = Math.Truncate(_player.position.z / 512);
                                                }
                                                string _ip = _cInfo.ip;
                                                XmlNode _newEntry = xmlDoc.CreateTextNode("\n");
                                                _playerNode.AppendChild(_newEntry);
                                                _newEntry = xmlDoc.CreateTextNode(string.Format("       {0}: EntityId {1} / Name {2} / IP Address {3} / Position {4} X {5} Y {6} Z / RegionFile r.{7}.{8}.7rg\n", _dt, _cInfo.entityId, _cInfo.playerName, _ip, _x, _y, _z, _regionX, _regionZ));
                                                _playerNode.AppendChild(_newEntry);
                                                _newEntry = xmlDoc.CreateTextNode(string.Format("       Health {0} / Stamina {1} / ZombieKills {2} / PlayerKills {3} / PlayerLevel {4}\n", (int)_player.Stats.Health.Value, (int)_player.Stats.Stamina.Value, _player.KilledZombies, _player.KilledPlayers, _player.Progression.GetLevel()));
                                                _playerNode.AppendChild(_newEntry);
                                                _newEntry = xmlDoc.CreateTextNode("       Belt:\n");
                                                _playerNode.AppendChild(_newEntry);
                                                _playerNode = PrintInv(_playerDataFile.inventory, _playerNode, xmlDoc);
                                                _newEntry = xmlDoc.CreateTextNode("       Backpack:\n");
                                                _playerNode.AppendChild(_newEntry);
                                                _playerNode = PrintInv(_playerDataFile.bag, _playerNode, xmlDoc);
                                                _newEntry = xmlDoc.CreateTextNode("       Equipment:\n");
                                                _playerNode.AppendChild(_newEntry);
                                                _playerNode = PrintEquipment(_playerDataFile.equipment, _playerNode, xmlDoc);
                                                if (Vehicle && _player.AttachedToEntity != null)
                                                {
                                                    _newEntry = xmlDoc.CreateTextNode("       Vehicle:\n");
                                                    _playerNode.AppendChild(_newEntry);
                                                    _playerNode = PrintVehicle(_player.AttachedToEntity.entityId, _playerNode, xmlDoc);
                                                }
                                                _newEntry = xmlDoc.CreateTextNode("----------------\n");
                                                _playerNode.AppendChild(_newEntry);
                                            }
                                            else if (!_player.IsDead() && !_player.IsSpawned())
                                            {
                                                XmlNode _newEntry = xmlDoc.CreateTextNode(string.Format("       {0}: EntityId {1} / Name {2} / Player has not spawned\n", _dt, _cInfo.entityId, _cInfo.playerName));
                                                _playerNode.AppendChild(_newEntry);
                                                _newEntry = xmlDoc.CreateTextNode("----------------\n");
                                                _playerNode.AppendChild(_newEntry);
                                            }
                                            else if (_player.IsDead())
                                            {
                                                XmlNode _newEntry = xmlDoc.CreateTextNode(string.Format("       {0}: EntityId {1} / Name {2} / Player is dead\n", _dt, _cInfo.entityId, _cInfo.playerName));
                                                _playerNode.AppendChild(_newEntry);
                                                _newEntry = xmlDoc.CreateTextNode("----------------\n");
                                                _playerNode.AppendChild(_newEntry);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        xmlDoc.Save(FilePath);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerLogs.Exec: {0}.", e.Message));
            }
        }

        private static XmlNode PrintInv(ItemStack[] _inv, XmlNode _playerNode, XmlDocument _xmlDoc)
        {
            for (int i = 0; i < _inv.Length; i++)
            {
                if (!_inv[i].IsEmpty())
                {
                    if (_inv[i].itemValue.HasQuality && _inv[i].itemValue.Quality > 0)
                    {
                        XmlNode _newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1:000} * {2} - quality: {3}\n", i, _inv[i].count, _inv[i].itemValue.ItemClass.GetItemName(), _inv[i].itemValue.Quality));
                        _playerNode.AppendChild(_newEntry);
                    }
                    else
                    {
                        XmlNode _newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1:000} * {2}\n", i, _inv[i].count, _inv[i].itemValue.ItemClass.GetItemName()));
                        _playerNode.AppendChild(_newEntry);
                    }
                    if (_inv[i].itemValue.Modifications != null && _inv[i].itemValue.Modifications.Length > 0)
                    {
                        _playerNode = Mods(_inv[i].itemValue.Modifications, 1, _playerNode, _xmlDoc);
                    }
                    if (_inv[i].itemValue.CosmeticMods != null && _inv[i].itemValue.CosmeticMods.Length > 0)
                    {
                        _playerNode = CosmeticMods(_inv[i].itemValue.CosmeticMods, 1, _playerNode, _xmlDoc);
                    }
                }
            }
            return _playerNode;
        }

        private static XmlNode PrintEquipment(Equipment _equipment, XmlNode _playerNode, XmlDocument _xmlDoc)
        {
            for (int i = 0; i < _equipment.GetSlotCount(); i++)
            {
                ItemValue _item = _equipment.GetSlotItem(i);
                if (_item != null && !_item.IsEmpty())
                {
                    if (_item.HasQuality && _item.Quality > 0)
                    {
                        XmlNode _newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1} - quality: {2}\n", _item.ItemClass.EquipSlot, _item.ItemClass.GetItemName(), _item.Quality));
                        _playerNode.AppendChild(_newEntry);
                    }
                    else
                    {
                        XmlNode _newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1}\n", _item.ItemClass.EquipSlot, _item.ItemClass.GetItemName()));
                        _playerNode.AppendChild(_newEntry);
                    }
                    if (_item.Modifications != null && _item.Modifications.Length > 0)
                    {
                        Mods(_item.Modifications, 1, _playerNode, _xmlDoc);
                    }
                    if (_item.CosmeticMods != null && _item.CosmeticMods.Length > 0)
                    {
                        CosmeticMods(_item.CosmeticMods, 1, _playerNode, _xmlDoc);
                    }
                }
            }
            return _playerNode;
        }

        private static XmlNode Mods(ItemValue[] _parts, int _indent, XmlNode _playerNode, XmlDocument _xmlDoc)
        {
            if (_parts != null && _parts.Length > 0)
            {
                string indenter = new string(' ', _indent * 4);
                for (int i = 0; i < _parts.Length; i++)
                {
                    if (_parts[i] != null && !_parts[i].IsEmpty())
                    {
                        XmlNode _newEntry = _xmlDoc.CreateTextNode(string.Format("{0}         - {1}\n", indenter, _parts[i].ItemClass.GetItemName()));
                        _playerNode.AppendChild(_newEntry);
                    }
                }
            }
            return _playerNode;
        }

        private static XmlNode CosmeticMods(ItemValue[] _parts, int _indent, XmlNode _playerNode, XmlDocument _xmlDoc)
        {
            if (_parts != null && _parts.Length > 0)
            {
                string indenter = new string(' ', _indent * 4);
                for (int i = 0; i < _parts.Length; i++)
                {
                    if (_parts[i] != null && !_parts[i].IsEmpty())
                    {
                        XmlNode _newEntry = _xmlDoc.CreateTextNode(string.Format("{0}         - {1}\n", indenter, _parts[i].ItemClass.GetItemName()));
                        _playerNode.AppendChild(_newEntry);
                    }
                }
            }
            return _playerNode;
        }

        private static XmlNode PrintVehicle(int _vehicleId, XmlNode _playerNode, XmlDocument _xmlDoc)
        {
            EntityVehicle _vehicle = (EntityVehicle)PersistentOperations.GetEntity(_vehicleId);
            if (_vehicle != null)
            {
                XmlNode _newEntry = _xmlDoc.CreateTextNode(string.Format("       Id {0} / Health {1} / Speed {2}\n", _vehicle.entityId, _vehicle.Health, (int)_vehicle.speedForward));
                _playerNode.AppendChild(_newEntry);
                ItemStack[] _inv = _vehicle.bag.GetSlots();
                for (int i = 0; i < _inv.Length; i++)
                {
                    if (!_inv[i].IsEmpty())
                    {
                        if (_inv[i].itemValue.HasQuality && _inv[i].itemValue.Quality > 0)
                        {
                            _newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1:000} * {2} - quality: {3}\n", i, _inv[i].count, _inv[i].itemValue.ItemClass.GetItemName(), _inv[i].itemValue.Quality));
                            _playerNode.AppendChild(_newEntry);
                        }
                        else
                        {
                            _newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1:000} * {2}\n", i, _inv[i].count, _inv[i].itemValue.ItemClass.GetItemName()));
                            _playerNode.AppendChild(_newEntry);
                        }
                        if (_inv[i].itemValue.Modifications != null && _inv[i].itemValue.Modifications.Length > 0)
                        {
                            _playerNode = Mods(_inv[i].itemValue.Modifications, 1, _playerNode, _xmlDoc);
                        }
                        if (_inv[i].itemValue.CosmeticMods != null && _inv[i].itemValue.CosmeticMods.Length > 0)
                        {
                            _playerNode = CosmeticMods(_inv[i].itemValue.CosmeticMods, 1, _playerNode, _xmlDoc);
                        }
                    }
                }
            }
            return _playerNode;
        }
    }
}

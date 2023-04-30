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
        public static int Days_Before_Log_Delete = 5;
        public static string Delay = "120";

        private static string EventDelay = "";
        private static readonly string file = string.Format("PlayerLog_{0}.xml", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string FilePath = string.Format("{0}/Logs/PlayerLogs/{1}", API.ConfigPath, file);

        public static void SetDelay(bool _reset, bool _first)
        {
            if (EventDelay != Delay)
            {
                if (!EventSchedule.Expired.Contains("PlayerLogs"))
                {
                    EventSchedule.Expired.Add("PlayerLogs");
                }
                EventDelay = Delay;
                _reset = true;
            }
            if (_reset || _first)
            {
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        string[] timeSplit = times[i].Split(':');
                        int.TryParse(timeSplit[0], out int hours);
                        int.TryParse(timeSplit[1], out int minutes);
                        DateTime time = DateTime.Today.AddHours(hours).AddMinutes(minutes);
                        if (DateTime.Now < time)
                        {
                            EventSchedule.AddToSchedule("PlayerLogs_", time);
                        }
                        else
                        {
                            time = DateTime.Today.AddDays(1).AddHours(hours).AddMinutes(minutes);
                            EventSchedule.AddToSchedule("PlayerLogs_", time);
                        }
                    }
                }
                else if (Delay.Contains(":"))
                {
                    string[] timeSplit = Delay.Split(':');
                    int.TryParse(timeSplit[0], out int hours);
                    int.TryParse(timeSplit[1], out int minutes);
                    DateTime time = DateTime.Today.AddHours(hours).AddMinutes(minutes);
                    if (DateTime.Now < time)
                    {
                        EventSchedule.AddToSchedule("PlayerLogs_", time);
                    }
                    else
                    {
                        time = DateTime.Today.AddDays(1).AddHours(hours).AddMinutes(minutes);
                        EventSchedule.AddToSchedule("PlayerLogs_", time);
                    }
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        DateTime time = DateTime.Now.AddMinutes(delay);
                        EventSchedule.AddToSchedule("PlayerLogs_", time);
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Invalid Player_Logs Interval detected. Use a single integer, 24h time or multiple 24h time entries"));
                        Log.Out(string.Format("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00"));
                    }
                }
            }
        }

        public static void Exec()
        {
            try
            {
                if (GameManager.Instance.World != null && GameManager.Instance.World.Players.Count > 0)
                {
                    if (!File.Exists(FilePath))
                    {
                        using (StreamWriter sw = new StreamWriter(FilePath, true, Encoding.UTF8))
                        {
                            sw.WriteLine("<PlayerLogs/>");
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    if (File.Exists(FilePath))
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
                        string dt = DateTime.Now.ToString("HH:mm:ss");
                        PlayerDataFile playerDataFile;
                        EntityPlayer player;
                        ClientInfo cInfo;
                        List<EntityPlayer> playerList = GameManager.Instance.World.Players.list;
                        for (int i = 0; i < playerList.Count; i++)
                        {
                            player = playerList[i];
                            if (player != null)
                            {
                                cInfo = GeneralOperations.GetClientInfoFromEntityId(player.entityId);
                                if (cInfo != null)
                                {
                                    playerDataFile = cInfo.latestPlayerData;
                                    if (playerDataFile != null)
                                    {
                                        XmlNode playerNode = null;
                                        XmlNodeList playerNodeList = xmlDoc.GetElementsByTagName("Player", "Id " + cInfo.CrossplatformId.CombinedString);
                                        if (playerNodeList == null || playerNodeList.Count == 0)
                                        {
                                            playerNode = xmlDoc.CreateNode(XmlNodeType.Element, "Player", "Id " + cInfo.CrossplatformId.CombinedString);
                                            xmlDoc.DocumentElement.AppendChild(playerNode);
                                        }
                                        else
                                        {
                                            playerNode = playerNodeList[0];
                                        }
                                        if (playerNode != null)
                                        {
                                            if (player.IsSpawned())
                                            {
                                                var x = (int)player.position.x;
                                                var y = (int)player.position.y;
                                                var z = (int)player.position.z;
                                                double regionX, regionZ;
                                                regionX = Math.Truncate(player.position.x / 512);
                                                if (player.position.x < 0)
                                                {
                                                    regionX -= 1;
                                                }
                                                regionZ = Math.Truncate(player.position.z / 512);
                                                if (player.position.z < 0)
                                                {
                                                    regionZ -= 1;
                                                }
                                                string ip = cInfo.ip;
                                                XmlNode newEntry = xmlDoc.CreateTextNode("\n");
                                                playerNode.AppendChild(newEntry);
                                                newEntry = xmlDoc.CreateTextNode(string.Format("       {0}: EntityId {1} / Name {2} / IP Address {3} / Position {4} X {5} Y {6} Z / RegionFile r.{7}.{8}.7rg\n", dt, cInfo.entityId, cInfo.playerName, ip, x, y, z, regionX, regionZ));
                                                playerNode.AppendChild(newEntry);
                                                newEntry = xmlDoc.CreateTextNode(string.Format("       Health {0} / Stamina {1} / ZombieKills {2} / PlayerKills {3} / PlayerLevel {4} / Deaths {5}\n", (int)player.Stats.Health.Value, (int)player.Stats.Stamina.Value, player.KilledZombies, player.KilledPlayers, player.Progression.GetLevel(), player.Died));
                                                playerNode.AppendChild(newEntry);
                                                newEntry = xmlDoc.CreateTextNode("       Belt:\n");
                                                playerNode.AppendChild(newEntry);
                                                playerNode = PrintInv(playerDataFile.inventory, playerNode, xmlDoc);
                                                newEntry = xmlDoc.CreateTextNode("       Backpack:\n");
                                                playerNode.AppendChild(newEntry);
                                                playerNode = PrintInv(playerDataFile.bag, playerNode, xmlDoc);
                                                newEntry = xmlDoc.CreateTextNode("       Equipment:\n");
                                                playerNode.AppendChild(newEntry);
                                                playerNode = PrintEquipment(playerDataFile.equipment, playerNode, xmlDoc);
                                                if (Vehicle && player.AttachedToEntity != null)
                                                {
                                                    newEntry = xmlDoc.CreateTextNode("       Vehicle:\n");
                                                    playerNode.AppendChild(newEntry);
                                                    playerNode = PrintVehicle(player.AttachedToEntity.entityId, playerNode, xmlDoc);
                                                }
                                                newEntry = xmlDoc.CreateTextNode("----------------\n");
                                                playerNode.AppendChild(newEntry);
                                            }
                                            else if (!player.IsDead() && !player.IsSpawned())
                                            {
                                                XmlNode newEntry = xmlDoc.CreateTextNode(string.Format("       {0}: EntityId {1} / Name {2} / Player has not spawned\n", dt, cInfo.entityId, cInfo.playerName));
                                                playerNode.AppendChild(newEntry);
                                                newEntry = xmlDoc.CreateTextNode("----------------\n");
                                                playerNode.AppendChild(newEntry);
                                            }
                                            else if (player.IsDead())
                                            {
                                                XmlNode newEntry = xmlDoc.CreateTextNode(string.Format("       {0}: EntityId {1} / Name {2} / Player is dead\n", dt, cInfo.entityId, cInfo.playerName));
                                                playerNode.AppendChild(newEntry);
                                                newEntry = xmlDoc.CreateTextNode("----------------\n");
                                                playerNode.AppendChild(newEntry);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerLogs.Exec: {0}", e.Message));
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
                        XmlNode newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1:000} * {2} - quality: {3}\n", i, _inv[i].count, _inv[i].itemValue.ItemClass.GetItemName(), _inv[i].itemValue.Quality));
                        _playerNode.AppendChild(newEntry);
                    }
                    else
                    {
                        XmlNode newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1:000} * {2}\n", i, _inv[i].count, _inv[i].itemValue.ItemClass.GetItemName()));
                        _playerNode.AppendChild(newEntry);
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
                ItemValue item = _equipment.GetSlotItem(i);
                if (item != null && !item.IsEmpty())
                {
                    if (item.HasQuality && item.Quality > 0)
                    {
                        XmlNode newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1} - quality: {2}\n", item.ItemClass.EquipSlot, item.ItemClass.GetItemName(), item.Quality));
                        _playerNode.AppendChild(newEntry);
                    }
                    else
                    {
                        XmlNode newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1}\n", item.ItemClass.EquipSlot, item.ItemClass.GetItemName()));
                        _playerNode.AppendChild(newEntry);
                    }
                    if (item.Modifications != null && item.Modifications.Length > 0)
                    {
                        Mods(item.Modifications, 1, _playerNode, _xmlDoc);
                    }
                    if (item.CosmeticMods != null && item.CosmeticMods.Length > 0)
                    {
                        CosmeticMods(item.CosmeticMods, 1, _playerNode, _xmlDoc);
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
                        XmlNode newEntry = _xmlDoc.CreateTextNode(string.Format("{0}         - {1}\n", indenter, _parts[i].ItemClass.GetItemName()));
                        _playerNode.AppendChild(newEntry);
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
                        XmlNode newEntry = _xmlDoc.CreateTextNode(string.Format("{0}         - {1}\n", indenter, _parts[i].ItemClass.GetItemName()));
                        _playerNode.AppendChild(newEntry);
                    }
                }
            }
            return _playerNode;
        }

        private static XmlNode PrintVehicle(int _vehicleId, XmlNode _playerNode, XmlDocument _xmlDoc)
        {
            EntityVehicle vehicle = (EntityVehicle)GeneralOperations.GetEntity(_vehicleId);
            if (vehicle != null)
            {
                XmlNode newEntry = _xmlDoc.CreateTextNode(string.Format("       Id {0} / Health {1} / Speed {2}\n", vehicle.entityId, vehicle.Health, (int)vehicle.speedForward));
                _playerNode.AppendChild(newEntry);
                ItemStack[] _inv = vehicle.bag.GetSlots();
                for (int i = 0; i < _inv.Length; i++)
                {
                    if (!_inv[i].IsEmpty())
                    {
                        if (_inv[i].itemValue.HasQuality && _inv[i].itemValue.Quality > 0)
                        {
                            newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1:000} * {2} - quality: {3}\n", i, _inv[i].count, _inv[i].itemValue.ItemClass.GetItemName(), _inv[i].itemValue.Quality));
                            _playerNode.AppendChild(newEntry);
                        }
                        else
                        {
                            newEntry = _xmlDoc.CreateTextNode(string.Format("       Slot {0}: {1:000} * {2}\n", i, _inv[i].count, _inv[i].itemValue.ItemClass.GetItemName()));
                            _playerNode.AppendChild(newEntry);
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

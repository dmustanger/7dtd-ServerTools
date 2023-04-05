using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class DupeLog
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static List<string> Dict = new List<string>();
        public static Dictionary<int, ItemStack[]> OldBags = new Dictionary<int, ItemStack[]>();
        public static Dictionary<int, ItemStack[]> OldInvs = new Dictionary<int, ItemStack[]>();

        private const string file = "DuplicateItemExemption.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly string DupeFile = string.Format("DupeLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string DupeFilepath = string.Format("{0}/Logs/DupeLogs/{1}", API.ConfigPath, DupeFile);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {

            Dict.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    UpdateXml();
                }
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(FilePath);
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Dict.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Name"))
                        {
                            continue;
                        }
                        string name = line.GetAttribute("Name");
                        if (name == "")
                        {
                            continue;
                        }
                        if (!Dict.Contains(name))
                        {
                            Dict.Add(name);
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        File.Delete(FilePath);
                        UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePath);
                    UpdateXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in DupeLog.LoadXml: {0}", e.Message));
                }
            }
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<DuplicateItemExemption>");
                    sw.WriteLine(string.Format("    <!-- <Version=\"{0}\" /> -->", Config.Version));
                    sw.WriteLine("    <!-- <Item Name=\"stone\" /> -->");
                    sw.WriteLine("    <Item Name=\"\" />");
                    if (Dict.Count > 0)
                    {
                        for (int i = 0; i < Dict.Count; i++)
                        {
                            string _name = Dict[i];
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" />", _name));
                        }
                    }
                    sw.WriteLine("</DuplicateItemExemption>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DupeLog.UpdateXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void Exec(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            try
            {
                if (_cInfo != null && _playerDataFile != null && _playerDataFile.bag != null && _playerDataFile.inventory != null)
                {
                    if (OldBags.ContainsKey(_cInfo.entityId))
                    {
                        OldBags.TryGetValue(_cInfo.entityId, out ItemStack[] oldBag);
                        OldInvs.TryGetValue(_cInfo.entityId, out ItemStack[] oldInventory);
                        OldBags[_cInfo.entityId] = _playerDataFile.bag;
                        OldInvs[_cInfo.entityId] = _playerDataFile.inventory;
                        for (int i = 0; i < _playerDataFile.bag.Length; i++)
                        {
                            List<int> slots = new List<int>();
                            if (!slots.Contains(i) && !_playerDataFile.bag[i].itemValue.IsEmpty() && (oldBag[i].itemValue.Seed != _playerDataFile.bag[i].itemValue.Seed || oldBag[i].itemValue.Seed == _playerDataFile.bag[i].itemValue.Seed && oldBag[i].count != _playerDataFile.bag[i].count))
                            {
                                for (int j = 0; j < _playerDataFile.bag.Length; j++)
                                {
                                    if (i != j && !_playerDataFile.bag[j].itemValue.IsEmpty() && _playerDataFile.bag[i].itemValue.Seed == _playerDataFile.bag[j].itemValue.Seed && _playerDataFile.bag[i].count == _playerDataFile.bag[j].count)
                                    {
                                        slots.Add(j);
                                        using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}'. Slot '{4}' with '{5}' '{6}' matches slot '{7}' in their bag.", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, i + 1, _playerDataFile.bag[i].count, _playerDataFile.bag[i].itemValue.ItemClass.GetItemName(), j + 1));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                    }
                                }
                                slots.Clear();
                                for (int j = 0; j < _playerDataFile.inventory.Length; j++)
                                {
                                    if (!_playerDataFile.inventory[j].itemValue.IsEmpty() && _playerDataFile.bag[i].itemValue.Seed == _playerDataFile.inventory[j].itemValue.Seed && _playerDataFile.bag[i].count == _playerDataFile.inventory[j].count)
                                    {
                                        slots.Add(j);
                                        using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}'. Slot '{4}' with '{5}' '{6}' matches slot '{7}' in their inventory.", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, i + 1, _playerDataFile.bag[i].count, _playerDataFile.bag[i].itemValue.ItemClass.GetItemName(), j + 1));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < _playerDataFile.inventory.Length; i++)
                        {
                            List<int> slots = new List<int>();
                            if (!slots.Contains(i) && !_playerDataFile.inventory[i].itemValue.IsEmpty() && (oldInventory[i].itemValue.Seed != _playerDataFile.inventory[i].itemValue.Seed || oldInventory[i].itemValue.Seed == _playerDataFile.inventory[i].itemValue.Seed && oldInventory[i].count != _playerDataFile.inventory[i].count))
                            {
                                for (int j = 0; j < _playerDataFile.inventory.Length; j++)
                                {
                                    if (i != j && !_playerDataFile.inventory[j].itemValue.IsEmpty() && _playerDataFile.inventory[i].itemValue.Seed == _playerDataFile.inventory[j].itemValue.Seed && _playerDataFile.inventory[i].count == _playerDataFile.inventory[j].count)
                                    {
                                        slots.Add(j);
                                        using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}'. Inventory slot '{4}' with '{5}' '{6}' matches slot '{7}' in their inventory.", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, i + 1, _playerDataFile.inventory[i].count, _playerDataFile.inventory[i].itemValue.ItemClass.GetItemName(), j + 1));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                    }
                                }
                                slots.Clear();
                                for (int j = 0; j < _playerDataFile.bag.Length; j++)
                                {
                                    if (!_playerDataFile.bag[j].itemValue.IsEmpty() && _playerDataFile.inventory[i].itemValue.Seed == _playerDataFile.bag[j].itemValue.Seed && _playerDataFile.inventory[i].count == _playerDataFile.bag[j].count)
                                    {
                                        slots.Add(j);
                                        using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}'. Slot '{4}' with '{5}' '{6}' matches slot '{7}' in their bag.", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, i + 1, _playerDataFile.inventory[i].count, _playerDataFile.inventory[i].itemValue.ItemClass.GetItemName(), j + 1));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        OldBags.Add(_cInfo.entityId, _playerDataFile.bag);
                        OldInvs.Add(_cInfo.entityId, _playerDataFile.inventory);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DupeLog.Exec: {0}", e.Message));
            }
        }

        private static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<DuplicateItemExemption>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- <Item Name=\"stone\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment && !nodeList[i].OuterXml.Contains("<!-- <Item Name=\"stone") &&
                        !nodeList[i].OuterXml.Contains("<!-- <Version"))
                        {
                            sw.WriteLine(nodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine("    <Item Name=\"\" />");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && (line.Name == "Duplicate" || line.Name == "Item"))
                            {
                                string name = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                sw.WriteLine(string.Format("    <Item Name=\"{0}\" />", name));
                            }
                        }
                    }
                    sw.WriteLine("</DuplicateItemExemption>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DupeLog.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}

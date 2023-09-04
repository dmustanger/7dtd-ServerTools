using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class StartingItems
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static Dictionary<string, int[]> Dict = new Dictionary<string, int[]>();

        private const string file = "StartingItems.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
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
                        if (!line.HasAttributes)
                        {
                            continue;
                        }
                        if (line.HasAttribute("Name") && line.HasAttribute("Count") && line.HasAttribute("Quality"))
                        {
                            string item = line.GetAttribute("Name");
                            if (item == "")
                            {
                                continue;
                            }
                            if (!int.TryParse(line.GetAttribute("Count"), out int count))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring StartingItems.xml entry. Invalid (non-numeric) value for 'Count' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            if (!int.TryParse(line.GetAttribute("Quality"), out int quality))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring StartingItems.xml entry. Invalid (non-numeric) value for 'Quality' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            ItemValue itemValue = ItemClass.GetItem(item, false);
                            if (itemValue.type == ItemValue.None.type)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring StartingItems.xml entry. Item not found: {0}", item));
                                continue;
                            }
                            if (count > itemValue.ItemClass.Stacknumber.Value)
                            {
                                count = itemValue.ItemClass.Stacknumber.Value;
                                Log.Out(string.Format("[SERVERTOOLS] StartingItems.xml entry {0} was set above the max stack value. It has been reduced to the maximum of {1}", item, count));
                            }
                            if (count > itemValue.ItemClass.Stacknumber.Value)
                            {
                                count = itemValue.ItemClass.Stacknumber.Value;
                            }
                            else if (count < 1)
                            {
                                count = 1;
                            }
                            if (quality < 1)
                            {
                                quality = 1;
                            }
                            int[] c = new int[] { count, quality };
                            if (!Dict.ContainsKey(item))
                            {
                                Dict.Add(item, c);
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeStartingItemsXml(nodeList);
                        //UpgradeXml(nodeList);
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in StartingItems.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<StartingItems>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Item Name=\"foodCanChili\" Count=\"1\" Quality=\"1\" /> -->");
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, int[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" Count=\"{1}\" Quality=\"{2}\" />", kvp.Key, kvp.Value[0], kvp.Value[1]));
                        }
                    }
                    sw.WriteLine("</StartingItems>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in StartingItems.UpdateXml: {0}", e.Message));
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

        public static void Exec(ClientInfo _cInfo, List<string> _items)
        {
            try
            {
                World world = GameManager.Instance.World;
                if (_items == null)
                {
                    _items = Dict.Keys.ToList();
                }
                string item = _items[0];
                if (Dict.TryGetValue(item, out int[] itemData))
                {
                    ItemValue itemValue = new ItemValue(ItemClass.GetItem(item, false).type, 1, 1, false, null);
                    if (itemValue == null)
                    {
                        return;
                    }
                    if (itemValue.HasQuality)
                    {
                        if (itemData[1] > 0)
                        {
                            itemValue.Quality = itemData[1];
                        }
                    }
                    EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                    {
                        entityClass = EntityClass.FromString("item"),
                        id = EntityFactory.nextEntityID++,
                        itemStack = new ItemStack(itemValue, itemData[0]),
                        pos = world.Players.dict[_cInfo.entityId].position,
                        rot = new Vector3(20f, 0f, 20f),
                        lifetime = 60f,
                        belongsPlayerId = _cInfo.entityId
                    });
                    world.SpawnEntityInWorld(entityItem);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                    _items.Remove(item);
                    if (_items.Count > 0)
                    {
                        Timers.StartingItemsDelayTimer(_cInfo, _items);
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Player named '{0}' with Id '{1}' '{2}' received their starting items", _cInfo.playerName, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString);
                        Phrases.Dict.TryGetValue("StartingItems1", out string phrase1);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in StartingItems.Exec: {0}", e.Message));
            }
        }

        public static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<StartingItems>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Item Name=\"foodCanChili\" Count=\"1\" Quality=\"1\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- <Item Name=\"foodCanChili") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<Item Name=\"\"") && !nodeList[i].OuterXml.Contains("<!-- <Version"))
                            {
                                sw.WriteLine(nodeList[i].OuterXml);
                            }
                        }
                    }
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Item")
                            {
                                string name = "", count = "", quality = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Count"))
                                {
                                    count = line.GetAttribute("Count");
                                }
                                if (line.HasAttribute("Quality"))
                                {
                                    quality = line.GetAttribute("Quality");
                                }
                                sw.WriteLine(string.Format("    <Item Name=\"{0}\" Count=\"{1}\" Quality=\"{2}\" />", name, count, quality));
                            }
                        }
                    }
                    sw.WriteLine("</StartingItems>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in StartingItems.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class StartingItems
    {
        public static bool IsEnabled = false, IsRunning = false;
        private const string File = "StartingItems.xml";
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, File);
        public static Dictionary<string, int[]> ItemList = new Dictionary<string, int[]>();
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, File);
        private static bool UpdateConfig = false;

        public static void Load()
        {
            if (IsEnabled && !IsRunning)
            {
                LoadXml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            if (!Utils.FileExists(FilePath))
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
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", File, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Items")
                {
                    ItemList.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'StartingItems' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring starting item entry because of missing Name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Count"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring starting item entry because of missing Count attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Quality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring starting item entry because of missing Quality attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("Count"), out int _count))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring starting item entry because of invalid (non-numeric) value for 'Count' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("Quality"), out int _quality))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring starting item entry because of invalid (non-numeric) value for 'Quality' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (_quality < 1)
                        {
                            _quality = 1;
                        }
                        string _item = _line.GetAttribute("Name");
                        ItemClass _class;
                        Block _block;
                        int _id;
                        if (int.TryParse(_item, out _id))
                        {
                            _class = ItemClass.GetForId(_id);
                            _block = Block.GetBlockByName(_item, true);
                        }
                        else
                        {
                            _class = ItemClass.GetItemClass(_item, true);
                            _block = Block.GetBlockByName(_item, true);
                        }
                        if (_class == null && _block == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Starting item entry skipped. Item not found: {0}", _item));
                            continue;
                        }
                        if (!ItemList.ContainsKey(_item))
                        {
                            int[] _c = new int[] { _count, _quality };
                            ItemList.Add(_item, _c);
                        }
                    }
                }
            }
            if (UpdateConfig)
            {
                UpdateConfig = false;
                UpdateXml();
            }
        }

        private static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(FilePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<StartingItems>");
                sw.WriteLine("    <Items>");
                if (ItemList.Count > 0)
                {
                    foreach (KeyValuePair<string, int[]> kvp in ItemList)
                    {
                        sw.WriteLine(string.Format("        <Item Name=\"{0}\" Count=\"{1}\" Quality=\"{2}\" />", kvp.Key, kvp.Value[0], kvp.Value[1]));
                    }
                }
                else
                {
                    sw.WriteLine("        <Item Name=\"meleeToolRepairT0StoneAxe\" Count=\"1\" Quality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"meleeToolTorch\" Count=\"1\" Quality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanChili\" Count=\"1\" Quality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"drinkJarBoiledWater\" Count=\"1\" Quality=\"1\" />");
                }
                sw.WriteLine("    </Items>");
                sw.WriteLine("</StartingItems>");
                sw.Flush();
                sw.Close();
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
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (ItemList.Count > 0)
            {
                if (!PersistentContainer.Instance.Players[_cInfo.playerId].StartingItems)
                {
                    SpawnItems(_cInfo);
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Starting items have already been spawned for player {0} with steam id {1}", _cInfo.playerName, _cInfo.playerId));
                }
            }
            else
            {
                Log.Out("[SERVERTOOLS] Starting items list empty. Check the StartingItems.xml file for entries or mistakes.");
            }
        }

        public static void SpawnItems(ClientInfo _cInfo)
        {
            try
            {
                World world = GameManager.Instance.World;
                List<string> _itemList = StartingItems.ItemList.Keys.ToList();
                for (int i = 0; i < _itemList.Count; i++)
                {
                    string _item = _itemList[i];
                    int[] _itemData;
                    StartingItems.ItemList.TryGetValue(_item, out _itemData);
                    ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_item, false).type, false);
                    if (_itemValue.HasQuality && _itemData[1] > 0)
                    {
                        _itemValue.Quality = _itemData[1];
                    }
                    EntityItem entityItem = new EntityItem();
                    entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                    {
                        entityClass = EntityClass.FromString("item"),
                        id = EntityFactory.nextEntityID++,
                        itemStack = new ItemStack(_itemValue, _itemData[0]),
                        pos = world.Players.dict[_cInfo.entityId].position,
                        rot = new Vector3(20f, 0f, 20f),
                        lifetime = 60f,
                        belongsPlayerId = _cInfo.entityId
                    });
                    world.SpawnEntityInWorld(entityItem);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                    Log.Out(string.Format("[SERVERTOOLS] Spawned starting item {0} for {1} with steam id {2}", _itemValue.ItemClass.Name, _cInfo.playerName, _cInfo.playerId));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId].StartingItems = true;
                PersistentContainer.Instance.Save();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in StartingItems.SpawnItems: {0}", e.Message));
            }
        }
    }
}


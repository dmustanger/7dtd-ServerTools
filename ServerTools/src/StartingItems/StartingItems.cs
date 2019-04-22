using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class StartingItems
    {
        public static bool IsEnabled = false, IsRunning = false;
        private const string file = "StartingItems.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static Dictionary<string, int[]> startItemList = new Dictionary<string, int[]>();
        public static List<string> Que = new List<string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false;

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
            fileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(filePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Items")
                {
                    startItemList.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'items' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("item"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring starting item entry because of missing item attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("count"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring starting item entry because of missing count attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("quality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring starting item entry because of missing quality attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _count = 1;
                        int _quality = 1;
                        if (!int.TryParse(_line.GetAttribute("count"), out _count))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring starting item entry because of invalid (non-numeric) value for 'count' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("quality"), out _quality))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring starting item entry because of invalid (non-numeric) value for 'quality' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _item = _line.GetAttribute("item");
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
                        if (!startItemList.ContainsKey(_item))
                        {
                            int[] _c = new int[] { _count, _quality };
                            startItemList.Add(_item, _c);
                        }
                    }
                }
            }
            if (updateConfig)
            {
                updateConfig = false;
                UpdateXml();
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<StartingItems>");
                sw.WriteLine("    <Items>");
                if (startItemList.Count > 0)
                {
                    foreach (KeyValuePair<string, int[]> kvp in startItemList)
                    {
                        sw.WriteLine(string.Format("        <item item=\"{0}\" count=\"{1}\" quality=\"{2}\" />", kvp.Key, kvp.Value[0], kvp.Value[1]));
                    }
                }
                else
                {
                    sw.WriteLine("        <item item=\"meleeToolStoneAxe\" count=\"1\" quality=\"1\" />");
                    sw.WriteLine("        <item item=\"meleeToolTorch\" count=\"1\" quality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodCanChili\" count=\"1\" quality=\"1\" />");
                    sw.WriteLine("        <item item=\"drinkJarBoiledWater\" count=\"1\" quality=\"1\" />");
                }
                sw.WriteLine("    </Items>");
                sw.WriteLine("</StartingItems>");
                sw.Flush();
                sw.Close();
            }
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void StartingItemCheck(ClientInfo _cInfo)
        {
            if (startItemList.Count > 0)
            {
                string _sql = string.Format("SELECT startingItems FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                bool _startingItems;
                bool.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _startingItems);
                _result.Dispose();
                if (!_startingItems)
                {
                    Exec(_cInfo);
                }
            }
            else
            {
                Log.Out("[SERVERTOOLS] Starting items list empty. No items found in the file have been added.");
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            World world = GameManager.Instance.World;
            foreach (KeyValuePair<string, int[]> kvp in startItemList)
            {
                ItemValue _itemValue = new ItemValue(ItemClass.GetItem(kvp.Key).type, kvp.Value[1], kvp.Value[1], true, default(FastTags), 1);
                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                {
                    entityClass = EntityClass.FromString("item"),
                    id = EntityFactory.nextEntityID++,
                    itemStack = new ItemStack(_itemValue, kvp.Value[0]),
                    pos = world.Players.dict[_cInfo.entityId].position,
                    rot = new Vector3(20f, 0f, 20f),
                    lifetime = 60f,
                    belongsPlayerId = _cInfo.entityId
                });
                world.SpawnEntityInWorld(entityItem);
                _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                Log.Out(string.Format("[SERVERTOOLS] Spawned starting item {0} for {1}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name, _cInfo.playerName));
            }
            string _sql = string.Format("UPDATE Players SET startingItems = 'true' WHERE steamid = '{0}'", _cInfo.playerId);
            SQL.FastQuery(_sql, "StartingItems");
        }
    }
}


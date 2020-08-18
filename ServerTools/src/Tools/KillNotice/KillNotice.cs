using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class KillNotice
    {
        public static bool IsEnabled = false, IsRunning = false, Show_Level = false;
        private const string file = "KillNotice.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static Dictionary<string, string> dict = new Dictionary<string, string>();
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
            if (!IsEnabled && IsRunning)
            {
                dict.Clear();
                fileWatcher.Dispose();
                IsRunning = false;
            }
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
                if (childNode.Name == "Weapons")
                {
                    dict.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Weapons' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring weapons entry because of missing Name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("NewName"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring weapons entry because of missing NewName attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _name = _line.GetAttribute("Name");
                        ItemClass _class = ItemClass.GetItemClass(_name, true);
                        if (_class == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Kill Notice entry skipped. Weapon not found: {0}", _name));
                            continue;
                        }
                        if (!dict.ContainsKey(_name))
                        {
                            dict.Add(_name, _line.GetAttribute("NewName"));
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
                sw.WriteLine("<KillNotice>");
                sw.WriteLine("    <Weapons>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        sw.WriteLine(string.Format("        <Weapon Name=\"{0}\" NewName=\"{1}\" />", kvp.Key, kvp.Value));
                    }
                }
                else
                {
                    List<ItemClass> _itemClassMelee = ItemClass.GetItemsWithTag(FastTags.Parse("melee"));
                    List<ItemClass> _itemClassRanged = ItemClass.GetItemsWithTag(FastTags.Parse("ranged"));
                    for (int i = 0; i < _itemClassMelee.Count; i++)
                    {
                        ItemClass _itemClass = _itemClassMelee[i];
                        List<string> _tags = _itemClass.ItemTags.GetTagNames();
                        if (_itemClass.CreativeMode != EnumCreativeMode.None && _itemClass.CreativeMode != EnumCreativeMode.Dev && !_tags.Contains("ammo"))
                        {
                            sw.WriteLine(string.Format("        <Weapon Name=\"{0}\" NewName=\"{1}\" />", _itemClass.GetItemName(), _itemClass.GetLocalizedItemName() ?? _itemClass.GetItemName()));
                        }
                    }
                    for (int i = 0; i < _itemClassRanged.Count; i++)
                    {
                        ItemClass _itemClass = _itemClassRanged[i];
                        List<string> _tags = _itemClass.ItemTags.GetTagNames();
                        if (_itemClass.CreativeMode != EnumCreativeMode.None && _itemClass.CreativeMode != EnumCreativeMode.Dev && !_tags.Contains("ammo"))
                        {
                            sw.WriteLine(string.Format("        <Weapon Name=\"{0}\" NewName=\"{1}\" />", _itemClass.GetItemName(), _itemClass.GetLocalizedItemName() ?? _itemClass.GetItemName()));
                        }
                    }
                }
                sw.WriteLine("    </Weapons>");
                sw.WriteLine("</KillNotice>");
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

        public static void Notice(ClientInfo _cInfo, ClientInfo _cInfo2, string _holdingItem)
        {
            if (dict.ContainsKey(_holdingItem))
            {
                string _newName;
                dict.TryGetValue(_holdingItem, out _newName);
                if (Show_Level)
                {
                    EntityPlayer _killer = PersistentOperations.GetEntityPlayer(_cInfo2.playerId);
                    EntityPlayer _victim = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                    if (_killer != null && _victim != null)
                    {
                        string _phrase916;
                        if (!Phrases.Dict.TryGetValue(916, out _phrase916))
                        {
                            _phrase916 = "{PlayerName} ({KillerLevel}) has killed {Victim} ({VictimLevel}) with {Item}.";
                        }
                        _phrase916 = _phrase916.Replace("{PlayerName}", _cInfo2.playerName);
                        _phrase916 = _phrase916.Replace("{KillerLevel}", _killer.Progression.Level.ToString());
                        _phrase916 = _phrase916.Replace("{Victim}", _cInfo.playerName);
                        _phrase916 = _phrase916.Replace("{VictimLevel}", _victim.Progression.Level.ToString());
                        _phrase916 = _phrase916.Replace("{Item}", _newName);
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase916 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else
                {
                    string _phrase915;
                    if (!Phrases.Dict.TryGetValue(915, out _phrase915))
                    {
                        _phrase915 = "{PlayerName} has killed {Victim} with {Item}.";
                    }
                    _phrase915 = _phrase915.Replace("{PlayerName}", _cInfo2.playerName);
                    _phrase915 = _phrase915.Replace("{Victim}", _cInfo.playerName);
                    _phrase915 = _phrase915.Replace("{Item}", _newName);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase915 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
            else
            {
                if (Show_Level)
                {
                    EntityPlayer _killer = PersistentOperations.GetEntityPlayer(_cInfo2.playerId);
                    EntityPlayer _victim = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                    if (_killer != null && _victim != null)
                    {
                        string _phrase916;
                        if (!Phrases.Dict.TryGetValue(916, out _phrase916))
                        {
                            _phrase916 = "{PlayerName} ({KillerLevel}) has killed {Victim} ({VictimLevel}) with {Item}.";
                        }
                        _phrase916 = _phrase916.Replace("{PlayerName}", _cInfo2.playerName);
                        _phrase916 = _phrase916.Replace("{KillerLevel}", _killer.Progression.Level.ToString());
                        _phrase916 = _phrase916.Replace("{Victim}", _cInfo.playerName);
                        _phrase916 = _phrase916.Replace("{VictimLevel}", _victim.Progression.Level.ToString());
                        _phrase916 = _phrase916.Replace("{Item}", _holdingItem);
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase916 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else
                {
                    string _phrase915;
                    if (!Phrases.Dict.TryGetValue(915, out _phrase915))
                    {
                        _phrase915 = "{PlayerName} has killed {Victim} with {Item}.";
                    }
                    _phrase915 = _phrase915.Replace("{PlayerName}", _cInfo2.playerName);
                    _phrase915 = _phrase915.Replace("{Victim}", _cInfo.playerName);
                    _phrase915 = _phrase915.Replace("{Item}", _holdingItem);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase915 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
        }
    }
}

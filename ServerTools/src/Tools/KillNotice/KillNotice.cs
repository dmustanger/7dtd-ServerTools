using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class KillNotice
    {
        public static bool IsEnabled = false, IsRunning = false, PvP = false, Zombie_Kills = false, Show_Level = false, Show_Damage = false;
        private const string file = "KillNotice.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static Dictionary<string, string> Dict = new Dictionary<string, string>();
        public static Dictionary<int, int> Damage = new Dictionary<int, int>();
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);
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
                Dict.Clear();
                FileWatcher.Dispose();
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
                    Dict.Clear();
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
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring weapons entry in KillNotice.xml because of missing Name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("NewName"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring weapons entry in KillNotice.xml because of missing NewName attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _name = _line.GetAttribute("Name");
                        ItemClass _class = ItemClass.GetItemClass(_name, true);
                        if (_class == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Kill Notice entry skipped. Weapon not found: {0}", _name));
                            continue;
                        }
                        if (!Dict.ContainsKey(_name))
                        {
                            Dict.Add(_name, _line.GetAttribute("NewName"));
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
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<KillNotice>");
                sw.WriteLine("    <Weapons>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in Dict)
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
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void Exec(ClientInfo _cInfo, EntityPlayer _victim, ClientInfo _cInfo2, EntityPlayer _killer, string _holdingItem)
        {
            string _item = _holdingItem;
            if (Dict.ContainsKey(_holdingItem))
            {
                Dict.TryGetValue(_holdingItem, out _item);
            }
            if (Show_Level)
            {
                if (Show_Damage)
                {
                    if (Damage.ContainsKey(_victim.entityId))
                    {
                        Damage.TryGetValue(_victim.entityId, out int _damage);
                        Phrases.Dict.TryGetValue(543, out string _phrase543);
                        _phrase543 = _phrase543.Replace("{Name1}", _cInfo2.playerName);
                        _phrase543 = _phrase543.Replace("{Level1}", _killer.Progression.Level.ToString());
                        _phrase543 = _phrase543.Replace("{Name2}", _cInfo.playerName);
                        _phrase543 = _phrase543.Replace("{Level2}", _victim.Progression.Level.ToString());
                        _phrase543 = _phrase543.Replace("{Item}", _item);
                        _phrase543 = _phrase543.Replace("{Damage}", _damage.ToString());
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase543 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return;
                    }
                }
                Phrases.Dict.TryGetValue(542, out string _phrase542);
                _phrase542 = _phrase542.Replace("{Name1}", _cInfo2.playerName);
                _phrase542 = _phrase542.Replace("{Level1}", _killer.Progression.Level.ToString());
                _phrase542 = _phrase542.Replace("{Name2}", _cInfo.playerName);
                _phrase542 = _phrase542.Replace("{Level2}", _victim.Progression.Level.ToString());
                _phrase542 = _phrase542.Replace("{Item}", _item);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase542 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            else
            {
                if (Show_Damage)
                {
                    if (Damage.ContainsKey(_victim.entityId))
                    {
                        Damage.TryGetValue(_victim.entityId, out int _damage);
                        Phrases.Dict.TryGetValue(541, out string _phrase541);
                        _phrase541 = _phrase541.Replace("{Name1}", _cInfo2.playerName);
                        _phrase541 = _phrase541.Replace("{Name2}", _cInfo.playerName);
                        _phrase541 = _phrase541.Replace("{Item}", _item);
                        _phrase541 = _phrase541.Replace("{Damage}", _damage.ToString());
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase541 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return;
                    }
                }
                Phrases.Dict.TryGetValue(544, out string _phrase544);
                _phrase544 = _phrase544.Replace("{Name1}", _cInfo2.playerName);
                _phrase544 = _phrase544.Replace("{Name2}", _cInfo.playerName);
                _phrase544 = _phrase544.Replace("{Item}", _item);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase544 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
        }

        public static void ProcessStrength(EntityPlayer _player, int _strength)
        {
            if (Damage.ContainsKey(_player.entityId))
            {
                Damage[_player.entityId] = _strength;
            }
            else
            {
                Damage.Add(_player.entityId, _strength);
            }
        }
    }
}

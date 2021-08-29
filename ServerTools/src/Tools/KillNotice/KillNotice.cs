using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class KillNotice
    {
        public static bool IsEnabled = false, IsRunning = false, PvP = false, Zombie_Kills = false, Show_Level = false, Show_Damage = false;
        public static Dictionary<int, int> Damage = new Dictionary<int, int>();

        private static Dictionary<string, string> Dict = new Dictionary<string, string>();
        private const string file = "KillNotice.xml";
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
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                    return;
                }
                XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (_childNodes != null && _childNodes.Count > 0)
                {
                    Dict.Clear();
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_childNodes[i];
                            if (_line.HasAttributes)
                            {
                                if (_line.HasAttribute("Version") && _line.GetAttribute("Version") != Config.Version)
                                {
                                    UpgradeXml(_childNodes);
                                    return;
                                }
                                else if (_line.HasAttribute("Name") && _line.HasAttribute("NewName"))
                                {
                                    string _name = _line.GetAttribute("Name");
                                    ItemClass _class = ItemClass.GetItemClass(_name, true);
                                    if (_class == null)
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring KillNotice.xml entry. Weapon not found: {0}", _name));
                                        continue;
                                    }
                                    if (!Dict.ContainsKey(_name))
                                    {
                                        Dict.Add(_name, _line.GetAttribute("NewName"));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNotice.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<KillNotice>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Weapon Name=\"{0}\" NewName=\"{1}\" />", kvp.Key, kvp.Value));
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
                                sw.WriteLine(string.Format("    <Weapon Name=\"{0}\" NewName=\"{1}\" />", _itemClass.GetItemName(), _itemClass.GetLocalizedItemName() ?? _itemClass.GetItemName()));
                            }
                        }
                        for (int i = 0; i < _itemClassRanged.Count; i++)
                        {
                            ItemClass _itemClass = _itemClassRanged[i];
                            List<string> _tags = _itemClass.ItemTags.GetTagNames();
                            if (_itemClass.CreativeMode != EnumCreativeMode.None && _itemClass.CreativeMode != EnumCreativeMode.Dev && !_tags.Contains("ammo"))
                            {
                                sw.WriteLine(string.Format("    <Weapon Name=\"{0}\" NewName=\"{1}\" />", _itemClass.GetItemName(), _itemClass.GetLocalizedItemName() ?? _itemClass.GetItemName()));
                            }
                        }
                    }
                    sw.WriteLine("</KillNotice>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNotice.UpdateXml: {0}", e.Message));
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

        public static void Exec(ClientInfo _cInfo, EntityPlayer _victim, ClientInfo _cInfo2, EntityPlayer _killer, string _holdingItem)
        {
            try
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
                            Phrases.Dict.TryGetValue("KillNotice3", out string _phrase);
                            _phrase = _phrase.Replace("{Name1}", _cInfo2.playerName);
                            _phrase = _phrase.Replace("{Level1}", _killer.Progression.Level.ToString());
                            _phrase = _phrase.Replace("{Name2}", _cInfo.playerName);
                            _phrase = _phrase.Replace("{Level2}", _victim.Progression.Level.ToString());
                            _phrase = _phrase.Replace("{Item}", _item);
                            _phrase = _phrase.Replace("{Damage}", _damage.ToString());
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            return;
                        }
                    }
                    Phrases.Dict.TryGetValue("KillNotice2", out string _phrase1);
                    _phrase1 = _phrase1.Replace("{Name1}", _cInfo2.playerName);
                    _phrase1 = _phrase1.Replace("{Level1}", _killer.Progression.Level.ToString());
                    _phrase1 = _phrase1.Replace("{Name2}", _cInfo.playerName);
                    _phrase1 = _phrase1.Replace("{Level2}", _victim.Progression.Level.ToString());
                    _phrase1 = _phrase1.Replace("{Item}", _item);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    if (Show_Damage)
                    {
                        if (Damage.ContainsKey(_victim.entityId))
                        {
                            Damage.TryGetValue(_victim.entityId, out int _damage);
                            Phrases.Dict.TryGetValue("KillNotice1", out string _phrase);
                            _phrase = _phrase.Replace("{Name1}", _cInfo2.playerName);
                            _phrase = _phrase.Replace("{Name2}", _cInfo.playerName);
                            _phrase = _phrase.Replace("{Item}", _item);
                            _phrase = _phrase.Replace("{Damage}", _damage.ToString());
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            return;
                        }
                    }
                    Phrases.Dict.TryGetValue("KillNotice4", out string _phrase1);
                    _phrase1 = _phrase1.Replace("{Name1}", _cInfo2.playerName);
                    _phrase1 = _phrase1.Replace("{Name2}", _cInfo.playerName);
                    _phrase1 = _phrase1.Replace("{Item}", _item);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNotice.Exec: {0}", e.Message));
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

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<KillNotice>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            sw.WriteLine(_oldChildNodes[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    bool _blank = true;
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_oldChildNodes[i];
                            if (_line.HasAttributes && _line.Name == "Weapon")
                            {
                                _blank = false;
                                string _name = "", _newName = "";
                                if (_line.HasAttribute("Name"))
                                {
                                    _name = _line.GetAttribute("Name");
                                }
                                if (_line.HasAttribute("NewName"))
                                {
                                    _newName = _line.GetAttribute("NewName");
                                }
                                sw.WriteLine(string.Format("    <Weapon Name=\"{0}\" NewName=\"{1}\" />", _name, _newName));
                            }
                        }
                    }
                    if (_blank)
                    {
                        List<ItemClass> _itemClassMelee = ItemClass.GetItemsWithTag(FastTags.Parse("melee"));
                        List<ItemClass> _itemClassRanged = ItemClass.GetItemsWithTag(FastTags.Parse("ranged"));
                        for (int i = 0; i < _itemClassMelee.Count; i++)
                        {
                            ItemClass _itemClass = _itemClassMelee[i];
                            List<string> _tags = _itemClass.ItemTags.GetTagNames();
                            if (_itemClass.CreativeMode != EnumCreativeMode.None && _itemClass.CreativeMode != EnumCreativeMode.Dev && !_tags.Contains("ammo"))
                            {
                                sw.WriteLine(string.Format("    <Weapon Name=\"{0}\" NewName=\"{1}\" />", _itemClass.GetItemName(), _itemClass.GetLocalizedItemName() ?? _itemClass.GetItemName()));
                            }
                        }
                        for (int i = 0; i < _itemClassRanged.Count; i++)
                        {
                            ItemClass _itemClass = _itemClassRanged[i];
                            List<string> _tags = _itemClass.ItemTags.GetTagNames();
                            if (_itemClass.CreativeMode != EnumCreativeMode.None && _itemClass.CreativeMode != EnumCreativeMode.Dev && !_tags.Contains("ammo"))
                            {
                                sw.WriteLine(string.Format("    <Weapon Name=\"{0}\" NewName=\"{1}\" />", _itemClass.GetItemName(), _itemClass.GetLocalizedItemName() ?? _itemClass.GetItemName()));
                            }
                        }
                    }
                    sw.WriteLine("</KillNotice>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNotice.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}

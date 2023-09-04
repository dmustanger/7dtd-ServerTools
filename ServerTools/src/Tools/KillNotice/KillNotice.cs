using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class KillNotice
    {
        public static bool IsEnabled = false, IsRunning = false, Player = false, Zombie = false,
            Animal = false, Show_Level = false, Show_Damage = false;
        public static Dictionary<int, int[]> Damage = new Dictionary<int, int[]>();

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
                    XmlElement line;
                    ItemValue itemValue;
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Name") || !line.HasAttribute("NewName"))
                        {
                            continue;
                        }
                        string name = line.GetAttribute("Name");
                        if (name == "")
                        {
                            continue;
                        }
                        string newName = line.GetAttribute("NewName");
                        itemValue = ItemClass.GetItem(name, false);
                        if (itemValue.type == ItemValue.None.type)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring KillNotice.xml entry. Weapon not found: {0}", name));
                            continue;
                        }
                        else if (!Dict.ContainsKey(name))
                        {
                            Dict.Add(name, newName);
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeKillNoticeXml(nodeList);
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in KillNotice.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<KillNotice>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Weapon Name=\"meleeToolRepairT0StoneAxe\" NewName=\"Stone Axe\" /> -->");
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
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void Exec(EntityAlive __deadEntity, EntityAlive ___entityKiller, DamageResponse ___RecordedDamage)
        {
            if (__deadEntity is EntityPlayer)
            {
                ClientInfo cInfoDead = GeneralOperations.GetClientInfoFromEntityId(__deadEntity.entityId);
                if (cInfoDead != null)
                {
                    if (Player && ___entityKiller is EntityPlayer)
                    {
                        ClientInfo cInfoKiller = GeneralOperations.GetClientInfoFromEntityId(___entityKiller.entityId);
                        if (cInfoKiller != null)
                        {
                            if (___RecordedDamage.Source != null && ___RecordedDamage.Source.AttackingItem != null)
                            {
                                PlayerKilledPlayer(cInfoDead, __deadEntity, cInfoKiller, ___entityKiller, ___RecordedDamage.Source.AttackingItem, ___RecordedDamage.Strength);
                            }
                        }
                    }
                    else if (Zombie && ___entityKiller is EntityZombie)
                    {
                        ZombieKilledPlayer(___entityKiller, __deadEntity, cInfoDead, ___RecordedDamage.Strength);
                    }
                    else if (Animal && (___entityKiller is EntityAnimal || ___entityKiller is EntityEnemyAnimal))
                    {
                        AnimalKilledPlayer(___entityKiller, __deadEntity, cInfoDead, ___RecordedDamage.Strength);
                    }
                }
            }
        }

        public static void PlayerKilledPlayer(ClientInfo _cInfo, EntityAlive _victim, ClientInfo _cInfo2, EntityAlive _killer, ItemValue _itemValue, int _damage)
        {
            try
            {
                ItemClass itemClass = _itemValue.ItemClass;
                string item = itemClass.Name;
                if (Dict.ContainsKey(item))
                {
                    Dict.TryGetValue(item, out item);
                }
                else if (string.IsNullOrEmpty(itemClass.GetLocalizedItemName()))
                {
                    item = itemClass.GetLocalizedItemName();
                }
                if (Show_Level)
                {
                    if (Show_Damage)
                    {
                        Phrases.Dict.TryGetValue("KillNotice3", out string phrase);
                        phrase = phrase.Replace("{Name1}", _cInfo2.playerName);
                        phrase = phrase.Replace("{Level1}", _killer.Progression.Level.ToString());
                        phrase = phrase.Replace("{Name2}", _cInfo.playerName);
                        phrase = phrase.Replace("{Level2}", _victim.Progression.Level.ToString());
                        phrase = phrase.Replace("{Item}", item);
                        phrase = phrase.Replace("{Value}", _damage.ToString());
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return;
                    }
                    Phrases.Dict.TryGetValue("KillNotice2", out string phrase1);
                    phrase1 = phrase1.Replace("{Name1}", _cInfo2.playerName);
                    phrase1 = phrase1.Replace("{Level1}", _killer.Progression.Level.ToString());
                    phrase1 = phrase1.Replace("{Name2}", _cInfo.playerName);
                    phrase1 = phrase1.Replace("{Level2}", _victim.Progression.Level.ToString());
                    phrase1 = phrase1.Replace("{Item}", item);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    if (Show_Damage)
                    {
                        Phrases.Dict.TryGetValue("KillNotice1", out string phrase);
                        phrase = phrase.Replace("{Name1}", _cInfo2.playerName);
                        phrase = phrase.Replace("{Name2}", _cInfo.playerName);
                        phrase = phrase.Replace("{Item}", item);
                        phrase = phrase.Replace("{Value}", _damage.ToString());
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return;
                    }
                    Phrases.Dict.TryGetValue("KillNotice4", out string phrase1);
                    phrase1 = phrase1.Replace("{Name1}", _cInfo2.playerName);
                    phrase1 = phrase1.Replace("{Name2}", _cInfo.playerName);
                    phrase1 = phrase1.Replace("{Item}", item);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNotice.PlayerKilledPlayer: {0}", e.Message));
            }
        }

        public static void ZombieKilledPlayer(EntityAlive _zombie, EntityAlive _victim, ClientInfo _cInfo, int _damage)
        {
            try
            {
                if (Show_Level)
                {
                    if (Show_Damage)
                    {
                        Phrases.Dict.TryGetValue("KillNotice5", out string phrase);
                        phrase = phrase.Replace("{Name1}", _cInfo.playerName);
                        phrase = phrase.Replace("{Level}", _victim.Progression.Level.ToString());
                        phrase = phrase.Replace("{Name2}", _zombie.EntityClass.entityClassName);
                        phrase = phrase.Replace("{Value}", _damage.ToString());
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return;
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("KillNotice6", out string phrase);
                        phrase = phrase.Replace("{Name1}", _cInfo.playerName);
                        phrase = phrase.Replace("{Level}", _victim.Progression.Level.ToString());
                        phrase = phrase.Replace("{Name2}", _zombie.EntityClass.entityClassName);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else
                {
                    if (Show_Damage)
                    {
                        Phrases.Dict.TryGetValue("KillNotice7", out string phrase);
                        phrase = phrase.Replace("{Name1}", _cInfo.playerName);
                        phrase = phrase.Replace("{Name2}", _zombie.EntityClass.entityClassName);
                        phrase = phrase.Replace("{Value}", _damage.ToString());
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return;
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("KillNotice8", out string phrase);
                        phrase = phrase.Replace("{Name1}", _cInfo.playerName);
                        phrase = phrase.Replace("{Name2}", _zombie.EntityClass.entityClassName);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNotice.ZombieKilledPlayer: {0}", e.Message));
            }
        }

        public static void AnimalKilledPlayer(EntityAlive _animal, EntityAlive _victim, ClientInfo _cInfo, int _damage)
        {
            try
            {
                if (Show_Level)
                {
                    if (Show_Damage)
                    {
                        Phrases.Dict.TryGetValue("KillNotice5", out string phrase);
                        phrase = phrase.Replace("{Name1}", _cInfo.playerName);
                        phrase = phrase.Replace("{Level}", _victim.Progression.Level.ToString());
                        phrase = phrase.Replace("{Name2}", _animal.EntityName);
                        phrase = phrase.Replace("{Value}", _damage.ToString());
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return;
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("KillNotice6", out string phrase);
                        phrase = phrase.Replace("{Name1}", _cInfo.playerName);
                        phrase = phrase.Replace("{Level}", _victim.Progression.Level.ToString());
                        phrase = phrase.Replace("{Name2}", _animal.EntityName);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else
                {
                    if (Show_Damage)
                    {
                        Phrases.Dict.TryGetValue("KillNotice7", out string phrase);
                        phrase = phrase.Replace("{Name1}", _cInfo.playerName);
                        phrase = phrase.Replace("{Name2}", _animal.EntityName);
                        phrase = phrase.Replace("{Value}", _damage.ToString());
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return;
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("KillNotice8", out string phrase);
                        phrase = phrase.Replace("{Name1}", _cInfo.playerName);
                        phrase = phrase.Replace("{Name2}", _animal.EntityName);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNotice.AnimalKilledPlayer: {0}", e.Message));
            }
        }

        public static void MiscDeath(ClientInfo _cInfo, EnumDamageTypes _type)
        {
            Phrases.Dict.TryGetValue("KillNotice9", out string phrase);
            phrase = phrase.Replace("{Name}", _cInfo.playerName);
            phrase = phrase.Replace("{DamageType}", _type.ToString());
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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
                    sw.WriteLine("<KillNotice>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Weapon Name=\"meleeToolRepairT0StoneAxe\" NewName=\"Stone Axe\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<Weapon Name=\"\"") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<!-- <Weapon Name=\"meleeToolRepairT0StoneAxe\""))
                            {
                                sw.WriteLine(nodeList[i].OuterXml);
                            }
                        }
                    }
                    bool blank = true;
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Weapon")
                            {
                                blank = false;
                                string name = "", newName = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("NewName"))
                                {
                                    newName = line.GetAttribute("NewName");
                                }
                                sw.WriteLine(string.Format("    <Weapon Name=\"{0}\" NewName=\"{1}\" />", name, newName));
                            }
                        }
                    }
                    if (blank)
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

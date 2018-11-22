using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class KillNotice
    {
        public static bool IsEnabled = false, IsRunning = false;
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
                if (childNode.Name == "weapons")
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'weapons' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring weapons entry because of missing name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("newName"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring weapons entry because of missing newName attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _name = _line.GetAttribute("name");
                        ItemValue _itemValue = ItemClass.GetItem(_name, true);
                        if (_itemValue.type == ItemValue.None.type)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Kill Notice entry skipped. Weapon not found: {0}", _name));
                            continue;
                        }
                        if (!dict.ContainsKey(_name))
                        {
                            dict.Add(_name, _line.GetAttribute("newName"));
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
                        sw.WriteLine(string.Format("        <weapon name=\"{0}\" newName=\"{1}\" />", kvp.Key, kvp.Value));
                    }
                }
                else
                {
                    sw.WriteLine("        <weapon name=\"handPlayer\" newName=\"Fists of Fury\" />");
                    sw.WriteLine("        <weapon name=\"clubWood\" newName=\"a Wooden Club\" />");
                    sw.WriteLine("        <weapon name=\"clubIron\" newName=\"a Iron Club\" />");
                    sw.WriteLine("        <weapon name=\"clubBarbed\" newName=\"a Barbed Club\" />");
                    sw.WriteLine("        <weapon name=\"clubSpiked\" newName=\"a Spiked Club\" />");
                    sw.WriteLine("        <weapon name=\"sledgehammer\" newName=\"a Sledge Hammer\" />");
                    sw.WriteLine("        <weapon name=\"boneShiv\" newName=\"a Bone Shiv\" />");
                    sw.WriteLine("        <weapon name=\"huntingKnife\" newName=\"a Hunting Knife\" />");
                    sw.WriteLine("        <weapon name=\"machete\" newName=\"a Machete\" />");
                    sw.WriteLine("        <weapon name=\"gunPistol\" newName=\"a Pistol\" />");
                    sw.WriteLine("        <weapon name=\"gun44Magnum\" newName=\"a Magnum\" />");
                    sw.WriteLine("        <weapon name=\"gunSawedOffPumpShotgun\" newName=\"a Sawed Off Pump Shotgun\" />");
                    sw.WriteLine("        <weapon name=\"gunPumpShotgun\" newName=\"a Pump Shotgun\" />");
                    sw.WriteLine("        <weapon name=\"gunMP5\" newName=\"a MP5\" />");
                    sw.WriteLine("        <weapon name=\"gunAK47\" newName=\"a AK-47\" />");
                    sw.WriteLine("        <weapon name=\"gunHuntingRifle\" newName=\"a Hunting Rifle\" />");
                    sw.WriteLine("        <weapon name=\"gunSniperRifle\" newName=\"a Sniper Rifle\" />");
                    sw.WriteLine("        <weapon name=\"gunRocketLauncher\" newName=\"a Rocket Launcher\" />");
                    sw.WriteLine("        <weapon name=\"blunderbuss\" newName=\"a Blunderbuss\" />");
                    sw.WriteLine("        <weapon name=\"stoneAxe\" newName=\"a Stone Axe\" />");
                    sw.WriteLine("        <weapon name=\"tazasStoneaxe\" newName=\"a Tazas Stone Axe\" />");
                    sw.WriteLine("        <weapon name=\"fireaxeIron\" newName=\"a Iron Axe\" />");
                    sw.WriteLine("        <weapon name=\"fireaxeSteel\" newName=\"a Steel Axe\" />");
                    sw.WriteLine("        <weapon name=\"pickaxeIron\" newName=\"a Iron Pickaxe\" />");
                    sw.WriteLine("        <weapon name=\"pickaxeSteel\" newName=\"a Steel Pickaxe\" />");
                    sw.WriteLine("        <weapon name=\"stoneShovel\" newName=\"a Stone Shovel\" />");
                    sw.WriteLine("        <weapon name=\"shovelIron\" newName=\"a Iron Shovel\" />");
                    sw.WriteLine("        <weapon name=\"shovelSteel\" newName=\"a Steel Shovel\" />");
                    sw.WriteLine("        <weapon name=\"hoeIron\" newName=\"a Hoe\" />");
                    sw.WriteLine("        <weapon name=\"wrench\" newName=\"a Wrench\" />");
                    sw.WriteLine("        <weapon name=\"clawHammer\" newName=\"a Claw Hammer\" />");
                    sw.WriteLine("        <weapon name=\"nailgun\" newName=\"a Nailgun\" />");
                    sw.WriteLine("        <weapon name=\"chainsaw\" newName=\"a Chainsaw\" />");
                    sw.WriteLine("        <weapon name=\"auger\" newName=\"a Auger\" />");
                    sw.WriteLine("        <weapon name=\"woodenBow\" newName=\"a Wooden Bow\" />");
                    sw.WriteLine("        <weapon name=\"crossbow\" newName=\"a Crossbow\" />");
                    sw.WriteLine("        <weapon name=\"compoundBow\" newName=\"a Compund Bow\" />");
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
                string _phrase915;
                if (!Phrases.Dict.TryGetValue(915, out _phrase915))
                {
                    _phrase915 = "{PlayerName} has killed {Victim} with {Item}.";
                }
                _phrase915 = _phrase915.Replace("{PlayerName}", _cInfo2.playerName);
                _phrase915 = _phrase915.Replace("{Victim}", _cInfo.playerName);
                _phrase915 = _phrase915.Replace("{Item}", _newName);
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase915 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global);
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
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase915 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global);
            }
        }
    }
}

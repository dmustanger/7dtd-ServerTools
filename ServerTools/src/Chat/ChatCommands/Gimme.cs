using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class Gimme
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static bool AlwaysShowResponse = false;
        public static int DelayBetweenUses = 60;
        private const string file = "GimmeItems.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<string, int[]> dict = new SortedDictionary<string, int[]>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static System.Random random = new System.Random();

        private static List<string> list
        {
            get { return new List<string>(dict.Keys); }
        }

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

        private static void LoadXml()
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
                if (childNode.Name == "items")
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'items' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("item"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring items entry because of missing item attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("min"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring items entry because of missing min attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("max"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring items entry because of missing max attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _min = 1;
                        int _max = 1;
                        if (!int.TryParse(_line.GetAttribute("min"), out _min))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring items entry because of invalid (non-numeric) value for 'min' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("max"), out _max))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring items entry because of invalid (non-numeric) value for 'max' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _item = _line.GetAttribute("item");
                        ItemValue _iv = ItemClass.GetItem(_item, true);
                        if (_iv.type == ItemValue.None.type)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Gimme item not found.: {0}", _item));
                            continue;
                        }
                        if (!dict.ContainsKey(_item))
                        {
                                int[] _c = new int[] { _min, _max };
                                dict.Add(_item, _c); 
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Gimme>");
                sw.WriteLine("    <items>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, int[]> kvp in dict)
                    {
                        sw.WriteLine(string.Format("        <item item=\"{0}\" min=\"{1}\" max=\"{2}\" />", kvp.Key, kvp.Value[0], kvp.Value[1]));
                    }
                }
                else
                {
                    sw.WriteLine("        <item item=\"bottledWater\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"beer\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"keystoneBlock\" min=\"1\" max=\"1\" />");
                    sw.WriteLine("        <item item=\"canChicken\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"canChili\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"corn\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"potato\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"firstAidBandage\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"painkillers\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"scrapBrass\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"antibiotics\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"moldyBread\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"oil\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"cornMeal\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"blueberries\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"canHam\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"coffeBeans\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"casinoCoin\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"boneShiv\" min=\"1\" max=\"1\" />");
                    sw.WriteLine("        <item item=\"canDogfood\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"animalHide\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"blueberryPie\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"canPeas\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"canStock\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"canCatfood\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"scrapIron\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"goldenrodPlant\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"clayLump\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"rottingFlesh\" min=\"1\" max=\"5\" />");
                }
                sw.WriteLine("    </items>");
                sw.WriteLine("</Gimme>");
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

        public static void Checkplayer(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            if (DelayBetweenUses < 1)
            {
                _GiveItem(_cInfo, _announce);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastGimme == null)
                {
                    _GiveItem(_cInfo, _announce);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastGimme;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed > DelayBetweenUses)
                    {
                        _GiveItem(_cInfo, _announce);
                    }
                    else
                    {
                        int _timeleft = DelayBetweenUses - _timepassed;
                        string _phrase6;
                        if (!Phrases.Dict.TryGetValue(6, out _phrase6))
                        {
                            _phrase6 = "{PlayerName} you can only use /gimme once every {DelayBetweenUses} minutes.Time remaining: {TimeRemaining} minutes.";
                        }
                        _phrase6 = _phrase6.Replace("{PlayerName}", _playerName);
                        _phrase6 = _phrase6.Replace("{DelayBetweenUses}", DelayBetweenUses.ToString());
                        _phrase6 = _phrase6.Replace("{TimeRemaining}", _timeleft.ToString());
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase6), "Server", false, "", false);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase6), "Server", false, "", false));
                        }
                    }
                }
            }
        }

        private static void _GiveItem(ClientInfo _cInfo, bool _announce)
        {
            string _randomItem = list.RandomObject();
            ItemValue iv = ItemClass.GetItem(_randomItem, true);
            iv = new ItemValue(iv.type, true);
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (iv.HasQuality)
            {
                int _quality = random.Next(1, 600);
                iv.Quality = _quality;
            }
            int[] _counts;
            if (dict.TryGetValue(_randomItem, out _counts))
            {
                int _count = random.Next(_counts[0], _counts[1]);
                ItemStack _itemDrop = new ItemStack(iv, _count);
                GameManager.Instance.ItemDropServer(_itemDrop, _player.GetPosition(), Vector3.zero);
                string _phrase7;
                if (!Phrases.Dict.TryGetValue(7, out _phrase7))
                {
                    _phrase7 = "{PlayerName} has received {ItemCount} {ItemName}.";
                }
                _phrase7 = _phrase7.Replace("{PlayerName}", _cInfo.playerName);
                _phrase7 = _phrase7.Replace("{ItemCount}", _count.ToString());
                _phrase7 = _phrase7.Replace("{ItemName}", _randomItem);
                if (_announce || AlwaysShowResponse)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase7), "Server", false, "", false);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase7), "Server", false, "", false));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, true].LastGimme = DateTime.Now;
                PersistentContainer.Instance.Save();
            }
        }
    }
}
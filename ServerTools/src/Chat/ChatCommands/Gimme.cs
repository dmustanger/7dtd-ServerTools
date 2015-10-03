using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class Gimme
    {
        public static bool AlwaysShowResponse = false;
        public static bool IsEnabled = false;
        public static int DelayBetweenUses = 60;
        private static string _file = "GimmeItems.xml";
        private static string _filepath = string.Format("{0}/{1}", Config._configpath, _file);
        public static FileSystemWatcher _fileWatcher = new FileSystemWatcher(Config._configpath, _file);
        private static SortedDictionary<string, string> Items = new SortedDictionary<string, string>();
        private static string _datafile = "GimmeData.xml";
        private static string _datafilepath = string.Format("{0}/{1}", Config._datapath, _datafile);
        private static Dictionary<string, DateTime> Players = new Dictionary<string, DateTime>();
        private static System.Random _random = new System.Random();
        public static bool IsRunning = false;
        
        private static List<string> ItemsList
        {
            get { return new List<string>(Items.Keys); }
        }

        private static List<string> PlayersList
        {
            get { return new List<string>(Players.Keys); }
        }

        public static void Init()
        {
            if (IsEnabled && !IsRunning)
            {
                if (!Utils.FileExists(_filepath))
                {
                    UpdateXml();
                }
                LoadGimmeItems();
                LoadPlayers();
                InitFileWatcher();
                IsRunning = true;
            }
        }

        private static void UpdateXml()
        {
            _fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Gimme>");
                sw.WriteLine("    <items>");
                if (Items.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in Items)
                    {
                        string[] _minmax = kvp.Value.Split(',');
                        sw.WriteLine(string.Format("        <item item=\"{0}\" min=\"{1}\" max=\"{2}\" />", kvp.Key, _minmax[0], _minmax[1]));
                    }
                }
                else
                {
                    sw.WriteLine("        <item item=\"bottledWater\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"beer\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"stick\" min=\"5\" max=\"10\" />");
                    sw.WriteLine("        <item item=\"keystone\" min=\"1\" max=\"1\" />");
                    sw.WriteLine("        <item item=\"ingotBrass\" min=\"1\" max=\"5\" />");
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
                    sw.WriteLine("        <item item=\"goldenrod\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"clayLump\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"rottingFlesh\" min=\"1\" max=\"5\" />");
                }
                sw.WriteLine("    </items>");
                sw.WriteLine("</Gimme>");
                sw.Flush();
                sw.Close();
            }
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            _fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateXml();
            }
            LoadGimmeItems();
        }

        private static void LoadGimmeItems()
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_filepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _file , e.Message));
                return;
            }
            XmlNode _GimmeXml = xmlDoc.DocumentElement;
            Items.Clear();
            foreach (XmlNode childNode in _GimmeXml.ChildNodes)
            {
                if (childNode.Name == "items")
                {
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
                        if (!_line.HasAttribute("item") || !_line.HasAttribute("min") || !_line.HasAttribute("max"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring items entry because of missing a item attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _minMax = _line.GetAttribute("min") + "," + _line.GetAttribute("max");
                        if (!Items.ContainsKey(_line.GetAttribute("item")))
                        {
                            Items.Add(_line.GetAttribute("item"), _minMax);
                        }
                    }
                }
            }
        }

        private static void LoadPlayers()
        {
            if (!Utils.FileExists(_datafilepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_datafilepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _datafile , e.Message));
                return;
            }
            XmlNode _GimmeXml = xmlDoc.DocumentElement;
            Players.Clear();
            foreach (XmlNode childNode in _GimmeXml.ChildNodes)
            {
                if (childNode.Name == "players")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'config' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ping entry because of missing 'SteamId' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        DateTime dt;
                        if (!DateTime.TryParse(_line.GetAttribute("last_gimme"), out dt))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring player entry because of invalid (date) value for 'last_gimme' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        Players.Add(_line.GetAttribute("SteamId"), dt);
                    }
                }
            }
        }

        public static void Checkplayer(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            DateTime _datetime;
            if (DelayBetweenUses > 0 && Players.TryGetValue(_cInfo.playerId, out _datetime))
            {
                int _timepassed = time.GetMinutes(_datetime);
                if (_timepassed < DelayBetweenUses)
                {
                    int _timeleft = DelayBetweenUses - _timepassed;
                    string _phrase6 = "{PlayerName} you can only use Gimme once every {1} minutes.Time remaining: {2} minutes.";
                    if (Phrases._Phrases.TryGetValue(6, out _phrase6))
                    {
                        _phrase6 = _phrase6.Replace("{0}", _playerName);
                        _phrase6 = _phrase6.Replace("{1}", DelayBetweenUses.ToString());
                        _phrase6 = _phrase6.Replace("{2}", _timeleft.ToString());
                        _phrase6 = _phrase6.Replace("{PlayerName}", _playerName);
                        _phrase6 = _phrase6.Replace("{DelayBetweenUses}", DelayBetweenUses.ToString());
                        _phrase6 = _phrase6.Replace("{TimeRemaining}", _timeleft.ToString());
                    }
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer(_cInfo, string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase6), "Server");
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase6), "Server"));
                    }
                }
                else
                {
                    Players.Remove(_cInfo.playerId);
                    _GiveItem(_cInfo, _announce);
                }
            }
            else
            {
                if(PlayersList.Contains(_cInfo.playerId))
                {
                    Players.Remove(_cInfo.playerId);
                }
                _GiveItem(_cInfo, _announce);
            }
        }

        private static void _GiveItem(ClientInfo _cInfo, bool _announce)
        {
            string _randomItem = ItemsList.RandomObject();
            ItemValue _itemValue = new ItemValue();
            if (GameItems._gameItems.ContainsKey(_randomItem))
            {
                _itemValue = GameItems._gameItems[_randomItem].Clone();
            }
            else
            {
                _randomItem = _randomItem.ToLower();
                foreach (KeyValuePair<string, ItemValue> _key in GameItems._gameItems)
                {
                    if (_key.Key.ToLower().Equals(_randomItem))
                    {
                        _itemValue = _key.Value.Clone();
                    }
                }
            }
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_itemValue.HasQuality)
            {
                int _lvl = _player.GetLevel() * 10;
                int _min = _lvl - 50;
                int _max = _lvl + 50;
                int _quality = _random.Next(_min, _max);
                if (_quality > 600)
                {
                    _quality = _lvl;
                }
                _itemValue.Quality = _quality;
            }
            string _itemcounts = "1,5";
            if (Items.TryGetValue(_randomItem, out _itemcounts))
            {
                string[] _minmax = _itemcounts.Split(',');
                int _min = 1;
                int _max = 5;
                if (int.TryParse(_minmax[0], out _min) && int.TryParse(_minmax[1], out _max))
                {
                    int _count = _random.Next(_min, _max);
                    ItemStack _itemDrop = new ItemStack(_itemValue, _count);
                    GameManager.Instance.ItemDropServer(_itemDrop, _player.GetPosition(), Vector3.zero, -1, 60);
                    string _phrase7 = "{PlayerName} has received {ItemCount} {ItemName}.";
                    if (Phrases._Phrases.TryGetValue(7, out _phrase7))
                    {
                        _phrase7 = _phrase7.Replace("{0}", _cInfo.playerName);
                        _phrase7 = _phrase7.Replace("{1}", _count.ToString());
                        _phrase7 = _phrase7.Replace("{2}", _randomItem);
                        _phrase7 = _phrase7.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase7 = _phrase7.Replace("{ItemCount}", _count.ToString());
                        _phrase7 = _phrase7.Replace("{ItemName}", _randomItem);
                    }
                    if (_announce || AlwaysShowResponse)
                    {
                        GameManager.Instance.GameMessageServer(_cInfo, string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase7), "Server");
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase7), "Server"));
                    }
                    Players.Add(_cInfo.playerId, DateTime.Now);
                    UpdatePlayerXml();
                }
            }
        }

        private static void UpdatePlayerXml()
        {
            if (!Directory.Exists(Config._datapath))
            {
                Directory.CreateDirectory(Config._datapath);
            }
            using (StreamWriter swp = new StreamWriter(_datafilepath))
            {
                swp.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                swp.WriteLine("<Gimme>");
                swp.WriteLine("    <players>");
                foreach (string _sid in PlayersList)
                {
                    DateTime _datetime;
                    if (Players.TryGetValue(_sid, out _datetime))
                    {
                        int _timepassed = time.GetMinutes(_datetime);
                        if (_timepassed > DelayBetweenUses)
                        {
                            Players.Remove(_sid);
                        }
                        else
                        {
                            swp.WriteLine(string.Format("        <Player SteamId=\"{0}\" last_gimme=\"{1}\" />", _sid, _datetime));
                        }
                    }
                }
                swp.WriteLine("    </players>");
                swp.WriteLine("</Gimme>");
                swp.Flush();
                swp.Close();
            }
        }
    }
}
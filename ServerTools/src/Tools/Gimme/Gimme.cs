using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class Gimme
    {
        public static bool IsEnabled = false, IsRunning = false, Zombies = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static string Command_gimme = "gimme", Command_gimmie = "gimmie", Zombie_Id = "4,9,11";

        private static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();
        private const string file = "Gimme.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static readonly System.Random Rnd = new System.Random();

        private static List<string> List
        {
            get { return new List<string>(Dict.Keys); }
        }

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
                bool upgrade = true;
                for (int i = 0; i < _childNodes.Count; i++)
                {
                    if (_childNodes[i].NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }
                    XmlElement _line = (XmlElement)_childNodes[i];
                    if (_line.HasAttributes)
                    {
                        if (_line.HasAttribute("Version") && _line.GetAttribute("Version") == Config.Version)
                        {
                            upgrade = false;
                        }
                        else if (_line.HasAttribute("Name") && _line.HasAttribute("SecondaryName") && _line.HasAttribute("MinCount") && _line.HasAttribute("MaxCount") &&
                            _line.HasAttribute("MinQuality") && _line.HasAttribute("MaxQuality"))
                        {
                            if (!int.TryParse(_line.GetAttribute("MinCount"), out int _minCount))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry because of invalid (non-numeric) value for 'MinCount' attribute: {0}", _line.OuterXml));
                                continue;
                            }
                            if (!int.TryParse(_line.GetAttribute("MaxCount"), out int _maxCount))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry because of invalid (non-numeric) value for 'MaxCount' attribute: {0}", _line.OuterXml));
                                continue;
                            }
                            if (!int.TryParse(_line.GetAttribute("MinQuality"), out int _minQuality))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry because of invalid (non-numeric) value for 'MinQuality' attribute: {0}", _line.OuterXml));
                                continue;
                            }
                            if (!int.TryParse(_line.GetAttribute("MaxQuality"), out int _maxQuality))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry because of invalid (non-numeric) value for 'MaxQuality' attribute: {0}", _line.OuterXml));
                                continue;
                            }
                            string _name = _line.GetAttribute("Name");
                            if (_name == "WalletCoin" || _name == "walletCoin" || _name == "walletcoin")
                            {
                                if (Wallet.IsEnabled)
                                {
                                    if (_minCount < 1)
                                    {
                                        _minCount = 1;
                                    }
                                }
                                else
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry. Wallet tool is not enabled: {0}", _line.OuterXml));
                                    continue;
                                }
                            }
                            else
                            {
                                ItemValue _itemValue = ItemClass.GetItem(_name, false);
                                if (_itemValue.type == ItemValue.None.type)
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry. Name not found: {0}", _name));
                                    continue;
                                }
                                if (_minCount > _itemValue.ItemClass.Stacknumber.Value)
                                {
                                    _minCount = _itemValue.ItemClass.Stacknumber.Value;
                                }
                                else if (_minCount < 1)
                                {
                                    _minCount = 1;
                                }
                                if (_maxCount > _itemValue.ItemClass.Stacknumber.Value)
                                {
                                    _maxCount = _itemValue.ItemClass.Stacknumber.Value;
                                }
                                else if (_maxCount < 1)
                                {
                                    _maxCount = 1;
                                }
                                if (_minCount > _maxCount)
                                {
                                    int _switch = _maxCount;
                                    _maxCount = _minCount;
                                    _minCount = _switch;
                                }
                                if (_minQuality > _maxQuality)
                                {
                                    int _switch = _maxQuality;
                                    _maxQuality = _minQuality;
                                    _minQuality = _switch;
                                }

                                string _secondary;
                                if (_line.HasAttribute("SecondaryName"))
                                {
                                    _secondary = _line.GetAttribute("SecondaryName");
                                }
                                else
                                {
                                    _secondary = _name;
                                }
                                if (!Dict.ContainsKey(_name))
                                {
                                    string[] _c = new string[] { _secondary, _minCount.ToString(), _maxCount.ToString(), _minQuality.ToString(), _maxQuality.ToString() };
                                    Dict.Add(_name, _c);
                                }
                            }
                        }
                    }
                }
                if (upgrade)
                {
                    UpgradeXml(_childNodes);
                    return;
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
                    sw.WriteLine("<Gimme>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!--  Secondary name is what will show in chat instead of the item name  -->");
                    sw.WriteLine("<!--  Items that do not require a quality should be set to 1 for both min and max  -->");
                    sw.WriteLine("<!--  WalletCoin can be used as the item name. Secondary name should be set to your Wallet Coin_Name option  -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" MinCount=\"{2}\" MaxCount=\"{3}\" MinQuality=\"{4}\" MaxQuality=\"{5}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4]));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <Item Name=\"drinkJarBoiledWater\" SecondaryName=\"boiled water\" MinCount=\"1\" MaxCount=\"6\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    }
                    sw.WriteLine("</Gimme>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.UpdateXml: {0}", e.Message));
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
            try
            {
                if (Dict.Count > 0)
                {
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            CommandCost(_cInfo);
                        }
                        else
                        {
                            ZCheck(_cInfo);
                        }
                    }
                    else
                    {
                        DateTime _lastgimme = DateTime.Now;
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme != null)
                        {
                            _lastgimme = PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme;
                        }
                        TimeSpan varTime = DateTime.Now - _lastgimme;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (ReservedSlots.IsEnabled)
                        {
                            if (ReservedSlots.Reduced_Delay && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                if (DateTime.Now < _dt)
                                {
                                    int _delay = Delay_Between_Uses / 2;
                                    Time(_cInfo, _timepassed, _delay);
                                    return;
                                }
                            }
                        }
                        Time(_cInfo, _timepassed, Delay_Between_Uses);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.Exec: {0}", e.Message));
            }
        }

        private static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo);
                    }
                    else
                    {
                        ZCheck(_cInfo);
                    }
                }
                else
                {
                    int _timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Gimme1", out string _phrase);
                    _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    _phrase = _phrase.Replace("{TimeRemaining}", _timeleft.ToString());
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_gimme}", Command_gimme);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.Time: {0}", e.Message));
            }
        }

        private static void CommandCost(ClientInfo _cInfo)
        {
            try
            {
                if (Wallet.IsEnabled && Command_Cost > 0)
                {
                    if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                    {
                        ZCheck(_cInfo);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Gimme3", out string _phrase);
                        _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ZCheck(_cInfo);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.CommandCost: {0}", e.Message));
            }
        }

        private static void ZCheck(ClientInfo _cInfo)
        {
            try
            {
                if (Zombies)
                {
                    int itemOrEntity = Rnd.Next(1, 9);
                    if (itemOrEntity != 4)
                    {
                        RandomItem(_cInfo);
                    }
                    else
                    {
                        RandomZombie(_cInfo);
                    }
                }
                else
                {
                    RandomItem(_cInfo);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.ZCheck: {0}", e.Message));
            }
        }

        private static void RandomItem(ClientInfo _cInfo)
        {
            try
            {
                string _randomItem = List.RandomObject();
                if (Dict.TryGetValue(_randomItem, out string[] _item))
                {
                    if (_randomItem == "WalletCoin" || _randomItem == "walletCoin" || _randomItem == "walletcoin")
                    {
                        if (Wallet.IsEnabled)
                        {
                            int.TryParse(_item[1], out int _minCount);
                            int.TryParse(_item[2], out int _maxCount);
                            int _count = Rnd.Next(_minCount, _maxCount + 1);
                            if (Command_Cost >= 1)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                            }
                            Wallet.AddCoinsToWallet(_cInfo.playerId, _count);
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Gimme2", out string _phrase);
                            _phrase = _phrase.Replace("{ItemCount}", _count.ToString());
                            if (_item[0] != "")
                            {
                                _phrase = _phrase.Replace("{ItemName}", _item[0]);
                            }
                            else
                            {
                                _phrase = _phrase.Replace("{ItemName}", Wallet.Coin_Name);
                            }
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Gimme5", out string _phrase);
                            Log.Out(string.Format("[SERVERTOOLS] {0}", _phrase));
                        }
                    }
                    else
                    {
                        ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_randomItem, false).type, false);
                        if (_itemValue != null)
                        {
                            int.TryParse(_item[1], out int _minCount);
                            int.TryParse(_item[2], out int _maxCount);
                            int.TryParse(_item[3], out int _minQuality);
                            int.TryParse(_item[4], out int _maxQuality);
                            int _count = Rnd.Next(_minCount, _maxCount + 1);
                            if (_itemValue.HasQuality)
                            {
                                _itemValue.Quality = Rnd.Next(_minQuality, _maxQuality + 1);
                            }
                            EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                            {
                                entityClass = EntityClass.FromString("item"),
                                id = EntityFactory.nextEntityID++,
                                itemStack = new ItemStack(_itemValue, _count),
                                pos = GameManager.Instance.World.Players.dict[_cInfo.entityId].position,
                                rot = new Vector3(20f, 0f, 20f),
                                lifetime = 60f,
                                belongsPlayerId = _cInfo.entityId
                            });
                            GameManager.Instance.World.SpawnEntityInWorld(entityItem);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                            GameManager.Instance.World.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                            }
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Gimme2", out string _phrase);
                            _phrase = _phrase.Replace("{ItemCount}", _count.ToString());
                            if (_item[0] != "")
                            {
                                _phrase = _phrase.Replace("{ItemName}", _item[0]);
                            }
                            else
                            {
                                _phrase = _phrase.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                            }
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.RandomItem: {0}", e.Message));
            }
        }

        private static void RandomZombie(ClientInfo _cInfo)
        {
            try
            {
                if (Zombie_Id != "")
                {
                    if (Zombie_Id.Contains(","))
                    {
                        string[] _zombieIds = Zombie_Id.Split(',');
                        int _count = Rnd.Next(1, _zombieIds.Length + 1);
                        string _zId = _zombieIds[_count];
                        if (int.TryParse(_zId, out int _zombieId))
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} 10 @ {1}", _cInfo.playerId, _zombieId), null);
                            Log.Out(string.Format("[SERVERTOOLS] Spawned an entity for {0}'s gimme", _cInfo.playerName));
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                            }
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Gimme4", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            RandomItem(_cInfo);
                        }
                    }
                    else
                    {
                        if (int.TryParse(Zombie_Id, out int _zombieId))
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} 10 @ {1}", _cInfo.playerId, _zombieId), null);
                            Log.Out(string.Format("[SERVERTOOLS] Spawned an entity for {0}'s gimme", _cInfo.playerName));
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                            }
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Gimme4", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            RandomItem(_cInfo);
                        }
                    }
                }
                else
                {
                    RandomItem(_cInfo);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.RandomZombie: {0}", e.Message));
            }
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Gimme>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!--  Secondary name is what will show in chat instead of the item name  -->");
                    sw.WriteLine("<!--  Items that do not require a quality should be set to 1 for both min and max  -->");
                    sw.WriteLine("<!--  WalletCoin can be used as the item name. Secondary name should be set to your Wallet Coin_Name option  -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes)
                        {
                            string _name = "", _secondary = "", _minCount = "", _maxCount = "", _minQuality = "", _maxQuality = "";
                            if (_line.HasAttribute("Name"))
                            {
                                _name = _line.GetAttribute("Name");
                            }
                            if (_line.HasAttribute("SecondaryName"))
                            {
                                _secondary = _line.GetAttribute("SecondaryName");
                            }
                            if (_line.HasAttribute("MinCount"))
                            {
                                _minCount = _line.GetAttribute("MinCount");
                            }
                            if (_line.HasAttribute("MaxCount"))
                            {
                                _maxCount = _line.GetAttribute("MaxCount");
                            }
                            if (_line.HasAttribute("MinQuality"))
                            {
                                _minQuality = _line.GetAttribute("MinQuality");
                            }
                            if (_line.HasAttribute("MaxQuality"))
                            {
                                _maxQuality = _line.GetAttribute("MaxQuality");
                            }
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" MinCount=\"{2}\" MaxCount=\"{3}\" MinQuality=\"{4}\" MaxQuality=\"{5}\" />", _name, _secondary, _minCount, _maxCount, _minQuality, _maxQuality));
                        }
                    }
                    sw.WriteLine("</Gimme>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
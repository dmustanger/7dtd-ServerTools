using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class Prayer
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static string Command_pray = "pray";
        public static int Delay_Between_Uses = 30, Command_Cost = 10;
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();

        private const string file = "Prayer.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly Random Random = new Random();
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
                            else if (_line.HasAttribute("Name") && _line.HasAttribute("Message"))
                            {
                                string _buff = _line.GetAttribute("Name");
                                string _message = _line.GetAttribute("Message");
                                BuffClass _class = BuffManager.GetBuff(_buff);
                                if (_class == null)
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer.xml entry. Buff is not valid: {0}", _line.OuterXml));
                                    continue;
                                }
                                if (!Dict.ContainsKey(_buff))
                                {
                                    Dict.Add(_buff, _message);
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Prayer.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<Prayer>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> _buff in Dict)
                        {
                            sw.WriteLine(string.Format("    <Buff Name=\"{0}\" Message=\"{1}\" />", _buff.Key, _buff.Value));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <Buff Name=\"buffPerkCharismaticNature\" Message=\"Your charisma has blossomed through your prayer\" />");
                        sw.WriteLine("    <Buff Name=\"buffPerkParkour\" Message=\"You can fall without taking damage by the grace of your prayers\" />");
                        sw.WriteLine("    <Buff Name=\"buffPistolPeteSwissKnees\" Message=\"Your prayers have developed in to a higher chance to cripple\" />");
                        sw.WriteLine("    <Buff Name=\"buffAutoWeaponsRagdoll\" Message=\"Your prayers have been answered with auto weapon knockdown damage\" />");
                    }
                    sw.WriteLine("</Prayer>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Prayer.UpdateXml: {0}", e.Message));
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
                if (Delay_Between_Uses < 1)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo);
                    }
                    else
                    {
                        SetBuff(_cInfo);
                    }
                }
                else
                {
                    DateTime _lastPrayer = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].LastPrayer != null)
                    {
                        _lastPrayer = PersistentContainer.Instance.Players[_cInfo.playerId].LastPrayer;
                    }
                    TimeSpan varTime = DateTime.Now - _lastPrayer;
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Prayer.Exec: {0}", e.Message));
            }
        }

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
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
                        SetBuff(_cInfo);
                    }
                }
                else
                {
                    int _timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Prayer1", out string _phrase);
                    _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    _phrase = _phrase.Replace("{TimeRemaining}", _timeleft.ToString());
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_pray}", Command_pray);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Prayer.Time: {0}", e.Message));
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
                        SetBuff(_cInfo);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Prayer2", out string _phrase);
                        _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    SetBuff(_cInfo);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Prayer.CommandCost: {0}", e.Message));
            }
        }

        private static void SetBuff(ClientInfo _cInfo)
        {
            try
            {
                if (Dict.Count > 0)
                {
                    List<string> Keys = new List<string>(Dict.Keys);
                    string randomKey = Keys[Random.Next(Dict.Count)];
                    string _message = Dict[randomKey];
                    SdtdConsole.Instance.ExecuteSync(string.Format("buffplayer {0} {1}", _cInfo.playerId, randomKey), null);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _message + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                    }
                    PersistentContainer.Instance.Players[_cInfo.playerId].LastPrayer = DateTime.Now;
                    PersistentContainer.DataChange = true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Prayer.SetBuff: {0}", e.Message));
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
                    sw.WriteLine("<Prayer>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.Name == "Buff")
                        {
                            string _name = "", _message = "";
                            if (_line.HasAttribute("Name"))
                            {
                                _name = _line.GetAttribute("Name");
                            }
                            if (_line.HasAttribute("Message"))
                            {
                                _message = _line.GetAttribute("Message");
                            }
                            sw.WriteLine(string.Format("    <Buff Name=\"{0}\" Message=\"{1}\" />", _name, _message));
                        }
                    }
                    sw.WriteLine("</Prayer>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Prayer.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}

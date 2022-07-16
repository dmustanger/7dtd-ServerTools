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

        private static XmlNodeList OldNodeList;

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
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    Dict.Clear();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)childNodes[i];
                            if (line.HasAttributes)
                            {
                                if (line.HasAttribute("Version") && line.GetAttribute("Version") == Config.Version)
                                {
                                    upgrade = false;
                                    continue;
                                }
                                else if (line.HasAttribute("Name") && line.HasAttribute("Message"))
                                {
                                    string buff = line.GetAttribute("Name");
                                    string message = line.GetAttribute("Message");
                                    BuffClass buffClass = BuffManager.GetBuff(buff);
                                    if (buffClass == null)
                                    {
                                        Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer.xml entry. Buff is not valid: {0}", line.OuterXml));
                                        continue;
                                    }
                                    if (!Dict.ContainsKey(buff))
                                    {
                                        Dict.Add(buff, message);
                                    }
                                }
                            }
                        }
                    }
                }
                if (upgrade)
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    XmlNode node = nodeList[0];
                    XmlElement line = (XmlElement)nodeList[0];
                    if (line != null)
                    {
                        if (line.HasAttributes)
                        {
                            OldNodeList = nodeList;
                            File.Delete(FilePath);
                            UpgradeXml();
                            return;
                        }
                        else
                        {
                            nodeList = node.ChildNodes;
                            line = (XmlElement)nodeList[0];
                            if (line != null)
                            {
                                if (line.HasAttributes)
                                {
                                    OldNodeList = nodeList;
                                    File.Delete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            File.Delete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing Prayer.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in Prayer.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<Prayer>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Buff Name=\"buffPerkCharismaticNature\" Message=\"Your charisma has blossomed through your prayer\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> _buff in Dict)
                        {
                            sw.WriteLine(string.Format("    <Buff Name=\"{0}\" Message=\"{1}\" />", _buff.Key, _buff.Value));
                        }
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
            if (!File.Exists(FilePath))
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
                    if (Command_Cost >= 1 && (Wallet.IsEnabled || Bank.IsEnabled && Bank.Payments))
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
                    DateTime lastPrayer = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastPrayer != null)
                    {
                        lastPrayer = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastPrayer;
                    }
                    TimeSpan varTime = DateTime.Now - lastPrayer;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled)
                    {
                        if (ReservedSlots.Reduced_Delay && ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime dt);
                            if (DateTime.Now < dt)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Time(_cInfo, timepassed, delay);
                                return;
                            }
                        }
                    }
                    Time(_cInfo, timepassed, Delay_Between_Uses);
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
                    if (Command_Cost >= 1 && (Wallet.IsEnabled || Bank.IsEnabled && Bank.Payments))
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
                    int timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Prayer1", out string _phrase);
                    _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    _phrase = _phrase.Replace("{TimeRemaining}", timeleft.ToString());
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
                int currency = 0;
                int bankValue = 0;
                if (Wallet.IsEnabled)
                {
                    currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                }
                if (Bank.IsEnabled && Bank.Payments)
                {
                    bankValue = Bank.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                }
                if (currency + bankValue >= Command_Cost)
                {
                    SetBuff(_cInfo);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Prayer2", out string phrase);
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    string message = Dict[randomKey];
                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("buffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, randomKey), null);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + message + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    if (Command_Cost >= 1 && Wallet.IsEnabled)
                    {
                        if (Bank.IsEnabled && Bank.Payments)
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost, true);
                        }
                        else
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost, false);
                        }
                    }
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastPrayer = DateTime.Now;
                    PersistentContainer.DataChange = true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Prayer.SetBuff: {0}", e.Message));
            }
        }

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Prayer>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Buff Name=\"buffPerkCharismaticNature\" Message=\"Your charisma has blossomed through your prayer\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.StartsWith("    <!-- <Buff Name=\"buffPerkCharismaticNature\"") &&
                            !OldNodeList[i].OuterXml.StartsWith("    <!-- <Buff Name=\"\""))
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)OldNodeList[i];
                            if (line.HasAttributes && line.Name == "Buff")
                            {
                                string name = "", message = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Message"))
                                {
                                    message = line.GetAttribute("Message");
                                }
                                sw.WriteLine(string.Format("    <Buff Name=\"{0}\" Message=\"{1}\" />", name, message));
                            }
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

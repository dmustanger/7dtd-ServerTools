using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes)
                        {
                            continue;
                        }
                        if (line.HasAttribute("Name") && line.HasAttribute("Message"))
                        {
                            string buff = line.GetAttribute("Name");
                            string message = line.GetAttribute("Message");
                            if (buff == "")
                            {
                                continue;
                            }
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
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradePrayerXml(nodeList);
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
                    sw.WriteLine(string.Format("    <!-- <Version=\"{0}\" /> -->", Config.Version));
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Buff Name=\"buffPerkCharismaticNature\" Message=\"Your charisma has blossomed through your prayer\" /> -->");
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
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                    {
                        int delay = Delay_Between_Uses / 2;
                        Time(_cInfo, timepassed, delay);
                        return;
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
                    if (Command_Cost > 0)
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
                    Phrases.Dict.TryGetValue("Prayer1", out string phrase);
                    phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_pray}", Command_pray);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                int currency = 0, bankCurrency = 0, cost = Command_Cost;
                if (Wallet.IsEnabled)
                {
                    currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                }
                if (Bank.IsEnabled && Bank.Direct_Payment)
                {
                    bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
                }
                if (currency + bankCurrency >= cost)
                {
                    if (currency > 0)
                    {
                        if (currency < cost)
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                            cost -= currency;
                            Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                        }
                        else
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                        }
                    }
                    else
                    {
                        Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                    }
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
                    string randomKey = Keys[new Random().Next(Dict.Count)];
                    string message = Dict[randomKey];
                    SdtdConsole.Instance.ExecuteSync(string.Format("buffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, randomKey), null);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + message + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastPrayer = DateTime.Now;
                    PersistentContainer.DataChange = true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Prayer.SetBuff: {0}", e.Message));
            }
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
                    sw.WriteLine("<Prayer>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Buff Name=\"buffPerkCharismaticNature\" Message=\"Your charisma has blossomed through your prayer\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (nodeList[i].NodeType == XmlNodeType.Comment && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<Buff Name=\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Buff Name=\"buffPerkCharismaticNature"))
                            {
                                sw.WriteLine(nodeList[i].OuterXml);
                            }
                        }
                    }
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
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

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
        public static string Command126 = "pray";
        public static int Delay_Between_Uses = 30, Command_Cost = 10;
        private const string file = "Prayer.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static Random random = new Random();
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();

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
                if (childNode.Name == "Prayers")
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Prayers' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry because of missing Name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Message"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry because of missing Message attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _buff = _line.GetAttribute("Name");
                        string _message = _line.GetAttribute("Message");
                        BuffClass _class = BuffManager.GetBuff(_buff);
                        if (_class == null)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry because buff was not valid: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Dict.ContainsKey(_buff))
                        {
                            Dict.Add(_buff, _message);
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Prayer>");
                sw.WriteLine("    <Prayers>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> _buff in Dict)
                    {
                        sw.WriteLine(string.Format("        <Buff Name=\"{0}\" Message=\"{1}\" />", _buff.Key, _buff.Value));
                    }
                }
                else
                {
                    sw.WriteLine("        <Buff Name=\"buffPerkCharismaticNature\" Message=\"Your charisma has blossomed through your prayer\" />");
                    sw.WriteLine("        <Buff Name=\"buffPerkParkour\" Message=\"You can fall without taking damage by the grace of your prayers\" />");
                    sw.WriteLine("        <Buff Name=\"buffPistolPeteSwissKnees\" Message=\"Your prayers have developed in to a higher chance to cripple\" />");
                    sw.WriteLine("        <Buff Name=\"buffAutoWeaponsRagdoll\" Message=\"Your prayers have been answered with auto weapon knockdown damage\" />");
                }
                sw.WriteLine("    </Prayers>");
                sw.WriteLine("</Prayer>");
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

        public static void Exec(ClientInfo _cInfo)
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
                        DateTime _dt;
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
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

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
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
                Phrases.Dict.TryGetValue(821, out string _phrase821);
                _phrase821 = _phrase821.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase821 = _phrase821.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase821 = _phrase821.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
                _phrase821 = _phrase821.Replace("{Command126}", Command126);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase821 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void CommandCost(ClientInfo _cInfo)
        {
            if (Wallet.IsEnabled && Command_Cost > 0)
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                {
                    SetBuff(_cInfo);
                }
                else
                {
                    Phrases.Dict.TryGetValue(822, out string _phrase822);
                    _phrase822 = _phrase822.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase822 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                SetBuff(_cInfo);
            }
        }

        private static void SetBuff(ClientInfo _cInfo)
        {
            if (Dict.Count > 0)
            {
                List<string> Keys = new List<string>(Dict.Keys);
                string randomKey = Keys[random.Next(Dict.Count)];
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
    }
}

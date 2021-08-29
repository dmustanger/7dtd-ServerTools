using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class InfoTicker
    {
        public static bool IsEnabled = false, IsRunning = false, Random = false;
        public static string Command_infoticker = "infoticker";
        public static int Delay = 60;
        public static List<string> ExemptionList = new List<string>();

        private static Dictionary<string, string> Dict = new Dictionary<string, string>();
        private static List<string> MsgList = new List<string>();
        private const string file = "InfoTicker.xml";
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
            MsgList.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
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
                    MsgList.Clear();
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
                                else if (_line.HasAttribute("Message"))
                                {
                                    string _message = _line.GetAttribute("Message");
                                    if (!Dict.ContainsKey(_message))
                                    {
                                        Dict.Add(_message, null);
                                    }
                                }
                            }
                        }
                    }
                }
                if (Dict.Count > 0)
                {
                    BuildList();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InfoTicker.LoadXml: {0}", e.Message));
            }
        }

        public static void BuildList()
        {
            MsgList = new List<string>(Dict.Keys);
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<InfoTicker>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- Possible variables {EntityId}, {SteamId}, {PlayerName} -->");
                    sw.WriteLine("<!-- <Ticker Message=\"Have a suggestion or complaint? Post on our forums or discord and let us know\" /> -->");
                    sw.WriteLine("<!-- <Ticker Message=\"Type /gimme once an hour for a free gift!\" /> -->");
                    sw.WriteLine("<!-- <Ticker Message=\"Type /commands for a list of the chat commands\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Ticker Message=\"{0}\" />", kvp.Key));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <!-- <Ticker Message=\"\" /> -->");
                    }
                    sw.WriteLine("</InfoTicker>");
                    sw.Flush();
                    sw.Close();
                }
                FileWatcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InfoTicker.UpdateXml: {0}", e.Message));
            }
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void Exec()
        {
            try
            {
                if (MsgList.Count > 0 && ConnectionManager.Instance.ClientCount() > 0)
                {
                    if (Random)
                    {
                        MsgList.RandomizeList();
                        string _message = MsgList[0];
                        List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                        if (_cInfoList != null && _cInfoList.Count > 0)
                        {
                            for (int i = 0; i < _cInfoList.Count; i++)
                            {
                                ClientInfo _cInfo = _cInfoList[i];
                                if (_cInfo != null)
                                {
                                    if (!ExemptionList.Contains(_cInfo.playerId))
                                    {
                                        _message = _message.Replace("{EntityId}", _cInfo.entityId.ToString());
                                        _message = _message.Replace("{SteamId}", _cInfo.playerId);
                                        _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _message + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            MsgList.RemoveAt(0);
                            if (MsgList.Count == 0)
                            {
                                BuildList();
                            }
                        }
                    }
                    else
                    {
                        string _message = MsgList[0];
                        List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                        if (_cInfoList != null && _cInfoList.Count > 0)
                        {
                            for (int i = 0; i < _cInfoList.Count; i++)
                            {
                                ClientInfo _cInfo = _cInfoList[i];
                                if (_cInfo != null)
                                {
                                    if (!ExemptionList.Contains(_cInfo.playerId))
                                    {
                                        _message = _message.Replace("{EntityId}", _cInfo.entityId.ToString());
                                        _message = _message.Replace("{SteamId}", _cInfo.playerId);
                                        _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _message + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            MsgList.RemoveAt(0);
                            if (MsgList.Count == 0)
                            {
                                BuildList();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InfoTicker.Exec: {0}", e.Message));
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
                    sw.WriteLine("<InfoTicker>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- Possible variables {EntityId}, {SteamId}, {PlayerName} -->");
                    sw.WriteLine("<!-- <Ticker Message=\"Have a suggestion or complaint? Post on our forums or discord and let us know\" /> -->");
                    sw.WriteLine("<!-- <Ticker Message=\"Type /gimme once an hour for a free gift!\" /> -->");
                    sw.WriteLine("<!-- <Ticker Message=\"Type /commands for a list of the chat commands\" /> -->");
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.StartsWith("<!-- Possible variables") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Ticker Message=\"Have a suggestion") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Ticker Message=\"Type /gimme") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Ticker Message=\"Type /commands") && 
                            !_oldChildNodes[i].OuterXml.StartsWith("    <!-- <Ticker Message=\"\""))
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
                            if (_line.HasAttributes && _line.Name == "Ticker")
                            {
                                _blank = false;
                                string _message = "";
                                if (_line.HasAttribute("Message"))
                                {
                                    _message = _line.GetAttribute("Message");
                                }
                                sw.WriteLine(string.Format("    <Ticker Message=\"{0}\" />", _message));
                            }
                        }
                    }
                    if (_blank)
                    {
                        sw.WriteLine("    <!-- <Ticker Message=\"\" /> -->");
                    }
                    sw.WriteLine("</InfoTicker>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InfoTicker.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
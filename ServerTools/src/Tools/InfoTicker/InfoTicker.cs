using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class InfoTicker
    {
        public static bool IsEnabled = false, IsRunning = false, Random = false;
        public static string Command104 = "infoticker";
        public static int Delay = 60;
        private const string File = "InfoTicker.xml";
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, File);
        private static Dictionary<string, string> Dict = new Dictionary<string, string>();
        private static List<string> MsgList = new List<string>();
        public static List<string> ExemptionList = new List<string>();
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, File);

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
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", File, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Messages")
                {
                    Dict.Clear();
                    MsgList.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Info_Ticker' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Message"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of missing Message attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _message = _line.GetAttribute("Message");
                        if (!Dict.ContainsKey(_message))
                        {
                            Dict.Add(_message, null);
                        }
                        else
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because this message already exists: {0}", subChild.OuterXml));
                            continue;
                        }
                    }
                }
            }
            if (Dict.Count == 0)
            {
                Log.Warning("------------------------------------------------------------------------------");
                Log.Warning("[SERVERTOOLS] Ignoring Info_Ticker because no messages were found on your list");
                Log.Warning("------------------------------------------------------------------------------");
            }
            else
            {
                BuildList();
            }
        }

        public static void BuildList()
        {
            if (Dict.Count > 0)
            {
                MsgList = new List<string>(Dict.Keys);
            }
            else
            {
                Log.Warning("[SERVERTOOLS] Ignoring Info_Ticker because no messages were found on your list");
            }
        }

        private static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(FilePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<InfoTicketer>");
                sw.WriteLine("    <Messages>");
                sw.WriteLine("        <!-- possible variables {EntityId} {SteamId} {PlayerName}-->");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in Dict)
                    {
                        sw.WriteLine(string.Format("        <Ticker Message=\"{0}\" />", kvp.Key));
                    }
                }
                else
                {
                    sw.WriteLine("        <Ticker Message=\"Have a suggestion or complaint? Post on our forums or discord and let us know.\" />");
                    sw.WriteLine("        <!-- <Ticker Message=\"Type /gimme once an hour for a free gift!\" /> -->");
                    sw.WriteLine("        <!-- <Ticker Message=\"Typ /gimme, einmal pro Stunde für ein freies Geschenk!\" /> -->");
                    sw.WriteLine("        <!-- <Ticker Message=\"Visit 'Yoursitehere' for rules, custom recipes and forum discussions!\" /> -->");
                    sw.WriteLine("        <!-- <Ticker Message=\"Besuchen Yoursitehere für Regelungen , kundenspezifische Rezepturen und Forumsdiskussionen!\" /> -->");
                    sw.WriteLine("        <!-- <Ticker Message=\"Type /commands for a list of the chat commands.\" /> -->");
                }
                sw.WriteLine("    </Messages>");
                sw.WriteLine("</InfoTicketer>");
                sw.Flush();
                sw.Close();
            }
            FileWatcher.EnableRaisingEvents = true;
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
            if (ConnectionManager.Instance.ClientCount() > 0)
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
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class Packages
    {
        public static bool IsEnabled = false, IsRunning = false;
        private const string file = "NetPackageBuffs.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static string _file = string.Format("PacketManipulationLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/Logs/PacketManipulationLogs/{1}", API.ConfigPath, _file);
        public static List<string> Dict = new List<string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                if (childNode.Name == "Buffs")
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Items' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring NetPackageBuffs entry because of missing Name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _name = _line.GetAttribute("Name");
                        if (!Dict.Contains(_name))
                        {
                            Dict.Add(_name);
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
                sw.WriteLine("<Buffs>");
                sw.WriteLine("    <Buff>");
                if (Dict.Count > 0)
                {
                    for (int i = 0; i < Dict.Count; i++)
                    {
                        sw.WriteLine(string.Format("        <Buff Name=\"{0}\" />", Dict[i]));
                    }
                }
                else
                {
                    sw.WriteLine("        <Buff Name=\"god\" />");
                    sw.WriteLine("        <Buff Name=\"megadamage\" />");
                    sw.WriteLine("        <Buff Name=\"buffme\" />");
                    sw.WriteLine("        <Buff Name=\"nerfme\" />");
                    sw.WriteLine("        <Buff Name=\"maxedout\" />");
                    sw.WriteLine("        <Buff Name=\"pegasus\" />");
                }
                sw.WriteLine("    </Buff>");
                sw.WriteLine("</Buffs>");
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

        public static void Kick(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue(1033, out string _phrase1033);
            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase1033), null);
            Phrases.Dict.TryGetValue(1034, out string _phrase1034);
            _phrase1034 = _phrase1034.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase1034 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        public static void Ban(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue(1031, out string _phrase1031);
            if (_cInfo.ownerId != _cInfo.playerId)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 50 years \"{1}\"", _cInfo.ownerId, _phrase1031), null);
            }
            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 50 years \"{1}\"", _cInfo.playerId, _phrase1031), null);
            Phrases.Dict.TryGetValue(1032, out string _phrase1032);
            _phrase1032 = _phrase1032.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase1032 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        public static void Writer(ClientInfo _cInfo, string _action)
        {
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("{0}: {1} {2} {3} Net package manipulation: {4}", DateTime.Now, _cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, _action));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }
    }
}

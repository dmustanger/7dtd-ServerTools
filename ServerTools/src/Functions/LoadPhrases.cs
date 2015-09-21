using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class Phrases
    {
        public static SortedDictionary<int, string> _Phrases = new SortedDictionary<int, string>();
        private static string _file = "ServerToolsPhrases.xml";
        private static string _filepath = string.Format("{0}/{1}", Config._configpath, _file);
        private static FileSystemWatcher _fileWatcher = new FileSystemWatcher(Config._configpath, _file);

        public static void Init()
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdatePhrases();
            }
            LoadPhrases();
            InitFileWatcher();
        }

        private static void LoadPhrases()
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdatePhrases();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_filepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _file, e.Message));
                return;
            }
            XmlNode _PhrasesXml = xmlDoc.DocumentElement;
            _Phrases.Clear();
            foreach (XmlNode childNode in _PhrasesXml.ChildNodes)
            {
                if (childNode.Name == "Phrases")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Phrases' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("id"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrase entry because of missing 'id' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Phrase"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrase entry because of missing 'Phrase' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _id;
                        if (!int.TryParse(_line.GetAttribute("id"), out _id))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrase entry because of invalid (non-numeric) value for 'id' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_Phrases.ContainsKey(_id))
                        {
                            _Phrases.Add(_id, _line.GetAttribute("Phrase"));
                        }
                    }
                }
            }
        }

        public static void UpdatePhrases()
        {
            _fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ServerTools>");
                sw.WriteLine("    <Phrases>");
                if (_Phrases.Count > 0)
                {
                    foreach (KeyValuePair<int, string> kvp in _Phrases)
                    {
                        sw.WriteLine(string.Format("        <Phrase id=\"{1}\" Phrase=\"{1}\" />", kvp.Key, kvp.Value));
                    }
                }
                else
                {
                    sw.WriteLine("        <!-- ******************************************************** -->");
                    sw.WriteLine("        <!-- *************** High Ping Kicker Phrases *************** -->");
                    sw.WriteLine("        <!-- ******************************************************** -->");
                    sw.WriteLine("        <!-- {0}=players name, {1}=players ping, {2}=servers max ping limit -->");
                    sw.WriteLine("        <Phrase id=\"1\" Phrase=\"Auto Kicking {0} for high ping. ({1}) Maxping is {2}.\" />");
                    sw.WriteLine("        <!-- {0}=players ping, {1}=servers max ping limit -->");
                    sw.WriteLine("        <Phrase id=\"2\" Phrase=\"Auto Kicked: Ping To High. ({0}) Max Ping is {1}.\" />");
                    sw.WriteLine("        <!-- ******************************************************** -->");
                    sw.WriteLine("        <!-- ***************** Invalid Item Phrases ***************** -->");
                    sw.WriteLine("        <!-- ******************************************************** -->");
                    sw.WriteLine("        <!-- {0}=players name, {1}=Item name, {2}=item count {3}=max count per stack -->");
                    sw.WriteLine("        <Phrase id=\"3\" Phrase=\"{0} you have a invalid item stack: {1} {2}. Max per stack: {3}.\" />");
                    sw.WriteLine("        <!-- {0}=players name, {1}=Item name -->");
                    sw.WriteLine("        <Phrase id=\"4\" Phrase=\"Cheat Detected: Auto banned {0} for having a invalid item: {1}.\" />");
                    sw.WriteLine("        <!-- {0}=players name, {1}=Item name -->");
                    sw.WriteLine("        <Phrase id=\"5\" Phrase=\"Cheat Detected: Auto kicked {0} for having a invalid item: {1}.\" />");
                    sw.WriteLine("        <!-- ******************************************************** -->");
                    sw.WriteLine("        <!-- ************************* Gimme ************************ -->");
                    sw.WriteLine("        <!-- ******************************************************** -->");
                    sw.WriteLine("        <!-- {0}=players name, {1}=WaitTime {2}= Time remaining -->");
                    sw.WriteLine("        <Phrase id=\"6\" Phrase=\"{0} you can only use Gimme once every {1} minutes. Time remaining: {2} minutes.\" />");
                    sw.WriteLine("        <!-- {0}=players name, {1}=Item count {2}= Item -->");
                    sw.WriteLine("        <Phrase id=\"7\" Phrase=\"{0} has received {1} {2}.\" />");
                    sw.WriteLine("        <!-- ******************************************************** -->");
                    sw.WriteLine("        <!-- ************************ Killme ************************ -->");
                    sw.WriteLine("        <!-- ******************************************************** -->");
                    sw.WriteLine("        <!-- {0}=players name, {1}=waitime {2}= Time remaining -->");
                    sw.WriteLine("        <Phrase id=\"8\" Phrase=\"{0} you can only use /killme once every {1} minutes. Time remaining: {2} minutes.\" />");
                    sw.WriteLine("        <!-- ******************************************************** -->");
                    sw.WriteLine("        <!-- *********************** SetHome ************************ -->");
                    sw.WriteLine("        <!-- ******************************************************** -->");
                    sw.WriteLine("        <!-- {0}=players name -->");
                    sw.WriteLine("        <Phrase id=\"9\" Phrase=\"You already have a home set.\" />");
                    sw.WriteLine("        <!-- {0}=players name -->");
                    sw.WriteLine("        <Phrase id=\"10\" Phrase=\"Your home has been saved.\" />");
                    sw.WriteLine("        <!-- {0}=players name -->");
                    sw.WriteLine("        <Phrase id=\"11\" Phrase=\"You do not have a home saved.\" />");
                    sw.WriteLine("        <!-- {0}=players name -->");
                    sw.WriteLine("        <Phrase id=\"12\" Phrase=\"Your home has been removed.\" />");
                    sw.WriteLine("        <!-- {0}=players name {1}=waitime {2}= Time remaining -->");
                    sw.WriteLine("        <Phrase id=\"13\" Phrase=\"{0} you can only use /home once every {1} minutes. Time remaining: {2} minutes.\" />");
                }
                sw.WriteLine("    </Phrases>");
                sw.WriteLine("</ServerTools>");
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
                UpdatePhrases();
            }
            LoadPhrases();
        }
    }
}
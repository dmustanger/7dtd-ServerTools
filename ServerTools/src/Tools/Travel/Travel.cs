using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Travel
    {
        public static bool IsEnabled = false, IsRunning = false, Player_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static string Command_travel = "travel";

        private static SortedDictionary<string, string[]> Dict = new SortedDictionary<string, string[]>();
        private static SortedDictionary<string, string> Destination = new SortedDictionary<string, string>();

        private const string file = "TravelLocations.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            Destination.Clear();
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
                Destination.Clear();
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
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
                        if (line.HasAttribute("Name") && line.HasAttribute("Corner1") && line.HasAttribute("Corner2") && line.HasAttribute("Destination"))
                        {
                            string name = line.GetAttribute("Name");
                            string corner1 = line.GetAttribute("Corner1");
                            string corner2 = line.GetAttribute("Corner2");
                            if (name == "" && corner1 == "" && corner2 == "")
                            {
                                continue;
                            }
                            string[] corner1Split = corner1.Split(',');
                            string[] corner2Split = corner2.Split(',');
                            string destination = line.GetAttribute("Destination");
                            int.TryParse(corner1Split[0], out int x1);
                            int.TryParse(corner1Split[1], out int y1);
                            int.TryParse(corner1Split[2], out int z1);
                            int.TryParse(corner2Split[0], out int x2);
                            int.TryParse(corner2Split[1], out int y2);
                            int.TryParse(corner2Split[2], out int z2);
                            if (x1 > x2)
                            {
                                int alt = x2;
                                x2 = x1;
                                x1 = alt;
                            }
                            if (y1 > y2)
                            {
                                int alt = y2;
                                y2 = y1;
                                y1 = alt;
                            }
                            if (y1 == y2)
                            {
                                y2++;
                            }
                            if (z1 > z2)
                            {
                                int alt = z2;
                                z2 = z1;
                                z1 = alt;
                            }
                            string c1 = x1 + "," + y1 + "," + z1;
                            string c2 = x2 + "," + y2 + "," + z2;
                            string[] box = { c1, c2, destination };
                            if (!Dict.ContainsKey(name))
                            {
                                Dict.Add(name, box);
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeTravelXml(nodeList);
                        //UpgradeXml(childNodes);
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in Travel.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<Travel>");
                    sw.WriteLine(string.Format("    <!-- <Version=\"{0}\" /> -->", Config.Version));
                    sw.WriteLine("    <!-- <Location Name=\"zone1\" Corner1=\"0,100,0\" Corner2=\"10,100,10\" Destination=\"-100,-1,-100\" /> -->");
                    sw.WriteLine("    <!-- <Location Name=\"zone2\" Corner1=\"-1,100,-1\" Corner2=\"-10,100,-10\" Destination=\"100,-1,100\" /> -->");
                    sw.WriteLine("    <Location Name=\"\" Corner1=\"\" Corner2=\"\" Destination=\"\" />");
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvpBox in Dict)
                        {
                            sw.WriteLine(string.Format("    <Location Name=\"{0}\" Corner1=\"{1}\" Corner2=\"{2}\" Destination=\"{3}\" />", kvpBox.Key, kvpBox.Value[0], kvpBox.Value[1], kvpBox.Value[2]));
                        }
                    }
                    sw.WriteLine("</Travel>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.UpdateXml: {0}", e.Message));
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
                        Tele(_cInfo);
                    }
                }
                else
                {
                    DateTime lastTravel = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastTravel;
                    TimeSpan varTime = DateTime.Now - lastTravel;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                            {
                                if (DateTime.Now < dt)
                                {
                                    int delay = Delay_Between_Uses / 2;
                                    Time(_cInfo, timepassed, delay);
                                    return;
                                }
                            }
                            else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                            {
                                if (DateTime.Now < dt)
                                {
                                    int delay = Delay_Between_Uses / 2;
                                    Time(_cInfo, timepassed, delay);
                                    return;
                                }
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
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.Exec: {0}", e.Message));
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
                        Tele(_cInfo);
                    }
                }
                else
                {
                    int timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Travel3", out string phrase);
                    phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_travel}", Command_travel);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.Time: {0}", e.Message));
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
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
                    Tele(_cInfo);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Travel4", out string phrase);
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.CommandCost: {0}", e.Message));
            }
        }

        public static void Tele(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (Dict.Count > 0)
                    {
                        int x = (int)player.position.x;
                        int y = (int)player.position.y;
                        int z = (int)player.position.z;
                        foreach (KeyValuePair<string, string[]> travel in Dict)
                        {
                            string[] c1 = travel.Value[0].Split(',');
                            int.TryParse(c1[0], out int x1);
                            int.TryParse(c1[1], out int y1);
                            int.TryParse(c1[2], out int z1);
                            string[] c2 = travel.Value[1].Split(',');
                            int.TryParse(c2[0], out int x2);
                            int.TryParse(c2[1], out int y2);
                            int.TryParse(c2[2], out int z2);
                            if (x >= x1 && x <= x2 && y >= y1 && y <= y2 && z >= z1 && z <= z2)
                            {
                                if (Player_Check)
                                {
                                    if (Teleportation.PCheck(_cInfo, player))
                                    {
                                        return;
                                    }
                                }
                                if (Zombie_Check)
                                {
                                    if (Teleportation.ZCheck(_cInfo, player))
                                    {
                                        return;
                                    }
                                }
                                string[] destination = travel.Value[2].Split(',');
                                int.TryParse(destination[0], out int destinationX);
                                int.TryParse(destination[1], out int destinationY);
                                int.TryParse(destination[2], out int destinationZ);
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(destinationX, destinationY, destinationZ), null, false));
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastTravel = DateTime.Now;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Travel1", out string phrase);
                                phrase = phrase.Replace("{Destination}", travel.Value[2]);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                        }
                        Phrases.Dict.TryGetValue("Travel2", out string phrase1);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.Tele: {0}", e.Message));
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
                    sw.WriteLine("<Travel>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- <Location Name=\"zone1\" Corner1=\"0,100,0\" Corner2=\"10,100,10\" Destination=\"-100,-1,-100\" /> -->");
                    sw.WriteLine("    <!-- <Location Name=\"zone2\" Corner1=\"-1,100,-1\" Corner2=\"-10,100,-10\" Destination=\"100,-1,100\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- <Location Name=\"Zone1\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Location Name=\"Zone2\"") && !nodeList[i].OuterXml.Contains("<Location Name=\"\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version"))
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
                            if (line.HasAttributes && line.Name == "Location")
                            {
                                string name = "", c1 = "", c2 = "", destination = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Corner1"))
                                {
                                    c1 = line.GetAttribute("Corner1");
                                }
                                if (line.HasAttribute("Corner2"))
                                {
                                    c2 = line.GetAttribute("Corner2");
                                }
                                if (line.HasAttribute("Destination"))
                                {
                                    destination = line.GetAttribute("Destination");
                                }
                                sw.WriteLine(string.Format("    <Location Name=\"{0}\" Corner1=\"{1}\" Corner2=\"{2}\" Destination=\"{3}\" />", name, c1, c2, destination));
                            }
                        }
                    }
                    sw.WriteLine("</Travel>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
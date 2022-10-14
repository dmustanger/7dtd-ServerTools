using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class LandClaimCount
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static string Command_claims = "claims";

		public static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();

		private const string file = "LandClaimCount.xml";
		private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
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
                                else if (line.HasAttribute("Id") && line.HasAttribute("Name") && line.HasAttribute("Limit"))
                                {
                                    string id = line.GetAttribute("Id");
                                    string[] nameAndLimit = new string[2];
                                    nameAndLimit[0] = line.GetAttribute("Name");
                                    if (!int.TryParse(line.GetAttribute("Limit"), out int limit))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring LandClaimCount.xml entry because of invalid (integer) value for 'Limit' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    nameAndLimit[1] = line.GetAttribute("Limit");
                                    if (!Dict.ContainsKey(id))
                                    {
                                        Dict.Add(id, nameAndLimit);
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
                            Log.Out(string.Format("[SERVERTOOLS] The existing LandClaimCount.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in LandClaimCount.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<LandClaimCount>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Player Id=\"Steam_76561191234567891\" Name=\"SaladFace\" Limit=\"2\" /> -->");
                    sw.WriteLine("    <!-- <Player Id=\"EOS_7a6b5c6d1e1f9g1h2i345678911234567890\" Name=\"SaladFace\" Limit=\"2\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" Limit=\"{2}\" />", kvp.Key[0], kvp.Value[0], kvp.Value[1]));
                        }
                    }
                    sw.WriteLine("</LandClaimCount>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LandClaimCount.UpdateXml: {0}", e.Message));
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
            EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
            if (player != null)
            {
                EntityBedrollPositionList positionList = player.SpawnPoints;
                if (positionList != null)
                {
                    string claimLimit = GameStats.GetInt(EnumGameStats.LandClaimCount).ToString();
                    if (Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                    {
                        Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out string[] nameAndLimit);
                        claimLimit = nameAndLimit[1];
                    }
                    Phrases.Dict.TryGetValue("LandClaim1", out string phrase);
                    phrase = phrase.Replace("{Value1}", positionList.Count.ToString());
                    phrase = phrase.Replace("{Value2}", claimLimit);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void RemoveExtraLandClaims(PersistentPlayerData _owner)
        {
			int claimLimit = GameStats.GetInt(EnumGameStats.LandClaimCount);
			if (Dict.ContainsKey(_owner.PlatformUserIdentifier.CombinedString) || Dict.ContainsKey(_owner.UserIdentifier.CombinedString))
            {
				if (Dict.TryGetValue(_owner.PlatformUserIdentifier.CombinedString, out string[] nameAndLimit))
                {
					claimLimit = int.Parse(nameAndLimit[1]);
				}
				else if (Dict.TryGetValue(_owner.UserIdentifier.CombinedString, out nameAndLimit))
                {
					claimLimit = int.Parse(nameAndLimit[1]);
				}
			}
			int numToRemove = _owner.LPBlocks.Count - claimLimit;
			for (int i = 0; i < numToRemove; i++)
			{
				Vector3i blockPos = _owner.LPBlocks[0];
				BlockLandClaim.HandleDeactivateLandClaim(blockPos);
				_owner.LPBlocks.RemoveAt(0);
				if (GameManager.Instance.World != null)
				{
					NavObjectManager.Instance.UnRegisterNavObjectByPosition(blockPos.ToVector3(), "land_claim");
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, blockPos.ToVector3()), false, -1, -1, -1, -1);
				}
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
                    sw.WriteLine("<LandClaimCount>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Player Id=\"Steam_76561191234567891\" Name=\"SaladFace\" Limit=\"2\" /> -->");
                    sw.WriteLine("    <!-- <Player Id=\"EOS_7a6b5c6d1e1f9g1h2i345678911234567890\" Name=\"SaladFace\" Limit=\"2\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- <Player Id=\"Steam_76561191234567891\"") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Player Id=\"EOS_7a6b5c6d1e1f9g1h2i345678911234567890\"") && !OldNodeList[i].OuterXml.Contains("    <!-- <Player Id=\"\""))
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
                            if (line.HasAttributes && line.Name == "Player")
                            {
                                string id = "", name = "", limit = "";
                                if (line.HasAttribute("Id"))
                                {
                                    id = line.GetAttribute("Id");
                                }
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Limit"))
                                {
                                    limit = line.GetAttribute("Limit");
                                }
                                sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" Limit=\"{2}\" />", id, name, limit));
                            }
                        }
                    }
                    sw.WriteLine("</LandClaimCount>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LandClaimCount.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class UndergroundCheck : ConsoleCmdAbstract
    {
        public static bool IsEnabled = false, IsRunning = false, Kill_Player = false, Announce = false, Jail_Enabled = false, Kick_Enabled = false,
            Ban_Enabled = false;
        public static int Admin_Level = 0, Max_Ping = 300, Days_Before_Log_Delete = 5;
        private const string file = "UndergroundBlocks.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static Dictionary<string, int> Flag = new Dictionary<string, int>();
        public static Dictionary<int, int[]> uLastPositionXZ = new Dictionary<int, int[]>();
        public static List<string> dict = new List<string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false;

        public override string GetDescription()
        {
            return "Check if a player is underground with no clip or stuck";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. uc <steam id / player name / entity id> \n" +
                "  2. uc \n\n" +
                "1. List 1 players underground check\n" +
                "2. List all players underground check\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-UndergroundCheck", "undergroundcheck", "uc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 1)
                {
                    SdtdConsole.Instance.Output("Wrong number of arguments, expected 0 or 1, found " + _params.Count + ".");
                    return;
                }
                else
                {
                    if (_params.Count == 1)
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (_cInfo == null)
                        {
                            SdtdConsole.Instance.Output("Playername or entity/steamid id not found.");
                            return;
                        }
                        EntityPlayer ep = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        SdtdConsole.Instance.Output("UC: entity_id=" + ep.entityId + " isUnderGround=" + getPlayerUnderground(ep));
                    }
                    else
                    {
                        World world = GameManager.Instance.World;
                        List<EntityPlayer>.Enumerator enumerator2 = world.Players.list.GetEnumerator();
                        using (List<EntityPlayer>.Enumerator enumerator = enumerator2)
                        {
                            while (enumerator.MoveNext())
                            {
                                EntityPlayer ep = enumerator.Current;
                                SdtdConsole.Instance.Output("UC: entity_id=" + ep.entityId + " isUnderGround=" + getPlayerUnderground(ep));
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Log.Out("Error in UndergroundCheck.Run: " + e);
            }
        }

        public bool getPlayerUnderground(EntityPlayer ep)
        {
            int x = (int)ep.position.x;
            int y = (int)ep.position.y;
            int z = (int)ep.position.z;
            for (int i = x - 2; i <= (x + 2); i++)
            {
                for (int j = z - 2; j <= (z + 2); j++)
                {
                    for (int k = y - 3; k <= (y + 2); k++)
                    {
                        BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        if (Block.type == BlockValue.Air.type || ep.IsInElevator() || ep.IsInWater())
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static void DetectionLogsDir()
        {
            if (!Directory.Exists(API.GamePath + "/DetectionLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/DetectionLogs");
            }
            string[] files = Directory.GetFiles(API.GamePath + "/DetectionLogs");
            int _daysBeforeDeleted = (Days_Before_Log_Delete * -1);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                {
                    fi.Delete();
                }
            }
        }

        public static void AutoUndergroundCheck()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                    AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                    if (Admin.PermissionLevel > Admin_Level)
                    {
                        World world = GameManager.Instance.World;
                        List<EntityPlayer>.Enumerator enumerator2 = world.Players.list.GetEnumerator();
                        using (List<EntityPlayer>.Enumerator enumerator = enumerator2)
                            while (enumerator.MoveNext())
                            {
                                EntityPlayer ep = enumerator.Current;
                                if (ep.entityId == _cInfo.entityId && ep.AttachedToEntity == null)
                                {
                                    var playerInGround = autoGetPlayerUnderground(ep, _cInfo);
                                    if (playerInGround == true)
                                    {
                                        if (!Flag.ContainsKey(_cInfo.playerId))
                                        {
                                            Flag.Add(_cInfo.playerId, 1);
                                        }
                                        else
                                        {
                                            int _flag = 0;
                                            if (Flag.TryGetValue(_cInfo.playerId, out _flag))
                                            {
                                                int x = (int)ep.position.x;
                                                int y = (int)ep.position.y;
                                                int z = (int)ep.position.z;
                                                {
                                                    Flag.Remove(_cInfo.playerId);
                                                    Flag.Add(_cInfo.playerId, _flag + 1);
                                                }
                                                if (_flag == 2)
                                                {
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} -1 {2}", ep.entityId, x, z), (ClientInfo)null);
                                                    Penalty(_cInfo);
                                                    Log.Warning(string.Format("[SERVERTOOLS] Detected {0}, Steam Id {1}, flying underground @ {2} {3} {4}. ", _cInfo.playerName, _cInfo.steamId, x, y, z));
                                                    string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                                    string _filepath = string.Format("{0}/DetectionLogs/{1}", API.GamePath, _file);
                                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, flying underground @ {2} {3} {4}. ", _cInfo.playerName, _cInfo.steamId, x, y, z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }                                                    
                                                    List<ClientInfo> _cInfoList2 = ConnectionManager.Instance.GetClients();
                                                    foreach (var _cInfo2 in _cInfoList2)
                                                    {
                                                        AdminToolsClientInfo Admin2 = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo2.playerId);
                                                        if (Admin2.PermissionLevel <= Admin_Level)
                                                        {
                                                            string _phrase710;
                                                            if (!Phrases.Dict.TryGetValue(710, out _phrase710))
                                                            {
                                                                _phrase710 = "Detected {PlayerName} flying underground @ {X} {Y} {Z}.";
                                                            }
                                                            _phrase710 = _phrase710.Replace("{PlayerName}", _cInfo.playerName);
                                                            _phrase710 = _phrase710.Replace("{X}", x.ToString());
                                                            _phrase710 = _phrase710.Replace("{Y}", y.ToString());
                                                            _phrase710 = _phrase710.Replace("{Z}", z.ToString());
                                                            _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase710), Config.Server_Response_Name, false, "ServerTools", false));
                                                        }
                                                    }
                                                    Flag.Remove(_cInfo.playerId);
                                                    if (Announce)
                                                    {
                                                        string _phrase711;
                                                        if (!Phrases.Dict.TryGetValue(711, out _phrase711))
                                                        {
                                                            _phrase711 = "{PlayerName} has been detected flying underground.";
                                                        }
                                                        _phrase711 = _phrase711.Replace("{PlayerName}", _cInfo.playerName);
                                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase711), Config.Server_Response_Name, false, "", false);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Flag.ContainsKey(_cInfo.playerId))
                                        {
                                            Flag.Remove(_cInfo.playerId);
                                        }
                                    }
                                }
                            }
                    }
                }
            }
        }

        public static bool autoGetPlayerUnderground(EntityPlayer ep, ClientInfo _cInfo)
        {
            int x = (int)ep.position.x;
            int y = (int)ep.position.y;
            int z = (int)ep.position.z;

            int Id = ep.entityId;
            int[] xz = { x, z };

            if (uLastPositionXZ.ContainsKey(Id))
            {
                int[] last_xz_pos;
                uLastPositionXZ.TryGetValue(Id, out last_xz_pos);
                if (last_xz_pos != xz)
                {
                    uLastPositionXZ[Id] = xz;
                    if (_cInfo.ping < Max_Ping)
                    {
                        for (int i = x - 2; i <= (x + 2); i++)
                        {
                            for (int j = z - 2; j <= (z + 2); j++)
                            {
                                for (int k = y - 3; k <= (y + 2); k++)
                                {
                                    BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                                    string _blockName = Block.Block.GetBlockName();
                                    if (Block.type == BlockValue.Air.type || ep.IsInElevator() || ep.IsInWater() || dict.Contains(_blockName))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Flag.Remove(_cInfo.playerId);
                    uLastPositionXZ[Id] = xz;
                    return false;
                }
            }
            else
            {
                uLastPositionXZ[Id] = xz;
                return false;
            }
        }

        public static void Penalty(ClientInfo _cInfo)
        {
            if (Jail_Enabled)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been jailed for flying underground[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0}", _cInfo.playerId), (ClientInfo)null);
            }
            if (Kill_Player)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been killed for flying underground[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), (ClientInfo)null);
            }
            if (Kick_Enabled)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been kicked for flying underground[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for flying\"", _cInfo.playerId), (ClientInfo)null);
            }
            if (Ban_Enabled)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been banned for flying underground[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for flying\"", _cInfo.playerId), (ClientInfo)null);
            }
        }

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
                dict.Clear();
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
                if (childNode.Name == "blocks")
                {
                    dict.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'blocks' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring blocks entry because of missing name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _blockName = _line.GetAttribute("name");
                        if (!dict.Contains(_blockName))
                        {
                            dict.Add(_blockName);
                        }
                    }
                }
            }
            if (updateConfig)
            {
                updateConfig = false;
                UpdateXml();
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<UndergroundCheck>");
                sw.WriteLine("    <blocks>");
                if (dict.Count > 0)
                {
                    for (int i = 0; i < dict.Count; i++)
                    {
                        string _blockName = dict[i];
                        sw.WriteLine(string.Format("        <block name=\"{0}\" />", _blockName));
                    }
                }
                else
                {
                    sw.WriteLine("        <block name=\"plantedBlueberry1\" />");
                    sw.WriteLine("        <block name=\"plantedBlueberry3Harvest\" />");
                    sw.WriteLine("        <block name=\"plantedPotato1\" />");
                    sw.WriteLine("        <block name=\"plantedPotato2\" />");
                    sw.WriteLine("        <block name=\"plantedPotato3Harvest\" />");
                    sw.WriteLine("        <block name=\"plantedBlueberry2\" />");
                    sw.WriteLine("        <block name=\"ladderMetal\" />");
                    sw.WriteLine("        <block name=\"plantedHop1\" />");
                    sw.WriteLine("        <block name=\"plantedHop2\" />");
                    sw.WriteLine("        <block name=\"plantedHop3Harvest\" />");
                    sw.WriteLine("        <block name=\"plantedMushroom1\" />");
                    sw.WriteLine("        <block name=\"plantedMushroom2\" />");
                    sw.WriteLine("        <block name=\"mushroom3Harvest\" />");
                    sw.WriteLine("        <block name=\"plantedYucca1\" />");
                    sw.WriteLine("        <block name=\"plantedYucca2\" />");
                    sw.WriteLine("        <block name=\"secureDoorWooden\" />");
                    sw.WriteLine("        <block name=\"secureReinforcedDoorWooden\" />");
                    sw.WriteLine("        <block name=\"metalReinforcedDoorWooden\" />");
                    sw.WriteLine("        <block name=\"plantedCotton1\" />");
                    sw.WriteLine("        <block name=\"plantedCotton3Harvest\" />");
                    sw.WriteLine("        <block name=\"plantedCoffee1\" />");
                    sw.WriteLine("        <block name=\"plantedCoffee2\" />");
                    sw.WriteLine("        <block name=\"plantedCoffee3Harvest\" />");
                    sw.WriteLine("        <block name=\"plantedGoldenrod1\" />");
                    sw.WriteLine("        <block name=\"plantedGoldenrod2\" />");
                    sw.WriteLine("        <block name=\"plantedGoldenrod3Harvest\" />");
                    sw.WriteLine("        <block name=\"goldenrodPlant\" />");
                    sw.WriteLine("        <block name=\"ladderWood\" />");
                    sw.WriteLine("        <block name=\"water\" />");
                    sw.WriteLine("        <block name=\"plantedYucca3Harvest\" />");
                    sw.WriteLine("        <block name=\"plantedAloe3Harvest\" />");
                    sw.WriteLine("        <block name=\"plantedAloe1\" />");
                    sw.WriteLine("        <block name=\"plantedAloe2\" />");
                    sw.WriteLine("        <block name=\"plantedChrysanthemum1\" />");
                    sw.WriteLine("        <block name=\"plantedChrysanthemum2\" />");
                    sw.WriteLine("        <block name=\"plantedChrysanthemum3Harvest\" />");
                    sw.WriteLine("        <block name=\"plantedCorn1\" />");
                    sw.WriteLine("        <block name=\"plantedCorn2\" />");
                    sw.WriteLine("        <block name=\"plantedCorn3Harvest\" />");
                    sw.WriteLine("        <block name=\"plantedCornDead\" />");
                    sw.WriteLine("        <block name=\"ladderSteel\" />");
                    sw.WriteLine("        <block name=\"ironDoor1_v1\" />");
                    sw.WriteLine("        <block name=\"ironDoor1_v2\" />");
                    sw.WriteLine("        <block name=\"ironDoor1_v3\" />");
                    sw.WriteLine("        <block name=\"woodHatch1_v1\" />");
                    sw.WriteLine("        <block name=\"woodHatch1_v2\" />");
                    sw.WriteLine("        <block name=\"woodHatch1_v3\" />");
                    sw.WriteLine("        <block name=\"vaultDoor01\" />");
                    sw.WriteLine("        <block name=\"vaultDoor02\" />");
                    sw.WriteLine("        <block name=\"vaultDoor03\" />");
                    sw.WriteLine("        <block name=\"scrapHatch_v1\" />");
                    sw.WriteLine("        <block name=\"scrapHatch_v2\" />");
                    sw.WriteLine("        <block name=\"scrapHatch_v3\" />");
                    sw.WriteLine("        <block name=\"vaultHatch_v1\" />");
                    sw.WriteLine("        <block name=\"vaultHatch_v2\" />");
                    sw.WriteLine("        <block name=\"vaultHatch_v3\" />");
                    sw.WriteLine("        <block name=\"waterSource8\" />");
                    sw.WriteLine("        <block name=\"bedroll\" />");
                }
                sw.WriteLine("    </blocks>");
                sw.WriteLine("</UndergroundCheck>");
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
    }
}
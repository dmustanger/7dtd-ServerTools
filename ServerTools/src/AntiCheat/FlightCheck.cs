using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class FlightCheck : ConsoleCmdAbstract
    {
        public static bool IsEnabled = false, Kill_Player = false, Announce = false, Jail_Enabled = false, Kick_Enabled = false, Ban_Enabled = false;
        public static int Admin_Level = 0, Max_Height = 2, Max_Ping = 300, Days_Before_Log_Delete = 5;
        public static Dictionary<string, int> Flag = new Dictionary<string, int>();
        public static Dictionary<int, int> fLastPositionY = new Dictionary<int, int>();
        public static Dictionary<int, int[]> fLastPositionXZ = new Dictionary<int, int[]>();

        public override string GetDescription()
        {
            return "Check if a player is flying";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. fc <steam id / player name / entity id> \n" +
                "  2. fc \n\n" +
                "1. List 1 player's height from the ground\n" +
                "2. List all player's height from the ground\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "flightcheck", "fc" };
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
                        SdtdConsole.Instance.Output("FC: entity_id=" + ep.entityId + " dist=" + getFlightCheck(ep));
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
                                SdtdConsole.Instance.Output("FC: entity_id=" + ep.entityId + " dist=" + getFlightCheck(ep));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in FlightCheck.Run: " + e);
            }
        }

        public static int getFlightCheck(EntityPlayer ep)
        {
            int x = (int)ep.position.x;
            int y = (int)ep.position.y;
            int z = (int)ep.position.z;

            int height = 0;
            for (int k = y; k > 0; k--)
            {
                Boolean groundFound = false;
                for (int i = x - 2; i <= (x + 2); i++)
                {
                    for (int j = z - 2; j <= (z + 2); j++)
                    {
                        BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        if (Block.type != BlockValue.Air.type)
                        {
                            groundFound = true;
                            break;
                        }
                    }
                    if (groundFound)
                    {
                        break;
                    }
                }
                if (groundFound)
                {
                    break;
                }
                height++;
            }
            return height;
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

        private static bool autoGetFlightCheck(EntityPlayer ep, ClientInfo _cInfo)
        {
            int x = (int)ep.position.x;
            int y = (int)ep.position.y;
            int z = (int)ep.position.z;

            int Id = ep.entityId;
            int[] xz = { x, z };

            if (fLastPositionY.ContainsKey(Id))
            {
                int last_y_pos;
                fLastPositionY.TryGetValue(Id, out last_y_pos);
                int y_change = (last_y_pos - y);
                if (y_change >= 10)
                {
                    fLastPositionXZ.Remove(Id);
                }
                fLastPositionY[Id] = y;
            }
            else
            {
                fLastPositionY[Id] = y;
            }

            if (fLastPositionXZ.ContainsKey(Id))
            {
                int[] last_xz_pos;
                fLastPositionXZ.TryGetValue(Id, out last_xz_pos);
                if (last_xz_pos != xz)
                {
                    fLastPositionXZ[Id] = xz;
                    if (_cInfo.ping < Max_Ping)
                    {
                        for (int k = y - Max_Height; k <= (y + 2); k++)
                        {
                            for (int i = x - 2; i <= (x + 2); i++)
                            {
                                for (int j = z - 2; j <= (z + 2); j++)
                                {
                                    BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                                    if (Block.type != BlockValue.Air.type)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        return true;
                    }
                    return false;
                }
                else
                {
                    Flag.Remove(_cInfo.playerId);
                    fLastPositionXZ[Id] = xz;
                    return false;
                }
            }
            else
            {
                fLastPositionXZ[Id] = xz;
                return false;
            }
        }

        public static void AutoFlightCheck()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                World world = GameManager.Instance.World;
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                    AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                    if (Admin.PermissionLevel > Admin_Level)
                    {                       
                        List<EntityPlayer>.Enumerator enumerator2 = world.Players.list.GetEnumerator();
                        using (List<EntityPlayer>.Enumerator enumerator = enumerator2)
                            while (enumerator.MoveNext())
                            {
                                EntityPlayer ep = enumerator.Current;
                                if (ep.entityId == _cInfo.entityId && ep.AttachedToEntity == null)
                                {
                                    var playerDistanceFromGround = autoGetFlightCheck(ep, _cInfo);
                                    if (playerDistanceFromGround == true)
                                    {
                                        if (!Flag.ContainsKey(_cInfo.playerId))
                                        {
                                            Flag[_cInfo.playerId] = 1;
                                        }
                                        else
                                        {
                                            int _flag;
                                            if (Flag.TryGetValue(_cInfo.playerId, out _flag))
                                            {
                                                Flag[_cInfo.playerId] = _flag + 1;
                                                if (_flag + 1 >= 2)
                                                {
                                                    int x = (int)ep.position.x;
                                                    int y = (int)ep.position.y;
                                                    int z = (int)ep.position.z;
                                                    ep.SetPosition(new UnityEngine.Vector3(x, -1, z));
                                                    Penalty(_cInfo);
                                                    Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, flying @ {2} {3} {4}. ", _cInfo.playerName, _cInfo.playerId, x, y, z);
                                                    string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                                    string _filepath = string.Format("{0}/DetectionLogs/{1}", API.GamePath, _file);
                                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, flying @ {2} {3} {4}. ", _cInfo.playerName, _cInfo.playerId, x, y, z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                    for (int j = 0; j < _cInfoList.Count; j++)
                                                    {
                                                        ClientInfo _cInfo2 = _cInfoList[j];
                                                        AdminToolsClientInfo Admin2 = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo2.playerId);
                                                        if (Admin2.PermissionLevel <= Admin_Level)
                                                        {
                                                            string _phrase705;
                                                            if (!Phrases.Dict.TryGetValue(705, out _phrase705))
                                                            {
                                                                _phrase705 = "Cheat Detected: {PlayerName} flying @ {X} {Y} {Z}";
                                                            }
                                                            _phrase705 = _phrase705.Replace("{PlayerName}", _cInfo.playerName);
                                                            _phrase705 = _phrase705.Replace("{X}", x.ToString());
                                                            _phrase705 = _phrase705.Replace("{Y}", y.ToString());
                                                            _phrase705 = _phrase705.Replace("{Z}", z.ToString());
                                                            _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase705), Config.Server_Response_Name, false, "ServerTools", false));
                                                        }
                                                    }
                                                    Flag.Remove(_cInfo.playerId);
                                                    if (Announce)
                                                    {
                                                        string _phrase706;
                                                        if (!Phrases.Dict.TryGetValue(706, out _phrase706))
                                                        {
                                                            _phrase706 = "Cheat Detected: {PlayerName} has been detected flying.";
                                                        }
                                                        _phrase706 = _phrase706.Replace("{PlayerName}", _cInfo.playerName);
                                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase706), Config.Server_Response_Name, false, "ServerTools", false);
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

        public static void Penalty(ClientInfo _cInfo)
        {
            if (Jail_Enabled)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0} has been jailed for flying[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0}", _cInfo.playerId), _cInfo);
            }
            if (Kill_Player)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0} has been killed for flying[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), (ClientInfo)null);
            }
            if (Kick_Enabled)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0} has been kicked for flying[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for flying\"", _cInfo.playerId), (ClientInfo)null);
            }
            if (Ban_Enabled)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0} has been banned for flying[-]", _cInfo.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for flying\"", _cInfo.playerId), (ClientInfo)null);
            }
        }
    }
}






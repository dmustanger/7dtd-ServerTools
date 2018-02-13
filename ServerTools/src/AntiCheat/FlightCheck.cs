using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace ServerTools
{
    class FlightCheck : ConsoleCmdAbstract
    {
        private static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        public static bool KillPlayer = false;
        public static bool Announce = false;
        public static bool JailEnabled = false;
        public static bool KickEnabled = false;
        public static bool BanEnabled = false;
        public static int AdminLevel = 0;
        public static int MaxHeight = 2;
        public static int MaxPing = 300;
        public static int DaysBeforeDeleted = 5;
        public static SortedDictionary<string, int> Flag = new SortedDictionary<string, int>();
        public static SortedDictionary<int, int> fLastPositionY = new SortedDictionary<int, int>();
        public static SortedDictionary<int, string> fLastPositionXZ = new SortedDictionary<int, string>();
        private static System.Timers.Timer t = new System.Timers.Timer();

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
                        ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (ci == null)
                        {
                            SdtdConsole.Instance.Output("Playername or entity/steamid id not found.");
                            return;
                        }
                        EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];

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
            int _daysBeforeDeleted = (DaysBeforeDeleted * -1);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                {
                    fi.Delete();
                }
            }
        }

        private static bool autoGetFlightCheck(EntityPlayer ep)
        {
            int x = (int)ep.position.x;
            int y = (int)ep.position.y;
            int z = (int)ep.position.z;

            int Id = ep.entityId;
            string xz = (x.ToString() + z.ToString());

            if (fLastPositionY.ContainsKey(Id))
            {
                int last_y_pos;
                fLastPositionY.TryGetValue(Id, out last_y_pos);
                int y_change = (last_y_pos - y);
                if (y_change >= 10)
                {
                    fLastPositionY.Remove(Id);
                    fLastPositionY.Add(Id, y);
                    fLastPositionXZ.Remove(Id);
                }
                else
                {
                    fLastPositionY.Remove(Id);
                    fLastPositionY.Add(Id, y);
                }
            }
            else
            {
                fLastPositionY.Remove(Id);
                fLastPositionY.Add(Id, y);
            }

            if (fLastPositionXZ.ContainsKey(Id))
            {
                string last_xz_pos;
                fLastPositionXZ.TryGetValue(Id, out last_xz_pos);
                if (last_xz_pos != xz)
                {
                    fLastPositionXZ.Remove(Id);
                    fLastPositionXZ.Add(Id, xz);
                    ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(ep.entityId);
                    if (_cInfo.ping < MaxPing)
                    {
                        for (int k = y - MaxHeight; k <= (y + 2); k++)
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
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    fLastPositionXZ.Remove(Id);
                    fLastPositionXZ.Add(Id, xz);
                    return false;
                }
            }
            else
            {
                fLastPositionXZ.Remove(Id);
                fLastPositionXZ.Add(Id, xz);
                return false;
            }
        }

        public static void FlightTimerStart()
        {
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {
                int d = 2500;
                t.Interval = d;
                t.Start();
                t.Elapsed += new ElapsedEventHandler(AutoFlightCheck);
            }
        }

        public static void FlightTimerStop()
        {
            t.Stop();
        }

        public static void AutoFlightCheck(object sender, ElapsedEventArgs e)
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (var _cInfo in _cInfoList)
                {
                    GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                    AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                    if (Admin.PermissionLevel > AdminLevel)
                    {
                        World world = GameManager.Instance.World;
                        List<EntityPlayer>.Enumerator enumerator2 = world.Players.list.GetEnumerator();
                        using (List<EntityPlayer>.Enumerator enumerator = enumerator2)
                        {
                            while (enumerator.MoveNext())
                            {
                                EntityPlayer ep = enumerator.Current;
                                if (ep.entityId == _cInfo.entityId && !ep.AttachedToEntity)
                                {
                                    var playerDistanceFromGround = autoGetFlightCheck(ep);
                                    if (playerDistanceFromGround == true)
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
                                                {
                                                    Flag.Remove(_cInfo.playerId);
                                                    Flag.Add(_cInfo.playerId, _flag + 1);
                                                }
                                                if (_flag > 1)
                                                {
                                                    int x = (int)ep.position.x;
                                                    int y = (int)ep.position.y;
                                                    int z = (int)ep.position.z;

                                                    Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, flying @ {2} {3} {4}. ", _cInfo.playerName, _cInfo.steamId, x, y, z);
                                                    string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                                    string _filepath = string.Format("{0}/DetectionLogs/{1}", API.GamePath, _file);
                                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, flying @ {2} {3} {4}. ", _cInfo.playerName, _cInfo.steamId, x, y, z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                    if (Announce)
                                                    {
                                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been detected flying[-]", Config.ChatColor, _cInfo.playerName), "Server", false, "", false);
                                                        if (_flag == 4)
                                                        {
                                                            Flag.Remove(_cInfo.playerId);
                                                            if (Admin.PermissionLevel <= AdminLevel && ep.entityId != _cInfo.entityId)
                                                            {
                                                                SdtdConsole.Instance.ExecuteSync(string.Format("pm {0} \"{1}Detected {2} flying @ {3} {4} {5}\"", _cInfo.playerId, Config.ChatColor, ep.EntityName, x, y, z), (ClientInfo)null);
                                                            }
                                                        }
                                                    }
                                                    if (JailEnabled)
                                                    {
                                                        Flag.Remove(_cInfo.playerId);
                                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been jailed for flying[-]", Config.ChatColor, _cInfo.playerName), "Server", false, "", false);
                                                        SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0}", _cInfo.playerId), _cInfo);
                                                    }
                                                    if (KillPlayer)
                                                    {
                                                        Flag.Remove(_cInfo.playerId);
                                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been killed for flying[-]", Config.ChatColor, _cInfo.playerName), "Server", false, "", false);
                                                        SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), (ClientInfo)null);
                                                    }
                                                    if (KickEnabled)
                                                    {
                                                        Flag.Remove(_cInfo.playerId);
                                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been kicked for flying[-]", Config.ChatColor, _cInfo.playerName), "Server", false, "", false);
                                                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for flying\"", _cInfo.playerId), (ClientInfo)null);
                                                    }
                                                    if (BanEnabled)
                                                    {
                                                        Flag.Remove(_cInfo.playerId);
                                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been banned for flying[-]", Config.ChatColor, _cInfo.playerName), "Server", false, "", false);
                                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for flying\"", _cInfo.playerId), (ClientInfo)null);
                                                    }
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} -1 {2}", ep.entityId, x, z), (ClientInfo)null);
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
        }
    }
}






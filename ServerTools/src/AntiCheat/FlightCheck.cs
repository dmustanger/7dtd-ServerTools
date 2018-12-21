﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class FlightCheck : ConsoleCmdAbstract
    {
        public static bool IsEnabled = false, Kill_Player = false, Announce = false, Jail_Enabled = false, Kick_Enabled = false, Ban_Enabled = false;
        public static int Admin_Level = 0, Max_Height = 2, Max_Ping = 300, Days_Before_Log_Delete = 5;
        public static Dictionary<int, int> Flag = new Dictionary<int, int>();
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
                if (y_change >= 4)
                {
                    fLastPositionXZ.Remove(Id);
                }
                fLastPositionY[Id] = y;
            }
            else
            {
                fLastPositionY.Add(Id, y);
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
                }
                return false;
            }
            else
            {
                fLastPositionXZ.Add(Id, xz);
                return false;
            }
        }

        public static void AutoFlightCheck()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                World world = GameManager.Instance.World;
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    if (_cInfo != null)
                    {
                        GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                        AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                        if (Admin.PermissionLevel > Admin_Level)
                        {
                            if (!Players.NoFlight.Contains(_cInfo.entityId))
                            {
                                EntityPlayer ep = world.Players.dict[_cInfo.entityId];
                                if (autoGetFlightCheck(ep, _cInfo) && ep.AttachedToEntity == null)
                                {
                                    if (!Flag.ContainsKey(_cInfo.entityId))
                                    {
                                        Flag.Add(_cInfo.entityId, 1);
                                    }
                                    else
                                    {
                                        int _value;
                                        if (Flag.TryGetValue(_cInfo.entityId, out _value))
                                        {
                                            if (_value == 1)
                                            {
                                                Flag[_cInfo.entityId] = 2;
                                            }
                                            else
                                            {
                                                int x = (int)ep.position.x;
                                                int y = (int)ep.position.y;
                                                int z = (int)ep.position.z;
                                                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, -1, z), null, false));
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
                                                    ClientInfo _cInfo1 = _cInfoList[j];
                                                    AdminToolsClientInfo Admin1 = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo1.playerId);
                                                    if (Admin1.PermissionLevel <= Admin_Level)
                                                    {
                                                        string _phrase706;
                                                        if (!Phrases.Dict.TryGetValue(706, out _phrase706))
                                                        {
                                                            _phrase706 = "Cheat Detected: {PlayerName} flying @ {X} {Y} {Z}";
                                                        }
                                                        _phrase706 = _phrase706.Replace("{PlayerName}", _cInfo.playerName);
                                                        _phrase706 = _phrase706.Replace("{X}", x.ToString());
                                                        _phrase706 = _phrase706.Replace("{Y}", y.ToString());
                                                        _phrase706 = _phrase706.Replace("{Z}", z.ToString());
                                                        ChatHook.ChatMessage(null, "[FF0000]" + _phrase706 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                                    }
                                                }
                                                if (Announce)
                                                {
                                                    string _phrase707;
                                                    if (!Phrases.Dict.TryGetValue(707, out _phrase707))
                                                    {
                                                        _phrase707 = "Cheat Detected: {PlayerName} has been detected flying.";
                                                    }
                                                    _phrase707 = _phrase707.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, "[FF0000]" + _phrase707 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Flag.Remove(_cInfo.entityId);
                                }
                            }
                            else
                            {
                                Flag.Remove(_cInfo.entityId);
                            }
                        }
                    }
                }
            }
        }

        public static void Penalty(ClientInfo _cInfo)
        {
            Flag.Remove(_cInfo.entityId);
            if (Jail_Enabled && Jail.IsEnabled && Jail.Jail_Position != "0,0,0")
            {
                string _message = "[FF0000]{PlayerName} has been jailed for flying.";
                _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0}", _cInfo.playerId), _cInfo);
            }
            if (Kill_Player)
            {
                string _message = "[FF0000]{PlayerName} has been killed for flying.";
                _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), (ClientInfo)null);
            }
            if (Kick_Enabled)
            {
                string _message = "[FF0000]{PlayerName} has been kicked for flying.";
                _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for flying\"", _cInfo.playerId), (ClientInfo)null);
            }
            if (Ban_Enabled)
            {
                string _message = "[FF0000]{PlayerName} has been banned for flying.";
                _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for flying\"", _cInfo.playerId), (ClientInfo)null);
            }
        }
    }
}






using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ServerTools.AntiCheat
{
    class PlayerChecks
    {
        public static bool GodEnabled = false, FlyEnabled = false, SpectatorEnabled = false, WaterEnabled = false;
        public static int Flying_Admin_Level = 0, Godmode_Admin_Level, Spectator_Admin_Level, Flying_Flags = 4;
        private static Dictionary<int, int> Flag = new Dictionary<int, int>();
        private static Dictionary<int, float> OldY = new Dictionary<int, float>();
        private static List<int> WaterFlag = new List<int>();

        public static void Exec()
        {
            try
            {
                if (GameManager.Instance.World.Players.dict.Count > 0)
                {
                    List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                    if (_cInfoList != null && _cInfoList.Count > 0)
                    {
                        for (int i = 0; i < _cInfoList.Count; i++)
                        {
                            ClientInfo _cInfo = _cInfoList[i];
                            if (_cInfo != null && _cInfo.playerId != null)
                            {
                                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                                if (_player != null)
                                {
                                    if (SpectatorEnabled && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Spectator_Admin_Level)
                                    {
                                        if (_player.IsSpectator)
                                        {
                                            string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                            string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                            using (StreamWriter sw = new StreamWriter(_filepath, true))
                                            {
                                                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, using spectator mode @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, using spectator mode @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                                            ChatHook.ChatMessage(null, "[FF0000]" + "Cheater! " + _cInfo.playerName + " detected using spectator mode!" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for spectator mode\"", _cInfo.playerId), null);
                                            string _message = "[FF0000]{PlayerName} has been banned for spectator mode.";
                                            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                                            ChatHook.ChatMessage(null, _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            continue;
                                        }
                                    }
                                    if (GodEnabled && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Godmode_Admin_Level)
                                    {
                                        if (_player.Buffs.HasBuff("god"))
                                        {
                                            ChatHook.ChatMessage(null, "[FF0000]" + "Cheater! " + _cInfo.playerName + " detected using god mode!" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for god mode\"", _cInfo.playerId), null);
                                            string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                            string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                            using (StreamWriter sw = new StreamWriter(_filepath, true))
                                            {
                                                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, using god mode @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, using god mode @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                                            string _message = "[FF0000]{PlayerName} has been banned for god mode.";
                                            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                                            ChatHook.ChatMessage(null, _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            continue;
                                        }
                                    }
                                    if (FlyEnabled && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Flying_Admin_Level)
                                    {
                                        if (_player.IsSpawned() && _player.IsAlive() && !_player.IsStuck && _player.AttachedToEntity == null)
                                        {

                                            if ((!_player.onGround && AirCheck(_player.position.x, _player.position.y, _player.position.z)) || GroundCheck(_player.position.x, _player.position.y, _player.position.z))
                                            {
                                                if (OldY.ContainsKey(_cInfo.entityId))
                                                {
                                                    OldY.TryGetValue(_cInfo.entityId, out float lastY);
                                                    OldY[_cInfo.entityId] = _player.position.y;
                                                    if (lastY - _player.position.y >= 4)
                                                    {
                                                        if (Flag.ContainsKey(_cInfo.entityId))
                                                        {
                                                            Flag.Remove(_cInfo.entityId);
                                                        }
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    OldY.Add(_cInfo.entityId, _player.position.y);
                                                }
                                                if (Flag.TryGetValue(_cInfo.entityId, out int _flags))
                                                {
                                                    if (_flags + 1 >= Flying_Flags)
                                                    {
                                                        Flag.Remove(_cInfo.entityId);
                                                        string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                                        string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                        {
                                                            sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, flying @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                            sw.WriteLine();
                                                            sw.Flush();
                                                            sw.Close();
                                                        }
                                                        ChatHook.ChatMessage(null, "[FF0000]" + "Cheater! Player " + _cInfo.playerName + " detected flying!" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                                        Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, flying @ {2} {3} {4}. Steam Id has been banned", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for flying\"", _cInfo.playerId), null);
                                                        string _message = "[FF0000]{PlayerName} has been banned for flying.";
                                                        _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                                                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        Flag[_cInfo.entityId] = _flags + 1;
                                                    }
                                                }
                                                else
                                                {
                                                    Flag.Add(_cInfo.entityId, 1);
                                                }
                                            }
                                            else if (Flag.ContainsKey(_cInfo.entityId))
                                            {
                                                Flag.Remove(_cInfo.entityId);
                                            }
                                        }
                                        else
                                        {
                                            if (OldY.ContainsKey(_cInfo.entityId))
                                            {
                                                OldY.Remove(_cInfo.entityId);
                                            }
                                            if (Flag.ContainsKey(_cInfo.entityId))
                                            {
                                                Flag.Remove(_cInfo.entityId);
                                            }
                                        }
                                    }
                                    if (WaterEnabled && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > 0)
                                    {
                                        if (_player.IsSpawned() && _player.IsAlive() && !_player.IsStuck && _player.AttachedToEntity == null)
                                        {
                                            int x = (int)_player.position.x;
                                            int y = (int)_player.position.y;
                                            int z = (int)_player.position.z;
                                            if (WaterCheck(_cInfo, _player, x, y, z))
                                            {
                                                if (WaterFlag.Contains(_cInfo.entityId))
                                                {
                                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, -1, z), null, false));
                                                    WaterFlag.Remove(_cInfo.entityId);
                                                }
                                                else
                                                {
                                                    WaterFlag.Add(_cInfo.entityId);
                                                }
                                            }
                                            else
                                            {
                                                if (WaterFlag.Contains(_cInfo.entityId))
                                                {
                                                    WaterFlag.Remove(_cInfo.entityId);
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerChecks.Exec: {0}", e.Message));
            }
        }

        private static bool AirCheck(float x, float y, float z)
        {
            for (float k = y - 2.8f; k <= (y + 1.5f); k++)
            {
                for (float i = x - 1f; i <= (x + 1f); i++)
                {
                    for (float j = z - 1f; j <= (z + 1f); j++)
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

        private static bool GroundCheck(float x, float y, float z)
        {
            for (float k = y - 1f; k <= (y + 1.5f); k++)
            {
                for (float i = x - 1f; i <= (x + 1f); i++)
                {
                    for (float j = z - 1f; j <= (z + 1f); j++)
                    {
                        BlockValue block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        if (block.Block.shape.ToString() != "Terrain")
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static bool WaterCheck(ClientInfo _cInfo, EntityPlayer _player, int x, int y, int z)
        {
            for (int i = x - 1; i <= (x + 1); i++)
            {
                for (int j = z - 1; j <= (z + 1); j++)
                {
                    for (int k = y - 0; k <= (y + 2); k++)
                    {
                        BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        string _blockName = Block.Block.GetBlockName();
                        if (_blockName != "waterMoving" || _blockName != "water")
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}

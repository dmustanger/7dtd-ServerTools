using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools.AntiCheat
{
    class PlayerChecks
    {
        public static bool GodEnabled = false, FlyEnabled = false, SpectatorEnabled = false, WaterEnabled = false;
        public static int Flying_Admin_Level = 0, Godmode_Admin_Level, Spectator_Admin_Level, Flying_Flags = 4;
        public static Dictionary<int, int> Flag = new Dictionary<int, int>();
        private static Dictionary<int, float> OldY = new Dictionary<int, float>();

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
                                    int _userPermissionLevel = GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo);
                                    if (SpectatorEnabled && _userPermissionLevel > Spectator_Admin_Level)
                                    {
                                        if (_player.IsSpectator)
                                        {
                                            Phrases.Dict.TryGetValue(962, out string _phrase962);
                                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase962), null);
                                            string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                            string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                            using (StreamWriter sw = new StreamWriter(_filepath, true))
                                            {
                                                sw.WriteLine(string.Format("Detected \"{0}\", Steam id {1}, using spectator mode @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, using spectator mode @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                                            Phrases.Dict.TryGetValue(961, out string _phrase961);
                                            _phrase961 = _phrase961.Replace("{PlayerName}", _cInfo.playerName);
                                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase961 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            continue;
                                        }
                                    }
                                    if (GodEnabled && _userPermissionLevel > Godmode_Admin_Level)
                                    {
                                        if (_player.Buffs.HasBuff("god"))
                                        {
                                            Phrases.Dict.TryGetValue(972, out string _phrase972);
                                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase972), null);
                                            string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                            string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                            using (StreamWriter sw = new StreamWriter(_filepath, true))
                                            {
                                                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, using godmode @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            Log.Warning("[SERVERTOOLS] Detected \"{0}\", Steam id {1}, using godmode @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                                            Phrases.Dict.TryGetValue(971, out string _phrase971);
                                            _phrase971 = _phrase971.Replace("{PlayerName}", _cInfo.playerName);
                                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase971 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            continue;
                                        }
                                    }
                                    if (FlyEnabled && _userPermissionLevel > Flying_Admin_Level)
                                    {
                                        if (!Teleportation.Teleporting.Contains(_cInfo.entityId) && _player.IsSpawned() && _player.IsAlive() &&!_player.IsStuck && !_player.isSwimming && _player.AttachedToEntity == null)
                                        {
                                            if (AirCheck(_player.position.x, _player.position.y, _player.position.z) || GroundCheck(_player.position.x, _player.position.y, _player.position.z))
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
                                                if (Flag.ContainsKey(_cInfo.entityId))
                                                {
                                                    Flag.TryGetValue(_cInfo.entityId, out int _flags);
                                                    if (_flags + 1 >= Flying_Flags)
                                                    {
                                                        Flag.Remove(_cInfo.entityId);
                                                        Phrases.Dict.TryGetValue(982, out string _phrase982);
                                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase982), null);
                                                        string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                                        string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                        {
                                                            sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, flying @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                            sw.WriteLine();
                                                            sw.Flush();
                                                            sw.Close();
                                                        }
                                                        Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, flying @ {2} {3} {4}. Steam Id has been banned", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                                                        Phrases.Dict.TryGetValue(981, out string _phrase981);
                                                        _phrase981 = _phrase981.Replace("{PlayerName}", _cInfo.playerName);
                                                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase981 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
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
            for (float k = y - 2.5f; k <= (y + 2f); k++)
            {
                for (float i = x - 1.5f; i <= (x + 1.5f); i++)
                {
                    for (float j = z - 1.5f; j <= (z + 1.5f); j++)
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
            for (float k = y - 1f; k <= (y + 2f); k++)
            {
                for (float i = x - 1.5f; i <= (x + 1.5f); i++)
                {
                    for (float j = z - 1.5f; j <= (z + 1.5f); j++)
                    {
                        BlockValue _block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        string _name = _block.Block.GetBlockName().ToLower();
                        if (_block.type == BlockValue.Air.type || _block.Block.isMultiBlock || _block.Block.IsTerrainDecoration || _block.Block.IsDecoration
                            || _name.Contains("ladder") || _name.Contains("trellis")
                            || _name.Contains("arch") || _name.ToLower().Contains("ramp")
                            || _name.Contains("drawbridge"))
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

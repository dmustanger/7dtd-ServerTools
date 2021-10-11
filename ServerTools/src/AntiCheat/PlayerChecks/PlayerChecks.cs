using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class PlayerChecks
    {
        public static bool GodEnabled = false, FlyEnabled = false, SpectatorEnabled = false, WaterEnabled = false;
        public static int Flying_Admin_Level = 0, Godmode_Admin_Level, Spectator_Admin_Level, Flying_Flags = 4;
        public static Dictionary<int, int> Flag = new Dictionary<int, int>();
        public static Dictionary<int, Vector3i> Movement = new Dictionary<int, Vector3i>();

        public static void Exec()
        {
            try
            {
                if (GameManager.Instance.World != null && GameManager.Instance.World.Players.Count > 0 && GameManager.Instance.World.Players.dict.Count > 0)
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
                                            Phrases.Dict.TryGetValue("Spectator2", out string _phrase);
                                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase), null);
                                            string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                            string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                            using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
                                            {
                                                sw.WriteLine(string.Format("Detected \"{0}\", Steam id {1}, using spectator mode @ {2} {3} {4}", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, using spectator mode @ {2} {3} {4}", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                                            Phrases.Dict.TryGetValue("Spectator1", out _phrase);
                                            _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                            continue;
                                        }
                                    }
                                    if (GodEnabled && _userPermissionLevel > Godmode_Admin_Level)
                                    {
                                        if (_player.Buffs.HasBuff("god"))
                                        {
                                            Phrases.Dict.TryGetValue("Godemode2", out string _phrase);
                                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase), null);
                                            string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                            string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                            using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
                                            {
                                                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, using godmode @ {2} {3} {4}", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            Log.Warning("[SERVERTOOLS] Detected \"{0}\", Steam id {1}, using godmode @ {2} {3} {4}", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                                            Phrases.Dict.TryGetValue("Godemode1", out _phrase);
                                            _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                            continue;
                                        }
                                    }
                                    if (FlyEnabled && _userPermissionLevel > Flying_Admin_Level)
                                    {
                                        if (_player.IsSpawned() && _player.IsAlive())
                                        {
                                            if (_player.isSwimming)
                                            {
                                                BlockValue _block = GameManager.Instance.World.GetBlock(new Vector3i(_player.position));
                                                if (_block.Block.blockMaterial.IsLiquid)
                                                {
                                                    continue;
                                                }
                                            }
                                            if (_player.AttachedToEntity != null)
                                            {
                                                Entity _entity = GameManager.Instance.World.GetEntity(_player.AttachedToEntity.entityId);
                                                if (_entity != null)
                                                {
                                                    float _distance = _player.GetDistance(_player.AttachedToEntity);
                                                    if (_distance <= 5)
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                            if (Movement.ContainsKey(_cInfo.entityId))
                                            {
                                                Movement.TryGetValue(_cInfo.entityId, out Vector3i position);
                                                Movement[_cInfo.entityId] = new Vector3i(_player.position);
                                                if (position.y - _player.position.y >= 4)
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
                                                Movement.Add(_cInfo.entityId, new Vector3i(_player.position));
                                            }
                                            if (AirCheck(_player.position.x, _player.position.y, _player.position.z) || GroundCheck(_player.position.x, _player.position.y, _player.position.z))
                                            {
                                                List<EntityPlayer> _playerList = PersistentOperations.PlayerList();
                                                for (int j = 0; j < _playerList.Count; j++)
                                                {
                                                    if (_playerList[j].entityId != _player.entityId)
                                                    {
                                                        float _distance = _player.GetDistance(_playerList[j]);
                                                        if (_distance <= 2)
                                                        {
                                                            continue;
                                                        }
                                                    }
                                                }
                                                if (Flag.ContainsKey(_cInfo.entityId))
                                                {
                                                    Flag.TryGetValue(_cInfo.entityId, out int _flags);
                                                    _flags++;
                                                    if (_flags >= Flying_Flags)
                                                    {
                                                        Flag.Remove(_cInfo.entityId);
                                                        Phrases.Dict.TryGetValue("Flying2", out string _phrase);
                                                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase), null);
                                                        string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                                        string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                                        using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
                                                        {
                                                            sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, flying @ {2} {3} {4}", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                            sw.WriteLine();
                                                            sw.Flush();
                                                            sw.Close();
                                                        }
                                                        Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, flying @ {2} {3} {4}. Steam Id has been banned", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                                                        Phrases.Dict.TryGetValue("Flying1", out _phrase);
                                                        _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        Flag[_cInfo.entityId] = _flags;
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
                                            if (Movement.ContainsKey(_cInfo.entityId))
                                            {
                                                Movement.TryGetValue(_cInfo.entityId, out Vector3i position);
                                                if (position != new Vector3i(_player.position))
                                                {
                                                    if (Flag.ContainsKey(_cInfo.entityId))
                                                    {
                                                        Flag.TryGetValue(_cInfo.entityId, out int _flags);
                                                        _flags++;
                                                        if (_flags >= Flying_Flags)
                                                        {
                                                            Flag.Remove(_cInfo.entityId);
                                                            Phrases.Dict.TryGetValue("Flying2", out string _phrase);
                                                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase), null);
                                                            string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                                            string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                                            using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
                                                            {
                                                                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, flying @ {2} {3} {4}", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                sw.WriteLine();
                                                                sw.Flush();
                                                                sw.Close();
                                                            }
                                                            Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, flying @ {2} {3} {4}. Steam Id has been banned", _cInfo.playerName, _cInfo.playerId, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                                                            Phrases.Dict.TryGetValue("Flying1", out _phrase);
                                                            _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            Flag[_cInfo.entityId] = _flags;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Flag.Add(_cInfo.entityId, 1);
                                                    }
                                                }
                                            }
                                            else if (Flag.ContainsKey(_cInfo.entityId))
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
                for (float i = x - 2f; i <= (x + 2f); i++)
                {
                    for (float j = z - 2f; j <= (z + 2f); j++)
                    {
                        BlockValue _block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        if (_block.type != BlockValue.Air.type)
                        {
                            return false;
                        }
                    }
                }
            }
            for (float k = y - 2f; k <= y; k++)
            {
                for (float i = x - 4f; i <= (x + 4f); i++)
                {
                    for (float j = z - 4f; j <= (z + 4f); j++)
                    {
                        BlockValue _block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        if (_block.Block.GetBlockName().Contains("MetalPillar"))
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
            for (float k = y - 2f; k <= (y + 2f); k++)
            {
                for (float i = x - 2f; i <= (x + 2f); i++)
                {
                    for (float j = z - 2f; j <= (z + 2f); j++)
                    {
                        BlockValue _block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        if (_block.type == BlockValue.Air.type || !Block.list[_block.type].shape.IsTerrain() ||
                            _block.Block is BlockDoor || _block.Block is BlockDoorSecure || _block.Block is BlockDrawBridge)
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServerTools
{
    class Flying
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Flags = 4;
        private static Dictionary<int, int> Flag = new Dictionary<int, int>();
        private static Dictionary<int, float> OldY = new Dictionary<int, float>();
        private static Dictionary<int, float[]> OldXZ = new Dictionary<int, float[]>();

        public static void Exec()
        {
            try
            {
                if ((int)GameManager.Instance.fps.Counter > 4)
                {
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                    for (int i = 0; i < _cInfoList.Count; i++)
                    {
                        ClientInfo _cInfo = _cInfoList[i];
                        if (_cInfo != null)
                        {
                            AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                            if (Admin.PermissionLevel > Admin_Level)
                            {
                                Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                if (_player != null && _player.IsSpawned())
                                {
                                    float _x = _player.position.x;
                                    float _y = _player.position.y;
                                    float _z = _player.position.z;
                                    if (_player.AttachedToEntity == null && (AirCheck(_x, _y, _z) || GroundCheck(_x, _y, _z)))
                                    {
                                        if (OldY.ContainsKey(_cInfo.entityId))
                                        {
                                            float last_y_pos;
                                            OldY.TryGetValue(_cInfo.entityId, out last_y_pos);
                                            float y_change = (last_y_pos - _y);
                                            if (y_change >= 4)
                                            {
                                                OldXZ.Remove(_cInfo.entityId);
                                                Flag.Remove(_cInfo.entityId);
                                            }
                                            OldY[_cInfo.entityId] = _y;
                                        }
                                        else
                                        {
                                            OldY.Add(_cInfo.entityId, _y);
                                        }
                                        float[] _xz = { _x, _z };
                                        if (OldXZ.ContainsKey(_cInfo.entityId))
                                        {
                                            float[] _xzPos;
                                            OldXZ.TryGetValue(_cInfo.entityId, out _xzPos);
                                            if (_xzPos != _xz)
                                            {
                                                int _flags;
                                                Flag.TryGetValue(_cInfo.entityId, out _flags);
                                                if (_flags + 1 >= Flags)
                                                {
                                                    OldXZ.Remove(_cInfo.entityId);
                                                    Flag.Remove(_cInfo.entityId);
                                                    ChatHook.ChatMessage(null, "[FF0000]" + "Cheater! Player " + _cInfo.playerName + " detected flying!" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                                    Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, flying @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, _x, _y, _z);
                                                    string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                                    string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, flying @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, _x, _y, _z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                    Penalty(_cInfo);
                                                }
                                                else
                                                {
                                                    OldXZ[_cInfo.entityId] = _xz;
                                                    Flag[_cInfo.entityId] = _flags + 1;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            OldXZ.Add(_cInfo.entityId, _xz);
                                            Flag.Add(_cInfo.entityId, 1);
                                        }
                                    }
                                    else
                                    {
                                        if (OldXZ.ContainsKey(_cInfo.entityId))
                                        {
                                            OldXZ.Remove(_cInfo.entityId);
                                            Flag.Remove(_cInfo.entityId);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in Flying.Exec: {0}.", e.Message));
            }
        }

        private static bool AirCheck(float x, float y, float z)
        {
            for (float k = y - 1.8f; k <= (y + 1.5f); k++)
            {
                for (float i = x - 0.5f; i <= (x + 0.5f); i++)
                {
                    for (float j = z - 0.5f; j <= (z + 0.5f); j++)
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
            for (float k = y - 0.25f; k <= (y + 1f); k++)
            {
                for (float i = x - 0.5f; i <= (x + 0.5f); i++)
                {
                    for (float j = z - 0.5f; j <= (z + 0.5f); j++)
                    {
                        BlockValue block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        MaterialBlock _material = Block.list[block.type].blockMaterial;
                        if (block.type == BlockValue.Air.type || _material.IsLiquid || _material.IsPlant || block.Block.IsTerrainDecoration || block.Block.isMultiBlock)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static void Penalty(ClientInfo _cInfo)
        {
            string _message = "[FF0000]{PlayerName} has been banned for flying.";
            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for flying\"", _cInfo.playerId), (ClientInfo)null);
        }
    }
}

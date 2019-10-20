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
        public static Dictionary<int, int> Flag = new Dictionary<int, int>();
        public static Dictionary<int, int[]> LastPositionXZ = new Dictionary<int, int[]>();

        public static void Exec()
        {
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
                        EntityAlive _player = GameManager.Instance.World.GetEntity(_cInfo.entityId) as EntityAlive;
                        if (_player.IsFlyMode.Value && _player.AttachedToEntity == null)
                        {
                            int _x = (int)_player.position.x;
                            int _z = (int)_player.position.z;
                            int[] _xz = { _x, _z };
                            if (LastPositionXZ.ContainsKey(_cInfo.entityId))
                            {
                                int[] _xzPos;
                                LastPositionXZ.TryGetValue(_cInfo.entityId, out _xzPos);
                                if (_xzPos != _xz)
                                {
                                    int _flags;
                                    Flag.TryGetValue(_cInfo.entityId, out _flags);
                                    if (_flags + 1 >= Flags)
                                    {
                                        LastPositionXZ.Remove(_cInfo.entityId);
                                        Flag.Remove(_cInfo.entityId);
                                        ChatHook.ChatMessage(null, "[FF0000]" + "Cheater! Player " + _cInfo.playerName + " detected flying!" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        int _y = (int)_player.position.y;
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
                                        //Penalty(_cInfo);
                                    }
                                    else
                                    {
                                        LastPositionXZ[_cInfo.entityId] = _xz;
                                        Flag[_cInfo.entityId] = _flags + 1;
                                        ChatHook.ChatMessage(null, "[FF0000]" + "[ST] Detected off ground and added one flag" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                }
                            }
                            else
                            {
                                LastPositionXZ.Add(_cInfo.entityId, _xz);
                                Flag.Add(_cInfo.entityId, 1);
                                ChatHook.ChatMessage(null, "[FF0000]" + "[ST] Detected off ground and added the first flag" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                        }
                        else
                        {
                            if (LastPositionXZ.ContainsKey(_cInfo.entityId))
                            {
                                LastPositionXZ.Remove(_cInfo.entityId);
                                Flag.Remove(_cInfo.entityId);
                                ChatHook.ChatMessage(null, "[FF0000]" + "[ST] Detected on the ground and removed all flags" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                ChatHook.ChatMessage(null, "[FF0000]" + "[ST] Detected on the ground. No flags to remove" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                        }
                    }
                }
            }
        }

        public static void Penalty(ClientInfo _cInfo)
        {
            string _message = "[FF0000]{PlayerName} has been banned for flying.";
            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for flying\"", _cInfo.playerId), (ClientInfo)null);
        }
    }
}

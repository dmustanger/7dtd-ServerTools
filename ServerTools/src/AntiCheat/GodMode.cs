using System;
using System.IO;

namespace ServerTools.AntiCheat
{
    class GodMode
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0;

        public static void GodCheck(ClientInfo _cInfo)
        {
            AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
            if (Admin.PermissionLevel > Admin_Level)
            {
                EntityAlive _player = PersistentOperations.GetPlayerAlive(_cInfo.playerId);
                if (_player != null && _player.Buffs.HasBuff("god"))
                {
                    ChatHook.ChatMessage(null, "[FF0000]" + "Cheater! Player " + _cInfo.playerName + " detected using god mode!" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    EntityPlayer _entityPlayer = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                    if (_entityPlayer != null)
                    {
                        int x = (int)_entityPlayer.position.x;
                        int y = (int)_entityPlayer.position.y;
                        int z = (int)_entityPlayer.position.z;
                        Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, using god mode @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, x, y, z);
                        string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                        string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);
                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                        {
                            sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, using god mode @ {2} {3} {4}.", _cInfo.playerName, _cInfo.playerId, x, y, z));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        string _message = "[FF0000]{PlayerName} has been banned for god mode.";
                        _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for god mode\"", _cInfo.playerId), (ClientInfo)null);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools.AntiCheat
{
    class PlayerStats
    {
        public static bool IsEnabled = false, Kick_Enabled = false, Ban_Enabled = false;
        public static int Admin_Level = 0, Days_Before_Log_Delete = 5, Max_Speed = 28, Health = 255, Stamina = 255;
        public static double Jump_Strength = 1.5, Height = 1.8;
        private static string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);

        public static void Exec()
        {
            try
            {
                if (GameManager.Instance.World != null && GameManager.Instance.World.Players.Count > 0 && GameManager.Instance.World.Players.dict.Count > 0)
                {
                    List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
                    if (ClientInfoList != null && ClientInfoList.Count > 0)
                    {
                        for (int i = 0; i < ClientInfoList.Count; i++)
                        {
                            ClientInfo _cInfo = ClientInfoList[i];
                            if (_cInfo != null && _cInfo.playerId != null)
                            {
                                if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
                                {
                                    if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                                    {
                                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        if (_player != null && _player.IsSpawned())
                                        {
                                            var p_speedForward = _player.speedForward;
                                            var p_Health = _player.Stats.Health.Value;
                                            var p_Stamina = _player.Stats.Stamina.Value;
                                            var p_jumpStrength = _player.jumpStrength;
                                            var p_height = _player.height;
                                            if (p_Health > 250)
                                            {
                                                using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0} \"{1}\" Steam id {2} was detected with an illegal health value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_Health));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Warning("Detected player \"{0}\" Steam id {1} with health @ {2}. 250 is maximum", _cInfo.playerName, _cInfo.playerId, p_Health);
                                                if (Kick_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue(991, out string _phrase991);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase991), null);
                                                    Phrases.Dict.TryGetValue(992, out string _phrase992);
                                                    _phrase992 = _phrase992.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase992 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                                if (Ban_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue(993, out string _phrase993);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase993), null);
                                                    Phrases.Dict.TryGetValue(994, out string _phrase994);
                                                    _phrase994 = _phrase994.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase994 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                            }
                                            if (p_Stamina > 250)
                                            {
                                                using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0} \"{1}\" Steam id {2} was detected with an illegal stamina value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_Stamina));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Warning("Detected player \"{0}\" Steam id {1} with stamina @ {2}. 250 is maximum", _cInfo.playerName, _cInfo.playerId, p_Stamina);
                                                if (Kick_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue(995, out string _phrase995);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase995), null);
                                                    Phrases.Dict.TryGetValue(996, out string _phrase996);
                                                    _phrase996 = _phrase996.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase996 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                                if (Ban_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue(997, out string _phrase997);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", _cInfo.playerId, _phrase997), null);
                                                    Phrases.Dict.TryGetValue(998, out string _phrase998);
                                                    _phrase998 = _phrase998.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase998 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                            }
                                            if (p_jumpStrength >= 1.5)
                                            {
                                                using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0} \"{1}\" Steam id {2} was detected with an illegal jump strength value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_jumpStrength));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Warning("Detected player \"{0}\" Steam id {1} at jump strength {2}.", _cInfo.playerName, _cInfo.playerId, p_jumpStrength);
                                                if (Kick_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue(999, out string _phrase999);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase999), null);
                                                    Phrases.Dict.TryGetValue(1000, out string _phrase1000);
                                                    _phrase1000 = _phrase1000.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1000 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                                if (Ban_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue(1001, out string _phrase1001);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", _cInfo.playerId, _phrase1001), null);
                                                    Phrases.Dict.TryGetValue(1002, out string _phrase1002);
                                                    _phrase1002 = _phrase1002.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1002 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                            }
                                            if (p_height > 1.8 || p_height < 1.7)
                                            {
                                                using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0} \"{1}\" Steam id {2} was detected with an illegal player height value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_height));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Warning("Detected player \"{0}\" Steam id {1} with player height @ {2}", _cInfo.playerName, _cInfo.playerId, p_height);
                                                if (Kick_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue(1003, out string _phrase1003);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase1003), null);
                                                    Phrases.Dict.TryGetValue(1004, out string _phrase1004);
                                                    _phrase1004 = _phrase1004.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1004 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                                if (Ban_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue(1005, out string _phrase1005);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", _cInfo.playerId, _phrase1005), null);
                                                    Phrases.Dict.TryGetValue(1006, out string _phrase1006);
                                                    _phrase1006 = _phrase1006.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1006 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                            }
                                            if (p_speedForward > Max_Speed)
                                            {
                                                using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0} \"{1}\" Steam id {2} was detected with an illegal run speed value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_speedForward));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Warning("Detected player \"{0}\" Steam id {1} was detected with an illegal run speed", _cInfo.playerName, _cInfo.playerId);
                                                if (Kick_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue(1007, out string _phrase1007);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase1007), null);
                                                    Phrases.Dict.TryGetValue(1008, out string _phrase1008);
                                                    _phrase1008 = _phrase1008.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1008 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                                if (Ban_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue(1009, out string _phrase1009);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", _cInfo.playerId, _phrase1009), null);
                                                    Phrases.Dict.TryGetValue(1010, out string _phrase1010);
                                                    _phrase1010 = _phrase1010.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1010 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerStats.Exec: {0}", e.Message));
            }
        }
    }
}

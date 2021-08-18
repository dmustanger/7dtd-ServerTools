using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class PlayerStats
    {
        public static bool IsEnabled = false, Kick_Enabled = false, Ban_Enabled = false;
        public static int Admin_Level = 0, Days_Before_Log_Delete = 5, Max_Speed = 28, Health = 255, Stamina = 255;
        public static double Jump_Strength = 1.5, Height = 1.8;

        private static readonly string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

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
                                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0} \"{1}\" Steam id {2} was detected with an illegal health value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_Health));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Warning("Detected player \"{0}\" Steam id {1} with health @ {2}. 250 is maximum", _cInfo.playerName, _cInfo.playerId, p_Health);
                                                if (Kick_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue("PlayerStats1", out string _phrase);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase), null);
                                                    Phrases.Dict.TryGetValue("PlayerStats2", out _phrase);
                                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                                if (Ban_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue("PlayerStats3", out string _phrase);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase), null);
                                                    Phrases.Dict.TryGetValue("PlayerStats4", out _phrase);
                                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                            }
                                            if (p_Stamina > 250)
                                            {
                                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0} \"{1}\" Steam id {2} was detected with an illegal stamina value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_Stamina));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Warning("Detected player \"{0}\" Steam id {1} with stamina @ {2}. 250 is maximum", _cInfo.playerName, _cInfo.playerId, p_Stamina);
                                                if (Kick_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue("PlayerStats5", out string _phrase);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase), null);
                                                    Phrases.Dict.TryGetValue("PlayerStats6", out _phrase);
                                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                                if (Ban_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue("PlayerStats7", out string _phrase);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", _cInfo.playerId, _phrase), null);
                                                    Phrases.Dict.TryGetValue("PlayerStats8", out _phrase);
                                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                            }
                                            if (p_jumpStrength >= 1.5)
                                            {
                                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0} \"{1}\" Steam id {2} was detected with an illegal jump strength value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_jumpStrength));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Warning("Detected player \"{0}\" Steam id {1} at jump strength {2}.", _cInfo.playerName, _cInfo.playerId, p_jumpStrength);
                                                if (Kick_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue("PlayerStats9", out string _phrase);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase), null);
                                                    Phrases.Dict.TryGetValue("PlayerStats10", out _phrase);
                                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                                if (Ban_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue("PlayerStats11", out string _phrase);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", _cInfo.playerId, _phrase), null);
                                                    Phrases.Dict.TryGetValue("PlayerStats12", out _phrase);
                                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                            }
                                            if (p_height > 1.8 || p_height < 1.7)
                                            {
                                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0} \"{1}\" Steam id {2} was detected with an illegal player height value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_height));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Warning("Detected player \"{0}\" Steam id {1} with player height @ {2}", _cInfo.playerName, _cInfo.playerId, p_height);
                                                if (Kick_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue("PlayerStats13", out string _phrase);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase), null);
                                                    Phrases.Dict.TryGetValue("PlayerStats14", out _phrase);
                                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                                if (Ban_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue("PlayerStats15", out string _phrase);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", _cInfo.playerId, _phrase), null);
                                                    Phrases.Dict.TryGetValue("PlayerStats16", out _phrase);
                                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                            }
                                            if (p_speedForward > Max_Speed)
                                            {
                                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0} \"{1}\" Steam id {2} was detected with an illegal run speed value: {3}", DateTime.Now, _cInfo.playerName, _cInfo.playerId, p_speedForward));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Warning("Detected player \"{0}\" Steam id {1} was detected with an illegal run speed", _cInfo.playerName, _cInfo.playerId);
                                                if (Kick_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue("PlayerStats17", out string _phrase);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase), null);
                                                    Phrases.Dict.TryGetValue("PlayerStats18", out _phrase);
                                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                                }
                                                if (Ban_Enabled)
                                                {
                                                    Phrases.Dict.TryGetValue("PlayerStats19", out string _phrase);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", _cInfo.playerId, _phrase), null);
                                                    Phrases.Dict.TryGetValue("PlayerStats20", out _phrase);
                                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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

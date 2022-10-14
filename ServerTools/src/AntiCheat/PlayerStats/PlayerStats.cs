using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class PlayerStats
    {
        public static bool IsEnabled = false, Kick_Enabled = false, Ban_Enabled = false;
        public static int Admin_Level = 0, Days_Before_Log_Delete = 5, Health = 255, Stamina = 255;
        public static double Jump_Strength = 1.5, Height = 1.8;

        private static readonly string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static void Exec()
        {
            try
            {
                List<ClientInfo> clientList = GeneralFunction.ClientList();
                if (clientList != null)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo = clientList[i];
                        if (cInfo != null && cInfo.PlatformId != null && cInfo.CrossplatformId != null)
                        {
                            if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                            {
                                EntityPlayer player = GeneralFunction.GetEntityPlayer(cInfo.entityId);
                                if (player != null && player.IsSpawned())
                                {
                                    var p_Health = player.Stats.Health.Value;
                                    var p_Stamina = player.Stats.Stamina.Value;
                                    var p_jumpStrength = player.jumpStrength;
                                    var p_height = player.height;
                                    if (p_Health > Health)
                                    {
                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: Detected id '{1}' '{2}' named '{3}' with an illegal health value of '{4}'", DateTime.Now, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, p_Health));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        Log.Warning(string.Format("Detected id '{0}' '{1}' named '{2}' with an illegal health value of '{3}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, p_Health));
                                        if (Kick_Enabled)
                                        {
                                            Phrases.Dict.TryGetValue("PlayerStats1", out string phrase);
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                                            Phrases.Dict.TryGetValue("PlayerStats2", out phrase);
                                            phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                        }
                                        if (Ban_Enabled)
                                        {
                                            Phrases.Dict.TryGetValue("PlayerStats3", out string phrase);
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                                            Phrases.Dict.TryGetValue("PlayerStats4", out phrase);
                                            phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                        }
                                    }
                                    if (p_Stamina > Stamina)
                                    {
                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: Detected id '{1}' '{2}' named '{3}' with an illegal stamina value of '{4}'", DateTime.Now, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, p_Stamina));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        Log.Warning(string.Format("Detected id '{0}' '{1}' named '{2}' with an illegal stamina value of '{3}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, p_Stamina));
                                        if (Kick_Enabled)
                                        {
                                            Phrases.Dict.TryGetValue("PlayerStats5", out string phrase);
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                                            Phrases.Dict.TryGetValue("PlayerStats6", out phrase);
                                            phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                        }
                                        if (Ban_Enabled)
                                        {
                                            Phrases.Dict.TryGetValue("PlayerStats7", out string phrase);
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                                            Phrases.Dict.TryGetValue("PlayerStats8", out phrase);
                                            phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                        }
                                    }
                                    if (p_jumpStrength >= Jump_Strength)
                                    {
                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: Detected id '{1}' '{2}' named '{3}' with an illegal jump strength value of '{4}'", DateTime.Now, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, p_jumpStrength));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        Log.Warning(string.Format("Detected id '{0}' '{1}' named '{2}' with an illegal jump strength value of '{3}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, p_jumpStrength));
                                        if (Kick_Enabled)
                                        {
                                            Phrases.Dict.TryGetValue("PlayerStats9", out string phrase);
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                                            Phrases.Dict.TryGetValue("PlayerStats10", out phrase);
                                            phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                        }
                                        if (Ban_Enabled)
                                        {
                                            Phrases.Dict.TryGetValue("PlayerStats11", out string phrase);
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                                            Phrases.Dict.TryGetValue("PlayerStats12", out phrase);
                                            phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                        }
                                    }
                                    if (p_height > 1.8 || p_height < 1.7)
                                    {
                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: Detected id '{1}' '{2}' named '{3}' with an illegal player height value of '{4}'", DateTime.Now, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, p_height));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        Log.Warning(string.Format("Detected id '{0}' '{1}' named '{2}' with an illegal player height value of '{3}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, p_height));
                                        if (Kick_Enabled)
                                        {
                                            Phrases.Dict.TryGetValue("PlayerStats13", out string phrase);
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                                            Phrases.Dict.TryGetValue("PlayerStats14", out phrase);
                                            phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                        }
                                        if (Ban_Enabled)
                                        {
                                            Phrases.Dict.TryGetValue("PlayerStats15", out string phrase);
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban {0} \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                                            Phrases.Dict.TryGetValue("PlayerStats16", out phrase);
                                            phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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

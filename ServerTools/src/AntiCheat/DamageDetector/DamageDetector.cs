using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class DamageDetector
    {
        public static bool IsEnabled = false, LogEnabled = false, WriteLock = false;
        public static int Admin_Level = 0, Entity_Limit = 1000, Player_Limit = 2000, Block_Limit = 2000, Claimed_Block_Limit = 200, ClaimProtection = 16;

        public static ConcurrentQueue<string> DamageLog = new ConcurrentQueue<string>();
        public static Dictionary<Vector3i,int> DamagedBlockId = new Dictionary<Vector3i,int>();
        public static Dictionary<Vector3i, DateTime> BrokenBlockTime = new Dictionary<Vector3i, DateTime>();

        public static readonly string DetectionFile = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static readonly string DetectionFilepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, DetectionFile);

        private static readonly string DamageFile = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string DamageFilepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, DamageFile);

        public static bool IsValidPlayerDamage(Entity _player, ClientInfo _cInfo, int _strength, ItemValue _itemValue)
        {
            try
            {
                if (_strength >= Player_Limit && (GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.PlatformId) > Admin_Level && 
                    GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.CrossplatformId) > Admin_Level))
                {
                    Phrases.Dict.TryGetValue("DamageDetector2", out string phrase);
                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                    if (LogEnabled)
                    {
                        using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("Detected Id '{0}' '{1}' named '{2}' @ '{3}' using item '{4}' exceeding the player damage limit. Target location '{5}'. Damage total '{6}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _cInfo.latestPlayerData.ecd.pos, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _player.position, _strength));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    Phrases.Dict.TryGetValue("DamageDetector1", out string phrase1);
                    phrase1 = phrase1.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DamageDetector.IsValidPlayerDamage: {0}", e.Message));
            }
            return true;
        }

        public static bool IsValidEntityDamage(Entity _entity, ClientInfo _cInfo, int _strength, ItemValue _itemValue)
        {
            try
            {
                if (_itemValue != null && _strength >= Entity_Limit && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.PlatformId) > Admin_Level &&
                    GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.CrossplatformId) > Admin_Level)
                {
                    Phrases.Dict.TryGetValue("DamageDetector2", out string phrase);
                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                    if (LogEnabled)
                    {
                        using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("Detected Id '{0}' '{1}' named '{2}' @ '{3}' using item '{4}' exceeding the entity damage limit. Damage total '{5}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _entity.serverPos.ToVector3() / 32f, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _strength));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    Phrases.Dict.TryGetValue("DamageDetector1", out string phrase1);
                    phrase1 = phrase1.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in DamageDetector.IsValidEntityDamage: {0}", e.Message);
            }
            return true;
        }

        public static void WriteQueue()
        {
            ThreadManager.AddSingleTask(delegate (ThreadManager.TaskInfo _taskInfo)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(DamageFilepath, true, Encoding.UTF8))
                    {
                        while (DamageLog.Count > 0)
                        {
                            if (DamageLog.TryDequeue(out string entry))
                            {
                                sw.WriteLine(entry);
                            }
                        }
                        sw.WriteLine();
                        sw.Dispose();
                    }
                }
                catch (Exception e)
                {
                    Log.Out("[SERVERTOOLS] Error in DamageDetector.WriteQueue: {0}", e.Message);
                }
            });
        }

        public static void GetClaimProtectionLevel()
        {
            ClaimProtection = GamePrefs.GetInt(EnumGamePrefs.LandClaimOnlineDurabilityModifier);
        }
    }
}

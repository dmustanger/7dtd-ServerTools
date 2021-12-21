using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class DamageDetector
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Entity_Damage_Limit = 500, Player_Damage_Limit = 3000, Block_Damage_Limit = 4000;

        private static readonly string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static bool IsValidPvP(EntityPlayer _player1, ClientInfo _cInfo2, int _strength, ItemValue _itemValue)
        {
            try
            {
                if (_itemValue != null && _strength >= Player_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo2) > Admin_Level)
                {
                    Phrases.Dict.TryGetValue("DamageDetector2", out string phrase);
                    phrase = phrase.Replace("{Value}", _strength.ToString());
                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo2.CrossplatformId.CombinedString, phrase), null);
                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(string.Format("Detected Id '{0}' '{1}' named '{2}' using item '{3}' that exceeded the player damage limit @ '{4}'. Damage total '{5}'", _cInfo2.PlatformId.CombinedString, _cInfo2.CrossplatformId.CombinedString, _cInfo2.playerName, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _player1.position, _strength));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                    Phrases.Dict.TryGetValue("DamageDetector1", out string phrase1);
                    phrase1 = phrase1.Replace("{PlayerName}", _cInfo2.playerName);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DamageDetector.IsValidPvP: {0}", e.Message));
            }
            return true;
        }

        public static bool IsValidEntityDamage(EntityAlive _entity, ClientInfo _cInfo, int _strength, ItemValue _itemValue)
        {
            try
            {
                if (_itemValue != null && _strength >= Entity_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
                {
                    Phrases.Dict.TryGetValue("DamageDetector2", out string phrase);
                    phrase = phrase.Replace("{Value}", _strength.ToString());
                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(string.Format("Detected Id '{0}' '{1}' named '{2}' using item '{3}' that exceeded the entity damage limit @ '{4}'. Damage total '{5}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _entity.position, _strength));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                    Phrases.Dict.TryGetValue("DamageDetector1", out string phrase1);
                    phrase1 = phrase1.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DamageDetector.IsValidEntityDamage: {0}", e.Message));
            }
            return true;
        }
    }
}

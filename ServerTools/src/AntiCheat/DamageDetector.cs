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
                if (_itemValue != null)
                {
                    if (_strength >= Player_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo2) > Admin_Level)
                    {
                        Phrases.Dict.TryGetValue(952, out string _phrase952);
                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1} {2}\"", _cInfo2.playerId, _phrase952, _strength), null);
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("Detected \"{0}\" Steam Id {1} using {2} that exceeded the damage limit @ {3}. Damage recorded: {4}", _cInfo2.playerName, _cInfo2.playerId, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _player1.position, _strength));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Phrases.Dict.TryGetValue(951, out string _phrase951);
                        _phrase951 = _phrase951.Replace("{PlayerName}", _cInfo2.playerName);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase951 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return false;
                    }
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
                if (_strength >= Entity_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
                {
                    Phrases.Dict.TryGetValue(952, out string _phrase952);
                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1} {2}\"", _cInfo.playerId, _phrase952, _strength), null);
                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(string.Format("Detected \"{0}\" Steam Id {1} using {2} exceeding the entity damage limit @ {3}. Damage recorded: {4}", _cInfo.playerName, _cInfo.playerId, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _entity.position, _strength));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                    Phrases.Dict.TryGetValue(951, out string _phrase951);
                    _phrase951 = _phrase951.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase951 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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

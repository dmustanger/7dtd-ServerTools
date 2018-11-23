

namespace ServerTools
{
    public class Bloodmoon
    {
        public static bool IsEnabled = false, Show_On_Login = false, Show_On_Respawn = false, Auto_Show = false;
        public static int Days_Until_Horde = 7;

        public static void GetBloodmoon(ClientInfo _cInfo, bool _announce)
        {
            int _daysUntilHorde = Days_Until_Horde - GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) % Days_Until_Horde;
            string _phrase301;
            string _phrase306;
            if (!Phrases.Dict.TryGetValue(301, out _phrase301))
            {
                _phrase301 = "Next horde night is in {DaysUntilHorde} days";
            }
            if (!Phrases.Dict.TryGetValue(306, out _phrase306))
            {
                _phrase306 = "Next horde night is today";
            }
            _phrase301 = _phrase301.Replace("{DaysUntilHorde}", _daysUntilHorde.ToString());
            if (_daysUntilHorde == Days_Until_Horde)
            {
                if (_announce)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase306 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase306 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                if (_announce)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase301 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase301 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void StatusCheck()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                int _daysUntilHorde = Days_Until_Horde - GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) % Days_Until_Horde;
                string _phrase301;
                string _phrase306;
                if (!Phrases.Dict.TryGetValue(301, out _phrase301))
                {
                    _phrase301 = "Next horde night is in {DaysUntilHorde} days";
                }
                if (!Phrases.Dict.TryGetValue(306, out _phrase306))
                {
                    _phrase306 = "Next horde night is today";
                }
                _phrase301 = _phrase301.Replace("{DaysUntilHorde}", _daysUntilHorde.ToString());
                if (_daysUntilHorde == Days_Until_Horde)
                {
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase306 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase301 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
        }
    }
}


namespace ServerTools
{
    public class Bloodmoon
    {
        public static bool IsEnabled = false, Show_On_Login = false, Show_On_Respawn = false, Auto_Show = false;
        public static int Days_Until_Horde = 7;
        public static string Command18 = "bloodmoon", Command19 = "bm";

        public static void GetBloodmoon(ClientInfo _cInfo, bool _announce)
        {
            int _daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
            string _phrase301;
            string _phrase305;
            string _phrase306;
            if (!Phrases.Dict.TryGetValue(301, out _phrase301))
            {
                _phrase301 = "Next horde night is in {DaysUntilHorde} days";
            }
            if (!Phrases.Dict.TryGetValue(305, out _phrase305))
            {
                _phrase305 = "The horde is here!";
            }
            if (!Phrases.Dict.TryGetValue(306, out _phrase306))
            {
                _phrase306 = "Next horde night is today";
            }
            _phrase301 = _phrase301.Replace("{DaysUntilHorde}", _daysRemaining.ToString());
            if (_daysRemaining == 0 && !SkyManager.BloodMoon())
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
            else if (SkyManager.BloodMoon())
            {
                if (_announce)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase305 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase305 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                int _daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
                string _phrase301;
                string _phrase305;
                string _phrase306;
                if (!Phrases.Dict.TryGetValue(301, out _phrase301))
                {
                    _phrase301 = "Next horde night is in {DaysUntilHorde} days";
                }
                if (!Phrases.Dict.TryGetValue(305, out _phrase305))
                {
                    _phrase305 = "The horde is here!";
                }
                if (!Phrases.Dict.TryGetValue(306, out _phrase306))
                {
                    _phrase306 = "Next horde night is today";
                }
                _phrase301 = _phrase301.Replace("{DaysUntilHorde}", _daysRemaining.ToString());
                if (_daysRemaining == 0 && !SkyManager.BloodMoon())
                {
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase306 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else if (SkyManager.BloodMoon())
                {
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase305 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase301 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
        }
    }
}
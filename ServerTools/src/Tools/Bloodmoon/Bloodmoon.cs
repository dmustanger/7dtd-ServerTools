

namespace ServerTools
{
    public class Bloodmoon
    {
        public static bool IsEnabled = false, Show_On_Login = false, Show_On_Respawn = false, Auto_Show = false;
        public static string Command18 = "bloodmoon", Command19 = "bm";
        public static int Delay = 60;

        public static void Exec(ClientInfo _cInfo)
        {
            int _daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
            if (_daysRemaining == 0 && !SkyManager.BloodMoon())
            {
                Phrases.Dict.TryGetValue(682, out string _phrase682);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase682 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else if (SkyManager.BloodMoon())
            {
                Phrases.Dict.TryGetValue(683, out string _phrase683);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase683 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue(681, out string _phrase681);
                _phrase681 = _phrase681.Replace("{Value}", _daysRemaining.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase681 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void StatusCheck()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                int _daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
                if (_daysRemaining == 0 && !SkyManager.BloodMoon())
                {
                    Phrases.Dict.TryGetValue(682, out string _phrase682);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase682 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else if (SkyManager.BloodMoon())
                {
                    Phrases.Dict.TryGetValue(683, out string _phrase683);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase683 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(681, out string _phrase681);
                    _phrase681 = _phrase681.Replace("{Value}", _daysRemaining.ToString());
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase681 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
        }
    }
}
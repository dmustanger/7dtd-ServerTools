

namespace ServerTools
{
    public class Bloodmoon
    {
        public static bool IsEnabled = false, Show_On_Login = false, Show_On_Respawn = false, Auto_Show = false;
        public static string Command_bloodmoon = "bloodmoon", Command_bm = "bm";
        public static int Delay = 60;

        public static void Exec(ClientInfo _cInfo)
        {
            int _daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
            if (_daysRemaining == 0 && !SkyManager.BloodMoon())
            {
                Phrases.Dict.TryGetValue("Bloodmoon2", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else if (SkyManager.BloodMoon())
            {
                Phrases.Dict.TryGetValue("Bloodmoon3", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Bloodmoon1", out string _phrase);
                _phrase = _phrase.Replace("{Value}", _daysRemaining.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void StatusCheck()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                int _daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
                if (_daysRemaining == 0 && !SkyManager.BloodMoon())
                {
                    Phrases.Dict.TryGetValue("Bloodmoon2", out string _phrase);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else if (SkyManager.BloodMoon())
                {
                    Phrases.Dict.TryGetValue("Bloodmoon3", out string _phrase);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Bloodmoon1", out string _phrase);
                    _phrase = _phrase.Replace("{Value}", _daysRemaining.ToString());
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
        }
    }
}
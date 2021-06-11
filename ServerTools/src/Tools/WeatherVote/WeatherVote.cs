using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WeatherVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
        public static string Command62 = "weathervote", Command63 = "sun", Command64 = "rain", Command65 = "snow";
        private static string _weather = "";
        public static List<int> Sun = new List<int>();
        public static List<int> Rain = new List<int>();
        public static List<int> Snow = new List<int>();
        public static DateTime LastVote = new DateTime();

        public static void CallForVote(ClientInfo _cInfo)
        {
            if (!VoteOpen)
            {
                DateTime _lastVote = PersistentContainer.Instance.LastWeather;
                if (_lastVote == null)
                {
                    _lastVote = DateTime.Now.AddMinutes(-70);
                }
                TimeSpan varTime = DateTime.Now - _lastVote;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (_timepassed >= 60)
                {
                    int _playerCount = ConnectionManager.Instance.ClientCount();
                    if (_playerCount >= Players_Online)
                    {

                        VoteOpen = true;
                        Phrases.Dict.TryGetValue(871, out string _phrase871);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase871 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        Phrases.Dict.TryGetValue(875, out string _phrase875);
                        _phrase875 = _phrase875.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
                        _phrase875 = _phrase875.Replace("{Command63}", Command63);
                        _phrase875 = _phrase875.Replace("{Command64}", Command64);
                        _phrase875 = _phrase875.Replace("{Command65}", Command65);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase875 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(876, out string _phrase876);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase876 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    int _timeleft = 60 - _timepassed;
                    Phrases.Dict.TryGetValue(877, out string _phrase877);
                    _phrase877 = _phrase877.Replace("{Value}", _timeleft.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase877 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(878, out string _phrase878);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase878 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void ProcessWeatherVote()
        {
            if (Sun.Count + Rain.Count + Snow.Count >= Votes_Needed)
            {
                if (Sun.Count > Rain.Count && Sun.Count > Snow.Count)
                {
                    _weather = "sun";
                    Phrases.Dict.TryGetValue(879, out string _phrase879);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase879 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else if (Rain.Count > Sun.Count && Rain.Count > Snow.Count)
                {
                    _weather = "rain";
                    Random rnd = new Random();
                    int _rndWeather = rnd.Next(1, 4);
                    if (_rndWeather == 1)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather rain 0.2", null);
                        Phrases.Dict.TryGetValue(880, out string _phrase880);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase880 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    if (_rndWeather == 2)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather rain 0.6", null);
                        SdtdConsole.Instance.ExecuteSync("weather wet 1", null);
                        Phrases.Dict.TryGetValue(881, out string _phrase881);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase881 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    if (_rndWeather == 3)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather rain 1", null);
                        SdtdConsole.Instance.ExecuteSync("weather wet 1", null);
                        Phrases.Dict.TryGetValue(882, out string _phrase882);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase882 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else if (Snow.Count > Sun.Count && Snow.Count > Rain.Count)
                {
                    _weather = "snow";
                    Random rnd = new Random();
                    int _rndWeather = rnd.Next(1, 4);
                    if (_rndWeather == 1)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 0.2", null);
                        Phrases.Dict.TryGetValue(883, out string _phrase883);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase883 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    if (_rndWeather == 2)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 0.6", null);
                        SdtdConsole.Instance.ExecuteSync("weather snow 0.6", null);
                        Phrases.Dict.TryGetValue(884, out string _phrase884);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase884 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    if (_rndWeather == 3)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 1", null);
                        SdtdConsole.Instance.ExecuteSync("weather snow 1", null);
                        Phrases.Dict.TryGetValue(885, out string _phrase885);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase885 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else if (Sun.Count == 0 && Rain.Count == 0 && Snow.Count == 0)
                {
                    Phrases.Dict.TryGetValue(872, out string _phrase872);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase872 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    if (_weather != "")
                    {
                        Phrases.Dict.TryGetValue(873, out string _phrase873);
                        _phrase873 = _phrase873.Replace("{Weather}", _weather.ToString());
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase873 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    else if (Sun.Count > 0 && Rain.Count > 0 && Snow.Count > 0)
                    {
                        Phrases.Dict.TryGetValue(874, out string _phrase874);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase874 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(886, out string _phrase886);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase886 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            Sun.Clear(); Rain.Clear(); Snow.Clear();
            _weather = "";
            PersistentContainer.Instance.LastWeather = DateTime.Now;
            PersistentContainer.DataChange = true;
        }
    }
}

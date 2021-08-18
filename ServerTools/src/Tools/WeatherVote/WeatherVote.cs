using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WeatherVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
        public static string Command_weathervote = "weathervote", Command_sun = "sun", Command_rain = "rain", Command_snow = "snow";
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
                        Phrases.Dict.TryGetValue("WeatherVote1", out string _phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        Phrases.Dict.TryGetValue("WeatherVote5", out _phrase);
                        _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                        _phrase = _phrase.Replace("{Command_sun}", Command_sun);
                        _phrase = _phrase.Replace("{Command_rain}", Command_rain);
                        _phrase = _phrase.Replace("{Command_snow}", Command_snow);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("WeatherVote6", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    int _timeleft = 60 - _timepassed;
                    Phrases.Dict.TryGetValue("WeatherVote7", out string _phrase);
                    _phrase = _phrase.Replace("{Value}", _timeleft.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("WeatherVote8", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void ProcessWeatherVote()
        {
            if (Sun.Count + Rain.Count + Snow.Count >= Votes_Needed)
            {
                if (Sun.Count > Rain.Count && Sun.Count > Snow.Count)
                {
                    _weather = "sun";
                    Phrases.Dict.TryGetValue("WeatherVote9", out string _phrase);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else if (Rain.Count > Sun.Count && Rain.Count > Snow.Count)
                {
                    _weather = "rain";
                    Random rnd = new Random();
                    int _rndWeather = rnd.Next(1, 4);
                    if (_rndWeather == 1)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather rain 0.2", null);
                        Phrases.Dict.TryGetValue("WeatherVote10", out string _phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    if (_rndWeather == 2)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather rain 0.6", null);
                        SdtdConsole.Instance.ExecuteSync("weather wet 1", null);
                        Phrases.Dict.TryGetValue("WeatherVote11", out string _phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    if (_rndWeather == 3)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather rain 1", null);
                        SdtdConsole.Instance.ExecuteSync("weather wet 1", null);
                        Phrases.Dict.TryGetValue("WeatherVote12", out string _phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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
                        Phrases.Dict.TryGetValue("WeatherVote13", out string _phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    if (_rndWeather == 2)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 0.6", null);
                        SdtdConsole.Instance.ExecuteSync("weather snow 0.6", null);
                        Phrases.Dict.TryGetValue("WeatherVote14", out string _phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    if (_rndWeather == 3)
                    {
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 1", null);
                        SdtdConsole.Instance.ExecuteSync("weather snow 1", null);
                        Phrases.Dict.TryGetValue("WeatherVote15", out string _phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else if (Sun.Count == 0 && Rain.Count == 0 && Snow.Count == 0)
                {
                    Phrases.Dict.TryGetValue("WeatherVote2", out string _phrase);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    if (_weather != "")
                    {
                        Phrases.Dict.TryGetValue("WeatherVote3", out string _phrase);
                        _phrase = _phrase.Replace("{Weather}", _weather.ToString());
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    else if (Sun.Count > 0 && Rain.Count > 0 && Snow.Count > 0)
                    {
                        Phrases.Dict.TryGetValue("WeatherVote4", out string _phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("WeatherVote16", out string _phrase);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            Sun.Clear(); Rain.Clear(); Snow.Clear();
            _weather = "";
            PersistentContainer.Instance.LastWeather = DateTime.Now;
            PersistentContainer.DataChange = true;
        }
    }
}

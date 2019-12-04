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
                TimeSpan varTime = DateTime.Now - _lastVote;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (_timepassed >= 60)
                {
                    int _playerCount = ConnectionManager.Instance.ClientCount();
                    if (_playerCount >= Players_Online)
                    {
                        string _phrase611;
                        if (!Phrases.Dict.TryGetValue(611, out _phrase611))
                        {
                            _phrase611 = "A vote to change the weather has begun and will close in 60 seconds.";
                        }
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase611 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        string _phrase615;
                        if (!Phrases.Dict.TryGetValue(615, out _phrase615))
                        {
                            _phrase615 = "Type {CommandPrivate}{Command63}, {CommandPrivate}{Command64} or {CommandPrivate}{Command65} to cast your vote.";
                        }
                        _phrase615 = _phrase615.Replace("{CommandPrivate}", ChatHook.Command_Private);
                        _phrase615 = _phrase615.Replace("{Command63}", Command63);
                        _phrase615 = _phrase615.Replace("{Command64}", Command64);
                        _phrase615 = _phrase615.Replace("{Command65}", Command65);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase615 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        VoteOpen = true;
                    }
                    else
                    {
                        string _phrase933;
                        if (!Phrases.Dict.TryGetValue(933, out _phrase933))
                        {
                            _phrase933 = " not enough players are online to start a weather vote.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase933 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    int _timeleft = 60 - _timepassed;
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Wait thirty minutes before starting a new vote to change the weather. " + _timeleft + " minutes remaining." + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                string _phrase824;
                if (!Phrases.Dict.TryGetValue(824, out _phrase824))
                {
                    _phrase824 = " there is a vote already open.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase824 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void ProcessWeatherVote()
        {
            if (Sun.Count + Rain.Count + Snow.Count >= Votes_Needed)
            {
                if (Sun.Count > Rain.Count && Sun.Count > Snow.Count)
                {
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "Clear skies ahead.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    SdtdConsole.Instance.ExecuteSync("weather rain 0", (ClientInfo)null);
                    SdtdConsole.Instance.ExecuteSync("weather rainfall 0", (ClientInfo)null);
                    SdtdConsole.Instance.ExecuteSync("weather wet 0", (ClientInfo)null);
                    SdtdConsole.Instance.ExecuteSync("weather snow 0", (ClientInfo)null);
                    SdtdConsole.Instance.ExecuteSync("weather snowfall 0", (ClientInfo)null);
                    VoteOpen = false;
                    _weather = "sun";
                }
                if (Rain.Count > Sun.Count && Rain.Count > Snow.Count)
                {
                    Random rnd = new Random();
                    int _rndWeather = rnd.Next(1, 4);
                    if (_rndWeather == 1)
                    {
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "Light rain has started.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        SdtdConsole.Instance.ExecuteSync("weather rain 0.2", (ClientInfo)null);
                    }
                    if (_rndWeather == 2)
                    {
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "A rain storm has started.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        SdtdConsole.Instance.ExecuteSync("weather rain 0.6", (ClientInfo)null);
                        SdtdConsole.Instance.ExecuteSync("weather wet 1", (ClientInfo)null);
                    }
                    if (_rndWeather == 3)
                    {
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "A heavy rain storm has started.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        SdtdConsole.Instance.ExecuteSync("weather rain 1", (ClientInfo)null);
                        SdtdConsole.Instance.ExecuteSync("weather wet 1", (ClientInfo)null);
                    }
                    VoteOpen = false;
                    _weather = "rain";
                }
                if (Snow.Count > Sun.Count && Snow.Count > Rain.Count)
                {
                    Random rnd = new Random();
                    int _rndWeather = rnd.Next(1, 4);
                    if (_rndWeather == 1)
                    {
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "Light snow has started.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 0.2", (ClientInfo)null);
                    }
                    if (_rndWeather == 2)
                    {
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "A snow storm has started.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 0.6", (ClientInfo)null);
                        SdtdConsole.Instance.ExecuteSync("weather snow 0.6", (ClientInfo)null);
                    }
                    if (_rndWeather == 3)
                    {
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "A heavy snow storm has started.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 1", (ClientInfo)null);
                        SdtdConsole.Instance.ExecuteSync("weather snow 1", (ClientInfo)null);
                    }
                    VoteOpen = false;
                    _weather = "snow";
                }
                if (Sun.Count == 0 && Rain.Count == 0 && Snow.Count == 0)
                {
                    string _phrase612;
                    if (!Phrases.Dict.TryGetValue(612, out _phrase612))
                    {
                        _phrase612 = "Weather vote complete, but no votes were cast. No changes were made.";
                    }
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase612 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    Sun.Clear(); Rain.Clear(); Snow.Clear();
                    VoteOpen = false;
                    _weather = "";
                }
                else
                {
                    if (_weather != "")
                    {
                        string _phrase613;
                        if (!Phrases.Dict.TryGetValue(613, out _phrase613))
                        {
                            _phrase613 = "Weather vote complete. Most votes went to {Weather}.";
                        }
                        _phrase613 = _phrase613.Replace("{Weather}", _weather.ToString());
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase613 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        Sun.Clear(); Rain.Clear(); Snow.Clear();
                    }
                    else if (Sun.Count > 0 && Rain.Count > 0 && Snow.Count > 0)
                    {
                        string _phrase614;
                        if (!Phrases.Dict.TryGetValue(614, out _phrase614))
                        {
                            _phrase614 = "Weather vote was a tie. No changes were made.";
                        }
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase614 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        Sun.Clear(); Rain.Clear(); Snow.Clear();
                    }
                    VoteOpen = false;
                    _weather = "";
                }
                PersistentContainer.Instance.LastWeather = DateTime.Now;
                PersistentContainer.Instance.Save();
            }
            else
            {
                VoteOpen = false;
                _weather = "";
                string _phrase805;
                if (!Phrases.Dict.TryGetValue(805, out _phrase805))
                {
                    _phrase805 = "Not enough votes were cast in the weather vote. No changes were made.";
                }
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase805 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}

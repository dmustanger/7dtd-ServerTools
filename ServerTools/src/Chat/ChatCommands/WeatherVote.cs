using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WeatherVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
        private static string _weather = "";
        public static List<int> clear = new List<int>();
        public static List<int> rain = new List<int>();
        public static List<int> snow = new List<int>();

        public static void CallForVote1(ClientInfo _cInfo)
        {
            if (!VoteOpen)
            {
                int _playerCount = ConnectionManager.Instance.ClientCount();
                if (_playerCount >= Players_Online)
                {
                    string _phrase611;
                    if (!Phrases.Dict.TryGetValue(611, out _phrase611))
                    {
                        _phrase611 = "A vote to change the weather has begun and will close in 30 seconds.";
                    }
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase611), Config.Server_Response_Name, false, "", false);
                    string _phrase615;
                    if (!Phrases.Dict.TryGetValue(615, out _phrase615))
                    {
                        _phrase615 = "Type /clear, /rain or /snow to cast your vote.";
                    }
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase615), Config.Server_Response_Name, false, "", false);
                    VoteOpen = true;
                }
                else
                {
                    string _phrase933;
                    if (!Phrases.Dict.TryGetValue(933, out _phrase933))
                    {
                        _phrase933 = "{PlayerName} you can only start this vote if at least {Count} players are online.";
                    }
                    _phrase933 = _phrase933.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase933 = _phrase933.Replace("{Count}", Players_Online.ToString());
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase933), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                string _phrase824;
                if (!Phrases.Dict.TryGetValue(824, out _phrase824))
                {
                    _phrase824 = "{PlayerName} there is a vote already open.";
                }
                _phrase824 = _phrase824.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase824), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void CallForVote2()
        {
            if (clear.Count + rain.Count + snow.Count >= Votes_Needed)
            {
                if (clear.Count > rain.Count & clear.Count > snow.Count)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}Clear skies ahead", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                    SdtdConsole.Instance.ExecuteSync("weather rain 0", (ClientInfo)null);
                    SdtdConsole.Instance.ExecuteSync("weather rainfall 0", (ClientInfo)null);
                    SdtdConsole.Instance.ExecuteSync("weather wet 0", (ClientInfo)null);
                    SdtdConsole.Instance.ExecuteSync("weather snow 0", (ClientInfo)null);
                    SdtdConsole.Instance.ExecuteSync("weather snowfall 0", (ClientInfo)null);
                    VoteOpen = false;
                    _weather = "clear";
                }
                if (rain.Count > clear.Count & rain.Count > snow.Count)
                {
                    Random rnd = new Random();
                    int _rndWeather = rnd.Next(1, 3);
                    if (_rndWeather == 1)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}Light rain has started", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                        SdtdConsole.Instance.ExecuteSync("weather rain 0.2", (ClientInfo)null);
                    }
                    if (_rndWeather == 2)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}A rain storm has started", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                        SdtdConsole.Instance.ExecuteSync("weather rain 0.6", (ClientInfo)null);
                        SdtdConsole.Instance.ExecuteSync("weather wet 1", (ClientInfo)null);
                    }
                    if (_rndWeather == 3)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}A heavy rain storm has started", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                        SdtdConsole.Instance.ExecuteSync("weather rain 1", (ClientInfo)null);
                        SdtdConsole.Instance.ExecuteSync("weather wet 1", (ClientInfo)null);
                    }
                    VoteOpen = false;
                    _weather = "rain";
                }
                if (snow.Count > clear.Count & snow.Count > rain.Count)
                {
                    Random rnd = new Random();
                    int _rndWeather = rnd.Next(1, 3);
                    if (_rndWeather == 1)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}Light snow has started", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 0.2", (ClientInfo)null);
                    }
                    if (_rndWeather == 2)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}A snow storm has started", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 0.6", (ClientInfo)null);
                        SdtdConsole.Instance.ExecuteSync("weather snow 0.6", (ClientInfo)null);
                    }
                    if (_rndWeather == 3)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}A heavy snow storm has started", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                        SdtdConsole.Instance.ExecuteSync("weather snowfall 1", (ClientInfo)null);
                        SdtdConsole.Instance.ExecuteSync("weather snow 1", (ClientInfo)null);
                    }
                    VoteOpen = false;
                    _weather = "snow";
                }
                if (clear.Count == 0 & rain.Count == 0 & snow.Count == 0)
                {
                    string _phrase612;
                    if (!Phrases.Dict.TryGetValue(612, out _phrase612))
                    {
                        _phrase612 = "Weather vote complete, but no votes were cast. No changes were made.";
                    }
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase612), Config.Server_Response_Name, false, "", false);
                    clear.Clear(); rain.Clear(); snow.Clear();
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
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase613), Config.Server_Response_Name, false, "", false);
                        clear.Clear(); rain.Clear(); snow.Clear();
                    }
                    else if (_weather != "" & clear.Count > 0 || _weather != "" & rain.Count > 0 || _weather != "" & snow.Count > 0)
                    {
                        string _phrase614;
                        if (!Phrases.Dict.TryGetValue(614, out _phrase614))
                        {
                            _phrase614 = "Weather vote was a tie. No changes were made.";
                        }
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase614), Config.Server_Response_Name, false, "", false);
                        clear.Clear(); rain.Clear(); snow.Clear();
                    }
                    VoteOpen = false;
                    _weather = "";
                }
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
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase805), Config.Server_Response_Name, false, "", false);
            }
        }
    }
}

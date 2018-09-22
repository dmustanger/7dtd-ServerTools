using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NightVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
        public static List<int> Night = new List<int>();

        public static void Vote(ClientInfo _cInfo)
        {
            if (!VoteOpen)
            {
                int _playerCount = ConnectionManager.Instance.ClientCount();
                if (_playerCount >= Players_Online)
                {
                    if (!GameManager.Instance.World.IsDaytime())
                    {
                        if (!SkyManager.BloodMoon())
                        {
                            VoteOpen = true;
                            string _phrase932;
                            if (!Phrases.Dict.TryGetValue(932, out _phrase932))
                            {
                                _phrase932 = "A vote to skip the night has begun. You have 30 seconds to type /yes.";
                            }
                            _phrase932 = _phrase932.Replace("{PlayerName}", _cInfo.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase932), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                        else
                        {
                            string _phrase930;
                            if (!Phrases.Dict.TryGetValue(930, out _phrase930))
                            {
                                _phrase930 = "{PlayerName} you can not start a vote during a bloodmoon.";
                            }
                            _phrase930 = _phrase930.Replace("{PlayerName}", _cInfo.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase930), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        string _phrase931;
                        if (!Phrases.Dict.TryGetValue(931, out _phrase931))
                        {
                            _phrase931 = "{PlayerName} you can not start a vote during the day.";
                        }
                        _phrase931 = _phrase931.Replace("{PlayerName}", _cInfo.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase931), Config.Server_Response_Name, false, "ServerTools", false));
                    }

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
        }

        public static void VoteCount()
        {
            if (Night.Count >= Votes_Needed)
            {
                int _dawn = (int)SkyManager.GetDawnTime();
                ulong _worldTime = GameManager.Instance.World.worldTime;
                int _worldHours = (int)(_worldTime / 1000UL) % 24;
                int _newTime = (24 - _worldHours + _dawn) * 1000;
                ulong _newWorldTime = _worldTime + Convert.ToUInt64(_newTime);
                GameManager.Instance.World.worldTime = _newWorldTime;
                string _phrase934;
                if (!Phrases.Dict.TryGetValue(934, out _phrase934))
                {
                    _phrase934 = "Players voted yes to skip this night. Good morning.";
                }
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase934), Config.Server_Response_Name, false, "", false);
            }
            Night.Clear();
            VoteOpen = false;
        }
    }
}
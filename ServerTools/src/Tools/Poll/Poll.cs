using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class Poll
    {
        public static bool IsEnabled = false;
        public static string Command91 = "poll yes", Command92 = "poll no", Command93 = "poll";
        public static string File = string.Format("PollLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string Filepath = string.Format("{0}/Logs/PollLogs/{1}", API.ConfigPath, File);

        public static void Message(ClientInfo _cInfo)
        {
            if (PersistentContainer.Instance.PollData != null)
            {
                string[] _pollData = PersistentContainer.Instance.PollData;
                DateTime.TryParse(_pollData[0], out DateTime _pollDate);
                int.TryParse(_pollData[1], out int _pollHours);
                TimeSpan varTime = DateTime.Now - _pollDate;
                double fractionalHours = varTime.TotalHours;
                int _timepassed = (int)fractionalHours;
                if (_timepassed <= _pollHours)
                {
                    string _phrase926;
                    if (!Phrases.Dict.TryGetValue(926, out _phrase926))
                    {
                        _phrase926 = "Poll: {Message}";
                    }
                    _phrase926 = _phrase926.Replace("{Message}", _pollData[2]);
                    string _phrase927;
                    if (!Phrases.Dict.TryGetValue(927, out _phrase927))
                    {
                        _phrase927 = "Type {CommandPrivate}{Command91} or {CommandPrivate}{Command92} to vote.";
                    }
                    _phrase927 = _phrase927.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase927 = _phrase927.Replace("{Command91}", Poll.Command91);
                    _phrase927 = _phrase927.Replace("{Command92}", Poll.Command92);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase926 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase927 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void Yes(ClientInfo _cInfo)
        {
            if (!PersistentContainer.Instance.PollVote.ContainsKey(_cInfo.playerId))
            {
                Dictionary<string, bool> _votes = PersistentContainer.Instance.PollVote;
                _votes.Add(_cInfo.playerId, true);
                PersistentContainer.Instance.PollVote = _votes;
                PersistentContainer.Instance.Save();
                string _phrase928;
                if (!Phrases.Dict.TryGetValue(928, out _phrase928))
                {
                    _phrase928 = "You have cast a vote for yes in the poll";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase928 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                using (StreamWriter sw = new StreamWriter(Poll.Filepath, true))
                {
                    sw.WriteLine(string.Format("{0}: Player name {1} with SteamId {2} has voted yes in the poll.", DateTime.Now, _cInfo.playerName, _cInfo.playerId));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
            else
            {
                string _phrase812;
                if (!Phrases.Dict.TryGetValue(812, out _phrase812))
                {
                    _phrase812 = "You have already voted on the poll.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase812 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void No(ClientInfo _cInfo)
        {
            if (!PersistentContainer.Instance.PollVote.ContainsKey(_cInfo.playerId))
            {
                Dictionary<string, bool> _votes = PersistentContainer.Instance.PollVote;
                _votes.Add(_cInfo.playerId, false);
                PersistentContainer.Instance.PollVote = _votes;
                PersistentContainer.Instance.Save();
                string _phrase928;
                if (!Phrases.Dict.TryGetValue(928, out _phrase928))
                {
                    _phrase928 = "You have cast a vote for no in the poll";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase928 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                using (StreamWriter sw = new StreamWriter(Poll.Filepath, true))
                {
                    sw.WriteLine(string.Format("{0}: Player name {1} with SteamId {2} has voted no in the poll.", DateTime.Now, _cInfo.playerName, _cInfo.playerId));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
            else
            {
                string _phrase812;
                if (!Phrases.Dict.TryGetValue(812, out _phrase812))
                {
                    _phrase812 = "You have already voted on the poll.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase812 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CheckTime()
        {
            if (PersistentContainer.Instance.PollData != null)
            {
                string[] _pollData = PersistentContainer.Instance.PollData;
                DateTime.TryParse(_pollData[0], out DateTime _pollDate);
                int.TryParse(_pollData[1], out int _pollHours);
                TimeSpan varTime = DateTime.Now - _pollDate;
                double fractionalHours = varTime.TotalHours;
                int _timepassed = (int)fractionalHours;
                if (_timepassed >= _pollHours)
                {
                    if (PersistentContainer.Instance.PollVote == null)
                    {
                        return;
                    }
                    Dictionary<string, bool> _pollVotes = PersistentContainer.Instance.PollVote;
                    if (_pollVotes.Count == 0)
                    {
                        PersistentContainer.Instance.PollData = null;
                        PersistentContainer.Instance.PollOpen = false;
                        PersistentContainer.Instance.Save();
                        using (StreamWriter sw = new StreamWriter(Poll.Filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: The poll has finished but no votes were cast.", DateTime.Now));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        return;
                    }
                    int _yes = 0, _no = 0;
                    foreach (var _vote in _pollVotes)
                    {
                        if (_vote.Value == true)
                        {
                            _yes++;
                        }
                        else
                        {
                            _no++;
                        }
                    }
                    if (_yes > _no)
                    {
                        if (PersistentContainer.Instance.PollOld != null && PersistentContainer.Instance.PollOld.Count > 0)
                        {
                            Dictionary<string[], string> _oldPoll = PersistentContainer.Instance.PollOld;
                            _oldPoll.Add(_pollData, "yes");
                            PersistentContainer.Instance.PollOld = _oldPoll;
                        }
                        else
                        {
                            Dictionary<string[], string> _oldPoll = new Dictionary<string[], string>();
                            _oldPoll.Add(_pollData, "yes");
                            PersistentContainer.Instance.PollOld = _oldPoll;
                        }
                    }
                    else if (_no > _yes)
                    {
                        if (PersistentContainer.Instance.PollOld != null && PersistentContainer.Instance.PollOld.Count > 0)
                        {
                            Dictionary<string[], string> _oldPoll = PersistentContainer.Instance.PollOld;
                            _oldPoll.Add(_pollData, "no");
                            PersistentContainer.Instance.PollOld = _oldPoll;
                        }
                        else
                        {
                            Dictionary<string[], string> _oldPoll = new Dictionary<string[], string>();
                            _oldPoll.Add(_pollData, "no");
                            PersistentContainer.Instance.PollOld = _oldPoll;
                        }
                    }
                    else
                    {
                        if (PersistentContainer.Instance.PollOld != null && PersistentContainer.Instance.PollOld.Count > 0)
                        {
                            Dictionary<string[], string> _oldPoll = PersistentContainer.Instance.PollOld;
                            _oldPoll.Add(_pollData, "tie");
                            PersistentContainer.Instance.PollOld = _oldPoll;
                        }
                        else
                        {
                            Dictionary<string[], string> _oldPoll = new Dictionary<string[], string>();
                            _oldPoll.Add(_pollData, "tie");
                            PersistentContainer.Instance.PollOld = _oldPoll;
                        }
                    }
                    using (StreamWriter sw = new StreamWriter(Poll.Filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}: The poll has completed and the players voted {1} yes / {2} no.", DateTime.Now, _yes, _no));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
        }
    }
}

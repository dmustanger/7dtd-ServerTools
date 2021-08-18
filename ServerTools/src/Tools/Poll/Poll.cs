using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class Poll
    {
        public static bool IsEnabled = false;
        public static string Command_poll_yes = "poll yes", Command_poll_no = "poll no", Command_poll = "poll";
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
                    Phrases.Dict.TryGetValue("Poll2", out string _phrase);
                    _phrase = _phrase.Replace("{Message}", _pollData[2]);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    Phrases.Dict.TryGetValue("Poll3", out _phrase);
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_poll_yes}", Command_poll_yes);
                    _phrase = _phrase.Replace("{Command_poll_no}", Command_poll_no);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                PersistentContainer.DataChange = true;
                using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Player {1} {2} has voted yes in the poll.", DateTime.Now, _cInfo.playerName, _cInfo.playerId));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("Poll4", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                
            }
            else
            {
                Phrases.Dict.TryGetValue("Poll6", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void No(ClientInfo _cInfo)
        {
            if (!PersistentContainer.Instance.PollVote.ContainsKey(_cInfo.playerId))
            {
                Dictionary<string, bool> _votes = PersistentContainer.Instance.PollVote;
                _votes.Add(_cInfo.playerId, false);
                PersistentContainer.Instance.PollVote = _votes;
                PersistentContainer.DataChange = true;
                using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Player {1} {2} has voted no in the poll.", DateTime.Now, _cInfo.playerName, _cInfo.playerId));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("Poll5", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Poll6", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                        PersistentContainer.DataChange = true;
                        using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
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
                        PersistentContainer.DataChange = true;
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
                        PersistentContainer.DataChange = true;
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
                        PersistentContainer.DataChange = true;
                    }
                    using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
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

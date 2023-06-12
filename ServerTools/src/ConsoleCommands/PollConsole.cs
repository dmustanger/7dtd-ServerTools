using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ServerTools
{
    class PollConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Open, cancel, stop, check, or list a poll for players to vote on.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. st-poll new <hours> <message>\n" +
                "  2. st-poll cancel\n" +
                "  3. st-poll stop\n" +
                "  4. st-poll check\n" +
                "  5. st-poll list\n" +
                "  6. st-poll open <number> <hours>\n" +
                "1. Opens a new poll that lasts for this many hours. The poll message will display when a player joins the server.\n" +
                "Players can vote once per poll. Results are logged to a file for review and stored to a list if you wish to open it again.\n" +
                "2. Cancels the open poll and does not announce or store it.\n" +
                "3. Stops an open poll and will announce the result so far.\n" +
                "4. Shows the current poll results in console.\n" +
                "5. Displays a list of old polls and their results.\n" +
                "6. Opens a poll from the list of old polls for this many hours.\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-Poll", "poll" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (!Poll.IsEnabled)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Polling is disabled. You must enable the tool first", _params.Count));
                return;
            }
            try
            {
                if (_params.Count < 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected more than 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower() == "new")
                {
                    if (!int.TryParse(_params[1], out int _hours))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer {0}", _params[1]));
                        return;
                    }
                    if (PersistentContainer.Instance.PollOpen)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] A poll is already open");
                        return;
                    }
                    else
                    {
                        _params.RemoveAt(0);
                        _params.RemoveAt(0);
                        string message = string.Join(" ", _params.ToArray());
                        string[] newPollData = { DateTime.Now.ToString(), _hours.ToString(), message };
                        PersistentContainer.Instance.PollData = newPollData;
                        PersistentContainer.Instance.PollVote = new Dictionary<string, bool>();
                        PersistentContainer.Instance.PollOpen = true;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] A new poll has opened for '{0}' hour. Poll message '{1}'", _hours, message));
                        using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("{0}: A new poll has opened for {1} hour. Message: {2}", DateTime.Now, _hours, message));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        return;
                    }
                }
                else if (_params[0] == "cancel")
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (PersistentContainer.Instance.PollOpen)
                    {
                        PersistentContainer.Instance.PollData = null;
                        PersistentContainer.Instance.PollVote = new Dictionary<string, bool>();
                        PersistentContainer.Instance.PollOpen = false;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] The open poll has been cancelled");
                        using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("{0}: The poll has been cancelled", DateTime.Now));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is no open poll to cancel");
                        return;
                    }
                }
                else if (_params[0] == "stop")
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    if (PersistentContainer.Instance.PollOpen)
                    {
                        if (PersistentContainer.Instance.PollData == null || PersistentContainer.Instance.PollVote == null)
                        {
                            PersistentContainer.Instance.PollData = null;
                            PersistentContainer.Instance.PollVote = new Dictionary<string, bool>();
                            PersistentContainer.Instance.PollOpen = false;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] The open poll has been stopped but no announcement made due to missing data");
                            using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                            {
                                sw.WriteLine(string.Format("{0}: The poll has been stopped. Data was missing so nothing was recorded", DateTime.Now));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            return;
                        }
                        else
                        {
                            string[] _pollData = PersistentContainer.Instance.PollData;
                            Dictionary<string, bool> _pollVotes = PersistentContainer.Instance.PollVote;
                            if (_pollVotes.Count == 0)
                            {
                                PersistentContainer.Instance.PollData = null;
                                PersistentContainer.Instance.PollOpen = false;
                                PersistentContainer.DataChange = true;
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] The open poll has been stopped but no announcement made due to no votes");
                                using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: The poll has been stopped. There was no votes recorded", DateTime.Now));
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
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The open poll has been stopped and recorded. Yes votes have won {0} to {1}. Poll message: {2}", _yes, _no, _pollData[2]));
                                using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: The poll has been stopped. Yes votes have won {1} to {2}", DateTime.Now, _yes, _no));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
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
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The open poll has been stopped and recorded. No votes have won {0} to {1}. Poll message: {2}", _no, _yes, _pollData[2]));
                                using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: The poll has been stopped. No votes have won {1} to {2}", DateTime.Now, _no, _yes));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
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
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The open poll has been stopped and recorded. It was a tie at {0} votes each. Poll message: {1}", _yes, _pollData[2]));
                                using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: The poll has been stopped. It was a tie at {1} votes each", DateTime.Now, _yes));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                            PersistentContainer.Instance.PollData = null;
                            PersistentContainer.Instance.PollVote = new Dictionary<string, bool>();
                            PersistentContainer.Instance.PollOpen = false;
                            PersistentContainer.DataChange = true;
                            if (!Phrases.Dict.TryGetValue("Poll2", out string _phrase))
                            {
                                _phrase = "Poll: {Message}";
                            }
                            _phrase = _phrase.Replace("{Message}", _pollData[2]);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            if (!Phrases.Dict.TryGetValue("Poll1", out _phrase))
                            {
                                _phrase = "Poll results: Yes {YesVote} / No {NoVote}";
                            }
                            _phrase = _phrase.Replace("{YesVote}", _yes.ToString());
                            _phrase = _phrase.Replace("{NoVote}", _no.ToString());
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is no open poll to stop");
                        return;
                    }
                }
                else if (_params[0] == "check")
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    if (PersistentContainer.Instance.PollOpen)
                    {
                        if (PersistentContainer.Instance.PollData != null && PersistentContainer.Instance.PollVote != null)
                        {
                            string[] _pollData = PersistentContainer.Instance.PollData;
                            DateTime.TryParse(_pollData[0], out DateTime _pollTime);
                            int.TryParse(_pollData[1], out int _pollHours);
                            TimeSpan varTime = DateTime.Now - _pollTime;
                            double fractionalHours = varTime.TotalHours;
                            int _timepassed = (int)fractionalHours;
                            int _timeLeft = _pollHours - _timepassed;
                            if (_timeLeft > 0)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is a poll running");
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] It is set to run for {0} hour. {1} hour remains. Poll message: {2}", _pollData[1], _timeLeft, _pollData[2]));
                                return;
                            }
                            else
                            {
                                Dictionary<string, bool> _pollVotes = PersistentContainer.Instance.PollVote;
                                if (_pollVotes.Count == 0)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] The open poll has been stopped but no announcement made due to no votes");
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
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The open poll has been stopped and recorded. Yes votes have won {0} to {1}. Poll message: {2}", _yes, _no, _pollData[2]));
                                    using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                                    {
                                        sw.WriteLine(string.Format("{0}: The poll has been stopped. Yes votes have won {1} to {2}", DateTime.Now, _yes, _no));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
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
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The open poll has been stopped and recorded. No votes have won {0} to {1}. Poll message: {2}", _no, _yes, _pollData[2]));
                                    using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                                    {
                                        sw.WriteLine(string.Format("{0}: The poll has been stopped. No votes have won {1} to {2}", DateTime.Now, _no, _yes));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
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
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The open poll has been stopped and recorded. It was a tie at {0} votes each. Poll message: {1}", _yes, _pollData[2]));
                                    using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                                    {
                                        sw.WriteLine(string.Format("{0}: The poll has been stopped. It was a tie at {1} votes each", DateTime.Now, _yes));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                }
                                if (!Phrases.Dict.TryGetValue("Poll2", out string _phrase))
                                {
                                    _phrase = "Poll: {Message}";
                                }
                                _phrase = _phrase.Replace("{Message}", _pollData[2]);
                                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                if (!Phrases.Dict.TryGetValue("Poll1", out _phrase))
                                {
                                    _phrase = "Poll results: Yes {YesVote} / No {NoVote}";
                                }
                                _phrase = _phrase.Replace("{YesVote}", _yes.ToString());
                                _phrase = _phrase.Replace("{NoVote}", _no.ToString());
                                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                _params.RemoveAt(0);
                                string _message = string.Join(" ", _params.ToArray());
                                string[] _newPollData = { DateTime.Now.ToString(), _pollHours.ToString(), _message };
                                PersistentContainer.Instance.PollData = _newPollData;
                                PersistentContainer.Instance.PollVote = new Dictionary<string, bool>();
                                PersistentContainer.Instance.PollOpen = true;
                                PersistentContainer.DataChange = true;
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] A new poll has opened for {0} hour. Poll message: {1}", _pollHours, _message));
                                return;
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is a open poll but the data is missing. Cancel the poll or start a new one");
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is no open poll");
                        return;
                    }
                }
                else if (_params[0] == "list")
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    else
                    {
                        if (PersistentContainer.Instance.PollOld != null && PersistentContainer.Instance.PollOld.Count > 0)
                        {
                            Dictionary<string[], string> _oldPoll = PersistentContainer.Instance.PollOld;
                            List<string[]> _keys = _oldPoll.Keys.ToList();
                            for (int i = 0; i < _keys.Count; i++)
                            {
                                KeyValuePair<string[], string> _poll = _oldPoll.ElementAt(i);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Poll {0} opened at {1} for {2} hour", i + 1, _poll.Key[0], _poll.Key[1]));
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Message: {0}", _poll.Key[2]));
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Vote: {0}", _poll.Value));
                            }
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There are no saved polls");
                            return;
                        }
                    }
                }
                else if (_params[0] == "open")
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[1], out int _number))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer {0}", _params[1]));
                        return;
                    }
                    if (!int.TryParse(_params[2], out int _hours))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer {0}", _params[2]));
                        return;
                    }
                    if (PersistentContainer.Instance.PollOpen)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] A poll is already open");
                        return;
                    }
                    int _numberAdjusted = _number - 1;
                    if (PersistentContainer.Instance.PollOld != null && PersistentContainer.Instance.PollOld.Count > 0 && _numberAdjusted >= 0)
                    {
                        Dictionary<string[], string> _oldPoll = PersistentContainer.Instance.PollOld;
                        if (_numberAdjusted <= _oldPoll.Count)
                        {
                            KeyValuePair<string[], string> _poll = _oldPoll.ElementAt(_number - 1);
                            string[] _newPollData = { DateTime.Now.ToString(), _hours.ToString(), _poll.Key[2] };
                            PersistentContainer.Instance.PollData = _newPollData;
                            PersistentContainer.Instance.PollVote = new Dictionary<string, bool>();
                            PersistentContainer.Instance.PollOpen = true;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Poll {0} has been opened for {1} hour", _number, _hours));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Message: {0}", _poll.Key[2]));
                            using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                            {
                                sw.WriteLine(string.Format("{0}: A new poll has opened for {1} hour. Message: {2}", DateTime.Now, _hours, _poll.Key[2]));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] That entry does not exist in the list");
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There are no saved polls");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PollConsole.Execute: {0}", e.Message));
            }
        }
    }
}

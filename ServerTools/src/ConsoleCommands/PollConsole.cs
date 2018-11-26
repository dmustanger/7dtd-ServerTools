using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ServerTools
{
    class PollConsole : ConsoleCmdAbstract
    {
        public static List<string> PolledYes = new List<string>();
        public static List<string> PolledNo = new List<string>();
        private static string _file = string.Format("PollLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/PollLogs/{1}", API.GamePath, _file);

        public override string GetDescription()
        {
            return "[ServerTools]-Opens a new poll for players to vote for.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. Poll new <time> <message>\n" +
                "  2. Poll close <true/false>\n" +
                "  3. Poll check\n" +
                "  4. Poll last\n" +
                "  5. Poll reopen <time>\n" +
                "1. Opens a new poll for <time> in hours. The poll message will display when a player joins the server.\n" +
                "Players can vote once per poll. Results are logged to a file for review.\n" +
                "2. Closes an open poll and will announce the result if true.\n" +
                "3. Shows the current poll results in console.\n" +
                "4. Displays the last poll in console.\n" +
                "5. Opens the last poll for <time> in hours.\n";
        }



        public override string[] GetCommands()
        {
            return new string[] { "st-Poll", "poll" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0] == "new")
                {
                    if (_params.Count < 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected at least 3, found {0}", _params.Count));
                        return;
                    }
                    else
                    {
                        int _hours;
                        if (!int.TryParse(_params[1], out _hours))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid integer {0}", _params[1]));
                            return;
                        }
                        _params.RemoveRange(0, 2);
                        string _message = string.Join(" ", _params.ToArray());
                        string _sql = "SELECT pollTime, pollHours, pollMessage FROM Polls WHERE pollOpen = 'true'";
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count > 0)
                        {
                            DateTime _pollTime;
                            DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _pollTime);
                            int _pollHours;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _pollHours);
                            string _pollMessage = _result.Rows[0].ItemArray.GetValue(2).ToString();
                            TimeSpan varTime = DateTime.Now - _pollTime;
                            double fractionalHours = varTime.TotalHours;
                            int _timepassed = (int)fractionalHours;
                            if (_timepassed >= _pollHours)
                            {
                                SdtdConsole.Instance.Output("There is a poll open but the time has expired.");
                                SdtdConsole.Instance.Output(string.Format("Poll: {0}", _pollMessage));
                                SdtdConsole.Instance.Output("You need to close the above poll before making a new one.");
                            }
                            else
                            {
                                int _timeleft = _pollHours - _timepassed;
                                SdtdConsole.Instance.Output("There is a poll open. Let it finish or close it");
                                SdtdConsole.Instance.Output(string.Format("Poll: {0}", _pollMessage));
                                SdtdConsole.Instance.Output(string.Format("Time Remaining: {0} hours", _timeleft));
                            }
                        }
                        else
                        {
                            _sql = string.Format("INSERT INTO Polls (pollOpen, pollTime, pollHours, pollMessage) VALUES ('true', '{0}', {1}, '{2}')", DateTime.Now, _hours, _message);
                            SQL.FastQuery(_sql);
                            string _phrase926;
                            if (!Phrases.Dict.TryGetValue(926, out _phrase926))
                            {
                                _phrase926 = "Poll: {Message}";
                            }
                            _phrase926 = _phrase926.Replace("{Message}", _message);
                            string _phrase927;
                            if (!Phrases.Dict.TryGetValue(927, out _phrase927))
                            {
                                _phrase927 = "Type /pollyes or /pollno to vote.";
                            }
                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase926, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase927, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            SdtdConsole.Instance.Output(string.Format("Opened a new poll for {0} hours.", _hours));
                            using (StreamWriter sw = new StreamWriter(_filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}  New poll {1} ... The poll will be open for {2} hours", DateTime.Now, _message, _hours));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                        }
                        _result.Dispose();
                    }
                }
                else if (_params[0] == "close")
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    bool _announce = false;
                    if (!bool.TryParse(_params[1], out _announce))
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid true/false argument: {0}", _params[1]));
                        return;
                    }
                    else
                    {
                        string _sql = "SELECT pollMessage, pollYes, pollNo FROM Polls WHERE pollOpen = 'true'";
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count > 0)
                        {
                            int _pollYes;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _pollYes);
                            int _pollNo;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(2).ToString(), out _pollNo);
                            if (_announce)
                            {
                                string _phrase925;
                                if (!Phrases.Dict.TryGetValue(925, out _phrase925))
                                {
                                    _phrase925 = "Poll results: Yes {YesVote} / No {NoVote}";
                                }
                                _phrase925 = _phrase925.Replace("{YesVote}", _pollYes.ToString());
                                _phrase925 = _phrase925.Replace("{NoVote}", _pollNo.ToString());
                                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase925, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                            using (StreamWriter sw = new StreamWriter(_filepath, true))
                            {
                                string _pollMessage = _result.Rows[0].ItemArray.GetValue(0).ToString();
                                sw.WriteLine(string.Format("{0}  Poll {1} ... has completed. The final results were yes {2} / no {3}", DateTime.Now, _pollMessage, _pollYes, _pollNo));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            _sql = "SELECT pollMessage, pollYes, pollNo FROM Polls WHERE pollOpen = 'false'";
                            DataTable _result1 = SQL.TQuery(_sql);
                            if (_result1.Rows.Count > 0)
                            {
                                _sql = "DELETE FROM Polls WHERE pollOpen = 'false'";
                                SQL.FastQuery(_sql);
                            }
                            _result1.Dispose();
                            _sql = "UPDATE Polls SET pollOpen = 'false' WHERE pollOpen = 'true'";
                            SQL.FastQuery(_sql);
                            PolledYes.Clear();
                            PolledNo.Clear();
                            SdtdConsole.Instance.Output("Closed the open poll.");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("No poll is open");
                        }
                        _result.Dispose();
                    }
                }
                else if (_params[0] == "last")
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    else
                    {
                        string _sql = "SELECT pollHours, pollMessage, pollYes, pollNo FROM Polls WHERE pollOpen = 'false'";
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count > 0)
                        {
                            int _pollHours;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _pollHours);
                            string _pollMessage = _result.Rows[0].ItemArray.GetValue(1).ToString();
                            int _pollYes;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(2).ToString(), out _pollYes);
                            int _pollNo;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(3).ToString(), out _pollNo);
                            SdtdConsole.Instance.Output(string.Format("The last poll message: {0}", _pollMessage));
                            SdtdConsole.Instance.Output(string.Format("Last poll results: Yes {0} / No {1}", _pollYes, _pollNo));
                            SdtdConsole.Instance.Output(string.Format("Poll was open for {0} hours", _pollHours));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("There are no saved prior poll results");
                        }
                        _result.Dispose();
                    }
                }
                else if (_params[0] == "reopen")
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    string _sql = "SELECT pollTime, pollHours, pollMessage FROM Polls WHERE pollOpen = 'true'";
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count > 0)
                    {
                        SdtdConsole.Instance.Output("A poll is open. Can not open a new poll until it is closed");
                    }
                    else
                    {
                        _sql = "SELECT pollTime, pollHours, pollMessage FROM Polls WHERE pollOpen = 'false'";
                        DataTable _result1 = SQL.TQuery(_sql);
                        if (_result1.Rows.Count > 0)
                        {
                            int _hours;
                            int.TryParse(_params[1], out _hours);
                            _sql = string.Format("UPDATE Polls SET pollOpen = 'true', pollTime = '{0}', pollHours = {1} WHERE pollOpen = 'false'", DateTime.Now, _hours);
                            SQL.FastQuery(_sql);
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("You have no previous poll");
                        }
                        _result1.Dispose();
                    }
                    _result.Dispose();
                }
                else if (_params[0] == "check")
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    string _sql = "SELECT pollTime, pollHours, pollMessage, pollYes, pollNo FROM Polls WHERE pollOpen = 'true'";
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count > 0)
                    {
                        DateTime _pollTime;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _pollTime);
                        int _pollHours;
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _pollHours);
                        string _pollMessage = _result.Rows[0].ItemArray.GetValue(2).ToString();
                        int _pollYes;
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(3).ToString(), out _pollYes);
                        int _pollNo;
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(4).ToString(), out _pollNo);
                        TimeSpan varTime = DateTime.Now - _pollTime;
                        double fractionalHours = varTime.TotalHours;
                        int _timepassed = (int)fractionalHours;
                        if (_timepassed >= _pollHours)
                        {
                            SdtdConsole.Instance.Output("There is a poll open but the time has expired.");
                            SdtdConsole.Instance.Output(string.Format("Poll: {0}", _pollMessage));
                            SdtdConsole.Instance.Output(string.Format("Current poll results: Yes votes {0} / No votes {1}", _pollYes, _pollNo));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Poll: {0}", _pollMessage));
                            SdtdConsole.Instance.Output(string.Format("Current poll results: Yes votes {0} / No votes {1}", _pollYes, _pollNo));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("No poll is open");
                    }
                    _result.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PollConsole.Run: {0}.", e));
            }
        }

        public static void CreateFolder()
        {
            if (!Directory.Exists(API.GamePath + "/PollLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/PollLogs");
            }
        }

        public static void Message(ClientInfo _cInfo)
        {
            string _sql = "SELECT pollTime, pollHours, pollMessage, pollYes, pollNo FROM Polls WHERE pollOpen = 'true'";
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                DateTime _pollTime;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _pollTime);
                int _pollHours;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _pollHours);
                string _pollMessage = _result.Rows[0].ItemArray.GetValue(2).ToString();
                int _pollYes;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(3).ToString(), out _pollYes);
                int _pollNo;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(4).ToString(), out _pollNo);
                TimeSpan varTime = DateTime.Now - _pollTime;
                double fractionalHours = varTime.TotalHours;
                int _timepassed = (int)fractionalHours;
                if (_timepassed <= _pollHours)
                {
                    string _phrase926;
                    if (!Phrases.Dict.TryGetValue(926, out _phrase926))
                    {
                        _phrase926 = "Poll: {Message}";
                    }
                    _phrase926 = _phrase926.Replace("{Message}", _pollMessage);
                    string _phrase927;
                    if (!Phrases.Dict.TryGetValue(927, out _phrase927))
                    {
                        _phrase927 = "Type /pollyes or /pollno to vote.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase926 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase927 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _phrase925;
                    if (!Phrases.Dict.TryGetValue(925, out _phrase925))
                    {
                        _phrase925 = "Poll results: Yes {YesVote} / No {NoVote}";
                    }
                    _phrase925 = _phrase925.Replace("{YesVote}", _pollYes.ToString());
                    _phrase925 = _phrase925.Replace("{NoVote}", _pollNo.ToString());
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase925 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
            _result.Dispose();
        }

        public static void VoteYes(ClientInfo _cInfo)
        {
            string _sql = "SELECT pollYes, pollNo FROM Polls WHERE pollOpen = 'true'";
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                if (PolledYes.Contains(_cInfo.playerId) || PolledNo.Contains(_cInfo.playerId))
                {
                    string _phrase812;
                    if (!Phrases.Dict.TryGetValue(812, out _phrase812))
                    {
                        _phrase812 = "you have already voted on the poll.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase812 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    PolledYes.Add(_cInfo.playerId);
                    int _pollYes;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _pollYes);
                    int _pollNo;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _pollNo);
                    _pollYes = _pollYes + 1;
                    _sql = string.Format("UPDATE Polls SET pollYes = {0} WHERE pollOpen = 'true'", _pollYes);
                    SQL.FastQuery(_sql);
                    string _phrase928;
                    if (!Phrases.Dict.TryGetValue(928, out _phrase928))
                    {
                        _phrase928 = "you have cast a vote for yes. Currently, the pole is yes {Yes} / no {No}.";
                    }
                    _phrase928 = _phrase928.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase928 = _phrase928.Replace("{Yes}", _pollYes.ToString());
                    _phrase928 = _phrase928.Replace("{No}", _pollNo.ToString());
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase928 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}  Player name {1} has voted yes in the poll. Yes {2} / no {3}", DateTime.Now, _cInfo.playerName, _pollYes, _pollNo));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            _result.Dispose(); 
        }

        public static void VoteNo(ClientInfo _cInfo)
        {
            string _sql = "SELECT pollYes, pollNo FROM Polls WHERE pollOpen = 'true'";
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                if (PolledYes.Contains(_cInfo.playerId) || PolledNo.Contains(_cInfo.playerId))
                {
                    string _phrase812;
                    if (!Phrases.Dict.TryGetValue(812, out _phrase812))
                    {
                        _phrase812 = "you have already voted on the poll";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase812 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    PolledNo.Add(_cInfo.playerId);
                    int _pollYes;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _pollYes);
                    int _pollNo;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _pollNo);
                    _pollNo = _pollNo + 1;
                    _sql = string.Format("UPDATE Polls SET pollNo = {0} WHERE pollOpen = 'true'", _pollNo);
                    SQL.FastQuery(_sql);
                    string _phrase929;
                    if (!Phrases.Dict.TryGetValue(929, out _phrase929))
                    {
                        _phrase929 = "you have cast a vote for no. Currently, the pole is yes {Yes} / no {No}.";
                    }
                    _phrase929 = _phrase929.Replace("{Yes}", _pollYes.ToString());
                    _phrase929 = _phrase929.Replace("{No}", _pollNo.ToString());
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase929 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}  Player name {1} has voted no in the poll. Yes {2} / no {3}", DateTime.Now, _cInfo.playerName, _pollYes, _pollNo));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            _result.Dispose();
        }

        public static void Check()
        {
            string _sql = "SELECT pollTime, pollHours, pollMessage, pollYes, pollNo FROM Polls WHERE pollOpen = 'true'";
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                DateTime _pollTime;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _pollTime);
                int _pollHours;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _pollHours);
                string _pollMessage = _result.Rows[0].ItemArray.GetValue(2).ToString();
                int _pollYes;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(3).ToString(), out _pollYes);
                int _pollNo;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(4).ToString(), out _pollNo);
                TimeSpan varTime = DateTime.Now - _pollTime;
                double fractionalHours = varTime.TotalHours;
                int _timepassed = (int)fractionalHours;
                if (_timepassed >= _pollHours)
                {
                    string _phrase925;
                    if (!Phrases.Dict.TryGetValue(925, out _phrase925))
                    {
                        _phrase925 = "Poll results: Yes {YesVote} / No {NoVote}";
                    }
                    _phrase925 = _phrase925.Replace("{YesVote}", _pollYes.ToString());
                    _phrase925 = _phrase925.Replace("{NoVote}", _pollNo.ToString());
                    _sql = "UPDATE Polls SET pollOpen = 'false' WHERE pollOpen = 'true'";
                    SQL.FastQuery(_sql);
                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}  Poll {1} ... has completed. The final results were yes {2} / no {3}", DateTime.Now, _pollMessage, _pollYes, _pollNo));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            _result.Dispose();
        }
    }
}

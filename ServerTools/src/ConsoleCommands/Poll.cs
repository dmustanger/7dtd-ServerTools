using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class Poll : ConsoleCmdAbstract
    {
        public static List<string> PollMessage = new List<string>();
        public static List<int> PolledYes = new List<int>();
        public static List<int> PolledNo = new List<int>();
        private static string _file = string.Format("PollLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/PollLogs/{1}", API.GamePath, _file);

        public override string GetDescription()
        {
            return "[ServerTools]-Opens a new poll for players to vote for.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. Poll open <time> <message>\n" +
                "  2. Poll close <true/false>\n" +
                "  3. Poll \n" +
                "  4. Poll last\n" +
                "  5. Poll reopen <time>\n" +
                "1. Opens a new poll for <time> in hours. The poll message will display when a player joins the server.\n" +
                "Players can vote once per poll. Results are logged to a file for review.\n" +
                "2. Closes an open poll and will announce the result if true.\n" +
                "3. Shows the current poll results in console.\n" +
                "4. Displays the last poll in console.\n" +
                "5. Opens the last poll for <time> in hours. If no time is given, it will repeat the same time as last.\n";
        }



        public override string[] GetCommands()
        {
            return new string[] { "st-Poll", "poll" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 0 to 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0] == "open")
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 3, found {0}", _params.Count));
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
                        _params.RemoveRange(0,1);
                        string _message = string.Join(" ", _params.ToArray());
                        if (!PersistentContainer.Instance.PollOpen)
                        {
                            PersistentContainer.Instance.PollOpen = true;
                            PersistentContainer.Instance.PollTime = DateTime.Now;
                            PersistentContainer.Instance.PollHours = _hours;
                            PersistentContainer.Instance.PollMessage = _message;
                            PersistentContainer.Instance.Save();
                            string _phrase926;
                            if (!Phrases.Dict.TryGetValue(926, out _phrase926))
                            {
                                _phrase926 = "Poll: {Message}";
                            }
                            _phrase926 = _phrase926.Replace("{Message}", PersistentContainer.Instance.PollMessage);
                            string _phrase927;
                            if (!Phrases.Dict.TryGetValue(927, out _phrase927))
                            {
                                _phrase927 = "Type /pollyes or /pollno to vote.";
                            }
                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase926), Config.Server_Response_Name, false, "ServerTools", true);
                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase927), Config.Server_Response_Name, false, "ServerTools", true);
                            SdtdConsole.Instance.Output(string.Format("Opened a new poll for {0} hours.", _hours));
                            using (StreamWriter sw = new StreamWriter(_filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}  New poll {1} ... The poll will be open for {2} hours", DateTime.Now, _message, _hours));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            return;
                        }
                        else
                        {
                            TimeSpan varTime = DateTime.Now - PersistentContainer.Instance.PollTime;
                            double fractionalHours = varTime.TotalHours;
                            int _timepassed = (int)fractionalHours;
                            if (_timepassed >= PersistentContainer.Instance.PollHours)
                            {
                                string _phrase925;
                                if (!Phrases.Dict.TryGetValue(925, out _phrase925))
                                {
                                    _phrase925 = "Poll results: Yes {YesVote} / No {NoVote}";
                                }
                                _phrase925 = _phrase925.Replace("{YesVote}", PersistentContainer.Instance.PollYes.ToString());
                                _phrase925 = _phrase925.Replace("{NoVote}", PersistentContainer.Instance.PollNo.ToString());
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}", _phrase925), Config.Server_Response_Name, false, "ServerTools", true);
                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}  Poll {1} ... has completed. The final results were yes {2} / no {3}", DateTime.Now, PersistentContainer.Instance.PollMessage, PersistentContainer.Instance.PollYes, PersistentContainer.Instance.PollNo));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                PersistentContainer.Instance.LastPollHours = PersistentContainer.Instance.PollHours;
                                PersistentContainer.Instance.LastPollMessage = PersistentContainer.Instance.PollMessage;
                                PersistentContainer.Instance.LastPollYes = PersistentContainer.Instance.PollYes;
                                PersistentContainer.Instance.LastPollNo = PersistentContainer.Instance.PollNo;
                                PersistentContainer.Instance.PollTime = DateTime.Now;
                                PersistentContainer.Instance.PollHours = _hours;
                                PersistentContainer.Instance.PollMessage = _message;
                                PersistentContainer.Instance.PollYes = 0;
                                PersistentContainer.Instance.PollNo = 0;
                                PersistentContainer.Instance.PolledYes = null;
                                PersistentContainer.Instance.PolledNo = null;
                                PersistentContainer.Instance.Save();
                                SdtdConsole.Instance.Output(string.Format("There was a poll open but the time expired. Opened a new poll and announced the result."));
                                return;
                            }
                            else
                            {
                                int _timeleft = PersistentContainer.Instance.PollHours - _timepassed;
                                SdtdConsole.Instance.Output("There is a poll open. Let it finish or close it");
                                SdtdConsole.Instance.Output(string.Format("Time Remaining: {0} hours", _timeleft));
                                return;
                            }
                        }
                    }
                }
                if (_params[0] == "close")
                {
                    if (_params.Count < 1 || _params.Count > 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                        return;
                    }
                    bool _announce = false;
                    if (_params.Count == 2)
                    {
                        if (!bool.TryParse(_params[1], out _announce))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid true/false argument: {0}", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        if (PersistentContainer.Instance.PollOpen)
                        {
                            if (_announce)
                            {
                                string _phrase925;
                                if (!Phrases.Dict.TryGetValue(925, out _phrase925))
                                {
                                    _phrase925 = "Poll results: Yes {YesVote} / No {NoVote}";
                                }
                                _phrase925 = _phrase925.Replace("{YesVote}", PersistentContainer.Instance.PollYes.ToString());
                                _phrase925 = _phrase925.Replace("{NoVote}", PersistentContainer.Instance.PollNo.ToString());
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}", _phrase925), Config.Server_Response_Name, false, "ServerTools", true);
                            }
                            using (StreamWriter sw = new StreamWriter(_filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}  Poll {1} ... has completed. The final results were yes {2} / no {3}", DateTime.Now, PersistentContainer.Instance.PollMessage, PersistentContainer.Instance.PollYes, PersistentContainer.Instance.PollNo));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            PersistentContainer.Instance.PollOpen = false;
                            PersistentContainer.Instance.LastPollHours = PersistentContainer.Instance.PollHours;
                            PersistentContainer.Instance.LastPollMessage = PersistentContainer.Instance.PollMessage;
                            PersistentContainer.Instance.LastPollYes = PersistentContainer.Instance.PollYes;
                            PersistentContainer.Instance.LastPollNo = PersistentContainer.Instance.PollNo;
                            PersistentContainer.Instance.PollYes = 0;
                            PersistentContainer.Instance.PollNo = 0;
                            PersistentContainer.Instance.PolledYes = null;
                            PersistentContainer.Instance.PolledNo = null;
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("Closed the open poll and announced the results"));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("No poll is open"));
                            return;
                        }
                    }
                }
                if (_params[0] == "last")
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    else
                    {
                        if (PersistentContainer.Instance.LastPollMessage != null)
                        {
                            SdtdConsole.Instance.Output(string.Format("The last poll message: {0}", PersistentContainer.Instance.LastPollMessage));
                            SdtdConsole.Instance.Output(string.Format("Last poll results: Yes {0} / No {1}", PersistentContainer.Instance.LastPollYes, PersistentContainer.Instance.LastPollNo));
                            SdtdConsole.Instance.Output(string.Format("Poll was open for {0} hours", PersistentContainer.Instance.LastPollHours));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("There are no saved prior poll results"));
                            return;
                        }
                    }
                }
                if (_params[0] == "reopen")
                {
                    if (_params.Count < 1 || _params.Count > 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                        return;
                    }
                    int _hours = PersistentContainer.Instance.PollHours;
                    int.TryParse(_params[1], out _hours);
                    if (!PersistentContainer.Instance.PollOpen)
                    {
                        if (PersistentContainer.Instance.LastPollMessage != null)
                        {
                            PersistentContainer.Instance.PollOpen = true;
                            PersistentContainer.Instance.PollHours = _hours;
                            PersistentContainer.Instance.PollTime = DateTime.Now;
                            PersistentContainer.Instance.Save();
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("You have no previous poll"));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("A poll is open. Can not open a new poll until it completes or is closed"));
                        return;
                    }
                }
                if (_params.Count == 0)
                {
                    if (PersistentContainer.Instance.PollOpen)
                    {
                        TimeSpan varTime = DateTime.Now - PersistentContainer.Instance.PollTime;
                        double fractionalHours = varTime.TotalHours;
                        int _timepassed = (int)fractionalHours;
                        if (_timepassed >= PersistentContainer.Instance.PollHours)
                        {
                            string _phrase925;
                            if (!Phrases.Dict.TryGetValue(925, out _phrase925))
                            {
                                _phrase925 = "Poll results: Yes {YesVote} / No {NoVote}";
                            }
                            _phrase925 = _phrase925.Replace("{YesVote}", PersistentContainer.Instance.PollYes.ToString());
                            _phrase925 = _phrase925.Replace("{NoVote}", PersistentContainer.Instance.PollNo.ToString());
                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase925), Config.Server_Response_Name, false, "ServerTools", true);
                            using (StreamWriter sw = new StreamWriter(_filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}  Poll {1} ... has completed. The final results were yes {2} / no {3}", DateTime.Now, PersistentContainer.Instance.PollMessage, PersistentContainer.Instance.PollYes, PersistentContainer.Instance.PollNo));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            PersistentContainer.Instance.PollOpen = false;
                            PersistentContainer.Instance.LastPollHours = PersistentContainer.Instance.PollHours;
                            PersistentContainer.Instance.LastPollMessage = PersistentContainer.Instance.PollMessage;
                            PersistentContainer.Instance.LastPollYes = PersistentContainer.Instance.PollYes;
                            PersistentContainer.Instance.LastPollNo = PersistentContainer.Instance.PollNo;
                            PersistentContainer.Instance.PollYes = 0;
                            PersistentContainer.Instance.PollNo = 0;
                            PersistentContainer.Instance.PolledYes = null;
                            PersistentContainer.Instance.PolledNo = null;
                            PersistentContainer.Instance.Save();
                            PolledYes.Clear();
                            PolledNo.Clear();
                            SdtdConsole.Instance.Output(string.Format("The poll time expired. Announced the results"));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Poll message: {0}", PersistentContainer.Instance.PollMessage));
                            SdtdConsole.Instance.Output(string.Format("Current poll results: Yes votes {0} / No votes {1}", PersistentContainer.Instance.PollYes, PersistentContainer.Instance.PollNo));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("No poll is open"));
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Poll.Run: {0}.", e));
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
            TimeSpan varTime = DateTime.Now - PersistentContainer.Instance.PollTime;
            double fractionalHours = varTime.TotalHours;
            int _timepassed = (int)fractionalHours;
            if (_timepassed <= PersistentContainer.Instance.PollHours)
            {
                string _phrase926;
                if (!Phrases.Dict.TryGetValue(926, out _phrase926))
                {
                    _phrase926 = "Poll: {Message}";
                }
                _phrase926 = _phrase926.Replace("{Message}", PersistentContainer.Instance.PollMessage);
                string _phrase927;
                if (!Phrases.Dict.TryGetValue(927, out _phrase927))
                {
                    _phrase927 = "Type /pollyes or /pollno to vote.";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase926), Config.Server_Response_Name, false, "ServerTools", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase927), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                string _phrase925;
                if (!Phrases.Dict.TryGetValue(925, out _phrase925))
                {
                    _phrase925 = "Poll results: Yes {YesVote} / No {NoVote}";
                }
                _phrase925 = _phrase925.Replace("{YesVote}", PersistentContainer.Instance.PollYes.ToString());
                _phrase925 = _phrase925.Replace("{NoVote}", PersistentContainer.Instance.PollNo.ToString());
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase925), Config.Server_Response_Name, false, "ServerTools", true);
                using (StreamWriter sw = new StreamWriter(_filepath, true))
                {
                    sw.WriteLine(string.Format("{0}  Poll {1} ... has completed. The final results were yes {2} / no {3}", DateTime.Now, PersistentContainer.Instance.PollMessage, PersistentContainer.Instance.PollYes, PersistentContainer.Instance.PollNo));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                PolledYes.Clear();
                PolledNo.Clear();
                PersistentContainer.Instance.PollOpen = false;
                PersistentContainer.Instance.LastPollHours = PersistentContainer.Instance.PollHours;
                PersistentContainer.Instance.LastPollMessage = PersistentContainer.Instance.PollMessage;
                PersistentContainer.Instance.LastPollYes = PersistentContainer.Instance.PollYes;
                PersistentContainer.Instance.LastPollNo = PersistentContainer.Instance.PollNo;
                PersistentContainer.Instance.PollYes = 0;
                PersistentContainer.Instance.PollNo = 0;
                PersistentContainer.Instance.PolledYes = null;
                PersistentContainer.Instance.PolledNo = null;
                PersistentContainer.Instance.Save();
            }
        }

        public static void VoteYes(ClientInfo _cInfo)
        {
            PolledYes.Add(_cInfo.entityId);
            PersistentContainer.Instance.PollYes += 1;
            PersistentContainer.Instance.PolledYes = PolledYes;
            PersistentContainer.Instance.Save();
            string _phrase928;
            if (!Phrases.Dict.TryGetValue(928, out _phrase928))
            {
                _phrase928 = "{PlayerName} you have cast a vote for yes. Currently, the pole is yes {Yes} / no {No}.";
            }
            _phrase928 = _phrase928.Replace("{PlayerName}", _cInfo.playerName);
            _phrase928 = _phrase928.Replace("{Yes}", PersistentContainer.Instance.PollYes.ToString());
            _phrase928 = _phrase928.Replace("{No}", PersistentContainer.Instance.PollNo.ToString());
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase928), Config.Server_Response_Name, false, "ServerTools", false));
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("{0}  Player name {1} has voted yes in the poll. Yes {2} / no {3}", DateTime.Now, _cInfo.playerName, PersistentContainer.Instance.PollYes, PersistentContainer.Instance.PollNo));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }

        public static void VoteNo(ClientInfo _cInfo)
        {
            PolledNo.Add(_cInfo.entityId);
            PersistentContainer.Instance.PollNo += 1;
            PersistentContainer.Instance.PolledNo = PolledNo;
            PersistentContainer.Instance.Save();
            string _phrase929;
            if (!Phrases.Dict.TryGetValue(929, out _phrase929))
            {
                _phrase929 = "{PlayerName} you have cast a vote for no. Currently, the pole is yes {Yes} / no {No}.";
            }
            _phrase929 = _phrase929.Replace("{PlayerName}", _cInfo.playerName);
            _phrase929 = _phrase929.Replace("{Yes}", PersistentContainer.Instance.PollYes.ToString());
            _phrase929 = _phrase929.Replace("{No}", PersistentContainer.Instance.PollNo.ToString());
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase929), Config.Server_Response_Name, false, "ServerTools", false));
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("{0}  Player name {1} has voted no in the poll. Yes {2} / no {3}", DateTime.Now, _cInfo.playerName, PersistentContainer.Instance.PollYes, PersistentContainer.Instance.PollNo));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }

        public static void Check()
        {
            if (PersistentContainer.Instance.PollOpen)
            {
                TimeSpan varTime = DateTime.Now - PersistentContainer.Instance.PollTime;
                double fractionalHours = varTime.TotalHours;
                int _timepassed = (int)fractionalHours;
                if (_timepassed >= PersistentContainer.Instance.PollHours)
                {
                    string _phrase925;
                    if (!Phrases.Dict.TryGetValue(925, out _phrase925))
                    {
                        _phrase925 = "Poll results: Yes {YesVote} / No {NoVote}";
                    }
                    _phrase925 = _phrase925.Replace("{YesVote}", PersistentContainer.Instance.PollYes.ToString());
                    _phrase925 = _phrase925.Replace("{NoVote}", PersistentContainer.Instance.PollNo.ToString());
                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}  Poll {1} ... has completed. The final results were yes {2} / no {3}", DateTime.Now, PersistentContainer.Instance.PollMessage, PersistentContainer.Instance.PollYes, PersistentContainer.Instance.PollNo));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                    PolledYes.Clear();
                    PolledNo.Clear();
                    PersistentContainer.Instance.PollOpen = false;
                    PersistentContainer.Instance.PollHours = 0;
                    PersistentContainer.Instance.PollMessage = null;
                    PersistentContainer.Instance.PollYes = 0;
                    PersistentContainer.Instance.PollNo = 0;
                    PersistentContainer.Instance.PolledYes = null;
                    PersistentContainer.Instance.PolledNo = null;
                    PersistentContainer.Instance.LastPollHours = PersistentContainer.Instance.PollHours;
                    PersistentContainer.Instance.LastPollMessage = PersistentContainer.Instance.PollMessage;
                    PersistentContainer.Instance.LastPollYes = PersistentContainer.Instance.PollYes;
                    PersistentContainer.Instance.LastPollNo = PersistentContainer.Instance.PollNo;
                    PersistentContainer.Instance.Save();
                }
                else
                {
                    PolledYes = PersistentContainer.Instance.PolledYes;
                    PolledNo = PersistentContainer.Instance.PolledNo;
                }
            }
        }
    }
}

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
                string[] pollData = PersistentContainer.Instance.PollData;
                DateTime.TryParse(pollData[0], out DateTime _pollDate);
                int.TryParse(pollData[1], out int _pollHours);
                TimeSpan varTime = DateTime.Now - _pollDate;
                double fractionalHours = varTime.TotalHours;
                int timepassed = (int)fractionalHours;
                if (timepassed <= _pollHours)
                {
                    Phrases.Dict.TryGetValue("Poll2", out string phrase);
                    phrase = phrase.Replace("{Message}", pollData[2]);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    Phrases.Dict.TryGetValue("Poll3", out phrase);
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_poll_yes}", Command_poll_yes);
                    phrase = phrase.Replace("{Command_poll_no}", Command_poll_no);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void Yes(ClientInfo _cInfo)
        {
            if (!PersistentContainer.Instance.PollVote.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                Dictionary<string, bool> votes = PersistentContainer.Instance.PollVote;
                votes.Add(_cInfo.CrossplatformId.CombinedString, true);
                PersistentContainer.Instance.PollVote = votes;
                PersistentContainer.DataChange = true;
                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Id '{1}' '{2}' named '{3}' has voted yes in the poll", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("Poll4", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                
            }
            else
            {
                Phrases.Dict.TryGetValue("Poll6", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void No(ClientInfo _cInfo)
        {
            if (!PersistentContainer.Instance.PollVote.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                Dictionary<string, bool> votes = PersistentContainer.Instance.PollVote;
                votes.Add(_cInfo.CrossplatformId.CombinedString, false);
                PersistentContainer.Instance.PollVote = votes;
                PersistentContainer.DataChange = true;
                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Id '{1}' '{2}' named '{3}' has voted no in the poll", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("Poll5", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Poll6", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CheckTime()
        {
            if (PersistentContainer.Instance.PollData != null)
            {
                string[] pollData = PersistentContainer.Instance.PollData;
                DateTime.TryParse(pollData[0], out DateTime pollDate);
                int.TryParse(pollData[1], out int pollHours);
                TimeSpan varTime = DateTime.Now - pollDate;
                double fractionalHours = varTime.TotalHours;
                int timepassed = (int)fractionalHours;
                if (timepassed >= pollHours)
                {
                    if (PersistentContainer.Instance.PollVote == null)
                    {
                        return;
                    }
                    Dictionary<string, bool> pollVotes = PersistentContainer.Instance.PollVote;
                    if (pollVotes.Count == 0)
                    {
                        PersistentContainer.Instance.PollData = null;
                        PersistentContainer.Instance.PollOpen = false;
                        PersistentContainer.DataChange = true;
                        using (StreamWriter sw = new StreamWriter(Poll.Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("{0}: The poll has finished but no votes were cast", DateTime.Now));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        return;
                    }
                    int yes = 0, no = 0;
                    foreach (var vote in pollVotes)
                    {
                        if (vote.Value == true)
                        {
                            yes++;
                        }
                        else
                        {
                            no++;
                        }
                    }
                    if (yes > no)
                    {
                        if (PersistentContainer.Instance.PollOld != null && PersistentContainer.Instance.PollOld.Count > 0)
                        {
                            Dictionary<string[], string> oldPoll = PersistentContainer.Instance.PollOld;
                            oldPoll.Add(pollData, "yes");
                            PersistentContainer.Instance.PollOld = oldPoll;
                        }
                        else
                        {
                            Dictionary<string[], string> oldPoll = new Dictionary<string[], string>();
                            oldPoll.Add(pollData, "yes");
                            PersistentContainer.Instance.PollOld = oldPoll;
                        }
                        PersistentContainer.DataChange = true;
                    }
                    else if (no > yes)
                    {
                        if (PersistentContainer.Instance.PollOld != null && PersistentContainer.Instance.PollOld.Count > 0)
                        {
                            Dictionary<string[], string> oldPoll = PersistentContainer.Instance.PollOld;
                            oldPoll.Add(pollData, "no");
                            PersistentContainer.Instance.PollOld = oldPoll;
                        }
                        else
                        {
                            Dictionary<string[], string> oldPoll = new Dictionary<string[], string>();
                            oldPoll.Add(pollData, "no");
                            PersistentContainer.Instance.PollOld = oldPoll;
                        }
                        PersistentContainer.DataChange = true;
                    }
                    else
                    {
                        if (PersistentContainer.Instance.PollOld != null && PersistentContainer.Instance.PollOld.Count > 0)
                        {
                            Dictionary<string[], string> oldPoll = PersistentContainer.Instance.PollOld;
                            oldPoll.Add(pollData, "tie");
                            PersistentContainer.Instance.PollOld = oldPoll;
                        }
                        else
                        {
                            Dictionary<string[], string> oldPoll = new Dictionary<string[], string>();
                            oldPoll.Add(pollData, "tie");
                            PersistentContainer.Instance.PollOld = oldPoll;
                        }
                        PersistentContainer.DataChange = true;
                    }
                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(string.Format("{0}: The poll has completed and the players voted '{1}' yes / '{2}' no", DateTime.Now, yes, no));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
        }
    }
}

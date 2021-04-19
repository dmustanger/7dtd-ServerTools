using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NextShutdownConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Shows the next scheduled shutdown.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-ShutdownCheck\n" +
                   "1. Shows the next scheduled shutdown\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ShutdownCheck", "scheck", "st-sc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (Shutdown.IsEnabled)
                {
                    if (_senderInfo.RemoteClientInfo != null && _senderInfo.RemoteClientInfo.playerId != null)
                    {
                        EventSchedule.Schedule.TryGetValue("Shutdown", out DateTime _timeLeft);
                        TimeSpan varTime = _timeLeft - DateTime.Now;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _remainingTime = (int)fractionalMinutes;
                        if (_remainingTime <= 10 && Event.Open)
                        {
                            Phrases.Dict.TryGetValue(422, out string _phrase422);
                            ChatHook.ChatMessage(_senderInfo.RemoteClientInfo, Config.Chat_Response_Color + _phrase422 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        string TimeLeft;
                        TimeLeft = string.Format("{0:00} H : {1:00} M", _remainingTime / 60, _remainingTime % 60);
                        Phrases.Dict.TryGetValue(421, out string _phrase421);
                        _phrase421 = _phrase421.Replace("{TimeLeft}", TimeLeft);
                        ChatHook.ChatMessage(_senderInfo.RemoteClientInfo, Config.Chat_Response_Color + _phrase421 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Shutdown is not enabled.");
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in NextShutdownConsole.Execute: {0}", e.Message));
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class StopServer : ConsoleCmdAbstract
    {
        public static bool Ten_Second_Countdown = false, stopServerCountingDown = false, Kick_30_Seconds = false, NoEntry = false,
            Kick_Login = false;

        public override string GetDescription()
        {
            return "[ServerTools]-Stops the game server with a warning countdown every minute.";
        }

        public override string GetHelp()
        {
            return "Usage: stopserver <minutes>\n" +
                "Usage: stopserver cancel";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-StopServer", "stopserver" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0] == "cancel")
                {
                    if (!stopServerCountingDown)
                    {
                        SdtdConsole.Instance.Output("Stopserver is not running.");
                    }
                    else
                    {
                        stopServerCountingDown = false;
                        NoEntry = false;
                        SdtdConsole.Instance.Output("Stopserver has stopped.");
                    }
                }
                else
                {
                    if (stopServerCountingDown)
                    {
                        SdtdConsole.Instance.Output(string.Format("Server is already stopping in {0} mins", Timers._newCount));
                    }
                    else
                    {
                        if (!int.TryParse(_params[0], out Timers.Stop_Server_Time))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid time specified: {0}", _params[0]));
                        }
                        else
                        {                            
                            StartShutdown();
                        }
                    }                   
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in StopServer.Run: {0}.", e));
            }
        }

        public static void StartShutdown()
        {
            if (Kick_Login)
            {
                NoEntry = true;
            }
            if (!Timers.timer1Running)
            {
                Timers.TimerStart1Second();
            }                       
            if (Timers.Stop_Server_Time < 1)
            {
                Timers.Stop_Server_Time = 1;
            }
            Timers._sSC = Timers.Stop_Server_Time;
            stopServerCountingDown = true;
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Shutdown In {Minutes} Minutes.";
            }
            _phrase450 = _phrase450.Replace("{Minutes}", Timers.Stop_Server_Time.ToString());
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase450), "Server", false, "", false);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase450), "Server", false, "", false);
        }

        public static void StartShutdown2(int _newCount)
        {
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Shutdown In {Minutes} Minutes.";
            }
            _phrase450 = _phrase450.Replace("{Minutes}", _newCount.ToString());
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase450), "Server", false, "", false);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase450), "Server", false, "", false);
        }

        public static void StartShutdown3()
        {
            string _phrase451;
            if (!Phrases.Dict.TryGetValue(451, out _phrase451))
            {
                _phrase451 = "Saving World Now. Do not exchange items from inventory or build.";
            }
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Shutdown In {Minutes} Minutes.";
            }
            _phrase450 = _phrase450.Replace("{Minutes}", "1");
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase451), "Server", false, "", false);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase450), "Server", false, "", false);
            SdtdConsole.Instance.ExecuteSync("saveworld", (ClientInfo)null);
        }

        public static void StartShutdown4()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]10 seconds until shutdown[-]"), "Server", false, "", false);
        }

        public static void StartShutdown5()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]5[-]"), "Server", false, "", false);
        }

        public static void StartShutdown6()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]4[-]"), "Server", false, "", false);
        }

        public static void StartShutdown7()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]3[-]"), "Server", false, "", false);
        }

        public static void StartShutdown8()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]2[-]"), "Server", false, "", false);
        }

        public static void StartShutdown9()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]1[-]"), "Server", false, "", false);
        }

        public static void Stop()
        {
            Log.Out("[SERVERTOOLS] Running shutdown.");
            SdtdConsole.Instance.ExecuteSync("shutdown", (ClientInfo)null);
        }

        public static void Kick30()
        {
            if (!Kick_Login)
            {
                NoEntry = true;
            }
            string _phrase453;
            if (!Phrases.Dict.TryGetValue(453, out _phrase453))
            {
                _phrase453 = "Shutdown is in 30 seconds. Please come back after the server restarts.";
            }
            SdtdConsole.Instance.ExecuteSync(string.Format("kickall \"{0}\"", _phrase453), (ClientInfo)null);
        }
    }
}
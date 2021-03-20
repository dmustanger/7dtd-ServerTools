using System;
using System.Collections.Generic;
using ServerTools.AntiCheat;

namespace ServerTools
{
    class GodmodeDetectorConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable godmode detector.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. GodmodeDetector off\n" +
                   "  2. GodmodeDetector on\n" +
                   "1. Turn off godmode detector\n" +
                   "2. Turn on godmode detector\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-GodmodeDetector", "gd", "st-gd" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (PlayerChecks.GodEnabled)
                    {
                        PlayerChecks.GodEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Godmode detector has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Godmode detector is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!PlayerChecks.GodEnabled)
                    {
                        PlayerChecks.GodEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Godmode detector has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Godmode detector is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNoticeConsole.Execute: {0}", e.Message));
            }
        }
    }
}
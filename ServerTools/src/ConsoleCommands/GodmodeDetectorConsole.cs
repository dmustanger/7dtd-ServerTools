using System;
using System.Collections.Generic;

namespace ServerTools
{
    class GodmodeDetectorConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable or disable godmode detector.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-gd off\n" +
                   "  2. st-gd on\n" +
                   "1. Turn off godmode detector\n" +
                   "2. Turn on godmode detector\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-GodmodeDetector", "gd", "st-gd" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (GodMode.IsEnabled)
                    {
                        GodMode.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Godmode detector has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Godmode detector is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!GodMode.IsEnabled)
                    {
                        GodMode.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Godmode detector has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Godmode detector is already on"));
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNoticeConsole.Execute: {0}", e.Message));
            }
        }
    }
}
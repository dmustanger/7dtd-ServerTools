using System;
using System.Collections.Generic;

namespace ServerTools
{
    class HordesConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable hordes";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-hrd off\n" +
                   "  2. st-hrd on\n" +
                   "1. Turn off extra hordes\n" +
                   "2. Turn on extra hordes\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Hordes", "hrd", "st-hrd" };
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
                    if (Hordes.IsEnabled)
                    {
                        Hordes.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Hordes has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Hordes is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Hordes.IsEnabled)
                    {
                        Hordes.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Hordes has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Hordes is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in HordesConsole.Execute: {0}", e.Message));
            }
        }
    }
}
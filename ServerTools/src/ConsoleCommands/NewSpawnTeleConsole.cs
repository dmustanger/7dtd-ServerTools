using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NewSpawnTeleConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or disable new spawn tele.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-nst off\n" +
                   "  2. st-nst on\n" +
                   "1. Turn off new spawn tele\n" +
                   "2. Turn on new spawn tele\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-NewSpawnTele", "nst", "st-nst" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (NewSpawnTele.IsEnabled)
                    {
                        NewSpawnTele.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] New spawn tele has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] New spawn tele is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!NewSpawnTele.IsEnabled)
                    {
                        NewSpawnTele.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] New spawn tele has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] New spawn tele is already on"));
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in NewSpawnTeleConsole.Execute: {0}", e.Message));
            }
        }
    }
}
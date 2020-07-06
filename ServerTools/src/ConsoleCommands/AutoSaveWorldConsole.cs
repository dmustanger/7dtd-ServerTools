using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AutoSaveWorldConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable auto save world.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. asw off\n" +
                   "  2. asw on\n" +
                   "1. Turn off the world auto save\n" +
                   "2. Turn on the world auto save\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-AutoSaveWorld", "asw", "st-asw" };
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
                else if (_params[0].ToLower().Equals("off"))
                {
                    if (AutoSaveWorld.IsEnabled)
                    {
                        AutoSaveWorld.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Auto save world has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Auto save world is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!AutoSaveWorld.IsEnabled)
                    {
                        AutoSaveWorld.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Auto save world has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Auto save world is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoSaveWorldConsole.Execute: {0}", e));
            }
        }
    }
}
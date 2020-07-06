using System;
using System.Collections.Generic;
using ServerTools.AntiCheat;

namespace ServerTools
{
    class WorldRadiusConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable world radius.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. WorldRadius off\n" +
                   "  2. WorldRadius on\n" +
                   "  3. WorldRadius normal <#>\n" +
                   "  4. WorldRadius reserved <#>\n" +
                   "1. Turn off world radius\n" +
                   "2. Turn on world radius\n" +
                   "3. Set the radius for normal players\n" +
                   "4. Set the radius for reserved players\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-WorldRadius", "wr", "st-wr" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (WorldRadius.IsEnabled)
                    {
                        WorldRadius.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("World radius has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("World radius is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!WorldRadius.IsEnabled)
                    {
                        WorldRadius.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("World radius has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("World radius is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("normal"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    int _radius = WorldRadius.Normal_Player;
                    if (int.TryParse(_params[1], out _radius))
                    {
                        WorldRadius.Normal_Player = _radius;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("World radius for normal players has been set to {0}", _radius));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reserved"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    int _radius = WorldRadius.Reserved;
                    if (int.TryParse(_params[1], out _radius))
                    {
                        WorldRadius.Reserved = _radius;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("World radius for reserved players has been set to {0}", _radius));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}", _params[1]));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WorldRadiusConsole.Execute: {0}", e.Message));
            }
        }
    }
}
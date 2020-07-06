using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SetHomeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable set home.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Home off\n" +
                   "  2. Home on\n" +
                   "  3. Home reset <steamId/entityId>\n" +
                   "1. Turn off set home\n" +
                   "2. Turn on set home\n" +
                   "3. Reset the delay of the player's /home1 and /home2 command delays\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Home", "home", "st-home" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (TeleportHome.IsEnabled)
                    {
                        TeleportHome.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Set home has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Set home is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!TeleportHome.IsEnabled)
                    {
                        TeleportHome.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Set home has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Set home is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not reset Id: Invalid Id {0}", _params[1]));
                        return;
                    }
                    PersistentPlayer p = PersistentContainer.Instance.Players[_params[1]];
                    if (p != null)
                    {
                        PersistentContainer.Instance.Players[_params[1]].LastHome1 = DateTime.Now.AddYears(-1);
                        PersistentContainer.Instance.Players[_params[1]].LastHome2 = DateTime.Now.AddYears(-1);
                        PersistentContainer.Instance.Save();
                        SdtdConsole.Instance.Output(string.Format("Home delay reset for {0}.", _params[1]));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a Home delay to reset.", _params[1]));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SetHomeConsole.Execute: {0}", e.Message));
            }
        }
    }
}
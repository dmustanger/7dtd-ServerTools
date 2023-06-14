using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BountiesConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable or disable bounties. Alter, remove or show a list of all current bounties";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-bty off\n" +
                   "  2. st-bty on\n" +
                   "  3. st-bty edit {EOS/Id/PlayerName} {Value}\n" +
                   "  4. st-bty remove {EOS/Id/PlayerName}\n" +
                   "  5. st-bty list\n" +
                   "1. Turn off bounties\n" +
                   "2. Turn on bounties\n" +
                   "3. Edit the bounty of a player\n" +
                   "4. Removed the bount on a player\n" +
                   "5. Show a list of all current bounties\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-Bounty", "bty", "st-bty" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 && _params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Bounties.IsEnabled)
                    {
                        Bounties.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Bounties has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Bounties is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Bounties.IsEnabled)
                    {
                        Bounties.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Bounties has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Bounties is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("edit"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found {0}", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[2], out int _value))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Must input a valid interger: {0}", _params[2]));
                        return;
                    }
                    ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (cInfo != null)
                    {
                        Bounties.ConsoleEdit(cInfo.CrossplatformId.CombinedString, _value);
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No player found online with id or name: {0}. Checking id", _params[1]));
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not edit: Invalid steam id {0}", _params[1]));
                            return;
                        }
                        Bounties.ConsoleEdit(_params[1], _value);
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        Bounties.ConsoleRemoveBounty(_cInfo.CrossplatformId.CombinedString);
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No player found online with id or name: {0}. Checking steam id", _params[1]));
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove: Invalid steam id {0}", _params[1]));
                            return;
                        }
                        Bounties.ConsoleRemoveBounty(_params[1]);
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    Bounties.ConsoleBountyList();
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BountiesConsole.Execute: {0}", e.Message));
            }
        }
    }
}
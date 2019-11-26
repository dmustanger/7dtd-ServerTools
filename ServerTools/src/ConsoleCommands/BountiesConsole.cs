using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class BountiesConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Bounties. Alter, remove or show a list of all current bounties";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Bounties off\n" +
                   "  2. Bounties on\n" +
                   "  3. Bounties edit {SteamId/EntityId/PlayerName} {Value}\n" +
                   "  4. Bounties remove {SteamId/EntityId/PlayerName}\n" +
                   "  5. Bounties list\n" +
                   "1. Turn off bounties\n" +
                   "2. Turn on bounties\n" +
                   "3. Edit the bounty of a player\n" +
                   "4. Removed the bount on a player\n" +
                   "5. Show a list of all current bounties\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Bounties", "bounties", "st-bounty", "bounty" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 && _params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    Bounties.IsEnabled = false;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Bounties has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    Bounties.IsEnabled = true;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Bounties has been set to on"));
                    return;
                }
                else if (_params[0].ToLower().Equals("edit"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 3, found {0}", _params.Count));
                        return;
                    }

                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }

                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in BountiesConsole.Run: {0}.", e));
            }
        }
    }
}
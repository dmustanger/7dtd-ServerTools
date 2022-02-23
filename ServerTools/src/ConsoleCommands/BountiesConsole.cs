using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BountiesConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable bounties. Alter, remove or show a list of all current bounties";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-bty off\n" +
                   "  2. st-bty on\n" +
                   "  3. st-bty edit {Id/EOS/EntityId/PlayerName} {Value}\n" +
                   "  4. st-bty remove {Id/EOS/EntityId/PlayerName}\n" +
                   "  5. st-bty list\n" +
                   "1. Turn off bounties\n" +
                   "2. Turn on bounties\n" +
                   "3. Edit the bounty of a player\n" +
                   "4. Removed the bount on a player\n" +
                   "5. Show a list of all current bounties\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Bounty", "bty", "st-bty" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 && _params.Count > 3)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Bounties.IsEnabled)
                    {
                        Bounties.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Bounties has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Bounties is already off"));
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
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Bounties has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Bounties is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("edit"))
                {
                    if (_params.Count != 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found {0}", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[2], out int _value))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Must input a valid interger: {0}", _params[2]));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        Bounties.ConsoleEdit(_cInfo.PlatformId.ReadablePlatformUserIdentifier, _value);
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] No player found online with id or name: {0}. Checking steam id", _params[1]));
                        if (_params[1].Length != 17)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not edit: Invalid steam id {0}", _params[1]));
                            return;
                        }
                        Bounties.ConsoleEdit(_params[1], _value);
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        Bounties.ConsoleRemoveBounty(_cInfo.PlatformId.ReadablePlatformUserIdentifier);
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] No player found online with id or name: {0}. Checking steam id", _params[1]));
                        if (_params[1].Length != 17)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not remove: Invalid steam id {0}", _params[1]));
                            return;
                        }
                        Bounties.ConsoleRemoveBounty(_params[1]);
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    Bounties.ConsoleBountyList();
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BountiesConsole.Execute: {0}", e.Message));
            }
        }
    }
}
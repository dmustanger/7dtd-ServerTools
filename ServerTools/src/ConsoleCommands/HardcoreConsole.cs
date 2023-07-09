using System;
using System.Collections.Generic;

namespace ServerTools
{
    class HardcoreConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable, disable or remove hardcore status.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-hc off\n" +
                   "  2. st-hc on\n" +
                   "  3. st-hc remove <Id/EntityId/PlayerName>\n" +
                   "1. Turn off hardcore\n" +
                   "2. Turn on hardcore\n" +
                   "3. Remove hardcore status from a player while set to optional\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-Hardcore", "hc", "st-hc" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 || _params.Count != 2)
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Wrong number of arguments, expected 1 or 2, found {0}", _params.Count);
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count);
                        return;
                    }
                    if (Hardcore.IsEnabled)
                    {
                        Hardcore.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Hardcore has been set to off");
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Hardcore is already off");
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count);
                        return;
                    }
                    if (!Hardcore.IsEnabled)
                    {
                        Hardcore.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Hardcore has been set to on");
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Hardcore is already on");
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count);
                        return;
                    }
                    if (!Hardcore.Optional)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Hardcore tool is not set for optional mode. Unable to remove status from player");
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].HardcoreEnabled)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].HardcoreEnabled = false;
                            PersistentContainer.DataChange = true;
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Hardcore status removed from player '{0}'", _params[1]);
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Hardcore status could not be removed from player '{0}'. They do not have it enabled", _params[1]);
                            return;
                        }
                    }
                    else
                    {
                        List<string> id = PersistentContainer.Instance.Players.IDs;
                        for (int i = 0; i < id.Count; i++)
                        {
                            if (PersistentContainer.Instance.Players[id[i]].PlayerName == _params[1])
                            {
                                if (PersistentContainer.Instance.Players[id[i]].HardcoreEnabled)
                                {
                                    PersistentContainer.Instance.Players[id[i]].HardcoreEnabled = false;
                                    PersistentContainer.DataChange = true;
                                    SdtdConsole.Instance.Output("[SERVERTOOLS] Hardcore status removed from player '{0}'", _params[1]);
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output("[SERVERTOOLS] Hardcore status could not be removed from player '{0}'. They are not enabled", _params[1]);
                                    return;
                                }
                            }
                        }
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Player '{0}' was not found online", _params[1]);
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid argument {0}.", _params[0]);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in HardcoreConsole.Execute: {0}", e.Message);
                return;
            }
        }
    }
}
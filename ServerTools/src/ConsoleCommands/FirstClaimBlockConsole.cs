using System;
using System.Collections.Generic;

namespace ServerTools
{
    class FirstClaimBlockConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable, reset first claim block.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. FirstClaimBlock off\n" +
                   "  2. FirstClaimBlock on\n" +
                   "  3. Firstclaimblock reset <steamId/entityId>\n" +
                   "1. Turn off first claim block\n" +
                   "2. Turn on first claim block\n" +
                   "3. Reset the status of a player's first claim block\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-FirstClaimBlock", "fcb", "st-fcb" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (FirstClaimBlock.IsEnabled)
                    {
                        FirstClaimBlock.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("First claim block has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("First claim block is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!FirstClaimBlock.IsEnabled)
                    {
                        FirstClaimBlock.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("First claim block has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("First claim block is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params[1].ToLower().Equals("all"))
                    {
                        for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
                        {
                            string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                            PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                            {
                                PersistentContainer.Instance.Players[_id].FirstClaimBlock = false;
                            }
                        }
                        PersistentContainer.Instance.Save();
                        SdtdConsole.Instance.Output("First claim block reset for all players.");
                    }
                    else
                    {
                        PersistentPlayer p = PersistentContainer.Instance.Players[_params[1]];
                        if (p != null)
                        {
                            PersistentContainer.Instance.Players[_params[1]].FirstClaimBlock = false;
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("First claim block reset for {0}.", _params[1]));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not reset player. Invalid Id {0}.", _params[1]));
                            return;
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in FirstClaimBlockConsole.Execute: {0}", e.Message));
            }
        }
    }
}
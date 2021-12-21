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
                   "  1. st-fcb off\n" +
                   "  2. st-fcb on\n" +
                   "  3. st-fcb reset <EOS/EntityId/PlayerName>\n" +
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
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 2, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (FirstClaimBlock.IsEnabled)
                    {
                        FirstClaimBlock.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] First claim block has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] First claim block is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!FirstClaimBlock.IsEnabled)
                    {
                        FirstClaimBlock.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] First claim block has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] First claim block is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params[1].ToLower().Equals("all"))
                    {
                        for (int i = 0; i < PersistentContainer.Instance.Players.IDs.Count; i++)
                        {
                            string id = PersistentContainer.Instance.Players.IDs[i];
                            PersistentContainer.Instance.Players[id].FirstClaimBlock = false;
                            PersistentContainer.DataChange = true;
                        }
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] First claim block reset for all players");
                    }
                    else if (PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        if (PersistentContainer.Instance.Players[_params[1]].FirstClaimBlock)
                        {
                            PersistentContainer.Instance.Players[_params[1]].FirstClaimBlock = false;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] First claim block reset for '{0}'", _params[1]));
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Reset is unnecessary for '{0}'", _params[1]));
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not reset player's first claim block. No saved data could be located for '{0}'", _params[1]));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in FirstClaimBlockConsole.Execute: {0}", e.Message));
            }
        }
    }
}
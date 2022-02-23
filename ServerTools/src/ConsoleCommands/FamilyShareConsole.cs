using System;
using System.Collections.Generic;

namespace ServerTools
{
    class FamilyShareConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable family share or add, remove permission";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-fs off\n" +
                   "  2. st-fs on\n" +
                   "  3. st-fs add <SteamId>\n" +
                   "  4. st-fs remove <SteamId>\n" +
                   "  5. st-fs list\n" +
                   "1. Turn off family share\n" +
                   "2. Turn on family share\n" +
                   "3. Add a steam id to the family share list\n" +
                   "4. Remove a steam id from the family share list\n" +
                   "5. Shows the steam id on the family share list\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-FamilyShare", "fs", "st-fs" };
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
                    if (FamilyShare.IsEnabled)
                    {
                        FamilyShare.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Family share has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Family share is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!FamilyShare.IsEnabled)
                    {
                        FamilyShare.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Family share has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Family share is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (!_params[0].Contains("Steam_"))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid steam id '{0}'", _params[0]));
                        return;
                    }
                    else if (PersistentContainer.Instance.Players.Players.ContainsKey(_params[0]) && PersistentContainer.Instance.Players[_params[0]].FamilyShareImmune)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Steam id '{0}' is already on the family share list", _params[0]));
                        return;
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_params[0]].FamilyShareImmune = true;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Steam id '{0}' has been added to the family share list", _params[0]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (!_params[0].Contains("Steam_"))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid steam id '{0}'", _params[0]));
                        return;
                    }
                    else if (PersistentContainer.Instance.Players.Players.ContainsKey(_params[0]) && PersistentContainer.Instance.Players[_params[0]].FamilyShareImmune)
                    {
                        PersistentContainer.Instance.Players[_params[0]].FamilyShareImmune = false;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Steam id '{0}' has been removed from the family share list", _params[0]));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Steam id '{0}' is not on the family share list", _params[0]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (PersistentContainer.Instance.Players.IDs.Count > 0)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Family share list:");
                        for (int i = 0; i < PersistentContainer.Instance.Players.IDs.Count; i++)
                        {
                            string id = PersistentContainer.Instance.Players.IDs[i];
                            if (PersistentContainer.Instance.Players[id].FamilyShareImmune)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}'", id));
                            }
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Family share list is empty");
                    }
                    return;
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in FamilyShareConsole.Execute: {0}", e.Message));
            }
        }
    }
}

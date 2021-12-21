using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class CommandWatchListConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, add, remove and view steam ids on the watchlist.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-wl off\n" +
                   "  2. st-wl on\n" +
                   "  3. st-wl add <EOS> <Reason>\n" +
                   "  4. st-wl remove <EOS>\n" +
                   "  5. st-wl list\n" +
                   "1. Turn off watch list\n" +
                   "2. Turn on watch list\n" +
                   "3. Adds an Id to the watch list\n" +
                   "4. Removes an Id from the watch list\n" +
                   "5. Lists all Id on the watch list";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-WatchList", "wl", "st-wl" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (WatchList.IsEnabled)
                    {
                        WatchList.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Watch list has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Watch list is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!WatchList.IsEnabled)
                    {
                        WatchList.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Watch list has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Watch list is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found '{0}'", _params.Count));
                        return;
                    }
                    if (_params[1].Contains("_"))
                    {
                        if (WatchList.Dict.ContainsKey(_params[1]))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not add '{0}'. Id is already in the watchlist", _params[1]));
                            return;
                        }
                        WatchList.Dict.Add(_params[1], _params[2]);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added Id '{0}' with the reason '{1}' to the watchlist", _params[1], _params[2]));
                        WatchList.UpdateXml();
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid Id '{0}'", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    if (_params[1].Contains("_"))
                    {
                        if (!WatchList.Dict.ContainsKey(_params[1]))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' was not found on the list", _params[1]));
                            return;
                        }
                        WatchList.Dict.Remove(_params[1]);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed Id '{0}' from the watchlist", _params[1]));
                        WatchList.UpdateXml();
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid Id '{0}'", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (WatchList.Dict.Count < 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There are no Id on the watchlist");
                        return;
                    }
                    foreach (KeyValuePair<string, string> _key in WatchList.Dict)
                    {
                        string _output = string.Format("{0} {1}", _key.Key, _key.Value);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(_output);
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WatchlistConsole.Execute: {0}", e.Message));
            }
        }
    }
}
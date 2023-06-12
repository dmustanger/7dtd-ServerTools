using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BadWordFilterConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable, disable, edit the bad word filter.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-bwf off\n" +
                   "  2. st-bwf on\n" +
                   "  3. st-bwf add <word>\n" +
                   "  4. st-bwf remove <word>\n" +
                   "  5. st-bwf list\n" +
                   "1. Turn off the bad word filter\n" +
                   "2. Turn on the bad word filter\n" +
                   "3. Add a word to the list\n" +
                   "4. Remove a word from the list\n" +
                   "5. Shows a list of the current bad word filters\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-BadWordFilter", "bwf", "st-bwf" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    if (Badwords.IsEnabled)
                    {
                        Badwords.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Bad word filter has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Bad word filter is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    if (!Badwords.IsEnabled)
                    {
                        Badwords.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Bad word filter has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Bad word filter is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count < 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected more than 2, found {0}", _params.Count));
                        return;
                    }
                    _params.RemoveAt(0);
                    string _word = _params.ToString().ToLower();
                    if (Badwords.Dict.Contains(_word))
                    {
                        Badwords.Dict.Add(_word);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added bad word to the list: {0}", _word));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Could not add entry. Bad word already found");
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count < 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected more than 2, found {0}", _params.Count));
                        return;
                    }
                    _params.RemoveAt(0);
                    string _word = _params.ToString().ToLower();
                    if (Badwords.Dict.Contains(_word))
                    {
                        Badwords.Dict.Remove(_word);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed bad word from the list: {0}", _word));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Could not remove entry. Bad word not found");
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    if (Badwords.Dict.Count > 0)
                    {
                        for (int i = 0; i < Badwords.Dict.Count; i++)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Bad word: {0}", Badwords.Dict[i]));
                        }
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] No entries were found on the bad word filter list");
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BadWordFilterConsole.Execute: {0}", e.Message));
            }
        }
    }
}
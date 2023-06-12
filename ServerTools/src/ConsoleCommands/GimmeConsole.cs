using System;
using System.Collections.Generic;

namespace ServerTools
{
    class GimmeConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable, disable, reset gimme.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-gim off\n" +
                   "  2. st-gim on\n" +
                   "  3. st-gim reset all\n" +
                   "  3. st-gim reset <EOS/entityId/playerName>\n" +
                   "1. Turn off gimme\n" +
                   "2. Turn on gimme\n" +
                   "3. Reset the delay of all players\n" +
                   "4. Reset the delay of a player\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-Gimme", "gim", "st-gim" };
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
                    if (Gimme.IsEnabled)
                    {
                        Gimme.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Gimme has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Gimme is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Gimme.IsEnabled)
                    {
                        Gimme.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Gimme has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Gimme is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params[1].ToLower().Equals("all"))
                    {
                        if (PersistentContainer.Instance.Players.IDs.Count > 0)
                        {
                            for (int i = 0; i < PersistentContainer.Instance.Players.IDs.Count; i++)
                            {
                                string id = PersistentContainer.Instance.Players.IDs[i];
                                PersistentPlayer p = PersistentContainer.Instance.Players[id];
                                {
                                    PersistentContainer.Instance.Players[id].LastGimme = DateTime.Now.AddYears(-1);
                                }
                            }
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Gimme delay reset for all players");
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] No players to reset");
                        }
                    }
                    else
                    {
                        if (PersistentContainer.Instance.Players[_params[1]] != null)
                        {
                            PersistentContainer.Instance.Players[_params[1]].LastGimme = DateTime.Now.AddYears(-1);
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Gimme tool delay has been reset for '{0}'", _params[1]));
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not reset the delay on Gimme tool. Invalid id '{0}'", _params[1]));
                            return;
                        }
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GimmeConsole.Execute: {0}", e.Message));
            }
        }
    }
}
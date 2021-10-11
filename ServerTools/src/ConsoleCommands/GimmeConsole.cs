using System;
using System.Collections.Generic;

namespace ServerTools
{
    class GimmeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable, reset gimme.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-gim off\n" +
                   "  2. st-gim on\n" +
                   "  3. st-gim reset all\n" +
                   "  3. st-gim reset <steamId/entityId/playerName>\n" +
                   "1. Turn off gimme\n" +
                   "2. Turn on gimme\n" +
                   "3. Reset the delay of all players\n" +
                   "4. Reset the delay of a player\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Gimme", "gim", "st-gim" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Gimme.IsEnabled)
                    {
                        Gimme.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Gimme has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Gimme is already off"));
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
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Gimme has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Gimme is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params[1].ToLower().Equals("all"))
                    {
                        if (PersistentContainer.Instance.Players.SteamIDs.Count > 0)
                        {
                            for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
                            {
                                string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                                PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                                {
                                    PersistentContainer.Instance.Players[_id].LastGimme = DateTime.Now.AddYears(-1);
                                }
                            }
                            PersistentContainer.DataChange = true;
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Gimme delay reset for all players.");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] No players to reset.");
                        }
                    }
                    else
                    {
                        if (PersistentContainer.Instance != null)
                        {
                            PersistentPlayer p = PersistentContainer.Instance.Players[_params[1]];
                            if (p != null)
                            {
                                PersistentContainer.Instance.Players[_params[1]].LastGimme = DateTime.Now.AddYears(-1);
                                PersistentContainer.DataChange = true;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Gimme delay reset for {0}.", _params[1]));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not reset player. Invalid Id {0}.", _params[1]));
                                return;
                            }
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GimmeConsole.Execute: {0}", e.Message));
            }
        }
    }
}
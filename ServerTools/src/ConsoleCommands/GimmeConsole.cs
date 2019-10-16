using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

namespace ServerTools
{
    class GimmeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable, Disable, Reset Gimme.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Gimme off\n" +
                   "  2. Gimme on\n" +
                   "  3. Gimme reset all\n" +
                   "  3. Gimme reset <steamId/entityId/playerName>\n" +
                   "1. Turn off gimme\n" +
                   "2. Turn on gimme\n" +
                   "3. Reset the delay of all players\n" +
                   "4. Reset the delay of a player\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Gimme", "gimme" };
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
                    Gimme.IsEnabled = false;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Gimme has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Gimme.IsEnabled = true;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Gimme has been set to on"));
                    return;
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
                                PersistentContainer.Instance.Players[_id].LastGimme = DateTime.Now.AddYears(-1);
                            }
                        }
                        PersistentContainer.Instance.Save();
                        SdtdConsole.Instance.Output("Gimme delay reset for all players.");
                    }
                    else
                    {
                        PersistentPlayer p = PersistentContainer.Instance.Players[_params[1]];
                        if (p != null)
                        {
                            PersistentContainer.Instance.Players[_params[1]].LastGimme = DateTime.Now.AddYears(-1);
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("Gimme delay reset for {0}.", _params[1]));
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
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GimmeConsole.Run: {0}.", e));
            }
        }
    }
}
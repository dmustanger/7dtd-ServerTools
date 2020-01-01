using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

namespace ServerTools
{
    class AnimalTrackingConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Animal Tracking.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. AnimalTracking off\n" +
                   "  2. AnimalTracking on\n" +
                   "  3. AnimalTracking reset <steamId/entityId/playerName>\n" +
                   "1. Turn off animal tracking\n" +
                   "2. Turn on animal tracking\n" +
                   "3. Reset the delay status of a player for the animal tracking command\n";

        }
        public override string[] GetCommands()
        {
            return new string[] { "st-AnimalTracking", "animaltracking" };
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
                    if (Animals.IsEnabled)
                    {
                        Animals.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Animal tracking has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Animal tracking is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Animals.IsEnabled)
                    {
                        Animals.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Animal tracking has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Animal tracking is already off"));
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
                                PersistentContainer.Instance.Players[_id].LastAnimal = DateTime.Now.AddYears(-1);
                            }
                        }
                        PersistentContainer.Instance.Save();
                        SdtdConsole.Instance.Output("Animal tracking delay reset for all players.");
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                        if (_cInfo != null)
                        {
                            PersistentContainer.Instance.Players[_params[1]].LastAnimal = DateTime.Now.AddYears(-1);
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("Animal tracking delay reset for {0}.", _cInfo.playerName));
                        }
                        else
                        {
                            if (_params[1].Length != 17)
                            {
                                SdtdConsole.Instance.Output(string.Format("Can not reset Id: Invalid Id {0}", _params[1]));
                                return;
                            }
                            PersistentContainer.Instance.Players[_params[1]].LastAnimal = DateTime.Now.AddYears(-1);
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("Animal tracking delay reset for {0}.", _params[1]));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in AnimalTrackingConsole.Execute: {0}", e));
            }
        }
    }
}
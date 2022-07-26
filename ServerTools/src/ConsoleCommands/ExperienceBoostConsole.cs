using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ExperienceBoostConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Add or remove more experience given to a player upon killing a zombie.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-eb add {EOS/ID/Name} {Amount}\n" +
                   "  2. st-eb remove {EOS/ID/Name}\n" +
                   "  3. st-eb show {EOS/ID/Name}\n" +
                   "1. Add an experience boost to a player when they kill a zombie\n" +
                   "2. Remove the experience boost from a player when they kill a zombie\n" +
                   "3. Shows the current experience boost for a player\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ExperienceBoost", "eb", "st-eb" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 3)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (!int.TryParse(_params[2], out int value))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: '{0}'", _params[2]));
                        return;
                    }
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost += value;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Experience boost for '{0}' named '{1}' has increased by '{2}'. Total: '{3}'", _params[1], cInfo.playerName, value, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost));
                        }
                    }
                    else if (PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost += value;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Experience boost for '{0}' named '{1}' has increased by '{2}'. Total: '{3}'", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName, value, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost));
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found", _params[1]));
                    }
                    return;
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (!int.TryParse(_params[2], out int value))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: '{0}'", _params[2]));
                        return;
                    }
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost -= value;
                            if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost < 0)
                            {
                                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost = 0;
                            }
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Experience boost for '{0}' named '{1}' has decreased by '{2}'. Total: '{3}'", _params[1], cInfo.playerName, value, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost));
                        }
                    }
                    else if (PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost -= value;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Experience boost for '{0}' named '{1}' has increased by '{2}'. Total: '{3}'", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName, value, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost));
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found", _params[1]));
                    }
                    return;
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ExperienceBoostConsole.Execute: {0}", e.Message));
            }
        }
    }
}

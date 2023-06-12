using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ExperienceBoostConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools]- Add or remove more experience given to a player upon killing a zombie.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-eb add {EOS/EntityId/Name} {Amount}\n" +
                   "  2. st-eb remove {EOS/EntityId/Name} {Amount}\n" +
                   "  3. st-eb show {EOS/EntityId/Name}\n" +
                   "1. Add an experience boost to a player when they kill a zombie\n" +
                   "2. Remove the experience boost from a player when they kill a zombie\n" +
                   "3. Shows the current experience boost for a player\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-ExperienceBoost", "eb", "st-eb" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, 3, found '{0}'", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[2], out int value))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: '{0}'", _params[2]));
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
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
                        PersistentContainer.Instance.Players[_params[1]].ExperienceBoost += value;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Experience boost for '{0}' named '{1}' has increased by '{2}'. Total: '{3}'", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName, value, PersistentContainer.Instance.Players[_params[1]].ExperienceBoost));
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found", _params[1]));
                    }
                    return;
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, 3, found '{0}'", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[2], out int value))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: '{0}'", _params[2]));
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
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
                        PersistentContainer.Instance.Players[_params[1]].ExperienceBoost -= value;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Experience boost for '{0}' named '{1}' has increased by '{2}'. Total: '{3}'", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName, value, PersistentContainer.Instance.Players[_params[1]].ExperienceBoost));
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found", _params[1]));
                    }
                    return;
                }
                else if (_params[0].ToLower().Equals("show"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, 2, found '{0}'", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Experience boost for '{0}' named '{1}' is set for '{2}'", _params[1], cInfo.playerName, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost));
                        }
                    }
                    else if (PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Experience boost for '{0}' named '{1}' is set for '{2}'", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName, PersistentContainer.Instance.Players[_params[1]].ExperienceBoost));
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found", _params[1]));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in ExperienceBoostConsole.Execute: {0}", e.Message));
            }
        }
    }
}

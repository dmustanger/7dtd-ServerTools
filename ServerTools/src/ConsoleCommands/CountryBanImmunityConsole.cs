using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class CountryBanImmunityConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Add, remove and view player on the country ban immunity list";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-cbi add <EOS/EntityId/PlayerName>\n" +
                   "  2. st-cbi remove <EOS/EntityId/PlayerName>\n" +
                   "  3. st-cbi list\n" +
                   "1. Adds a player to the country ban immunity list\n" +
                   "2. Removes a player from the country ban immunity list\n" +
                   "3. Shows all entry on the country ban immunity list" +
                   "4. *Note* You can use the entity id, player name or eos id if they are online, otherwise use their eos id";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-CountryBanImmunity", "cbi", "st-cbi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].CountryBanImmune = true;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added id '{0}' to the country ban immunity list", cInfo.CrossplatformId.CombinedString));
                        return;
                    }
                    else if (_params[1].Contains("EOS_"))
                    {
                        PersistentContainer.Instance.Players[_params[1]].CountryBanImmune = true;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added id '{0}' to the country ban immunity list", _params[1]));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid id or player name '{0}'", _params[1]));
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
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].CountryBanImmune)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].CountryBanImmune = false;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed id '{0}' from the country ban immunity list", _params[1]));
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' is not on the country ban immunity list. Unable to remove", _params[1]));
                        }
                    }
                    else if (_params[1].Contains("EOS_"))
                    {
                        if (PersistentContainer.Instance.Players[_params[1]].CountryBanImmune)
                        {
                            PersistentContainer.Instance.Players[_params[1]].CountryBanImmune = false;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed id '{0}' from the country ban immunity list", _params[1]));
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' is not on the country ban immunity list. Unable to remove", _params[1]));
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid id or player name '{0}'", _params[1]));
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
                    List<string> persistentPlayers = PersistentContainer.Instance.Players.IDs;
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Country Ban Immune List:"));
                    for (int i = 0; i < persistentPlayers.Count; i++)
                    {
                        string persistentPlayer = persistentPlayers[i];
                        if (PersistentContainer.Instance.Players[persistentPlayer].CountryBanImmune)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Id '{0}'", persistentPlayer));
                        }
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CountryBanImmunityConsole.Execute: {0}", e.Message));
            }
        }
    }
}

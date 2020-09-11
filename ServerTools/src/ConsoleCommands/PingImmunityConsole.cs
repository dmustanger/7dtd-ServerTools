using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class PingImmunityConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Add, remove and view steam ids on the ping immunity list.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. pingimmunity add <steamId/entityId/playerName>\n" +
                   "  2. pingimmunity remove <steamId/entityId/playerName>\n" +
                   "  3. pingimmunity list\n" +
                   "1. Adds a steam ID to the Ping Immunity list\n" +
                   "2. Removes a steam ID from the Ping Immunity list\n" +
                   "3. Lists all steam IDs that have Ping Immunity" +
                   "4. *Note* You can use the player id or name if they are online otherwise use their steam Id";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-PingImmunity", "pi", "st-pi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (HighPingKicker.Dict.ContainsKey(_cInfo.playerId))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id. {0} is already in the Ping Immunity list", _params[1]));
                        }
                        else
                        {
                            HighPingKicker.Dict.Add(_cInfo.playerId, _cInfo.playerName);
                            HighPingKicker.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added Id {0} to the Ping Immunity list", _params[1]));
                        }
                    }
                    else
                    {
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player must be online or when offline use their steam Id which must be 17 numbers in length. Can not add Id: {0}", _params[1]));
                            return;
                        }
                        else
                        {
                            PersistentPlayerData _ppd = PersistentOperations.GetPersistentPlayerDataFromSteamId(_params[1]);
                            if (_ppd != null)
                            {
                                if (HighPingKicker.Dict.ContainsKey(_ppd.PlayerId))
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id. {0} is already in the Ping Immunity list", _ppd.PlayerId));
                                }
                                else
                                {
                                    PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_ppd.PlayerId);
                                    if (_pdf != null)
                                    {
                                        HighPingKicker.Dict.Add(_ppd.PlayerId, _pdf.ecd.entityName);
                                        HighPingKicker.UpdateXml();
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added Id {0}, with player name {1} to the Ping Immunity list", _params[1], _pdf.ecd.entityName));
                                    }
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id. {0} could not be found", _params[1]));
                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (!HighPingKicker.Dict.ContainsKey(_cInfo.playerId))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove Id. {0} is not in the Ping Immunity list", _params[1]));
                        }
                        else
                        {
                            HighPingKicker.Dict.Remove(_cInfo.playerId);
                            HighPingKicker.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed Id {0} from the Ping Immunity list", _params[1]));
                        }
                    }
                    else
                    {
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player must be online to use their name or entity id. Otherwise you must use their steam Id. Can not remove Id: {0}", _params[1]));
                            return;
                        }
                        else
                        {
                            if (!HighPingKicker.Dict.ContainsKey(_params[1]))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove Id. {0} is not in the Ping Immunity list", _params[1]));
                            }
                            else
                            {
                                HighPingKicker.Dict.Remove(_params[1]);
                                HighPingKicker.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed Id {0} from the Ping Immunity list", _params[1]));
                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    else
                    {
                        if (HighPingKicker.Dict.Count == 0)
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] There are no players on the Ping Immunity list.");
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] High Ping List:"));
                            foreach (KeyValuePair<string, string> _c in HighPingKicker.Dict)
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player Id {0}, player name {1}", _c.Key, _c.Value));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PingImmunityCommandConsole.Execute: {0}", e.Message));
            }
        }
    }
}
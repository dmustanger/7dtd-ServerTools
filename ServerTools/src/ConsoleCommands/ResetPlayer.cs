using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    public class ResetPlayer : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Reset a players profile. Warning, can not be undone without a backup.";
        }
        public override string GetHelp()
        {
            return "Usage: resetplayer <steamId/entityId>";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-ResetPlayerProfile", "resetplayerprofile", "rpp" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].Length < 1 || _params[0].Length > 17)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}", _params[0]));
                    return;
                }
                string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _params[0]);
                string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _params[0]);
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (_cInfo != null)
                {
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    if (p != null)
                    {
                        string _phrase400;
                        if (!Phrases.Dict.TryGetValue(400, out _phrase400))
                        {
                            _phrase400 = "Reseting players profile.";
                        }
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase400), _cInfo);
                        if (!File.Exists(_filepath))
                        {
                            SdtdConsole.Instance.Output(string.Format("Could not find file {0}.map", _params[0]));
                        }
                        else
                        {
                            File.Delete(_filepath);
                        }
                        if (!File.Exists(_filepath1))
                        {
                            SdtdConsole.Instance.Output(string.Format("Could not find file {0}.ttp", _params[0]));
                        }
                        else
                        {
                            File.Delete(_filepath1);
                        }
                        string _phrase401;
                        if (!Phrases.Dict.TryGetValue(401, out _phrase401))
                        {
                            _phrase401 = "You have reset the profile for Player {SteamId}.";
                        }
                        _phrase401 = _phrase401.Replace("{SteamId}", _params[0]);
                        SdtdConsole.Instance.Output(string.Format("{0}", _phrase401));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player file {0}.ttp does not exist", _params[0]));
                    }
                }
                else
                {
                    int _counter = 0;
                    string _id = "";
                    if (_params[0].Length == 17)
                    {
                        int _value = 0;
                        if (!int.TryParse(_params[0], out _value))
                        {

                            SdtdConsole.Instance.Output(string.Format("Player id {0} is not a valid interger", _params[0]));
                            return;
                        }
                        else
                        {
                            _id = _value.ToString();
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player id {0} is not a valid steam id", _params[0]));
                        return;
                    }
                    List<string> playerlist = PersistentContainer.Instance.Players.SteamIDs;
                    for (int i = 0; i < playerlist.Count; i++)
                    {
                        string _steamId = playerlist[i];
                        if (_steamId == _id)
                        {
                            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                            if (p != null)
                            {
                                string _phrase400;
                                if (!Phrases.Dict.TryGetValue(400, out _phrase400))
                                {
                                    _phrase400 = "Reseting players profile.";
                                }
                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase400), _cInfo);
                                if (!File.Exists(_filepath))
                                {
                                    SdtdConsole.Instance.Output(string.Format("Could not find file {0}.map", _params[0]));
                                }
                                else
                                {
                                    File.Delete(_filepath);
                                }
                                if (!File.Exists(_filepath1))
                                {
                                    SdtdConsole.Instance.Output(string.Format("Could not find file {0}.ttp", _params[0]));
                                }
                                else
                                {
                                    File.Delete(_filepath1);
                                }
                                string _phrase401;
                                if (!Phrases.Dict.TryGetValue(401, out _phrase401))
                                {
                                    _phrase401 = "You have reset the profile for Player {SteamId}.";
                                }
                                _phrase401 = _phrase401.Replace("{SteamId}", _params[0]);
                                SdtdConsole.Instance.Output(string.Format("{0}", _phrase401));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player file {0}.ttp does not exist", _params[0]));
                            }
                        }
                        else
                        {
                            _counter++;
                            if (_counter == playerlist.Count)
                            {
                                SdtdConsole.Instance.Output(string.Format("Player file {0}.ttp does not exist", _params[0]));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayer.Run: {0}.", e));
            }
        }
    }
}
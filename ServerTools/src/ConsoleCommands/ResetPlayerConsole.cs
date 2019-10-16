using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ServerTools
{
    public class ResetPlayerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Reset A Players Profile. Warning, Can Not Be Undone Without A Backup.";
        }
        public override string GetHelp()
        {
            return "Usage: resetplayerprofile <steamId/entityId>";
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
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (_cInfo != null)
                {
                    string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                    string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                    string _sql = string.Format("SELECT steamid FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count != 0)
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
                    }
                    _result.Dispose();
                    string _phrase401;
                    if (!Phrases.Dict.TryGetValue(401, out _phrase401))
                    {
                        _phrase401 = "You have reset the profile for Player {SteamId}.";
                    }
                    _phrase401 = _phrase401.Replace("{SteamId}", _params[0]);
                    SdtdConsole.Instance.Output(string.Format("{0}", _phrase401));
                }
                else if (_params[0].Length == 17)
                {
                    string _steamid = SQL.EscapeString(_params[0]);
                    string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _steamid);
                    string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _steamid);
                    string _sql = string.Format("SELECT steamid FROM Players WHERE steamid = '{0}'", _steamid);
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count != 0)
                    {
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
                    }
                    _result.Dispose();
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
                    SdtdConsole.Instance.Output(string.Format("Player id {0} is not a valid integer", _params[0]));
                    return;
                }
                
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayerConsole.Run: {0}.", e));
            }
        }
    }
}
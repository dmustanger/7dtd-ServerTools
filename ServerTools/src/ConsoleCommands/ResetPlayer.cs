using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    public class ResetPlayer : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Reset a players profile.";
        }
        public override string GetHelp()
        {
            return "Usage: resetplayer <steamId>";
        }
        public override string[] GetCommands()
        {
            return new string[] { "resetplayer", "rp" };
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
                if (_params[0].Length != 17)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not add SteamId: Invalid SteamId {0}", _params[0]));
                    return;
                }
                string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _params[0]);
                string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _params[0]);
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (ClientInfo _cInfo in _cInfoList)
                {
                    if (_cInfo.playerId == _params[0])
                    {
                        string _phrase400;
                        if (!Phrases.Dict.TryGetValue(400, out _phrase400))
                        {
                            _phrase400 = "Reseting your player profile.";
                        }
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase400), _cInfo);
                        break;
                    }
                }
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
                    _phrase401 = "You have reset the profile for player {SteamId}.";
                }
                _phrase401 = _phrase401.Replace("{SteamId}", _params[0]);
                SdtdConsole.Instance.Output(string.Format("{0}", _phrase401));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayer.Run: {0}.", e));
            }
        }
    }
}
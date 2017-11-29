using System;
using System.Collections.Generic;

namespace ServerTools
{
    class TempBan : ConsoleCmdAbstract
    {
        public static bool IsEnabled = false;
        public static int AdminLevel = 0;
        private static int TimeDefault = 15;

        public override string GetDescription()
        {
            return "Ban a player temporarily.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. tempban <Id> <time>\n" +
                   "1. Ban a player by their steamId for up to 60 minutes\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "tempban", "tb" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (IsEnabled)
            {
                try
                {
                    if (_params.Count < 1 || _params.Count > 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 2, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[0].Length < 1 || _params.Count > 10)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not ban Id: Invalid Id {0}", _params[0]));
                        return;
                    }
                    if (_params[1].Length < 1 || _params.Count > 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not ban PlayerId: expected 1 to 2 digit number, found {0}.", _params[1]));
                        return;
                    }
                    else
                    {
                        int _id = Convert.ToInt32(_params[0]);
                        ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_id);
                        if (_cInfo != null)
                        {
                            GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                            AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                            if (Admin.PermissionLevel > AdminLevel)
                            {
                                int _time = Convert.ToInt32(_params[1]);
                                if (_time > 60 || _params[1] == null)
                                {
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} minutes \"You have been temporarily banned for {2} minutes\"", _cInfo.playerId, TimeDefault, TimeDefault), _cInfo);

                                }
                                else
                                {
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} minutes \"You have been temporarily banned for {2} minutes\"", _cInfo.playerId, _params[1], _params[1]), _cInfo);
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Can not ban Id: {0} Id belongs to an administrator", _cInfo.playerName));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in TempBan.Run: {0}.", e));
                }
            }
            else
            {
                SdtdConsole.Instance.Output(string.Format("TempBan command is off", _senderInfo));
            }
        }
    }
}

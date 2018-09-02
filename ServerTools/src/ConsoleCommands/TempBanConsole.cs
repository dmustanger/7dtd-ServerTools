using System;
using System.Collections.Generic;

namespace ServerTools
{
    class TempBanConsole : ConsoleCmdAbstract
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0;

        public override string GetDescription()
        {
            return "[ServerTools]-Ban a player temporarily.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. tempban <steamId/entityId> <time>\n" +
                   "1. Temporarily ban a player by their Id for up to 60 minutes\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-TempBan", "tempban", "tb" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].Length < 1 || _params.Count > 17)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not ban Id: Invalid Id {0}", _params[0]));
                    return;
                }
                int _time;
                if (!int.TryParse(_params[1], out _time))
                {
                    _time = 15;
                }
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (_cInfo != null)
                {
                    GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                    AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                    if (Admin.PermissionLevel > Admin_Level)
                    {
                        if (_time > 60)
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} minutes \"You have been temporarily banned for {2} minutes\"", _cInfo.playerId, _time, _time), _cInfo);

                        }
                        else
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} minutes \"You have been temporarily banned for {2} minutes\"", _cInfo.playerId, _params[1], _params[1]), _cInfo);
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not ban Id: {0}. The Id belongs to an Admin.", _params[1]));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Player with Id {0} does not exist.", _cInfo.entityId));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TempBanConsole.Run: {0}.", e));
            }
        }
    }
}

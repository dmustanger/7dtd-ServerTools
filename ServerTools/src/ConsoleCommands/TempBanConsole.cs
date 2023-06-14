using System;
using System.Collections.Generic;

namespace ServerTools
{
    class TempBanConsole : ConsoleCmdAbstract
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0;

        protected override string getDescription()
        {
            return "[ServerTools] - Ban a player temporarily.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-TempBan <EOS/EntityId/PlayerName> <Time>\n" +
                   "1. Temporarily ban a player by their Id for up to 60 minutes\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-TempBan", "tb", "st-tb" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 2, found '{0}'", _params.Count));
                    return;
                }
                if (!int.TryParse(_params[1], out int _time))
                {
                    _time = 15;
                }
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                if (cInfo != null)
                {
                    if (GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                        GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                    {
                        if (_time > 60)
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} minutes \"You have been temporarily banned for {2} minutes\"", cInfo.CrossplatformId.CombinedString, _time, _time), cInfo);

                        }
                        else
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} minutes \"You have been temporarily banned for {2} minutes\"", cInfo.CrossplatformId.CombinedString, _params[1], _params[1]), cInfo);
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not ban Id '{0}'. The Id belongs to an Admin", _params[1]));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No Id '{0}' found online", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TempBanConsole.Run: {0}", e.Message));
            }
        }
    }
}

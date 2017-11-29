using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AdminChatColor : ConsoleCmdAbstract
    {
        public static List<string> AdminColorOff = new List<string>();

        public override string GetDescription()
        {
            return "Turn your admin chat color and prefix off or on.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. adminchatcolor off <steamID>\n" +
                   "1. Turn off admin chat color for admin\n" +
                   "  2. adminchatcolor on <steamID>\n" +
                   "2. Turn on admin chat color for admin\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "adminchatcolor", "acc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    else
                    {
                        var _id = _senderInfo.RemoteClientInfo;
                        AdminColorOff.Add(_id.playerId);
                        SdtdConsole.Instance.Output(string.Format("Set admin chat color off for you {0}.", _id.playerName));
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    else
                    {
                        var _id = _senderInfo.RemoteClientInfo;
                        AdminColorOff.Remove(_id.playerId);
                        SdtdConsole.Instance.Output(string.Format("Set admin chat color on for you {0}.", _id.playerName));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AdminChatColor.Run: {0}.", e));
            }
        }
    }
}

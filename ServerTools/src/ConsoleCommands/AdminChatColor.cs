using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AdminChatColor : ConsoleCmdAbstract
    {
        public static List<string> AdminColorOff = new List<string>();

        public override string GetDescription()
        {
            return "[ServerTools]-Turn your admin chat color and prefix off and on.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. adminchatcolor off\n" +
                   "  2. adminchatcolor on\n" +
                   "1. Turn off your admin chat color\n" +                   
                   "2. Turn on your admin chat color\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-AdminChatColor", "adminchatcolor", "acc" };
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
                if (_params[0].Length < 2 || _params[0].Length > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not set Admin Chat Color on or off. Invalid \"on\" or \"off\" {0}", _params[0]));
                    return;
                }
                else if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    else
                    {
                        var _id = _senderInfo.RemoteClientInfo;
                        if (_id != null)
                        {
                            AdminColorOff.Add(_id.playerId);
                        }
                        SdtdConsole.Instance.Output(string.Format("Set your admin chat color to off {0}.", _id.playerName));
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
                        if (_id != null)
                        {
                            AdminColorOff.Remove(_id.playerId);
                        }
                        SdtdConsole.Instance.Output(string.Format("Set your admin chat color to on {0}.", _id.playerName));
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

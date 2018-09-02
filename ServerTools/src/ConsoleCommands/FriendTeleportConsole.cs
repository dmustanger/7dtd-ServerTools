using System;
using System.Collections.Generic;

namespace ServerTools
{
    class FriendTeleportConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Friend Teleport.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. FriendTeleport off\n" +
                   "  2. FriendTeleport on\n" +
                   "1. Turn off friend teleport\n" +
                   "2. Turn on friend teleport\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-FriendTeleport", "friendteleport" };
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
                if (_params[0].ToLower().Equals("off"))
                {
                    FriendTeleport.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Friend teleport has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    FriendTeleport.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Friend teleport has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in FriendTeleportConsole.Run: {0}.", e));
            }
        }
    }
}
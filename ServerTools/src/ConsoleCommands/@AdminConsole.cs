using ServerTools;
using System;
using System.Collections.Generic;
using System.Linq;

public class @AdminsConsole : ConsoleCmdAbstract
{

    public static int Admin_Level = 0; 

    public override string GetDescription()
    {
        return "[ServerTools]-Sends a message to all online admins.";
    }
    public override string GetHelp()
    {
        return "Usage:\n" +
            "  1. @Admin Your message\n" +
            "1. Sends your private chat message to all online admins.\n";
    }
    public override string[] GetCommands()
    {
        return new string[] { "st-@Admin", "@A", "@a" };
    }
    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        try
        {
            string _message = string.Join(" ", _params.ToArray());
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _player = _cInfoList[i];
                if (GameManager.Instance.adminTools.IsAdmin(_player.playerId))
                {
                    _player.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", LoadConfig.Chat_Response_Color, _message), _player.playerName, false, "", false));
                    SdtdConsole.Instance.Output(string.Format("Message sent to {0}.", _player.playerName));
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in @AdminConsole.Run: {0}.", e));
        }
    }
}


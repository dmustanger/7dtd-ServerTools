using ServerTools;
using System;
using System.Collections.Generic;

public class @AdminsConsole : ConsoleCmdAbstract
{

    public static int Admin_Level = 0; 

    public override string GetDescription()
    {
        return "[ServerTools] - Sends a message to all online admins.";
    }

    public override string GetHelp()
    {
        return "Usage:\n" +
            "  1. @A Your message\n" +
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
            List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo = _cInfoList[i];
                if (GameManager.Instance.adminTools.IsAdmin(_cInfo))
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, _cInfo.playerName, EChatType.Whisper, null);
                    SdtdConsole.Instance.Output(string.Format("Message sent to {0}", _cInfo.playerName));
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in @AdminConsole.Execute: {0}", e.Message));
        }
    }
}


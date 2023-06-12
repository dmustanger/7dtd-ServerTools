using ServerTools;
using System;
using System.Collections.Generic;

public class @AdminsConsole : ConsoleCmdAbstract
{

    public static int Admin_Level = 0;

    protected override string getDescription()
    {
        return "[ServerTools] - Sends a message to all online admins.";
    }

    protected override string getHelp()
    {
        return "Usage:\n" +
            "  1. st-@a Your message\n" +
            "1. Sends your private chat message to all online admins.\n";
    }

    protected override string[] getCommands()
    {
        return new string[] { "st-@Admin", "@admin", "st-@a" };
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        try
        {
            string message = string.Join(" ", _params.ToArray());
            List<ClientInfo> cInfoList = GeneralOperations.ClientList();
            if (cInfoList != null)
            {
                for (int i = 0; i < cInfoList.Count; i++)
                {
                    ClientInfo cInfo = cInfoList[i];
                    if (GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo) == 0)
                    {
                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + message + "[-]", -1, cInfo.playerName, EChatType.Whisper, null);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Message sent to {0}", cInfo.playerName));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in @AdminConsole.Execute: {0}", e.Message));
        }
    }
}


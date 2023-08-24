using System;
using System.Collections.Generic;

namespace ServerTools
{
    class StopServerCommandConsole : ConsoleCmdAbstract
    {

        protected override string getDescription()
        {
            return "[ServerTools] - Starts a countdown with an alert system for the time specified and then stops the server";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. st-StopServer <Minutes>\n" +
                "  2. st-StopServer cancel\n" +
                "1. Starts a shutdown process with a countdown for the specified time\n" +
                "2. Cancels the shutdown\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-StopServer", "ss", "st-ss" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0] == "cancel")
                {
                    if (!Shutdown.ShuttingDown)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Stopserver is not running");
                    }
                    else
                    {
                        Shutdown.ShuttingDown = false;
                        Shutdown.NoEntry = false;
                        Shutdown.UI_Locked = false;
                        if (ExitCommand.IsEnabled)
                        {
                            List<ClientInfo> clientList = GeneralOperations.ClientList();
                            if (clientList != null)
                            {
                                ClientInfo cInfo;
                                EntityPlayer player;
                                for (int i = 0; i < clientList.Count; i++)
                                {
                                    cInfo = clientList[i];
                                    if (!ExitCommand.Players.ContainsKey(cInfo.entityId) && (GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > ExitCommand.Admin_Level &&
                                        GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > ExitCommand.Admin_Level))
                                    {
                                        player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                                        if (player != null)
                                        {
                                            ExitCommand.Players.Add(cInfo.entityId, player.position);
                                        }
                                    }
                                }
                            }
                        }
                        if (Shutdown.IsEnabled)
                        {
                            Shutdown.SetDelay(true);
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Stopserver has been cancelled and the next shutdown has been reset");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Stopserver has been cancelled");
                        }
                    }
                }
                else
                {
                    if (Shutdown.ShuttingDown)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Server is already set to shutdown. Cancel it if you wish to set a new countdown"));
                    }
                    else
                    {
                        if (int.TryParse(_params[0], out int countdown))
                        {
                            Shutdown.StartShutdown(countdown);
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid time specified '{0}'", _params[0]));
                        }
                    }                   
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in StopServerCommandConsole.Execute: {0}", e.Message));
            }
        }
    }
}
using System;
using System.Collections.Generic;

namespace ServerTools
{
    class StopServerCommandConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Starts a countdown with an alert system for the time specified and then stops the server";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-StopServer <Minutes>\n" +
                "  2. st-StopServer cancel\n" +
                "1. Starts a shutdown process with a countdown for the specified time\n" +
                "2. Cancels the shutdown\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-StopServer", "ss", "st-ss" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0] == "cancel")
                {
                    if (!Shutdown.ShuttingDown)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Stopserver is not running");
                    }
                    else
                    {
                        Shutdown.ShuttingDown = false;
                        Shutdown.NoEntry = false;
                        Shutdown.UI_Locked = false;
                        Lottery.ShuttingDown = false;
                        if (ExitCommand.IsEnabled)
                        {
                            List<ClientInfo> clientList = GeneralFunction.ClientList();
                            if (clientList != null)
                            {
                                for (int i = 0; i < clientList.Count; i++)
                                {
                                    ClientInfo cInfo = clientList[i];
                                    if (!ExitCommand.Players.ContainsKey(cInfo.entityId) && (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > ExitCommand.Admin_Level &&
                                        GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > ExitCommand.Admin_Level))
                                    {
                                        EntityPlayer player = GeneralFunction.GetEntityPlayer(cInfo.entityId);
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
                            Shutdown.SetDelay();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Stopserver has been cancelled and the next shutdown has been reset");
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Stopserver has been cancelled");
                        }
                    }
                }
                else
                {
                    if (Shutdown.ShuttingDown)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Server is already set to shutdown. Cancel it if you wish to set a new countdown"));
                    }
                    else
                    {
                        if (int.TryParse(_params[0], out int countdown))
                        {
                            Shutdown.StartShutdown(countdown);
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid time specified '{0}'", _params[0]));
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
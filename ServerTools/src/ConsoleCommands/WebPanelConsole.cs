using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class WebPanelConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable, restart web panel. Add, remove, reset, ban or list clients.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-web off\n" +
                   "  2. st-web on\n" +
                   "  3. st-web add {steamId}\n" +
                   "  4. st-web remove {steamId}\n" +
                   "  5. st-web reset {steamId}\n" +
                   "  6. st-web timeout add {IP}\n" +
                   "  7. st-web timeout remove {IP}\n" +
                   "  8. st-web clear\n" +
                   "  9. st-web list\n" +
                   "1. Turn off the web panel\n" +
                   "2. Turn on the web panel\n" +
                   "3. Add a client id to the web panel list\n" +
                   "4. Remove a client id from the web panel list or ban list\n" +
                   "5. Reset a client. Changes their current password and clears out their existing session. They must relog with the new password\n" +
                   "6. Add a IP address to the timeout list, restricting all access for ten minutes\n" +
                   "7. Remove a IP from the timeout list\n" +
                   "8. Web panel will clear all client sessions, timed out players and login attempts. All clients must relog\n" +
                   "9. Shows a list of client id that has been added. Also shows banned and timed out IP address\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Webpanel", "web", "st-web" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (WebAPI.IsEnabled)
                    {
                        WebAPI.IsEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Web panel has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Web panel is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!WebAPI.IsEnabled)
                    {
                        WebAPI.IsEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Web panel has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Web panel is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, 2, found {0}", _params.Count));
                        return;
                    }
                    else if (_params[1].Length < 6 || _params[1].Length > 30)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id must be 6 - 30 characters in length, found {0}", _params[1].Length));
                        return;
                    }
                    else
                    {
                        string _password = PersistentOperations.CreatePassword(16);
                        if (!PersistentContainer.Instance.Players.Players.ContainsKey(_params[1]) && PersistentContainer.Instance.Players[_params[1]].WebPass == null)
                        {
                            PersistentContainer.Instance.Players.Players.Add(_params[1], new PersistentPlayer(_params[1]));
                        }
                        PersistentContainer.Instance.Players[_params[1]].WebPass = _password;
                        PersistentContainer.DataChange = true;
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added {0} to the web panel client list. Their password is {1}", _params[1], _password));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, 2, found {0}", _params.Count));
                        return;
                    }
                    else if (_params[1].Length < 6 || _params[1].Length > 30)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id must be 6 - 30 characters in length, found {0}", _params[1].Length));
                        return;
                    }
                    else
                    {
                        if (PersistentContainer.Instance.Players.Players.ContainsKey(_params[1]) && PersistentContainer.Instance.Players[_params[1]].WebPass != null && PersistentContainer.Instance.Players[_params[1]].WebPass != "")
                        {
                            PersistentContainer.Instance.Players[_params[1]].WebPass = "";
                            PersistentContainer.DataChange = true;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed {0} from the web panel client list", _params[1]));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] {0} is not on the web panel client list", _params[1]));
                        }
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, 2, found {0}", _params.Count));
                        return;
                    }
                    else if (_params[1].Length < 6 || _params[1].Length > 30)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id must be 6 - 30 characters in length, found {0}", _params[1].Length));
                        return;
                    }
                    else
                    {
                        if (WebPanel.TimeOut.ContainsKey(_params[1]))
                        {
                            WebPanel.TimeOut.Remove(_params[1]);
                            if (PersistentContainer.Instance.WebPanelTimeoutList != null)
                            {
                                Dictionary<string, DateTime> _timeouts = PersistentContainer.Instance.WebPanelTimeoutList;
                                _timeouts.Remove(_params[1]);
                                PersistentContainer.Instance.WebPanelTimeoutList = _timeouts;
                            }
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} has been removed from the time out list", _params[1]));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} was not found on the time out list", _params[1]));
                        }
                        if (WebAPI.Visitor.ContainsKey(_params[1]))
                        {
                            WebAPI.Visitor.Remove(_params[1]);
                        }
                        if (WebAPI.AuthorizedIvKey.ContainsKey(_params[1]))
                        {
                            WebAPI.AuthorizedIvKey.Remove(_params[1]);
                            WebAPI.AuthorizedTime.Remove(_params[1]);
                        }
                        if (PersistentContainer.Instance.Players.Players.ContainsKey(_params[1]) && PersistentContainer.Instance.Players[_params[1]].WebPass != null && PersistentContainer.Instance.Players[_params[1]].WebPass != "")
                        {
                            string _password = PersistentOperations.CreatePassword(16);
                            PersistentContainer.Instance.Players[_params[1]].WebPass = _password;
                            PersistentContainer.DataChange = true;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client {0} has been reset. Their password is {1}", _params[1], _password));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] {0} is not on the web panel client list", _params[1]));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("timeout"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, 3, found {0}", _params.Count));
                        return;
                    }
                    else if (_params[1].ToLower() == "add")
                    {
                        string _ip = _params[2];
                        if (!WebPanel.TimeOut.ContainsKey(_ip))
                        {
                            WebPanel.TimeOut.Add(_ip, DateTime.Now);
                            PersistentContainer.Instance.WebPanelTimeoutList.Add(_ip, DateTime.Now);
                            PersistentContainer.DataChange = true;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} has been added to the timeout list", _ip));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add IP {0}. It is already on the timeout list", _ip));
                            return;
                        }
                    }
                    else if (_params[1].ToLower() == "remove")
                    {
                        string _ip = _params[2];
                        if (WebPanel.TimeOut.ContainsKey(_ip))
                        {
                            WebPanel.TimeOut.Remove(_ip);
                            PersistentContainer.Instance.WebPanelTimeoutList.Remove(_ip);
                            PersistentContainer.DataChange = true;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} has been removed from the timeout list", _ip));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove IP {0}. It is not on the timeout list", _ip));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[1]));
                    }
                }
                else if (_params[0].ToLower().Equals("clear"))
                {
                    if (WebAPI.IsEnabled)
                    {
                        WebAPI.AuthorizedIvKey.Clear();
                        WebAPI.AuthorizedTime.Clear();
                        WebAPI.Visitor.Clear();
                        WebPanel.PageHits.Clear();
                        WebPanel.LoginAttempts.Clear();
                        WebPanel.TimeOut.Clear();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Web panel has been cleared of all active and timed out clients along with login attempts"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Web API is not enabled. There is no web panel data to clear"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    List<string> _steamId = PersistentContainer.Instance.Players.SteamIDs;
                    if (_steamId != null && _steamId.Count > 0)
                    {
                        for (int i = 0; i < _steamId.Count; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(PersistentContainer.Instance.Players[_steamId[i]].WebPass))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id = {0}", _steamId[i]));
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no clients on the web panel list"));
                    }
                    if (WebPanel.TimeOut.Count > 0)
                    {
                        foreach (var _timeout in WebPanel.TimeOut)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Timed out IP = {0}. Lasts until = {1}", _timeout.Key, _timeout.Value.AddMinutes(10)));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no timed out ip address"));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPIConsole.Execute: {0}", e.Message));
            }
        }
    }
}

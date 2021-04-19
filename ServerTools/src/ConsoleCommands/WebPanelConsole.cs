using ServerTools.Website;
using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WebPanelConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable, restart website. Add, remove, reset, ban or list clients.";
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
                   "  8. st-web ban add {IP}\n" +
                   "  9. st-web ban remove {IP}\n" +
                   "  10. st-web restart\n" +
                   "  11. st-web list\n" +
                   "1. Turn off the web panel\n" +
                   "2. Turn on the web panel\n" +
                   "3. Add a client id to the web panel list\n" +
                   "4. Remove a client id from the web panel list or ban list\n" +
                   "5. Reset a client. Clears their current password and existing session forcing them to relog with a new password\n" +
                   "6. Add a IP address to the timeout list, restricting all access for ten minutes\n" +
                   "7. Remove a IP from the timeout list\n" +
                   "8. Add a IP address to the ban list, restricting all access permanently\n" +
                   "9. Remove a IP from the ban list\n" +
                   "10. Web panel will clear all sessions, close and restart. All users must relog\n" +
                   "11. Shows a list of allowed client id and banned IP addresses\n";
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
                    if (WebPanel.IsEnabled)
                    {
                        WebPanel.IsEnabled = false;
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
                    if (!WebPanel.IsEnabled)
                    {
                        WebPanel.IsEnabled = true;
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
                    else if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id must be 17 characters in length, found {0}", _params[1].Length));
                        return;
                    }
                    else
                    {
                        if (!WebPanel.Clients.Contains(_params[1]) && !WebPanel.BannedIP.Contains(_params[1]))
                        {
                            string _password = WebPanel.SetPassword();
                            WebPanel.Clients.Add(_params[1]);
                            if (PersistentContainer.Instance.WebPanelClientList != null)
                            {
                                List<string> _clients = PersistentContainer.Instance.WebPanelClientList;
                                _clients.Add(_params[1]);
                                PersistentContainer.Instance.WebPanelClientList = _clients;
                            }
                            else
                            {
                                List<string> _clients = new List<string>();
                                _clients.Add(_params[1]);
                                PersistentContainer.Instance.WebPanelClientList = _clients;
                            }
                            PersistentContainer.Instance.Players[_params[1]].WP = _password;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added {0} to the web panel client list. Their password is {1}", _params[1], _password));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add this id. Client is already on the list or banned"));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, 2, found {0}", _params.Count));
                        return;
                    }
                    else if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id must be 17 characters in length, found {0}", _params[1].Length));
                        return;
                    }
                    else
                    {
                        if (WebPanel.Clients.Contains(_params[1]))
                        {
                            WebPanel.Clients.Remove(_params[1]);
                            if (PersistentContainer.Instance.WebPanelClientList != null)
                            {
                                List<string> _clients = PersistentContainer.Instance.WebPanelClientList;
                                _clients.Remove(_params[1]);
                                PersistentContainer.Instance.WebPanelClientList = _clients;
                            }
                            PersistentContainer.Instance.Players[_params[1]].WP = "";
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed {0} from the web panel client list", _params[1]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove this id. Client is not on the list"));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, 2, found {0}", _params.Count));
                        return;
                    }
                    else if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id must be 17 characters in length, found {0}", _params[1].Length));
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
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} not found on the time out list", _params[1]));
                        }
                        if (WebPanel.Clients.Contains(_params[1]))
                        {
                            if (WebPanel.Visitor.ContainsKey(_params[1]))
                            {
                                WebPanel.Visitor.Remove(_params[1]);
                            }
                            if (WebPanel.Authorized.ContainsKey(_params[1]))
                            {
                                WebPanel.Authorized.Remove(_params[1]);
                                WebPanel.AuthorizedTime.Remove(_params[1]);
                            }
                            string _password = WebPanel.SetPassword();
                            PersistentContainer.Instance.Players[_params[1]].WP = _password;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client {0} has been reset. Their password is {1}", _params[1], _password));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client {0} was not found on the list", _params[1]));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("ban"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, 3, found {0}", _params.Count));
                        return;
                    }
                    else if (_params[1].ToLower() == "add")
                    {
                        string _ip = _params[3];
                        if (!WebPanel.BannedIP.Contains(_ip))
                        {
                            WebPanel.BannedIP.Add(_ip);
                            List<string> _banList = PersistentContainer.Instance.WebPanelBanList;
                            if (_banList != null && _banList.Count > 0)
                            {
                                if (_banList.Contains(_ip))
                                {
                                    _banList.Add(_ip);
                                    PersistentContainer.Instance.WebPanelBanList = _banList;
                                }
                            }
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} has been added to the ban list", _ip));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add IP {0}. It is already on the ban list", _ip));
                            return;
                        }
                    }
                    else if (_params[1].ToLower() == "remove")
                    {
                        string _ip = _params[2];
                        if (WebPanel.BannedIP.Contains(_ip))
                        {
                            WebPanel.BannedIP.Remove(_ip);
                            List<string> _banList = PersistentContainer.Instance.WebPanelBanList;
                            if (_banList != null && _banList.Count > 0)
                            {
                                if (_banList.Contains(_ip))
                                {
                                    _banList.Remove(_ip);
                                    PersistentContainer.Instance.WebPanelBanList = _banList;
                                }
                            }
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} has been removed from the ban list", _ip));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove IP {0}. It is not on the ban list", _ip));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[1]));
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
                        string _ip = _params[3];
                        if (!WebPanel.TimeOut.ContainsKey(_ip))
                        {
                            WebPanel.TimeOut.Add(_ip, DateTime.Now);
                            Dictionary<string, DateTime> _timeoutList = PersistentContainer.Instance.WebPanelTimeoutList;
                            if (_timeoutList != null && _timeoutList.Count > 0)
                            {
                                if (_timeoutList.ContainsKey(_ip))
                                {
                                    _timeoutList.Add(_ip, DateTime.Now);
                                    PersistentContainer.Instance.WebPanelTimeoutList = _timeoutList;
                                }
                            }
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
                            Dictionary<string, DateTime> _timeoutList = PersistentContainer.Instance.WebPanelTimeoutList;
                            if (_timeoutList != null && _timeoutList.Count > 0)
                            {
                                if (_timeoutList.ContainsKey(_ip))
                                {
                                    _timeoutList.Remove(_ip);
                                    PersistentContainer.Instance.WebPanelTimeoutList = _timeoutList;
                                }
                            }
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
                else if (_params[0].ToLower().Equals("restart"))
                {
                    if (WebPanel.IsEnabled)
                    {
                        WebPanel.IsEnabled = false;
                        WebPanel.Authorized.Clear();
                        WebPanel.Visitor.Clear();
                        WebPanel.PageHits.Clear();
                        WebPanel.LoginAttempts.Clear();
                        WebPanel.TimeOut.Clear();
                        WebPanel.IsEnabled = true;
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Web panel server has been cleared and restarted. All users must relog"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Web panel is not enabled, unable to restart it"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (WebPanel.Clients.Count > 0)
                    {
                        for (int i = 0; i < WebPanel.Clients.Count; i++)
                        {
                            string _client = WebPanel.Clients[i];
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id = {0}", _client));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no clients on the web panel list"));
                    }
                    if (WebPanel.BannedIP.Count > 0)
                    {
                        for (int i = 0; i < WebPanel.BannedIP.Count; i++)
                        {
                            string _bannedIp = WebPanel.BannedIP[i];
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Banned IP = {0}", _bannedIp));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no banned ip address"));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanelConsole.Execute: {0}", e.Message));
            }
        }
    }
}

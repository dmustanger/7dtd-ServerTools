using ServerTools.Website;
using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WebsiteConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable, restart website. Add, remove, reset, ban or list clients.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. ws off\n" +
                   "  2. ws on\n" +
                   "  3. ws add {steamId}\n" +
                   "  4. ws remove {steamId}\n" +
                   "  5. ws reset {steamId}\n" +
                   "  6. ws timeout add {IP}\n" +
                   "  7. ws timeout remove {IP}\n" +
                   "  8. ws ban add {IP}\n" +
                   "  9. ws ban remove {IP}\n" +
                   "  10. ws restart\n" +
                   "  11. ws list\n" +
                   "1. Turn off the website\n" +
                   "2. Turn on the website\n" +
                   "3. Add a client id to the website list\n" +
                   "4. Remove a client id from the website list or ban list\n" +
                   "5. Reset a client. Clears their current password and existing session forcing them to relog with a new password\n" +
                   "6. Add a IP address to the timeout list, restricting all access for ten minutes\n" +
                   "7. Remove a IP from the timeout list\n" +
                   "8. Add a IP address to the ban list, restricting all access permanently\n" +
                   "9. Remove a IP from the ban list\n" +
                   "10. Website will clear all sessions, close and restart. All users must relog\n" +
                   "11. Shows a list of allowed client id and banned IP addresses\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Website", "ws", "st-ws" };
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
                    if (WebsiteServer.IsEnabled)
                    {
                        WebsiteServer.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Website has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Website is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!WebsiteServer.IsEnabled)
                    {
                        WebsiteServer.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Website has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Website is already on"));
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
                        if (!WebsiteServer.Clients.Contains(_params[1]) && !WebsiteServer.BannedIP.Contains(_params[1]))
                        {
                            string _password = WebsiteServer.SetPassword();
                            WebsiteServer.Clients.Add(_params[1]);
                            if (PersistentContainer.Instance.WebsiteClientList != null)
                            {
                                List<string> _clients = PersistentContainer.Instance.WebsiteClientList;
                                _clients.Add(_params[1]);
                                PersistentContainer.Instance.WebsiteClientList = _clients;
                            }
                            else
                            {
                                List<string> _clients = new List<string>();
                                _clients.Add(_params[1]);
                                PersistentContainer.Instance.WebsiteClientList = _clients;
                            }
                            PersistentContainer.Instance.Players[_params[1]].WP = _password;
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added {0} to the website client list. Their password is {1}", _params[1], _password));
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
                        if (WebsiteServer.Clients.Contains(_params[1]))
                        {
                            WebsiteServer.Clients.Remove(_params[1]);
                            if (PersistentContainer.Instance.WebsiteClientList != null)
                            {
                                List<string> _clients = PersistentContainer.Instance.WebsiteClientList;
                                _clients.Remove(_params[1]);
                                PersistentContainer.Instance.WebsiteClientList = _clients;
                            }
                            PersistentContainer.Instance.Players[_params[1]].WP = "";
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed {0} from the website client list", _params[1]));
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
                        if (WebsiteServer.TimeOut.ContainsKey(_params[1]))
                        {
                            WebsiteServer.TimeOut.Remove(_params[1]);
                            if (PersistentContainer.Instance.WebsiteTimeoutList != null)
                            {
                                Dictionary<string, DateTime> _timeouts = PersistentContainer.Instance.WebsiteTimeoutList;
                                _timeouts.Remove(_params[1]);
                                PersistentContainer.Instance.WebsiteTimeoutList = _timeouts;
                                PersistentContainer.Instance.Save();
                            }
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} has been removed from the time out list", _params[1]));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} not found on the time out list", _params[1]));
                        }
                        if (WebsiteServer.Clients.Contains(_params[1]))
                        {
                            if (WebsiteServer.Visitor.ContainsKey(_params[1]))
                            {
                                WebsiteServer.Visitor.Remove(_params[1]);
                            }
                            if (WebsiteServer.Authorized.ContainsKey(_params[1]))
                            {
                                WebsiteServer.Authorized.Remove(_params[1]);
                                WebsiteServer.AuthorizedTime.Remove(_params[1]);
                            }
                            string _password = WebsiteServer.SetPassword();
                            PersistentContainer.Instance.Players[_params[1]].WP = _password;
                            PersistentContainer.Instance.Save();
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
                        if (!WebsiteServer.BannedIP.Contains(_ip))
                        {
                            WebsiteServer.BannedIP.Add(_ip);
                            List<string> _banList = PersistentContainer.Instance.WebsiteBanList;
                            if (_banList != null && _banList.Count > 0)
                            {
                                if (_banList.Contains(_ip))
                                {
                                    _banList.Add(_ip);
                                    PersistentContainer.Instance.WebsiteBanList = _banList;
                                    PersistentContainer.Instance.Save();
                                }
                            }
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} has been added to the website ban list", _ip));
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
                        if (WebsiteServer.BannedIP.Contains(_ip))
                        {
                            WebsiteServer.BannedIP.Remove(_ip);
                            List<string> _banList = PersistentContainer.Instance.WebsiteBanList;
                            if (_banList != null && _banList.Count > 0)
                            {
                                if (_banList.Contains(_ip))
                                {
                                    _banList.Remove(_ip);
                                    PersistentContainer.Instance.WebsiteBanList = _banList;
                                    PersistentContainer.Instance.Save();
                                }
                            }
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} has been removed from the website ban list", _ip));
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
                        if (!WebsiteServer.TimeOut.ContainsKey(_ip))
                        {
                            WebsiteServer.TimeOut.Add(_ip, DateTime.Now);
                            Dictionary<string, DateTime> _timeoutList = PersistentContainer.Instance.WebsiteTimeoutList;
                            if (_timeoutList != null && _timeoutList.Count > 0)
                            {
                                if (_timeoutList.ContainsKey(_ip))
                                {
                                    _timeoutList.Add(_ip, DateTime.Now);
                                    PersistentContainer.Instance.WebsiteTimeoutList = _timeoutList;
                                    PersistentContainer.Instance.Save();
                                }
                            }
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} has been added to the website timeout list", _ip));
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
                        if (WebsiteServer.TimeOut.ContainsKey(_ip))
                        {
                            WebsiteServer.TimeOut.Remove(_ip);
                            Dictionary<string, DateTime> _timeoutList = PersistentContainer.Instance.WebsiteTimeoutList;
                            if (_timeoutList != null && _timeoutList.Count > 0)
                            {
                                if (_timeoutList.ContainsKey(_ip))
                                {
                                    _timeoutList.Remove(_ip);
                                    PersistentContainer.Instance.WebsiteTimeoutList = _timeoutList;
                                    PersistentContainer.Instance.Save();
                                }
                            }
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] IP {0} has been removed from the website timeout list", _ip));
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
                    if (WebsiteServer.IsEnabled)
                    {
                        WebsiteServer.IsEnabled = false;
                        WebsiteServer.Authorized.Clear();
                        WebsiteServer.Visitor.Clear();
                        WebsiteServer.PageHits.Clear();
                        WebsiteServer.LoginAttempts.Clear();
                        WebsiteServer.TimeOut.Clear();
                        WebsiteServer.IsEnabled = true;
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Website server has been cleared and restarted. All users must relog"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Website is not enabled, unable to restart it"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (WebsiteServer.Clients.Count > 0)
                    {
                        for (int i = 0; i < WebsiteServer.Clients.Count; i++)
                        {
                            string _client = WebsiteServer.Clients[i];
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id = {0}", _client));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no clients on the website list"));
                    }
                    if (WebsiteServer.BannedIP.Count > 0)
                    {
                        for (int i = 0; i < WebsiteServer.BannedIP.Count; i++)
                        {
                            string _bannedIp = WebsiteServer.BannedIP[i];
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebsiteConsole.Execute: {0}", e.Message));
            }
        }
    }
}

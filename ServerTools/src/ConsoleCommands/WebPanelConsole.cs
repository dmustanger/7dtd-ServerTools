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
                        string _password = WebPanel.SetPassword();
                        PersistentContainer.Instance.Players[_params[1]].WP = _password;
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
                    else if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id must be 17 characters in length, found {0}", _params[1].Length));
                        return;
                    }
                    else
                    {
                        if (PersistentContainer.Instance.Players[_params[1]].WP != null)
                        {
                            PersistentContainer.Instance.Players[_params[1]].WP = "";
                        }
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed {0} from the web panel client list", _params[1]));
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
                }
                else if (_params[0].ToLower().Equals("ban"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found {0}", _params.Count));
                        return;
                    }
                    else if (_params[1].ToLower() == "add")
                    {
                        string _ip = _params[2];
                        if (_ip.Contains("."))
                        {
                            if (!WebPanel.BannedIP.Contains(_ip))
                            {
                                WebPanel.BannedIP.Add(_ip);
                                if (PersistentContainer.Instance.WebPanelBanList != null)
                                {
                                    PersistentContainer.Instance.WebPanelBanList.Add(_ip);
                                }
                                else
                                {
                                    List<string> _banList = new List<string>();
                                    _banList.Add(_ip);
                                    PersistentContainer.Instance.WebPanelBanList = _banList;
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
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add IP. Inproper format: {0}", _ip));
                            return;
                        }
                    }
                    else if (_params[1].ToLower() == "remove")
                    {
                        string _ip = _params[2];
                        if (WebPanel.BannedIP.Contains(_ip))
                        {
                            WebPanel.BannedIP.Remove(_ip);
                            if (PersistentContainer.Instance.WebPanelBanList != null && PersistentContainer.Instance.WebPanelBanList.Contains(_ip))
                            {
                                PersistentContainer.Instance.WebPanelBanList.Remove(_ip);
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
                        string _ip = _params[2];
                        if (!WebPanel.TimeOut.ContainsKey(_ip))
                        {
                            WebPanel.TimeOut.Add(_ip, DateTime.Now);
                            PersistentContainer.Instance.WebPanelTimeoutList.Add(_ip, DateTime.Now);
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
                        WebPanel.AuthorizedTime.Clear();
                        WebPanel.Visitor.Clear();
                        WebPanel.PageHits.Clear();
                        WebPanel.LoginAttempts.Clear();
                        WebPanel.TimeOut.Clear();
                        WebPanel.IsEnabled = true;
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Web panel server has been cleared and restarted. All active clients must relog"));
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
                    List<string> _steamId = PersistentContainer.Instance.Players.SteamIDs;
                    if (_steamId != null && _steamId.Count > 0)
                    {
                        for (int i = 0; i < _steamId.Count; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(PersistentContainer.Instance.Players.Players[_steamId[i]].WP))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Client id = {0}", _steamId[i]));
                            }
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
                            if (WebPanel.BannedIP[i] != "")
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Banned IP = {0}", WebPanel.BannedIP[i]));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Banned IP = {0}", WebPanel.BannedIP[i]));
                            }
                        }
                    }
                    else if (WebPanel.TimeOut.Count > 0)
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanelConsole.Execute: {0}", e.Message));
            }
        }
    }
}

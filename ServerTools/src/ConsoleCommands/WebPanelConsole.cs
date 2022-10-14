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
                   "  3. st-web add <ClientId>\n" +
                   "  4. st-web remove <ClientId>\n" +
                   "  5. st-web reset <ClientId>\n" +
                   "  6. st-web reset all\n" +
                   "  7. st-web timeout add <IP>\n" +
                   "  8. st-web timeout remove <IP>\n" +
                   "  9. st-web ban add <IP>\n" +
                   "  10. st-web ban remove <IP>\n" +
                   "  11. st-web list\n" +
                   "1. Turn off the web panel\n" +
                   "2. Turn on the web panel\n" +
                   "3. Add a client id to the web panel list. Id must be 6-30 chars in length\n" +
                   "4. Remove a client id from the web panel list. This will also remove them from the timeout and ban list\n" +
                   "5. Reset a single client. Clears out their existing session. They must relog\n" +
                   "6. Reset all clients. Clear all existing sessions. All clients must relog\n" +
                   "7. Add a IP address to the timeout list, restricting all access for five minutes\n" +
                   "8. Remove a IP from the timeout list\n" +
                   "9. Add a IP address to the ban list, restricting all access\n" +
                   "10. Remove a IP from the ban list\n" +
                   "11. Shows a list of client id that has been added. Also shows banned and timed out IP address\n";
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
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (WebAPI.IsEnabled)
                    {
                        WebAPI.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Web panel has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Web panel is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!WebAPI.IsEnabled)
                    {
                        WebAPI.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Web panel has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Web panel is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    else if (_params[1].Length < 6 || _params[1].Length > 30)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Client id must be 6 - 30 characters in length, found '{0}'", _params[1].Length));
                        return;
                    }
                    else
                    {
                        string password = GeneralFunction.CreatePassword(8);
                        if (!PersistentContainer.Instance.Players.Players.ContainsKey(_params[1]))
                        {
                            PersistentContainer.Instance.Players.Players.Add(_params[1], new PersistentPlayer(_params[1]));
                        }
                        PersistentContainer.Instance.Players[_params[1]].WebPass = password;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added '{0}' to the web panel client list. Their password = {1}", _params[1], password));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    else if (_params[1].Length < 6 || _params[1].Length > 30)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Client id must be 6 - 30 characters in length, found '{0}'", _params[1].Length));
                        return;
                    }
                    else
                    {
                        if (PersistentContainer.Instance.Players[_params[1]].WebPass != null && PersistentContainer.Instance.Players[_params[1]].WebPass != "")
                        {
                            PersistentContainer.Instance.Players[_params[1]].WebPass = "";
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed '{0}' from the web panel client list", _params[1]));
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] '{0}' is not on the web panel client list", _params[1]));
                        }
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    else if (_params[1].Length < 6 || _params[1].Length > 30)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Client id must be 6 - 30 characters in length, found '{0}'", _params[1].Length));
                        return;
                    }
                    else
                    {
                        if (PersistentContainer.Instance.Players[_params[1]].WebPass != null && PersistentContainer.Instance.Players[_params[1]].WebPass != "")
                        {
                            string password = GeneralFunction.CreatePassword(8);
                            PersistentContainer.Instance.Players[_params[1]].WebPass = password;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Client '{0}' has been reset. Their password is '{1}'", _params[1], password));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] '{0}' is not on the web panel client list", _params[1]));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("reset") && _params[1].ToLower().Equals("all"))
                {
                    if (WebAPI.IsEnabled)
                    {
                        WebAPI.Authorized.Clear();
                        WebAPI.AuthorizedTime.Clear();
                        WebAPI.Visitor.Clear();
                        WebAPI.PageHits.Clear();
                        WebAPI.LoginAttempts.Clear();
                        WebAPI.TimeOut.Clear();
                        if (PersistentContainer.Instance.WebTimeoutList != null && PersistentContainer.Instance.WebTimeoutList.Count > 0)
                        {
                            PersistentContainer.Instance.WebTimeoutList.Clear();
                            PersistentContainer.DataChange = true;
                        }
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Web panel has been reset of all session data. Clients must relog"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Web API is not enabled. There is no web panel data to clear"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("timeout"))
                {
                    if (_params.Count != 2 && _params.Count != 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or 3, found '{0}'", _params.Count));
                        return;
                    }
                    else if (_params[1].ToLower() == "add")
                    {
                        string ip = _params[2];
                        if (!WebAPI.TimeOut.ContainsKey(ip))
                        {
                            WebAPI.TimeOut.Add(ip, DateTime.Now.AddMinutes(5));
                            if (PersistentContainer.Instance.WebTimeoutList != null)
                            {
                                PersistentContainer.Instance.WebTimeoutList.Add(ip, DateTime.Now.AddMinutes(5));
                            }
                            else
                            {
                                Dictionary<string, DateTime> timeouts = new Dictionary<string, DateTime>();
                                timeouts.Add(ip, DateTime.Now.AddMinutes(5));
                                PersistentContainer.Instance.WebTimeoutList = timeouts;
                            }
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] IP '{0}' has been added to the timeout list", ip));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not add IP '{0}'. It is already on the timeout list", ip));
                            return;
                        }
                    }
                    else if (_params[1].ToLower() == "remove")
                    {
                        string ip = _params[2];
                        if (WebAPI.TimeOut.ContainsKey(ip))
                        {
                            WebAPI.TimeOut.Remove(ip);
                            PersistentContainer.Instance.WebTimeoutList.Remove(ip);
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] IP '{0}' has been removed from the timeout list", ip));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not remove IP '{0}'. It is not on the timeout list", ip));
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[1]));
                    }
                }
                else if (_params[0].ToLower().Equals("ban"))
                {
                    if (_params.Count != 2 && _params.Count != 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or 3, found '{0}'", _params.Count));
                        return;
                    }
                    else if (_params[1].ToLower() == "add")
                    {
                        string ip = _params[2];
                        if (!WebAPI.Ban.Contains(ip))
                        {
                            WebAPI.Ban.Add(ip);
                            if (PersistentContainer.Instance.WebBanList != null)
                            {
                                PersistentContainer.Instance.WebBanList.Add(ip);
                            }
                            else
                            {
                                List<string> bans = new List<string>();
                                bans.Add(ip);
                                PersistentContainer.Instance.WebBanList = bans;
                            }
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] IP '{0}' has been added to the ban list", ip));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not add IP '{0}'. It is already on the ban list", ip));
                            return;
                        }
                    }
                    else if (_params[1].ToLower() == "remove")
                    {
                        string ip = _params[2];
                        if (WebAPI.Ban.Contains(ip))
                        {
                            WebAPI.Ban.Remove(ip);
                            if (PersistentContainer.Instance.WebBanList != null)
                            {
                                PersistentContainer.Instance.WebBanList.Remove(ip);
                                PersistentContainer.DataChange = true;
                            }
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] IP '{0}' has been removed from the ban list", ip));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not remove IP '{0}'. It is not on the ban list", ip));
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[1]));
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    List<string> _steamId = PersistentContainer.Instance.Players.IDs;
                    if (_steamId != null && _steamId.Count > 0)
                    {
                        for (int i = 0; i < _steamId.Count; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(PersistentContainer.Instance.Players[_steamId[i]].WebPass))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Client id '{0}'", _steamId[i]));
                            }
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] There are no clients on the web panel list"));
                    }
                    if (WebAPI.TimeOut.Count > 0)
                    {
                        foreach (var timeout in WebAPI.TimeOut)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Timed out IP '{0}'. Lasts until '{1}'", timeout.Key, timeout.Value.AddMinutes(10)));
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] There are no timed out ip address"));
                    }
                    if (WebAPI.Ban.Count > 0)
                    {
                        for (int i = 0; i < WebAPI.Ban.Count; i++)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Banned IP '{0}'", WebAPI.Ban[i]));
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] There are no banned ip address"));
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPIConsole.Execute: {0}", e.Message));
            }
        }
    }
}

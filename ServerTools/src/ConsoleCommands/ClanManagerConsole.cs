using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ClanManagerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Clan Manager.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. ClanManager off\n" +
                   "  2. ClanManager on\n" +
                   "  3. ClanManager list\n" +
                   "  4. ClanManager list <ClanName>\n" +
                   "  5. ClanManager delete <ClanName>\n" +
                   "  6. ClanManager delete <SteamId>\n" +
                   "1. Turn off clan manager\n" +
                   "2. Turn on clan manager\n" +
                   "3. Shows a list of the clans\n" +
                   "4. Shows a list of members from a clan\n" +
                   "5. Deletes a clan and all of its members from the clan system\n" +
                   "6. Deletes a player from the clan system\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-ClanManager", "ClanManager", "clanmanager", "st-cm", "cm" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 && _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ClanManager.IsEnabled)
                    {
                        ClanManager.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Clan manager has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Clan manager is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!ClanManager.IsEnabled)
                    {
                        ClanManager.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Clan manager has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Clan manager is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    if (ClanManager.Clans.Count > 0)
                    {
                        SdtdConsole.Instance.Output(string.Format("Clan List:"));
                        foreach (KeyValuePair<string, string> i in ClanManager.Clans)
                        {
                            string _playerName = PersistentContainer.Instance.Players[i.Key].PlayerName;
                            if (!string.IsNullOrEmpty(_playerName))
                            {
                                SdtdConsole.Instance.Output(string.Format("Clan named {0}, owned by {1}, with id {2}", i.Value, _playerName, i.Key));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Clan named {0}, owned by player with id {1}", i.Value, i.Key));
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("No clans were found"));
                    }
                    return;
                }
                else if (_params[0].ToLower().Equals("delete"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    int _steamId;
                    if (int.TryParse(_params[1], out _steamId))
                    {
                        if (ClanManager.ClanMember.Contains(_params[1]))
                        {
                            string _clanName = PersistentContainer.Instance.Players[_params[1]].ClanName;
                            string _playerName = PersistentContainer.Instance.Players[_params[1]].PlayerName;
                            if (PersistentContainer.Instance.Players[_params[1]].ClanOwner)
                            {
                                ClanManager.Clans.Remove(_params[1]);
                                PersistentContainer.Instance.Players[_params[1]].ClanOwner = false;
                                PersistentContainer.Instance.Players[_params[1]].ClanRequestToJoin = null;
                                for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
                                {
                                    string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                                    PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                                    {
                                        if (p.ClanName != null && p.ClanName == _clanName)
                                        {
                                            p.ClanOfficer = false;
                                            p.ClanName = "";
                                            ClanManager.ClanMember.Remove(_id);
                                            SdtdConsole.Instance.Output(string.Format("Deleted {0} with id {1} from the clan system", p.PlayerName, _id));
                                            ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_id);
                                            if (_cInfo2 != null)
                                            {
                                                string _phrase121;
                                                if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                                                {
                                                    _phrase121 = " you have been removed from the clan {ClanName}.";
                                                }
                                                _phrase121 = _phrase121.Replace("{ClanName}", _clanName);
                                                ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo2.playerName + LoadConfig.Chat_Response_Color + _phrase121 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                        else if (p.ClanInvite != null && p.ClanInvite == _clanName)
                                        {
                                            PersistentContainer.Instance.Players[_id].ClanInvite = "";
                                        }
                                    }
                                }
                            }
                            PersistentContainer.Instance.Players[_params[1]].ClanOfficer = false;
                            PersistentContainer.Instance.Players[_params[1]].ClanName = "";
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("Deleted {0} with id {1} from the clan system", _playerName, _params[1]));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("{0} was not found in the clan system", _params[1]));
                            
                        }
                    }
                    else
                    {
                        if (ClanManager.Clans.ContainsValue(_params[1]))
                        {
                            foreach (KeyValuePair<string, string> _clan in ClanManager.Clans)
                            {
                                if (_clan.Value == _params[1])
                                {
                                    ClanManager.Clans.Remove(_clan.Key);
                                    break;
                                }
                            }
                            for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
                            {
                                string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                                PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                                {
                                    if (p.ClanName != null && p.ClanName == _params[1])
                                    {
                                        p.ClanName = "";
                                        p.ClanOwner = false;
                                        p.ClanOfficer = false;
                                        p.ClanRequestToJoin = null;
                                        ClanManager.ClanMember.Remove(_id);
                                        SdtdConsole.Instance.Output(string.Format("Deleted {0} with id {1} from the clan system", p.PlayerName, _id));
                                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_id);
                                        if (_cInfo2 != null)
                                        {
                                            string _phrase121;
                                            if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                                            {
                                                _phrase121 = " you have been removed from the clan {ClanName}.";
                                            }
                                            _phrase121 = _phrase121.Replace("{ClanName}", _params[0]);
                                            ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo2.playerName + LoadConfig.Chat_Response_Color + _phrase121 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else if (p.ClanInvite != null && p.ClanInvite == _params[1])
                                    {
                                        PersistentContainer.Instance.Players[_id].ClanInvite = "";
                                    }
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("{0} was not found in the clan system", _params[1]));

                        }
                    }
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManagerConsole.Execute: {0}", e));
            }
        }
    }
}
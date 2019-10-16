using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    public class CommandJailConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]- Puts A Player In Jail.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. jail off\n" +
                "  2. jail on\n" +
                "  3. jail add <steamId/entityId> <time>\n" +
                "  4. jail remove <steamId>" +
                "  5. jail list\n" +
                "1. Turn off jail\n" +
                "2. Turn on jail\n" +
                "3. Adds a steam Id to the jail list for a specific time in minutes\n" +
                "4. Removes a steam Id from the jail list\n" +
                "5. Lists all steam Id in the jail list" +
                "*Note Use -1 for time to jail indefinitely*";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Jail", "jail", string.Empty };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 3, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    Jail.IsEnabled = false;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Jail has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Jail.IsEnabled = true;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Jail has been set to on"));
                    return;
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 3, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}.", _params[1]));
                        return;
                    }
                    if (Jail.Jailed.Contains(_params[1]))
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the Jail list.", _params[1]));
                        return;
                    }
                    int _jailTime;
                    if (!int.TryParse(_params[2], out _jailTime))
                    {
                        SdtdConsole.Instance.Output(string.Format("Jail time is not valid: {0}", _params[2]));
                        return;
                    }
                    if (Jail.Jail_Position == "0,0,0" || Jail.Jail_Position == "0 0 0" || Jail.Jail_Position == "")
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not put a player in jail: Jail position has not been set."));
                        return;
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                        if (_cInfo != null)
                        {
                            if (Jail.Jailed.Contains(_cInfo.playerId))
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with Id {0} is already in jail.", _params[1]));
                                return;
                            }
                            else
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                if (_player.IsSpawned())
                                {
                                    int x, y, z;
                                    string[] _cords = Jail.Jail_Position.Split(',');
                                    int.TryParse(_cords[0], out x);
                                    int.TryParse(_cords[1], out y);
                                    int.TryParse(_cords[2], out z);
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                                }
                                Jail.Jailed.Add(_cInfo.playerId);
                                PersistentContainer.Instance.Players[_cInfo.playerId].JailTime = _jailTime;
                                PersistentContainer.Instance.Players[_cInfo.playerId].JailName = _cInfo.playerName;
                                PersistentContainer.Instance.Players[_cInfo.playerId].JailDate = DateTime.Now;
                                PersistentContainer.Instance.Save();
                                if (_jailTime > 0)
                                {
                                    string _phrase500;
                                    if (!Phrases.Dict.TryGetValue(500, out _phrase500))
                                    {
                                        _phrase500 = " you have been sent to jail.";
                                    }
                                    _phrase500 = _phrase500.Replace("{Minutes}", _jailTime.ToString());
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase500 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    SdtdConsole.Instance.Output(string.Format("You have put {0} in jail for {1} minutes.", _cInfo.playerName, _jailTime));
                                }
                                if (_jailTime == -1)
                                {
                                    string _phrase500;
                                    if (!Phrases.Dict.TryGetValue(500, out _phrase500))
                                    {
                                        _phrase500 = " you have been sent to jail.";
                                    }
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase500 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    SdtdConsole.Instance.Output(string.Format("You have put {0} in jail for life.", _cInfo.playerName));
                                }
                            }
                        }
                        else
                        {
                            PersistentPlayer p = PersistentContainer.Instance.Players[_params[1]];
                            {
                                if (p != null)
                                {
                                    Jail.Jailed.Add(_cInfo.playerId);
                                    PersistentContainer.Instance.Players[_params[1]].JailTime = _jailTime;
                                    PersistentContainer.Instance.Players[_params[1]].JailName = _cInfo.playerName;
                                    PersistentContainer.Instance.Players[_params[1]].JailDate = DateTime.Now;
                                    PersistentContainer.Instance.Save();
                                    SdtdConsole.Instance.Output(string.Format("Player with Id {0} can not be found online but has been set for jail.", _params[1]));
                                    return;
                                }
                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not remove player Id: Invalid steam Id {0}.", _params[1]));
                        return;
                    }
                    else
                    {
                        if (!Jail.Jailed.Contains(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with steam Id {0} is not in jail. ", _params[1]));
                            return;
                        }
                        else
                        {
                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_params[1]);
                            if (_cInfo != null)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                EntityBedrollPositionList _position = _player.SpawnPoints;
                                Jail.Jailed.Remove(_cInfo.playerId);
                                PersistentContainer.Instance.Players[_cInfo.playerId].JailTime = 0;
                                PersistentContainer.Instance.Save();
                                if (_position.Count > 0)
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_position[0].x, -1, _position[0].z), null, false));
                                }
                                else
                                {
                                    Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, -1, _pos[0].z), null, false));
                                }
                                SdtdConsole.Instance.Output(string.Format("You have released a player with steam id {0} from jail. ", _params[1]));
                                return;
                            }
                            else
                            {

                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    if (Jail.Jailed.Count == 0)
                    {
                        SdtdConsole.Instance.Output("There are no Ids on the Jail list.");
                        return;
                    }
                    else
                    {
                        for (int i = 0; i < Jail.Jailed.Count; i++)
                        {
                            string _id = Jail.Jailed[i];
                            int _jailTime = PersistentContainer.Instance.Players[_id].JailTime;
                            string _jailName = PersistentContainer.Instance.Players[_id].JailName;
                            if (_jailTime > 0)
                            {
                                DateTime _jailDate = PersistentContainer.Instance.Players[_id].JailDate;
                                TimeSpan varTime = DateTime.Now - _jailDate;
                                double fractionalMinutes = varTime.TotalMinutes;
                                int _timepassed = (int)fractionalMinutes;
                                int _timeleft = _jailTime - _timepassed;
                                SdtdConsole.Instance.Output(string.Format("Jailed player: steam Id {0} named {1} for {2} more minutes.", _id, _jailName, _timeleft));
                            }
                            else if (_jailTime == -1)
                            {
                                SdtdConsole.Instance.Output(string.Format("Jailed player: steam Id {0} named {1} forever.", _id, _jailName));
                            }
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CommandJailConsole.Run: {0}.", e));
            }
        }
    }
}
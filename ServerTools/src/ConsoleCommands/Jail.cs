using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class CommandJail : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "Puts a player in jail.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. jail add <steamId>\n" +
                "  2. jail remove <steamId>" +
                "  3. jail list\n" +
                "1. Adds a steamID to the jail list\n" +
                "2. Removes a steamID from the jail list\n" +
                "3. Lists all steamID in the jail list";
        }
        public override string[] GetCommands()
        {
            return new string[] { "jail", string.Empty };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 2, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if(_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count)); 
                        return;
                    }
                    if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add SteamId: Invalid SteamId {0}.", _params[1]));
                        return;
                    }
                    if (Jail.Dict.ContainsKey(_params[1]))
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add SteamId. {0} is already in the Jail list.", _params[1]));
                        return;
                    }
                    if (Jail.JailPosition == "0,0,0")
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not put a player in jail: Jail position has not been set."));
                        return;
                    }
                    else
                    {
                        Player p = PersistentContainer.Instance.Players[_params[1], false];
                        if (p != null)
                        {
                            if (p.IsJailed)
                            {
                                SdtdConsole.Instance.Output(string.Format("SteamId {0} is already in jail. ", _params[1]));
                                return;
                            }
                        }
                        PersistentContainer.Instance.Players[_params[1], true].IsJailed = true;
                        PersistentContainer.Instance.Players[_params[1], true].IsRemovedFromJail = false;
                        PersistentContainer.Instance.Save();
                        if (ConnectionManager.Instance.ClientCount() > 0)
                        {
                            ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_params[1]);
                            if (_cInfo != null)
                            {
                                float xf;
                                float yf;
                                float zf;
                                string[] _cords = Jail.JailPosition.Split(',');
                                float.TryParse(_cords[0], out xf);
                                float.TryParse(_cords[1], out yf);
                                float.TryParse(_cords[2], out zf);
                                int x = (int)xf;
                                int y = (int)yf;
                                int z = (int)zf;
                                SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, x, y, z), _cInfo);
                                if (!Jail.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    Jail.Dict.Add(_cInfo.playerId, null);
                                    Jail.Dict1.Add(_cInfo.playerId, _cInfo.playerName);
                                }
                                string _phrase500;
                                if (!Phrases.Dict.TryGetValue(500, out _phrase500))
                                {
                                    _phrase500 = "{PlayerName} you have been sent to jail.";
                                }
                                _phrase500 = _phrase500.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase500), "Server", false, "", false));
                            }
                        }
                        SdtdConsole.Instance.Output(string.Format("You have put {0} in jail. ", _params[1]));
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
                        SdtdConsole.Instance.Output(string.Format("Can not remove SteamId: Invalid SteamId {0}.", _params[1]));
                        return;
                    }
                    else
                    {
                        Player p = PersistentContainer.Instance.Players[_params[1], false];
                        if (p == null)
                        {
                            SdtdConsole.Instance.Output(string.Format("SteamId {0} is not in jail. ", _params[1]));
                            return;
                        }
                        if (!p.IsJailed)
                        {
                            SdtdConsole.Instance.Output(string.Format("SteamId {0} is not in jail. ", _params[1]));
                            return;
                        }
                        PersistentContainer.Instance.Players[_params[1], false].IsJailed = false;
                        if (ConnectionManager.Instance.ClientCount() > 0)
                        {
                            ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_params[1]);
                            if (_cInfo != null)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                EntityBedrollPositionList _position = _player.SpawnPoints;
                                if (Jail.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    Jail.Dict.Remove(_cInfo.playerId);
                                    Jail.Dict1.Remove(_cInfo.playerId);
                                }
                                if (_position.Count > 0)
                                {
                                    SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, _position[0].x, _position[0].y, _position[0].z), _cInfo);
                                }
                                else
                                {
                                    Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, _pos[0].x, _pos[0].y, _pos[0].z), _cInfo);
                                }
                                string _phrase501;
                                if (!Phrases.Dict.TryGetValue(501, out _phrase501))
                                {
                                    _phrase501 = "{PlayerName} you have been released from jail.";
                                }
                                _phrase501 = _phrase501.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase501), "Server", false, "", false));
                                PersistentContainer.Instance.Players[_params[1], false].IsRemovedFromJail = true;
                            }
                        }
                        PersistentContainer.Instance.Save();
                        SdtdConsole.Instance.Output(string.Format("You have released {0} from jail. ", _params[1]));
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    if (Jail.Dict1.Count < 1)
                    {
                        SdtdConsole.Instance.Output("There are no steamIds on the Jail list.");
                        return;
                    }
                    foreach (KeyValuePair<string, string> _key in Jail.Dict1)
                    {
                        string _name;
                        if (Jail.Dict1.TryGetValue(_key.Key, out _name))
                        {
                            SdtdConsole.Instance.Output(string.Format("{0} {1}", _key.Key, _key.Value));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in CommandJail.Run: {0}.", e));
            }
        }
    }
}
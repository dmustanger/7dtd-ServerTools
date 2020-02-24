using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class LobbyConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Sets the lobby location.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. Lobby off" +
                "  2. Lobby on" +
                "  3. Lobby set" +
                "1. Turn off lobby\n" +
                "2. Turn on lobby\n" +
                "3. Sets the lobby position to your current location\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Lobby", "Lobby", "lobby" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Lobby.IsEnabled)
                    {
                        Lobby.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Lobby has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Lobby is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Lobby.IsEnabled)
                    {
                        Lobby.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Lobby has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Lobby is already on"));
                        return;
                    }
                }
                else if (_params[0] == ("set"))
                {
                    ClientInfo _cInfo = _senderInfo.RemoteClientInfo;
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    Vector3 _position = _player.GetPosition();
                    int x = (int)_position.x;
                    int y = (int)_position.y;
                    int z = (int)_position.z;
                    string _lposition = x + "," + y + "," + z;
                    Lobby.Lobby_Position = _lposition;
                    string _phrase551;
                    if (!Phrases.Dict.TryGetValue(551, out _phrase551))
                    {
                        _phrase551 = "{PlayerName} you have set the lobby position as {LobbyPosition}";
                    }
                    _phrase551 = _phrase551.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase551 = _phrase551.Replace("{LobbyPosition}", _lposition);
                    SdtdConsole.Instance.Output(string.Format("{0}", _phrase551));
                    LoadConfig.WriteXml();
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CommandJailConsole.Execute: {0}", e.Message));
            }
        }
    }
}

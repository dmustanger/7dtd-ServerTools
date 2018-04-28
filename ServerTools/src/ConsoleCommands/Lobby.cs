using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Lobby : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Sets the lobby location.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. lobby set" +
                "1. Sets the lobby position to your current location\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Lobby", "lobby" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                    return;
                }
                if (_params[0] == ("set"))
                {
                    ClientInfo _cInfo = _senderInfo.RemoteClientInfo;
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    Vector3 _position = _player.GetPosition();
                    int x = (int)_position.x;
                    int y = (int)_position.y;
                    int z = (int)_position.z;
                    string _lposition = x + "," + y + "," + z;
                    SetLobby.Lobby_Position = _lposition;
                    string _phrase551;
                    if (!Phrases.Dict.TryGetValue(551, out _phrase551))
                    {
                        _phrase551 = "{PlayerName} you have set the lobby position as {LobbyPosition}.";
                    }
                    _phrase551 = _phrase551.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase551 = _phrase551.Replace("{LobbyPosition}", _lposition);
                    SdtdConsole.Instance.Output(string.Format("{0}", _phrase551));
                    Config.UpdateXml();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CommandJail.Run: {0}.", e));
            }
        }
    }
}

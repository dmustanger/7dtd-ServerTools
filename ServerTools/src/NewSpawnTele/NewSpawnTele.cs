using UnityEngine;

namespace ServerTools
{
    public class NewSpawnTele
    {
        public static bool IsEnabled = false, Return = false;
        private static string[] _cmd = { "tele" };
        public static string New_Spawn_Tele_Position = "0,0,0";

        public static void SetNewSpawnTele(ClientInfo _cInfo)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase200), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                string _sposition = x + "," + y + "," + z;
                New_Spawn_Tele_Position = _sposition;
                string _phrase525;
                if (!Phrases.Dict.TryGetValue(525, out _phrase525))
                {
                    _phrase525 = "{PlayerName} you have set the New Spawn position as {NewSpawnTelePosition}.";
                }
                _phrase525 = _phrase525.Replace("{PlayerName}", _cInfo.playerName);
                _phrase525 = _phrase525.Replace("{NewSpawnTelePosition}", New_Spawn_Tele_Position);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase525), Config.Server_Response_Name, false, "ServerTools", false));
                Config.UpdateXml();
            }
        }

        public static void TeleNewSpawn(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player.Level == 1 && _player.totalItemsCrafted == 0 && _player.distanceWalked <= 0.5 && New_Spawn_Tele_Position != "0,0,0")
            {
                TelePlayer(_cInfo, _player);
            }
        }

        public static void TelePlayer(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (Return)
            {
                Vector3 Vec3 = _player.position;
                string _position = _player.position.x + "," + _player.position.y + "," + _player.position.z;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].NewTeleSpawn = _position;
                PersistentContainer.Instance.Save();
            }
            int x, y, z;
            string[] _cords = New_Spawn_Tele_Position.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
            if (!Return)
            {
                string _phrase526;
                if (!Phrases.Dict.TryGetValue(526, out _phrase526))
                {
                    _phrase526 = "{PlayerName} you have been teleported to the new spawn location.";
                }
                _phrase526 = _phrase526.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase526), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                string _phrase527;
                if (!Phrases.Dict.TryGetValue(527, out _phrase527))
                {
                    _phrase527 = "{PlayerName} type /ready when you are prepared to leave. You will teleport back to your spawn location.";
                }
                _phrase527 = _phrase527.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase527), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void ReturnPlayer(ClientInfo _cInfo)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null)
            {
                if (p.NewTeleSpawn != ("0, 0, 0"))
                {
                    int x, y, z;
                    string[] _coords = New_Spawn_Tele_Position.Split(',');
                    int.TryParse(_coords[0], out x);
                    int.TryParse(_coords[2], out z);
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= 50 * 50)
                    {
                        string[] _oldCoords = p.NewTeleSpawn.Split(',');
                        int.TryParse(_oldCoords[0], out x);
                        int.TryParse(_oldCoords[1], out y);
                        int.TryParse(_oldCoords[2], out z);
                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].NewTeleSpawn = ("0, 0, 0");
                        PersistentContainer.Instance.Save();
                        string _phrase530;
                        if (!Phrases.Dict.TryGetValue(530, out _phrase530))
                        {
                            _phrase530 = "{PlayerName} you have been sent back to your original spawn location. Good luck.";
                        }
                        _phrase530 = _phrase530.Replace("{PlayerName}", _cInfo.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase530), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        string _phrase529;
                        if (!Phrases.Dict.TryGetValue(529, out _phrase529))
                        {
                            _phrase529 = "{PlayerName} you have left the new player area. Return to it before using /ready.";
                        }
                        _phrase529 = _phrase529.Replace("{PlayerName}", _cInfo.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase529), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    string _phrase528;
                    if (!Phrases.Dict.TryGetValue(528, out _phrase528))
                    {
                        _phrase528 = "{PlayerName} you have no saved return point or you have used it.";
                    }
                    _phrase528 = _phrase528.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase528), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                string _phrase528;
                if (!Phrases.Dict.TryGetValue(528, out _phrase528))
                {
                    _phrase528 = "{PlayerName} you have no saved return point or you have used it.";
                }
                _phrase528 = _phrase528.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase528), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}
using UnityEngine;

namespace ServerTools
{
    public class NewSpawnTele
    {
        public static bool IsEnabled = false;
        private static string[] _cmd = { "tele" };
        public static string NewSpawnTelePosition = "0,0,0";

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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                string x = _position.x.ToString();
                string y = _position.y.ToString();
                string z = _position.z.ToString();
                string _sposition = x + "," + y + "," + z;
                NewSpawnTelePosition = _sposition;
                string _phrase525;
                if (!Phrases.Dict.TryGetValue(525, out _phrase525))
                {
                    _phrase525 = "{PlayerName} you have set the New Spawn position as {NewSpawnTelePosition}.";
                }
                _phrase525 = _phrase525.Replace("{PlayerName}", _cInfo.playerName);
                _phrase525 = _phrase525.Replace("{NewSpawnTelePosition}", NewSpawnTelePosition);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase525), "Server", false, "", false));
                Config.UpdateXml();
            }
        }

        public static void TeleNewSpawn(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p == null && _player.Level == 1 && _player.totalItemsCrafted == 0 && NewSpawnTelePosition != "0,0,0")
            {
                TelePlayer(_cInfo);
            }
            else
            {
                if (_player.Level == 1 && _player.distanceWalked == 0 && NewSpawnTelePosition != "0,0,0" && !p.NewSpawnTele)
                {

                    TelePlayer(_cInfo);
                }
            }
        }

        public static void TelePlayer(ClientInfo _cInfo)
        {
            float xf;
            float yf;
            float zf;
            string[] _cords = NewSpawnTelePosition.Split(',');
            float.TryParse(_cords[0], out xf);
            float.TryParse(_cords[1], out yf);
            float.TryParse(_cords[2], out zf);
            int x = (int)xf;
            int y = (int)yf;
            int z = (int)zf;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].NewSpawnTele = true;
            PersistentContainer.Instance.Save();
            SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, x, y, z), _cInfo);
            string _phrase526;
            if (!Phrases.Dict.TryGetValue(526, out _phrase526))
            {
                _phrase526 = "{PlayerName} you have been teleported to the new spawn location.";
            }
            _phrase526 = _phrase526.Replace("{PlayerName}", _cInfo.playerName);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase526), "Server", false, "", false));
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class NewSpawnTele
    {
        public static bool IsEnabled = false;
        private static string[] _cmd = { "tele" };
        public static string New_Spawn_Tele_Position = "0,0,0";
        public static List<int> NewSpawn = new List<int>();

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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase200), "Server", false, "", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase525), "Server", false, "", false));
                Config.UpdateXml();
            }
        }

        public static void TeleNewSpawn(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player.Level == 1 && _player.totalItemsCrafted == 0 && _player.distanceWalked == 0 && New_Spawn_Tele_Position != "0,0,0" && !NewSpawn.Contains(_cInfo.entityId))
            {
                TelePlayer(_cInfo);
            }
        }

        public static void TelePlayer(ClientInfo _cInfo)
        {
            float xf;
            float yf;
            float zf;
            string[] _cords = New_Spawn_Tele_Position.Split(',');
            float.TryParse(_cords[0], out xf);
            float.TryParse(_cords[1], out yf);
            float.TryParse(_cords[2], out zf);
            int x = (int)xf;
            int y = (int)yf;
            int z = (int)zf;
            SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, x, y, z), (ClientInfo)null);
            NewSpawn.Add(_cInfo.entityId);
            string _phrase526;
            if (!Phrases.Dict.TryGetValue(526, out _phrase526))
            {
                _phrase526 = "{PlayerName} you have been teleported to the new spawn location.";
            }
            _phrase526 = _phrase526.Replace("{PlayerName}", _cInfo.playerName);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase526), "Server", false, "", false));
        }
    }
}
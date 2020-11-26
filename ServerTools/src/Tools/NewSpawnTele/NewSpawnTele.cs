using UnityEngine;

namespace ServerTools
{
    public class NewSpawnTele
    {
        public static bool IsEnabled = false, Return = false;
        public static string Command29 = "setspawn", Command86 = "ready";
        private static string[] _cmd = { Command29 };
        public static string New_Spawn_Tele_Position = "0,0,0";

        public static void SetNewSpawnTele(ClientInfo _cInfo)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo))
            {
                Phrases.Dict.TryGetValue(217, out string _phrase217);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase217 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                LoadConfig.WriteXml();
                Phrases.Dict.TryGetValue(211, out string _phrase211);
                _phrase211 = _phrase211.Replace("{Position}", New_Spawn_Tele_Position);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase211 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void TeleNewSpawn(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (!PersistentContainer.Instance.Players[_cInfo.playerId].NewSpawn)
            {
                if (Return)
                {
                    Vector3 Vec3 = _player.position;
                    string _position = (int)_player.position.x + "," + (int)_player.position.y + "," + (int)_player.position.z;
                    PersistentContainer.Instance.Players[_cInfo.playerId].NewSpawnPosition = _position;
                }
                string[] _cords = New_Spawn_Tele_Position.Split(',');
                int x, y, z;
                int.TryParse(_cords[0], out x);
                int.TryParse(_cords[1], out y);
                int.TryParse(_cords[2], out z);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                PersistentContainer.Instance.Players[_cInfo.playerId].NewSpawn = true;
                Phrases.Dict.TryGetValue(212, out string _phrase212);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase212 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                if (Return)
                {
                    Phrases.Dict.TryGetValue(213, out string _phrase213);
                    _phrase213 = _phrase213.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase213 = _phrase213.Replace("{Command86}", Command86);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase213 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void ReturnPlayer(ClientInfo _cInfo)
        {
            string _pos = PersistentContainer.Instance.Players[_cInfo.playerId].NewSpawnPosition;
            if (_pos != "")
            {
                string[] _cords = { };
                if (New_Spawn_Tele_Position.Contains(","))
                {
                    _cords = New_Spawn_Tele_Position.Split(',');
                }
                else
                {
                    _cords = New_Spawn_Tele_Position.Split(' ');
                }
                int x, y, z;
                int.TryParse(_cords[0], out x);
                int.TryParse(_cords[2], out z);
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= 50 * 50)
                {
                    string[] _oldCords = _pos.Split(',');
                    int.TryParse(_oldCords[0], out x);
                    int.TryParse(_oldCords[1], out y);
                    int.TryParse(_oldCords[2], out z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                    PersistentContainer.Instance.Players[_cInfo.playerId].NewSpawnPosition = "";
                    Phrases.Dict.TryGetValue(216, out string _phrase216);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase216 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(215, out string _phrase215);
                    _phrase215 = _phrase215.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase215 = _phrase215.Replace("{Command86}", Command86);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase215 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(214, out string _phrase214);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase214 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
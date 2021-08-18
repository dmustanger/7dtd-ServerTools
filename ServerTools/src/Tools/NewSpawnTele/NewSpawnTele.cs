using UnityEngine;

namespace ServerTools
{
    public class NewSpawnTele
    {
        public static bool IsEnabled = false, Return = false;
        public static string Command_setspawn = "setspawn", Command_ready = "ready";
        private static string[] _cmd = { Command_setspawn };
        public static string New_Spawn_Tele_Position = "0,0,0";

        public static void SetNewSpawnTele(ClientInfo _cInfo)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo))
            {
                Phrases.Dict.TryGetValue("NewSpawnTele7", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                Config.WriteXml();
                Phrases.Dict.TryGetValue("NewSpawnTele1", out string _phrase);
                _phrase = _phrase.Replace("{Position}", New_Spawn_Tele_Position);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void TeleNewSpawn(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (!PersistentContainer.Instance.Players[_cInfo.playerId].NewSpawn)
            {
                if (Return)
                {
                    string _position = (int)_player.position.x + "," + (int)_player.position.y + "," + (int)_player.position.z;
                    PersistentContainer.Instance.Players[_cInfo.playerId].NewSpawnPosition = _position;
                }
                string[] _cords = New_Spawn_Tele_Position.Split(',');
                int.TryParse(_cords[0], out int x);
                int.TryParse(_cords[1], out int y);
                int.TryParse(_cords[2], out int z);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                PersistentContainer.Instance.Players[_cInfo.playerId].NewSpawn = true;
                PersistentContainer.DataChange = true;
                Phrases.Dict.TryGetValue("NewSpawnTele2", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                if (Return)
                {
                    Phrases.Dict.TryGetValue("NewSpawnTele3", out _phrase);
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_ready}", Command_ready);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                int.TryParse(_cords[0], out int x);
                int.TryParse(_cords[2], out int z);
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= 50 * 50)
                {
                    string[] _oldCords = _pos.Split(',');
                    int.TryParse(_oldCords[0], out x);
                    int.TryParse(_oldCords[1], out int y);
                    int.TryParse(_oldCords[2], out z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                    PersistentContainer.Instance.Players[_cInfo.playerId].NewSpawnPosition = "";
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("NewSpawnTele6", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("NewSpawnTele5", out string _phrase);
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_ready}", Command_ready);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("NewSpawnTele4", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
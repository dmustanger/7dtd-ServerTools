using UnityEngine;

namespace ServerTools
{
    public class NewSpawnTele
    {
        public static bool IsEnabled = false, Return = false;
        public static string Command_setspawn = "setspawn", Command_ready = "ready";
        private static string[] Cmd = { Command_setspawn };
        public static string New_Spawn_Tele_Position = "0,0,0";

        public static void SetNewSpawnTele(ClientInfo _cInfo)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(Cmd, _cInfo))
            {
                Phrases.Dict.TryGetValue("NewSpawnTele7", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    Vector3 position = player.GetPosition();
                    int x = (int)position.x;
                    int y = (int)position.y;
                    int z = (int)position.z;
                    string sposition = x + "," + y + "," + z;
                    New_Spawn_Tele_Position = sposition;
                    Config.WriteXml();
                    Phrases.Dict.TryGetValue("NewSpawnTele1", out string phrase);
                    phrase = phrase.Replace("{Position}", New_Spawn_Tele_Position);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void TeleNewSpawn(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (!PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].NewSpawn)
            {
                if (Return)
                {
                    string position = (int)_player.position.x + "," + (int)_player.position.y + "," + (int)_player.position.z;
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].NewSpawnPosition = position;
                }
                string[] cords = New_Spawn_Tele_Position.Split(',');
                int.TryParse(cords[0], out int x);
                int.TryParse(cords[1], out int y);
                int.TryParse(cords[2], out int z);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].NewSpawn = true;
                PersistentContainer.DataChange = true;
                Phrases.Dict.TryGetValue("NewSpawnTele2", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                if (Return)
                {
                    Phrases.Dict.TryGetValue("NewSpawnTele3", out phrase);
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_ready}", Command_ready);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void ReturnPlayer(ClientInfo _cInfo)
        {
            string pos = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].NewSpawnPosition;
            if (pos != "")
            {
                string[] cords = { };
                if (New_Spawn_Tele_Position.Contains(","))
                {
                    cords = New_Spawn_Tele_Position.Split(',');
                }
                else
                {
                    cords = New_Spawn_Tele_Position.Split(' ');
                }
                int.TryParse(cords[0], out int x);
                int.TryParse(cords[2], out int z);
                EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if ((x - player.position.x) * (x - player.position.x) + (z - player.position.z) * (z - player.position.z) <= 50 * 50)
                    {
                        string[] oldCords = pos.Split(',');
                        int.TryParse(oldCords[0], out x);
                        int.TryParse(oldCords[1], out int y);
                        int.TryParse(oldCords[2], out z);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].NewSpawnPosition = "";
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("NewSpawnTele6", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("NewSpawnTele5", out string phrase);
                        phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                        phrase = phrase.Replace("{Command_ready}", Command_ready);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("NewSpawnTele4", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
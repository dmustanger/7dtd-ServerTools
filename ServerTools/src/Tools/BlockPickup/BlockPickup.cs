using System.Collections.Generic;

namespace ServerTools
{
    class BlockPickup
    {
        public static bool IsEnabled = false, Admin_Only = false, Reserved = false;
        public static int Admin_Level = 0;
        public static string Command_pickup = "pickup";

        public static List<int> PickupEnabled = new List<int>();

        public static void Exec(ClientInfo _cInfo)
        {
            if (Admin_Only && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.PlatformId) > Admin_Level &&
                GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.CrossplatformId) > Admin_Level)
            {
                return;
            }
            if (Reserved)
            {
                if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    if (!PickupEnabled.Contains(_cInfo.entityId))
                    {
                        PickupEnabled.Add(_cInfo.entityId);
                        Phrases.Dict.TryGetValue("Pickup1", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        PickupEnabled.Remove(_cInfo.entityId);
                        Phrases.Dict.TryGetValue("Pickup2", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Pickup3", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else if (!PickupEnabled.Contains(_cInfo.entityId))
            {
                PickupEnabled.Add(_cInfo.entityId);
                Phrases.Dict.TryGetValue("Pickup1", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                PickupEnabled.Remove(_cInfo.entityId);
                Phrases.Dict.TryGetValue("Pickup2", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

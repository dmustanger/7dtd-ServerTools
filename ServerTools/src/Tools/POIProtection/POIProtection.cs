using System;

namespace ServerTools
{
    class POIProtection
    {
        public static bool IsEnabled = false, Bed = false, Claim = false;
        public static int Extra_Distance = 20;

        public static void ReturnBed(ClientInfo _cInfo, Vector3i _position, string _name)
        {
            GeneralOperations.ReturnBlock(_cInfo, _name, 1, "GiveItem1");
            Phrases.Dict.TryGetValue("POI1", out string phrase);
            phrase = phrase.Replace("{Distance}", POIProtection.Extra_Distance.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void ReturnClaim(ClientInfo _cInfo, Vector3i _position, string _name)
        {
            GeneralOperations.ReturnBlock(_cInfo, _name, 1, "GiveItem1");
            Phrases.Dict.TryGetValue("POI2", out string phrase);
            phrase = phrase.Replace("{Distance}", POIProtection.Extra_Distance.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}

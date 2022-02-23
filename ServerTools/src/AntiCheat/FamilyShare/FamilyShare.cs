using Platform.Steam;
using System;

namespace ServerTools
{
    class FamilyShare
    {
        public static bool IsEnabled = false;

        public static void Exec(ClientInfo _cInfo)
        {
            UserIdentifierSteam uIS = _cInfo.PlatformId as UserIdentifierSteam;
            if (!uIS.SteamId.Equals(uIS.OwnerId))
            {
                Phrases.Dict.TryGetValue("FamilyShare1", out string phrase);
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 1 month \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                return;
            }
        }
    }
}

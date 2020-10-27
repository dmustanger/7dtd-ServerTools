using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageGameMessage
    {
        static AccessTools.FieldRef<NetPackageGameMessage, string> _mainName = AccessTools.FieldRefAccess<NetPackageGameMessage, string>("mainName");

        public static bool PackageGameMessage_ProcessPackage_Prefix(NetPackageGameMessage __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                if (_cInfo.playerName != _mainName(__instance))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageGameMessage uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying their name to {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _mainName(__instance)));
                    Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                    Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted modifying their name to {0}", _mainName(__instance)));
                    return false;
                }
            }
            return true;
        }
    }
}

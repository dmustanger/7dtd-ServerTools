using HarmonyLib;
using System;

namespace ServerTools
{
    class PackagePartyActions
    {
        static AccessTools.FieldRef<NetPackagePartyActions, int> _invitedByEntityID = AccessTools.FieldRefAccess<NetPackagePartyActions, int>("invitedByEntityID");
        static AccessTools.FieldRef<NetPackagePartyActions, int> _invitedEntityID = AccessTools.FieldRefAccess<NetPackagePartyActions, int>("invitedEntityID");

        public static bool PackagePartyActions_ProcessPackage_Prefix(NetPackagePartyActions __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                if (_cInfo.entityId != _invitedByEntityID(__instance))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePartyActions uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying their entity id to {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _invitedByEntityID(__instance)));
                    Packages.Ban(_cInfo);
                    Packages.Writer(_cInfo, string.Format("Attempted modifying their entity id to {0}", _invitedByEntityID(__instance)));
                    return false;
                }
                if (_cInfo.entityId == _invitedEntityID(__instance))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePartyActions uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted inviting themselves to a party", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName));
                    Packages.Ban(_cInfo);
                    Packages.Writer(_cInfo, string.Format("Attempted inviting themselves to a party"));
                    return false;
                }
            }
            return true;
        }
    }
}

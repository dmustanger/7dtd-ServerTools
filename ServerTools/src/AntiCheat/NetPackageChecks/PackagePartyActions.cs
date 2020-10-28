using HarmonyLib;
using System;

namespace ServerTools
{
    class PackagePartyActions
    {
        static AccessTools.FieldRef<NetPackagePartyActions, int> _invitedByEntityID = AccessTools.FieldRefAccess<NetPackagePartyActions, int>("invitedByEntityID");
        static AccessTools.FieldRef<NetPackagePartyActions, int> _invitedEntityID = AccessTools.FieldRefAccess<NetPackagePartyActions, int>("invitedEntityID");
        static AccessTools.FieldRef<NetPackagePartyActions, NetPackagePartyActions.PartyActions> _currentOperation = AccessTools.FieldRefAccess<NetPackagePartyActions, NetPackagePartyActions.PartyActions>("currentOperation");

        public static bool PackagePartyActions_ProcessPackage_Prefix(NetPackagePartyActions __instance, World _world)
        {
            try
            {
                if (__instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    if (_currentOperation(__instance) == NetPackagePartyActions.PartyActions.SendInvite && _cInfo.entityId != _invitedByEntityID(__instance))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePartyActions uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted sending a party invitation as entity id {4} to {5}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _invitedByEntityID(__instance), _invitedEntityID(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted sending a party invitation as entity id {0} to {1}", _invitedByEntityID(__instance), _invitedEntityID(__instance)));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        return false;
                    }
                    else if (_currentOperation(__instance) == NetPackagePartyActions.PartyActions.AcceptInvite && _cInfo.entityId != _invitedEntityID(__instance))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePartyActions uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted sending a party accept as entity id {4} for invitation from {5}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _invitedEntityID(__instance), _invitedByEntityID(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted sending a party accept as entity id {0} for invitation from {1}", _invitedEntityID(__instance), _invitedByEntityID(__instance)));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        return false;
                    }
                    else if (_currentOperation(__instance) == NetPackagePartyActions.PartyActions.LeaveParty && (_cInfo.entityId != _invitedByEntityID(__instance) || _cInfo.entityId != _invitedEntityID(__instance)))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePartyActions uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted leaving a party with mismatched entity id {4} or {5}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _invitedEntityID(__instance), _invitedByEntityID(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted leaving a party with mismatched entity id {0} or {1}", _invitedEntityID(__instance), _invitedByEntityID(__instance)));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        return false;
                    }
                    else if (_currentOperation(__instance) == NetPackagePartyActions.PartyActions.KickFromParty && _cInfo.entityId != _invitedByEntityID(__instance))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePartyActions uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted kicking a party member with mismatched entity id {4} against {5}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _invitedByEntityID(__instance), _invitedEntityID(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted kicking a party member with mismatched entity id {0} against {1}", _invitedByEntityID(__instance), _invitedEntityID(__instance)));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PackagePersistentPlayerState.PackagePersistentPlayerState_ProcessPackage_Prefix: {0}", e.Message));
            }
            return true;
        }
    }
}

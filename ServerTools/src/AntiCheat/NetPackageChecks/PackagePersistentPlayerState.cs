using HarmonyLib;
using System;

namespace ServerTools
{
    class PackagePersistentPlayerState
    {
        static AccessTools.FieldRef<NetPackagePersistentPlayerState, PersistentPlayerData> _persistentPlayerData = AccessTools.FieldRefAccess<NetPackagePersistentPlayerState, PersistentPlayerData>("m_ppData");
        static AccessTools.FieldRef<NetPackagePersistentPlayerState, EnumPersistentPlayerDataReason> _reason = AccessTools.FieldRefAccess<NetPackagePersistentPlayerState, EnumPersistentPlayerDataReason>("m_reason");
        static AccessTools.FieldRef<NetPackagePersistentPlayerState, string> _playerID = AccessTools.FieldRefAccess<NetPackagePersistentPlayerState, string>("m_playerID");
        static AccessTools.FieldRef<NetPackagePersistentPlayerState, string> _otherPlayerID = AccessTools.FieldRefAccess<NetPackagePersistentPlayerState, string>("m_otherPlayerID");

        public static bool PackagePersistentPlayerState_ProcessPackage_Prefix(NetPackagePersistentPlayerState __instance, World _world)
        {
            try
            {
                if (Packages.IsEnabled && __instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    if (_persistentPlayerData(__instance) != null)
                    {
                        if (_cInfo.playerId != _persistentPlayerData(__instance).PlayerId || _cInfo.entityId != _persistentPlayerData(__instance).EntityId)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackagePersistentPlayerState uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempted modifying persistent player data with steam id {4} entity id {5}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _persistentPlayerData(__instance).PlayerId, _persistentPlayerData(__instance).EntityId));
                            Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted modifying persistent player data with steam id {0} entity id {1}", _persistentPlayerData(__instance).PlayerId, _persistentPlayerData(__instance).EntityId));
                            Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                            return false;
                        }
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

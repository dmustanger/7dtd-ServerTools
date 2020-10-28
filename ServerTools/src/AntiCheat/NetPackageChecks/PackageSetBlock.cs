using HarmonyLib;
using System;


namespace ServerTools
{
    class PackageSetBlock
    {
        static AccessTools.FieldRef<NetPackageSetBlock, string> _persistentPlayerId = AccessTools.FieldRefAccess<NetPackageSetBlock, string>("persistentPlayerId");

        public static bool PackageSetBlock_ProcessPackage_Prefix(NetPackageSetBlock __instance, World _world)
        {
            try
            {
                if (Packages.IsEnabled && __instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    if (_cInfo.playerId != _persistentPlayerId(__instance))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageSetBlock uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted changing a block with mismatched player id {4}", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName, _persistentPlayerId(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted changing a block with mismatched player id {0}", _persistentPlayerId(__instance)));
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

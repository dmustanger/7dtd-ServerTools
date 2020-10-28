using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageExplosionClient
    {
        static AccessTools.FieldRef<NetPackageExplosionClient, int> _blastPower = AccessTools.FieldRefAccess<NetPackageExplosionClient, int>("blastPower");
        static AccessTools.FieldRef<NetPackageExplosionClient, int> _blastRadius = AccessTools.FieldRefAccess<NetPackageExplosionClient, int>("blastRadius");

        public static bool PackageExplosionClient_ProcessPackage_Prefix(NetPackageExplosionClient __instance, World _world)
        {
            try
            {
                if (__instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    if (_blastPower(__instance) > 100)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageExplosionClient uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted to run an explosion with an excessive blast power of {4}", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName, _blastPower(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted to run an explosion with an excessive blast power of {0}", _blastPower(__instance)));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        return false;
                    }
                    else if (_blastRadius(__instance) > 10)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageExplosionClient uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted to run an explosion with an excessive blast radius of {4}", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName, _blastRadius(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted to run an explosion with an excessive blast radius of {0}", _blastRadius(__instance)));
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

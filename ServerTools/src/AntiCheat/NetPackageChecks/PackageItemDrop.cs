using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageItemDrop
    {
        static AccessTools.FieldRef<NetPackageItemDrop, ItemStack> _itemStack = AccessTools.FieldRefAccess<NetPackageItemDrop, ItemStack>("itemStack");
        static AccessTools.FieldRef<NetPackageItemDrop, int> _entityId = AccessTools.FieldRefAccess<NetPackageItemDrop, int>("entityId");

        public static bool PackageItemDrop_ProcessPackage_Prefix(NetPackageItemDrop __instance, World _world)
        {
            try
            {
                if (Packages.IsEnabled && __instance.Sender != null)
                {
                    ClientInfo _cInfo = __instance.Sender;
                    if (!_itemStack(__instance).itemValue.ItemClass.CanHold())
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageItemDrop uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted to send item drop of {4}. This item can not be held", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName, _itemStack(__instance).itemValue.ItemClass.GetLocalizedItemName() ?? _itemStack(__instance).itemValue.ItemClass.GetItemName()));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted to send item drop of {0}. This item can not be held", _itemStack(__instance).itemValue.ItemClass.GetLocalizedItemName() ?? _itemStack(__instance).itemValue.ItemClass.GetItemName()));
                        Packages.Ban(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName);
                        return false;
                    }
                    else if (_cInfo.entityId != _entityId(__instance))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageItemDrop uploaded by steam id = {0}, owner id = {1} entity id = {2} name = {3}. Attempted to send item drop of {4} using a mismatched entity id {5}", __instance.Sender.playerId, __instance.Sender.ownerId, __instance.Sender.entityId, __instance.Sender.playerName, _itemStack(__instance).itemValue.ItemClass.GetLocalizedItemName() ?? _itemStack(__instance).itemValue.ItemClass.GetItemName(), _entityId(__instance)));
                        Packages.Writer(_cInfo.ownerId, _cInfo.playerId, _cInfo.playerName, string.Format("Attempted to send item drop of {0} using a mismatched entity id {1}", _itemStack(__instance).itemValue.ItemClass.GetLocalizedItemName() ?? _itemStack(__instance).itemValue.ItemClass.GetItemName(), _entityId(__instance)));
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

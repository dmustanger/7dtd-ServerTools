using System.Collections.Generic;

namespace ServerTools
{
    class InfiniteAmmo
    {
        public static bool IsEnabled = false;

        public static Dictionary<int, int[]> Dict = new Dictionary<int, int[]>();

        public static bool Exec(ClientInfo _cInfo, EntityPlayer _player, int slot, ItemValue _itemValue)
        {
            if (_itemValue.ItemClass.DisplayType == "rangedRepairTool" || _itemValue.ItemClass.DisplayType == "rangedBow" || _itemValue.ItemClass.DisplayType == "adminRanged")
            {
                return false;
            }
            else if (!Dict.ContainsKey(_cInfo.entityId))
            {
                int rayCount = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, _itemValue, 1f, _player, null, default(FastTags), false, true);
                Dict.Add(_cInfo.entityId, new int[] { slot, _itemValue.ItemClass.Id, _cInfo.latestPlayerData.inventory[slot].itemValue.Meta * rayCount - 1 });
            }
            else
            {
                Dict.TryGetValue(_cInfo.entityId, out int[] ammoData);
                if (slot == ammoData[0] && _itemValue.ItemClass.Id == ammoData[1])
                {
                    ammoData[2] -= 1;
                    if (ammoData[2] < 0)
                    {
                        Phrases.Dict.TryGetValue("AntiCheat2", out string phrase);
                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, phrase), null);
                        return true;
                    }
                    else
                    {
                        Dict[_cInfo.entityId] = ammoData;
                    }
                }
                else
                {
                    ammoData[0] = slot;
                    ammoData[1] = _itemValue.ItemClass.Id;
                    int rayCount = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, _itemValue, 1f, _player, null, default(FastTags), false, true);
                    ammoData[2] = _cInfo.latestPlayerData.inventory[slot].itemValue.Meta * rayCount - 1;
                    Dict[_cInfo.entityId] = ammoData;
                }
            }
            return false;
        }
    }
}

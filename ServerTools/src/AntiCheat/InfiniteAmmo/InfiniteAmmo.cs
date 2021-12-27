using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class InfiniteAmmo
    {
        public static bool IsEnabled = false;

        public static Dictionary<int, int[]> Dict = new Dictionary<int, int[]>();

        private static string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string Filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

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
                    if (ammoData[2] < -3)
                    {
                        Dict.Remove(_cInfo.entityId);
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("Detected Id '{0}' '{1}' named '{2}' using infinite ammo @ '{3}'. Gun name '{4}' had '{5}' ammo", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _player.position, _itemValue.ItemClass.GetItemName(), ammoData[2]));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Phrases.Dict.TryGetValue("AntiCheat2", out string phrase);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
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

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class InfiniteAmmo
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0;

        public static Dictionary<int, int[]> Dict = new Dictionary<int, int[]>();

        private static Dictionary<int, int> Flags = new Dictionary<int, int>();

        private static string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string Filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);
        private static AccessTools.FieldRef<NetPackagePlayerInventory, ItemStack[]> ToolBelt = AccessTools.FieldRefAccess<NetPackagePlayerInventory, ItemStack[]>("toolbelt");


        public static void Exec(ClientInfo _cInfo, int slot, ItemValue _itemValue)
        {
            if (_itemValue.ItemClass.DisplayType == "rangedRepairTool" || _itemValue.ItemClass.DisplayType == "rangedBow" || _itemValue.ItemClass.DisplayType == "adminRanged")
            {
                return;
            }
            else if (!Dict.ContainsKey(_cInfo.entityId))
            {
                Dict.Add(_cInfo.entityId, new int[] { slot, _itemValue.Seed, _cInfo.latestPlayerData.inventory[slot].itemValue.Meta });
            }
            else
            {
                Dict.TryGetValue(_cInfo.entityId, out int[] ammoData);
                if (slot != ammoData[0] || _itemValue.Seed != ammoData[1] || _cInfo.latestPlayerData.inventory[slot].itemValue.Meta != ammoData[2])
                {
                    Dict[_cInfo.entityId] = new int[] { slot, _itemValue.Seed, _cInfo.latestPlayerData.inventory[slot].itemValue.Meta };
                }
            }
        }

        public static void Process(NetPackagePlayerInventory __instance)
        {
            if (Dict.ContainsKey(__instance.Sender.entityId))
            {
                Dict.TryGetValue(__instance.Sender.entityId, out int[] value);
                Dict.Remove(__instance.Sender.entityId);
                if (ToolBelt(__instance) != null)
                {
                    ItemValue itemValue = ToolBelt(__instance)[value[0]].itemValue;
                    if (!itemValue.IsEmpty() && itemValue.Seed == value[1] && itemValue.Meta == value[2])
                    {
                        if (!Flags.ContainsKey(__instance.Sender.entityId))
                        {
                            Flags.Add(__instance.Sender.entityId, 1);
                        }
                        else
                        {
                            Flags[__instance.Sender.entityId] += 1;
                            if (Flags[__instance.Sender.entityId] == 3)
                            {
                                Flags.Remove(__instance.Sender.entityId);
                                Phrases.Dict.TryGetValue("InfiniteAmmo1", out string phrase);
                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", __instance.Sender.CrossplatformId.CombinedString, phrase), null);
                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("Detected Id '{0}' '{1}' named '{2}' using infinite ammo", __instance.Sender.PlatformId.CombinedString, __instance.Sender.CrossplatformId.CombinedString, __instance.Sender.playerName));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                Log.Warning("[SERVERTOOLS] Detected Id '{0}' '{1}' named '{2}' using infinite ammo. They have been banned", __instance.Sender.PlatformId.CombinedString, __instance.Sender.CrossplatformId.CombinedString, __instance.Sender.playerName);
                                Phrases.Dict.TryGetValue("InfiniteAmmo2", out phrase);
                                phrase = phrase.Replace("{PlayerName}", __instance.Sender.playerName);
                                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            }
                        }
                    }
                    else if (Flags.ContainsKey(__instance.Sender.entityId))
                    {
                        Flags.Remove(__instance.Sender.entityId);
                    }
                }
            }
        }
    }
}

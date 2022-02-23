using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class BlockChange
    {
        public static int Admin_Level = 0;

        private static readonly string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string DetectionFilepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);

        public static bool ProcessBlockChange(GameManager __instance, PlatformUserIdentifierAbs _persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
        {
            try
            {
                if (__instance != null && _persistentPlayerId != null && _blocksToChange != null)
                {
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromUId(_persistentPlayerId);
                    if (cInfo != null)
                    {
                        EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                        if (player != null)
                        {
                            int slot = player.inventory.holdingItemIdx;
                            ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                            if (itemValue != null && InfiniteAmmo.IsEnabled && itemValue.ItemClass.IsGun() && InfiniteAmmo.Exec(cInfo, player, slot, itemValue))
                            {
                                return false;
                            }
                            World world = __instance.World;
                            for (int i = 0; i < _blocksToChange.Count; i++)
                            {
                                BlockChangeInfo newBlockInfo = _blocksToChange[i];//new block info
                                BlockValue oldBlockValue = world.GetBlock(newBlockInfo.pos);//old block value
                                Block oldBlock = oldBlockValue.Block;
                                if (newBlockInfo != null && newBlockInfo.bChangeBlockValue)//has new block value
                                {
                                    Block newBlock = newBlockInfo.blockValue.Block;
                                    if (oldBlockValue.Equals(BlockValue.Air))//old block was air
                                    {
                                        if (newBlock is BlockSleepingBag)//placed a sleeping bag
                                        {
                                            if (POIProtection.IsEnabled && POIProtection.Bed && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), 8))
                                            {
                                                GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                                                PersistentOperations.ReturnBlock(cInfo, newBlock.GetBlockName(), 1);
                                                Phrases.Dict.TryGetValue("POI1", out string phrase);
                                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return false;
                                            }
                                        }
                                        else if (newBlock is BlockLandClaim)//placed a land claim
                                        {
                                            if (POIProtection.IsEnabled && POIProtection.Claim && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), 5))
                                            {
                                                GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                                                PersistentOperations.ReturnBlock(cInfo, newBlock.GetBlockName(), 1);
                                                Phrases.Dict.TryGetValue("POI2", out string _phrase);
                                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return false;
                                            }
                                        }
                                        if (BlockLogger.IsEnabled && !_blocksToChange[i].blockValue.Equals(BlockValue.Air))
                                        {
                                            BlockLogger.PlacedBlock(cInfo, newBlock, newBlockInfo.pos);
                                        }
                                        return true;
                                    }
                                    else if (oldBlockValue.Block is BlockTrapDoor)
                                    {
                                        return true;
                                    }
                                    else if (newBlockInfo.blockValue.Equals(BlockValue.Air.type))//new block is air
                                    {
                                        if (oldBlockValue.Block is BlockLandClaim)
                                        {
                                            if (!PersistentOperations.ClaimedByAllyOrSelf(_persistentPlayerId, newBlockInfo.pos))
                                            {
                                                if (DamageDetector.IsEnabled)
                                                {
                                                    int total = oldBlock.MaxDamage - oldBlockValue.damage;
                                                    if (total >= DamageDetector.Block_Damage_Limit && (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                                        GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level))
                                                    {

                                                        Penalty(total, player, cInfo);
                                                        return false;
                                                    }
                                                }
                                                if (BlockLogger.IsEnabled)
                                                {
                                                    BlockLogger.BrokeBlock(cInfo, oldBlock, newBlockInfo.pos);
                                                }
                                            }
                                            else if (BlockLogger.IsEnabled)
                                            {
                                                BlockLogger.RemovedBlock(cInfo, oldBlock, newBlockInfo.pos);
                                            }
                                        }
                                        else if (!oldBlock.CanPickup)//old block can not be picked up
                                        {
                                            if (!PersistentOperations.ClaimedByAllyOrSelf(_persistentPlayerId, newBlockInfo.pos))
                                            {
                                                if (DamageDetector.IsEnabled)
                                                {
                                                    int total = oldBlock.MaxDamage - oldBlockValue.damage;
                                                    if (total >= DamageDetector.Block_Damage_Limit && (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                                        GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level))
                                                    {
                                                        Penalty(total, player, cInfo);
                                                        return false;
                                                    }
                                                }
                                            }
                                            if (BlockLogger.IsEnabled)
                                            {
                                                BlockLogger.RemovedBlock(cInfo, oldBlock, newBlockInfo.pos);
                                            }
                                        }
                                    }
                                    else if (oldBlock.blockID == newBlock.blockID)//block is the same
                                    {
                                        if (newBlockInfo.bChangeDamage)//block took damage
                                        {
                                            if (DamageDetector.IsEnabled)
                                            {
                                                int total = newBlockInfo.blockValue.damage - oldBlockValue.damage;
                                                if (total >= DamageDetector.Block_Damage_Limit && (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                                    GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level))
                                                {
                                                    Penalty(total, player, cInfo);
                                                    return false;
                                                }
                                            }
                                        }
                                    }
                                    else if (oldBlock.DowngradeBlock.Block.blockID == newBlock.blockID)//downgraded
                                    {
                                        if (oldBlockValue.damage == newBlockInfo.blockValue.damage || newBlockInfo.blockValue.damage == 0)
                                        {
                                            if (BlockLogger.IsEnabled)
                                            {
                                                BlockLogger.DowngradedBlock(cInfo, oldBlock, newBlock, newBlockInfo.pos);
                                            }
                                            return true;
                                        }
                                        if (DamageDetector.IsEnabled)
                                        {
                                            int total = oldBlock.MaxDamage - oldBlockValue.damage + newBlockInfo.blockValue.damage;
                                            if (total >= DamageDetector.Block_Damage_Limit && (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                                GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level))
                                            {
                                                Penalty(total, player, cInfo);
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BlockChange.ProcessBlockChange: {0}", e.Message));
            }
            return true;
        }

        private static void Penalty(int _total, EntityPlayer _player, ClientInfo _cInfo)
        {
            try
            {
                Phrases.Dict.TryGetValue("DamageDetector3", out string phrase);
                phrase = phrase.Replace("{Value}", _total.ToString());
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected \"'{0}'\" with id '{1}' '{2}' using '{3}' that exceeded the damage limit @ '{4}'. Damage value '{5}'", _cInfo.playerName, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _player.inventory.holdingItem.GetLocalizedItemName() ?? _player.inventory.holdingItem.GetItemName(), _player.position, _total));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("DamageDetector1", out phrase);
                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BlockChange.BlockPenalty: {0}", e.Message));
            }
        }
    }
}

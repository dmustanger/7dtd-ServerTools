using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class BlockChange
    {
        public static int Admin_Level = 0;
        private static readonly string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string DetectionFilepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);

        public static bool ProcessBlockChange(GameManager __instance, string _persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
        {
            try
            {
                if (__instance != null && !string.IsNullOrEmpty(_persistentPlayerId) && _blocksToChange != null)
                {
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromSteamId(_persistentPlayerId);
                    if (cInfo != null)
                    {
                        EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.playerId);
                        if (player != null)
                        {
                            int slot = player.inventory.holdingItemIdx;
                            ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                            if (itemValue != null && InfiniteAmmo.IsEnabled && itemValue.ItemClass.IsGun() && InfiniteAmmo.Exec(cInfo, player, slot, itemValue))
                            {
                                return false;
                            }
                            if (BlockLogger.IsEnabled || DamageDetector.IsEnabled || POIProtection.IsEnabled)
                            {
                                World world = __instance.World;
                                for (int i = 0; i < _blocksToChange.Count; i++)
                                {
                                    BlockChangeInfo newBlockInfo = _blocksToChange[i];//new block info
                                    BlockValue oldBlockValue = world.GetBlock(newBlockInfo.pos);//old block value
                                    Block oldBlock = oldBlockValue.Block;
                                    if (newBlockInfo != null && newBlockInfo.bChangeBlockValue)//has new block value
                                    {
                                        Block newBlock = newBlockInfo.blockValue.Block;
                                        if (oldBlockValue.type == BlockValue.Air.type)//old block was air
                                        {
                                            if (newBlock is BlockSleepingBag)//placed a sleeping bag
                                            {
                                                if (POIProtection.IsEnabled && POIProtection.Bed && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), 5))
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
                                            if (BlockLogger.IsEnabled && newBlockInfo.blockValue.type != BlockValue.Air.type)
                                            {
                                                BlockLogger.PlacedBlock(_persistentPlayerId, newBlock, newBlockInfo.pos);
                                            }
                                            return true;
                                        }
                                        else if (newBlockInfo.blockValue.type == BlockValue.Air.type)//new block is air
                                        {
                                            if (oldBlockValue.Block is BlockLandClaim)
                                            {
                                                if (!PersistentOperations.ClaimedByAllyOrSelf(_persistentPlayerId, newBlockInfo.pos))
                                                {
                                                    if (DamageDetector.IsEnabled)
                                                    {
                                                        int total = oldBlock.MaxDamage - oldBlockValue.damage;
                                                        if (oldBlock.MaxDamage - oldBlockValue.damage >= DamageDetector.Block_Damage_Limit &&
                                                            GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.playerId) > Admin_Level)
                                                        {
                                                            Penalty(total, _persistentPlayerId, cInfo);
                                                            return false;
                                                        }
                                                    }
                                                    if (BlockLogger.IsEnabled)
                                                    {
                                                        BlockLogger.BrokeBlock(_persistentPlayerId, oldBlock, newBlockInfo.pos);
                                                    }
                                                }
                                                if (BlockLogger.IsEnabled)
                                                {
                                                    BlockLogger.RemovedBlock(_persistentPlayerId, oldBlock, newBlockInfo.pos);
                                                }
                                            }
                                            if (!oldBlock.CanPickup)//old block can not be picked up
                                            {
                                                if (!PersistentOperations.ClaimedByAllyOrSelf(_persistentPlayerId, newBlockInfo.pos))
                                                {
                                                    if (DamageDetector.IsEnabled)
                                                    {
                                                        int total = oldBlock.MaxDamage - oldBlockValue.damage;
                                                        if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.playerId) > Admin_Level)
                                                        {
                                                            Penalty(total, _persistentPlayerId, cInfo);
                                                            return false;
                                                        }
                                                    }
                                                }
                                                if (BlockLogger.IsEnabled)
                                                {
                                                    BlockLogger.RemovedBlock(_persistentPlayerId, oldBlock, newBlockInfo.pos);
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
                                                    if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.playerId) > Admin_Level)
                                                    {
                                                        Penalty(total, _persistentPlayerId, cInfo);
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
                                                    BlockLogger.DowngradedBlock(_persistentPlayerId, oldBlock, newBlock, newBlockInfo.pos);
                                                }
                                                return true;
                                            }
                                            if (DamageDetector.IsEnabled)
                                            {
                                                int total = oldBlock.MaxDamage - oldBlockValue.damage + newBlockInfo.blockValue.damage;
                                                if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.playerId) > Admin_Level)
                                                {
                                                    Penalty(total, _persistentPlayerId, cInfo);
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
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BlockChange.ProcessBlockChange: {0}", e.Message));
            }
            return true;
        }

        private static void Penalty(int _total, string _persistentPlayerId, ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
                if (_player != null)
                {
                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(_persistentPlayerId) > Admin_Level)
                    {
                        Phrases.Dict.TryGetValue("DamageDetector3", out string _phrase);
                        _phrase = _phrase.Replace("{Value}", _total.ToString());
                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase), null);
                        using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("Detected \"{0}\" Steam id {1} using {2} that exceeded the damage limit @ {3}. Damage recorded: {4}", _cInfo.playerName, _persistentPlayerId, _player.inventory.holdingItem.GetLocalizedItemName() ?? _player.inventory.holdingItem.GetItemName(), _player.position, _total));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Phrases.Dict.TryGetValue("DamageDetector1", out _phrase);
                        _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BlockChange.BlockPenalty: {0}", e.Message));
            }
        }
    }
}

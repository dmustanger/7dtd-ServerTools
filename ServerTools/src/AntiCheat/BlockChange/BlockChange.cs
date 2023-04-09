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

        public static void ProcessBlockChange(GameManager __instance, PlatformUserIdentifierAbs _persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
        {
            try
            {
                if (__instance == null || _persistentPlayerId == null || _blocksToChange == null)
                {
                    return;
                }
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromUId(_persistentPlayerId);
                if (cInfo == null)
                {
                    return;
                }
                EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                if (player == null)
                {
                    return;
                }
                World world = __instance.World;
                int blockChanges = _blocksToChange.Count;
                for (int i = 0; i < blockChanges; i++)
                {
                    BlockChangeInfo newBlockInfo = _blocksToChange[i];//new block info
                    BlockValue oldBlockValue = world.GetBlock(newBlockInfo.pos);//old block value
                    Block oldBlock = oldBlockValue.Block;
                    if (newBlockInfo == null || !newBlockInfo.bChangeBlockValue)//no new block value
                    {
                        continue;
                    }
                    if (world.GetChunkFromWorldPos(newBlockInfo.pos) == null)//chunk is null
                    {
                        continue;
                    }
                    Block newBlock = newBlockInfo.blockValue.Block;
                    if (!oldBlockValue.Equals(newBlockInfo.blockValue) && oldBlockValue.Equals(BlockValue.Air))
                    {
                        if (newBlock is BlockSleepingBag)//placed a sleeping bag
                        {
                            if (POIProtection.IsEnabled && POIProtection.Bed && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), POIProtection.Extra_Distance))
                            {
                                GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                                POIProtection.ReturnBed(cInfo, newBlockInfo.pos, newBlock.GetBlockName());
                                continue;
                            }
                        }
                        else if (newBlock is BlockLandClaim)//placed a land claim
                        {
                            if (POIProtection.IsEnabled && POIProtection.Claim && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), POIProtection.Extra_Distance))
                            {
                                GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                                POIProtection.ReturnClaim(cInfo, newBlockInfo.pos, newBlock.GetBlockName());
                                continue;
                            }
                        }
                        if (BlockLogger.IsEnabled)
                        {
                            BlockLogger.PlacedBlock(cInfo, newBlock, newBlockInfo.pos);
                        }
                        if (Wall.IsEnabled && Wall.WallEnabled.Contains(cInfo.entityId))
                        {
                            Wall.BuildWall(cInfo, newBlockInfo.pos, newBlockInfo.blockValue);
                        }
                    }
                    if (oldBlockValue.Block is BlockTrapDoor)
                    {
                        continue;
                    }
                    else if (newBlockInfo.blockValue.Equals(BlockValue.Air))//new block is air
                    {
                        if (oldBlockValue.Block is BlockLandClaim)
                        {
                            if (GeneralOperations.ClaimedByWho(_persistentPlayerId, newBlockInfo.pos) != EnumLandClaimOwner.None)
                            {
                                if (DamageDetector.IsEnabled)
                                {
                                    int total = oldBlock.MaxDamage - oldBlockValue.damage;
                                    if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                        GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                                    {
                                        GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, oldBlockValue);
                                        DamagePenalty(total, player, cInfo);
                                        continue;
                                    }
                                }
                                if (BlockLogger.IsEnabled)
                                {
                                    BlockLogger.BrokeBlock(cInfo, oldBlock, newBlockInfo.pos);
                                }
                                if (InfiniteAmmo.IsEnabled)
                                {
                                    int slot = player.inventory.holdingItemIdx;
                                    ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                                    if (itemValue != null && itemValue.ItemClass.IsGun())
                                    {
                                        InfiniteAmmo.Exec(cInfo, player, slot, itemValue);
                                    }
                                }
                            }
                            else if (BlockLogger.IsEnabled)
                            {
                                BlockLogger.RemovedBlock(cInfo, oldBlock, newBlockInfo.pos);
                            }
                        }
                        else if (!oldBlock.CanPickup)//old block can not be picked up
                        {
                            if (DamageDetector.IsEnabled)
                            {
                                int total = oldBlock.MaxDamage - oldBlockValue.damage;
                                if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                    GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                                {
                                    GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, oldBlockValue);
                                    DamagePenalty(total, player, cInfo);
                                    continue;
                                }
                            }
                            if (BlockLogger.IsEnabled)
                            {
                                BlockLogger.BrokeBlock(cInfo, oldBlock, newBlockInfo.pos);
                            }
                            if (InfiniteAmmo.IsEnabled)
                            {
                                int slot = player.inventory.holdingItemIdx;
                                ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                                if (itemValue != null && itemValue.ItemClass.IsGun())
                                {
                                    InfiniteAmmo.Exec(cInfo, player, slot, itemValue);
                                }
                            }
                        }
                    }
                    else if (oldBlock.blockID == newBlock.blockID)//block is the same
                    {
                        if (!newBlockInfo.bChangeDamage)//block took damage
                        {
                            continue;
                        }
                        if (BlockPickup.IsEnabled && BlockPickup.PickupEnabled.Contains(player.entityId) &&
                            player.inventory.holdingItemItemValue.GetItemId() == GeneralOperations.MeleeHandPlayer)
                        {
                            if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) <= BlockPickup.Admin_Level ||
                                GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) <= BlockPickup.Admin_Level)
                            {
                                GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                                GeneralOperations.ReturnBlock(cInfo, oldBlock.GetBlockName(), 1, "Pickup5");
                                continue;
                            }
                            if (!world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), 2))
                            {
                                if (GeneralOperations.ClaimedByWho(cInfo.CrossplatformId, new Vector3i(player.position)) != EnumLandClaimOwner.Self)
                                {
                                    Phrases.Dict.TryGetValue("Pickup3", out string phrase);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    continue;
                                }
                                TileEntity tileEntity = GameManager.Instance.World.GetTileEntity(0, newBlockInfo.pos);
                                if (tileEntity != null && tileEntity is TileEntitySecureDoor)
                                {
                                    GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                                    GeneralOperations.ReturnBlock(cInfo, oldBlock.GetBlockName(), 1, "Pickup5");
                                    continue;
                                }
                                if (oldBlock.shape.IsTerrain() || oldBlock.IsTerrainDecoration || oldBlock.IsPlant() || oldBlock.isMultiBlock
                                    || oldBlockValue.ischild || oldBlock.shape is BlockShapeModelEntity || oldBlock.shape is BlockShapeWater ||
                                    oldBlock is BlockWorkstation)
                                {
                                    Phrases.Dict.TryGetValue("Pickup7", out string phrase);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    continue;
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Pickup9", out string phrase);
                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                continue;
                            }
                            
                            if (oldBlockValue.damage != 0)
                            {
                                Phrases.Dict.TryGetValue("Pickup4", out string phrase);
                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                continue;
                            }
                            GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                            GeneralOperations.ReturnBlock(cInfo, oldBlock.GetBlockName(), 1, "Pickup5");
                            continue;
                        }
                        if (DamageDetector.IsEnabled)
                        {
                            int total = newBlockInfo.blockValue.damage - oldBlockValue.damage;
                            if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                            {
                                GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, oldBlockValue);
                                DamagePenalty(total, player, cInfo);
                                continue;
                            }
                        }
                        if (InfiniteAmmo.IsEnabled)
                        {
                            int slot = player.inventory.holdingItemIdx;
                            ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                            if (itemValue != null && itemValue.ItemClass.IsGun())
                            {
                                InfiniteAmmo.Exec(cInfo, player, slot, itemValue);
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
                            continue;
                        }
                        if (DamageDetector.IsEnabled)
                        {
                            int total = oldBlock.MaxDamage - oldBlockValue.damage + newBlockInfo.blockValue.damage;
                            if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                            {
                                GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, oldBlockValue);
                                DamagePenalty(total, player, cInfo);
                                continue;
                            }
                        }
                        if (InfiniteAmmo.IsEnabled)
                        {
                            int slot = player.inventory.holdingItemIdx;
                            ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                            if (itemValue != null && itemValue.ItemClass.IsGun())
                            {
                                InfiniteAmmo.Exec(cInfo, player, slot, itemValue);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BlockChange.ProcessBlockChange: {0}", e.Message));
            }
        }

        private static void DamagePenalty(int _total, EntityPlayer _player, ClientInfo _cInfo)
        {
            try
            {
                Phrases.Dict.TryGetValue("DamageDetector3", out string phrase);
                phrase = phrase.Replace("{Value}", _total.ToString());
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' using '{3}' that exceeded the damage limit @ '{4}'. Damage value '{5}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _player.inventory.holdingItem.GetLocalizedItemName() ?? _player.inventory.holdingItem.GetItemName(), _player.position, _total));
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

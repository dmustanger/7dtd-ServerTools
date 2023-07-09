using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class BlockChange
    {
        public static int Admin_Level = 0;

        private static World world;
        private static ClientInfo cInfo;
        private static EntityPlayer player;
        private static BlockChangeInfo newBlockInfo;
        private static BlockValue oldBlockValue, newBlockValue;
        private static Block oldBlock, newBlock;
        private static Chunk chunk;
        private static TileEntity tileEntity;
        private static BlockShape shape;
        private static ItemValue heldItem;
        private static EnumLandClaimOwner claimOwner;

        private static readonly string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string DetectionFilepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);

        public static void ProcessBlockChange(GameManager __instance, List<BlockChangeInfo> _blocksToChange, int ___localPlayerThatChanged)
        {
            try
            {
                if (__instance == null || _blocksToChange == null || ___localPlayerThatChanged == -1)
                {
                    return;
                }
                cInfo = GeneralOperations.GetClientInfoFromEntityId(___localPlayerThatChanged);
                if (cInfo == null)
                {
                    return;
                }
                player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                if (player == null)
                {
                    return;
                }
                int slot = cInfo.latestPlayerData.selectedInventorySlot;
                heldItem = cInfo.latestPlayerData.inventory[slot].itemValue;
                if (heldItem == null || heldItem.IsEmpty() || heldItem.ItemClass.CreativeMode == EnumCreativeMode.Dev)
                {
                    return;
                }
                world = __instance.World;
                int blockChanges = _blocksToChange.Count;
                for (int i = 0; i < blockChanges; i++)
                {
                    newBlockInfo = _blocksToChange[i];
                    chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(newBlockInfo.pos);
                    if (chunk != null && newBlockInfo != null)
                    {
                        oldBlockValue = world.GetBlock(newBlockInfo.pos);
                        oldBlock = oldBlockValue.Block;
                        shape = oldBlock.shape;
                        if (world.GetChunkFromWorldPos(newBlockInfo.pos) == null || shape == null || shape.IsTerrain() || oldBlock is BlockTrapDoor)
                        {
                            continue;
                        }
                        if (newBlockInfo.bChangeBlockValue)
                        {
                            newBlockValue = newBlockInfo.blockValue;
                            newBlock = newBlockValue.Block;
                            if (newBlockInfo.bChangeDamage)
                            {
                                if (BlockPickup.IsEnabled && BlockPickup.PickupEnabled.Contains(player.entityId) &&
                                    heldItem.IsEmpty() && oldBlock.Damage == 0)
                                {
                                    if (GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) <= BlockPickup.Admin_Level ||
                                    GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) <= BlockPickup.Admin_Level)
                                    {
                                        ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                        {
                                            ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, BlockValue.Air));
                                        }, null);
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
                                        tileEntity = GameManager.Instance.World.GetTileEntity(0, newBlockInfo.pos);
                                        if (tileEntity != null && tileEntity is TileEntitySecureDoor)
                                        {
                                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                            {
                                                ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, BlockValue.Air));
                                            }, null);
                                            GeneralOperations.ReturnBlock(cInfo, oldBlock.GetBlockName(), 1, "Pickup5");
                                            continue;
                                        }
                                        if (shape.IsTerrain() || oldBlock.IsTerrainDecoration || oldBlock.IsPlant() || oldBlock.isMultiBlock
                                            || oldBlockValue.ischild || shape is BlockShapeModelEntity || shape is BlockShapeWater ||
                                            oldBlock is BlockWorkstation)
                                        {
                                            Phrases.Dict.TryGetValue("Pickup7", out string phrase);
                                            ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            continue;
                                        }
                                        ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                        {
                                            ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, BlockValue.Air));
                                        }, null);
                                        GeneralOperations.ReturnBlock(cInfo, oldBlock.GetBlockName(), 1, "Pickup5");
                                        continue;
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue("Pickup9", out string phrase);
                                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        continue;
                                    }
                                }
                                if (InfiniteAmmo.IsEnabled && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                    GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level && heldItem.ItemClass.IsGun())
                                {
                                    InfiniteAmmo.Exec(cInfo, slot, heldItem);
                                }
                                if (DamageDetector.IsEnabled)
                                {
                                    claimOwner = GeneralOperations.ClaimedByWho(cInfo.CrossplatformId, new Vector3i(player.position));
                                    if (claimOwner != EnumLandClaimOwner.None && claimOwner != EnumLandClaimOwner.Self)
                                    {
                                        int total = newBlockValue.damage - (int)oldBlock.Damage;
                                        if (total >= DamageDetector.Claimed_Block_Limit && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                            GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                                        {
                                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                            {
                                                ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                            }, null);
                                            DamagePenalty(total, player, cInfo);
                                            continue;
                                        }
                                        if (oldBlock.MaxDamage >= 200)
                                        {
                                            if (DamageDetector.DamagedBlockId.ContainsKey(newBlockInfo.pos))
                                            {
                                                if (DamageDetector.DamagedBlockId.TryGetValue(newBlockInfo.pos, out int id))
                                                {
                                                    if (___localPlayerThatChanged != id)
                                                    {
                                                        DamageDetector.DamagedBlockId[newBlockInfo.pos] = ___localPlayerThatChanged;
                                                        DamageDetector.BrokenBlockTime[newBlockInfo.pos] = DateTime.Now.AddSeconds(oldBlock.MaxDamage - oldBlockValue.damage / 200);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                DamageDetector.DamagedBlockId.Add(newBlockInfo.pos, ___localPlayerThatChanged);
                                                DamageDetector.BrokenBlockTime.Add(newBlockInfo.pos, DateTime.Now.AddSeconds(oldBlock.MaxDamage - oldBlockValue.damage / 200));
                                            }
                                        }
                                    }
                                    else if (claimOwner == EnumLandClaimOwner.None)
                                    {
                                        int total = newBlockValue.damage - (int)oldBlock.Damage;
                                        if (total >= DamageDetector.Block_Limit && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                            GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                                        {
                                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                            {
                                                ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                            }, null);
                                            DamagePenalty(total, player, cInfo);
                                            continue;
                                        }
                                    }
                                }
                                if (oldBlock.DowngradeBlock.Block.blockID == newBlock.blockID && !newBlockValue.isair)//downgraded
                                {
                                    if (BlockLogger.IsEnabled)
                                    {
                                        BlockLogger.DowngradedBlock(cInfo, oldBlock, newBlock, newBlockInfo.pos);
                                    }
                                }
                                else if (oldBlock.UpgradeBlock.Block.blockID == newBlock.blockID)//upgraded
                                {
                                    if (BlockLogger.IsEnabled)
                                    {
                                        BlockLogger.UpgradedBlock(cInfo, oldBlock, newBlock, newBlockInfo.pos);
                                    }
                                }
                            }
                            else if (!oldBlockValue.Block.CanPickup)
                            {
                                if (oldBlockValue.isair && !newBlockValue.isair)//Added new block
                                {
                                    if (newBlock is BlockSleepingBag)
                                    {
                                        if (POIProtection.IsEnabled && POIProtection.Bed && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), POIProtection.Extra_Distance))
                                        {
                                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                            {
                                                ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                            }, null);
                                            POIProtection.ReturnBed(cInfo, newBlockInfo.pos, newBlock.GetBlockName());
                                            continue;
                                        }
                                    }
                                    else if (newBlock is BlockLandClaim)
                                    {
                                        if (POIProtection.IsEnabled && POIProtection.Claim && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), POIProtection.Extra_Distance))
                                        {
                                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                            {
                                                ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                            }, null);
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
                                        Wall.BuildWall(cInfo, newBlockInfo.pos, newBlockValue);
                                    }
                                }
                                else if (!oldBlockValue.isair && newBlockValue.isair)//Removed a block
                                {
                                    if (DamageDetector.IsEnabled)
                                    {
                                        claimOwner = GeneralOperations.ClaimedByWho(cInfo.CrossplatformId, new Vector3i(player.position));
                                        if (claimOwner != EnumLandClaimOwner.None && claimOwner != EnumLandClaimOwner.Self)
                                        {
                                            int total = oldBlock.MaxDamage - (int)oldBlock.Damage;
                                            if (total >= DamageDetector.Claimed_Block_Limit && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                                GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                                            {
                                                ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                                {
                                                    ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                                }, null);
                                                DamagePenalty(total, player, cInfo);
                                                continue;
                                            }
                                            if (DamageDetector.DamagedBlockId.ContainsKey(newBlockInfo.pos))
                                            {
                                                if (DamageDetector.BrokenBlockTime.TryGetValue(newBlockInfo.pos, out DateTime expectedTime) && DateTime.Now < expectedTime)
                                                {
                                                    ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                                    {
                                                        ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                                    }, null);
                                                    TimePenalty(player, cInfo, heldItem);
                                                }
                                                DamageDetector.DamagedBlockId.Remove(newBlockInfo.pos);
                                                DamageDetector.BrokenBlockTime.Remove(newBlockInfo.pos);
                                            }
                                        }
                                        else if (claimOwner == EnumLandClaimOwner.None)
                                        {
                                            int total = oldBlock.MaxDamage - (int)oldBlock.Damage;
                                            if (total >= DamageDetector.Block_Limit && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                                GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                                            {
                                                ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                                {
                                                    ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                                }, null);
                                                DamagePenalty(total, player, cInfo);
                                                continue;
                                            }
                                            if (DamageDetector.DamagedBlockId.ContainsKey(newBlockInfo.pos))
                                            {
                                                DamageDetector.DamagedBlockId.Remove(newBlockInfo.pos);
                                                DamageDetector.BrokenBlockTime.Remove(newBlockInfo.pos);
                                            }
                                        }
                                    }
                                    if (BlockLogger.IsEnabled)
                                    {
                                        BlockLogger.RemovedBlock(cInfo, oldBlock, newBlockInfo.pos);
                                    }
                                }
                                else if (oldBlock is BlockDoor && oldBlockValue.meta2and1 != newBlockValue.meta2and1)
                                {
                                    BlockLogger.DoorOpenClose(cInfo, oldBlock, newBlockInfo.pos);
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
        }

        public static IEnumerator SetBlock(Vector3i _position, BlockValue _blockValue)
        {
            try
            {
                if (_position != null)
                {
                    GameManager.Instance.World.SetBlockRPC(_position, _blockValue);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProcessBlockChange.SetBlock: {0}", e.StackTrace));
            }
            yield break;
        }

        private static void DamagePenalty(int _total, EntityPlayer _player, ClientInfo _cInfo)
        {
            try
            {
                Phrases.Dict.TryGetValue("DamageDetector2", out string phrase);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' using '{3}' that exceeded the damage limit @ '{4}'. Damage value '{5}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _player.inventory.holdingItem.GetLocalizedItemName() ?? _player.inventory.holdingItem.GetItemName(), _player.serverPos, _total));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in BlockChange.DamagePenalty: {0}", e.Message));
            }
        }

        private static void TimePenalty(EntityPlayer _player, ClientInfo _cInfo, ItemValue _heldItem)
        {
            try
            {
                Phrases.Dict.TryGetValue("DamageDetector2", out string phrase);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' using '{3}' that broke a block @ '{4}' too fast", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _heldItem.ItemClass.GetLocalizedItemName() ?? _heldItem.ItemClass.GetItemName(), _player.serverPos));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in BlockChange.TimePenalty: {0}", e.Message));
            }
        }
    }
}

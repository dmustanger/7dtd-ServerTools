using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class BlockChange
    {

        public static void ProcessBlockChange(GameManager __instance, ref List<BlockChangeInfo> _blocksToChange, int ___localPlayerThatChanged)
        {
            try
            {
                if (__instance == null || _blocksToChange == null || ___localPlayerThatChanged == -1)
                {
                    return;
                }
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(___localPlayerThatChanged);
                if (cInfo == null || cInfo.latestPlayerData == null || cInfo.latestPlayerData.inventory == null)
                {
                    return;
                }
                EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                if (player == null)
                {
                    return;
                }
                World world = __instance.World;
                BlockChangeInfo newBlockInfo;
                BlockValue oldBlockValue, newBlockValue;
                Block oldBlock, newBlock;
                TileEntity tileEntity;
                EnumLandClaimOwner claimOwner;
                for (int i = 0; i < _blocksToChange.Count; i++)
                {
                    newBlockInfo = _blocksToChange[i];
                    if (newBlockInfo == null)
                    {
                        continue;
                    }
                    oldBlockValue = world.GetBlock(newBlockInfo.pos);
                    oldBlock = oldBlockValue.Block;
                    if (oldBlock is BlockTrapDoor)
                    {
                        continue;
                    }
                    if (newBlockInfo.bChangeBlockValue)
                    {
                        newBlockValue = newBlockInfo.blockValue;
                        newBlock = newBlockValue.Block;
                        if (newBlockInfo.bChangeDamage)
                        {
                            if (ProtectedZones.IsEnabled && ProtectedZones.ProtectedList.Count > 0)
                            {
                                if (ProtectedZones.IsProtected(newBlockInfo.pos))
                                {
                                    _blocksToChange[i].blockValue = oldBlockValue;
                                    _blocksToChange[i].bChangeDamage = false;
                                    Phrases.Dict.TryGetValue("ProtectedZone1", out string phrase);
                                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(phrase, string.Empty));
                                    continue;
                                }
                            }
                            if (BlockPickup.IsEnabled && BlockPickup.PickupEnabled.Contains(player.entityId) &&
                                cInfo.latestPlayerData.inventory[cInfo.latestPlayerData.selectedInventorySlot].IsEmpty() && oldBlock.Damage <= 10)
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
                                    if (oldBlock.shape.IsTerrain() || oldBlock.IsTerrainDecoration || oldBlock.IsPlant() || oldBlock.isMultiBlock
                                        || oldBlockValue.ischild || oldBlock.shape is BlockShapeModelEntity || oldBlockValue.isWater ||
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
                            if (InfiniteAmmo.IsEnabled && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > InfiniteAmmo.Admin_Level &&
                                GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > InfiniteAmmo.Admin_Level)
                            {
                                ItemStack heldItem = cInfo.latestPlayerData.inventory[cInfo.latestPlayerData.selectedInventorySlot];
                                if (!heldItem.IsEmpty() && heldItem.itemValue.ItemClass.IsGun())
                                {
                                    InfiniteAmmo.Exec(cInfo, cInfo.latestPlayerData.selectedInventorySlot, heldItem.itemValue);
                                }
                            }
                            if (DamageDetector.IsEnabled)
                            {
                                claimOwner = GeneralOperations.ClaimedByWho(cInfo.CrossplatformId, new Vector3i(player.position));
                                if (claimOwner != EnumLandClaimOwner.None && claimOwner != EnumLandClaimOwner.Self)
                                {
                                    int total = newBlockValue.damage - (int)oldBlock.Damage;
                                    if (total >= DamageDetector.Claimed_Block_Limit && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > DamageDetector.Admin_Level &&
                                        GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > DamageDetector.Admin_Level)
                                    {
                                        ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                        {
                                            ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                        }, null);
                                        DamagePenalty(total, player, cInfo);
                                        continue;
                                    }
                                }
                                else if (claimOwner == EnumLandClaimOwner.None)
                                {
                                    int total = newBlockValue.damage - (int)oldBlock.Damage;
                                    if (total >= DamageDetector.Block_Limit && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > DamageDetector.Admin_Level &&
                                        GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > DamageDetector.Admin_Level)
                                    {
                                        ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                        {
                                            ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                        }, null);
                                        DamagePenalty(total, player, cInfo);
                                        continue;
                                    }
                                }
                                if (GeneralOperations.LandClaimDurability >= 4 && oldBlock.MaxDamage >= 200)
                                {
                                    if (DamageDetector.DamagedBlockId.ContainsKey(newBlockInfo.pos))
                                    {
                                        if (DamageDetector.DamagedBlockId.TryGetValue(newBlockInfo.pos, out int id))
                                        {
                                            if (___localPlayerThatChanged != id)
                                            {
                                                DamageDetector.DamagedBlockId[newBlockInfo.pos] = ___localPlayerThatChanged;
                                                DamageDetector.BrokenBlockTime[newBlockInfo.pos] = DateTime.Now.AddSeconds(oldBlock.MaxDamage - newBlockValue.damage / 200);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        DamageDetector.DamagedBlockId.Add(newBlockInfo.pos, ___localPlayerThatChanged);
                                        DamageDetector.BrokenBlockTime.Add(newBlockInfo.pos, DateTime.Now.AddSeconds(oldBlock.MaxDamage - newBlockValue.damage / 200));
                                    }
                                }
                            }
                        }
                        else if (!oldBlockValue.Block.CanPickup)
                        {
                            if (oldBlock.blockID != newBlock.blockID)
                            {
                                if (!newBlockValue.isair)//Added or altered block
                                {
                                    if (POIProtection.IsEnabled)
                                    {
                                        if (newBlock is BlockSleepingBag && POIProtection.Bed && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), POIProtection.Extra_Distance))
                                        {
                                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                            {
                                                ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                            }, null);
                                            POIProtection.ReturnBed(cInfo, newBlock.GetBlockName());
                                            continue;
                                        }
                                        else if (newBlock is BlockLandClaim && POIProtection.Claim && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), POIProtection.Extra_Distance))
                                        {
                                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                            {
                                                ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                            }, null);
                                            POIProtection.ReturnClaim(cInfo, newBlock.GetBlockName());
                                            continue;
                                        }
                                    }
                                    if (Wall.IsEnabled && Wall.WallEnabled.Contains(cInfo.entityId))
                                    {
                                        Wall.BuildWall(cInfo, newBlockInfo.pos, newBlockValue);
                                    }
                                    if (BlockLogger.IsEnabled)
                                    {
                                        if (oldBlock.DowngradeBlock.Block.blockID == newBlock.blockID)
                                        {
                                            BlockLogger.DowngradedBlock(cInfo, oldBlock, newBlock, newBlockInfo.pos);
                                        }
                                        else if (oldBlock.UpgradeBlock.Block.blockID == newBlock.blockID)
                                        {
                                            BlockLogger.UpgradedBlock(cInfo, oldBlock, newBlock, newBlockInfo.pos);
                                        }
                                        else
                                        {
                                            BlockLogger.PlacedBlock(cInfo, newBlock, newBlockInfo.pos);
                                        }
                                    }
                                }
                                else if (!oldBlockValue.isair && !oldBlockValue.isWater && !oldBlock.shape.IsTerrain())//Removed a block
                                {
                                    if (ProtectedZones.IsEnabled && ProtectedZones.ProtectedList.Count > 0)
                                    {
                                        if (GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > ProtectedZones.Admin_Level &&
                                            GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > ProtectedZones.Admin_Level &&
                                            ProtectedZones.IsProtected(newBlockInfo.pos))
                                        {
                                            oldBlockValue.damage = 0;
                                            _blocksToChange[i].blockValue = oldBlockValue;
                                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                            {
                                                ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                            }, null);
                                            Phrases.Dict.TryGetValue("ProtectedZone1", out string phrase);
                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(phrase, string.Empty));
                                            continue;
                                        }
                                    }
                                    if (DamageDetector.IsEnabled)
                                    {
                                        int limit = 0;
                                        bool claimed = true;
                                        claimOwner = GeneralOperations.ClaimedByWho(cInfo.CrossplatformId, new Vector3i(player.position));
                                        if (claimOwner != EnumLandClaimOwner.None && claimOwner != EnumLandClaimOwner.Self)
                                        {
                                            limit = DamageDetector.Claimed_Block_Limit;
                                        }
                                        else if (claimOwner == EnumLandClaimOwner.None)
                                        {
                                            limit = DamageDetector.Block_Limit;
                                            claimed = false;
                                        }
                                        int total = oldBlock.MaxDamage - (int)oldBlock.Damage;
                                        if (total >= limit && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > DamageDetector.Admin_Level &&
                                            GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > DamageDetector.Admin_Level)
                                        {
                                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                            {
                                                ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                            }, null);
                                            DamagePenalty(total, player, cInfo);
                                            continue;
                                        }
                                        if (GeneralOperations.LandClaimDurability >= 4 && oldBlock.MaxDamage >= 200 && DamageDetector.DamagedBlockId.ContainsKey(newBlockInfo.pos) &&
                                            DamageDetector.DamagedBlockId.TryGetValue(newBlockInfo.pos, out int id))
                                        {
                                            if (claimed && ___localPlayerThatChanged == id)
                                            {
                                                if (DamageDetector.BrokenBlockTime.TryGetValue(newBlockInfo.pos, out DateTime time) && DateTime.Now < time)
                                                {
                                                    ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                                                    {
                                                        ThreadManager.StartCoroutine(SetBlock(newBlockInfo.pos, oldBlockValue));
                                                    }, null);
                                                    TimePenalty(player, cInfo, cInfo.latestPlayerData.inventory[cInfo.latestPlayerData.selectedInventorySlot].IsEmpty() ? ItemValue.None : cInfo.latestPlayerData.inventory[cInfo.latestPlayerData.selectedInventorySlot].itemValue);
                                                }
                                            }
                                            DamageDetector.DamagedBlockId.Remove(newBlockInfo.pos);
                                            DamageDetector.BrokenBlockTime.Remove(newBlockInfo.pos);
                                        }
                                    }
                                    if (BlockLogger.IsEnabled)
                                    {
                                        BlockLogger.RemovedBlock(cInfo, oldBlock, newBlockInfo.pos);
                                    }
                                }
                            }
                            else if (oldBlock is BlockDoor && oldBlockValue.meta2and1 != newBlockValue.meta2and1)
                            {
                                if (BlockLogger.IsEnabled)
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
                Log.Out("[SERVERTOOLS] Error in BlockChange.ProcessBlockChange: {0}", e.Message);
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
                Log.Out("[SERVERTOOLS] Error in ProcessBlockChange.SetBlock: {0}", e.StackTrace);
            }
            yield break;
        }

        private static void DamagePenalty(int _total, EntityPlayer _player, ClientInfo _cInfo)
        {
            try
            {
                Phrases.Dict.TryGetValue("DamageDetector1", out string phrase);
                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                Phrases.Dict.TryGetValue("DamageDetector2", out phrase);
                phrase = phrase.Replace("{Value}", _total.ToString());
                //SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                using (StreamWriter sw = new StreamWriter(DamageDetector.DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' using '{3}' that exceeded the damage limit @ '{4}'. Damage value '{5}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _player.inventory.holdingItem.GetLocalizedItemName() ?? _player.inventory.holdingItem.GetItemName(), _player.serverPos.ToVector3() / 32f, _total));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                GeneralOperations.BanPlayer(_cInfo, phrase);
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in BlockChange.DamagePenalty: {0}", e.Message);
            }
        }

        private static void TimePenalty(EntityPlayer _player, ClientInfo _cInfo, ItemValue _heldItem)
        {
            try
            {
                Phrases.Dict.TryGetValue("DamageDetector1", out string phrase);
                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                Phrases.Dict.TryGetValue("DamageDetector2", out phrase);
                //SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
                using (StreamWriter sw = new StreamWriter(DamageDetector.DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' using '{3}' that broke a block @ '{4}' too fast", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _heldItem.ItemClass.GetLocalizedItemName() ?? _heldItem.ItemClass.GetItemName(), _player.serverPos.ToVector3() / 32f));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                GeneralOperations.BanPlayer(_cInfo, phrase);
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in BlockChange.TimePenalty: {0}", e.Message);
            }
        }
    }
}

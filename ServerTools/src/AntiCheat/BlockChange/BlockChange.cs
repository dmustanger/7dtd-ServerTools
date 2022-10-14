using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    ClientInfo cInfo = GeneralFunction.GetClientInfoFromUId(_persistentPlayerId);
                    if (cInfo != null)
                    {
                        EntityPlayer player = GeneralFunction.GetEntityPlayer(cInfo.entityId);
                        if (player != null)
                        {
                            World world = __instance.World;
                            for (int i = 0; i < _blocksToChange.Count; i++)
                            {
                                BlockChangeInfo newBlockInfo = _blocksToChange[i];//new block info
                                BlockValue oldBlockValue = world.GetBlock(newBlockInfo.pos);//old block value
                                Block oldBlock = oldBlockValue.Block;
                                if (newBlockInfo != null && oldBlock != null && newBlockInfo.bChangeBlockValue)//has new block value
                                {
                                    Block newBlock = newBlockInfo.blockValue.Block;
                                    if (newBlock != null && !oldBlockValue.Equals(newBlockInfo.blockValue) && oldBlockValue.Equals(BlockValue.Air))
                                    {
                                        if (newBlock is BlockSleepingBag)//placed a sleeping bag
                                        {
                                            if (POIProtection.IsEnabled && POIProtection.Bed && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), POIProtection.Extra_Distance))
                                            {
                                                GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                                                GeneralFunction.ReturnBlock(cInfo, newBlock.GetBlockName(), 1, "GiveItem1");
                                                Phrases.Dict.TryGetValue("POI1", out string phrase);
                                                phrase = phrase.Replace("{Distance}", POIProtection.Extra_Distance.ToString());
                                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return false;
                                            }
                                        }
                                        else if (newBlock is BlockLandClaim)//placed a land claim
                                        {
                                            if (POIProtection.IsEnabled && POIProtection.Claim && world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), POIProtection.Extra_Distance))
                                            {
                                                GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                                                GeneralFunction.ReturnBlock(cInfo, newBlock.GetBlockName(), 1, "GiveItem1");
                                                Phrases.Dict.TryGetValue("POI2", out string phrase);
                                                phrase = phrase.Replace("{Distance}", POIProtection.Extra_Distance.ToString());
                                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return false;
                                            }
                                        }
                                        if (BlockLogger.IsEnabled)
                                        {
                                            BlockLogger.PlacedBlock(cInfo, newBlock, newBlockInfo.pos);
                                        }
                                        if (Wall.IsEnabled && Wall.WallEnabled.ContainsKey(cInfo.entityId))
                                        {
                                            if (newBlock.GetBlockName().ToLower().Contains("cube"))
                                            {
                                                Wall.WallEnabled.TryGetValue(cInfo.entityId, out Vector3i vector);
                                                if (GeneralFunction.ClaimedByWho(_persistentPlayerId, newBlockInfo.pos) == EnumLandClaimOwner.Self)
                                                {
                                                    if (vector.Equals(Vector3i.zero))
                                                    {
                                                        Wall.WallEnabled[cInfo.entityId] = newBlockInfo.pos;
                                                    }
                                                    else
                                                    {
                                                        Wall.WallEnabled.Remove(cInfo.entityId);
                                                        Wall.BuildWall(cInfo, player, vector, newBlockInfo.pos, newBlockInfo.blockValue);
                                                    }
                                                }
                                                else
                                                {
                                                    Phrases.Dict.TryGetValue("Wall4", out string phrase);
                                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                }
                                            }
                                            else
                                            {
                                                Phrases.Dict.TryGetValue("Wall5", out string phrase);
                                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                    if (oldBlockValue.Block is BlockTrapDoor)
                                    {
                                        return true;
                                    }
                                    else if (newBlockInfo.blockValue.Equals(BlockValue.Air.type))//new block is air
                                    {
                                        if (oldBlockValue.Block is BlockLandClaim)
                                        {
                                            if (GeneralFunction.ClaimedByWho(_persistentPlayerId, newBlockInfo.pos) == EnumLandClaimOwner.None)
                                            {
                                                if (BlockPickup.IsEnabled)
                                                {
                                                    if (PersistentContainer.Instance.BlockPickUp != null)
                                                    {
                                                        if (!PersistentContainer.Instance.BlockPickUp.ContainsKey(newBlockInfo.pos.ToString()))
                                                        {
                                                            PersistentContainer.Instance.BlockPickUp.Add(newBlockInfo.pos.ToString(), DateTime.Now.AddHours(24));
                                                        }
                                                        else
                                                        {
                                                            PersistentContainer.Instance.BlockPickUp[newBlockInfo.pos.ToString()] = DateTime.Now.AddHours(24);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Dictionary<string, DateTime> editBlock = new Dictionary<string, DateTime>();
                                                        editBlock.Add(newBlockInfo.pos.ToString(), DateTime.Now.AddHours(24));
                                                        PersistentContainer.Instance.BlockPickUp = editBlock;
                                                    }
                                                    PersistentContainer.DataChange = true;
                                                }
                                                if (DamageDetector.IsEnabled)
                                                {
                                                    int total = oldBlock.MaxDamage - oldBlockValue.damage;
                                                    if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                                        GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                                                    {
                                                        Penalty(total, player, cInfo);
                                                        return false;
                                                    }
                                                }
                                                if (BlockLogger.IsEnabled)
                                                {
                                                    BlockLogger.BrokeBlock(cInfo, oldBlock, newBlockInfo.pos);
                                                }
                                                int slot = player.inventory.holdingItemIdx;
                                                ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                                                if (itemValue != null && InfiniteAmmo.IsEnabled && itemValue.ItemClass.IsGun() && InfiniteAmmo.Exec(cInfo, player, slot, itemValue))
                                                {
                                                    return false;
                                                }
                                            }
                                            else if (BlockLogger.IsEnabled)
                                            {
                                                BlockLogger.RemovedBlock(cInfo, oldBlock, newBlockInfo.pos);
                                            }
                                        }
                                        else if (!oldBlock.CanPickup)//old block can not be picked up
                                        {
                                            if (GeneralFunction.ClaimedByWho(_persistentPlayerId, newBlockInfo.pos) == EnumLandClaimOwner.None)
                                            {
                                                if (DamageDetector.IsEnabled)
                                                {
                                                    int total = oldBlock.MaxDamage - oldBlockValue.damage;
                                                    if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                                        GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
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
                                            if (BlockPickup.IsEnabled && BlockPickup.PickupEnabled.Contains(player.entityId) &&
                                                player.inventory.holdingItemItemValue.GetItemId() == GeneralFunction.MeleeHandPlayer)
                                            {
                                                if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) <= BlockPickup.Admin_Level ||
                                                    GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) <= BlockPickup.Admin_Level)
                                                {
                                                    GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                                                    GeneralFunction.ReturnBlock(cInfo, oldBlock.GetBlockName(), 1, "Pickup5");
                                                    return false;
                                                }
                                                if (!world.IsPositionWithinPOI(newBlockInfo.pos.ToVector3(), 2))
                                                {
                                                    if (oldBlock.shape.IsTerrain() || oldBlock.IsTerrainDecoration || oldBlock.IsPlant() || oldBlock.isMultiBlock
                                                        || oldBlockValue.ischild || oldBlock.shape is BlockShapeModelEntity || oldBlock.shape is BlockShapeWater)
                                                    {
                                                        Phrases.Dict.TryGetValue("Pickup7", out string phrase);
                                                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                        return true;
                                                    }
                                                }
                                                else
                                                {
                                                    Phrases.Dict.TryGetValue("Pickup9", out string phrase);
                                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                    return true;
                                                }
                                                if (GeneralFunction.ClaimedByWho(cInfo.CrossplatformId, new Vector3i(player.position)) == EnumLandClaimOwner.Self)
                                                {
                                                    if (oldBlockValue.damage == 0)
                                                    {
                                                            if (PersistentContainer.Instance.BlockPickUp != null && PersistentContainer.Instance.BlockPickUp.Count > 0)
                                                            {
                                                                int claimSize = GameStats.GetInt(EnumGameStats.LandClaimSize);
                                                                List<string> vectors = PersistentContainer.Instance.BlockPickUp.Keys.ToList();
                                                                for (int j = 0; j < vectors.Count; j++)
                                                                {
                                                                    Vector3i position = Vector3i.Parse(vectors[j]);
                                                                    if (position != null)
                                                                    {
                                                                        float distance = (position.ToVector3() - player.position).magnitude;
                                                                        if (distance <= claimSize / 2)
                                                                        {
                                                                            PersistentContainer.Instance.BlockPickUp.TryGetValue(vectors[j], out DateTime time);
                                                                            if (DateTime.Now <= time)
                                                                            {
                                                                                Phrases.Dict.TryGetValue("Pickup6", out string phrase);
                                                                                phrase = phrase.Replace("{DateTime}", time.ToString());
                                                                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                                return true;
                                                                            }
                                                                            else
                                                                            {
                                                                                PersistentContainer.Instance.BlockPickUp.Remove(vectors[j]);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            GameManager.Instance.World.SetBlockRPC(newBlockInfo.pos, BlockValue.Air);
                                                            GeneralFunction.ReturnBlock(cInfo, oldBlock.GetBlockName(), 1, "Pickup5");
                                                            return false;
                                                    }
                                                    else
                                                    {
                                                        Phrases.Dict.TryGetValue("Pickup4", out string phrase);
                                                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                    }
                                                }
                                                else
                                                {
                                                    Phrases.Dict.TryGetValue("Pickup3", out string phrase);
                                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                }
                                            }
                                            if (DamageDetector.IsEnabled)
                                            {
                                                int total = newBlockInfo.blockValue.damage - oldBlockValue.damage;
                                                if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                                    GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                                                {
                                                    Penalty(total, player, cInfo);
                                                    return false;
                                                }
                                            }
                                            int slot = player.inventory.holdingItemIdx;
                                            ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                                            if (itemValue != null && InfiniteAmmo.IsEnabled && itemValue.ItemClass.IsGun() && InfiniteAmmo.Exec(cInfo, player, slot, itemValue))
                                            {
                                                return false;
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
                                            if (total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                                GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                                            {
                                                Penalty(total, player, cInfo);
                                                return false;
                                            }
                                        }
                                        int slot = player.inventory.holdingItemIdx;
                                        ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                                        if (itemValue != null && InfiniteAmmo.IsEnabled && itemValue.ItemClass.IsGun() && InfiniteAmmo.Exec(cInfo, player, slot, itemValue))
                                        {
                                            return false;
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

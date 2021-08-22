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
                if ((BlockLogger.IsEnabled || DamageDetector.IsEnabled) && __instance != null && _blocksToChange != null && !string.IsNullOrEmpty(_persistentPlayerId) &&
                    _blocksToChange != null)
                {
                    ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_persistentPlayerId);
                    if (_cInfo != null)
                    {
                        World _world = __instance.World;
                        for (int i = 0; i < _blocksToChange.Count; i++)
                        {
                            BlockChangeInfo _newBlockInfo = _blocksToChange[i];//new block info
                            BlockValue _oldBlockValue = _world.GetBlock(_newBlockInfo.pos);//old block value
                            Block _oldBlock = _oldBlockValue.Block;
                            if (_newBlockInfo != null && _newBlockInfo.bChangeBlockValue)//new block value
                            {
                                Block _newBlock = _newBlockInfo.blockValue.Block;
                                if (_oldBlockValue.type == BlockValue.Air.type)//old block was air
                                {
                                    if (_newBlock is BlockSleepingBag)//placed a sleeping bag
                                    {
                                        if (POIProtection.IsEnabled && POIProtection.Bed && _world.IsPositionWithinPOI(_newBlockInfo.pos.ToVector3(), POIProtection.Offset))
                                        {
                                            GameManager.Instance.World.SetBlockRPC(_newBlockInfo.pos, BlockValue.Air);
                                            PersistentOperations.ReturnBlock(_cInfo, _newBlock.GetBlockName(), 1);
                                            Phrases.Dict.TryGetValue("POI1", out string _phrase);
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            return false;
                                        }
                                    }
                                    else if (_newBlock is BlockLandClaim)//placed a land claim
                                    {
                                        if (POIProtection.IsEnabled && POIProtection.Claim && _world.IsPositionWithinPOI(_newBlockInfo.pos.ToVector3(), POIProtection.Offset))
                                        {
                                            GameManager.Instance.World.SetBlockRPC(_newBlockInfo.pos, BlockValue.Air);
                                            PersistentOperations.ReturnBlock(_cInfo, _newBlock.GetBlockName(), 1);
                                            Phrases.Dict.TryGetValue("POI2", out string _phrase);
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            return false;
                                        }
                                    }
                                    if (BlockLogger.IsEnabled)
                                    {
                                        BlockLogger.PlacedBlock(_persistentPlayerId, _newBlock, _newBlockInfo.pos);
                                    }
                                    return true;
                                }

                                if (_newBlockInfo.blockValue.type == BlockValue.Air.type)//new block is air
                                {
                                    if (_oldBlockValue.type == BlockValue.Air.type)
                                    {
                                        return true;
                                    }
                                    if (_oldBlockValue.Block is BlockLandClaim)
                                    {
                                        if (!PersistentOperations.ClaimedByAllyOrSelf(_persistentPlayerId, _newBlockInfo.pos))
                                        {
                                            if (DamageDetector.IsEnabled)
                                            {
                                                int _total = _oldBlock.MaxDamage - _oldBlockValue.damage;
                                                if (_oldBlock.MaxDamage - _oldBlockValue.damage >= DamageDetector.Block_Damage_Limit &&
                                                    GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.playerId) > Admin_Level)
                                                {
                                                    Penalty(_total, _persistentPlayerId, _cInfo);
                                                    return false;
                                                }
                                            }
                                            if (BlockLogger.IsEnabled)
                                            {
                                                BlockLogger.BrokeBlock(_persistentPlayerId, _oldBlock, _newBlockInfo.pos);
                                            }
                                        }
                                        if (BlockLogger.IsEnabled)
                                        {
                                            BlockLogger.RemovedBlock(_persistentPlayerId, _oldBlock, _newBlockInfo.pos);
                                        }
                                    }
                                    if (!_oldBlock.CanPickup)//old block can not be picked up
                                    {
                                        if (!PersistentOperations.ClaimedByAllyOrSelf(_persistentPlayerId, _newBlockInfo.pos))
                                        {
                                            int _total = _oldBlock.MaxDamage - _oldBlockValue.damage;
                                            if (_total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.playerId) > Admin_Level)
                                            {
                                                Penalty(_total, _persistentPlayerId, _cInfo);
                                                return false;
                                            }
                                            if (BlockLogger.IsEnabled)
                                            {
                                                BlockLogger.BrokeBlock(_persistentPlayerId, _oldBlock, _newBlockInfo.pos);
                                            }
                                        }
                                        if (BlockLogger.IsEnabled)
                                        {
                                            BlockLogger.RemovedBlock(_persistentPlayerId, _oldBlock, _newBlockInfo.pos);
                                        }
                                    }
                                }
                                else if (_oldBlock.blockID == _newBlock.blockID)//block is the same
                                {
                                    if (_newBlockInfo.bChangeDamage)//block took damage
                                    {
                                        int _total = _newBlockInfo.blockValue.damage - _oldBlockValue.damage;
                                        if (_total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.playerId) > Admin_Level)
                                        {
                                            Penalty(_total, _persistentPlayerId, _cInfo);
                                            return false;
                                        }
                                    }
                                    if (_oldBlockValue.damage == _newBlockInfo.blockValue.damage || _newBlockInfo.blockValue.damage == 0)//block replaced
                                    {
                                        return true;
                                    }
                                }
                                else if (_oldBlock.DowngradeBlock.Block.blockID == _newBlock.blockID)//downgraded
                                {
                                    if (_oldBlockValue.damage == _newBlockInfo.blockValue.damage || _newBlockInfo.blockValue.damage == 0)
                                    {
                                        if (BlockLogger.IsEnabled)
                                        {
                                            BlockLogger.DowngradedBlock(_persistentPlayerId, _oldBlock, _newBlock, _newBlockInfo.pos);
                                        }
                                        return true;
                                    }
                                    int _total = _oldBlock.MaxDamage - _oldBlockValue.damage + _newBlockInfo.blockValue.damage;
                                    if (_total >= DamageDetector.Block_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.playerId) > Admin_Level)
                                    {
                                        Penalty(_total, _persistentPlayerId, _cInfo);
                                        return false;
                                    }
                                    if (BlockLogger.IsEnabled)
                                    {
                                        BlockLogger.BrokeBlock(_persistentPlayerId, _oldBlock, _newBlockInfo.pos);
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
                        Phrases.Dict.TryGetValue("DamageDetector2", out string _phrase);
                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1} {2}\"", _cInfo.playerId, _phrase, _total.ToString()), null);
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

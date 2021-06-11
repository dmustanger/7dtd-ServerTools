using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class BlockChange
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Block_Damage_Limit = 3500;
        private static readonly string detectionFile = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string detectionFilepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, detectionFile);

        public static bool ProcessBlockChange(GameManager __instance, string _persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
        {
            try
            {
                World _world = __instance.World;
                if (__instance != null && _blocksToChange != null && !string.IsNullOrEmpty(_persistentPlayerId) && _blocksToChange != null)
                {
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
                                    if (POIProtection.IsEnabled && POIProtection.Bed && _world.IsPositionWithinPOI(_newBlockInfo.pos.ToVector3(), 5))
                                    {
                                        ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_persistentPlayerId);
                                        if (_cInfo != null)
                                        {
                                            GameManager.Instance.World.SetBlockRPC(_newBlockInfo.pos, BlockValue.Air);
                                            PersistentOperations.ReturnBlock(_cInfo, _newBlock.GetBlockName(), 1);
                                            Phrases.Dict.TryGetValue(1031, out string _phrase1031);
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1031 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            return false;
                                        }
                                    }
                                }
                                else if (_newBlock is BlockLandClaim)//placed a land claim
                                {
                                    if (POIProtection.IsEnabled && POIProtection.Claim && _world.IsPositionWithinPOI(_newBlockInfo.pos.ToVector3(), 5))
                                    {
                                        ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_persistentPlayerId);
                                        if (_cInfo != null)
                                        {
                                            GameManager.Instance.World.SetBlockRPC(_newBlockInfo.pos, BlockValue.Air);
                                            PersistentOperations.ReturnBlock(_cInfo, _newBlock.GetBlockName(), 1);
                                            Phrases.Dict.TryGetValue(1032, out string _phrase1032);
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1032 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            return false;
                                        }
                                    }
                                }
                                if (BlockLogger.IsEnabled)//placed block
                                {
                                    BlockLogger.Log(_persistentPlayerId, _newBlockInfo);
                                }
                                return true;
                            }
                            if (IsEnabled)
                            {
                                if (_newBlockInfo.blockValue.type == BlockValue.Air.type)//new block is air
                                {
                                    if (_oldBlockValue.type == BlockValue.Air.type)//replaced block
                                    {
                                        return true;
                                    }
                                    if (_oldBlockValue.Block is BlockLandClaim)//removed claim
                                    {
                                        if (!string.IsNullOrEmpty(_persistentPlayerId))//id is valid
                                        {
                                            if (!PersistentOperations.ClaimedByAllyOrSelf(_persistentPlayerId, _newBlockInfo.pos))
                                            {
                                                int _total = _oldBlock.MaxDamage - _oldBlockValue.damage;
                                                if (_oldBlock.MaxDamage - _oldBlockValue.damage >= Block_Damage_Limit)
                                                {
                                                    ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_persistentPlayerId);
                                                    if (_cInfo != null)
                                                    {
                                                        if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.playerId) > Admin_Level)
                                                        {
                                                            BlockPenalty(_total, _persistentPlayerId);
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (!_oldBlock.CanPickup && !PersistentOperations.ClaimedByAllyOrSelf(_persistentPlayerId, _newBlockInfo.pos))//old block can not be picked up and unclaimed space
                                    {
                                        int _total = _oldBlock.MaxDamage - _oldBlockValue.damage;
                                        if (_total >= Block_Damage_Limit)
                                        {
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_persistentPlayerId);
                                            if (_cInfo != null)
                                            {
                                                if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.playerId) > Admin_Level)
                                                {
                                                    BlockPenalty(_total, _persistentPlayerId);
                                                    return false;
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (_oldBlock.blockID == _newBlock.blockID)//block is the same
                                {
                                    if (_newBlockInfo.bChangeDamage)//block took damage
                                    {
                                        int _total = _newBlockInfo.blockValue.damage - _oldBlockValue.damage;
                                        if (_total >= Block_Damage_Limit)
                                        {
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_persistentPlayerId);
                                            if (_cInfo != null)
                                            {
                                                if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.playerId) > Admin_Level)
                                                {
                                                    BlockPenalty(_total, _persistentPlayerId);
                                                    return false;
                                                }
                                            }
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
                                        return true;
                                    }
                                    int _total = _oldBlock.MaxDamage - _oldBlockValue.damage + _newBlockInfo.blockValue.damage;
                                    if (_total >= Block_Damage_Limit)
                                    {
                                        ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_persistentPlayerId);
                                        if (_cInfo != null)
                                        {
                                            if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.playerId) > Admin_Level)
                                            {
                                                BlockPenalty(_total, _persistentPlayerId);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in ProcessDamage.ProcessBlockDamage: {0}", e.Message));
            }
            return true;
        }

        private static void BlockPenalty(int _total, string _persistentPlayerId)
        {
            try
            {
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
                if (_player != null)
                {
                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(_persistentPlayerId) > Admin_Level)
                    {
                        ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_persistentPlayerId);
                        if (_cInfo != null)
                        {
                            Phrases.Dict.TryGetValue(952, out string _phrase952);
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1} {2}\"", _cInfo.playerId, _phrase952, _total.ToString()), null);
                            using (StreamWriter sw = new StreamWriter(detectionFilepath, true, Encoding.UTF8))
                            {
                                sw.WriteLine(string.Format("Detected \"{0}\" Steam id {1} exceeding the damage limit @ position {2}. Damage: {3}", _cInfo.playerName, _persistentPlayerId, _player.position, _total));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            Phrases.Dict.TryGetValue(951, out string _phrase951);
                            _phrase951 = _phrase951.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase951 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProcessDamage.ProcessPenalty: {0}", e.Message));
            }
        }
    }
}

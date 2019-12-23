using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    public static class DamageDetector
    {
        public static bool IsEnabled = false, Block_Log = false;
        public static int Admin_Level = 0, Entity_Damage_Limit = 500, Block_Damage_Limit = 3000;
        private static string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string filepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);
        private static string _detectionFile = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string _detectionFilepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _detectionFile);
        private static string _blockFile = string.Format("BlockLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string _blockFilepath = string.Format("{0}/Logs/BlockLogs/{1}", API.ConfigPath, _blockFile);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        public static TraderArea TraderArea = GameManager.Instance.World.TraderAreas[0];

        public static bool ProcessPlayerDamage(EntityAlive __instance, DamageResponse _dmResponse)
        {
            try
            {
                if (_dmResponse.Source != null)
                {
                    int _sourceId = _dmResponse.Source.getEntityId();
                    if (_sourceId > 0)
                    {
                        if (__instance is EntityPlayer)
                        {
                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_sourceId);
                            if (_cInfo != null)
                            {
                                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                                if (_player != null)
                                {
                                    if (DamageDetector.IsEnabled)
                                    {
                                        ItemValue _itemValue = ItemClass.GetItem(_player.inventory.holdingItem.Name, true);
                                        float _distance = _player.GetDistance(__instance);
                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                        {
                                            sw.WriteLine(string.Format("{0}: {1} {2} hit {3} with entity id {4} using {5} for {6} damage @ {7}. Distance: {8}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, __instance.EntityName, __instance.entityId, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _dmResponse.Strength, __instance.position, _distance));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        if (__instance is EntityPlayer && _dmResponse.Strength >= Entity_Damage_Limit && GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId).PermissionLevel > Admin_Level)
                                        {
                                            string _message = "[FF0000]{PlayerName} has been banned for using damage manipulation.";
                                            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for using damage manipulation. Damage recorded: {1}\"", _cInfo.playerId, _dmResponse.Strength), (ClientInfo)null);
                                            using (StreamWriter sw = new StreamWriter(_detectionFilepath, true))
                                            {
                                                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, using damage manipulation @ {2}. Damage recorded: {3}.", _cInfo.playerName, _cInfo.playerId, __instance.position, _dmResponse.Strength));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            return false;
                                        }
                                    }
                                }
                                if (Zones.IsEnabled)
                                {
                                    if (Zones.ZonePvE.Contains(__instance.entityId) || Zones.ZonePvE.Contains(_cInfo.entityId))
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Do not attack players inside a pve zone or while standing in one!" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        return false;
                                    }
                                }
                                if (Lobby.IsEnabled && Lobby.PvE && Lobby.LobbyPlayers.Contains(__instance.entityId) || Market.IsEnabled && Market.PvE && Market.MarketPlayers.Contains(__instance.entityId))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DamageDetector.ProcessPlayerDamage: {0}", e.Message));
            }
            return true;
        }

        public static bool ProcessBlockDamage(GameManager __instance, string _persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
        {
            try
            {
                World _world = __instance.World;
                for (int i = 0; i < _blocksToChange.Count; i++)
                {
                    BlockChangeInfo _newBlockInfo = _blocksToChange[i];//new block info
                    BlockValue _blockValue = _world.GetBlock(_newBlockInfo.pos);//old block value
                    if (_newBlockInfo.bChangeBlockValue)//new block value
                    {
                        if (_newBlockInfo.blockValue.type == BlockValue.Air.type)//new block is air
                        {
                            if (_blockValue.type == BlockValue.Air.type)//replaced block
                            {
                                return true;
                            }
                            if (_blockValue.Block is BlockLandClaim)//removed claim
                            {
                                if (!string.IsNullOrEmpty(_persistentPlayerId))//id is valid
                                {
                                    PersistentPlayerData _ppd = PersistentOperations.GetPersistentPlayerData(_persistentPlayerId);
                                    if (_ppd != null)
                                    {
                                        List<Vector3i> _protectionBlocks = _ppd.GetLandProtectionBlocks();
                                        if (_protectionBlocks != null && !_protectionBlocks.Contains(_newBlockInfo.pos))//protection block list valid and does not contain this location
                                        {
                                            int _total = _blockValue.Block.MaxDamage - _blockValue.damage;
                                            if (_blockValue.Block.MaxDamage - _blockValue.damage >= Block_Damage_Limit && ProcessPenalty(_total, _persistentPlayerId, _newBlockInfo))
                                            {
                                                //_world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                                                //return false;
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_blockValue.Block.CanPickup)//old block can not be picked up
                            {
                                int _total = _blockValue.Block.MaxDamage - _blockValue.damage;
                                if (_total >= Block_Damage_Limit && ProcessPenalty(_total, _persistentPlayerId, _newBlockInfo))
                                {
                                    //_world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                                    //return false;
                                }
                            }
                        }
                        else if (_blockValue.type == BlockValue.Air.type)//old block was air
                        {
                            if (_newBlockInfo.blockValue.Block is BlockSleepingBag)//placed a sleeping bag
                            {
                                PersistentOperations.BedBug(_persistentPlayerId);
                                if (Block_Log)
                                {
                                    BlockLog(_persistentPlayerId, _newBlockInfo);
                                }
                            }
                            else if (_newBlockInfo.blockValue.Block is BlockLandClaim)//placed a land claim
                            {
                                PersistentOperations.ClaimBug(_persistentPlayerId);
                                if (Block_Log)
                                {
                                    BlockLog(_persistentPlayerId, _newBlockInfo);
                                }
                            }
                            else//placed block
                            {
                                if (Block_Log)
                                {
                                    BlockLog(_persistentPlayerId, _newBlockInfo);
                                }
                            }
                        }
                        else if (_blockValue.Block.blockID == _newBlockInfo.blockValue.Block.blockID)//block is the same
                        {
                            if (_newBlockInfo.bChangeDamage)//block took damage
                            {
                                int _total = _newBlockInfo.blockValue.damage - _blockValue.damage;
                                if (_total >= Block_Damage_Limit && ProcessPenalty(_total, _persistentPlayerId, _newBlockInfo))
                                {
                                    //_world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                                    //return false;
                                }
                            }
                            if (_blockValue.damage == _newBlockInfo.blockValue.damage || _newBlockInfo.blockValue.damage == 0)//protected block replaced
                            {
                                //return true;
                            }
                        }
                        else if (_blockValue.Block.DowngradeBlock.Block.blockID == _newBlockInfo.blockValue.Block.blockID)//downgraded
                        {
                            int _total = _blockValue.Block.MaxDamage - _blockValue.damage + _newBlockInfo.blockValue.damage;
                            if (_total >= Block_Damage_Limit && ProcessPenalty(_total, _persistentPlayerId, _newBlockInfo))
                            {
                                //_world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                                //return false;
                            }
                        }
                    }
                    //if (Lobby.IsEnabled && Lobby.Protected && Lobby.Lobby_Position != "0,0,0")
                    //{
                    //    if (Lobby.ProtectedSpace(_newBlockInfo.pos.x, _newBlockInfo.pos.z))
                    //    {
                    //        _world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                    //        return false;
                    //    }
                    //}
                    //if (Market.IsEnabled && Market.Protected && Market.Market_Position != "0,0,0")
                    //{
                    //    if (Market.ProtectedSpace(_newBlockInfo.pos.x, _newBlockInfo.pos.z))
                    //    {
                    //        _world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                    //        return false;
                    //    }
                    //}
                    //if (Zones.IsEnabled)
                    //{
                    //    if (Zones.Protected(_newBlockInfo.pos))
                    //    {
                    //        _world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                    //        return false;
                    //    }
                    //}
                    //if (ProtectedSpace.IsEnabled)
                    //{
                    //    if (ProtectedSpace.Exec(_newBlockInfo.pos))
                    //    {
                    //        _world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                    //        return false;
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DamageDetector.ProcessBlockDamage: {0}", e.Message));
            }
            return true;
        }

        public static void BlockLog(string _persistentPlayerId, BlockChangeInfo _bChangeInfo)
        {
            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
            if (_player != null)
            {
                using (StreamWriter sw = new StreamWriter(_blockFilepath, true))
                {
                    sw.WriteLine(string.Format("{0}: Player named {1} with steam id {2} placed {3} @ {4}.", DateTime.Now, _player.EntityName, _persistentPlayerId, _bChangeInfo.blockValue.Block.GetBlockName(), _bChangeInfo.pos));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static bool ProcessPenalty(int _total, string _persistentPlayerId, BlockChangeInfo _bChangeInfo)
        {
            try
            {
                if (!string.IsNullOrEmpty(_persistentPlayerId))
                {
                    EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
                    if (_player != null)
                    {
                        AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_persistentPlayerId);
                        if (Admin.PermissionLevel > Admin_Level)
                        {
                            if (DamageDetector.IsEnabled)
                            {
                                string _message = "[FF0000]{PlayerName} has been banned for using damage manipulation.";
                                _message = _message.Replace("{PlayerName}", _player.EntityName);
                                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_persistentPlayerId);
                                if (_cInfo != null)
                                {
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for using a damage manipulator. Damage recorded: {1}\"", _persistentPlayerId, _total.ToString()), null);
                                    using (StreamWriter sw = new StreamWriter(_detectionFilepath, true))
                                    {
                                        sw.WriteLine(string.Format("Detected {0} with steam id {1} using a damage manipulator @ position {2}. Damage recorded: {3}", _player.EntityName, _persistentPlayerId, _player.position, _total));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                }
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DamageDetector.ProcessPenalty: {0}", e.Message));
            }
            return false;
        }
    }
}

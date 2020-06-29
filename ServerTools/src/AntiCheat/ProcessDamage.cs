using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools.AntiCheat
{
    public static class ProcessDamage
    {
        public static bool Damage_Detector = false, IsEnabled = false;
        public static int Admin_Level = 0, Entity_Damage_Limit = 500, Block_Damage_Limit = 3000;
        private static string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string filepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);
        private static string _detectionFile = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string _detectionFilepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _detectionFile);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static bool ProcessPlayerDamage(EntityAlive __instance, DamageResponse _dmResponse)
        {
            try
            {
                if (__instance != null && __instance is EntityPlayer && _dmResponse.Source != null)
                {
                    int _sourceId = _dmResponse.Source.getEntityId();
                    if (_sourceId > 0)
                    {
                        ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_sourceId);
                        if (_cInfo != null)
                        {
                            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                            if (_player != null)
                            {
                                if (Damage_Detector)
                                {
                                    ItemValue _itemValue = ItemClass.GetItem(_player.inventory.holdingItem.Name, true);
                                    if (_itemValue != null)
                                    {
                                        int _distance = (int)_player.GetDistance(__instance);
                                        using (StreamWriter sw = new StreamWriter(filepath, true))
                                        {
                                            sw.WriteLine(string.Format("{0}: {1} {2} hit {3} with entity id {4} using {5} for {6} damage @ {7}. Distance: {8}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, __instance.EntityName, __instance.entityId, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _dmResponse.Strength, __instance.position, _distance));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        if (_dmResponse.Strength >= Entity_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProcessDamage.ProcessPlayerDamage: {0}", e.Message));
            }
            return true;
        }

        public static bool ProcessBlockDamage(GameManager __instance, string _persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
        {
            try
            {
                World _world = __instance.World;
                if (__instance != null && _world != null && _blocksToChange != null && !string.IsNullOrEmpty(_persistentPlayerId) && _blocksToChange != null)
                {
                    for (int i = 0; i < _blocksToChange.Count; i++)
                    {
                        BlockChangeInfo _newBlockInfo = _blocksToChange[i];//new block info
                        BlockValue _blockValue = _world.GetBlock(_newBlockInfo.pos);//old block value
                        if (_newBlockInfo != null && _newBlockInfo.bChangeBlockValue)//new block value
                        {
                            if (_blockValue.type == BlockValue.Air.type)//old block was air
                            {
                                if (_newBlockInfo.blockValue.Block is BlockSleepingBag)//placed a sleeping bag
                                {
                                    PersistentOperations.BedBug(_persistentPlayerId);
                                    if (BlockLogger.IsEnabled)
                                    {
                                        BlockLogger.Log(_persistentPlayerId, _newBlockInfo);
                                    }
                                }
                                else if (BlockLogger.IsEnabled && _newBlockInfo.blockValue.Block is BlockLandClaim)//placed a land claim
                                {
                                    PersistentOperations.ClaimBug(_persistentPlayerId);
                                    BlockLogger.Log(_persistentPlayerId, _newBlockInfo);
                                }
                                else if (BlockLogger.IsEnabled)//placed block
                                {
                                    BlockLogger.Log(_persistentPlayerId, _newBlockInfo);
                                }
                            }
                            if (ProcessDamage.Damage_Detector)
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
                                            if (!PersistentOperations.ClaimedByAllySelfOrParty(_persistentPlayerId, _newBlockInfo.pos))
                                            {
                                                int _total = _blockValue.Block.MaxDamage - _blockValue.damage;
                                                if (_blockValue.Block.MaxDamage - _blockValue.damage >= Block_Damage_Limit && ProcessPenalty(_total, _persistentPlayerId))
                                                {
                                                    if (ProtectedSpaces.IsEnabled && ProtectedSpaces.IsProtectedSpace(_newBlockInfo.pos))
                                                    {
                                                        _world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (!_blockValue.Block.CanPickup)//old block can not be picked up
                                    {
                                        int _total = _blockValue.Block.MaxDamage - _blockValue.damage;
                                        if (_total >= Block_Damage_Limit && ProcessPenalty(_total, _persistentPlayerId))
                                        {
                                            if (ProtectedSpaces.IsEnabled && ProtectedSpaces.IsProtectedSpace(_newBlockInfo.pos))
                                            {
                                                _world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                                                return false;
                                            }
                                        }
                                    }
                                }
                                else if (_blockValue.Block.blockID == _newBlockInfo.blockValue.Block.blockID)//block is the same
                                {
                                    if (_newBlockInfo.bChangeDamage)//block took damage
                                    {
                                        int _total = _newBlockInfo.blockValue.damage - _blockValue.damage;
                                        if (_total >= Block_Damage_Limit && ProcessPenalty(_total, _persistentPlayerId))
                                        {
                                            if (ProtectedSpaces.IsEnabled && ProtectedSpaces.IsProtectedSpace(_newBlockInfo.pos) || Zones.IsEnabled && Zones.Protected(_newBlockInfo.pos))
                                            {
                                                _world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
                                                return false;
                                            }
                                        }
                                    }
                                    if (_blockValue.damage == _newBlockInfo.blockValue.damage || _newBlockInfo.blockValue.damage == 0)//protected block replaced
                                    {
                                        return true;
                                    }
                                }
                                else if (_blockValue.Block.DowngradeBlock.Block.blockID == _newBlockInfo.blockValue.Block.blockID)//downgraded
                                {
                                    int _total = _blockValue.Block.MaxDamage - _blockValue.damage + _newBlockInfo.blockValue.damage;
                                    if (_total >= Block_Damage_Limit && ProcessPenalty(_total, _persistentPlayerId))
                                    {
                                        if (ProtectedSpaces.IsEnabled && ProtectedSpaces.IsProtectedSpace(_newBlockInfo.pos))
                                        {
                                            _world.SetBlockRPC(_newBlockInfo.clrIdx, _newBlockInfo.pos, _blockValue);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in ProcessDamage.ProcessBlockDamage: {0}", e.Message));
            }
            return true;
        }

        public static bool ProcessPenalty(int _total, string _persistentPlayerId)
        {
            try
            {
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
                if (_player != null)
                {
                    var permissionLevel = GameManager.Instance.adminTools.GetUserPermissionLevel(_persistentPlayerId);
                    if (permissionLevel > Admin_Level)
                    {
                        if (Damage_Detector)
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProcessDamage.ProcessPenalty: {0}", e.Message));
            }
            return false;
        }
    }
}

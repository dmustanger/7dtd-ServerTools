using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    public static class DamageDetector
    {
        public static bool IsEnabled = false, Block_Log = false;
        public static int Admin_Level = 0, Entity_Damage_Limit = 500, Block_Damage_Limit = 3000;
        private static Vector3i BlockPos = new Vector3i();
        private static int TotalDamage = 0;
        private static string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);
        private static string _detectionFile = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _detectionFilepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _detectionFile);
        private static string _blockFile = string.Format("BlockLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _blockFilepath = string.Format("{0}/Logs/BlockLogs/{1}", API.ConfigPath, _blockFile);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void ProcessEntityDamage(EntityAlive __instance, DamageResponse _dmResponse)
        {
            try
            {
                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_dmResponse.Source.getEntityId());
                if (_cInfo != null)
                {
                    EntityPlayer _attacker = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                    if (_attacker != null)
                    {
                        if (DamageDetector.IsEnabled)
                        {
                            ItemValue _itemValue = ItemClass.GetItem(_attacker.inventory.holdingItem.Name, true);
                            if (_dmResponse.Fatal)
                            {
                                float _distance = _attacker.GetDistance(__instance);
                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}: {1} {2} hit {3} with entity id {4} using {5} for {6} damage @ {7}. Fatal blow distance: {8}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, __instance.EntityName, __instance.entityId, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _dmResponse.Strength, __instance.position, _distance));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                            else
                            {
                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}: {1} {2} hit {3} with entity id {4} using {5} for {6} damage @ {7}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, __instance.EntityName, __instance.entityId, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _dmResponse.Strength, __instance.position));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
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
                            }
                        }
                        if (__instance is EntityPlayer && Zones.IsEnabled && (Zones.ZonePvE.Contains(__instance.entityId) || Zones.ZonePvE.Contains(_cInfo.entityId)))
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Do not attack players inside a pve zone or while standing in one!" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DamageDetector.ProcessEntityDamage: {0}.", e.Message));
            }
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
                    if (_newBlockInfo.pos != BlockPos)//position changed from last block
                    {
                        if (_newBlockInfo.bChangeBlockValue)//new block value
                        {
                            if (_blockValue.Block is BlockSleepingBag && _newBlockInfo.blockValue.type == BlockValue.Air.type)//destroyed or removed sleeping bag
                            {
                                TotalDamage = 0;

                            }
                            else if (_blockValue.type == BlockValue.Air.type)
                            {
                                if (_newBlockInfo.blockValue.Block is BlockSleepingBag)//placed a sleeping bag
                                {
                                    TotalDamage = 0;
                                    PersistentOperations.BedBug(_persistentPlayerId);
                                }
                                else if (_newBlockInfo.bChangeDensity)//placed new block
                                {
                                    TotalDamage = 0;
                                    if (Block_Log)
                                    {
                                        BlockLog(_persistentPlayerId, _newBlockInfo);
                                    }
                                }
                                else if (_newBlockInfo.blockValue.Block.CanPickup)//placed new block
                                {
                                    TotalDamage = 0;
                                    if (Block_Log)
                                    {
                                        BlockLog(_persistentPlayerId, _newBlockInfo);
                                    }
                                }
                            }
                            else if (_newBlockInfo.blockValue.type == BlockValue.Air.type)//old block destroyed, now air
                            {
                                int _total = _blockValue.Block.MaxDamage - _blockValue.damage;
                                TotalDamage = 0;
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_persistentPlayerId);
                                if (Admin.PermissionLevel > Admin_Level && DamageDetector.IsEnabled)
                                {
                                    ProcessPenalty(_total, _persistentPlayerId, _newBlockInfo);
                                }
                            }
                            else if (_newBlockInfo.bChangeDamage)//damaged block, did not destroy
                            {
                                int _total = _newBlockInfo.blockValue.damage - _blockValue.damage;
                                TotalDamage = 0;
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_persistentPlayerId);
                                if (Admin.PermissionLevel > Admin_Level && DamageDetector.IsEnabled)
                                {
                                    ProcessPenalty(_total, _persistentPlayerId, _newBlockInfo);
                                }
                            }
                            else if (_blockValue.Block.DowngradeBlock.ToString() != "")//block downgraded
                            {
                                int _total = _blockValue.Block.MaxDamage - _blockValue.damage + _newBlockInfo.blockValue.damage;
                                TotalDamage = _total;
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_persistentPlayerId);
                                if (Admin.PermissionLevel > Admin_Level && DamageDetector.IsEnabled)
                                {
                                    ProcessPenalty(_total, _persistentPlayerId, _newBlockInfo);
                                }
                            }
                            else
                            {
                                TotalDamage = 0;
                            }
                        }
                        else if (!_newBlockInfo.bChangeBlockValue)//block was placed, cycling through tier
                        {
                            TotalDamage = 0;
                        }
                        BlockPos = _blocksToChange[i].pos;
                    }
                    else//position is the same
                    {
                        if (_newBlockInfo.bChangeBlockValue)//new block value
                        {
                            if (_blockValue.Block is BlockSleepingBag && _newBlockInfo.blockValue.type == BlockValue.Air.type)//destroyed or removed sleeping bag
                            {
                                TotalDamage = 0;

                            }
                            else if (_blockValue.type == BlockValue.Air.type)
                            {
                                if (_newBlockInfo.blockValue.Block is BlockSleepingBag)//placed a sleeping bag
                                {
                                    TotalDamage = 0;
                                    PersistentOperations.BedBug(_persistentPlayerId);
                                }
                                else if (_newBlockInfo.bChangeDensity)//placed new block
                                {
                                    TotalDamage = 0;
                                    if (Block_Log)
                                    {
                                        BlockLog(_persistentPlayerId, _newBlockInfo);
                                    }
                                }
                                else if (_newBlockInfo.blockValue.Block.CanPickup)//placed new block
                                {
                                    TotalDamage = 0;
                                    if (Block_Log)
                                    {
                                        BlockLog(_persistentPlayerId, _newBlockInfo);
                                    }
                                }
                            }
                            else if (_newBlockInfo.blockValue.type == BlockValue.Air.type)//old block destroyed, now air
                            {
                                int _total = TotalDamage + _blockValue.Block.MaxDamage - _blockValue.damage;
                                TotalDamage = 0;
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_persistentPlayerId);
                                if (Admin.PermissionLevel > Admin_Level && DamageDetector.IsEnabled)
                                {
                                    ProcessPenalty(_total, _persistentPlayerId, _newBlockInfo);
                                }
                            }
                            else if (_newBlockInfo.bChangeDamage)//damaged block, did not destroy
                            {
                                int _total = TotalDamage + _newBlockInfo.blockValue.damage - _blockValue.damage;
                                TotalDamage = 0;
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_persistentPlayerId);
                                if (Admin.PermissionLevel > Admin_Level && DamageDetector.IsEnabled)
                                {
                                    ProcessPenalty(_total, _persistentPlayerId, _newBlockInfo);
                                }
                            }
                            else if (_blockValue.Block.DowngradeBlock.ToString() != "")//block downgraded
                            {
                                int _total = TotalDamage + _blockValue.Block.MaxDamage - _blockValue.damage + _newBlockInfo.blockValue.damage;
                                TotalDamage = _total;
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_persistentPlayerId);
                                if (Admin.PermissionLevel > Admin_Level && DamageDetector.IsEnabled)
                                {
                                    ProcessPenalty(_total, _persistentPlayerId, _newBlockInfo);
                                }
                            }
                            else
                            {
                                TotalDamage = 0;
                            }
                        }
                        else if (!_newBlockInfo.bChangeBlockValue)//block was placed, cycling through tier
                        {
                            TotalDamage = 0;
                        }
                        BlockPos = _newBlockInfo.pos;
                    }

                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DamageDetector.ProcessBlockDamage: {0}.", e.Message));
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

        public static void ProcessPenalty(int _total, string _persistentPlayerId, BlockChangeInfo _bChangeInfo)
        {
            try
            {
                if (!string.IsNullOrEmpty(_persistentPlayerId))
                {
                    if (_total >= Block_Damage_Limit)
                    {
                        EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
                        if (_player != null)
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
                        }
                    }
                }
                else
                {
                    List<EntityPlayer> _players = PersistentOperations.PlayersWithin100Blocks(_bChangeInfo.pos.x, _bChangeInfo.pos.z);
                    if (_players != null)
                    {
                        for (int i = 0; i < _players.Count; i++)
                        {
                            ItemValue _itemValue = ItemClass.GetItem(_players[i].inventory.holdingItem.Name, true);
                            if (_itemValue.type != ItemValue.None.type)
                            {
                                using (StreamWriter sw = new StreamWriter(_detectionFilepath, true))
                                {
                                    sw.WriteLine(string.Format("Detected player named {0} with steam id {1} holding item {2} at position {3}. They are within 100 blocks of the damage manipulation. Damage recorded: {4}", _players[i].EntityName, _players[i].belongsPlayerId, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _players[i].position, _total));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DamageDetector.ProcessPenalty: {0}.", e.Message));
            }
        }
    }
}

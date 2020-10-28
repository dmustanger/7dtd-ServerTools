using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools.AntiCheat
{
    public static class ProcessDamage
    {
        public static bool Damage_Detector = false, IsEnabled = false;
        public static int Admin_Level = 0, Entity_Damage_Limit = 3000, Block_Damage_Limit = 3500;
        private static string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string filepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);
        private static string _detectionFile = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string _detectionFilepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _detectionFile);

        public static bool ProcessPlayerDamage(EntityAlive __instance, DamageResponse _dmResponse)
        {
            try
            {
                if (__instance != null)
                {
                    if (__instance is EntityPlayer)
                    {
                        if (_dmResponse.Source != null && _dmResponse.Strength > 1)
                        {
                            EntityPlayer _player1 = (EntityPlayer)__instance;
                            int _sourceId = _dmResponse.Source.getEntityId();
                            if (_sourceId > 0 && _player1.entityId != _sourceId)
                            {
                                ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromEntityId(_sourceId);
                                if (_cInfo2 != null)
                                {
                                    EntityPlayer _player2 = PersistentOperations.GetEntityPlayer(_cInfo2.playerId);
                                    if (_player2 != null)
                                    {
                                        ItemValue _itemValue = ItemClass.GetItem(_dmResponse.Source.ItemClass.GetItemName(), true);
                                        if (_itemValue != null)
                                        {
                                            if (Damage_Detector)
                                            {
                                                int _distance = (int)_player2.GetDistance(__instance);
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} \"{2}\" hit \"{3}\" with entity id {4} using {5} for {6} damage @ {7}. Distance: {8}", DateTime.Now, _cInfo2.playerId, _cInfo2.playerName, __instance.EntityName, __instance.entityId, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _dmResponse.Strength, __instance.position, _distance));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                if (_dmResponse.Strength >= Entity_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo2) > Admin_Level)
                                                {
                                                    Phrases.Dict.TryGetValue(952, out string _phrase952);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1} {2}\"", _cInfo2.playerId, _phrase952, _dmResponse.Strength), null);
                                                    using (StreamWriter sw = new StreamWriter(_detectionFilepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("Detected \"{0}\" Steam Id {1}, exceeded damage limit @ {2}. Damage: {3}", _cInfo2.playerName, _cInfo2.playerId, __instance.position, _dmResponse.Strength));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                    Phrases.Dict.TryGetValue(951, out string _phrase951);
                                                    _phrase951 = _phrase951.Replace("{PlayerName}", _cInfo2.playerName);
                                                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase951 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                                    return false;
                                                }
                                            }
                                        }
                                        if (Zones.IsEnabled)
                                        {
                                            if (Zones.ZonePvE.Contains(__instance.entityId) || Zones.ZonePvE.Contains(_cInfo2.entityId))
                                            {
                                                Phrases.Dict.TryGetValue(323, out string _phrase323);
                                                ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _phrase323 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                if (!_player1.IsFriendsWith(_player2))
                                                {
                                                    if (PersistentOperations.PvEViolations.ContainsKey(_cInfo2.entityId))
                                                    {
                                                        PersistentOperations.PvEViolations.TryGetValue(_cInfo2.entityId, out int _flags);
                                                        _flags++;
                                                        if (PersistentOperations.Jail_Violation > 0 && _flags >= PersistentOperations.Jail_Violation)
                                                        {
                                                            Jail(_cInfo2, __instance);
                                                        }
                                                        if (PersistentOperations.Kill_Violation > 0 && _flags >= PersistentOperations.Kill_Violation)
                                                        {
                                                            Kill(_cInfo2);
                                                        }
                                                        if (PersistentOperations.Kick_Violation > 0 && _flags >= PersistentOperations.Kick_Violation)
                                                        {
                                                            Kick(_cInfo2);
                                                        }
                                                        if (PersistentOperations.Ban_Violation > 0 && _flags >= PersistentOperations.Ban_Violation)
                                                        {
                                                            Ban(_cInfo2);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        PersistentOperations.PvEViolations.Add(_cInfo2.entityId, 1);
                                                    }
                                                }
                                                return false;
                                            }
                                        }
                                        if (Lobby.IsEnabled && Lobby.PvE && Lobby.LobbyPlayers.Contains(__instance.entityId) || Market.IsEnabled && Market.PvE && Market.MarketPlayers.Contains(__instance.entityId))
                                        {
                                            Phrases.Dict.TryGetValue(260, out string _phrase260);
                                            ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _phrase260 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            if (!_player1.IsFriendsWith(_player2))
                                            {
                                                if (PersistentOperations.PvEViolations.ContainsKey(_cInfo2.entityId))
                                                {
                                                    PersistentOperations.PvEViolations.TryGetValue(_cInfo2.entityId, out int _violations);
                                                    _violations++;
                                                    if (PersistentOperations.Jail_Violation > 0 && _violations >= PersistentOperations.Jail_Violation)
                                                    {
                                                        Jail(_cInfo2, __instance);
                                                    }
                                                    if (PersistentOperations.Kill_Violation > 0 && _violations >= PersistentOperations.Kill_Violation)
                                                    {
                                                        Kill(_cInfo2);
                                                    }
                                                    if (PersistentOperations.Kick_Violation > 0 && _violations >= PersistentOperations.Kick_Violation)
                                                    {
                                                        Kick(_cInfo2);
                                                    }
                                                    else if (PersistentOperations.Ban_Violation > 0 && _violations >= PersistentOperations.Ban_Violation)
                                                    {
                                                        Ban(_cInfo2);
                                                    }
                                                }
                                                else
                                                {
                                                    PersistentOperations.PvEViolations.Add(_cInfo2.entityId, 1);
                                                }
                                            }
                                            return false;
                                        }
                                        if (KillNotice.IsEnabled)
                                        {
                                            if (KillNotice.Damage.ContainsKey(_player1.entityId))
                                            {
                                                KillNotice.Damage[_player1.entityId] = _dmResponse.Strength;
                                            }
                                            else
                                            {
                                                KillNotice.Damage.Add(_player1.entityId, _dmResponse.Strength);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (__instance is EntityZombie || __instance is EntityAnimal)
                    {
                        if (_dmResponse.Source != null && _dmResponse.Strength > 1)
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
                                        ItemValue _itemValue = ItemClass.GetItem(_player.inventory.holdingItem.Name, true);
                                        if (_itemValue != null)
                                        {
                                            if (Damage_Detector)
                                            {
                                                int _distance = (int)_player.GetDistance(__instance);
                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("{0}: {1} \"{2}\" hit \"{3}\" with entity id {4} using {5} for {6} damage @ {7}. Distance: {8}", DateTime.Now, _cInfo.playerId, _cInfo.playerName, __instance.EntityName, __instance.entityId, _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName(), _dmResponse.Strength, __instance.position, _distance));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                if (_dmResponse.Strength >= Entity_Damage_Limit && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
                                                {
                                                    Phrases.Dict.TryGetValue(952, out string _phrase952);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1} {2}\"", _cInfo.playerId, _phrase952, _dmResponse.Strength), null);
                                                    using (StreamWriter sw = new StreamWriter(_detectionFilepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("Detected \"{0}\" Steam Id {1}, exceeded damage limit @ {2}. Damage: {3}", _cInfo.playerName, _cInfo.playerId, __instance.position, _dmResponse.Strength));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                    Phrases.Dict.TryGetValue(951, out string _phrase951);
                                                    _phrase951 = _phrase951.Replace("{PlayerName}", _cInfo.playerName);
                                                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase951 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in ProcessDamage.ProcessPlayerDamage: {0}", e.Message));
            }
            return true;
        }

        public static bool ProcessBlockDamage(GameManager __instance, string _persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
        {
            try
            {
                World _world = __instance.World;
                if (__instance != null && _blocksToChange != null && !string.IsNullOrEmpty(_persistentPlayerId) && _blocksToChange != null)
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
                                }
                                else if (_newBlockInfo.blockValue.Block is BlockLandClaim)//placed a land claim
                                {
                                    
                                }
                                if (BlockLogger.IsEnabled)//placed block
                                {
                                    BlockLogger.Log(_persistentPlayerId, _newBlockInfo);
                                }
                                return true;
                            }
                            if (Damage_Detector)
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
                                            if (!PersistentOperations.ClaimedByAllyOrSelf(_persistentPlayerId, _newBlockInfo.pos))
                                            {
                                                int _total = _blockValue.Block.MaxDamage - _blockValue.damage;
                                                if (_blockValue.Block.MaxDamage - _blockValue.damage >= Block_Damage_Limit)
                                                {
                                                    BlockPenalty(_total, _persistentPlayerId);
                                                }
                                            }
                                        }
                                    }
                                    if (!_blockValue.Block.CanPickup && !PersistentOperations.ClaimedByAllyOrSelf(_persistentPlayerId, _newBlockInfo.pos))//old block can not be picked up and unclaimed space
                                    {
                                        int _total = _blockValue.Block.MaxDamage - _blockValue.damage;
                                        if (_total >= Block_Damage_Limit)
                                        {
                                            BlockPenalty(_total, _persistentPlayerId);
                                        }
                                    }
                                }
                                else if (_blockValue.Block.blockID == _newBlockInfo.blockValue.Block.blockID)//block is the same
                                {
                                    if (_newBlockInfo.bChangeDamage)//block took damage
                                    {
                                        int _total = _newBlockInfo.blockValue.damage - _blockValue.damage;
                                        if (_total >= Block_Damage_Limit)
                                        {
                                            BlockPenalty(_total, _persistentPlayerId);
                                        }
                                    }
                                    if (_blockValue.damage == _newBlockInfo.blockValue.damage || _newBlockInfo.blockValue.damage == 0)//block replaced
                                    {
                                        return true;
                                    }
                                }
                                else if (_blockValue.Block.DowngradeBlock.Block.blockID == _newBlockInfo.blockValue.Block.blockID)//downgraded
                                {
                                    if (_blockValue.damage == _newBlockInfo.blockValue.damage || _newBlockInfo.blockValue.damage == 0)
                                    {
                                        return true;
                                    }
                                    int _total = _blockValue.Block.MaxDamage - _blockValue.damage + _newBlockInfo.blockValue.damage;
                                    if (_total >= Block_Damage_Limit)
                                    {
                                        BlockPenalty(_total, _persistentPlayerId);
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
                            using (StreamWriter sw = new StreamWriter(_detectionFilepath, true))
                            {
                                sw.WriteLine(string.Format("Detected \"{0}\" Steam id {1} exceeding the damage limit @ position {2}. Damage: {3}", _cInfo.playerName, _persistentPlayerId, _player.position, _total));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            Phrases.Dict.TryGetValue(951, out string _phrase951);
                            _phrase951 = _phrase951.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase951 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProcessDamage.ProcessPenalty: {0}", e.Message));
            }
        }

        private static void Jail(ClientInfo _cInfoKiller, EntityAlive _cInfoVictim)
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0} 120", _cInfoKiller.playerId), null);
            if (!Zones.Forgive.ContainsKey(_cInfoVictim.entityId))
            {
                Zones.Forgive.Add(_cInfoVictim.entityId, _cInfoKiller.entityId);
            }
            else
            {
                Zones.Forgive[_cInfoVictim.entityId] = _cInfoKiller.entityId;
            }
            Phrases.Dict.TryGetValue(204, out string _phrase204);
            _phrase204 = _phrase204.Replace("{PlayerName}", _cInfoKiller.playerName);
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase204 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        private static void Kill(ClientInfo _cInfo)
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), null);
            Phrases.Dict.TryGetValue(324, out string _phrase324);
            _phrase324 = _phrase324.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase324 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        private static void Kick(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue(326, out string _phrase326);
            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase326), null);
            Phrases.Dict.TryGetValue(325, out string _phrase325);
            _phrase325 = _phrase325.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase325 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        private static void Ban(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue(328, out string _phrase328);
            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase328), null);
            Phrases.Dict.TryGetValue(327, out string _phrase327);
            _phrase327 = _phrase327.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase327 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class BattleLogger
    {
        public static bool IsEnabled = false, Drop = false, Remove = false, All = false, Belt = false, Bag = false, Equipment = false;
        public static string Command131 = "exit", Command132 = "quit";
        public static int Admin_Level = 0;
        public static Dictionary<string, string> Players = new Dictionary<string, string>();
        private static int LogLineCount = 0;

        public static void ScanLog(string _id)
        {
            try
            {
                int _lineCount = 0;
                string _dateTime1 = DateTime.Now.AddSeconds(-2).ToString("yyyy-MM-ddTHH:mm:ss"), _dateTime2 = DateTime.Now.AddSeconds(-3).ToString("yyyy-MM-ddTHH:mm:ss"),
                    _dateTime3 = DateTime.Now.AddSeconds(-4).ToString("yyyy-MM-ddTHH:mm:ss"), _dateTime4 = DateTime.Now.AddSeconds(-5).ToString("yyyy-MM-ddTHH:mm:ss"),
                    _dateTime5 = DateTime.Now.AddSeconds(-6).ToString("yyyy-MM-ddTHH:mm:ss");
                string _ip;
                BattleLogger.Players.TryGetValue(_id, out _ip);
                BattleLogger.Players.Remove(_id);
                using (FileStream fs = new FileStream(Confirm.LogName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        for (int i = 0; i < int.MaxValue; i++)
                        {
                            string _line = sr.ReadLine();
                            if (_line != null)
                            {
                                _lineCount++;
                                if (_lineCount > LogLineCount && _line.Contains("Client disconnect") && _line.Contains(_ip) && (_line.Contains(_dateTime1) || _line.Contains(_dateTime2) || _line.Contains(_dateTime3) || _line.Contains(_dateTime4) || _line.Contains(_dateTime5)))
                                {
                                    string _reason = _line.Split(' ').Last();
                                    if (_reason == "(RemoteConnectionClose)")
                                    {
                                        Penalty(_id);
                                    }
                                    LogLineCount = _lineCount;
                                    break;
                                }
                            }
                            else
                            {
                                LogLineCount = _lineCount;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BattleLogger.ScanLog: {0}", e.Message));
            }
        }

        public static bool BattleLog(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (_player != null)
                {
                    List<EntityPlayer> _players = PersistentOperations.PlayerList();
                    if (_players != null)
                    {
                        for (int i = 0; i < _players.Count; i++)
                        {
                            EntityPlayer _player2 = _players[i];
                            if (_player2 != null && _player2 != _player)
                            {
                                EntityPlayer _damageTarget = (EntityPlayer)_player.GetDamagedTarget();
                                EntityPlayer _attackTarget = (EntityPlayer)_player.GetAttackTarget();
                                EntityPlayer _damageTarget2 = (EntityPlayer)_player2.GetDamagedTarget();
                                EntityPlayer _attackTarget2 = (EntityPlayer)_player2.GetAttackTarget();
                                if ((_damageTarget != null && _damageTarget == _player2 && !_damageTarget.IsFriendsWith(_player2)) ||
                                    (_attackTarget != null && _attackTarget == _player2 && !_attackTarget.IsFriendsWith(_player2)) ||
                                    (_damageTarget2 != null && _damageTarget2 == _player && !_damageTarget2.IsFriendsWith(_player)) ||
                                    (_attackTarget2 != null && _attackTarget2 == _player && !_damageTarget2.IsFriendsWith(_player)))
                                {
                                    float _distance = _player2.GetDistanceSq(_player);
                                    if (_distance <= 80f)
                                    {
                                        Timers.BattleLogTool(_cInfo.playerId);
                                    }
                                }
                            }
                        }
                    }
                }
                BattleLogger.Players.Remove(_cInfo.playerId);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.BattleLog: {0}", e.Message));
            }
            return true;
        }

        private static void Penalty(string _id)
        {
            try
            {
                PlayerDataFile _playerDataFile = PersistentOperations.GetPlayerDataFileFromSteamId(_id);
                if (_playerDataFile != null)
                {
                    PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_id);
                    if (_persistentPlayerData != null)
                    {
                        GC.Collect();
                        MemoryPools.Cleanup();
                        EntityBackpack entityBackpack = new EntityBackpack();
                        entityBackpack = EntityFactory.CreateEntity("Backpack".GetHashCode(), _playerDataFile.ecd.pos + Vector3.up * 2f) as EntityBackpack;
                        entityBackpack.RefPlayerId = _playerDataFile.ecd.clientEntityId;
                        entityBackpack.lootContainer = new TileEntityLootContainer(null);
                        entityBackpack.lootContainer.SetUserAccessing(true);
                        entityBackpack.lootContainer.SetEmpty();
                        entityBackpack.lootContainer.lootListIndex = entityBackpack.GetLootList();
                        entityBackpack.lootContainer.SetContainerSize(LootContainer.lootList[entityBackpack.GetLootList()].size, true);
                        if (All || Bag)
                        {
                            for (int i = 0; i < _playerDataFile.bag.Length; i++)
                            {
                                if (!_playerDataFile.bag[i].IsEmpty())
                                {
                                    entityBackpack.lootContainer.AddItem(_playerDataFile.bag[i]);
                                    _playerDataFile.bag[i] = ItemStack.Empty.Clone();
                                }
                            }
                        }
                        if (All || Belt)
                        {
                            for (int i = 0; i < _playerDataFile.inventory.Length; i++)
                            {
                                if (!_playerDataFile.inventory[i].IsEmpty())
                                {
                                    entityBackpack.lootContainer.AddItem(_playerDataFile.inventory[i]);
                                    _playerDataFile.inventory[i] = ItemStack.Empty.Clone();
                                }
                            }
                        }
                        if (All || Equipment)
                        {
                            ItemValue[] _equipmentValues = _playerDataFile.equipment.GetItems();
                            for (int i = 0; i < _equipmentValues.Length; i++)
                            {
                                if (!_equipmentValues[i].IsEmpty())
                                {
                                    entityBackpack.lootContainer.AddItem(new ItemStack(_equipmentValues[i], 1));
                                }
                            }
                            if (!_playerDataFile.equipment.HasAnyItems())
                            {
                                _playerDataFile.equipment = new Equipment();
                            }
                        }
                        _playerDataFile.droppedBackpackPosition = new Vector3i(_playerDataFile.ecd.pos);
                        entityBackpack.lootContainer.bPlayerBackpack = true;
                        entityBackpack.lootContainer.SetUserAccessing(false);
                        entityBackpack.lootContainer.SetModified();
                        entityBackpack.entityId = -1;
                        entityBackpack.RefPlayerId = _playerDataFile.ecd.clientEntityId;
                        EntityCreationData entityCreationData = new EntityCreationData(entityBackpack);
                        entityCreationData.entityName = string.Format(Localization.Get("playersBackpack", ""), _playerDataFile.ecd.entityName);
                        entityCreationData.id = -1;
                        entityCreationData.lootContainer = entityBackpack.lootContainer.Clone();
                        PersistentOperations.SavePlayerDataFile(_id, _playerDataFile);
                        GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
                        entityBackpack.OnEntityUnload();
                        BattleLogger.Players.Remove(_id);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BattleLogger.Penalty: {0}", e.Message));
            }
        }

        public static void PlayerExit(string _id)
        {
            try
            {
                BattleLogger.Players.Remove(_id);
                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_id);
                if (_cInfo != null)
                {
                    PlayerDataFile _playerDataFile = PersistentOperations.GetPlayerDataFileFromSteamId(_id);
                    if (_playerDataFile != null)
                    {
                        PersistentOperations.SavePlayerDataFile(_id, _playerDataFile);
                    }
                    Disconnect(_cInfo);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BattleLogger.PlayerExit: {0}", e.Message));
            }
        }

        public static void Disconnect(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer entityPlayer = (EntityPlayer)GameManager.Instance.World.GetEntity(_cInfo.entityId);
                if (entityPlayer != null)
                {
                    if (_cInfo.entityId != -1)
                    {
                        Log.Out("Player {0} disconnected after {1} minutes", new object[]
                    {
                GameUtils.SafeStringFormat(entityPlayer.EntityName),
                ((Time.timeSinceLevelLoad - entityPlayer.CreationTimeSinceLevelLoad) / 60f).ToCultureInvariantString("0.0")
                    });
                    }
                }
                GC.Collect();
                MemoryPools.Cleanup();
                PersistentPlayerData persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_cInfo.playerId);
                if (persistentPlayerData != null)
                {
                    persistentPlayerData.LastLogin = DateTime.Now;
                    persistentPlayerData.EntityId = -1;
                }
                PersistentOperations.SavePersistentPlayerDataXML();
                ConnectionManager.Instance.DisconnectClient(_cInfo, false);
                GameManager.Instance.World.aiDirector.RemoveEntity(entityPlayer);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BattleLogger.Disconnect: {0}", e.Message));
            }
        }

        public static void AlertPlayer(ClientInfo _cInfo)
        {
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You must type " + ChatHook.Command_Private + BattleLogger.Command131 + " to leave the game while near a hostile player. Do not worry about an internet drop out or server shutdown.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}

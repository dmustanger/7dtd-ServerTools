using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class BattleLogger
    {
        public static bool IsEnabled = false, Drop = false, Remove = false, All = false, Belt = false, Bag = false, Equipment = false;
        public static string Command131 = "exit", Command132 = "quit";
        public static int Admin_Level = 0, Player_Distance = 200;
        public static Dictionary<string, string> Players = new Dictionary<string, string>();

        public static void ScanLog(string _id, string _ip)
        {
            try
            {
                string _dateTime1 = DateTime.Now.AddSeconds(-2).ToString("yyyy-MM-ddTHH:mm:ss"), _dateTime2 = DateTime.Now.AddSeconds(-3).ToString("yyyy-MM-ddTHH:mm:ss"),
                    _dateTime3 = DateTime.Now.AddSeconds(-4).ToString("yyyy-MM-ddTHH:mm:ss"), _dateTime4 = DateTime.Now.AddSeconds(-5).ToString("yyyy-MM-ddTHH:mm:ss"),
                    _dateTime5 = DateTime.Now.AddSeconds(-6).ToString("yyyy-MM-ddTHH:mm:ss");
                using (FileStream fs = new FileStream(Confirm.LogName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string _line = sr.ReadToEnd();
                        if (_line != null)
                        {
                            if (_line.ToLower().Contains("client disconnect") && _line.ToLower().Contains("disconnectpeercalled") && _line.Contains(_ip) && (_line.Contains(_dateTime1) || _line.Contains(_dateTime2) || _line.Contains(_dateTime3) || _line.Contains(_dateTime4) || _line.Contains(_dateTime5)))
                            {
                                Penalty(_id);
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BattleLogger.ScanLog: {0}", e.Message));
            }
        }

        public static void BattleLog(ClientInfo _cInfo)
        {
            try
            {
                PersistentPlayerData _ppd = PersistentOperations.GetPersistentPlayerDataFromSteamId(_cInfo.playerId);
                PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_cInfo.playerId);
                if (_ppd != null && _pdf != null)
                {
                    if (_ppd.ACL != null && _ppd.ACL.Count > 0)// has friends
                    {
                        List<EntityPlayer> _players = PersistentOperations.PlayerList();
                        if (_players != null && _players.Count > 0)
                        {
                            for (int i = 0; i < _players.Count; i++)
                            {
                                EntityPlayer _player = _players[i];
                                if (_player != null && _player.entityId != _cInfo.entityId && _player.IsAlive() && _player.IsSpawned())// player is alive
                                {
                                    PersistentPlayerData _ppd2 = PersistentOperations.GetPersistentPlayerDataFromEntityId(_player.entityId);
                                    if (_ppd2 != null && _ppd2.ACL.Count > 0)// player has friends
                                    {
                                        if (!_ppd.ACL.Contains(_ppd2.PlayerId) || !_ppd2.ACL.Contains(_ppd.PlayerId))// not a friend
                                        {
                                            if (Player_Distance > 0)// distance check
                                            {
                                                if ((_pdf.ecd.pos.x - _player.position.x) * (_pdf.ecd.pos.x - _player.position.x) + (_pdf.ecd.pos.z - _player.position.z) * (_pdf.ecd.pos.z - _player.position.z) <= Player_Distance * Player_Distance)
                                                {
                                                    Players.TryGetValue(_cInfo.playerId, out string _ip);
                                                    Players.Remove(_cInfo.playerId);
                                                    Timers.BattleLogTool(_cInfo.playerId, _ip);
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                Players.TryGetValue(_cInfo.playerId, out string _ip);
                                                Players.Remove(_cInfo.playerId);
                                                Timers.BattleLogTool(_cInfo.playerId, _ip);
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Player_Distance > 0)// distance check
                                        {
                                            if ((_pdf.ecd.pos.x - _player.position.x) * (_pdf.ecd.pos.x - _player.position.x) + (_pdf.ecd.pos.z - _player.position.z) * (_pdf.ecd.pos.z - _player.position.z) <= Player_Distance * Player_Distance)
                                            {
                                                Players.TryGetValue(_cInfo.playerId, out string _ip);
                                                Players.Remove(_cInfo.playerId);
                                                Timers.BattleLogTool(_cInfo.playerId, _ip);
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            Players.TryGetValue(_cInfo.playerId, out string _ip);
                                            Players.Remove(_cInfo.playerId);
                                            Timers.BattleLogTool(_cInfo.playerId, _ip);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else //no friends
                    {
                        List<EntityPlayer> _players = PersistentOperations.PlayerList();
                        if (_players != null && _players.Count > 0)
                        {
                            for (int i = 0; i < _players.Count; i++)
                            {
                                EntityPlayer _player = _players[i];
                                if (_player != null && _player.entityId != _cInfo.entityId && _player.IsAlive() && _player.IsSpawned())
                                {
                                    if (Player_Distance > 0)
                                    {
                                        if ((_pdf.ecd.pos.x - _player.position.x) * (_pdf.ecd.pos.x - _player.position.x) + (_pdf.ecd.pos.z - _player.position.z) * (_pdf.ecd.pos.z - _player.position.z) <= Player_Distance * Player_Distance)
                                        {
                                            Players.TryGetValue(_cInfo.playerId, out string _ip);
                                            Players.Remove(_cInfo.playerId);
                                            Timers.BattleLogTool(_cInfo.playerId, _ip);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        Players.TryGetValue(_cInfo.playerId, out string _ip);
                                        Players.Remove(_cInfo.playerId);
                                        Timers.BattleLogTool(_cInfo.playerId, _ip);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                Players.Remove(_cInfo.playerId);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.BattleLog: {0}", e.Message));
            }
        }

        private static void Penalty(string _id)
        {
            try
            {
                Log.Out(string.Format("[SERVERTOOLS] Penalty Starting"));
                PlayerDataFile _playerDataFile = PersistentOperations.GetPlayerDataFileFromSteamId(_id);
                if (_playerDataFile != null)
                {
                    PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_id);
                    if (_persistentPlayerData != null)
                    {
                        EntityBackpack entityBackpack = EntityFactory.CreateEntity("Backpack".GetHashCode(), _playerDataFile.ecd.pos + Vector3.up * 2f) as EntityBackpack;
                        TileEntityLootContainer tileEntityLootContainer = new TileEntityLootContainer(null);
                        tileEntityLootContainer.SetUserAccessing(true);
                        tileEntityLootContainer.SetEmpty();
                        tileEntityLootContainer.lootListIndex = entityBackpack.GetLootList();
                        tileEntityLootContainer.SetContainerSize(LootContainer.lootList[entityBackpack.GetLootList()].size, true);
                        if (All || Bag)
                        {
                            for (int i = 0; i < _playerDataFile.bag.Length; i++)
                            {
                                if (!_playerDataFile.bag[i].IsEmpty())
                                {
                                    tileEntityLootContainer.AddItem(_playerDataFile.bag[i]);
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
                                    tileEntityLootContainer.AddItem(_playerDataFile.inventory[i]);
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
                                    tileEntityLootContainer.AddItem(new ItemStack(_equipmentValues[i], 1));
                                }
                            }
                            if (!_playerDataFile.equipment.HasAnyItems())
                            {
                                _playerDataFile.equipment = new Equipment();
                            }
                        }
                        tileEntityLootContainer.SetUserAccessing(false);
                        tileEntityLootContainer.SetModified();
                        entityBackpack.RefPlayerId = _playerDataFile.ecd.clientEntityId;
                        EntityCreationData entityCreationData = new EntityCreationData(entityBackpack);
                        entityCreationData.entityName = string.Format(Localization.Get("playersBackpack"), _playerDataFile.ecd.entityName);
                        entityCreationData.id = -1;
                        entityCreationData.lootContainer = tileEntityLootContainer;
                        GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
                        entityBackpack.OnEntityUnload();
                        _playerDataFile.droppedBackpackPosition = new Vector3i(_playerDataFile.ecd.pos);
                        PersistentOperations.SavePlayerDataFile(_id, _playerDataFile);
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
                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_id);
                if (_cInfo != null)
                {
                    PlayerDataFile _playerDataFile = PersistentOperations.GetPlayerDataFileFromSteamId(_cInfo.playerId);
                    if (_playerDataFile != null)
                    {
                        PersistentOperations.SavePlayerDataFile(_cInfo.playerId, _playerDataFile);
                    }
                    Players.Remove(_cInfo.playerId);
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
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"You have disconnected. Thank you for playing with us. Come back soon\"", _cInfo.playerId), (ClientInfo)null);
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

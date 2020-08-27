using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class BattleLogger
    {
        public static bool IsEnabled = false, Drop = false, Remove = false, All = false, Belt = false, Bag = false, Equipment = false;
        public static string Command131 = "exit", Command132 = "quit";
        public static int Admin_Level = 0, Player_Distance = 200;
        public static Dictionary<string, DateTime> DisconnectedIp = new Dictionary<string, DateTime>();
        public static Dictionary<string, Vector3> ExitPos = new Dictionary<string, Vector3>();
        public static List<string> Exit = new List<string>();

        public static void BattleLog(ClientInfo _cInfo, string _ip)
        {
            try
            {
                if (DisconnectedIp != null && DisconnectedIp.Count > 0)
                {
                    if (DisconnectedIp.ContainsKey(_ip))
                    {
                        if (DisconnectedIp.TryGetValue(_ip, out DateTime _dateTime))
                        {
                            TimeSpan varTime = DateTime.Now - _dateTime;
                            double fractionalSeconds = varTime.TotalSeconds;
                            int _timepassed = (int)fractionalSeconds;
                            if (_timepassed <= 5)
                            {
                                PersistentPlayerData _ppd = PersistentOperations.GetPersistentPlayerDataFromSteamId(_cInfo.playerId);
                                PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_cInfo.playerId);
                                List<EntityPlayer> _players = PersistentOperations.PlayerList();
                                if (_ppd != null && _pdf != null && _players != null && _players.Count > 0)
                                {
                                    if (_ppd.ACL != null && _ppd.ACL.Count > 0)// has friends
                                    {
                                        for (int i = 0; i < _players.Count; i++)
                                        {
                                            EntityPlayer _player = _players[i];
                                            if (_player != null && _player.entityId != _cInfo.entityId && _player.IsAlive() && _player.IsSpawned())// player is alive and spawned
                                            {
                                                PersistentPlayerData _ppd2 = PersistentOperations.GetPersistentPlayerDataFromEntityId(_player.entityId);
                                                if (_ppd2 != null && _ppd2.ACL.Count > 0)// player has friends
                                                {
                                                    if (!_ppd.ACL.Contains(_ppd2.PlayerId) || !_ppd2.ACL.Contains(_ppd.PlayerId))// not a friend
                                                    {
                                                        if (Player_Distance > 0)// distance check
                                                        {
                                                            if ((_pdf.ecd.pos - _player.position).magnitude <= Player_Distance)
                                                            {
                                                                Penalty(_pdf, _cInfo);
                                                                return;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Penalty(_pdf, _cInfo);
                                                            return;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (Player_Distance > 0)// distance check
                                                    {
                                                        if ((_pdf.ecd.pos - _player.position).magnitude <= Player_Distance)
                                                        {
                                                            Penalty(_pdf, _cInfo);
                                                            return;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Penalty(_pdf, _cInfo);
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else //no friends
                                    {
                                        for (int i = 0; i < _players.Count; i++)
                                        {
                                            EntityPlayer _player = _players[i];
                                            if (_player != null && _player.entityId != _cInfo.entityId && _player.IsAlive() && _player.IsSpawned())
                                            {
                                                if (Player_Distance > 0)// distance check
                                                {
                                                    if ((_pdf.ecd.pos - _player.position).magnitude <= Player_Distance)
                                                    {
                                                        Penalty(_pdf, _cInfo);
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    Penalty(_pdf, _cInfo);
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        DisconnectedIp.Remove(_ip);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.BattleLog: {0}", e.Message));
            }
        }

        private static void Penalty(PlayerDataFile _pdf, ClientInfo _cInfo)
        {
            try
            {
                Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)_pdf.ecd.pos.x, (int)_pdf.ecd.pos.y, (int)_pdf.ecd.pos.z);
                if (chunk != null)
                {
                    BlockValue _blockValue = Block.GetBlockValue("cntStorageChest");
                    if (_blockValue.Block != null)
                    {
                        Vector3i _pos = new Vector3i((int)_pdf.ecd.pos.x, (int)_pdf.ecd.pos.y, (int)_pdf.ecd.pos.z);
                        GameManager.Instance.World.SetBlockRPC(chunk.ClrIdx, _pos, _blockValue);
                        TileEntityLootContainer tileEntityLootContainer = GameManager.Instance.World.GetTileEntity(chunk.ClrIdx, _pos) as TileEntityLootContainer;
                        if (tileEntityLootContainer != null)
                        {
                            if (All || Bag)
                            {
                                for (int i = 0; i < _pdf.bag.Length; i++)
                                {
                                    if (!_pdf.bag[i].IsEmpty())
                                    {
                                        tileEntityLootContainer.AddItem(_pdf.bag[i]);
                                        _pdf.bag[i] = ItemStack.Empty.Clone();
                                    }
                                }
                            }
                            if (All || Belt)
                            {
                                for (int i = 0; i < _pdf.inventory.Length; i++)
                                {
                                    if (!_pdf.inventory[i].IsEmpty())
                                    {
                                        tileEntityLootContainer.AddItem(_pdf.inventory[i]);
                                        _pdf.inventory[i] = ItemStack.Empty.Clone();
                                    }
                                }
                            }
                            if (All || Equipment)
                            {
                                ItemValue[] _equipmentValues = _pdf.equipment.GetItems();
                                for (int i = 0; i < _equipmentValues.Length; i++)
                                {
                                    if (!_equipmentValues[i].IsEmpty())
                                    {
                                        ItemStack _itemStack = new ItemStack(_equipmentValues[i], 1);
                                        tileEntityLootContainer.AddItem(_itemStack);
                                        _equipmentValues[i].Clear();
                                    }
                                }
                            }
                            if (tileEntityLootContainer.IsEmpty())
                            {
                                GameManager.Instance.World.SetBlockRPC(chunk.ClrIdx, _pos, BlockValue.Air);
                            }
                            else
                            {
                                tileEntityLootContainer.SetModified();
                            }
                            PersistentOperations.SavePlayerDataFile(_cInfo.playerId, _pdf);
                        }
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
                    if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (_player != null)
                        {
                            if (ExitPos.TryGetValue(_cInfo.playerId, out Vector3 _pos))
                            {
                                ExitPos.Remove(_cInfo.playerId);
                                if (_player.position != _pos)
                                {
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You moved and need to restart your countdown." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                            }
                        }
                    }
                    PlayerDataFile _playerDataFile = PersistentOperations.GetPlayerDataFileFromSteamId(_cInfo.playerId);
                    if (_playerDataFile != null)
                    {
                        PersistentOperations.SavePlayerDataFile(_cInfo.playerId, _playerDataFile);
                    }
                    if (Exit.Contains(_cInfo.playerId))
                    {
                        Exit.Remove(_cInfo.playerId);
                    }
                    if (ExitPos.ContainsKey(_cInfo.playerId))
                    {
                        ExitPos.Remove(_cInfo.playerId);
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
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"You have disconnected. Thank you for playing with us. Come back soon\"", _cInfo.playerId), null);
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

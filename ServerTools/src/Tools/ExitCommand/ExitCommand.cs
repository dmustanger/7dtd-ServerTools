using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ServerTools
{
    class ExitCommand
    {
        public static bool IsEnabled = false, Drop = false, Remove = false, All = false, Belt = false, Bag = false, Equipment = false;
        public static string Command131 = "exit", Command132 = "quit";
        public static int Admin_Level = 0, Exit_Time = 15;
        public static Dictionary<int, Vector3> Players = new Dictionary<int, Vector3>();

        public static void ExitWithoutCommand(ClientInfo _cInfo, string _ip)
        {
            try
            {
                if (OutputLog.ActiveLog.Count > 0)
                {
                    List<string> _log = OutputLog.ActiveLog;
                    _log.Reverse();
                    for (int i = 0; i < 50; i++)
                    {
                        if (!string.IsNullOrEmpty(_log[i]) && _log[i].Contains(_ip) && _log[i].Contains("NET: LiteNetLib: Client disconnect from:"))
                        {
                            if (_log[i].ToLower().Contains("timeout"))
                            {
                                Players.Remove(_cInfo.entityId);
                                return;
                            }
                            else
                            {
                                Players.Remove(_cInfo.entityId);
                                Penalty(_cInfo);
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Players.Remove(_cInfo.entityId);
                Log.Out(string.Format("[SERVERTOOLS] Error in ExitCommand.ExitWithoutCommand: {0}", e.Message));
            }
        }

        private static void Penalty(ClientInfo _cInfo)
        {
            try
            {
                PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_cInfo.playerId);
                if (_pdf != null)
                {
                    Vector3i _pos = new Vector3i((int)_pdf.ecd.pos.x, (int)_pdf.ecd.pos.y, (int)_pdf.ecd.pos.z);
                    if (GameManager.Instance.World.IsChunkAreaLoaded(_pos.x, _pos.y, _pos.z))
                    {
                        Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_pos.x, _pos.y, _pos.z);
                        if (chunk != null)
                        {
                            BlockValue _blockValue = Block.GetBlockValue("cntStorageChest");
                            if (_blockValue.Block != null)
                            {
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
                    else
                    {
                        ChunkManager.ChunkObserver _observer = GameManager.Instance.AddChunkObserver(_pos.ToVector3(), false, 1, -1);
                        if (_observer != null)
                        {
                            Thread.Sleep(1000);
                            Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_pos.x, _pos.y, _pos.z);
                            if (_chunk != null)
                            {
                                BlockValue _blockValue = Block.GetBlockValue("cntStorageChest");
                                if (_blockValue.Block != null)
                                {
                                    EntityBackpack entityBackpack = new EntityBackpack();
                                    entityBackpack = EntityFactory.CreateEntity("Backpack".GetHashCode(), _pdf.ecd.pos + Vector3.up * 2f) as EntityBackpack;
                                    entityBackpack.RefPlayerId = _pdf.ecd.clientEntityId;
                                    entityBackpack.lootContainer = new TileEntityLootContainer(null);
                                    entityBackpack.lootContainer.SetUserAccessing(true);
                                    entityBackpack.lootContainer.SetEmpty();
                                    entityBackpack.lootContainer.lootListIndex = entityBackpack.GetLootList();
                                    entityBackpack.lootContainer.SetContainerSize(LootContainer.lootList[entityBackpack.GetLootList()].size, true);
                                    if (All || Bag)
                                    {
                                        for (int i = 0; i < _pdf.bag.Length; i++)
                                        {
                                            if (!_pdf.bag[i].IsEmpty())
                                            {
                                                entityBackpack.lootContainer.AddItem(_pdf.bag[i]);
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
                                                entityBackpack.lootContainer.AddItem(_pdf.inventory[i]);
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
                                                entityBackpack.lootContainer.AddItem(_itemStack);
                                                _equipmentValues[i].Clear();
                                            }
                                        }
                                    }
                                    _pdf.droppedBackpackPosition = new Vector3i(_pdf.ecd.pos);
                                    entityBackpack.lootContainer.bPlayerBackpack = true;
                                    entityBackpack.lootContainer.SetUserAccessing(false);
                                    entityBackpack.lootContainer.SetModified();
                                    entityBackpack.entityId = -1;
                                    entityBackpack.RefPlayerId = _pdf.ecd.clientEntityId;
                                    EntityCreationData entityCreationData = new EntityCreationData(entityBackpack);
                                    entityCreationData.entityName = string.Format(Localization.Get("playersBackpack"), _pdf.ecd.entityName);
                                    entityCreationData.id = -1;
                                    entityCreationData.lootContainer = entityBackpack.lootContainer.Clone();
                                    PersistentOperations.SavePlayerDataFile(_cInfo.playerId, _pdf);
                                    GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
                                    entityBackpack.OnEntityUnload();
                                }
                            }
                            GameManager.Instance.RemoveChunkObserver(_observer);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ExitCommand.Penalty: {0}", e.Message));
            }
        }

        public static void ExitWithCommand(int _id)
        {
            try
            {
                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_id);
                if (_cInfo != null)
                {
                    if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (_player != null)
                        {
                            if (Players.TryGetValue(_cInfo.entityId, out Vector3 _pos))
                            {
                                if (_player.position != _pos)
                                {
                                    Phrases.Dict.TryGetValue(671, out string _phrase671);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase671 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                                else
                                {
                                    PlayerDataFile _playerDataFile = PersistentOperations.GetPlayerDataFileFromSteamId(_cInfo.playerId);
                                    if (_playerDataFile != null)
                                    {
                                        PersistentOperations.SavePlayerDataFile(_cInfo.playerId, _playerDataFile);
                                    }
                                    Players.Remove(_cInfo.entityId);
                                    Disconnect(_cInfo);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ExitCommand.ExitWithCommand: {0}", e.Message));
            }
        }

        public static void Disconnect(ClientInfo _cInfo)
        {
            try
            {
                Phrases.Dict.TryGetValue(673, out string _phrase673);
                _phrase673 = _phrase673.Replace("{PlayerName}", _cInfo.playerName);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase673), null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ExitCommand.Disconnect: {0}", e.Message));
            }
        }

        public static void AlertPlayer(ClientInfo _cInfo)
        {
            try
            {
                Phrases.Dict.TryGetValue(672, out string _phrase672);
                _phrase672 = _phrase672.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase672 = _phrase672.Replace("{Command131}", Command131);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase672 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ExitCommand.AlertPlayer: {0}", e.Message));
            }
        }
    }
}

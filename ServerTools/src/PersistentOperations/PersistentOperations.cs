using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class PersistentOperations
    {
        private static bool IsRunning = false;
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();

        public static void PlayerCheck()
        {
            try
            {
                if (!IsRunning)
                {
                    IsRunning = true;
                    List<ClientInfo> _cInfoList = API.Players;
                    for (int i = 0; i < _cInfoList.Count; i++)
                    {
                        ClientInfo _cInfo = _cInfoList[i];
                        if (_cInfo != null)
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                            if (_player != null)
                            {
                                if (!_player.IsDead())
                                {
                                    if (_player.IsSpawned() && _player.IsAlive())
                                    {
                                        if (Zones.IsEnabled)
                                        {
                                            Zones.ZoneCheck(_cInfo, _player);
                                        }
                                        if (GodMode.IsEnabled)
                                        {
                                            GodMode.GodCheck(_cInfo);
                                        }
                                    }
                                }
                                else
                                {
                                    if (API.Players.Contains(_cInfo))
                                    {
                                        API.Players.Remove(_cInfo);
                                    }
                                    BedBugExec(_cInfo, _player);
                                    if (BloodmoonWarrior.WarriorList.Contains(_cInfo.entityId))
                                    {
                                        BloodmoonWarrior.WarriorList.Remove(_cInfo.entityId);
                                        BloodmoonWarrior.KilledZombies.Remove(_cInfo.entityId);
                                    }
                                }
                            }
                            else if (API.Players.Contains(_cInfo))
                            {
                                API.Players.Remove(_cInfo);
                            }
                        }
                    }
                }
                IsRunning = false;
            }
            catch (Exception e)
            {
                IsRunning = false;
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.PlayerCheck: {0}.", e.Message));
            }
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            if (!Session.ContainsKey(_cInfo.playerId))
            {
                Session.Add(_cInfo.playerId, DateTime.Now);
            }
        }

        public static void BedBugExec(ClientInfo _cInfo, EntityPlayer _player)
        {
            try
            {
                EntityBedrollPositionList _bedrollList = _player.SpawnPoints;
                if (_bedrollList != null)
                {
                    Vector3i _bedrollPosition = _bedrollList.GetPos();
                    World world = GameManager.Instance.World;
                    BlockValue _blockValue = world.GetBlock(_bedrollPosition);
                    Block _block = _blockValue.Block;
                    if (_block != null && _block is BlockSleepingBag)
                    {
                        PersistentPlayerList _persistentPlayers = PersistentOperations.GetPersistentPlayerList();
                        if (_persistentPlayers != null)
                        {
                            bool _bedBug = false;
                            List<string> _persistentPlayerList = _persistentPlayers.Players.Keys.ToList();
                            for (int j = 0; j < _persistentPlayerList.Count; j++)
                            {
                                PersistentPlayerData _persistentPlayerData;
                                _persistentPlayers.Players.TryGetValue(_persistentPlayerList[j], out _persistentPlayerData);
                                if (_persistentPlayerData.EntityId != _cInfo.entityId && _persistentPlayerData.BedrollPos.Equals(_bedrollPosition))
                                {
                                    _bedBug = true;
                                    PersistentOperations.RemoveAndSetOneSpawnPoint(_persistentPlayerData);
                                }
                            }
                            if (_bedBug)
                            {
                                PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerData(_cInfo.playerId);
                                if (_persistentPlayerData != null)
                                {
                                    PersistentOperations.RemoveAndSetOneSpawnPoint(_persistentPlayerData);
                                }
                                GameManager.Instance.World.SetBlockRPC(0, _bedrollPosition, BlockValue.Air);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.BedBugExec: {0}.", e.Message));
            }
        }

        public static bool BloodMoonSky()
        {
            try
            {
                if (SkyManager.BloodMoon())
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.BloodMoonSky: {0}.", e));
            }
            return false;
        }

        public static bool BloodMoonDuskSky()
        {
            try
            {
                int _daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
                float _duskTime = SkyManager.GetDuskTime();
                float _timeInMinutes = SkyManager.GetTimeOfDayAsMinutes();
                if (_daysRemaining == 0 && !SkyManager.BloodMoon() && _timeInMinutes > _duskTime && !GameManager.Instance.World.IsDark())
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.BloodMoonDuskSky: {0}.", e));
            }
            return false;
        }

        public static bool DuskSky()
        {
            try
            {
                float _duskTime = SkyManager.GetDuskTime();
                float _timeInMinutes = SkyManager.GetTimeOfDayAsMinutes();
                if (!SkyManager.BloodMoon() && _timeInMinutes > _duskTime && !GameManager.Instance.World.IsDark())
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.DuskSky: {0}.", e));
                
            }
            return false;
        }

        public static List<ClientInfo> ClientList()
        {
            List<ClientInfo> _clientList = ConnectionManager.Instance.Clients.List.ToList();
            if (_clientList != null)
            {
                return _clientList;
            }
            return null;
        }

        public static ClientInfo GetClientInfo(string _playerId)
        {
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_playerId);
            if (_cInfo != null)
            {
                return _cInfo;
            }
            return null;
        }

        public static EntityPlayer GetEntityPlayer(string _playerId)
        {
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerData(_playerId);
            if (_persistentPlayerData != null)
            {
                EntityPlayer _entityPlayer = GameManager.Instance.World.Players.dict[_persistentPlayerData.EntityId];
                if (_entityPlayer != null)
                {
                    return _entityPlayer;
                }
            }
            return null;
        }

        public static EntityAlive GetEntityAlive(string _playerId)
        {
            EntityPlayer _entityPlayer = GetEntityPlayer(_playerId);
            if (_entityPlayer != null)
            {
                EntityAlive _entityAlive = (EntityAlive)GameManager.Instance.World.GetEntity(_entityPlayer.entityId);
                if (_entityAlive != null)
                {
                    return _entityAlive;
                }
            }
            return null;
        }

        public static PersistentPlayerList GetPersistentPlayerList()
        {
            PersistentPlayerList _persistentPlayerList = GameManager.Instance.persistentPlayers;
            if (_persistentPlayerList != null)
            {
                return _persistentPlayerList;
            }
            return null;
        }

        public static PersistentPlayerData GetPersistentPlayerData(string _playerId)
        {
            PersistentPlayerList _persistentPlayerList = GameManager.Instance.persistentPlayers;
            if (_persistentPlayerList != null)
            {
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerData(_playerId);
                if (_persistentPlayerData != null)
                {
                    return _persistentPlayerData;
                }
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFile(string _playerId)
        {
            PlayerDataFile _playerDatafile = new PlayerDataFile();
            _playerDatafile.Load(GameUtils.GetPlayerDataDir(), _playerId.Trim());
            if (_playerDatafile != null)
            {
                return _playerDatafile;
            }
            return null;
        }



        public static void RemoveAndSetSpawnPointForAll(PersistentPlayerList _persistentPlayers)
        {
            if (_persistentPlayers != null)
            {
                List<string> _persistentPlayerList = _persistentPlayers.Players.Keys.ToList();
                for (int i = 0; i < _persistentPlayerList.Count; i++)
                {
                    PersistentPlayerData _persistentPlayerData;
                    _persistentPlayers.Players.TryGetValue(_persistentPlayerList[i], out _persistentPlayerData);
                    _persistentPlayerData.ClearBedroll();
                    EntityPlayer _entityPlayer = GetEntityPlayer(_persistentPlayerData.PlayerId);
                    if (_entityPlayer != null)
                    {
                        GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.SleepingBag, _entityPlayer.entityId);
                        ConnectionManager.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.SleepingBag, _entityPlayer.entityId), false, -1, -1, -1, -1);
                        if (_persistentPlayerData.LPBlocks != null && _persistentPlayerData.LPBlocks.Count > 0)
                        {
                            int _x, _y, _z;
                            if (GameManager.Instance.World.FindRandomSpawnPointNearPosition(_persistentPlayerData.LPBlocks[0].ToVector3(), 15, out _x, out _y, out _z, new UnityEngine.Vector3(50f, 50f, 50f), true, true))
                            {
                                _entityPlayer.SpawnPoints.Set(new Vector3i(_x, _y, _z));
                            }
                            else
                            {
                                if (GameManager.Instance.World.FindRandomSpawnPointNearPosition(_persistentPlayerData.LPBlocks[0].ToVector3(), 15, out _x, out _y, out _z, new UnityEngine.Vector3(50f + 10f, 50f + 30f, 50f + 10f), true, true))
                                {
                                    _entityPlayer.SpawnPoints.Set(new Vector3i(_x, _y, _z));
                                }
                            }
                        }
                    }
                }
                SavePersistentPlayerDataXML();
            }
        }

        public static void RemoveAllSpawnPoints(PersistentPlayerList _persistentPlayers)
        {
            if (_persistentPlayers != null)
            {
                List<string> _persistentPlayerList = _persistentPlayers.Players.Keys.ToList();
                for (int i = 0; i < _persistentPlayerList.Count; i++)
                {
                    PersistentPlayerData _persistentPlayerData;
                    _persistentPlayers.Players.TryGetValue(_persistentPlayerList[i], out _persistentPlayerData);
                    EntityPlayer _entityPlayer = GetEntityPlayer(_persistentPlayerData.PlayerId);
                    if (_entityPlayer != null)
                    {
                        _entityPlayer.SpawnPoints.Clear();
                    }
                }
                SavePersistentPlayerDataXML();
            }
        }

        public static void RemoveAndSetOneSpawnPoint(PersistentPlayerData _persistentPlayerData)
        {
            if (_persistentPlayerData != null && _persistentPlayerData.BedrollPos != null)
            {
                _persistentPlayerData.ClearBedroll();
                EntityPlayer _entityPlayer = GetEntityPlayer(_persistentPlayerData.PlayerId);
                if (_entityPlayer != null)
                {
                    GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.SleepingBag, _entityPlayer.entityId);
                    ConnectionManager.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.SleepingBag, _entityPlayer.entityId), false, -1, -1, -1, -1);
                    if (_persistentPlayerData.LPBlocks != null && _persistentPlayerData.LPBlocks.Count > 0)
                    {
                        int _x, _y, _z;
                        if (GameManager.Instance.World.FindRandomSpawnPointNearPosition(_persistentPlayerData.LPBlocks[0].ToVector3(), 15, out _x, out _y, out _z, new UnityEngine.Vector3(50f, 50f, 50f), true, true))
                        {

                            _entityPlayer.SpawnPoints.Set(new Vector3i(_x, _y, _z));
                        }
                        else
                        {
                            if (GameManager.Instance.World.FindRandomSpawnPointNearPosition(_persistentPlayerData.LPBlocks[0].ToVector3(), 15, out _x, out _y, out _z, new UnityEngine.Vector3(50f + 10f, 50f + 30f, 50f + 10f), true, true))
                            {
                                _entityPlayer.SpawnPoints.Set(new Vector3i(_x, _y, _z));
                            }
                        }
                    }
                }
                else
                {
                    ConnectionManager.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.SleepingBag, _persistentPlayerData.EntityId), false, -1, -1, -1, -1);
                }
                SavePersistentPlayerDataXML();
            }
        }

        public static void RemoveSpawnPoint(PersistentPlayerData _persistentPlayerData)
        {
            if (_persistentPlayerData != null)
            {
                _persistentPlayerData.ClearBedroll();
                SavePersistentPlayerDataXML();
                EntityPlayer _entityPlayer = GetEntityPlayer(_persistentPlayerData.PlayerId);
                if (_entityPlayer != null)
                {
                    _entityPlayer.SpawnPoints.Clear();
                }
            }
        }

        public static void RemoveAllClaims(string _playerId)
        {
            PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerData(_playerId);
            if (_persistentPlayerData != null)
            {
                List<Vector3i> landProtectionBlocks = _persistentPlayerData.LPBlocks;
                if (landProtectionBlocks != null)
                {
                    PersistentPlayerList _persistentPlayerList = GetPersistentPlayerList();
                    for (int i = 0; i < landProtectionBlocks.Count; i++)
                    {
                        Vector3i _position = landProtectionBlocks[i];
                        World world = GameManager.Instance.World;
                        BlockValue _blockValue = world.GetBlock(_position);
                        Block _block = _blockValue.Block;
                        if (_block != null && _block is BlockLandClaim)
                        {
                            world.SetBlockRPC(0, _position, BlockValue.Air);
                            ConnectionManager.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, _position.ToVector3()), false, -1, -1, -1, -1);
                            world.ObjectOnMapRemove(EnumMapObjectType.LandClaim, _position.ToVector3());
                            LandClaimBoundsHelper.RemoveBoundsHelper(_position.ToVector3());
                        }
                        GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(_position);
                        _persistentPlayerData.LPBlocks.Remove(_position);
                    }
                    SavePersistentPlayerDataXML();
                }
            }
        }

        public static void RemoveOneClaim(string _playerId, Vector3i _position)
        {
            PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerData(_playerId);
            if (_persistentPlayerData != null)
            {
                World world = GameManager.Instance.World;
                BlockValue _blockValue = world.GetBlock(_position);
                Block _block = _blockValue.Block;
                if (_block != null && _block is BlockLandClaim)
                {
                    world.SetBlockRPC(0, _position, BlockValue.Air);
                    ConnectionManager.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, _position.ToVector3()), false, -1, -1, -1, -1);
                    world.ObjectOnMapRemove(EnumMapObjectType.LandClaim, _position.ToVector3());
                    LandClaimBoundsHelper.RemoveBoundsHelper(_position.ToVector3());
                }
                GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(_position);
                _persistentPlayerData.LPBlocks.Remove(_position);
                SavePersistentPlayerDataXML();
            }
        }

        public static void RemovePersistentPlayerData(string _playerId)
        {
            PersistentPlayerList _persistentPlayerList = GetPersistentPlayerList();
            if (_persistentPlayerList != null)
            {
                if (_persistentPlayerList.Players.ContainsKey(_playerId))
                {
                    _persistentPlayerList.Players.Remove(_playerId);
                    SavePersistentPlayerDataXML();
                }
            }
        }

        public static void RemoveAllACL(string _playerId)
        {
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerData(_playerId);
            if (_persistentPlayerData != null)
            {
                PersistentPlayerList _persistentPlayerList = PersistentOperations.GetPersistentPlayerList();
                foreach (KeyValuePair<string, PersistentPlayerData> _persistentPlayerData2 in _persistentPlayerList.Players)
                {
                    if (_persistentPlayerData2.Key != _persistentPlayerData.PlayerId)
                    {
                        if (_persistentPlayerData2.Value.ACL != null && _persistentPlayerData2.Value.ACL.Contains(_persistentPlayerData.PlayerId))
                        {
                            _persistentPlayerData2.Value.RemovePlayerFromACL(_persistentPlayerData.PlayerId);
                            _persistentPlayerData2.Value.Dispatch(_persistentPlayerData, EnumPersistentPlayerDataReason.ACL_Removed);
                        }
                        if (_persistentPlayerData.ACL != null && _persistentPlayerData.ACL.Contains(_persistentPlayerData2.Value.PlayerId))
                        {
                            _persistentPlayerData.RemovePlayerFromACL(_persistentPlayerData2.Key);
                            _persistentPlayerData.Dispatch(_persistentPlayerData2.Value, EnumPersistentPlayerDataReason.ACL_Removed);
                        }
                    }
                }
                SavePersistentPlayerDataXML();
            }
        }

        public static void SavePlayerDataFile(string _playerId, PlayerDataFile _playerDataFile)
        {
            _playerDataFile.Save(GameUtils.GetPlayerDataDir(), _playerId.Trim());
            ClientInfo _cInfo = GetClientInfo(_playerId);
            if (_cInfo != null)
            {
                ModEvents.SavePlayerData.Invoke(_cInfo, _playerDataFile);
            }
        }

        public static void SavePersistentPlayerDataXML()
        {
            if (GameManager.Instance.persistentPlayers != null)
            {
                GameManager.Instance.persistentPlayers.Write(GameUtils.GetSaveGameDir(null, null) + "/players.xml");
            }
        }

        public static void InitLogWatch()
        {
            Logger.Main.LogCallbacks += LogAction;
        }

        private static void LogAction(string msg, string trace, LogType type)
        {
            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Log out detected. msg = {0}, trace = {1}, type = {2} ", msg, trace, type.ToString()));
        }

        public static void TestExec(ClientInfo _cInfo)
        {
            Log.Out("[SERVERTOOLS] Test operation has begun.");
            try
            {
                //LinkedList<Chunk> chunkArray = new LinkedList<Chunk>();
                //DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
                //ChunkClusterList chunklist = GameManager.Instance.World.ChunkClusters;
                //for (int i = 0; i < chunklist.Count; i++)
                //{
                //    ChunkCluster chunk = chunklist[i];
                //    chunkArray = chunk.GetChunkArray();
                //    foreach (Chunk _c in chunkArray)
                //    {
                //        tiles = _c.GetTileEntities();
                //        foreach (TileEntity tile in tiles.dict.Values)
                //        {
                //            Log.Out(string.Format("[SERVERTOOLS] Tile Entity Type: {0}.", tile.GetTileEntityType().ToString()));
                //        }
                //    }
                //}

                //List<ClientInfo> _clientList = ClientList();
                //for (int i = 0; i < _clientList.Count; i++)
                //{
                //    ClientInfo _cInfo2 = _clientList[i];
                //    EntityPlayer _entityPlayer = PersistentOperations.EntityPlayer(_cInfo2);
                //    EntityBedrollPositionList _bedrollList = _entityPlayer.SpawnPoints;
                //    for (int j = 0; j < _bedrollList.Count; j++)
                //    {
                //        Vector3i _bedrollPosition = _bedrollList[j];
                //        Log.Out(string.Format("[SERVERTOOLS] _entityPlayer id = {0} , _bedrollPosition: {1}.", _entityPlayer.entityId, _bedrollPosition));
                //    }
                //}


                //EntityAlive _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                //if (_player != null && _player.IsAlive())
                //{
                //    int _deathCount = _player.Died - 1;
                //    _player.Died = _deathCount;
                //    _player.bPlayerStatsChanged = true;
                //    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(_player));
                //    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have survived and been rewarded by hades himself. Your death count was reduced by one." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                //}
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TestOperation.TestExec: {0}.", e.Message));
            }
        }
    }
}

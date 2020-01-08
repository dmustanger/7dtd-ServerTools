using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class PersistentOperations
    {
        private static bool IsRunning = false;
        public static int MaxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();

        public static void PlayerCheck()
        {
            try
            {
                if (!IsRunning)
                {
                    IsRunning = true;
                    List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                    if (_cInfoList != null)
                    {
                        for (int i = 0; i < _cInfoList.Count; i++)
                        {
                            ClientInfo _cInfo = _cInfoList[i];
                            if (_cInfo != null && !string.IsNullOrEmpty(_cInfo.playerId) && _cInfo.entityId > 0)
                            {
                                EntityAlive _player = PersistentOperations.GetPlayerAlive(_cInfo.playerId);
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
                                        if (BloodmoonWarrior.IsEnabled && BloodmoonWarrior.WarriorList.Contains(_cInfo.entityId))
                                        {
                                            BloodmoonWarrior.WarriorList.Remove(_cInfo.entityId);
                                            BloodmoonWarrior.KilledZombies.Remove(_cInfo.entityId);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.PlayerCheck: {0}.", e.Message));
            }
            IsRunning = false;
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            if (!Session.ContainsKey(_cInfo.playerId))
            {
                Session.Add(_cInfo.playerId, DateTime.Now);
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

        public static ClientInfo GetClientInfoFromSteamId(string _playerId)
        {
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_playerId);
            if (_cInfo != null)
            {
                return _cInfo;
            }
            return null;
        }

        public static ClientInfo GetClientInfoFromEntityId(int _playerId)
        {
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_playerId);
            if (_cInfo != null)
            {
                return _cInfo;
            }
            return null;
        }

        public static List<EntityPlayer> PlayerList()
        {
            List<EntityPlayer> _playerList = GameManager.Instance.World.Players.list;
            if (_playerList != null)
            {
                return _playerList;
            }
            return null;
        }

        public static EntityPlayer GetEntityPlayer(string _playerId)
        {
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromSteamId(_playerId);
            if (_persistentPlayerData != null)
            {
                if (GameManager.Instance.World.Players.dict.ContainsKey(_persistentPlayerData.EntityId))
                {
                    EntityPlayer _entityPlayer = GameManager.Instance.World.Players.dict[_persistentPlayerData.EntityId];
                    if (_entityPlayer != null)
                    {
                        return _entityPlayer;
                    }
                }
            }
            return null;
        }

        public static EntityAlive GetPlayerAlive(string _playerId)
        {
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromSteamId(_playerId);
            if (_persistentPlayerData != null)
            {
                if (GameManager.Instance.World.Players.dict.ContainsKey(_persistentPlayerData.EntityId))
                {
                    EntityAlive _entityAlive = (EntityAlive)GetEntity(_persistentPlayerData.EntityId);
                    if (_entityAlive != null)
                    {
                        return _entityAlive;
                    }
                }
            }
            return null;
        }

        public static Entity GetEntity(int _id)
        {
            Entity _entity = GameManager.Instance.World.GetEntity(_id);
            if (_entity != null)
            {
                return _entity;
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

        public static PersistentPlayerData GetPersistentPlayerDataFromSteamId(string _playerId)
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

        public static PersistentPlayerData GetPersistentPlayerDataFromEntityId(int _entityId)
        {
            PersistentPlayerList _persistentPlayerList = GameManager.Instance.persistentPlayers;
            if (_persistentPlayerList != null)
            {
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(_entityId);
                if (_persistentPlayerData != null)
                {
                    return _persistentPlayerData;
                }
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromSteamId(string _playerId)
        {
            PlayerDataFile _playerDatafile = new PlayerDataFile();
            _playerDatafile.Load(GameUtils.GetPlayerDataDir(), _playerId.Trim());
            if (_playerDatafile != null)
            {
                return _playerDatafile;
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromEntityId(int _entityId)
        {
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromEntityId(_entityId);
            if (_persistentPlayerData != null)
            {
                PlayerDataFile _playerDatafile = new PlayerDataFile();
                _playerDatafile.Load(GameUtils.GetPlayerDataDir(), _persistentPlayerData.PlayerId.Trim());
                if (_playerDatafile != null)
                {
                    return _playerDatafile;
                }
            }
            return null;
        }

        public static void SetSpawnPointNearClaim(ClientInfo _cInfo, EntityPlayer _entityPlayer)
        {
            if (_cInfo != null)
            {
                PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_cInfo.playerId);
                if (_persistentPlayerData != null && _persistentPlayerData.LPBlocks != null && _persistentPlayerData.LPBlocks.Count > 0)
                {
                    int _x, _y, _z;
                    if (GameManager.Instance.World.FindRandomSpawnPointNearPosition(_persistentPlayerData.LPBlocks[0].ToVector3(), 15, out _x, out _y, out _z, new UnityEngine.Vector3(50f, 50f, 50f), true, false))
                    {
                        _entityPlayer.SpawnPoints.Set(new Vector3i(_x, _y, _z));
                    }
                    else
                    {
                        if (GameManager.Instance.World.FindRandomSpawnPointNearPosition(_persistentPlayerData.LPBlocks[0].ToVector3(), 15, out _x, out _y, out _z, new UnityEngine.Vector3(500f, 70f, 500f), true, false))
                        {
                            _entityPlayer.SpawnPoints.Set(new Vector3i(_x, _y, _z));
                        }
                    }
                }
            }
        }

        public static void RemoveAllClaims(string _playerId)
        {
            PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_playerId);
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
            PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_playerId);
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
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromSteamId(_playerId);
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
            ClientInfo _cInfo = GetClientInfoFromSteamId(_playerId);
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

        public static bool ClaimedBySelf(string _id, Vector3i _position)
        {
            PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_id);
            if (_persistentPlayerData != null)
            {
                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(_position, _persistentPlayerData);
                if (_owner == EnumLandClaimOwner.Self)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ClaimedBySelfVec3(string _id, Vector3i _position)
        {
            PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_id);
            if (_persistentPlayerData != null)
            {
                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(_position, _persistentPlayerData);
                if (_owner == EnumLandClaimOwner.Self)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ClaimedByAlly(string _id, Vector3i _position)
        {
            PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_id);
            if (_persistentPlayerData != null)
            {
                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(_position, _persistentPlayerData);
                if (_owner == EnumLandClaimOwner.Ally)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ClaimedByNone(string _id, Vector3i _position)
        {
            PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_id);
            if (_persistentPlayerData != null)
            {
                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(_position, _persistentPlayerData);
                if (_owner == EnumLandClaimOwner.None)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ClaimedByAllyOrSelf(string _id, Vector3i _position)
        {
            PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_id);
            if (_persistentPlayerData != null)
            {
                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(_position, _persistentPlayerData);
                if (_owner == EnumLandClaimOwner.Ally || _owner == EnumLandClaimOwner.Self)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ClaimedByUnknown(string _id, Vector3i _position)
        {
            PersistentPlayerData _persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_id);
            if (_persistentPlayerData != null)
            {
                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(_position, _persistentPlayerData);
                if (_owner != EnumLandClaimOwner.None && _owner != EnumLandClaimOwner.Ally && _owner != EnumLandClaimOwner.Self)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<EntityPlayer> PlayersWithin100Blocks(int _x, int _z)
        {
            List<EntityPlayer> _players = new List<EntityPlayer>();
            List<EntityPlayer> _playerList = PersistentOperations.PlayerList();
            for (int i = 0; i < _playerList.Count; i++)
            {
                EntityPlayer _player = _playerList[i];
                if (!_player.IsDead() && _player.Spawned)
                {
                    if ((_x - _player.position.x) * (_x - _player.position.x) + (_z - _player.position.z) * (_z - _player.position.z) <= 100 * 100)
                    {
                        _players.Add(_player);
                    }
                }
            }
            if (_players.Count > 0)
            {
                return _players;
            }
            return null;
        }

        public static List<EntityPlayer> PlayersWithin200Blocks(int _x, int _z)
        {
            List<EntityPlayer> _players = new List<EntityPlayer>();
            List<EntityPlayer> _playerList = PersistentOperations.PlayerList();
            for (int i = 0; i < _playerList.Count; i++)
            {
                EntityPlayer _player = _playerList[i];
                if (!_player.IsDead() && _player.Spawned)
                {
                    if ((_x - _player.position.x) * (_x - _player.position.x) + (_z - _player.position.z) * (_z - _player.position.z) <= 200 * 200)
                    {
                        _players.Add(_player);
                    }
                }
            }
            if (_players.Count > 0)
            {
                return _players;
            }
            return null;
        }

        public static List<ClientInfo> ClientsWithin200Blocks(int _x, int _z)
        {
            List<ClientInfo> _clients = new List<ClientInfo>();
            List<ClientInfo> _clientList = PersistentOperations.ClientList();
            for (int i = 0; i < _clientList.Count; i++)
            {
                ClientInfo _cInfo = _clientList[i];
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (_player != null)
                {
                    if (!_player.IsDead() && _player.Spawned)
                    {
                        if ((_x - _player.position.x) * (_x - _player.position.x) + (_z - _player.position.z) * (_z - _player.position.z) <= 200 * 200)
                        {
                            _clients.Add(_cInfo);
                        }
                    }
                }
            }
            if (_clients.Count > 0)
            {
                return _clients;
            }
            return null;
        }

        public static void ClearChunkProtection(ClientInfo _cInfo)
        {
            try
            {
                List<Chunk> _chunkList = new List<Chunk>();
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (_player != null)
                {
                    Vector3 _position = _player.position;
                    int _x = (int)_position.x, _z = (int)_position.z;
                    if (GameManager.Instance.World.IsChunkAreaLoaded(_x, 1, _z))
                    {
                        Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_x, 1, _z);
                        if (!_chunkList.Contains(_chunk))
                        {
                            _chunkList.Add(_chunk);
                        }
                        Bounds bounds = _chunk.GetAABB();
                        for (int i = (int)bounds.min.x; i < (int)bounds.max.x; i++)
                        {
                            for (int j = (int)bounds.min.z; j < (int)bounds.max.z; j++)
                            {
                                _x = i - (int)bounds.min.x;
                                _z = j - (int)bounds.min.z;
                                _chunk.SetTraderArea(_x, _z, false);
                            }
                        }
                    }
                }
                if (_chunkList.Count > 0)
                {
                    for (int k = 0; k < _chunkList.Count; k++)
                    {
                        Chunk _chunk = _chunkList[k];
                        List<ClientInfo> _clientList = PersistentOperations.ClientList();
                        if (_clientList != null && _clientList.Count > 0)
                        {
                            for (int l = 0; l < _clientList.Count; l++)
                            {
                                ClientInfo _cInfo2 = _clientList[l];
                                if (_cInfo2 != null)
                                {
                                    _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.ClearChunkProtection: {0}", e.Message));
            }
        }

        public static void BedBug(string _persistentPlayerId)
        {
            try
            {
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
                if (_player != null)
                {
                    EntityBedrollPositionList _bedrollList = _player.SpawnPoints;
                    if (_bedrollList != null)
                    {
                        Vector3i _bedrollPosition = _bedrollList.GetPos();
                        List<EntityPlayer> _playerList = PersistentOperations.PlayerList();
                        if (_bedrollPosition != null && _playerList != null)
                        {
                            for (int i = 0; i < _playerList.Count; i++)
                            {
                                EntityPlayer _player2 = _playerList[i];
                                if (_player2 != null && _player2 != _player && _player2.SpawnPoints != null && _player2.SpawnPoints.GetPos().Equals(_bedrollPosition))
                                {
                                    PersistentPlayerData _ppd = PersistentOperations.GetPersistentPlayerDataFromSteamId(_persistentPlayerId);
                                    if (_ppd != null && !_player2.SpawnPoints.GetPos().Equals(_ppd.BedrollPos))
                                    {
                                        _player2.SpawnPoints.Set(_ppd.BedrollPos);
                                        continue;
                                    }
                                    _player2.SpawnPoints.Clear();
                                    GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.SleepingBag, _player2.entityId);
                                    ConnectionManager.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.SleepingBag, _player2.entityId), false, -1, -1, -1, -1);
                                    ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromEntityId(_player2.entityId);
                                    if (_cInfo2 != null)
                                    {
                                        ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + "Detected bug with your bed. Check your bed and replace if needed[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.BedBug: {0}", e.Message));
            }
        }

        public static void ClaimBug(string _persistentPlayerId)
        {

        }

        public static void InitLogWatch()
        {
            Logger.Main.LogCallbacks += LogAction;
        }

        private static void LogAction(string msg, string trace, LogType type)
        {
            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Log out detected. msg = {0}, trace = {1}, type = {2}", msg, trace, type.ToString()));
        }

        public static void TestExec1(ClientInfo _cInfo)
        {
            Log.Out("[SERVERTOOLS] Test operation 1 has begun.");
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

                //Log.Out(string.Format("[SERVERTOOLS] Test 1"));
                //List<string[]> _protectedList = ProtectedSpace.ProtectedList;
                //List<Chunk> _chunkList = new List<Chunk>();
                //string[] _vector = { "-20", "-20", "-18", "-18" };
                //if (!_protectedList.Contains(_vector))
                //{
                //    _protectedList.Add(_vector);
                //}
                //int _xMin = int.Parse(_vector[0]), _zMin = int.Parse(_vector[1]), _xMax = int.Parse(_vector[2]), _zMax = int.Parse(_vector[3]);
                //Log.Out(string.Format("[SERVERTOOLS] Test 2"));
                //for (int i = _xMin; i <= _xMax; i++)
                //{
                //    for (int j = _zMin; j <= _zMax; j++)
                //    {
                //        Log.Out(string.Format("[SERVERTOOLS] Test 3"));
                //        if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
                //        {
                //            Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(i, 1, j);
                //            if (!_chunkList.Contains(_chunk))
                //            {
                //                _chunkList.Add(_chunk);
                //            }
                //            Log.Out(string.Format("[SERVERTOOLS] Test 4 {0}x {1}z", i, j));
                //            Bounds bounds = _chunk.GetAABB();
                //            Log.Out(string.Format("[SERVERTOOLS] (int)bounds.min.x = {0}, (int)bounds.max.x = {1}, (int)bounds.min.z = {2}, (int)bounds.max.z = {3}", (int)bounds.min.x, (int)bounds.max.x, (int)bounds.min.z, (int)bounds.max.z));
                //            if (i >= (int)bounds.min.x && i <= (int)bounds.max.x && j >= (int)bounds.min.z && j <= (int)bounds.max.z)
                //            {
                //                int _x = i, _z = j;
                //                if (i < 0)
                //                {
                //                    _x = i * -1;
                //                }
                //                if (j < 0)
                //                {
                //                    _z = j * -1;
                //                }
                //                Log.Out(string.Format("[SERVERTOOLS] Test 5"));
                //                _chunk.SetTraderArea(_x, _z, true);
                //                Log.Out(string.Format("[SERVERTOOLS] Set area"));
                //            }
                //        }
                //        else
                //        {
                //            continue;
                //        }
                //    }
                //}
                //ProtectedSpace.ProtectedList = _protectedList;
                //if (_chunkList.Count > 0)
                //{
                //    for (int k = 0; k < _chunkList.Count; k++)
                //    {
                //        Chunk _chunk = _chunkList[k];
                //        List<ClientInfo> _clientList = PersistentOperations.ClientList();
                //        if (_clientList != null && _clientList.Count > 0)
                //        {
                //            for (int l = 0; l < _clientList.Count; l++)
                //            {
                //                ClientInfo _cInfo2 = _clientList[l];
                //                if (_cInfo2 != null)
                //                {
                //                    _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                //                }
                //            }
                //        }
                //    }
                //}

                //Log.Out(string.Format("[SERVERTOOLS] Test 1"));
                //List<Chunk> _chunkList = new List<Chunk>();
                //string[] _vector = { "0", "0", "15", "15" };
                //int _xMin = int.Parse(_vector[0]), _zMin = int.Parse(_vector[1]), _xMax = int.Parse(_vector[2]), _zMax = int.Parse(_vector[3]);
                //Log.Out(string.Format("[SERVERTOOLS] Test 2"));
                //for (int i = _xMin; i <= _xMax; i++)
                //{
                //    for (int j = _zMin; j <= _zMax; j++)
                //    {
                //        Log.Out(string.Format("[SERVERTOOLS] Test 3"));
                //        if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
                //        {
                //            Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(i, 1, j);
                //            if (!_chunkList.Contains(_chunk))
                //            {
                //                _chunkList.Add(_chunk);
                //                Log.Out(string.Format("[SERVERTOOLS] Test 4 {0}x {1}z", i, j));
                //            }
                //            Bounds bounds = _chunk.GetAABB();
                //            Log.Out(string.Format("[SERVERTOOLS] (int)bounds.min.x = {0}, (int)bounds.max.x = {1}, (int)bounds.min.z = {2}, (int)bounds.max.z = {3}", (int)bounds.min.x, (int)bounds.max.x, (int)bounds.min.z, (int)bounds.max.z));
                //            if (i >= (int)bounds.min.x && i <= (int)bounds.max.x && j >= (int)bounds.min.z && j <= (int)bounds.max.z)
                //            {
                //                int _x = i - (int)bounds.min.x, _z = j - (int)bounds.min.z;
                //                Log.Out(string.Format("[SERVERTOOLS] Test 5, _x = {0}, _z = {1}", _x, _z));
                //                _chunk.SetTraderArea(_x, _z, true);
                //                Log.Out(string.Format("[SERVERTOOLS] Area protected"));
                //            }
                //        }
                //    }
                //}
                //if (_chunkList.Count > 0)
                //{
                //    for (int k = 0; k < _chunkList.Count; k++)
                //    {
                //        Chunk _chunk = _chunkList[k];
                //        List<ClientInfo> _clientList = PersistentOperations.ClientList();
                //        if (_clientList != null && _clientList.Count > 0)
                //        {
                //            for (int l = 0; l < _clientList.Count; l++)
                //            {
                //                ClientInfo _cInfo2 = _clientList[l];
                //                if (_cInfo2 != null)
                //                {
                //                    _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TestOperation.TestExec1: {0}", e.Message));
            }
        }

        //public static void TestExec2(ClientInfo _cInfo)
        //{
        //    try
        //    {
        //        Log.Out(string.Format("[SERVERTOOLS] Test 6"));
        //        List<Chunk> _chunkList = new List<Chunk>();
        //        string[] _vector = { "0", "0", "15", "15" };
        //        int _xMin = int.Parse(_vector[0]), _zMin = int.Parse(_vector[1]), _xMax = int.Parse(_vector[2]), _zMax = int.Parse(_vector[3]);
        //        Log.Out(string.Format("[SERVERTOOLS] Test 7"));
        //        for (int i = _xMin; i <= _xMax; i++)
        //        {
        //            for (int j = _zMin; j <= _zMax; j++)
        //            {
        //                Log.Out(string.Format("[SERVERTOOLS] Test 8"));
        //                if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
        //                {
        //                    Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(i, 1, j);
        //                    if (!_chunkList.Contains(_chunk))
        //                    {
        //                        _chunkList.Add(_chunk);
        //                        Log.Out(string.Format("[SERVERTOOLS] Test 9 {0}x {1}z", i, j));
        //                    }
        //                    Bounds bounds = _chunk.GetAABB();
        //                    Log.Out(string.Format("[SERVERTOOLS] (int)bounds.min.x = {0}, (int)bounds.max.x = {1}, (int)bounds.min.z = {2}, (int)bounds.max.z = {3}", (int)bounds.min.x, (int)bounds.max.x, (int)bounds.min.z, (int)bounds.max.z));
        //                    if (i >= (int)bounds.min.x && i <= (int)bounds.max.x && j >= (int)bounds.min.z && j <= (int)bounds.max.z)
        //                    {
        //                        int _x = i - (int)bounds.min.x, _z = j - (int)bounds.min.z;
        //                        Log.Out(string.Format("[SERVERTOOLS] Test 10, _x = {0}, _z = {1}", _x, _z));
        //                        _chunk.SetTraderArea(_x, _z, false);
        //                        Log.Out(string.Format("[SERVERTOOLS] Protection disabled"));
        //                    }
        //                }
        //            }
        //        }
        //        if (_chunkList.Count > 0)
        //        {
        //            for (int k = 0; k < _chunkList.Count; k++)
        //            {
        //                Chunk _chunk = _chunkList[k];
        //                List<ClientInfo> _clientList = PersistentOperations.ClientList();
        //                if (_clientList != null && _clientList.Count > 0)
        //                {
        //                    for (int l = 0; l < _clientList.Count; l++)
        //                    {
        //                        ClientInfo _cInfo2 = _clientList[l];
        //                        if (_cInfo2 != null)
        //                        {
        //                            _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Out(string.Format("[SERVERTOOLS] Error in TestOperation.TestExec2: {0}", e.Message));
        //    }
        //}
        //
        //public static void TestExec3(ClientInfo _cInfo)
        //{
        //    try
        //    {
        //        Log.Out(string.Format("[SERVERTOOLS] Test 11"));
        //        List<Chunk> _chunkList = new List<Chunk>();
        //        string[] _vector = { "-20", "-20", "-18", "-18" };
        //        int _xMin = int.Parse(_vector[0]), _zMin = int.Parse(_vector[1]), _xMax = int.Parse(_vector[2]), _zMax = int.Parse(_vector[3]);
        //        Log.Out(string.Format("[SERVERTOOLS] Test 12"));
        //        for (int i = _xMin; i <= _xMax; i++)
        //        {
        //            for (int j = _zMin; j <= _zMax; j++)
        //            {
        //                Log.Out(string.Format("[SERVERTOOLS] Test 13"));
        //                if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
        //                {
        //                    Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(i, 1, j);
        //                    if (!_chunkList.Contains(_chunk))
        //                    {
        //                        _chunkList.Add(_chunk);
        //                        Log.Out(string.Format("[SERVERTOOLS] Test 14 {0}x {1}z", i, j));
        //                    }
        //                    Bounds bounds = _chunk.GetAABB();
        //                    Log.Out(string.Format("[SERVERTOOLS] (int)bounds.min.x = {0}, (int)bounds.max.x = {1}, (int)bounds.min.z = {2}, (int)bounds.max.z = {3}", (int)bounds.min.x, (int)bounds.max.x, (int)bounds.min.z, (int)bounds.max.z));
        //                    if (i >= (int)bounds.min.x && i <= (int)bounds.max.x && j >= (int)bounds.min.z && j <= (int)bounds.max.z)
        //                    {
        //                        int _x = i - (int)bounds.min.x, _z = j - (int)bounds.min.z;
        //                        Log.Out(string.Format("[SERVERTOOLS] Test 15, _x = {0}x, _z = {1}z", _x, _z));
        //                        _chunk.SetTraderArea(_x, _z, true);
        //                        Log.Out(string.Format("[SERVERTOOLS] Area protected"));
        //                    }
        //                }
        //            }
        //        }
        //        if (_chunkList.Count > 0)
        //        {
        //            for (int k = 0; k < _chunkList.Count; k++)
        //            {
        //                Chunk _chunk = _chunkList[k];
        //                List<ClientInfo> _clientList = PersistentOperations.ClientList();
        //                if (_clientList != null && _clientList.Count > 0)
        //                {
        //                    for (int l = 0; l < _clientList.Count; l++)
        //                    {
        //                        ClientInfo _cInfo2 = _clientList[l];
        //                        if (_cInfo2 != null)
        //                        {
        //                            _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Out(string.Format("[SERVERTOOLS] Error in TestOperation.TestExec3: {0}", e.Message));
        //    }
        //}
        //
        //public static void TestExec4(ClientInfo _cInfo)
        //{
        //    try
        //    {
        //        Log.Out(string.Format("[SERVERTOOLS] Test 16"));
        //        List<Chunk> _chunkList = new List<Chunk>();
        //        string[] _vector = { "-20", "-20", "-18", "-18" };
        //        int _xMin = int.Parse(_vector[0]), _zMin = int.Parse(_vector[1]), _xMax = int.Parse(_vector[2]), _zMax = int.Parse(_vector[3]);
        //        Log.Out(string.Format("[SERVERTOOLS] Test 17"));
        //        for (int i = _xMin; i <= _xMax; i++)
        //        {
        //            for (int j = _zMin; j <= _zMax; j++)
        //            {
        //                Log.Out(string.Format("[SERVERTOOLS] Test 18"));
        //                if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
        //                {
        //                    Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(i, 1, j);
        //                    if (!_chunkList.Contains(_chunk))
        //                    {
        //                        _chunkList.Add(_chunk);
        //                        Log.Out(string.Format("[SERVERTOOLS] Test 19 {0}x {1}z", i, j));
        //                    }
        //                    Bounds bounds = _chunk.GetAABB();
        //                    Log.Out(string.Format("[SERVERTOOLS] (int)bounds.min.x = {0}, (int)bounds.max.x = {1}, (int)bounds.min.z = {2}, (int)bounds.max.z = {3}", (int)bounds.min.x, (int)bounds.max.x, (int)bounds.min.z, (int)bounds.max.z));
        //                    if (i >= (int)bounds.min.x && i <= (int)bounds.max.x && j >= (int)bounds.min.z && j <= (int)bounds.max.z)
        //                    {
        //                        int _x = i - (int)bounds.min.x, _z = j - (int)bounds.min.z;
        //                        Log.Out(string.Format("[SERVERTOOLS] Test 20, setting chunk point _x = {0}x, j = {1}z", _x, _z));
        //                        _chunk.SetTraderArea(_x, _z, false);
        //                        Log.Out(string.Format("[SERVERTOOLS] Protection disabled"));
        //                    }
        //                }
        //            }
        //        }
        //        if (_chunkList.Count > 0)
        //        {
        //            for (int k = 0; k < _chunkList.Count; k++)
        //            {
        //                Chunk _chunk = _chunkList[k];
        //                List<ClientInfo> _clientList = PersistentOperations.ClientList();
        //                if (_clientList != null && _clientList.Count > 0)
        //                {
        //                    for (int l = 0; l < _clientList.Count; l++)
        //                    {
        //                        ClientInfo _cInfo2 = _clientList[l];
        //                        if (_cInfo2 != null)
        //                        {
        //                            _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Out(string.Format("[SERVERTOOLS] Error in TestOperation.TestExec4: {0}", e.Message));
        //    }
        //}
        //
        public static void TestExec5(ClientInfo _cInfo)
        {
            try
            {
                Log.Out(string.Format("[SERVERTOOLS] Test 1"));
                int _bloodmoonFrequency = GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency);
                int _bloodmoonRange = GamePrefs.GetInt(EnumGamePrefs.BloodMoonRange);
                int _worldTimeToDays = GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime());
                int _worldTimeToHours = GameUtils.WorldTimeToHours(GameManager.Instance.World.GetWorldTime());
                Log.Out(string.Format("[SERVERTOOLS] Test 2, _bloodmoonFrequency = {0}, _bloodmoonRange = {1}, _worldTimeToDays = {2}, _worldTimeToHours = {3}", _bloodmoonFrequency, _bloodmoonRange, _worldTimeToDays, _worldTimeToHours));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TestOperation.TestExec5: {0}", e.Message));
            }
        }

        public static void ConfigChange(string _target, string _replacement)
        {
            LoadConfig.fileWatcher.EnableRaisingEvents = false;
            List<string> _oldConfig = new List<string>();
            using (FileStream fs1 = new FileStream(LoadConfig.configFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs1, Encoding.UTF8))
                {
                    for (int i = 0; i < int.MaxValue; i++)
                    {
                        string _line = sr.ReadLine();
                        if (_line != null)
                        {
                            if (!_line.Contains(_target))
                            {
                                _oldConfig.Add(_line);
                            }
                            else
                            {
                                _oldConfig.Add(_replacement);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            if (_oldConfig.Count > 0)
            {
                using (FileStream fs2 = new FileStream(LoadConfig.configFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs2, Encoding.UTF8))
                    {
                        for (int i = 0; i < _oldConfig.Count; i++)
                        {
                            string _line = _oldConfig[i];
                            sw.WriteLine(_line);
                        }
                    }
                }
            }
            LoadConfig.fileWatcher.EnableRaisingEvents = true;
            LoadConfig.LoadXml();
            Mods.Load();
        }
    }
}

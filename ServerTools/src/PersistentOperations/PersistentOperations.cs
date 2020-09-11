using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class PersistentOperations
    {
        private static bool IsRunning = false;
        public static int Jail_Violation = 4, Kill_Violation = 6, Kick_Violation = 8, Ban_Violation = 10;
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static Dictionary<int, int> PvEViolations = new Dictionary<int, int>();
        public static Dictionary<int, int> EntityId = new Dictionary<int, int>();

        public static void PlayerCheck()
        {
            try
            {
                if (!IsRunning)
                {
                    IsRunning = true;
                    List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                    if (_cInfoList != null && _cInfoList.Count > 0)
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
                                            if (Lobby.IsEnabled)
                                            {
                                                Lobby.LobbyCheck(_cInfo, _player);
                                            }
                                            if (Market.IsEnabled)
                                            {
                                                Market.MarketCheck(_cInfo, _player);
                                            }
                                        }
                                    }
                                    else if (BloodmoonWarrior.IsEnabled && BloodmoonWarrior.WarriorList.Contains(_cInfo.playerId))
                                    {
                                        BloodmoonWarrior.WarriorList.Remove(_cInfo.playerId);
                                        BloodmoonWarrior.KilledZombies.Remove(_cInfo.playerId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.PlayerCheck: {0}", e.Message));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.BloodMoonSky: {0}", e.Message));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.BloodMoonDuskSky: {0}", e.Message));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.DuskSky: {0}", e.Message));

            }
            return false;
        }

        public static bool BloodMoonOver()
        {
            try
            {
                if (Shutdown.Bloodmoon && !SkyManager.BloodMoon() && !GameManager.Instance.World.IsDark())
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.BloodMoonOver: {0}", e.Message));
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
            if (GameManager.Instance.World.Players.list != null && GameManager.Instance.World.Players.list.Count > 0)
            {
                return GameManager.Instance.World.Players.list;
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

        public static void EntityIdList()
        {
            if (EntityClass.list.Dict != null && EntityClass.list.Dict.Count > 0)
            {
                int _count = 1;
                foreach (KeyValuePair<int, EntityClass> _entityClass in EntityClass.list.Dict)
                {
                    if (_entityClass.Value.bAllowUserInstantiate)
                    {
                        EntityId.Add(_count, _entityClass.Key);
                        _count++;
                    }
                }
            }
        }
    }
}

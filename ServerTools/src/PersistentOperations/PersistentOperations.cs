using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace ServerTools
{
    class PersistentOperations
    {
        public static bool IsRunning = false, Shutdown_Initiated = false;
        public static int Jail_Violation = 4, Kill_Violation = 6, Kick_Violation = 8, Ban_Violation = 10, Player_Killing_Mode = 0;

        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static Dictionary<int, int> EntityId = new Dictionary<int, int>();

        public static Dictionary<int, int> PvEViolations = new Dictionary<int, int>();

        public static readonly string AlphaNumSet = "jJkqQr9Kl3wXAbyYz0ZLmFpPRsMn5NoO6dDe1EfStaBc2CgGhH7iITu4U8vWxV";
        public static readonly char[] InvalidPrefix = new char[] { '!', '@', '#', '$', '%', '&', '/', '\\' };

        public static void SetInstallFolder()
        {
            string _mainDir = Directory.GetCurrentDirectory();
            string[] _directories = Directory.GetDirectories(_mainDir);
            if (_directories.Length > 0)
            {
                for (int i = 0; i < _directories.Length; i++)
                {
                    if (_directories[i].Contains("Mods"))
                    {
                        string[] _childDirectories = Directory.GetDirectories(_directories[i]);
                        if (_childDirectories.Length > 0)
                        {
                            for (int j = 0; j < _childDirectories.Length; j++)
                            {
                                if (_childDirectories[j].Contains("ServerTools"))
                                {
                                    API.InstallPath = _childDirectories[j];
                                    if (File.Exists(_childDirectories[j] + "/IP2Location.txt"))
                                    {
                                        CountryBan.FileLocation = _childDirectories[j] + "/IP2Location.txt";
                                    }
                                    string[] _subChildDirectories = Directory.GetDirectories(_childDirectories[j]);
                                    if (_subChildDirectories.Length > 0)
                                    {
                                        for (int k = 0; k < _subChildDirectories.Length; k++)
                                        {
                                            if (_subChildDirectories[k].Contains("WebPanel"))
                                            {
                                                WebAPI.Directory = _subChildDirectories[k] + "/";
                                                return;
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

        public static void PlayerCheck()
        {
            try
            {
                if (!IsRunning)
                {
                    IsRunning = true;
                    List<ClientInfo> _cInfoList = ClientList();
                    if (_cInfoList != null && _cInfoList.Count > 0)
                    {
                        for (int i = 0; i < _cInfoList.Count; i++)
                        {
                            ClientInfo _cInfo = _cInfoList[i];
                            if (_cInfo != null && !string.IsNullOrEmpty(_cInfo.playerId) && _cInfo.entityId > 0)
                            {
                                EntityPlayer _player = GetEntityPlayer(_cInfo.playerId);
                                if (_player != null)
                                {
                                    if (!_player.IsDead())
                                    {
                                        if (_player.IsSpawned() && _player.IsAlive() && !Teleportation.Teleporting.Contains(_cInfo.entityId))
                                        {
                                            if (Zones.IsEnabled)
                                            {
                                                Zones.ZoneCheck(_cInfo, _player);
                                            }
                                            if (Lobby.IsEnabled && Lobby.LobbyPlayers.Contains(_cInfo.entityId))
                                            {
                                                Lobby.LobbyCheck(_cInfo, _player);
                                            }
                                            if (Market.IsEnabled && Market.MarketPlayers.Contains(_cInfo.entityId))
                                            {
                                                Market.MarketCheck(_cInfo, _player);
                                            }
                                            if (PrefabReset.IsEnabled) {
                                                PrefabReset.PlayerCheck(_cInfo,_player);
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

        public static bool IsBloodmoon()
        {
            try
            {
                World _world = GameManager.Instance.World;
                if (GameUtils.IsBloodMoonTime(_world.GetWorldTime(), (_world.DuskHour, _world.DawnHour), GameStats.GetInt(EnumGameStats.BloodMoonDay)))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.IsBLoodmoon: {0}", e.Message));
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
                PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromSteamId(_cInfo.playerId);
                if (_persistentPlayerData != null && _persistentPlayerData.LPBlocks != null && _persistentPlayerData.LPBlocks.Count > 0)
                {
                    if (GameManager.Instance.World.FindRandomSpawnPointNearPosition(_persistentPlayerData.LPBlocks[0].ToVector3(), 15, out int _x, out int _y, out int _z, new UnityEngine.Vector3(50f, 50f, 50f), true, false))
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
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromSteamId(_playerId);
            if (_persistentPlayerData != null)
            {
                List<Vector3i> landProtectionBlocks = _persistentPlayerData.LPBlocks;
                if (landProtectionBlocks != null)
                {
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
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromSteamId(_playerId);
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
                PersistentPlayerList _persistentPlayerList = GetPersistentPlayerList();
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
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromSteamId(_id);
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
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromSteamId(_id);
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
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromSteamId(_id);
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
            PersistentPlayerData _persistentPlayerData = GetPersistentPlayerDataFromSteamId(_id);
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
            List<EntityPlayer> _playerList = PlayerList();
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
            List<EntityPlayer> _playerList = PlayerList();
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
            List<ClientInfo> _clientList = ClientList();
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
                EntityPlayer _player = GetEntityPlayer(_cInfo.playerId);
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
                        List<ClientInfo> _clientList = ClientList();
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

        public static void ReturnBlock(ClientInfo _cInfo, string _blockName, int _quantity)
        {
            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
            if (_player != null && _player.IsSpawned() && !_player.IsDead())
            {
                World _world = GameManager.Instance.World;
                ItemValue _itemValue = ItemClass.GetItem(_blockName, false);
                if (_itemValue != null)
                {
                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                    {
                        entityClass = EntityClass.FromString("item"),
                        id = EntityFactory.nextEntityID++,
                        itemStack = new ItemStack(_itemValue, _quantity),
                        pos = _world.Players.dict[_cInfo.entityId].position,
                        rot = new Vector3(20f, 0f, 20f),
                        lifetime = 60f,
                        belongsPlayerId = _cInfo.entityId
                    });
                    _world.SpawnEntityInWorld(entityItem);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                    _world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                    Phrases.Dict.TryGetValue("GiveItem1", out string _phrase);
                    _phrase = _phrase.Replace("{Value}", _quantity.ToString());
                    _phrase = _phrase.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static string CreatePassword(int _length)
        {
            string _pass = "";
            try
            {
                System.Random _rnd = new System.Random();
                for (int i = 0; i < _length; i++)
                {
                    _pass += AlphaNumSet.ElementAt(_rnd.Next(0, 62));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.SetPassword: {0}", e.Message));
            }
            return _pass;
        }

        public static long ConvertIPToLong(string ipAddress)
        {
            try
            {
                if (IPAddress.TryParse(ipAddress, out IPAddress _ip))
                {
                    byte[] _bytes = _ip.MapToIPv4().GetAddressBytes();
                    return 16777216L * _bytes[0] +
                        65536 * _bytes[1] +
                        256 * _bytes[2] +
                        _bytes[3]
                        ;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CountryBan.ConvertIPToLong: {0}", e.Message));
            }
            return 0;
        }

        public static void Jail(ClientInfo _cInfoKiller)
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("st-Jail add {0} 120", _cInfoKiller.playerId), null);
            Phrases.Dict.TryGetValue("Jail1", out string _phrase);
            _phrase = _phrase.Replace("{PlayerName}", _cInfoKiller.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void Kill(ClientInfo _cInfo)
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), null);
            Phrases.Dict.TryGetValue("Zones4", out string _phrase);
            _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void Kick(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue("Zones6", out string _phrase);
            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase), null);
            Phrases.Dict.TryGetValue("Zones5", out _phrase);
            _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void Ban(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue("Zones8", out string _phrase);
            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, _phrase), null);
            Phrases.Dict.TryGetValue("Zones7", out _phrase);
            _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }
    }
}

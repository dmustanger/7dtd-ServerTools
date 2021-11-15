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
        public static bool ZoneRunning = false, Shutdown_Initiated = false, No_Vehicle_Pickup = false, ThirtySeconds = false, No_Currency = false;
        public static int Jail_Violation = 4, Kill_Violation = 6, Kick_Violation = 8, Ban_Violation = 10, Player_Killing_Mode = 0;
        public static string Currency_Item;
        public static FastTags Shotgun = FastTags.Parse("shotgun");
        public static FastTags Falling = FastTags.Parse("falling");

        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static Dictionary<int, int> EntityId = new Dictionary<int, int>();
        public static Dictionary<int, int> PvEViolations = new Dictionary<int, int>();

        public static List<ClientInfo> NewPlayerQue = new List<ClientInfo>();
        public static List<ClientInfo> BlockChatCommands = new List<ClientInfo>();

        public static readonly string AlphaNumSet = "jJkqQr9Kl3wXAbyYz0ZLmFpPRsMn5NoO6dDe1EfStaBc2CgGhH7iITu4U8vWxV";
        public static readonly char[] InvalidPrefix = new char[] { '!', '@', '#', '$', '%', '&', '/', '\\' };

        public static void SetInstallFolder()
        {
            string mainDir = Directory.GetCurrentDirectory();
            string[] directories = Directory.GetDirectories(mainDir);
            if (directories.Length > 0)
            {
                for (int i = 0; i < directories.Length; i++)
                {
                    if (directories[i].Contains("Mods"))
                    {
                        string[] childDirectories = Directory.GetDirectories(directories[i]);
                        if (childDirectories.Length > 0)
                        {
                            for (int j = 0; j < childDirectories.Length; j++)
                            {
                                if (childDirectories[j].Contains("ServerTools"))
                                {
                                    API.InstallPath = childDirectories[j];
                                    if (File.Exists(childDirectories[j] + "/IP2Location.txt"))
                                    {
                                        CountryBan.FileLocation = childDirectories[j] + "/IP2Location.txt";
                                    }
                                    if (File.Exists(childDirectories[j] + "/7za.exe"))
                                    {
                                        AutoBackup.FileLocation = childDirectories[j] + "/7za.exe";
                                    }
                                    string[] subChildDirectories = Directory.GetDirectories(childDirectories[j]);
                                    if (subChildDirectories.Length > 0)
                                    {
                                        for (int k = 0; k < subChildDirectories.Length; k++)
                                        {
                                            if (subChildDirectories[k].Contains("WebPanel"))
                                            {
                                                WebAPI.Directory = subChildDirectories[k] + "/";
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

        public static void CheckZone()
        {
            try
            {
                if (!ZoneRunning)
                {
                    ZoneRunning = true;
                    List<ClientInfo> clientList = ClientList();
                    if (clientList != null)
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo = clientList[i];
                            if (cInfo != null && !string.IsNullOrEmpty(cInfo.playerId) && cInfo.entityId > 0)
                            {
                                EntityPlayer player = GetEntityPlayer(cInfo.playerId);
                                if (player != null)
                                {
                                    if (!player.IsDead())
                                    {
                                        if (player.IsSpawned())
                                        {
                                            if (Zones.IsEnabled && Zones.ZoneList.Count > 0)
                                            {
                                                Zones.ZoneCheck(cInfo, player);
                                            }
                                            if (Lobby.IsEnabled && Lobby.LobbyPlayers.Contains(cInfo.entityId))
                                            {
                                                Lobby.InsideLobby(cInfo, player);
                                            }
                                            if (Market.IsEnabled && Market.MarketPlayers.Contains(cInfo.entityId))
                                            {
                                                Market.InsideMarket(cInfo, player);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.CheckZone: {0}", e.Message));
            }
            ZoneRunning = false;
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
                World world = GameManager.Instance.World;
                if (GameUtils.IsBloodMoonTime(world.GetWorldTime(), (world.DuskHour, world.DawnHour), GameStats.GetInt(EnumGameStats.BloodMoonDay)))
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
                float duskTime = SkyManager.GetDuskTime();
                float timeInMinutes = SkyManager.GetTimeOfDayAsMinutes();
                if (!SkyManager.BloodMoon() && timeInMinutes > duskTime && !GameManager.Instance.World.IsDark())
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
            if (ConnectionManager.Instance.Clients != null && ConnectionManager.Instance.Clients.Count > 0)
            {
                List<ClientInfo> clientList = ConnectionManager.Instance.Clients.List.ToList();
                return clientList;
            }
            return null;
        }

        public static ClientInfo GetClientInfoFromSteamId(string _playerId)
        {
            ClientInfo cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_playerId);
            if (cInfo != null)
            {
                return cInfo;
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
            Entity entity = GameManager.Instance.World.GetEntity(_id);
            if (entity != null)
            {
                return entity;
            }
            return null;
        }

        public static EntityZombie GetZombie(int _id)
        {
            Entity entity = GameManager.Instance.World.GetEntity(_id);
            if (entity != null && entity is EntityZombie)
            {
                return entity as EntityZombie;
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

        public static void ClearChunkProtection(ClientInfo _cInfo)
        {
            try
            {
                List<Chunk> chunkList = new List<Chunk>();
                EntityPlayer player = GetEntityPlayer(_cInfo.playerId);
                if (player != null)
                {
                    Vector3 position = player.position;
                    int x = (int)position.x, z = (int)position.z;
                    if (GameManager.Instance.World.IsChunkAreaLoaded(x, 1, z))
                    {
                        Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(x, 1, z);
                        if (!chunkList.Contains(chunk))
                        {
                            chunkList.Add(chunk);
                        }
                        Bounds bounds = chunk.GetAABB();
                        for (int i = (int)bounds.min.x; i < (int)bounds.max.x; i++)
                        {
                            for (int j = (int)bounds.min.z; j < (int)bounds.max.z; j++)
                            {
                                x = i - (int)bounds.min.x;
                                z = j - (int)bounds.min.z;
                                chunk.SetTraderArea(x, z, false);
                            }
                        }
                    }
                }
                if (chunkList.Count > 0)
                {
                    for (int k = 0; k < chunkList.Count; k++)
                    {
                        Chunk chunk = chunkList[k];
                        List<ClientInfo> clientList = ClientList();
                        if (clientList != null)
                        {
                            for (int l = 0; l < clientList.Count; l++)
                            {
                                ClientInfo cInfo2 = clientList[l];
                                if (cInfo2 != null)
                                {
                                    cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunk, true));
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
            EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
            if (player != null && player.IsSpawned() && !player.IsDead())
            {
                World world = GameManager.Instance.World;
                ItemValue itemValue = ItemClass.GetItem(_blockName, false);
                if (itemValue != null)
                {
                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                    {
                        entityClass = EntityClass.FromString("item"),
                        id = EntityFactory.nextEntityID++,
                        itemStack = new ItemStack(itemValue, _quantity),
                        pos = world.Players.dict[_cInfo.entityId].position,
                        rot = new Vector3(20f, 0f, 20f),
                        lifetime = 60f,
                        belongsPlayerId = _cInfo.entityId
                    });
                    world.SpawnEntityInWorld(entityItem);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                    Phrases.Dict.TryGetValue("GiveItem1", out string _phrase);
                    _phrase = _phrase.Replace("{Value}", _quantity.ToString());
                    _phrase = _phrase.Replace("{ItemName}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static Dictionary<int, EntityPlayer> GetEntityPlayers()
        {
            try
            {
                if (GameManager.Instance.World != null && GameManager.Instance.World.Players.dict != null && GameManager.Instance.World.Players.dict.Count > 0)
                {
                    return GameManager.Instance.World.Players.dict;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.GetEntityPlayers: {0}", e.Message));
            }
            return null;
        }

        public static bool IsValidItem(string itemName)
        {
            ItemValue itemValue = ItemClass.GetItem(itemName, false);
            if (itemValue.type != ItemValue.None.type)
            {
                return true;
            }
            return false;
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.SetPassword: {0}", e.Message));
            }
            return _pass;
        }

        public static long ConvertIPToLong(string ipAddress)
        {
            try
            {
                if (IPAddress.TryParse(ipAddress, out IPAddress _ip))
                {
                    byte[] bytes = _ip.MapToIPv4().GetAddressBytes();
                    return 16777216L * bytes[0] +
                        65536 * bytes[1] +
                        256 * bytes[2] +
                        bytes[3]
                        ;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.ConvertIPToLong: {0}", e.Message));
            }
            return 0;
        }

        public static void GetCurrencyName()
        {
            List<ItemClass> itemClassCurrency = ItemClass.GetItemsWithTag(FastTags.Parse("currency"));
            if (itemClassCurrency != null && itemClassCurrency.Count > 0)
            {
                Currency_Item = itemClassCurrency[0].Name;
                Log.Out(string.Format("[SERVERTOOLS] Wallet and Bank tool set to utilize item named {0}", Currency_Item));
            }
            else
            {
                No_Currency = true;
                Wallet.IsEnabled = false;
                Bank.IsEnabled = false;
                Config.WriteXml();
                Config.LoadXml();
                Log.Out(string.Format("[SERVERTOOLS] Unable to find an item with the tag 'currency' in the item list. Wallet and Bank tool are disabled"));
            }
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

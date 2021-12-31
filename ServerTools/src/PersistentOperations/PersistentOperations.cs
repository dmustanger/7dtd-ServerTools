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
        public static bool ZoneRunning = false, Shutdown_Initiated = false, No_Vehicle_Pickup = false, ThirtySeconds = false, No_Currency = false, Net_Package_Detector = false;
        public static int Jail_Violation = 4, Kill_Violation = 6, Kick_Violation = 8, Ban_Violation = 10, Player_Killing_Mode = 0;
        public static string Currency_Item;

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
                            if (cInfo != null && !Teleportation.Teleporting.Contains(cInfo.entityId))
                            {
                                EntityPlayer player = GetEntityPlayer(cInfo.entityId);
                                if (player != null && !player.IsDead() && player.IsSpawned())
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.CheckZone: {0}", e.Message));
            }
            ZoneRunning = false;
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            if (!Session.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                Session.Add(_cInfo.CrossplatformId.CombinedString, DateTime.Now);
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

        public static List<ClientInfo> ClientList()
        {
            if (ConnectionManager.Instance.Clients != null && ConnectionManager.Instance.Clients.Count > 0)
            {
                List<ClientInfo> clientList = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List.ToList();
                return clientList;
            }
            return null;
        }

        public static ClientInfo GetClientInfoFromNameOrId(string _id)
        {
            return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForNameOrId(_id);
        }

        public static ClientInfo GetClientInfoFromEntityId(int _entityId)
        {
            return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(_entityId);
        }

        public static ClientInfo GetClientInfoFromUId(PlatformUserIdentifierAbs _uId)
        {
            return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(_uId);
        }

        public static ClientInfo GetClientInfoFromName(string _name)
        {
            return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForPlayerName(_name);
        }

        public static List<EntityPlayer> PlayerList()
        {
            return GameManager.Instance.World.Players.list;
        }

        public static EntityPlayer GetEntityPlayer(int _id)
        {
            if (GameManager.Instance.World.Players.dict.ContainsKey(_id))
            {
                return GameManager.Instance.World.Players.dict[_id];
            }
            return null;
        }

        public static Entity GetEntity(int _id)
        {
            if (GameManager.Instance.World.Entities.dict.ContainsKey(_id))
            {
                return GameManager.Instance.World.Entities.dict[_id];
            }
            return null;
        }

        public static EntityZombie GetZombie(int _id)
        {
            if (GameManager.Instance.World.Entities.dict.ContainsKey(_id))
            {
                Entity entity = GameManager.Instance.World.Entities.dict[_id];
                if (entity != null && entity is EntityZombie)
                {
                    return entity as EntityZombie;
                }
            }
            return null;
        }

        public static PersistentPlayerList GetPersistentPlayerList()
        {
            return GameManager.Instance.persistentPlayers;
        }

        public static PersistentPlayerData GetPersistentPlayerDataFromId(string _id)
        {
            PlatformUserIdentifierAbs uId = GetPlatformUserFromNameOrId(_id);
            if (uId != null)
            {
                PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
                if (persistentPlayerList != null)
                {
                    PersistentPlayerData ppd = persistentPlayerList.GetPlayerData(uId);
                    if (ppd != null)
                    {
                        return ppd;
                    }
                }
            }
            return null;
        }

        public static PersistentPlayerData GetPersistentPlayerDataFromUId(PlatformUserIdentifierAbs _uId)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                PersistentPlayerData ppd = persistentPlayerList.GetPlayerData(_uId);
                if (ppd != null)
                {
                    return ppd;
                }
            }
            return null;
        }

        public static PersistentPlayerData GetPersistentPlayerDataFromEntityId(int _entityId)
        {
            PersistentPlayerList persistentPlayerList = GameManager.Instance.persistentPlayers;
            if (persistentPlayerList != null)
            {
                PersistentPlayerData persistentPlayerData = persistentPlayerList.GetPlayerDataFromEntityID(_entityId);
                if (persistentPlayerData != null)
                {
                    return persistentPlayerData;
                }
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromUId(PlatformUserIdentifierAbs _uId)
        {
            PlayerDataFile playerDatafile = new PlayerDataFile();
            playerDatafile.Load(GameIO.GetPlayerDataDir(), _uId.CombinedString.Trim());
            if (playerDatafile != null)
            {
                return playerDatafile;
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromEntityId(int _entityId)
        {
            PersistentPlayerData persistentPlayerData = GetPersistentPlayerDataFromEntityId(_entityId);
            if (persistentPlayerData != null)
            {
                PlayerDataFile playerDatafile = new PlayerDataFile();
                playerDatafile.Load(GameIO.GetPlayerDataDir(), persistentPlayerData.UserIdentifier.CombinedString.Trim());
                if (playerDatafile != null)
                {
                    return playerDatafile;
                }
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromId(string _id)
        {
            if (ConsoleHelper.ParseParamPartialNameOrId(_id, out PlatformUserIdentifierAbs platformUserIdentifierAbs, out ClientInfo clientInfo, true) == 1 && platformUserIdentifierAbs != null)
            {
                PlayerDataFile playerDatafile = GetPlayerDataFileFromUId(platformUserIdentifierAbs);
                if (playerDatafile != null)
                {
                    return playerDatafile;
                }
            }
            return null;
        }

        public static PlatformUserIdentifierAbs GetPlatformUserFromNameOrId(string _id)
        {
            if (ConsoleHelper.ParseParamPartialNameOrId(_id, out PlatformUserIdentifierAbs platformUserIdentifierAbs, out ClientInfo clientInfo, true) == 1 && platformUserIdentifierAbs != null)
            {
                return platformUserIdentifierAbs;
            }
            return null;
        }

        public static void RemoveAllClaims(string _id)
        {
            PersistentPlayerData persistentPlayerData = GetPersistentPlayerDataFromId(_id);
            if (persistentPlayerData != null)
            {
                List<Vector3i> landProtectionBlocks = persistentPlayerData.LPBlocks;
                if (landProtectionBlocks != null)
                {
                    for (int i = 0; i < landProtectionBlocks.Count; i++)
                    {
                        Vector3i position = landProtectionBlocks[i];
                        World world = GameManager.Instance.World;
                        BlockValue blockValue = world.GetBlock(position);
                        Block block = blockValue.Block;
                        if (block != null && block is BlockLandClaim)
                        {
                            world.SetBlockRPC(0, position, BlockValue.Air);
                            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, position.ToVector3()), false, -1, -1, -1, -1);
                            world.ObjectOnMapRemove(EnumMapObjectType.LandClaim, position.ToVector3());
                            LandClaimBoundsHelper.RemoveBoundsHelper(position.ToVector3());
                        }
                        GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(position);
                        persistentPlayerData.LPBlocks.Remove(position);
                    }
                    SavePersistentPlayerDataXML();
                }
            }
        }

        public static void RemoveOneClaim(string _playerId, Vector3i _position)
        {
            PersistentPlayerData persistentPlayerData = GetPersistentPlayerDataFromId(_playerId);
            if (persistentPlayerData != null)
            {
                World world = GameManager.Instance.World;
                BlockValue blockValue = world.GetBlock(_position);
                Block block = blockValue.Block;
                if (block != null && block is BlockLandClaim)
                {
                    world.SetBlockRPC(0, _position, BlockValue.Air);
                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, _position.ToVector3()), false, -1, -1, -1, -1);
                    world.ObjectOnMapRemove(EnumMapObjectType.LandClaim, _position.ToVector3());
                    LandClaimBoundsHelper.RemoveBoundsHelper(_position.ToVector3());
                }
                GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(_position);
                persistentPlayerData.LPBlocks.Remove(_position);
                SavePersistentPlayerDataXML();
            }
        }

        public static void RemovePersistentPlayerData(string _id)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                PlatformUserIdentifierAbs uId = GetPlatformUserFromNameOrId(_id);
                if (uId != null)
                {
                    if (persistentPlayerList.Players.ContainsKey(uId))
                    {
                        persistentPlayerList.Players.Remove(uId);
                        SavePersistentPlayerDataXML();
                    }
                }
            }
        }

        public static void RemoveAllACL(string _playerId)
        {
            PersistentPlayerData persistentPlayerData = GetPersistentPlayerDataFromId(_playerId);
            if (persistentPlayerData != null)
            {
                PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
                foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> persistentPlayerData2 in persistentPlayerList.Players)
                {
                    if (persistentPlayerData2.Key != persistentPlayerData.UserIdentifier)
                    {
                        if (persistentPlayerData2.Value.ACL != null && persistentPlayerData2.Value.ACL.Contains(persistentPlayerData.UserIdentifier))
                        {
                            persistentPlayerData2.Value.RemovePlayerFromACL(persistentPlayerData.UserIdentifier);
                            persistentPlayerData2.Value.Dispatch(persistentPlayerData, EnumPersistentPlayerDataReason.ACL_Removed);
                        }
                        if (persistentPlayerData.ACL != null && persistentPlayerData.ACL.Contains(persistentPlayerData2.Value.UserIdentifier))
                        {
                            persistentPlayerData.RemovePlayerFromACL(persistentPlayerData2.Key);
                            persistentPlayerData.Dispatch(persistentPlayerData2.Value, EnumPersistentPlayerDataReason.ACL_Removed);
                        }
                    }
                }
                SavePersistentPlayerDataXML();
            }
        }

        public static void SavePlayerDataFile(string _id, PlayerDataFile _playerDataFile)
        {
            _playerDataFile.Save(GameIO.GetPlayerDataDir(), _id.Trim());
            ClientInfo cInfo = GetClientInfoFromNameOrId(_id);
            if (cInfo != null)
            {
                ModEvents.SavePlayerData.Invoke(cInfo, _playerDataFile);
            }
        }

        public static void SavePersistentPlayerDataXML()
        {
            if (GameManager.Instance.persistentPlayers != null)
            {
                GameManager.Instance.persistentPlayers.Write(GameIO.GetSaveGameDir(null, null) + "/players.xml");
            }
        }

        public static bool ClaimedBySelf(PlatformUserIdentifierAbs _uId, Vector3i _position)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                PersistentPlayerData persistentPlayerData = persistentPlayerList.GetPlayerData(_uId);
                if (persistentPlayerData != null)
                {
                    EnumLandClaimOwner owner = GameManager.Instance.World.GetLandClaimOwner(_position, persistentPlayerData);
                    if (owner == EnumLandClaimOwner.Self)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool ClaimedByAlly(PlatformUserIdentifierAbs _uId, Vector3i _position)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                PersistentPlayerData persistentPlayerData = persistentPlayerList.GetPlayerData(_uId);
                if (persistentPlayerData != null)
                {
                    EnumLandClaimOwner owner = GameManager.Instance.World.GetLandClaimOwner(_position, persistentPlayerData);
                    if (owner == EnumLandClaimOwner.Ally)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool ClaimedByNone(PlatformUserIdentifierAbs _uId, Vector3i _position)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                PersistentPlayerData persistentPlayerData = persistentPlayerList.GetPlayerData(_uId);
                if (persistentPlayerData != null)
                {
                    EnumLandClaimOwner owner = GameManager.Instance.World.GetLandClaimOwner(_position, persistentPlayerData);
                    if (owner == EnumLandClaimOwner.None)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool ClaimedByAllyOrSelf(PlatformUserIdentifierAbs _uId, Vector3i _position)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                PersistentPlayerData persistentPlayerData = persistentPlayerList.GetPlayerData(_uId);
                if (persistentPlayerData != null)
                {
                    EnumLandClaimOwner owner = GameManager.Instance.World.GetLandClaimOwner(_position, persistentPlayerData);
                    if (owner == EnumLandClaimOwner.Ally || owner == EnumLandClaimOwner.Self)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void ClearChunkProtection(ClientInfo _cInfo)
        {
            try
            {
                List<Chunk> chunkList = new List<Chunk>();
                EntityPlayer player = GetEntityPlayer(_cInfo.entityId);
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
                int count = 1;
                foreach (KeyValuePair<int, EntityClass> entityClass in EntityClass.list.Dict)
                {
                    if (entityClass.Value.bAllowUserInstantiate)
                    {
                        EntityId.Add(count, entityClass.Key);
                        count++;
                    }
                }
            }
        }

        public static void ReturnBlock(ClientInfo _cInfo, string _blockName, int _quantity)
        {
            EntityPlayer player = GetEntityPlayer(_cInfo.entityId);
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
            return GameManager.Instance.World.Players.dict;
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
            string pass = String.Empty;
            try
            {
                System.Random rnd = new System.Random();
                for (int i = 0; i < _length; i++)
                {
                    pass += AlphaNumSet.ElementAt(rnd.Next(0, 62));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PersistentOperations.SetPassword: {0}", e.Message));
            }
            return pass;
        }

        public static long ConvertIPToLong(string ipAddress)
        {
            try
            {
                if (IPAddress.TryParse(ipAddress, out IPAddress ip))
                {
                    byte[] bytes = ip.MapToIPv4().GetAddressBytes();
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
                Log.Out(string.Format("[SERVERTOOLS] Wallet and Bank tool set to utilize item named '{0}'", Currency_Item));
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
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("st-Jail add {0} 120", _cInfoKiller.CrossplatformId.CombinedString), null);
            Phrases.Dict.TryGetValue("Jail1", out string phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfoKiller.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void Kill(ClientInfo _cInfo)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.CrossplatformId.CombinedString), null);
            Phrases.Dict.TryGetValue("Zones4", out string phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void Kick(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue("Zones6", out string phrase);
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
            Phrases.Dict.TryGetValue("Zones5", out phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void Ban(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue("Zones8", out string phrase);
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
            Phrases.Dict.TryGetValue("Zones7", out phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }
    }
}

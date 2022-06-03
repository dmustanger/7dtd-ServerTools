using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    public class PersistentOperations
    {
        public static bool ZoneRunning = false, Shutdown_Initiated = false, No_Vehicle_Pickup = false, ThirtySeconds = false, No_Currency = false, Net_Package_Detector = false;
        public static int Jail_Violation = 4, Kill_Violation = 6, Kick_Violation = 8, Ban_Violation = 10, Player_Killing_Mode = 0, MeleeHandPlayer = 0;
        public static string Currency_Item, XPathDir, Command_expire = "expire", Command_commands = "commands";

        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static Dictionary<int, int> EntityId = new Dictionary<int, int>();
        public static Dictionary<int, int> PvEViolations = new Dictionary<int, int>();

        public static List<ClientInfo> NewPlayerQue = new List<ClientInfo>();
        public static List<ClientInfo> BlockChatCommands = new List<ClientInfo>();

        public static readonly string AlphaNumSet = "jJkqQr9Kl3wXAbyYz0ZLmFpPRsMn5NoO6dDe1EfStaBc2CgGhH7iITu4U8vWxV";
        public static readonly char[] InvalidPrefix = new char[] { '!', '@', '#', '$', '%', '&', '/', '\\' };

        public static DateTime StartTime = DateTime.Now;

        public static void SetFolders()
        {
            if (Directory.Exists(API.GamePath + "/Mods/ServerTools"))
            {
                if (Directory.Exists(API.GamePath + "/Mods/ServerTools/WebAPI"))
                {
                    WebAPI.Directory = API.GamePath + "/Mods/ServerTools/WebAPI/";
                }
                if (Directory.Exists(API.GamePath + "/Mods/ServerTools/Config"))
                {
                    XPathDir = API.GamePath + "/Mods/ServerTools/Config/";
                }
            }
        }

        public static void CreateCustomXUi()
        {
            if (XPathDir != "")
            {
                if (!File.Exists(XPathDir + "gameevents.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "gameevents.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/gameevents\">");
                        sw.WriteLine();
                        sw.WriteLine("  <action_sequence name=\"action_admin\">");
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"RemoveItems\">");
                        sw.WriteLine("          <property name=\"items_location\" value=\"Backpack\" />");
                        sw.WriteLine("          <property name=\"remove_item_tag\" value=\"admin\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"PlaySound\">");
                        sw.WriteLine("          <property name=\"sound\" value=\"ui_trader_purchase\" />");
                        sw.WriteLine("          <property name=\"inside_head\" value=\"true\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("  </action_sequence>");
                        sw.WriteLine();
                        sw.WriteLine("  <action_sequence name=\"action_dukes\">");
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"RemoveItems\">");
                        sw.WriteLine("          <property name=\"items_location\" value=\"Backpack\" />");
                        sw.WriteLine("          <property name=\"remove_item_tag\" value=\"dukes\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"PlaySound\">");
                        sw.WriteLine("          <property name=\"sound\" value=\"ui_trader_purchase\" />");
                        sw.WriteLine("          <property name=\"inside_head\" value=\"true\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("  </action_sequence>");
                        sw.WriteLine();
                        sw.WriteLine("  <action_sequence name=\"action_currency\">");
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"RemoveItems\">");
                        sw.WriteLine("          <property name=\"items_location\" value=\"Backpack\" />");
                        sw.WriteLine("          <property name=\"remove_item_tag\" value=\"currency\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"PlaySound\">");
                        sw.WriteLine("          <property name=\"sound\" value=\"ui_trader_purchase\" />");
                        sw.WriteLine("          <property name=\"inside_head\" value=\"true\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("  </action_sequence>");
                        sw.WriteLine();
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                }
                if (!File.Exists(XPathDir + "items.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "items.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<set xpath=\"/items/item[@name='casinoCoin']/property[@name='Tags']/@value\">dukes,currency</set>");
                        sw.WriteLine("<!-- ..... Wallet and Bank currency ^ ..... -->");
                        sw.WriteLine("<!-- Replace with custom item name from items.xml if desired -->");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                }
                if (!File.Exists(XPathDir + "XUi/windows.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "XUi/windows.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/windows\">");
                        sw.WriteLine();
                        sw.WriteLine("  <window name=\"browserMap\" controller=\"ServerInfo\">");
                        sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" bordercolor=\"[white]\" borderthickness=\"5\" >");
                        sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"257\" text=\"World Map\" />");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                        sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"260\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                        sw.WriteLine("          <label name=\"ServerDescription\" />");
                        sw.WriteLine("          <label depth=\"4\" pos=\"0,-40\" height=\"30\" width=\"257\" name=\"ServerWebsiteURL\" text=\"http://0.0.0.0:8082\" justify=\"center\" style=\"press,hover\" font_size=\"30\" upper_case=\"false\" />");
                        sw.WriteLine("          <!-- Change the text IP and Port to the one needed by Allocs web map. Check it functions through a web browser first -->");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("  </window>");
                        sw.WriteLine();
                        sw.WriteLine("  <window name=\"browserDiscord\" controller=\"ServerInfo\">");
                        sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" bordercolor=\"[white]\" borderthickness=\"5\" >");
                        sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"257\" text=\"Discord Invite\" />");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                        sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"260\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                        sw.WriteLine("          <label name=\"ServerDescription\" />");
                        sw.WriteLine("          <label depth=\"4\" pos=\"0,-40\" height=\"30\" width=\"257\" name=\"ServerWebsiteURL\" text=\"http://discord.gg/linkHere\" justify=\"center\" style=\"press,hover\" font_size=\"30\" upper_case=\"false\" />");
                        sw.WriteLine("          <!-- Change the text to a Discord invite link of your choice. Check it functions through a web browser first -->");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("  </window>");
                        sw.WriteLine();
                        sw.WriteLine("  <window name=\"browserVote\" controller=\"ServerInfo\">");
                        sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" bordercolor=\"[white]\" borderthickness=\"5\" >");
                        sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"257\" text=\"Voting Site\" />");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                        sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"260\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                        sw.WriteLine("          <label name=\"ServerDescription\" />");
                        sw.WriteLine("          <label depth=\"4\" pos=\"0,-40\" height=\"30\" width=\"257\" name=\"ServerWebsiteURL\" text=\"https://7daystodie-servers.com/server/12345\" justify=\"center\" style=\"press,hover\" font_size=\"30\" upper_case=\"false\" />");
                        sw.WriteLine("          <!-- Change the text to a voting link of your choice. Check it functions through a web browser first -->");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("  </window>");
                        sw.WriteLine();
                        sw.WriteLine("  <window name=\"browserBlackJack\" controller=\"ServerInfo\">");
                        sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" bordercolor=\"[white]\" borderthickness=\"5\" >");
                        sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"257\" text=\"Black Jack\" />");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                        sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"260\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                        sw.WriteLine("          <label name=\"ServerDescription\" />");
                        sw.WriteLine("          <label depth=\"4\" pos=\"0,-40\" height=\"30\" width=\"257\" name=\"ServerWebsiteURL\" text=\"http://0.0.0.0:8084/blackJack.html\" justify=\"center\" style=\"press,hover\" font_size=\"30\" upper_case=\"false\" />");
                        sw.WriteLine("          <!-- Change the text IP and Port to the one needed by ServerTools web api -->");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("  </window>");
                        sw.WriteLine();
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                    }
                }
                if (!File.Exists(XPathDir + "XUi/xui.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "XUi/xui.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/xui/ruleset\">");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserMap\">");
                        sw.WriteLine("      <window name=\"browserMap\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserDiscord\">");
                        sw.WriteLine("      <window name=\"browserDiscord\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserVote\">");
                        sw.WriteLine("      <window name=\"browserVote\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserBlackJack\">");
                        sw.WriteLine("      <window name=\"browserBlackJack\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
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
                                if (player != null && !player.IsDead() && player.IsSpawned() && player.position != null)
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
                return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List.ToList();
            }
            return null;
        }

        public static ClientInfo GetClientInfoFromNameOrId(string _id)
        {
            ClientInfo cInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForNameOrId(_id);
            if (cInfo != null)
            {
                return cInfo;
            }
            else if (int.TryParse(_id, out int entityId))
            {
                cInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityId);
                if (cInfo != null)
                {
                    return cInfo;
                }
            }
            return null;
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

        public static List<EntityPlayer> ListPlayers()
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

        public static EntityAnimal GetAnimal(int _id)
        {
            if (GameManager.Instance.World.Entities.dict.ContainsKey(_id))
            {
                Entity entity = GameManager.Instance.World.Entities.dict[_id];
                if (entity != null && entity is EntityAnimal)
                {
                    return entity as EntityAnimal;
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

        public static bool ClaimedByNone(Vector3i _position)
        {
            Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
                if (persistentPlayerList != null)
                {
                    int claimSize = GameStats.GetInt(EnumGameStats.LandClaimSize);
                    Dictionary<Vector3i, PersistentPlayerData> claims = persistentPlayerList.m_lpBlockMap;
                    foreach (var claim in claims)
                    {
                        float distance = (claim.Key.ToVector3() - _position.ToVector3()).magnitude;
                        if (distance <= claimSize / 2 && GameManager.Instance.World.IsLandProtectionValidForPlayer(claim.Value))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static EnumLandClaimOwner ClaimedByWho(PlatformUserIdentifierAbs _uId, Vector3i _position)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                PersistentPlayerData persistentPlayerData = persistentPlayerList.GetPlayerData(_uId);
                if (persistentPlayerData != null)
                {
                    return GameManager.Instance.World.GetLandClaimOwner(_position, persistentPlayerData);
                }
            }
            return EnumLandClaimOwner.None;
        }

        public static bool ClaimedBySelfOrAlly(ClientInfo _cInfo, Vector3i _position)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                int claimSize = GameStats.GetInt(EnumGameStats.LandClaimSize);
                Dictionary<Vector3i, PersistentPlayerData> claims = persistentPlayerList.m_lpBlockMap;
                foreach (var claim in claims)
                {
                    float distance = (claim.Key.ToVector3() - _position.ToVector3()).magnitude;
                    if (distance <= claimSize / 2 && GameManager.Instance.World.IsLandProtectionValidForPlayer(claim.Value))
                    {
                        if (claim.Value.EntityId == _cInfo.entityId)
                        {
                            return true;
                        }
                        else if (claim.Value.ACL.Contains(_cInfo.PlatformId) || claim.Value.ACL.Contains(_cInfo.CrossplatformId))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
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

        public static void ReturnBlock(ClientInfo _cInfo, string _blockName, int _quantity, string _phrase)
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
                    Phrases.Dict.TryGetValue(_phrase, out string phrase);
                    phrase = phrase.Replace("{Value}", _quantity.ToString());
                    phrase = phrase.Replace("{ItemName}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name);
                    phrase = phrase.Replace("{BlockName}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                Log.Out(string.Format("[SERVERTOOLS] Unable to find an item with the tag 'currency' in the item list. Wallet and Bank tool are disabled until server restart"));
            }
        }

        public static void GetMeleeHandPlayer()
        {
            ItemValue meleeHand = ItemClass.GetItem("meleeHandPlayer");
            if (meleeHand != null)
            {
                MeleeHandPlayer = meleeHand.GetItemId();
            }
        }

        public static void JailPlayer(ClientInfo _cInfoKiller)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("st-Jail add {0} 120", _cInfoKiller.CrossplatformId.CombinedString), null);
            Phrases.Dict.TryGetValue("Jail1", out string phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfoKiller.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void KillPlayer(ClientInfo _cInfo)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.CrossplatformId.CombinedString), null);
            Phrases.Dict.TryGetValue("Zones4", out string phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void KickPlayer(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue("Zones6", out string phrase);
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
            Phrases.Dict.TryGetValue("Zones5", out phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void BanPlayer(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue("Zones8", out string phrase);
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
            Phrases.Dict.TryGetValue("Zones7", out phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void CommandList(ClientInfo _cInfo)
        {
            try
            {
                string commands = "";
                if (AdminList.IsEnabled)
                {
                    if (AdminList.Command_adminlist != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AdminList.Command_adminlist);
                    }
                }
                if (AllocsMap.IsEnabled)
                {
                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AllocsMap.Command_map);
                }
                if (AnimalTracking.IsEnabled)
                {
                    if (AnimalTracking.Command_trackanimal != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AnimalTracking.Command_trackanimal);
                    }
                    if (AnimalTracking.Command_track != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AnimalTracking.Command_track);
                    }
                }
                if (Auction.IsEnabled)
                {
                    if (Auction.Command_auction != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction);
                    }
                    if (Auction.Command_auction_cancel != "")
                    {
                        commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_cancel);
                    }
                    if (Auction.Command_auction_buy != "")
                    {
                        commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_buy);
                    }
                    if (Auction.Command_auction_sell != "")
                    {
                        commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_sell);
                    }
                }
                if (AutoPartyInvite.IsEnabled)
                {
                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AutoPartyInvite.Command_party_add);
                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AutoPartyInvite.Command_party_remove);
                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AutoPartyInvite.Command_party);
                }
                if (Bank.IsEnabled)
                {
                    if (Bank.Command_bank != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_bank);
                    }
                    if (Wallet.IsEnabled && Bank.Command_deposit != "")
                    {
                        commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_deposit);
                    }
                    if (Wallet.IsEnabled && Bank.Command_withdraw != "")
                    {
                        commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_withdraw);
                    }
                    if (Bank.Player_Transfers)
                    {
                        if (Bank.Command_transfer != "")
                        {
                            commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_transfer);
                        }
                    }
                }
                if (Bed.IsEnabled)
                {
                    if (Bed.Command_bed != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bed.Command_bed);
                    }
                }
                if (Pickup.IsEnabled)
                {
                    if (Pickup.Command_pickup != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Pickup.Command_pickup);
                    }
                }
                if (Bloodmoon.IsEnabled)
                {
                    if (Bloodmoon.Command_bloodmoon != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bloodmoon.Command_bloodmoon);
                    }
                }
                if (Bounties.IsEnabled)
                {
                    if (Bounties.Command_bounty != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bounties.Command_bounty);
                    }
                    if (Bounties.Command_bounty != "")
                    {
                        commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bounties.Command_bounty);
                    }
                }
                if (ChatColor.IsEnabled && ChatColor.Players.ContainsKey(_cInfo.PlatformId.ReadablePlatformUserIdentifier))
                {
                    if (ChatColor.Command_ccc != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccc);
                    }
                    if (ChatColor.Rotate)
                    {
                        if (ChatColor.Command_ccpr != "")
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccpr);
                        }
                        if (ChatColor.Command_ccnr != "")
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccnr);
                        }
                    }
                    if (ChatColor.Custom_Color)
                    {
                        if (ChatColor.Command_ccpr != "")
                        {
                            commands = string.Format("{0} {1}{2} [******]", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccpr);
                        }
                        if (ChatColor.Command_ccnr != "")
                        {
                            commands = string.Format("{0} {1}{2} [******]", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccnr);
                        }
                    }
                }
                if (ClanManager.IsEnabled)
                {
                    if (ClanManager.Command_chat != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_chat);
                    }
                }
                if (ClanManager.IsEnabled)
                {
                    if (ClanManager.Command_clan_list != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_clan_list);
                    }
                }
                if (ClanManager.IsEnabled && !ClanManager.ClanMember.Contains(_cInfo.PlatformId.ReadablePlatformUserIdentifier))
                {
                    if (ClanManager.Command_request != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_request);
                    }
                }
                if (Day7.IsEnabled)
                {
                    if (Day7.Command_day7 != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Day7.Command_day7);
                    }
                }
                if (Died.IsEnabled)
                {
                    if (Died.Command_died != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Died.Command_died);
                    }
                }
                if (DiscordLink.IsEnabled)
                {
                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, DiscordLink.Command_discord);
                }
                if (ExitCommand.IsEnabled)
                {
                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ExitCommand.Command_exit);
                }
                if (FirstClaimBlock.IsEnabled)
                {
                    if (FirstClaimBlock.Command_claim != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, FirstClaimBlock.Command_claim);
                    }
                }
                if (Fps.IsEnabled)
                {
                    if (Fps.Command_fps != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Fps.Command_fps);
                    }
                }
                if (FriendTeleport.IsEnabled)
                {
                    if (FriendTeleport.Command_friend != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_friend);
                        commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_friend);
                    }
                    if (FriendTeleport.Command_accept != "" && FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_accept);
                    }
                }
                if (Gimme.IsEnabled)
                {
                    if (Gimme.Command_gimme != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Gimme.Command_gimme);
                    }
                }
                if (Hardcore.IsEnabled)
                {
                    if (!Hardcore.Optional)
                    {
                        if (Hardcore.Command_top3 != "")
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_top3);
                        }
                        if (Hardcore.Command_score != "")
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_score);
                        }
                        if (Hardcore.Command_hardcore != "")
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore);
                        }
                        if (Hardcore.Max_Extra_Lives > 0)
                        {
                            if (Hardcore.Command_buy_life != "")
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_buy_life);
                            }
                        }
                    }
                    else
                    {
                        if (Hardcore.Command_top3 != "")
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_top3);
                        }
                        if (Hardcore.Command_score != "")
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_score);
                        }
                        if (PersistentContainer.Instance.Players[_cInfo.PlatformId.ReadablePlatformUserIdentifier].HardcoreEnabled)
                        {
                            if (Hardcore.Command_hardcore != "")
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore);
                            }
                            if (Hardcore.Max_Extra_Lives > 0)
                            {
                                if (Hardcore.Command_buy_life != "")
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_buy_life);
                                }
                            }
                        }
                        else
                        {
                            if (Hardcore.Command_hardcore_on != "")
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore_on);
                            }
                        }
                    }
                }
                if (Homes.IsEnabled)
                {
                    if (Homes.Command_home != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_home);
                    }
                    if (Homes.Command_fhome != "")
                    {
                        commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_fhome);
                    }
                    if (Homes.Command_save != "")
                    {
                        commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_save);
                    }
                    if (Homes.Command_delete != "")
                    {
                        commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_delete);
                    }
                    if (Homes.Command_go != "" && Homes.Invite.ContainsKey(_cInfo.entityId))
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_go);
                    }
                    if (Homes.Command_set != "")
                    {
                        commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_set);
                    }
                }
                if (InfoTicker.IsEnabled)
                {
                    if (InfoTicker.Command_infoticker != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, InfoTicker.Command_infoticker);
                    }
                }
                if (KickVote.IsEnabled)
                {
                    if (KickVote.Command_kickvote != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, KickVote.Command_kickvote);
                    }
                }
                if (Lobby.IsEnabled)
                {
                    if (Lobby.Command_lobby != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lobby.Command_lobby);
                    }
                    if (Lobby.Return && Lobby.LobbyPlayers.Contains(_cInfo.entityId))
                    {
                        if (Lobby.Command_lobbyback != "")
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lobby.Command_lobbyback);
                        }
                    }
                }
                if (Loc.IsEnabled)
                {
                    if (Loc.Command_loc != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Loc.Command_loc);
                    }
                }
                if (Lottery.IsEnabled)
                {
                    if (Lottery.Command_lottery != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery);
                    }
                    if (Lottery.Command_lottery != "")
                    {
                        commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery);
                    }
                    if (Lottery.Command_lottery_enter != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery_enter);
                    }
                }
                if (Market.IsEnabled)
                {
                    if (Market.Command_market != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Market.Command_market);
                    }
                    if (Market.Return && Market.MarketPlayers.Contains(_cInfo.entityId))
                    {
                        if (Market.Command_marketback != "")
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Market.Command_marketback);
                        }
                    }
                }
                if (Mute.IsEnabled)
                {
                    if (Mute.Command_mute != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Mute.Command_mute);
                    }
                    if (Mute.Command_unmute != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Mute.Command_unmute);
                    }
                    if (Mute.Command_mutelist != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Mute.Command_mutelist);
                    }
                }
                if (MuteVote.IsEnabled && Mute.IsEnabled)
                {
                    if (MuteVote.Command_mutevote != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, MuteVote.Command_mutevote);
                    }
                }
                if (PlayerList.IsEnabled)
                {
                    if (PlayerList.Command_playerlist != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, PlayerList.Command_playerlist);
                    }
                }
                if (Prayer.IsEnabled)
                {
                    if (Prayer.Command_pray != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Prayer.Command_pray);
                    }
                }
                if (Report.IsEnabled)
                {
                    if (Report.Command_report != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Report.Command_report);
                    }
                }
                if (RestartVote.IsEnabled)
                {
                    if (RestartVote.Command_restartvote != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, RestartVote.Command_restartvote);
                    }
                }
                if (Shop.IsEnabled)
                {
                    if (Shop.Command_shop != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Shop.Command_shop);
                    }
                    if (Shop.Command_shop_buy != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Shop.Command_shop_buy);
                    }
                }
                if (Shutdown.IsEnabled)
                {
                    if (Shutdown.Command_shutdown != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Shutdown.Command_shutdown);
                    }
                }
                if (Stuck.IsEnabled)
                {
                    if (Stuck.Command_stuck != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Stuck.Command_stuck);
                    }
                }
                if (Suicide.IsEnabled)
                {
                    if (Suicide.Command_killme != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Suicide.Command_killme);
                    }
                    if (Suicide.Command_suicide != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Suicide.Command_suicide);
                    }
                }
                if (Travel.IsEnabled)
                {
                    if (Travel.Command_travel != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Travel.Command_travel);
                    }
                }
                if (VehicleRecall.IsEnabled)
                {
                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, VehicleRecall.Command_vehicle_save);
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles.Count > 0)
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, VehicleRecall.Command_vehicle);
                        commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, VehicleRecall.Command_vehicle_remove);
                    }
                }
                if (VoteReward.IsEnabled)
                {
                    if (VoteReward.Command_reward != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, VoteReward.Command_reward);
                    }
                    if (VoteReward.Command_vote != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, VoteReward.Command_vote);
                    }
                }
                if (Wall.IsEnabled)
                {
                    if (Wall.Command_wall != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Wall.Command_wall);
                    }
                }
                if (Waypoints.IsEnabled)
                {
                    if (Waypoints.Command_waypoint != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint);
                    }
                    if (Waypoints.Command_waypoint != "")
                    {
                        commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint);
                    }
                    if (Waypoints.Command_waypoint_save != "")
                    {
                        commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint_save);
                    }
                    if (Waypoints.Command_waypoint_del != "")
                    {
                        commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint_del);
                    }
                    if (Waypoints.Command_fwaypoint != "")
                    {
                        commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_fwaypoint);
                    }
                    if (Waypoints.Command_go_way != "" && Waypoints.Invite.ContainsKey(_cInfo.entityId))
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_go_way);
                    }
                }
                if (Whisper.IsEnabled)
                {
                    if (Whisper.Command_rmessage != "")
                    {
                        commands = string.Format("{0} {1}{2} {3}{4}", commands, ChatHook.Chat_Command_Prefix1, Whisper.Command_pmessage, ChatHook.Chat_Command_Prefix1, Whisper.Command_rmessage);
                    }
                }
                
                if (ReservedSlots.IsEnabled && (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                {
                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Command_expire);
                }
                else if (ChatColor.IsEnabled && (ChatColor.Players.ContainsKey(_cInfo.PlatformId.CombinedString) || ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                {
                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Command_expire);
                }
                else if (LoginNotice.IsEnabled && (LoginNotice.Dict1.ContainsKey(_cInfo.PlatformId.CombinedString) || LoginNotice.Dict1.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                {
                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Command_expire);
                }

                if (commands.Length > 100)
                {
                    for (int i = 0; i < 10; i += 100)
                    {
                        commands = commands.Insert(i, "╚");
                        if (commands.Substring(i).Length < 100)
                        {
                            break;
                        }
                    }
                    string[] commandSplit = commands.Split('╚');
                    for (int i = 0; i < commandSplit.Length; i ++)
                    {
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commandSplit[i], -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else if (commands.Length > 0)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.CustomCommandList: {0}", e.Message));
            }
        }

        public static void AdminCommandList(ClientInfo _cInfo)
        {
            try
            {
                string commands = "";
                if (AdminChat.IsEnabled)
                {
                    commands = string.Format("{0} @" + AdminChat.Command_admin, commands);
                }
                if (Jail.IsEnabled)
                {
                    if (Jail.Command_set != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Jail.Command_set);
                    }
                }
                if (NewSpawnTele.IsEnabled)
                {
                    if (NewSpawnTele.Command_setspawn != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, NewSpawnTele.Command_setspawn);
                    }
                }
                if (commands.Length > 0)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.AdminCommandList: {0}", e.Message));
            }
        }
    }
}

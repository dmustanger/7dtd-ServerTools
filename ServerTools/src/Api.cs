using System;
using System.IO;

namespace ServerTools
{
    public class API : ModApiAbstract
    {
        public static string GamePath = GamePrefs.GetString(EnumGamePrefs.SaveGameFolder);
        public static string ConfigPath = string.Format("{0}/ServerTools", GamePath);
        public static int MaxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);

        public override void GameAwake()
        {
            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
            StateManager.Awake();
            Config.Load();
        }

        public override void SavePlayerData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            if (HighPingKicker.IsEnabled)
            {
                HighPingKicker.CheckPing(_cInfo);
            }
            if (InventoryCheck.IsEnabled || InventoryCheck.Anounce_Invalid_Stack)
            {
                InventoryCheck.CheckInv(_cInfo, _playerDataFile);
            }
            if (Watchlist.IsEnabled)
            {
                Watchlist.CheckWatchlist(_cInfo);
            }
            if (FamilyShareAccount.IsEnabled)
            {
                FamilyShareAccount.AccCheck(_cInfo);
            }
        }

        public override void PlayerLogin(ClientInfo _cInfo, string _compatibilityVersion)
        {
            if (ReservedSlots.IsEnabled)
            {
                ReservedSlots.CheckReservedSlot(_cInfo);
            }          
        }

        public override void PlayerSpawning(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)
        {
            if (FamilyShareAccount.IsEnabled)
            {
                FamilyShareAccount.AccCheck(_cInfo);
            }
            if (ClanManager.IsEnabled)
            {
                ClanManager.CheckforClantag(_cInfo);
            }
            if (Motd.IsEnabled & !Motd.Show_On_Respawn)
            {
                Motd.Send(_cInfo);
            }
            if (Bloodmoon.Show_On_Login & !Bloodmoon.Show_On_Respawn)
            {
                Bloodmoon.GetBloodmoon(_cInfo, false);
            }
            if (Shop.IsEnabled)
            {
                Wallet.AddWorldSeed(_cInfo);
            }
        }

        public override void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {
            if (NewSpawnTele.IsEnabled)
            {
                NewSpawnTele.TeleNewSpawn(_cInfo);
            }
            if (Jail.IsEnabled)
            {
                Jail.CheckPlayer(_cInfo);
            }
            if (Motd.IsEnabled & Motd.Show_On_Respawn)
            {
                Motd.Send(_cInfo);
            }
            if (Bloodmoon.Show_On_Login & Bloodmoon.Show_On_Respawn)
            {
                Bloodmoon.GetBloodmoon(_cInfo, false);
            }
            if (HatchElevator.IsEnabled & _respawnReason == RespawnType.Teleport)
            {
                HatchElevator.LastPositionY.Remove(_cInfo.entityId);
            }
            if (TeleportCheck.IsEnabled & _respawnReason == RespawnType.Teleport)
            {
                if (!Travel.TeleportCheckProtection.Contains(_cInfo.entityId))
                {
                    TeleportCheck.TeleportCheckValid(_cInfo);
                }
                else
                {
                    Travel.TeleportCheckProtection.Remove(_cInfo.entityId);
                }
                if (!CustomCommands.TeleportCheckProtection.Contains(_cInfo.entityId))
                {
                    TeleportCheck.TeleportCheckValid(_cInfo);
                }
                else
                {
                    CustomCommands.TeleportCheckProtection.Remove(_cInfo.entityId);
                }
                if (!TeleportHome.TeleportCheckProtection.Contains(_cInfo.entityId))
                {
                    TeleportCheck.TeleportCheckValid(_cInfo);
                }
                else
                {
                    TeleportHome.TeleportCheckProtection.Remove(_cInfo.entityId);
                }
            }
            if (StartingItems.IsEnabled)
            {
                StartingItems.StartingItemCheck(_cInfo);
            }
            if (ZoneProtection.Victim.ContainsKey(_cInfo.entityId))
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Type /return to teleport back to your death position. There is a two minute limit.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                PersistentContainer.Instance.Players[_cInfo.playerId, false].RespawnTime = DateTime.Now;
                PersistentContainer.Instance.Save();
                if (ZoneProtection.Forgive.ContainsKey(_cInfo.entityId))
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Type /forgive to release your killer from jail.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                }
            }
        }

        public override bool ChatMessage(ClientInfo _cInfo, EnumGameMessages _type, string _message, string _playerName, bool _localizeMain, string _secondaryName, bool _localizeSecondary)
        {
            return ChatHook.Hook(_cInfo, _message, _playerName, _secondaryName, _localizeSecondary);
        }

        public override void PlayerDisconnected(ClientInfo _cInfo, bool _bShutdown)
        {
            if (FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
            {
                FriendTeleport.Dict.Remove(_cInfo.entityId);
                FriendTeleport.Dict1.Remove(_cInfo.entityId);
            }
            if (Travel.TeleportCheckProtection.Contains(_cInfo.entityId))
            {
                Travel.TeleportCheckProtection.Remove(_cInfo.entityId);
            }
            if (CustomCommands.TeleportCheckProtection.Contains(_cInfo.entityId))
            {
                CustomCommands.TeleportCheckProtection.Remove(_cInfo.entityId);
            }
            if (TeleportHome.TeleportCheckProtection.Contains(_cInfo.entityId))
            {
                TeleportHome.TeleportCheckProtection.Remove(_cInfo.entityId);
            }
            if (Jail.Dict.ContainsKey(_cInfo.playerId))
            {
                Jail.Dict.Remove(_cInfo.playerId);
            }
            if (ZoneProtection.PvEFlag.Contains(_cInfo.entityId))
            {
                ZoneProtection.PvEFlag.Remove(_cInfo.entityId);
            }
            if (FlightCheck.Flag.ContainsKey(_cInfo.playerId))
            {
                FlightCheck.Flag.Remove(_cInfo.playerId);
            }
            if (FlightCheck.fLastPositionXZ.ContainsKey(_cInfo.entityId))
            {
                FlightCheck.fLastPositionXZ.Remove(_cInfo.entityId);
            }
            if (FlightCheck.fLastPositionY.ContainsKey(_cInfo.entityId))
            {
                FlightCheck.fLastPositionY.Remove(_cInfo.entityId);
            }
            if (UndergroundCheck.Flag.ContainsKey(_cInfo.playerId))
            {
                UndergroundCheck.Flag.Remove(_cInfo.playerId);
            }
            if (UndergroundCheck.uLastPositionXZ.ContainsKey(_cInfo.entityId))
            {
                UndergroundCheck.uLastPositionXZ.Remove(_cInfo.entityId);
            }            
        }

        public override void GameShutdown()
        {
            StateManager.Shutdown();
        }
    }
}
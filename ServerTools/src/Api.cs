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
            Timers.LogAlert();
            if (AuctionBox.IsEnabled)
            {
                AuctionBox.BuildAuctionList();
            }
            if (Animals.IsEnabled)
            {
                Animals.BuildList();
            }
            Timers.LoadAlert();
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
        }

        public override void PlayerLogin(ClientInfo _cInfo, string _compatibilityVersion)
        {
            if (StopServer.NoEntry)
            {
                int _seconds = (60 - Timers._sSCD);
                string _phrase452;
                if (!Phrases.Dict.TryGetValue(452, out _phrase452))
                {
                    _phrase452 = "Shutdown is in {Minutes} minutes {Seconds} seconds. Please come back after the server restarts.";
                }
                _phrase452 = _phrase452.Replace("{Minutes}", Timers._sSC.ToString());
                _phrase452 = _phrase452.Replace("{Seconds}", _seconds.ToString());
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase452), (ClientInfo)null);
            }
            if (ReservedSlots.IsEnabled)
            {
                ReservedSlots.SessionTime(_cInfo);
                ReservedSlots.CheckReservedSlot(_cInfo);
            }
        }

        public override void PlayerSpawning(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)
        {
            if (CredentialCheck.IsEnabled)
            {
                CredentialCheck.AccCheck(_cInfo);
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
                Bloodmoon.GetBloodmoon(_cInfo);
            }
            if (AutoShutdown.IsEnabled)
            {
                if (AutoShutdown.Alert_On_Login)
                {
                    AutoShutdown.CheckNextShutdown(_cInfo, false);
                }
            }
        }

        public override void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {
            if (Jail.IsEnabled)
            {
                Jail.CheckPlayer(_cInfo);
            }
            if (StartingItems.IsEnabled)
            {
                StartingItems.StartingItemCheck(_cInfo);
            }
            if (Motd.IsEnabled & Motd.Show_On_Respawn)
            {
                Motd.Send(_cInfo);
            }
            if (Bloodmoon.Show_On_Login & Bloodmoon.Show_On_Respawn)
            {
                Bloodmoon.GetBloodmoon(_cInfo);
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
                if (!FriendTeleport.TeleportCheckProtection.Contains(_cInfo.entityId))
                {
                    TeleportCheck.TeleportCheckValid(_cInfo);
                }
                else
                {
                    FriendTeleport.TeleportCheckProtection.Remove(_cInfo.entityId);
                }
            }           
            if (NewSpawnTele.IsEnabled)
            {
                NewSpawnTele.TeleNewSpawn(_cInfo);
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
            if (DeathSpot.IsEnabled && _respawnReason == RespawnType.Died)
            {
                DeathSpot.Died.Remove(_cInfo.entityId);
                DeathSpot.Died.Add(_cInfo.entityId, DateTime.Now);
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
            if (ZoneProtection.PvEFlag.ContainsKey(_cInfo.entityId))
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
    }
}
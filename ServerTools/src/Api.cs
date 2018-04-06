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
            HowToSetup.Load();
            Timers.LogAlert();
            if (Fps.IsEnabled)
            {
                Fps.SetTarget();
            }
            Timers.LoadAlert();
        }

        public override void SavePlayerData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            if (_cInfo != null)
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
                ReservedSlots.CheckReservedSlot1(_cInfo);
            }
        }

        public override void PlayerSpawning(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)
        {
            if (CredentialCheck.IsEnabled)
            {
                CredentialCheck.AccCheck(_cInfo);
            }
            if (Motd.IsEnabled && !Motd.Show_On_Respawn)
            {
                Motd.Send(_cInfo);
            }
            if (AutoShutdown.IsEnabled)
            {
                if (AutoShutdown.Alert_On_Login)
                {
                    AutoShutdown.CheckNextShutdown(_cInfo, false);
                }
            }
            if (Bloodmoon.Show_On_Login && !Bloodmoon.Show_On_Respawn)
            {
                Bloodmoon.GetBloodmoon(_cInfo);
            }
            if (LoginNotice.IsEnabled)
            {
                LoginNotice.PlayerCheck(_cInfo);
            }
        }

        public override void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {
            if (Motd.IsEnabled & Motd.Show_On_Respawn)
            {
                Motd.Send(_cInfo);
            }
            if (HatchElevator.IsEnabled && _respawnReason == RespawnType.Teleport)
            {
                HatchElevator.LastPositionY.Remove(_cInfo.entityId);
            }
            if (Bloodmoon.Show_On_Login && Bloodmoon.Show_On_Respawn)
            {
                Bloodmoon.GetBloodmoon(_cInfo);
            }
            if (StartingItems.IsEnabled && _respawnReason == RespawnType.EnterMultiplayer && !StartingItems.Received.Contains(_cInfo.playerId) || StartingItems.IsEnabled && _respawnReason == RespawnType.Teleport && !StartingItems.Received.Contains(_cInfo.playerId))
            {
                StartingItems.StartingItemCheck(_cInfo);
            }
            if (_respawnReason == RespawnType.EnterMultiplayer)
            {
                if (NewSpawnTele.IsEnabled)
                {
                    NewSpawnTele.TeleNewSpawn(_cInfo);
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, true].ZKills = 0;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].Deaths = 0;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].Kills = 0;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = 0;
                PersistentContainer.Instance.Save();
            }
            if (_respawnReason == RespawnType.JoinMultiplayer)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int _zCount = XUiM_Player.GetZombieKills(_player);
                int _deathCount = XUiM_Player.GetDeaths(_player);
                int _killCount = XUiM_Player.GetPlayerKills(_player);
                PersistentContainer.Instance.Players[_cInfo.playerId, true].ZKills = _zCount;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].Deaths = _deathCount;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].Kills = _killCount;
                PersistentContainer.Instance.Save();
            }
            if (_respawnReason == RespawnType.Died)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int _deathCount = XUiM_Player.GetDeaths(_player);
                PersistentContainer.Instance.Players[_cInfo.playerId, true].Deaths = _deathCount;
                PersistentContainer.Instance.Save();
                if (ZoneProtection.IsEnabled && ZoneProtection.Victim.ContainsKey(_cInfo.entityId))
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Type /return to teleport back to your death position. There is a two minute limit.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].RespawnTime = DateTime.Now;
                    PersistentContainer.Instance.Save();
                    if (ZoneProtection.Forgive.ContainsKey(_cInfo.entityId))
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Type /forgive to release your killer from jail.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    }
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
            if (DeathSpot.IsEnabled)
            {
                if (DeathSpot.Died.ContainsKey(_cInfo.entityId))
                {
                    DeathSpot.Died.Remove(_cInfo.entityId);
                    DeathSpot.Position.Remove(_cInfo.entityId);
                    DeathSpot.Flag.Remove(_cInfo.entityId);
                }
            }
            if (ZoneProtection.PvEFlag.ContainsKey(_cInfo.entityId))
            {
                ZoneProtection.PvEFlag.Remove(_cInfo.entityId);
                ZoneProtection.PlayerKills.Remove(_cInfo.entityId);
                ZoneProtection.Forgive.Remove(_cInfo.entityId);
                ZoneProtection.Victim.Remove(_cInfo.entityId);
            }
            if (FlightCheck.Flag.Contains(_cInfo.entityId))
            {
                FlightCheck.Flag.Remove(_cInfo.entityId);
            }
            if (FlightCheck.fLastPositionXZ.ContainsKey(_cInfo.entityId))
            {
                FlightCheck.fLastPositionXZ.Remove(_cInfo.entityId);
            }
            if (FlightCheck.fLastPositionY.ContainsKey(_cInfo.entityId))
            {
                FlightCheck.fLastPositionY.Remove(_cInfo.entityId);
            }
            if (FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
            {
                FriendTeleport.Dict.Remove(_cInfo.entityId);
            }
            if (FriendTeleport.Dict1.ContainsKey(_cInfo.entityId))
            {
                FriendTeleport.Dict1.Remove(_cInfo.entityId);
            }
            if (ReservedSlots.Session.ContainsKey(_cInfo.playerId))
            {
                ReservedSlots.Session.Remove(_cInfo.playerId);
            }
            if (Travel.Flag.Contains(_cInfo.entityId))
            {
                Travel.Flag.Remove(_cInfo.entityId);
            }
            if (UndergroundCheck.Flag.Contains(_cInfo.entityId))
            {
                UndergroundCheck.Flag.Remove(_cInfo.entityId);
            }
            if (UndergroundCheck.uLastPositionXZ.ContainsKey(_cInfo.entityId))
            {
                UndergroundCheck.uLastPositionXZ.Remove(_cInfo.entityId);
            }
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            int _zCount = XUiM_Player.GetZombieKills(_player);
            int _deathCount = XUiM_Player.GetDeaths(_player);
            int _killCount = XUiM_Player.GetPlayerKills(_player);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].ZKills = _zCount;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].Deaths = _deathCount;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].Kills = _killCount;
            PersistentContainer.Instance.Save();
        }
    }
}
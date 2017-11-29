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
            if (InventoryCheck.IsEnabled || InventoryCheck.AnounceInvalidStack)
            {
                InventoryCheck.CheckInv(_cInfo, _playerDataFile);
            }
            if (Watchlist.IsEnabled)
            {
                Watchlist.CheckWatchlist(_cInfo);
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
            if (ClanManager.IsEnabled)
            {
                ClanManager.CheckforClantag(_cInfo);
            }
            if (Motd.IsEnabled & !Motd.ShowOnRespawn)
            {
                Motd.Send(_cInfo);
            }
            if (Bloodmoon.ShowOnSpawn & !Bloodmoon.ShowOnRespawn)
            {
                Bloodmoon.GetBloodmoon(_cInfo, false);
            }
        }

        public override void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {
            if (Jail.IsEnabled)
            {
                Jail.CheckPlayer(_cInfo);
            }
            if (NewSpawnTele.IsEnabled)
            {
                NewSpawnTele.TeleNewSpawn(_cInfo);
            }
            if (Motd.IsEnabled & Motd.ShowOnRespawn)
            {
                Motd.Send(_cInfo);
            }
            if (Bloodmoon.ShowOnSpawn & Bloodmoon.ShowOnRespawn)
            {
                Bloodmoon.GetBloodmoon(_cInfo, false);
            }
        }

        public override bool ChatMessage(ClientInfo _cInfo, EnumGameMessages _type, string _message, string _playerName, bool _localizeMain, string _secondaryName, bool _localizeSecondary)
        {
            return ChatHook.Hook(_cInfo, _message, _playerName, _secondaryName, _localizeSecondary);
        }

        public override void PlayerDisconnected(ClientInfo _cInfo, bool _bShutdown)
        {
            if (Jail.Dict.ContainsKey(_cInfo.playerId))
            {
                Jail.Dict.Remove(_cInfo.playerId);
            }
        }

        public override void GameShutdown()
        {
            StateManager.Shutdown();
        }
    }
}
using System.IO;

namespace ServerTools
{
    public class API : ModApiAbstract
    {
        public static string GamePath = GamePrefs.GetString(EnumGamePrefs.SaveGameFolder);
        public static string ConfigPath = string.Format("{0}/ServerTools", GamePath);
        public static int MaxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
        public static string GameWorld = GamePrefs.GetString(EnumGamePrefs.GameWorld);
        public static string GameName = GamePrefs.GetString(EnumGamePrefs.GameName);

        public override void GameAwake()
        {
            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
            StateManager.Awake();
            GameItems.LoadGameItems();
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
            if (Motd.IsEnabled)
            {
                Motd.Send(_cInfo);
            }
            if (ClanManager.IsEnabled)
            {
                ClanManager.CheckforClantag(_cInfo);
            }
        }

        public override bool ChatMessage(ClientInfo _cInfo, EnumGameMessages _type, string _message, string _playerName, bool _localizeMain, string _secondaryName, bool _localizeSecondary)
        {
            return ChatHook.Hook(_cInfo, _message, _playerName, _secondaryName, _localizeSecondary);
        }

        public override void GameShutdown()
        {
            StateManager.Shutdown();
        }
    }
}
using System.IO;

namespace ServerTools
{
    public class API : ModApiAbstract
    {
        public override void GameAwake()
        {
            if (!Directory.Exists(string.Format("{0}/ServerTools", GamePrefs.GetString(EnumGamePrefs.SaveGameFolder))))
            {
                Directory.CreateDirectory(string.Format("{0}/ServerTools", GamePrefs.GetString(EnumGamePrefs.SaveGameFolder)));
            }
            Config.Init();
        }
        public override void SavePlayerData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            if (HighPingKicker.IsEnabled)
            {
                HighPingKicker.CheckPing(_cInfo);
            }
            if (InventoryCheck.IsEnabled)
            {
                InventoryCheck.CheckInv(_cInfo, _playerDataFile);
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
        public override bool ChatMessage(ClientInfo _cInfo, string _message, string _playerName)
        {
            return ChatHook.IsCommand(_cInfo, _message, _playerName);
        }
    }
}
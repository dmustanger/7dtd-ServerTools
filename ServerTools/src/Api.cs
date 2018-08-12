using System;
using System.Data;
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
            RestartVote.Startup = true;
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
                if (DupeLog.IsEnabled)
                {
                    DupeLog.Exec(_cInfo, _playerDataFile);
                }
            }
        }

        public override void PlayerLogin(ClientInfo _cInfo, string _compatibilityVersion)
        {
            if (_cInfo != null)
            {
                string _sql = string.Format("SELECT steamid FROM Players WHERE steamid = '{0}'", _cInfo.playerId); ;
                DataTable _result = SQL.TQuery(_sql);
                string _name = SQL.EscapeString(_cInfo.playerName);
                if (_result.Rows.Count == 0)
                {
                    _sql = string.Format("INSERT INTO Players (steamid, playername, last_joined) VALUES ('{0}', '{1}', '{2}')", _cInfo.playerId, _name, DateTime.Now);
                    SQL.FastQuery(_sql);
                }
                else
                {
                    _sql = string.Format("UPDATE Players SET playername = '{0}, last_joined = '{1}' WHERE steamid = '{2}'", _name, DateTime.Now, _cInfo.playerId);
                }
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
                    if (ReservedSlots.Kicked.ContainsKey(_cInfo.playerName))
                    {
                        DateTime _dateTime;
                        ReservedSlots.Kicked.TryGetValue(_cInfo.playerId, out _dateTime);
                        TimeSpan varTime = DateTime.Now - _dateTime;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed >= 5)
                        {
                            int _timeleft = 5 - _timepassed;
                            string _phrase22;
                            if (!Phrases.Dict.TryGetValue(22, out _phrase22))
                            {
                                _phrase22 = "Sorry {PlayerName} you have reached the max session time. Please wait {TimeRemaining} minutes before rejoining.";
                            }
                            _phrase22 = _phrase22.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase22 = _phrase22.Replace("{TimeRemaining}", _timeleft.ToString());
                            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase22), (ClientInfo)null);
                        }
                        else
                        {
                            ReservedSlots.CheckReservedSlot(_cInfo);
                        }
                    }
                    else
                    {
                        ReservedSlots.CheckReservedSlot(_cInfo);
                    }
                }
            }
        }

        public override void PlayerSpawning(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)
        {
            if (_cInfo != null)
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
                    Bloodmoon.GetBloodmoon(_cInfo, false);
                }
                if (LoginNotice.IsEnabled)
                {
                    LoginNotice.PlayerCheck(_cInfo);
                }
                Players.SessionTime(_cInfo);
            }
        }

        public override void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {
            if (_cInfo != null)
            {
                if (Motd.IsEnabled & Motd.Show_On_Respawn)
                {
                    Motd.Send(_cInfo);
                }
                if (Bloodmoon.Show_On_Login && Bloodmoon.Show_On_Respawn)
                {
                    Bloodmoon.GetBloodmoon(_cInfo, false);
                }
                if (_respawnReason == RespawnType.EnterMultiplayer)
                {
                    if (NewPlayer.IsEnabled)
                    {
                        NewPlayer.Exec(_cInfo);
                    }
                    if (NewSpawnTele.IsEnabled)
                    {
                        NewSpawnTele.TeleNewSpawn(_cInfo);
                    }
                    if (StartingItems.IsEnabled)
                    {
                        if (!NewSpawnTele.IsEnabled)
                        {
                            StartingItems.StartingItemCheck(_cInfo);
                        }
                        else if (NewSpawnTele.IsEnabled && NewSpawnTele.New_Spawn_Tele_Position != "0,0,0")
                        {
                            StartingItems.Que.Add(_cInfo.playerId);
                        }
                    }
                    if (PersistentContainer.Instance.PollOpen && !Poll.PolledYes.Contains(_cInfo.entityId) && !Poll.PolledNo.Contains(_cInfo.entityId))
                    {
                        Poll.Message(_cInfo);
                    }
                    if (Hardcore.IsEnabled)
                    {
                        Hardcore.Announce(_cInfo);
                    }
                    string _sql = string.Format("UPDATE Players playername = '{0}' wallet = 0, playerSpentCoins = 0, sessionTime = 0, zkills = 0, kills = 0, deaths = 0 WHERE steamid = '{1}'", _cInfo.playerName, _cInfo.playerId);
                    SQL.FastQuery(_sql);
                }
                if (_respawnReason == RespawnType.JoinMultiplayer)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    int _zCount = XUiM_Player.GetZombieKills(_player);
                    int _deathCount = XUiM_Player.GetDeaths(_player);
                    int _killCount = XUiM_Player.GetPlayerKills(_player);
                    Players.FriendList(_cInfo);
                    if (PersistentContainer.Instance.PollOpen && !Poll.PolledYes.Contains(_cInfo.entityId) && !Poll.PolledNo.Contains(_cInfo.entityId))
                    {
                        Poll.Message(_cInfo);
                    }
                    if (Hardcore.IsEnabled)
                    {
                        Hardcore.Check(_cInfo);
                    }
                    if (PersistentContainer.Instance.Players[_cInfo.playerId, true].EventReturn != null)
                    {
                        Event.OfflineReturn(_cInfo);
                    }
                    string _sql = string.Format("UPDATE Players SET playername = '{0}', zkills = {1}, kills = {2}, deaths = {3} WHERE steamid = '{4}'", _cInfo.playerName, _zCount, _killCount, _deathCount, _cInfo.playerId);
                    SQL.FastQuery(_sql);
                    if (Mogul.IsEnabled)
                    {
                        if (Wallet.IsEnabled)
                        {
                            World world = GameManager.Instance.World;
                            _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                            DataTable _result = SQL.TQuery(_sql);
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int spentCoins);
                            _result.Dispose();
                            int currentCoins = 0;
                            int gameMode = world.GetGameMode();
                            if (gameMode == 7)
                            {
                                currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + spentCoins;
                            }
                            else
                            {
                                currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + spentCoins;
                            }
                            _sql = string.Format("UPDATE Players SET wallet = {0} WHERE steamid = '{1}'", currentCoins, _cInfo.playerId);
                            SQL.FastQuery(_sql);
                        }
                    }
                }
                if (_respawnReason == RespawnType.Died)
                {
                    if (Hardcore.IsEnabled)
                    {
                        Hardcore.Check(_cInfo);
                    }
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    string _sql = string.Format("UPDATE Players SET deaths = {0} WHERE steamid = '{1}'", XUiM_Player.GetDeaths(_player), _cInfo.playerId);
                    SQL.FastQuery(_sql);
                    if (Event.Open && Event.SpawnList.Contains(_cInfo.entityId))
                    {
                        Event.Died(_cInfo);
                    }
                    if (Zones.IsEnabled && Players.Victim.ContainsKey(_cInfo.entityId))
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Type /return to teleport back to your death position. There is a time limit.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].RespawnTime = DateTime.Now;
                        if (Players.Forgive.ContainsKey(_cInfo.entityId))
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Type /forgive to release your killer from jail.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    if (Mogul.IsEnabled)
                    {
                        if (Wallet.IsEnabled)
                        {
                            World world = GameManager.Instance.World;
                            _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                            DataTable _result = SQL.TQuery(_sql);
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int spentCoins);
                            _result.Dispose();
                            int currentCoins = 0;
                            int gameMode = world.GetGameMode();
                            if (gameMode == 7)
                            {
                                currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + spentCoins;
                            }
                            else
                            {
                                currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + spentCoins;
                            }
                            _sql = string.Format("UPDATE Players SET wallet = {0} WHERE steamid = '{1}'", currentCoins, _cInfo.playerId);
                            SQL.FastQuery(_sql);
                        }
                    }
                    PersistentContainer.Instance.Save();
                }
                if (_respawnReason == RespawnType.Teleport)
                {
                    if (StartingItems.IsEnabled && StartingItems.Que.Contains(_cInfo.playerId))
                    {
                        StartingItems.StartingItemCheck(_cInfo);
                        StartingItems.Que.Remove(_cInfo.playerId);
                    }
                    if (Players.NoFlight.Contains(_cInfo.entityId))
                    {
                        Players.NoFlight.Remove(_cInfo.entityId);
                    }
                    if (HatchElevator.IsEnabled)
                    {
                        HatchElevator.LastPositionY.Remove(_cInfo.entityId);
                    }
                }
                if (Players.Dead.Contains(_cInfo.entityId))
                {
                    Players.Dead.Remove(_cInfo.entityId);
                }
            }
        }

        public override bool ChatMessage(ClientInfo _cInfo, EnumGameMessages _type, string _message, string _playerName, bool _localizeMain, string _secondaryName, bool _localizeSecondary)
        {
            return ChatHook.Hook(_cInfo, _message, _playerName, _secondaryName, _localizeSecondary);
        }

        public override void PlayerDisconnected(ClientInfo _cInfo, bool _bShutdown)
        {
            if (_cInfo != null)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    if (_player.IsAlive())
                    {
                        string _sql = string.Format("UPDATE Players SET zkills = {0}, kills = {1}, deaths = {2} WHERE steamid = '{3}'", XUiM_Player.GetZombieKills(_player), XUiM_Player.GetPlayerKills(_player), XUiM_Player.GetDeaths(_player), _cInfo.playerId);
                        SQL.FastQuery(_sql);
                        if (Mogul.IsEnabled)
                        {
                            if (Wallet.IsEnabled)
                            {
                                World world = GameManager.Instance.World;
                                _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                DataTable _result = SQL.TQuery(_sql);
                                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int spentCoins);
                                _result.Dispose();
                                int currentCoins = 0;
                                int gameMode = world.GetGameMode();
                                if (gameMode == 7)
                                {
                                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + spentCoins;
                                }
                                else
                                {
                                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + spentCoins;
                                }
                                _sql = string.Format("UPDATE Players SET wallet = {0} WHERE steamid = '{1}'", currentCoins, _cInfo.playerId);
                                SQL.FastQuery(_sql);
                            }
                        }
                        PersistentContainer.Instance.Save();
                    }
                }
                if (HatchElevator.LastPositionY.ContainsKey(_cInfo.entityId))
                {
                    HatchElevator.LastPositionY.Remove(_cInfo.entityId);
                }
                if (FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                {
                    FriendTeleport.Dict.Remove(_cInfo.entityId);
                    FriendTeleport.Dict1.Remove(_cInfo.entityId);
                }
                if (Players.ZoneExit.ContainsKey(_cInfo.entityId))
                {
                    Players.ZoneExit.Remove(_cInfo.entityId);
                    Players.Forgive.Remove(_cInfo.entityId);
                    Players.Victim.Remove(_cInfo.entityId);
                }
                if (FlightCheck.Flag.ContainsKey(_cInfo.entityId))
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
                if (Travel.Flag.Contains(_cInfo.entityId))
                {
                    Travel.Flag.Remove(_cInfo.entityId);
                }
                if (UndergroundCheck.Flag.ContainsKey(_cInfo.entityId))
                {
                    UndergroundCheck.Flag.Remove(_cInfo.entityId);
                }
                if (UndergroundCheck.uLastPositionXZ.ContainsKey(_cInfo.entityId))
                {
                    UndergroundCheck.uLastPositionXZ.Remove(_cInfo.entityId);
                }
                if (Wallet.IsEnabled)
                {
                    if (PersistentContainer.Instance.Players[_cInfo.playerId, false] != null)
                    {
                        DateTime _time;
                        if (Players.Session.TryGetValue(_cInfo.playerId, out _time))
                        {
                            TimeSpan varTime = DateTime.Now - _time;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (_timepassed > 60)
                            {
                                int _hours = _timepassed / 60 * 10;
                                string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                DataTable _result = SQL.TQuery(_sql);
                                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int _oldCoin);
                                _result.Dispose();
                                _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _oldCoin + _hours, _cInfo.playerId);
                                SQL.FastQuery(_sql);
                            }
                            string _sql1 = string.Format("SELECT sessionTime FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                            DataTable _result1 = SQL.TQuery(_sql1);
                            int.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out int _sessionTime);
                            _result1.Dispose();
                            _sql1 = string.Format("UPDATE Players SET sessionTime = {0} WHERE steamid = '{1}'", _sessionTime + _timepassed, _cInfo.playerId);
                            SQL.FastQuery(_sql1);
                        }
                    }
                }
                if (Players.Session.ContainsKey(_cInfo.playerId))
                {
                    Players.Session.Remove(_cInfo.playerId);
                }
                if (Bank.TransferId.ContainsKey(_cInfo.playerId))
                {
                    Bank.TransferId.Remove(_cInfo.playerId);
                }
                if (Zones.reminder.ContainsKey(_cInfo.entityId))
                {
                    Zones.reminder.Remove(_cInfo.entityId);
                    Zones.reminderMsg.Remove(_cInfo.entityId);
                }
            }
        }
    }
}
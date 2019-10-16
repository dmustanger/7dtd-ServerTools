using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace ServerTools
{
    public class API : IModApi
    {
        public static string GamePath = Directory.GetCurrentDirectory();
        public static string ConfigPath = string.Format("{0}/ServerTools", GamePath);
        public static int MaxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
        public static List<ClientInfo> Que = new List<ClientInfo>();

        public void InitMod()
        {
            ModEvents.GameAwake.RegisterHandler(GameAwake);
            ModEvents.GameShutdown.RegisterHandler(GameShutdown);
            ModEvents.SavePlayerData.RegisterHandler(SavePlayerData);
            ModEvents.PlayerSpawning.RegisterHandler(PlayerSpawning);
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(PlayerSpawnedInWorld);
            ModEvents.PlayerDisconnected.RegisterHandler(PlayerDisconnected);
            ModEvents.ChatMessage.RegisterHandler(ChatMessage);
            ModEvents.GameStartDone.RegisterHandler(GameStartDone);
            ModEvents.EntityKilled.RegisterHandler(EntityKilled);
        }

        public void GameAwake()
        {

        }

        public void SavePlayerData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            if (_cInfo != null && _playerDataFile != null)
            {
                if (HighPingKicker.IsEnabled)
                {
                    HighPingKicker.CheckPing(_cInfo);
                }
                if (InventoryCheck.IsEnabled || InventoryCheck.Announce_Invalid_Stack)
                {
                    InventoryCheck.CheckInv(_cInfo, _playerDataFile);
                }
                if (DupeLog.IsEnabled)
                {
                    DupeLog.Exec(_cInfo, _playerDataFile);
                }
            }
        }

        public void PlayerSpawning(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)
        {
            if (_cInfo != null)
            {
                if (CountryBan.IsEnabled && _cInfo.ip != "127.0.0.1" && !_cInfo.ip.StartsWith("192.168"))
                {
                    if (CountryBan.IsCountryBanned(_cInfo))
                    {
                        return;
                    }
                }
                string _sql = string.Format("SELECT steamid FROM Players WHERE steamid = '{0}'", _cInfo.playerId); ;
                DataTable _result = SQL.TQuery(_sql);
                string _name = SQL.EscapeString(_cInfo.playerName);
                _name = Regex.Replace(_name, @"[^\u0000-\u007F]+", string.Empty);
                if (_result.Rows.Count == 0)
                {
                    _sql = string.Format("INSERT INTO Players (steamid, playername, last_joined) VALUES ('{0}', '{1}', '{2}')", _cInfo.playerId, _name, DateTime.Now);
                }
                else
                {
                    _sql = string.Format("UPDATE Players SET playername = '{0}', last_joined = '{1}' WHERE steamid = '{2}'", _name, DateTime.Now, _cInfo.playerId);
                }
                _result.Dispose();
                SQL.FastQuery(_sql, "API");
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
                        if (_timepassed <= 5)
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
                            ReservedSlots.Kicked.Remove(_cInfo.playerId);
                            ReservedSlots.CheckReservedSlot(_cInfo);
                        }
                    }
                    else
                    {
                        ReservedSlots.CheckReservedSlot(_cInfo);
                    }
                }
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

        public void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {
            if (_cInfo != null)
            {
                string _name = SQL.EscapeString(_cInfo.playerName);
                _name = Regex.Replace(_name, @"[^\u0000-\u007F]+", string.Empty);
                if (_respawnReason == RespawnType.EnterMultiplayer)
                {
                    Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    Que.Add(_cInfo);
                }
                if (_respawnReason == RespawnType.JoinMultiplayer)
                {
                    if (Motd.IsEnabled & Motd.Show_On_Respawn)
                    {
                        Motd.Send(_cInfo);
                    }
                    if (Bloodmoon.Show_On_Login && Bloodmoon.Show_On_Respawn)
                    {
                        Bloodmoon.GetBloodmoon(_cInfo, false);
                    }
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    int _zCount = XUiM_Player.GetZombieKills(_player);
                    int _deathCount = XUiM_Player.GetDeaths(_player);
                    int _killCount = XUiM_Player.GetPlayerKills(_player);
                    string _sql = "SELECT pollOpen FROM Polls WHERE pollOpen = 'true'";
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count > 0 && !PollConsole.PolledYes.Contains(_cInfo.playerId) && !PollConsole.PolledNo.Contains(_cInfo.playerId))
                    {
                        PollConsole.Message(_cInfo);
                    }
                    _result.Dispose();
                    if (Event.Open)
                    {
                        if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
                        {
                            if (Hardcore.IsEnabled)
                            {
                                Hardcore.Check(_cInfo);
                            }
                        }
                        else
                        {
                            _sql = string.Format("SELECT eventRespawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                            DataTable _result1 = SQL.TQuery(_sql);
                            bool _eventRespawn;
                            bool.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _eventRespawn);
                            _result1.Dispose();
                            if (_eventRespawn)
                            {
                                Event.Died(_cInfo);
                            }
                            else
                            {
                                _sql = string.Format("SELECT return, eventSpawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                DataTable _result2 = SQL.TQuery(_sql);
                                bool _return1 = false, _return2 = false;
                                bool.TryParse(_result2.Rows[0].ItemArray.GetValue(0).ToString(), out _return1);
                                bool.TryParse(_result2.Rows[0].ItemArray.GetValue(1).ToString(), out _return2);
                                _result2.Dispose();
                                if (_return1)
                                {
                                    if (_return2)
                                    {
                                        _sql = string.Format("UPDATE Players SET eventSpawn = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                                        SQL.FastQuery(_sql, "API");
                                    }
                                    _sql = string.Format("UPDATE Players SET return = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                                    SQL.FastQuery(_sql, "API");
                                    Event.EventReturn(_cInfo);
                                }
                                else if (_return2)
                                {
                                    Event.EventSpawn(_cInfo);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Hardcore.IsEnabled)
                        {
                            Hardcore.Check(_cInfo);
                        }
                    }
                    _sql = string.Format("UPDATE Players SET playername = '{0}', zkills = {1}, kills = {2}, deaths = {3} WHERE steamid = '{4}'", _name, _zCount, _killCount, _deathCount, _cInfo.playerId);
                    SQL.FastQuery(_sql, "API");
                    if (Mogul.IsEnabled)
                    {
                        if (Wallet.IsEnabled)
                        {
                            int currentCoins = Wallet.GetcurrentCoins(_cInfo);
                            _sql = string.Format("UPDATE Players SET wallet = {0} WHERE steamid = '{1}'", currentCoins, _cInfo.playerId);
                            SQL.FastQuery(_sql, "API");
                        }
                    }
                }
                if (_respawnReason == RespawnType.Died)
                {
                    if (Players.Died.Contains(_cInfo.entityId))
                    {
                        Players.Died.Remove(_cInfo.entityId);
                    }
                    if (Bloodmoon.Show_On_Login && Bloodmoon.Show_On_Respawn)
                    {
                        Bloodmoon.GetBloodmoon(_cInfo, false);
                    }
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (Event.Open)
                    {
                        if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
                        {
                            if (Wallet.Lose_On_Death)
                            {
                                Wallet.ClearWallet(_cInfo);
                            }
                            if (Hardcore.IsEnabled)
                            {
                                Hardcore.Check(_cInfo);
                            }
                            string _sql = string.Format("SELECT return, eventSpawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                            DataTable _result1 = SQL.TQuery(_sql);
                            bool _return1 = false, _return2 = false;
                            bool.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _return1);
                            bool.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _return2);
                            _result1.Dispose();
                            if (_return1)
                            {
                                if (_return2)
                                {
                                    _sql = string.Format("UPDATE Players SET eventSpawn = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                                    SQL.FastQuery(_sql, "API");
                                }
                                _sql = string.Format("UPDATE Players SET return = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                                SQL.FastQuery(_sql, "API");
                                Event.EventReturn(_cInfo);
                            }
                        }
                        else
                        {
                            string _sql = string.Format("SELECT eventRespawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                            DataTable _result = SQL.TQuery(_sql);
                            bool _eventRespawn;
                            bool.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _eventRespawn);
                            _result.Dispose();
                            if (_eventRespawn)
                            {
                                Event.Died(_cInfo);
                            }
                            else
                            {
                                _sql = string.Format("SELECT return, eventSpawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                DataTable _result1 = SQL.TQuery(_sql);
                                bool _return1 = false, _return2 = false;
                                bool.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _return1);
                                bool.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _return2);
                                _result1.Dispose();
                                if (_return1)
                                {
                                    if (_return2)
                                    {
                                        _sql = string.Format("UPDATE Players SET eventSpawn = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                                        SQL.FastQuery(_sql, "API");
                                    }
                                    _sql = string.Format("UPDATE Players SET return = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                                    SQL.FastQuery(_sql, "API");
                                    Event.EventReturn(_cInfo);
                                }
                                else if (_return2)
                                {
                                    Event.EventSpawn(_cInfo);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Wallet.Lose_On_Death)
                        {
                            Wallet.ClearWallet(_cInfo);
                        }
                        if (Hardcore.IsEnabled)
                        {
                            Hardcore.Check(_cInfo);
                        }
                        string _sql = string.Format("SELECT return, eventSpawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result1 = SQL.TQuery(_sql);
                        bool _return1 = false, _return2 = false;
                        bool.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _return1);
                        bool.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _return2);
                        _result1.Dispose();
                        if (_return1)
                        {
                            if (_return2)
                            {
                                _sql = string.Format("UPDATE Players SET eventSpawn = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                                SQL.FastQuery(_sql, "API");
                            }
                            _sql = string.Format("UPDATE Players SET return = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                            SQL.FastQuery(_sql, "API");
                            Event.EventReturn(_cInfo);
                        }
                    }
                    string _sql2 = string.Format("UPDATE Players SET deaths = {0} WHERE steamid = '{1}'", XUiM_Player.GetDeaths(_player), _cInfo.playerId);
                    SQL.FastQuery(_sql2, "API");
                    if (Zones.IsEnabled && Zones.Victim.ContainsKey(_cInfo.entityId))
                    {
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", type /return to teleport back to your death position. There is a time limit.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        _sql2 = string.Format("UPDATE Players SET respawnTime = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                        SQL.FastQuery(_sql2, "API");
                        if (Zones.Forgive.ContainsKey(_cInfo.entityId))
                        {
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", type /forgive to release your killer from jail.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    if (Mogul.IsEnabled)
                    {
                        if (Wallet.IsEnabled)
                        {
                            int currentCoins = Wallet.GetcurrentCoins(_cInfo);
                            _sql2 = string.Format("UPDATE Players SET wallet = {0} WHERE steamid = '{1}'", currentCoins, _cInfo.playerId);
                            SQL.FastQuery(_sql2, "API");
                        }
                    }
                }
                if (_respawnReason == RespawnType.Teleport)
                {
                    if (StartingItems.IsEnabled && StartingItems.Que.Contains(_cInfo.playerId))
                    {
                        StartingItems.StartingItemCheck(_cInfo);
                        StartingItems.Que.Remove(_cInfo.playerId);
                    }
                    if (HatchElevator.IsEnabled)
                    {
                        HatchElevator.LastPositionY.Remove(_cInfo.entityId);
                    }
                }
            }
        }

        public static void NewPlayerExec()
        {
            ClientInfo _cInfo = Que[0];
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_cInfo != null && _player.IsSpawned())
            {
                if (Motd.IsEnabled & Motd.Show_On_Respawn)
                {
                    Motd.Send(_cInfo);
                }
                if (Bloodmoon.Show_On_Login && Bloodmoon.Show_On_Respawn)
                {
                    Bloodmoon.GetBloodmoon(_cInfo, false);
                }
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
                    else if (NewSpawnTele.IsEnabled && NewSpawnTele.New_Spawn_Tele_Position == "0,0,0")
                    {
                        StartingItems.StartingItemCheck(_cInfo);
                    }
                }
                string _sql = "SELECT pollOpen FROM Polls WHERE pollOpen = 'true'";
                DataTable _result1 = SQL.TQuery(_sql);
                if (_result1.Rows.Count > 0 && !PollConsole.PolledYes.Contains(_cInfo.playerId) && !PollConsole.PolledNo.Contains(_cInfo.playerId))
                {
                    PollConsole.Message(_cInfo);
                }
                _result1.Dispose();
                if (Hardcore.IsEnabled)
                {
                    Hardcore.Announce(_cInfo);
                }
                string _name = SQL.EscapeString(_cInfo.playerName);
                _name = Regex.Replace(_name, @"[^\u0000-\u007F]+", string.Empty);
                _sql = string.Format("UPDATE Players SET playername = '{0}', wallet = 0, playerSpentCoins = 0, sessionTime = 0, zkills = 0, kills = 0, deaths = 0 WHERE steamid = '{1}'", _name, _cInfo.playerId);
                SQL.FastQuery(_sql, "API");
                Que.RemoveAt(0);
            }
        }

        public bool ChatMessage(ClientInfo _cInfo, EChatType _type, int _senderId, string _msg, string _mainName, bool _localizeMain, List<int> _recipientEntityIds)
        {
            return ChatHook.Hook(_cInfo, _type, _senderId, _msg, _mainName, _localizeMain, _recipientEntityIds);
        }

        public void PlayerDisconnected(ClientInfo _cInfo, bool _bShutdown)
        {
            if (_cInfo != null)
            {
                try
                {
                    if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (_player != null)
                        {
                            string _sql = string.Format("UPDATE Players SET zkills = {0}, kills = {1}, deaths = {2} WHERE steamid = '{3}'", XUiM_Player.GetZombieKills(_player), XUiM_Player.GetPlayerKills(_player), XUiM_Player.GetDeaths(_player), _cInfo.playerId);
                            SQL.FastQuery(_sql, "API");
                            if (Wallet.IsEnabled)
                            {
                                int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                                _sql = string.Format("UPDATE Players SET wallet = {0} WHERE steamid = '{1}'", _currentCoins, _cInfo.playerId);
                                SQL.FastQuery(_sql, "API");
                            }
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
                    if (Zones.ZoneExit.ContainsKey(_cInfo.entityId))
                    {
                        Zones.ZoneExit.Remove(_cInfo.entityId);
                    }
                    if (Zones.Forgive.ContainsKey(_cInfo.entityId))
                    {
                        Zones.Forgive.Remove(_cInfo.entityId);
                    }
                    if (Zones.Victim.ContainsKey(_cInfo.entityId))
                    {
                        Zones.Victim.Remove(_cInfo.entityId);
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
                    if (Wallet.IsEnabled)
                    {
                        string _sql2 = string.Format("SELECT steamid FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result2 = SQL.TQuery(_sql2);
                        if (_result2.Rows.Count > 0)
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
                                    Wallet.AddCoinsToWallet(_cInfo.playerId, _hours);
                                }
                                string _sql1 = string.Format("SELECT sessionTime FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                DataTable _result1 = SQL.TQuery(_sql1);
                                int _sessionTime;
                                int.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _sessionTime);
                                _result1.Dispose();
                                _sql1 = string.Format("UPDATE Players SET sessionTime = {0} WHERE steamid = '{1}'", _sessionTime + _timepassed, _cInfo.playerId);
                                SQL.FastQuery(_sql1, "API");
                            }
                        }
                        _result2.Dispose();
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
                    }
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in Player Disconnection: {0}.", e.Message));
                }
            }
        }

        public void EntityKilled(Entity _deadEnt, Entity _killer)
        {

        }

        public void GameStartDone()
        {
            LoadProcess.Load(1);
            Tracking.Cleanup();
        }

        public void GameShutdown()
        {

        }
    }
}
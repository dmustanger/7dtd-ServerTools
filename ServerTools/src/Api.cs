using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ServerTools.AntiCheat;

namespace ServerTools
{
    public class API : IModApi
    {
        public static string GamePath = Directory.GetCurrentDirectory();
        public static string ConfigPath = string.Format("{0}/ServerTools", GamePath);
        public static List<string> Verified = new List<string>();

        public void InitMod()
        {
            ModEvents.GameAwake.RegisterHandler(GameAwake);
            ModEvents.GameStartDone.RegisterHandler(GameStartDone);
            ModEvents.GameShutdown.RegisterHandler(GameShutdown);

            ModEvents.PlayerLogin.RegisterHandler(PlayerLogin);
            ModEvents.PlayerSpawning.RegisterHandler(PlayerSpawning);
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(PlayerSpawnedInWorld);
            ModEvents.ChatMessage.RegisterHandler(ChatMessage);
            ModEvents.GameMessage.RegisterHandler(GameMessage);
            ModEvents.SavePlayerData.RegisterHandler(SavePlayerData);
            ModEvents.PlayerDisconnected.RegisterHandler(PlayerDisconnected);

            ModEvents.EntityKilled.RegisterHandler(EntityKilled);
        }

        private void GameAwake()
        {
            
        }

        private static void GameStartDone()
        {
            LoadProcess.Load(1);
            Tracking.Cleanup();
            Confirm.Exec();
        }

        private static void GameShutdown()
        {
            StateManager.Shutdown();
            StopServer.Shutdown = true;
        }

        private static bool PlayerLogin(ClientInfo _cInfo, string _message, StringBuilder _stringBuild)//Initiating player login
        {
            try
            {
                Log.Out("[SERVERTOOLS] Player detected connecting");
                if (StopServer.NoEntry)
                {
                    _stringBuild = new StringBuilder("{ServerResponseName}- Server is shutting down. Rejoin when it restarts");
                    _stringBuild = _stringBuild.Replace("{ServerResponseName}", LoadConfig.Server_Response_Name);
                    return false;
                }
                if (_cInfo.playerId != null && _cInfo.playerId.Length == 17)
                {
                    if (ReservedSlots.IsEnabled && ReservedSlots.Kicked.ContainsKey(_cInfo.playerId))
                    {
                        DateTime _dateTime;
                        ReservedSlots.Kicked.TryGetValue(_cInfo.playerId, out _dateTime);
                        TimeSpan varTime = DateTime.Now - _dateTime;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed < 5)
                        {
                            _stringBuild = new StringBuilder("{" + LoadConfig.Server_Response_Name + "}" + " You reached the max session time. Come back in a few minutes");
                            _stringBuild = _stringBuild.Replace("{ServerResponseName}", LoadConfig.Server_Response_Name);
                            return false;
                        }
                        else
                        {
                            ReservedSlots.Kicked.Remove(_cInfo.playerId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.PlayerLogin: {0}", e.Message));
            }
            return true;
        }

        private static void PlayerSpawning(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)//Setting player view and profile
        {
            try
            {
                if (_cInfo != null)
                {
                    if (CredentialCheck.IsEnabled && !CredentialCheck.AccCheck(_cInfo))
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 years \"Auto detection has banned you for false credentials. Contact an admin if this is a mistake\"", _cInfo.playerId), (ClientInfo)null);
                        return;
                    }
                    if (CountryBan.IsEnabled && CountryBan.IsCountryBanned(_cInfo))
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 years \"Auto detection has banned your country or region code\"", _cInfo.playerId), (ClientInfo)null);
                        return;
                    }
                    PersistentContainer.Instance.Players[_cInfo.playerId].LastJoined = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.PlayerSpawning: {0}", e.Message));
            }
        }

        private static void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)//Spawning player
        {
            try
            {
                if (_cInfo != null)
                {
                    if (_respawnReason == RespawnType.EnterMultiplayer)//New player spawned
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].PlayerName = _cInfo.playerName;
                        PersistentContainer.Instance.Save();
                        PersistentOperations.SessionTime(_cInfo);
                        Timers.NewPlayerExecTimer(_cInfo);
                    }
                    else if (_respawnReason == RespawnType.JoinMultiplayer)//Old player spawned
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].PlayerName = _cInfo.playerName;
                        PersistentContainer.Instance.Save();
                        PersistentOperations.SessionTime(_cInfo);
                        if (!PersistentContainer.Instance.Players[_cInfo.playerId].OldPlayer)
                        {
                            Timers.NewPlayerExecTimer(_cInfo);
                        }
                        else
                        {
                            if (Hardcore.IsEnabled && !Hardcore.Optional)
                            {
                                string _sql = string.Format("SELECT * FROM Hardcore WHERE steamid = '{0}'", _cInfo.playerId);
                                DataTable _result = SQL.TypeQuery(_sql);
                                if (_result.Rows.Count == 0)
                                {
                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                    if (_player != null)
                                    {
                                        int _deaths = XUiM_Player.GetDeaths(_player);
                                        string _playerName = SQL.EscapeString(_cInfo.playerName);
                                        SQL.FastQuery(string.Format("INSERT INTO Hardcore (steamid, playerName, deaths) VALUES ('{0}', '{1}', {2})", _cInfo.playerId, _playerName, _deaths), null);
                                    }
                                }
                                _result.Dispose();
                            }
                            if (LoginNotice.IsEnabled && LoginNotice.dict.ContainsKey(_cInfo.playerId))
                            {
                                LoginNotice.PlayerNotice(_cInfo);
                            }
                            if (Motd.IsEnabled)
                            {
                                Motd.Send(_cInfo);
                            }
                            if (Bloodmoon.IsEnabled)
                            {
                                Bloodmoon.Exec(_cInfo);
                            }
                            if (AutoShutdown.IsEnabled && AutoShutdown.Alert_On_Login)
                            {
                                AutoShutdown.NextShutdown(_cInfo);
                            }
                            if (Hardcore.IsEnabled)
                            {
                                if (!Hardcore.Optional)
                                {
                                    Hardcore.Alert(_cInfo);
                                }
                                else if (PersistentContainer.Instance.Players[_cInfo.playerId].Hardcore)
                                {
                                    Hardcore.Alert(_cInfo);
                                }
                            }
                            if (BattleLogger.IsEnabled)
                            {
                                BattleLogger.AlertPlayer(_cInfo);
                            }
                            if (PollConsole.IsEnabled)
                            {
                                string _sql = "SELECT pollOpen FROM Polls WHERE pollOpen = 'true'";
                                if (!string.IsNullOrEmpty(_sql))
                                {
                                    DataTable _result = SQL.TypeQuery(_sql);
                                    if (_result.Rows.Count > 0 && !PollConsole.PolledYes.Contains(_cInfo.playerId) && !PollConsole.PolledNo.Contains(_cInfo.playerId))
                                    {
                                        PollConsole.Message(_cInfo);
                                    }
                                    _result.Dispose();
                                }
                            }
                            if (Event.Open)
                            {
                                if (Event.PlayersTeam.ContainsKey(_cInfo.playerId))
                                {
                                    string _sql = string.Format("SELECT eventRespawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                    if (!string.IsNullOrEmpty(_sql))
                                    {
                                        DataTable _result1 = SQL.TypeQuery(_sql);
                                        bool _eventRespawn = false;
                                        if (_result1.Rows.Count > 0)
                                        {
                                            bool.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _eventRespawn);
                                        }
                                        _result1.Dispose();
                                        if (_eventRespawn)
                                        {
                                            Event.Died(_cInfo);
                                        }
                                        else
                                        {
                                            _sql = string.Format("SELECT return, eventSpawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                            DataTable _result2 = SQL.TypeQuery(_sql);
                                            bool _return1 = false, _return2 = false;
                                            if (_result2.Rows.Count > 0)
                                            {
                                                bool.TryParse(_result2.Rows[0].ItemArray.GetValue(0).ToString(), out _return1);
                                                bool.TryParse(_result2.Rows[0].ItemArray.GetValue(1).ToString(), out _return2);
                                            }
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
                                                Event.EventSpawn(_cInfo);
                                            }
                                            else if (_return2)
                                            {
                                                Event.EventSpawn(_cInfo);
                                            }
                                        }
                                    }
                                }
                            }
                            if (ClanManager.IsEnabled)
                            {
                                List<string[]> _clanRequests = PersistentContainer.Instance.Players[_cInfo.playerId].ClanRequestToJoin;
                                if (_clanRequests != null && _clanRequests.Count > 0)
                                {
                                    string[] _request = _clanRequests[0];
                                    string _playerName = _request[1];
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "There is a request to join the group from " + _playerName + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                    else if (_respawnReason == RespawnType.Died)//Player Died
                    {
                        PersistentOperations.SessionTime(_cInfo);
                        if (Bloodmoon.IsEnabled && Bloodmoon.Show_On_Respawn)
                        {
                            Bloodmoon.Exec(_cInfo);
                        }
                        if (BattleLogger.IsEnabled)
                        {
                            BattleLogger.AlertPlayer(_cInfo);
                        }
                        if (Event.Open)
                        {
                            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
                            {
                                string _sql = string.Format("SELECT return, eventSpawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                if (!string.IsNullOrEmpty(_sql))
                                {
                                    DataTable _result1 = SQL.TypeQuery(_sql);
                                    bool _return1 = false, _return2 = false;
                                    if (_result1.Rows.Count > 0)
                                    {
                                        bool.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _return1);
                                        bool.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _return2);
                                    }
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
                                        Event.EventSpawn(_cInfo);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                string _sql = string.Format("SELECT eventRespawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                if (!string.IsNullOrEmpty(_sql))
                                {
                                    DataTable _result = SQL.TypeQuery(_sql);
                                    bool _eventRespawn = false;
                                    if (_result.Rows.Count > 0)
                                    {
                                        bool.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _eventRespawn);
                                    }
                                    _result.Dispose();
                                    if (_eventRespawn)
                                    {
                                        Event.Died(_cInfo);
                                        return;
                                    }
                                    else
                                    {
                                        _sql = string.Format("SELECT return, eventSpawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                        DataTable _result1 = SQL.TypeQuery(_sql);
                                        bool _return1 = false, _return2 = false;
                                        if (_result1.Rows.Count > 0)
                                        {
                                            bool.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _return1);
                                            bool.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _return2);
                                        }
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
                                            Event.EventSpawn(_cInfo);
                                            return;
                                        }
                                        else if (_return2)
                                        {
                                            Event.EventSpawn(_cInfo);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            string _sql = string.Format("SELECT return, eventSpawn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                            if (!string.IsNullOrEmpty(_sql))
                            {
                                DataTable _result1 = SQL.TypeQuery(_sql);
                                bool _return1 = false, _return2 = false;
                                if (_result1.Rows.Count > 0)
                                {
                                    bool.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _return1);
                                    bool.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _return2);
                                }
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
                                    Event.EventSpawn(_cInfo);
                                    return;
                                }
                            }
                        }
                        if (Wallet.IsEnabled)
                        {
                            if (Wallet.Lose_On_Death)
                            {
                                Wallet.ClearWallet(_cInfo);
                            }
                            else if (Wallet.Deaths > 0)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Wallet.Deaths);
                            }
                        }
                        if (Hardcore.IsEnabled)
                        {
                            if (!Hardcore.Optional)
                            {
                                Hardcore.Check(_cInfo);
                            }
                            else if (PersistentContainer.Instance.Players[_cInfo.playerId].Hardcore)
                            {
                                Hardcore.Check(_cInfo);
                            }
                        }
                        if (Zones.IsEnabled && Zones.Victim.ContainsKey(_cInfo.entityId))
                        {
                            string _response = " type {CommandPrivate}{Command50} to teleport back to your death position. There is a time limit.";
                            _response = _response.Replace("{CommandPrivate}", ChatHook.Command_Private);
                            _response = _response.Replace("{Command50}", Zones.Command50);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _response + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            PersistentContainer.Instance.Players[_cInfo.playerId].ZoneDeathTime = DateTime.Now;
                            PersistentContainer.Instance.Save();
                            if (Zones.Forgive.ContainsKey(_cInfo.entityId))
                            {
                                string _response2 = " type {CommandPrivate}{Command55} to release your killer from jail.";
                                _response2 = _response2.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                _response2 = _response2.Replace("{Command55}", Jail.Command55);
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _response2 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else if (_respawnReason == RespawnType.Teleport)
                    {

                    }
                    string _ip = _cInfo.ip;
                    if (_ip.Contains(":"))
                    {
                        _ip = _ip.Split(':').First();
                    }
                    if (!string.IsNullOrEmpty(_ip) && BattleLogger.IsEnabled && Confirm.LogFound && !StopServer.StopServerCountingDown && !StopServer.Shutdown && GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId).PermissionLevel > BattleLogger.Admin_Level)
                    {
                        if (!BattleLogger.Players.ContainsKey(_cInfo.playerId))
                        {
                            BattleLogger.Players.Add(_cInfo.playerId, _cInfo.ip);
                        }
                        else
                        {
                            string _recordedIp;
                            BattleLogger.Players.TryGetValue(_cInfo.playerId, out _recordedIp);
                            if (_recordedIp != _ip)
                            {
                                BattleLogger.Players[_cInfo.playerId] = _ip;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.PlayerSpawnedInWorld: {0}", e.Message));
            }
        }

        private static bool ChatMessage(ClientInfo _cInfo, EChatType _type, int _senderId, string _msg, string _mainName, bool _localizeMain, List<int> _recipientEntityIds)
        {
            return ChatHook.Hook(_cInfo, _type, _senderId, _msg, _mainName, _localizeMain, _recipientEntityIds);
        }

        private static bool GameMessage(ClientInfo _cInfo, EnumGameMessages _type, string _msg, string _mainName, bool _localizeMain, string _secondaryName, bool _localizeSecondary)
        {
            try
            {
                if (_type == EnumGameMessages.EntityWasKilled && _cInfo != null)
                {
                    EntityPlayer _player1 = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player1 != null)
                    {
                        bool _notice = false;
                        if (!string.IsNullOrEmpty(_secondaryName) && _mainName != _secondaryName)
                        {
                            ClientInfo _cInfo2 = ConsoleHelper.ParseParamIdOrName(_secondaryName);
                            if (_cInfo2 != null)
                            {
                                EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                                if (_player2 != null)
                                {
                                    if (KillNotice.IsEnabled && _player2.IsAlive())
                                    {
                                        string _holdingItem = _player2.inventory.holdingItem.Name;
                                        if (!string.IsNullOrEmpty(_holdingItem))
                                        {
                                            ItemValue _itemValue = ItemClass.GetItem(_holdingItem, true);
                                            if (_itemValue.type != ItemValue.None.type)
                                            {
                                                _holdingItem = _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName();
                                                KillNotice.Notice(_cInfo, _cInfo2, _holdingItem);
                                                _notice = true;
                                            }
                                        }
                                    }
                                    if (Zones.IsEnabled)
                                    {
                                        Zones.Check(_cInfo, _cInfo2);
                                    }
                                    if (Bounties.IsEnabled)
                                    {
                                        Bounties.PlayerKilled(_player1, _player2, _cInfo, _cInfo2);
                                    }
                                    if (Wallet.IsEnabled)
                                    {
                                        if (Wallet.PVP && Wallet.Player_Kills > 0)
                                        {
                                            Wallet.AddCoinsToWallet(_cInfo2.playerId, Wallet.Player_Kills);
                                        }
                                        else if (Wallet.Player_Kills > 0)
                                        {
                                            Wallet.SubtractCoinsFromWallet(_cInfo2.playerId, Wallet.Player_Kills);
                                        }
                                    }
                                }
                            }
                        }
                        if (DeathSpot.IsEnabled)
                        {
                            DeathSpot.PlayerKilled(_player1);
                        }
                        if (Event.Open && Event.PlayersTeam.ContainsKey(_cInfo.playerId))
                        {
                            string _sql = string.Format("UPDATE Players SET eventReturn = 'true' WHERE steamid = '{0}'", _cInfo.playerId);
                            SQL.FastQuery(_sql, "Players");
                        }
                        if (_notice)
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.GameMessage: {0}", e.Message));
            }
            return true;
        }

        private static void SavePlayerData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            try
            {
                if (_cInfo != null && _playerDataFile != null)
                {
                    if (HighPingKicker.IsEnabled)
                    {
                        HighPingKicker.Exec(_cInfo);
                    }
                    if (InvalidItems.IsEnabled || InvalidItems.Announce_Invalid_Stack)
                    {
                        InvalidItems.CheckInv(_cInfo, _playerDataFile);
                    }
                    if (DupeLog.IsEnabled)
                    {
                        DupeLog.Exec(_cInfo, _playerDataFile);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.SavePlayerData: {0}", e.Message));
            }
        }

        public static void PlayerDisconnected(ClientInfo _cInfo, bool _bShutdown)
        {
            try
            {
                Log.Out("[SERVERTOOLS] Player detected disconnecting");
                if (_cInfo != null && !string.IsNullOrEmpty(_cInfo.playerId) && _cInfo.entityId != -1)
                {
                    if (BattleLogger.IsEnabled && Confirm.LogFound && !_bShutdown && !StopServer.StopServerCountingDown && !StopServer.Shutdown && BattleLogger.Players.ContainsKey(_cInfo.playerId))
                    {
                        BattleLogger.BattleLog(_cInfo);
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
                    if (Wallet.IsEnabled && Wallet.Session_Bonus > 0)
                    {
                        DateTime _time;
                        if (PersistentOperations.Session.TryGetValue(_cInfo.playerId, out _time))
                        {
                            TimeSpan varTime = DateTime.Now - _time;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (_timepassed > 60)
                            {
                                int _sessionBonus = _timepassed / 60 * Wallet.Session_Bonus;
                                if (_sessionBonus > 0)
                                {
                                    Wallet.AddCoinsToWallet(_cInfo.playerId, _sessionBonus);
                                }
                            }
                            int _timePlayed = PersistentContainer.Instance.Players[_cInfo.playerId].TotalTimePlayed;
                            PersistentContainer.Instance.Players[_cInfo.playerId].TotalTimePlayed = _timePlayed + _timepassed;
                            PersistentContainer.Instance.Save();
                        }
                    }
                    if (PersistentOperations.Session.ContainsKey(_cInfo.playerId))
                    {
                        PersistentOperations.Session.Remove(_cInfo.playerId);
                    }
                    if (Bank.TransferId.ContainsKey(_cInfo.playerId))
                    {
                        Bank.TransferId.Remove(_cInfo.playerId);
                    }
                    if (Zones.Reminder.ContainsKey(_cInfo.entityId))
                    {
                        Zones.Reminder.Remove(_cInfo.entityId);
                    }
                    if (BloodmoonWarrior.WarriorList.Contains(_cInfo.entityId))
                    {
                        BloodmoonWarrior.WarriorList.Remove(_cInfo.entityId);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.PlayerDisconnected: {0}", e.Message));
            }
        }

        private static void EntityKilled(Entity _entity1, Entity _entity2)
        {
            try
            {
                if (_entity1 != null && _entity2 != null && _entity2.IsClientControlled())
                {
                    if (Wallet.IsEnabled && Wallet.Zombie_Kills > 0)
                    {
                        string _tags = _entity1.EntityClass.Tags.ToString();
                        if (_tags.Contains("zombie") || (_tags.Contains("hostile") && _tags.Contains("animal")))
                        {
                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_entity2.entityId);
                            if (_cInfo != null)
                            {
                                Wallet.AddCoinsToWallet(_cInfo.playerId, Wallet.Zombie_Kills);
                            }
                        }
                    }
                    if (BloodmoonWarrior.IsEnabled && BloodmoonWarrior.BloodmoonStarted && BloodmoonWarrior.WarriorList.Contains(_entity2.entityId))
                    {
                        int _killedZ;
                        BloodmoonWarrior.KilledZombies.TryGetValue(_entity2.entityId, out _killedZ);
                        BloodmoonWarrior.KilledZombies[_entity2.entityId] = _killedZ + 1;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.EntityKilled: {0}", e.Message));
            }
        }

        public static void NewPlayerExec1(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    if (_player.IsSpawned() && _player.IsAlive())
                    {
                        if (NewSpawnTele.IsEnabled && NewSpawnTele.New_Spawn_Tele_Position != "0,0,0")
                        {
                            NewSpawnTele.TeleNewSpawn(_cInfo, _player);
                            if (StartingItems.IsEnabled && StartingItems.ItemList.Count > 0)
                            {
                                Timers.NewPlayerStartingItemsTimer(_cInfo);
                            }
                            else
                            {
                                NewPlayerExec3(_cInfo, _player);
                            }
                        }
                        else
                        {
                            NewPlayerExec2(_cInfo);
                        }
                    }
                    else
                    {
                        Timers.NewPlayerExecTimer(_cInfo);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.NewPlayerExec1: {0}", e.Message));
            }
        }

        public static void NewPlayerExec2(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (_player != null)
                {
                    if (_player.IsSpawned() && _player.IsAlive())
                    {
                        if (StartingItems.IsEnabled && StartingItems.ItemList.Count > 0)
                        {
                            StartingItems.SpawnItems(_cInfo);
                        }
                        NewPlayerExec3(_cInfo, _player);
                    }
                    else
                    {
                        Timers.NewPlayerStartingItemsTimer(_cInfo);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.NewPlayerExec2: {0}", e.Message));
            }
        }

        public static void NewPlayerExec3(ClientInfo _cInfo, EntityPlayer _player)
        {
            try
            {
                if (NewPlayer.IsEnabled)
                {
                    NewPlayer.Exec(_cInfo);
                }
                if (Motd.IsEnabled)
                {
                    Motd.Send(_cInfo);
                }
                if (Bloodmoon.IsEnabled)
                {
                    Bloodmoon.Exec(_cInfo);
                }
                if (BattleLogger.IsEnabled)
                {
                    BattleLogger.AlertPlayer(_cInfo);
                }
                if (PollConsole.IsEnabled)
                {
                    string _sql = "SELECT pollOpen FROM Polls WHERE pollOpen = 'true'";
                    if (!string.IsNullOrEmpty(_sql))
                    {
                        DataTable _result = SQL.TypeQuery(_sql);
                        if (_result.Rows.Count > 0 && !PollConsole.PolledYes.Contains(_cInfo.playerId) && !PollConsole.PolledNo.Contains(_cInfo.playerId))
                        {
                            PollConsole.Message(_cInfo);
                        }
                        _result.Dispose();
                    }
                }
                if (Hardcore.IsEnabled && !Hardcore.Optional)
                {
                    string _sql = string.Format("SELECT * FROM Hardcore WHERE steamid = '{0}'", _cInfo.playerId);
                    DataTable _result = SQL.TypeQuery(_sql);
                    if (_result.Rows.Count == 0)
                    {
                        int _deaths = XUiM_Player.GetDeaths(_player);
                        string _playerName = SQL.EscapeString(_cInfo.playerName);
                        SQL.FastQuery(string.Format("INSERT INTO Hardcore (steamid, playerName, deaths) VALUES ('{0}', '{1}', {2})", _cInfo.playerId, _playerName, _deaths), null);
                    }
                    else
                    {
                        SQL.FastQuery(_sql = string.Format("UPDATE Hardcore SET deaths = {0} WHERE steamid = '{1}'", 0, _cInfo.playerId), "Hardcore");
                    }
                    _result.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.NewPlayerExec3: {0}", e.Message));
            }
            PersistentContainer.Instance.Players[_cInfo.playerId].OldPlayer = true;
            PersistentContainer.Instance.Save();
        }
    }
}
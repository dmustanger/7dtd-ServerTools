using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    public class API : IModApi
    {
        public static string GamePath = Directory.GetCurrentDirectory();
        public static string ConfigPath = string.Format("{0}/ServerTools", GamePath);
        public static string InstallPath = "";
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
            OutputLog.Exec();
        }

        private static void GameStartDone()
        {
            try
            {
                LoadProcess.Load(true);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.GameStartDone: {0}", e.Message));
            }
        }

        private static void GameShutdown()
        {
            try
            {
                PersistentOperations.Shutdown_Initiated = true;
                if (WebAPI.IsEnabled && WebAPI.IsRunning)
                {
                    WebAPI.Unload();
                }
                Timers.TimerStop();
                RegionReset.Exec();
                Phrases.Unload();
                CommandList.Unload();
                OutputLog.Shutdown();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.GameShutdown: {0}", e.Message));
            }
        }

        private static bool PlayerLogin(ClientInfo _cInfo, string _message, StringBuilder _stringBuild)//Initiating player login
        {
            try
            {
                if (_cInfo != null && !string.IsNullOrEmpty(_cInfo.playerId))
                {
                    if (!string.IsNullOrEmpty(_cInfo.ip))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Player {0} connected from IP: {1}", _cInfo.playerId, _cInfo.ip));
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Player {0} connected", _cInfo.playerId));
                    }
                    if (Shutdown.NoEntry)
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Server is shutting down. Rejoin when it restarts\"", _cInfo.playerId), null);
                    }
                    if (NewPlayer.Block_During_Bloodmoon && PersistentOperations.IsBloodmoon())
                    {
                        PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_cInfo.playerId);
                        if (_pdf != null)
                        {
                            if (_pdf.totalTimePlayed < 5)
                            {
                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Currently in bloodmoon. Please join when it finishes\"", _cInfo.playerId), null);
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Currently in bloodmoon. Please join when it finishes\"", _cInfo.playerId), null);
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
            if (_cInfo != null)
            {
                if (Credentials.IsEnabled && !Credentials.AccCheck(_cInfo))
                {
                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 years \"Auto detection has banned you for false credentials. Contact an admin if this is a mistake\"", _cInfo.playerId), null);
                    return;
                }
                if (CountryBan.IsEnabled && CountryBan.IsCountryBanned(_cInfo))
                {
                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 years \"Auto detection has banned you for country IP region\"", _cInfo.playerId), null);
                    return;
                }
            }
        }

        private static void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)//Spawning player
        {
            try
            {
                if (_cInfo != null && _cInfo.playerId != null)
                {
                    if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                    {
                        EntityPlayer player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (player != null)
                        {
                            if (_respawnReason == RespawnType.EnterMultiplayer)//New player spawning. Game bug can trigger this incorrectly
                            {
                                PersistentOperations.SessionTime(_cInfo);
                                PersistentContainer.Instance.Players[_cInfo.playerId].PlayerName = _cInfo.playerName;
                                PersistentContainer.Instance.Players[_cInfo.playerId].LastJoined = DateTime.Now;
                                PersistentContainer.DataChange = true;
                                if (player.AttachedToEntity != null)
                                {
                                    player.Detach();
                                }
                                if (player.distanceWalked < 1 && player.totalTimePlayed <= 1 && !PersistentOperations.NewPlayerQue.Contains(_cInfo))
                                {
                                    PersistentOperations.NewPlayerQue.Add(_cInfo);
                                }
                                else
                                {
                                    OldPlayerJoined(_cInfo, player);
                                }
                                if (AutoPartyInvite.IsEnabled)
                                {
                                    AutoPartyInvite.Exec(_cInfo, player);
                                }
                            }
                            else if (_respawnReason == RespawnType.JoinMultiplayer)//Old player spawning
                            {
                                PersistentOperations.SessionTime(_cInfo);
                                PersistentContainer.Instance.Players[_cInfo.playerId].PlayerName = _cInfo.playerName;
                                PersistentContainer.Instance.Players[_cInfo.playerId].LastJoined = DateTime.Now;
                                PersistentContainer.DataChange = true;
                                if (player.AttachedToEntity != null)
                                {
                                    player.Detach();
                                }
                                if (player.distanceWalked < 1 && player.totalTimePlayed <= 1 && !PersistentOperations.NewPlayerQue.Contains(_cInfo))
                                {
                                    PersistentOperations.NewPlayerQue.Add(_cInfo);
                                }
                                else
                                {
                                    OldPlayerJoined(_cInfo, player);
                                }
                                if (AutoPartyInvite.IsEnabled)
                                {
                                    AutoPartyInvite.Exec(_cInfo, player);
                                }
                            }
                            else if (_respawnReason == RespawnType.Died)//Player died, respawning
                            {
                                if (player.AttachedToEntity != null)
                                {
                                    player.Detach();
                                }
                                PlayerDied(_cInfo);
                            }
                            else if (_respawnReason == RespawnType.Teleport)
                            {
                                if (Teleportation.Teleporting.Contains(_cInfo.entityId))
                                {
                                    Teleportation.Teleporting.Remove(_cInfo.entityId);
                                }
                            }
                        }
                        if (ExitCommand.IsEnabled && !ExitCommand.Players.ContainsKey(_cInfo.entityId) && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > ExitCommand.Admin_Level)
                        {
                            ExitCommand.Players.Add(_cInfo.entityId, player.position);
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
            return ChatHook.Hook(_cInfo, _type, _senderId, _msg, _mainName, _recipientEntityIds);
        }

        private static bool GameMessage(ClientInfo _cInfo, EnumGameMessages _type, string _msg, string _mainName, bool _localizeMain, string _secondaryName, bool _localizeSecondary)
        {
            try
            {
                if (_cInfo != null && GameManager.Instance.World != null && _type == EnumGameMessages.EntityWasKilled &&
                    GameManager.Instance.World.Players.dict.Count > 0 && GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                {
                    EntityPlayer player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (player != null)
                    {
                        if (PlayerChecks.FlyEnabled && PlayerChecks.Movement.ContainsKey(_cInfo.entityId))
                        {
                            PlayerChecks.Movement.Remove(_cInfo.entityId);
                        }
                        if (Died.IsEnabled)
                        {
                            Died.PlayerKilled(player);
                        }
                        if (KillNotice.IsEnabled && KillNotice.Zombie_Kills)
                        {
                            KillNotice.ZombieDamage.TryGetValue(player.entityId, out int[] damage);
                            EntityZombie zombie = PersistentOperations.GetZombie(damage[0]);
                            if (zombie != null)
                            {
                                KillNotice.ZombieKilledPlayer(zombie, player, _cInfo, damage[1]);
                            }
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
                    if (InvalidItems.IsEnabled || InvalidItems.Invalid_Stack)
                    {
                        InvalidItems.CheckInv(_cInfo, _playerDataFile);
                    }
                    if (DupeLog.IsEnabled)
                    {
                        DupeLog.Exec(_cInfo, _playerDataFile);
                    }
                    if (LevelUp.IsEnabled)
                    {
                        LevelUp.CheckLevel(_cInfo);
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
                if (_cInfo != null)
                {
                    if (!string.IsNullOrEmpty(_cInfo.playerId) && _cInfo.entityId != -1)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Player {0} disconnected", _cInfo.playerId));
                        if (ExitCommand.IsEnabled && ExitCommand.Players.ContainsKey(_cInfo.entityId) && !_bShutdown)
                        {
                            Timers.ExitWithoutCommand(_cInfo, _cInfo.ip);
                        }
                        if (FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                        {
                            FriendTeleport.Dict.Remove(_cInfo.entityId);
                            FriendTeleport.Dict1.Remove(_cInfo.entityId);
                        }
                        if (FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                        {
                            FriendTeleport.Dict.Remove(_cInfo.entityId);
                        }
                        if (FriendTeleport.Dict1.ContainsKey(_cInfo.entityId))
                        {
                            FriendTeleport.Dict1.Remove(_cInfo.entityId);
                        }
                        if (Wallet.IsEnabled && Wallet.Session_Bonus > 0)
                        {
                            if (PersistentOperations.Session.TryGetValue(_cInfo.playerId, out DateTime _time))
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
                                PersistentContainer.DataChange = true;
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
                        if (Zones.ZonePlayer.ContainsKey(_cInfo.entityId))
                        {
                            Zones.ZonePlayer.Remove(_cInfo.entityId);
                        }
                        if (Zones.Reminder.ContainsKey(_cInfo.entityId))
                        {
                            Zones.Reminder.Remove(_cInfo.entityId);
                        }
                        if (BloodmoonWarrior.WarriorList.Contains(_cInfo.playerId))
                        {
                            BloodmoonWarrior.WarriorList.Remove(_cInfo.playerId);
                        }
                        if (PlayerChecks.Flag.ContainsKey(_cInfo.entityId))
                        {
                            PlayerChecks.Flag.Remove(_cInfo.entityId);
                        }
                        if (Teleportation.Teleporting.Contains(_cInfo.entityId))
                        {
                            Teleportation.Teleporting.Remove(_cInfo.entityId);
                        }
                        if (PlayerChecks.Movement.ContainsKey(_cInfo.entityId))
                        {
                            PlayerChecks.Movement.Remove(_cInfo.entityId);
                        }
                        if (LevelUp.PlayerLevels.ContainsKey(_cInfo.entityId))
                        {
                            LevelUp.PlayerLevels.Remove(_cInfo.entityId);
                        }
                        if (PersistentOperations.NewPlayerQue.Contains(_cInfo))
                        {
                            PersistentOperations.NewPlayerQue.Remove(_cInfo);
                        }
                        if (PersistentOperations.BlockChatCommands.Contains(_cInfo))
                        {
                            PersistentOperations.BlockChatCommands.Remove(_cInfo);
                        }
                        if (KillNotice.ZombieDamage.ContainsKey(_cInfo.entityId))
                        {
                            KillNotice.ZombieDamage.Remove(_cInfo.entityId);
                        }
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Player disconnected");
                    }
                }
                else
                {
                    Log.Out("[SERVERTOOLS] Amorphous blob with no client information disconnected");
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
                if (_entity1 != null && _entity2 != null && !_entity1.IsClientControlled() && _entity2.IsClientControlled())
                {
                    ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_entity2.entityId);
                    if (_cInfo != null)
                    {
                        if (Wallet.IsEnabled && Wallet.Zombie_Kills > 0)
                        {
                            string _tags = _entity1.EntityClass.Tags.ToString();
                            if (_tags.Contains("zombie") || (_tags.Contains("hostile") && _tags.Contains("animal")))
                            {
                                Wallet.AddCoinsToWallet(_cInfo.playerId, Wallet.Zombie_Kills);
                            }
                        }
                        if (BloodmoonWarrior.IsEnabled && BloodmoonWarrior.BloodmoonStarted && BloodmoonWarrior.WarriorList.Contains(_cInfo.playerId))
                        {
                            if (BloodmoonWarrior.KilledZombies.TryGetValue(_cInfo.playerId, out int _killedZ))
                            {
                                BloodmoonWarrior.KilledZombies[_cInfo.playerId] = _killedZ + 1;
                            }
                            else
                            {
                                BloodmoonWarrior.KilledZombies.Add(_cInfo.playerId, 1);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.EntityKilled: {0}", e.Message));
            }
        }

        public static void NewPlayerExec(ClientInfo _cInfo)
        {
            try
            {
                if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player != null)
                    {
                        if (_player.IsSpawned() && _player.IsAlive())
                        {
                            if (NewSpawnTele.IsEnabled && NewSpawnTele.New_Spawn_Tele_Position != "0,0,0")
                            {
                                NewSpawnTele.TeleNewSpawn(_cInfo, _player);
                                if (StartingItems.IsEnabled && StartingItems.Dict.Count > 0)
                                {
                                    Timers.StartingItemsTimer(_cInfo);
                                }
                            }
                            else if (StartingItems.IsEnabled && StartingItems.Dict.Count > 0)
                            {
                                StartingItems.Exec(_cInfo);
                            }
                            ProcessPlayer(_cInfo, _player);
                        }
                        else
                        {
                            PersistentOperations.NewPlayerQue.Add(_cInfo);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.NewPlayerExec: {0}", e.Message));
            }
        }

        public static void ProcessPlayer(ClientInfo _cInfo, EntityPlayer _player)
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
                if (ExitCommand.IsEnabled)
                {
                    ExitCommand.AlertPlayer(_cInfo);
                }
                if (Poll.IsEnabled && PersistentContainer.Instance.PollOpen && !PersistentContainer.Instance.PollVote.ContainsKey(_cInfo.playerId))
                {
                    Poll.Message(_cInfo);
                }
                if (Hardcore.IsEnabled)
                {
                    if (Hardcore.Optional)
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                        {
                            Hardcore.Check(_cInfo, _player);
                        }
                    }
                    else if (!PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                    {
                        string[] _hardcoreStats = { _cInfo.playerName, XUiM_Player.GetDeaths(_player).ToString(), "0" };
                        PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreStats = _hardcoreStats;
                        PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled = true;
                        Hardcore.Check(_cInfo, _player);
                    }
                }
                PersistentContainer.Instance.Players[_cInfo.playerId].OldPlayer = true;
                PersistentContainer.DataChange = true;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.ProcessPlayer: {0}", e.Message));
            }
        }

        public static void OldPlayerJoined(ClientInfo _cInfo, EntityPlayer _player)
        {
            try
            {
                if (Hardcore.IsEnabled)
                {
                    if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                    {
                        if (Hardcore.Optional)
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                            {
                                Hardcore.Check(_cInfo, _player);
                            }
                        }
                        else if (!PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                        {
                            string[] _hardcoreStats = { _cInfo.playerName, XUiM_Player.GetDeaths(_player).ToString(), "0" };
                            PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreStats = _hardcoreStats;
                            PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled = true;
                            PersistentContainer.DataChange = true;
                            Hardcore.Check(_cInfo, _player);
                        }
                    }
                }
                if (LoginNotice.IsEnabled && LoginNotice.Dict.ContainsKey(_cInfo.playerId))
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
                if (Shutdown.IsEnabled && Shutdown.Alert_On_Login)
                {
                    Shutdown.NextShutdown(_cInfo);
                }
                if (ExitCommand.IsEnabled)
                {
                    ExitCommand.AlertPlayer(_cInfo);
                }
                if (Poll.IsEnabled && PersistentContainer.Instance.PollOpen && !PersistentContainer.Instance.PollVote.ContainsKey(_cInfo.playerId))
                {
                    Poll.Message(_cInfo);
                }
                if (ClanManager.IsEnabled)
                {
                    Dictionary<string, string> _clanRequests = PersistentContainer.Instance.Players[_cInfo.playerId].ClanRequestToJoin;
                    if (_clanRequests != null && _clanRequests.Count > 0)
                    {
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "New clan requests from:[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        foreach (var _request in _clanRequests)
                        {
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _request.Value + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                if (Event.Open && Event.Teams.ContainsKey(_cInfo.playerId) && PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn)
                {
                    Event.Spawn(_cInfo);
                }
                else if (PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn = false;
                    PersistentContainer.DataChange = true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.OldPlayerJoined: {0}", e.Message));
            }
        }

        public static void PlayerDied(ClientInfo _cInfo)
        {
            if (Bloodmoon.IsEnabled && Bloodmoon.Show_On_Respawn)
            {
                Bloodmoon.Exec(_cInfo);
            }
            if (Event.Open && Event.Teams.ContainsKey(_cInfo.playerId))
            {
                Event.Respawn(_cInfo);
            }
            if (PersistentContainer.Instance.Players[_cInfo.playerId].EventOver)
            {
                Event.EventOver(_cInfo);
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
            if (Hardcore.IsEnabled && PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    Hardcore.Check(_cInfo, _player);
                }
            }
            if (Zones.ZonePlayer.ContainsKey(_cInfo.entityId))
            {
                Zones.ZonePlayer.Remove(_cInfo.entityId);
            }
        }
    }
}
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

            //ModEvents.EntityKilled.RegisterHandler(EntityKilled);
        }

        private void GameAwake()
        {
            OutputLog.Exec();
        }

        private static void GameStartDone()
        {
            try
            {
                LoadProcess.Load();
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
                Timers.CoreTimerStop();
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
                    EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
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
                            //Log.Out(string.Format("[SERVERTOOLS] Test 1"));
                            if (AutoPartyInvite.IsEnabled)
                            {
                                AutoPartyInvite.Exec(_cInfo, player);
                            }
                            //Log.Out(string.Format("[SERVERTOOLS] Test 2"));
                        }
                        else if (_respawnReason == RespawnType.Died)//Player died, respawning
                        {
                            if (player.AttachedToEntity != null)
                            {
                                player.Detach();
                            }
                        }
                        else if (_respawnReason == RespawnType.Teleport)
                        {
                            if (Teleportation.Teleporting.Contains(_cInfo.entityId))
                            {
                                Teleportation.Teleporting.Remove(_cInfo.entityId);
                            }
                        }
                        if (PlayerChecks.TwoSecondMovement.ContainsKey(_cInfo.entityId))
                        {
                            PlayerChecks.TwoSecondMovement.Remove(_cInfo.entityId);
                        }
                        if (FlyingDetector.Flags.ContainsKey(_cInfo.entityId))
                        {
                            FlyingDetector.Flags.Remove(_cInfo.entityId);
                        }
                        if (SpeedDetector.Flags.ContainsKey(_cInfo.entityId))
                        {
                            SpeedDetector.Flags.Remove(_cInfo.entityId);
                        }
                    }
                    if (ExitCommand.IsEnabled && !ExitCommand.Players.ContainsKey(_cInfo.entityId) && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > ExitCommand.Admin_Level)
                    {
                        ExitCommand.Players.Add(_cInfo.entityId, player.position);
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
                if (_cInfo != null && _type == EnumGameMessages.EntityWasKilled)
                {
                    EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                    if (player != null)
                    {
                        if (KillNotice.IsEnabled && KillNotice.Zombie_Kills && string.IsNullOrEmpty(_secondaryName))
                        {
                            if (KillNotice.ZombieDamage.ContainsKey(player.entityId))
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
                            if (PersistentOperations.Session.TryGetValue(_cInfo.playerId, out DateTime time))
                            {
                                TimeSpan varTime = DateTime.Now - time;
                                double fractionalMinutes = varTime.TotalMinutes;
                                int timepassed = (int)fractionalMinutes;
                                if (timepassed > 60)
                                {
                                    int sessionBonus = timepassed / 60 * Wallet.Session_Bonus;
                                    if (sessionBonus > 0)
                                    {
                                        Wallet.AddCurrency(_cInfo.playerId, sessionBonus);
                                    }
                                }
                                int timePlayed = PersistentContainer.Instance.Players[_cInfo.playerId].TotalTimePlayed;
                                PersistentContainer.Instance.Players[_cInfo.playerId].TotalTimePlayed = timePlayed + timepassed;
                                PersistentContainer.DataChange = true;
                            }
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
                            BloodmoonWarrior.KilledZombies.Remove(_cInfo.playerId);
                        }
                        if (Teleportation.Teleporting.Contains(_cInfo.entityId))
                        {
                            Teleportation.Teleporting.Remove(_cInfo.entityId);
                        }
                        if (LevelUp.PlayerLevels.ContainsKey(_cInfo.entityId))
                        {
                            LevelUp.PlayerLevels.Remove(_cInfo.entityId);
                        }
                        if (KillNotice.ZombieDamage.ContainsKey(_cInfo.entityId))
                        {
                            KillNotice.ZombieDamage.Remove(_cInfo.entityId);
                        }
                        if (InfiniteAmmo.Dict.ContainsKey(_cInfo.entityId))
                        {
                            InfiniteAmmo.Dict.Remove(_cInfo.entityId);
                        }
                        if (PersistentOperations.NewPlayerQue.Contains(_cInfo))
                        {
                            PersistentOperations.NewPlayerQue.Remove(_cInfo);
                        }
                        if (PersistentOperations.BlockChatCommands.Contains(_cInfo))
                        {
                            PersistentOperations.BlockChatCommands.Remove(_cInfo);
                        }
                        if (PersistentOperations.Session.ContainsKey(_cInfo.playerId))
                        {
                            PersistentOperations.Session.Remove(_cInfo.playerId);
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

        //private static void EntityKilled(Entity _entity1, Entity _entity2)
        //{
        //    try
        //    {
        //        
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Out(string.Format("[SERVERTOOLS] Error in API.EntityKilled: {0}", e.Message));
        //    }
        //}

        public static void NewPlayerExec(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (player != null)
                {
                    if (player.IsSpawned() && player.IsAlive())
                    {
                        if (NewSpawnTele.IsEnabled && NewSpawnTele.New_Spawn_Tele_Position != "0,0,0")
                        {
                            NewSpawnTele.TeleNewSpawn(_cInfo, player);
                            if (StartingItems.IsEnabled && StartingItems.Dict.Count > 0)
                            {
                                Timers.StartingItemsTimer(_cInfo);
                            }
                        }
                        else if (StartingItems.IsEnabled && StartingItems.Dict.Count > 0)
                        {
                            StartingItems.Exec(_cInfo);
                        }
                        ProcessPlayer(_cInfo, player);
                    }
                    else
                    {
                        PersistentOperations.NewPlayerQue.Add(_cInfo);
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
                        string[] hardcoreStats = { _cInfo.playerName, XUiM_Player.GetDeaths(_player).ToString(), "0" };
                        PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreStats = hardcoreStats;
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
                if (Hardcore.IsEnabled && PersistentContainer.Instance.Players[_cInfo.playerId] != null)
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
                        string[] hardcoreStats = { _cInfo.playerName, XUiM_Player.GetDeaths(_player).ToString(), "0" };
                        PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreStats = hardcoreStats;
                        PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled = true;
                        PersistentContainer.DataChange = true;
                        Hardcore.Check(_cInfo, _player);
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
                if (ClanManager.IsEnabled && PersistentContainer.Instance.Players[_cInfo.playerId] != null)
                {
                    Dictionary<string, string> clanRequests = PersistentContainer.Instance.Players[_cInfo.playerId].ClanRequestToJoin;
                    if (clanRequests != null && clanRequests.Count > 0)
                    {
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "New clan requests from:[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        foreach (var request in clanRequests)
                        {
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + request.Value + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                if (Event.Open && Event.Teams.ContainsKey(_cInfo.playerId) && PersistentContainer.Instance.Players[_cInfo.playerId] != null && PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn)
                {
                    Event.Spawn(_cInfo);
                }
                else if (PersistentContainer.Instance.Players[_cInfo.playerId] != null && PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn = false;
                    PersistentContainer.DataChange = true;
                }
                if (PersistentContainer.Instance.Players[_cInfo.playerId] != null && PersistentContainer.Instance.Players[_cInfo.playerId].PlayerWallet > 0)
                {
                    Wallet.AddCurrency(_cInfo.playerId, PersistentContainer.Instance.Players[_cInfo.playerId].PlayerWallet);
                    PersistentContainer.Instance.Players[_cInfo.playerId].PlayerWallet = 0;
                    PersistentContainer.DataChange = true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.OldPlayerJoined: {0}", e.Message));
            }
        }

        public static void PlayerDied(EntityAlive __instance)
        {
            ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(__instance.entityId);
            if (cInfo != null)
            {
                if (__instance.AttachedToEntity != null)
                {
                    __instance.Detach();
                }
                if (Died.IsEnabled)
                {
                    Died.PlayerKilled(__instance);
                }
                if (ProcessDamage.lastEntityKilled == cInfo.entityId)
                {
                    ProcessDamage.lastEntityKilled = 0;
                }
                if (Bloodmoon.IsEnabled && Bloodmoon.Show_On_Respawn)
                {
                    Bloodmoon.Exec(cInfo);
                }
                if (Event.Open && Event.Teams.ContainsKey(cInfo.playerId))
                {
                    Event.Respawn(cInfo);
                }
                if (PersistentContainer.Instance.Players[cInfo.playerId].EventOver)
                {
                    Event.EventOver(cInfo);
                }
                if (Hardcore.IsEnabled && PersistentContainer.Instance.Players[cInfo.playerId].HardcoreEnabled)
                {
                    Hardcore.Check(cInfo, (EntityPlayer)__instance);
                }
                if (Zones.ZonePlayer.ContainsKey(cInfo.entityId))
                {
                    Zones.ZonePlayer.Remove(cInfo.entityId);
                }
                if (PlayerChecks.TwoSecondMovement.ContainsKey(cInfo.entityId))
                {
                    PlayerChecks.TwoSecondMovement.Remove(cInfo.entityId);
                }
                if (FlyingDetector.Flags.ContainsKey(cInfo.entityId))
                {
                    FlyingDetector.Flags.Remove(cInfo.entityId);
                }
                if (SpeedDetector.Flags.ContainsKey(cInfo.entityId))
                {
                    SpeedDetector.Flags.Remove(cInfo.entityId);
                }
            }
        }
    }
}
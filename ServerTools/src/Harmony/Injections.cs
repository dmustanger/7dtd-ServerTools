using ServerTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Injections
{

    public static void PlayerLoginRPC_Prefix(ClientInfo _cInfo, string _playerName, ValueTuple<PlatformUserIdentifierAbs, string> _platformUserAndToken, ValueTuple<PlatformUserIdentifierAbs, string> _crossplatformUserAndToken, out bool __state)
    {
        __state = false;
        try
        {
            if (_platformUserAndToken.Item1 != null && _crossplatformUserAndToken.Item1 != null)
            {
                if (ReservedSlots.IsEnabled)
                {
                    int maxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
                    if (ConnectionManager.Instance.ClientCount() > maxPlayers && ReservedSlots.FullServer(_cInfo, _platformUserAndToken.Item1, _crossplatformUserAndToken.Item1))
                    {
                        GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, maxPlayers + 1);
                        __state = true;
                        return;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.PlayerLoginRPC_Prefix: {0}", e.Message));
        }
    }

    public static void PlayerLoginRPC_Postfix(bool __state)
    {
        try
        {
            if (__state)
            {
                int maxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
                GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, maxPlayers - 1);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.PlayerLoginRPC_Postfix: {0}", e.Message));
        }
    }

    public static bool GameManager_ChangeBlocks_Prefix(GameManager __instance, PlatformUserIdentifierAbs persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
    {
        try
        {
            return BlockChange.ProcessBlockChange(__instance, persistentPlayerId, _blocksToChange);
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_ChangeBlocks_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static void ServerConsoleCommand_Postfix(ClientInfo _cInfo, string _cmd)
    {
        try
        {
            ConsoleCommandLog.Exec(_cInfo, _cmd);
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ServerConsoleCommand_Postfix: {0}", e.Message));
        }
    }

    public static void AddFallingBlock_Prefix(World __instance, Vector3i _blockPos)
    {
        try
        {
            if (FallingBlocks.IsEnabled && _blockPos != null)
            {
                BlockValue blockValue = __instance.GetBlock(_blockPos);
                if (!(blockValue.Block is BlockSleepingBag) && !(blockValue.Block is BlockPlant) && !(blockValue.Block is BlockDoor)
                    && !(blockValue.Block is BlockCropsGrown) && !(blockValue.Block is BlockBackpack) && !(blockValue.Block is BlockDrawBridge)
                    && !blockValue.Block.IsTerrainDecoration && !blockValue.Block.GetBlockName().ToLower().Contains("fence"))
                {
                    GameManager.Instance.World.SetBlockRPC(_blockPos, BlockValue.Air);
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.AddFallingBlock_Prefix: {0}", e.Message));
        }
    }

    public static void AddFallingBlocks_Prefix(World __instance, IList<Vector3i> _list)
    {
        try
        {
            if (FallingBlocks.IsEnabled && _list != null)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    BlockValue blockValue = __instance.GetBlock(_list[i]);
                    if (!(blockValue.Block is BlockSleepingBag) && !(blockValue.Block is BlockPlant) && !(blockValue.Block is BlockDoor)
                        && !(blockValue.Block is BlockCropsGrown) && !(blockValue.Block is BlockBackpack) && !(blockValue.Block is BlockDrawBridge)
                        && !blockValue.Block.IsTerrainDecoration && !blockValue.Block.GetBlockName().ToLower().Contains("fence"))
                    {
                        GameManager.Instance.World.SetBlockRPC(_list[i], BlockValue.Air);
                    }
                }
                if (_list.Count > FallingBlocks.Max_Blocks && FallingBlocks.OutputLog)
                {
                    EntityPlayer closestPlayer = __instance.GetClosestPlayer(_list[0].x, _list[0].y, _list[0].z, -1, 75);
                    if (closestPlayer != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' falling blocks around '{1}'. The closest player entity id was '{2}' named '{3}' @ '{4}'", _list.Count, _list[0], closestPlayer.entityId, closestPlayer.EntityName, closestPlayer.position));
                    }
                    else
                    {
                        closestPlayer = __instance.GetClosestPlayer(_list[_list.Count - 1].x, _list[_list.Count - 1].y, _list[_list.Count - 1].z, -1, 75);
                        if (closestPlayer != null)
                        {

                            Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' falling blocks around '{1}'. The closest player entity id was '{2}' named '{3}' @ '{4}'", _list.Count, _list[_list.Count - 1], closestPlayer.entityId, closestPlayer.EntityName, closestPlayer.position));
                        }
                        else
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' falling blocks around '{1}'. No players were located near by", _list.Count, _list[0]));
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.AddFallingBlocks_Prefix: {0}", e.Message));
        }
    }

    public static void ChatMessageServer_Postfix(ClientInfo _cInfo, EChatType _chatType, string _msg, string _mainName)
    {
        try
        {
            if (DiscordBot.IsEnabled && DiscordBot.Webhook != "" && DiscordBot.Webhook.StartsWith("https://discord.com/api/webhooks") &&
                _chatType == EChatType.Global && !string.IsNullOrWhiteSpace(_mainName) && !_mainName.Contains(DiscordBot.Prefix))
            {
                if (_msg.Contains("[") && _msg.Contains("]"))
                {
                    _msg = Regex.Replace(_msg, @"\[.*?\]", "");
                }
                if (!PersistentOperations.InvalidPrefix.Contains(_msg[0]))
                {
                    if (_mainName.Contains("[") && _mainName.Contains("]"))
                    {
                        _mainName = Regex.Replace(_mainName, @"\[.*?\]", "");
                    }
                    if (_cInfo != null)
                    {
                        if (DiscordBot.LastEntry != _msg)
                        {
                            DiscordBot.LastPlayer = _cInfo.PlatformId.ToString();
                            DiscordBot.LastEntry = _msg;
                            DiscordBot.Queue.Add("[Game] **" + _mainName + "** : " + DiscordBot.LastEntry);
                        }
                        else if (DiscordBot.LastPlayer != _cInfo.PlatformId.ToString())
                        {
                            DiscordBot.LastPlayer = _cInfo.PlatformId.ToString();
                            DiscordBot.LastEntry = _msg;
                            DiscordBot.Queue.Add("[Game] **" + _mainName + "** : " + DiscordBot.LastEntry);
                        }
                    }
                    else if (DiscordBot.LastEntry != _msg)
                    {
                        DiscordBot.LastPlayer = "-1";
                        DiscordBot.LastEntry = _msg;
                        DiscordBot.Queue.Add("[Game] **" + _mainName + "** : " + DiscordBot.LastEntry);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ChatMessageServer_Postfix: {0}", e.Message));
        }
    }

    public static void GameManager_Cleanup_Finalizer()
    {
        try
        {
            Log.Out("[SERVERTOOLS] SHUTDOWN");
            Process process = Process.GetCurrentProcess();
            if (process != null)
            {
                process.Kill();
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_Cleanup_Finalizer: {0}", e.Message));
        }
    }

    public static bool GameManager_CollectEntityServer_Prefix(int _entityId)
    {
        try
        {
            if (PersistentOperations.No_Vehicle_Pickup)
            {
                Entity _entity = PersistentOperations.GetEntity(_entityId);
                if (_entity != null && _entity is EntityVehicle)
                {
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_CollectEntityServer_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool GameManager_OpenTileEntityAllowed_Prefix(ref bool __result, ref bool __state, int _entityIdThatOpenedIt, TileEntity _te)
    {
        try
        {
            ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(_entityIdThatOpenedIt);
            if (cInfo != null)
            {
                if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > 0 &&
                    GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > 0)
                {
                    if (DroppedBagProtection.IsEnabled)
                    {
                        if (_te is TileEntityLootContainer)
                        {
                            TileEntityLootContainer lootContainer = _te as TileEntityLootContainer;
                            if (lootContainer.bPlayerBackpack)
                            {
                                if (!DroppedBagProtection.IsAllowed(_entityIdThatOpenedIt, lootContainer))
                                {
                                    Phrases.Dict.TryGetValue("DroppedBagProtection1", out string phrase);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    __result = false;
                                    return false;
                                }
                            }
                        }
                    }
                    if (Shutdown.UI_Locked)
                    {
                        if (_te is TileEntityLootContainer)
                        {
                            TileEntityLootContainer lootContainer = _te as TileEntityLootContainer;
                            if (lootContainer.bPlayerBackpack)
                            {
                                return true;
                            }
                        }
                        if (_te is TileEntityWorkstation || _te is TileEntityLootContainer || _te is TileEntitySecureLootContainer
                        || _te is TileEntityVendingMachine || _te is TileEntityTrader)
                        {
                            if (_te is TileEntityTrader)
                            {
                                __state = true;
                            }
                            Phrases.Dict.TryGetValue("Shutdown3", out string phrase);
                            ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            __result = false;
                            return false;
                        }
                    }
                    if (WorkstationLock.IsEnabled && _te is TileEntityWorkstation)
                    {
                        EntityPlayer entityPlayer = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                        if (entityPlayer != null)
                        {
                            EnumLandClaimOwner owner = PersistentOperations.ClaimedByWho(cInfo.CrossplatformId, new Vector3i(entityPlayer.position));
                            if (owner != EnumLandClaimOwner.Self && owner != EnumLandClaimOwner.Ally && !PersistentOperations.ClaimedByNone(new Vector3i(entityPlayer.position)))
                            {
                                Phrases.Dict.TryGetValue("WorkstationLock1", out string phrase);
                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                __result = false;
                                return false;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_OpenTileEntityAllowed_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static void GameManager_OpenTileEntityAllowed_Postfix(bool __state, int _entityIdThatOpenedIt, TileEntity _te)
    {
        try
        {
            if (__state)
            {
                ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(_entityIdThatOpenedIt);
                if (cInfo != null)
                {
                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close trader", true));
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_OpenTileEntityAllowed_Postfix: {0}", e.Message));
        }
    }

    public static bool EntityAlive_OnEntityDeath_Prefix(EntityAlive __instance)
    {
        try
        {
            if (ReservedSlots.Bonus_Exp && __instance is EntityZombie)
            {
                EntityAlive entityAlive = __instance.GetAttackTarget();
                if (entityAlive != null && entityAlive is EntityPlayer)
                {
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityAlive.entityId);
                    if (cInfo != null && ReservedSlots.IsEnabled && (ReservedSlots.Dict.ContainsKey(cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(cInfo.CrossplatformId.CombinedString)))
                    {

                        int experience = EntityClass.list[__instance.entityClass].ExperienceValue;
                        experience = (int)experience / 4;
                        NetPackageEntityAddExpClient package = NetPackageManager.GetPackage<NetPackageEntityAddExpClient>().Setup(entityAlive.entityId, experience, Progression.XPTypes.Kill);
                        ConnectionManager.Instance.SendPackage(package, false, entityAlive.entityId, -1, -1, -1);
                        Log.Out(string.Format("[SERVERTOOLS] Added bonus experience of '{0}' to reserved player '{1}' '{2}' named '{3}'", experience, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.EntityAlive_OnEntityDeath_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static void ChunkCluster_AddChunkSync_Postfix(Chunk _chunk)
    {
        try
        {
            if (ProtectedZones.IsEnabled && _chunk != null)
            {
                Vector3i vector = _chunk.ChunkPos;
                string vectorString = _chunk.ChunkPos.ToString();
                if (ProtectedZones.AddProtection.Contains(vectorString))
                {
                    Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(vector);
                    if (chunk != null)
                    {
                        ProtectedZones.Add(chunk, vector, vectorString);
                    }
                }
                else if (ProtectedZones.RemoveProtection.Contains(vectorString))
                {
                    Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(vector);
                    if (chunk != null)
                    {
                        ProtectedZones.Remove(chunk, vector, vectorString);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ChunkCluster_AddChunkSync_Postfix: {0}", e.Message));
        }
    }

    public static void World_SpawnEntityInWorld_Postfix(Entity _entity)
    {
        try
        {
            if (DroppedBagProtection.IsEnabled && _entity != null && _entity is EntityBackpack)
            {
                List<ClientInfo> clientList = PersistentOperations.ClientList();
                if (clientList != null)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo = clientList[i];
                        if (cInfo.latestPlayerData != null && cInfo.latestPlayerData.droppedBackpackPosition != null && cInfo.latestPlayerData.droppedBackpackPosition == new Vector3i(_entity.position))
                        {
                            if (!DroppedBagProtection.Backpacks.ContainsKey(_entity.entityId))
                            {
                                DroppedBagProtection.Backpacks.Add(_entity.entityId, cInfo.entityId);
                                PersistentContainer.Instance.Backpacks.Add(_entity.entityId, cInfo.entityId);
                            }
                            else
                            {
                                DroppedBagProtection.Backpacks[_entity.entityId] = cInfo.entityId;
                                PersistentContainer.Instance.Backpacks[_entity.entityId] = cInfo.entityId;
                            }
                            PersistentContainer.DataChange = true;
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.World_SpawnEntityInWorld_Postfix: {0}", e.Message));
        }
    }

    public static void GameManager_ItemReloadServer_Postfix(int _entityId)
    {
        try
        {
            if (InfiniteAmmo.IsEnabled && InfiniteAmmo.Dict.ContainsKey(_entityId))
            {
                InfiniteAmmo.Dict.Remove(_entityId);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_ItemReloadServer_Postfix: {0}", e.Message));
        }
    }

    public static void GameManager_PlayerSpawnedInWorld_Postfix(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos, int _entityId)
    {
        try
        {
            if (SpeedDetector.Flags.ContainsKey(_cInfo.entityId))
            {
                SpeedDetector.Flags.Remove(_cInfo.entityId);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.GameManager_PlayerSpawnedInWorld_Postfix: {0}", e.Message));
        }
    }

    public static void EntityAlive_SetDead_Postfix(EntityAlive __instance)
    {
        try
        {
            if (__instance is EntityPlayer)
            {
                API.PlayerDied(__instance);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.EntityAlive_SetDead_Postfix: {0}", e.Message));
        }
    }

    public static void NetPackagePlayerInventory_ProcessPackage_Postfix(NetPackagePlayerInventory __instance)
    {
        try
        {
            if (__instance.Sender != null && Wallet.UpdateRequired.ContainsKey(__instance.Sender.entityId))
            {
                ClientInfo cInfo = __instance.Sender;
                Wallet.UpdateRequired.TryGetValue(cInfo.entityId, out int value);
                Wallet.UpdateRequired.Remove(cInfo.entityId);
                Wallet.AddCurrency(cInfo.CrossplatformId.CombinedString, value);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackagePlayerInventory_ProcessPackage_Postfix: {0}", e.Message));
        }
    }

    public static bool ClientInfoCollection_GetForNameOrId_Prefix(ref ClientInfo __result, string _nameOrId)
    {
        try
        {
            ClientInfo clientInfo = null;
            int entityId;
            if (int.TryParse(_nameOrId, out entityId))
            {
                clientInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                if (clientInfo != null)
                {
                    __result = clientInfo;
                    return false;
                }
            }
            clientInfo = PersistentOperations.GetClientInfoFromName(_nameOrId);
            if (clientInfo != null)
            {
                __result = clientInfo;
                return false;
            }
            if (_nameOrId.Contains("Local_") || _nameOrId.Contains("EOS_") || _nameOrId.Contains("Steam_") || _nameOrId.Contains("XBL_") || _nameOrId.Contains("PSN_") || _nameOrId.Contains("EGS_"))
            {
                PlatformUserIdentifierAbs userIdentifier;
                if (PlatformUserIdentifierAbs.TryFromCombinedString(_nameOrId, out userIdentifier))
                {
                    clientInfo = PersistentOperations.GetClientInfoFromUId(userIdentifier);
                    if (clientInfo != null)
                    {
                        __result = clientInfo;
                        return false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ClientInfoCollection_GetForNameOrId_Prefix: {0}", e.Message));
        }
        return false;
    }

    public static bool NetPackagePlayerStats_ProcessPackage_Prefix(NetPackagePlayerStats __instance)
    {
        if (PersistentOperations.Net_Package_Detector && __instance.Sender != null)
        {
            if (!PlayerStatsPackage.IsValid(__instance))
            {
                return false;
            }
        }
        return true;
    }

    public static bool NetPackageEntityAddScoreClient_ProcessPackage_Prefix(NetPackageEntityAddScoreClient __instance)
    {
        if (PersistentOperations.Net_Package_Detector && __instance.Sender != null)
        {
            if (!EntityAddScoreClientPackage.IsValid(__instance))
            {
                return false;
            }
        }
        return true;
    }

    public static bool NetPackageChat_ProcessPackage_Prefix(NetPackageChat __instance)
    {
        if (PersistentOperations.Net_Package_Detector && __instance.Sender != null)
        {
            if (!ChatPackage.IsValid(__instance))
            {
                return false;
            }
        }
        return true;
    }

    public static bool NetPackageEntityPosAndRot_ProcessPackage_Prefix(NetPackageEntityPosAndRot __instance)
    {
        if (PersistentOperations.Net_Package_Detector && __instance.Sender != null)
        {
            if (!EntityPosAndRotPackage.IsValid(__instance))
            {
                return false;
            }
        }
        return true;
    }

    public static bool NetPackagePlayerData_ProcessPackage_Prefix(NetPackagePlayerData __instance)
    {
        if (PersistentOperations.Net_Package_Detector && __instance.Sender != null)
        {
            if (!PlayerDataPackage.IsValid(__instance))
            {
                return false;
            }
        }
        return true;
    }

    public static void NetPackageDamageEntity_ProcessPackage_Prefix(ref NetPackageDamageEntity __instance)
    {
        if (__instance.Sender != null && ProcessDamage.Exec(__instance))
        {
            if (ProcessDamage.bFatal(__instance))
            {
                ProcessDamage.bFatal(__instance) = false;
            }
            ProcessDamage.strength(__instance) = 0;
        }
    }
}
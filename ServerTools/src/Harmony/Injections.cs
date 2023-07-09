using ServerTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class Injections
{

    public static void PlayerLoginRPC_Prefix(ClientInfo _cInfo, ValueTuple<PlatformUserIdentifierAbs, string> _platformUserAndToken, ValueTuple<PlatformUserIdentifierAbs, string> _crossplatformUserAndToken, out bool __state)
    {
        __state = false;
        try
        {
            if (GameManager.Instance == null || GameManager.Instance.World == null)
            {
                return;
            }
            if (_platformUserAndToken.Item1 == null || _crossplatformUserAndToken.Item1 == null)
            {
                return;
            }
            if (!ReservedSlots.IsEnabled)
            {
                return;
            }
            int maxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
            if (ConnectionManager.Instance.ClientCount() > maxPlayers && ReservedSlots.FullServer(_cInfo, _platformUserAndToken.Item1, _crossplatformUserAndToken.Item1))
            {
                GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, maxPlayers + 1);
                __state = true;
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

    public static void NetPackageSetBlock_ProcessPackage_Prefix(GameManager _callbacks, List<BlockChangeInfo> ___blockChanges, int ___localPlayerThatChanged)
    {
        try
        {
            BlockChange.ProcessBlockChange(_callbacks, ___blockChanges, ___localPlayerThatChanged);
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackageSetBlock_ProcessPackage_Prefix: {0}", e.Message));
        }
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
                if (blockValue.ischild || blockValue.Block.StabilityIgnore || blockValue.isair)
                {
                    return;
                }
                else
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
                    if (blockValue.ischild || blockValue.Block.StabilityIgnore || blockValue.isair)
                    {
                        return;
                    }
                    else
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

    public static void GameManager_Cleanup_Finalizer()
    {
        try
        {
            Log.Out(string.Format("[SERVERTOOLS] SHUTDOWN"));
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
            if (GeneralOperations.No_Vehicle_Pickup)
            {
                Entity entity = GeneralOperations.GetEntity(_entityId);
                if (entity != null && entity is EntityVehicle)
                {
                    if (GeneralOperations.Allow_Bicycle && entity is EntityBicycle)
                    {
                        return true;
                    }
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

    public static bool GameManager_ExplosionServer_Prefix(int _entityId)
    {
        Entity entity = GeneralOperations.GetEntity(_entityId);
        if ((entity == null || !entity.IsSpawned()) && _entityId != -1)
        {
            return false;
        }
        return true;
    }

    public static bool GameManager_OpenTileEntityAllowed_Prefix(ref bool __result, ref bool __state, int _entityIdThatOpenedIt, ref TileEntity _te)
    {
        try
        {
            ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(_entityIdThatOpenedIt);
            if (cInfo != null)
            {
                if (GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) > 0 &&
                    GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) > 0)
                {
                    if (DroppedBagProtection.IsEnabled && _te is TileEntityLootContainer)
                    {
                        TileEntityLootContainer lootContainer = _te as TileEntityLootContainer;
                        if (lootContainer.bPlayerBackpack && !DroppedBagProtection.IsAllowed(_entityIdThatOpenedIt, lootContainer))
                        {
                            Phrases.Dict.TryGetValue("DroppedBagProtection1", out string phrase);
                            ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            __result = false;
                            return false;
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
                        EntityPlayer entityPlayer = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                        if (entityPlayer != null)
                        {
                            EnumLandClaimOwner owner = GeneralOperations.ClaimedByWho(cInfo.CrossplatformId, new Vector3i(entityPlayer.position));
                            if (owner != EnumLandClaimOwner.Self && owner != EnumLandClaimOwner.Ally && !GeneralOperations.ClaimedByNone(new Vector3i(entityPlayer.position)))
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

    public static void GameManager_OpenTileEntityAllowed_Postfix(bool __state, int _entityIdThatOpenedIt)
    {
        try
        {
            if (__state)
            {
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(_entityIdThatOpenedIt);
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

    public static void GameManager_PlayerSpawnedInWorld_Postfix(ClientInfo _cInfo)
    {
        if (SpeedDetector.Flags.ContainsKey(_cInfo.entityId))
        {
            SpeedDetector.Flags.Remove(_cInfo.entityId);
        }
    }

    public static void EntityAlive_SetDead_Prefix(EntityAlive __instance)
    {
        try
        {
            if (__instance is EntityPlayer)
            {
                API.PlayerDied(__instance);
                if (Confetti.IsEnabled && Confetti.Player && __instance.IsAlive())
                {
                    Confetti.Exec(__instance);
                }
                if (!Hardcore.IsEnabled)
                {
                    return;
                }
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(__instance.entityId);
                if (cInfo == null)
                {
                    return;
                }
                if (Hardcore.Optional)
                {
                    if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].HardcoreEnabled)
                    {
                        Hardcore.Check(cInfo, __instance as EntityPlayer, true);
                    }
                }
                else
                {
                    Hardcore.Check(cInfo, __instance as EntityPlayer, true);
                }
            }
            else if (__instance is EntityZombie)
            {
                if (Confetti.IsEnabled && Confetti.Zombie && __instance.IsAlive())
                {
                    Confetti.Exec(__instance);
                }
                if (!ReservedSlots.IsEnabled || ReservedSlots.Bonus_Exp <= 0)
                {
                    return;
                }
                EntityAlive entityAlive = __instance.GetAttackTarget();
                if (entityAlive == null || !(entityAlive is EntityPlayer))
                {
                    return;
                }
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(entityAlive.entityId);
                if (cInfo == null)
                {
                    return;
                }
                if (ReservedSlots.Dict.ContainsKey(cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(cInfo.CrossplatformId.CombinedString))
                {
                    if (ReservedSlots.Bonus_Exp > 100)
                    {
                        ReservedSlots.Bonus_Exp = 100;
                    }
                    int experience = EntityClass.list[__instance.entityClass].ExperienceValue;
                    float percent = ReservedSlots.Bonus_Exp / 100f;
                    float bonus = experience * percent;
                    experience = (int)bonus / 2;
                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAddExpClient>().Setup(entityAlive.entityId, experience, Progression.XPTypes.Kill));
                }
                int experienceBoost = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ExperienceBoost;
                if (experienceBoost > 0)
                {
                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAddExpClient>().Setup(entityAlive.entityId, experienceBoost, Progression.XPTypes.Kill));
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.EntityAlive_SetDead_Prefix: {0}", e.Message));
        }
    }

    public static void EntityAlive_OnAddedToWorld_Postfix(EntityAlive __instance)
    {
        if (BigHead.IsEnabled && __instance is EntityZombie && !__instance.IsSleeper)
        {
            __instance.SetBigHead();
        }
    }

    public static void NetPackagePlayerInventory_ProcessPackage_Prefix(NetPackagePlayerInventory __instance)
    {
        try
        {
            if (InfiniteAmmo.IsEnabled)
            {
                InfiniteAmmo.Process(__instance);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackagePlayerInventory_ProcessPackage_Prefix: {0}", e.Message));
        }
    }

    public static void NetPackagePlayerInventory_ProcessPackage_Postfix(NetPackagePlayerInventory __instance)
    {
        if (__instance.Sender != null && Wallet.IsEnabled && (Wallet.UpdateMainCurrency.ContainsKey(__instance.Sender.entityId) || Wallet.UpdateAltCurrency.ContainsKey(__instance.Sender.entityId) || Bank.AddToBank.ContainsKey(__instance.Sender.CrossplatformId.CombinedString)))
        {
            ItemStack[] stacks = __instance.Sender.latestPlayerData.bag;
            for (int i = 0; i < stacks.Length; i++)
            {
                if (!stacks[i].IsEmpty() && stacks[i].itemValue.ItemClass.Name == GeneralOperations.Currency_Item)
                {
                    return;
                }
            }
            if (Bank.AddToBank.TryGetValue(__instance.Sender.CrossplatformId.CombinedString, out int bankAddition))
            {
                Bank.AddToBank.Remove(__instance.Sender.CrossplatformId.CombinedString);
                Bank.AddCurrencyToBank(__instance.Sender.CrossplatformId.CombinedString, bankAddition);
            }
            if (Wallet.UpdateMainCurrency.TryGetValue(__instance.Sender.entityId, out int walletAddition))
            {
                Wallet.UpdateMainCurrency.Remove(__instance.Sender.entityId);
                Wallet.AddCurrency(__instance.Sender.CrossplatformId.CombinedString, walletAddition, false);
            }
            if (Wallet.UpdateAltCurrency.TryGetValue(__instance.Sender.entityId, out List<string[]> altCurrency))
            {
                Wallet.UpdateAltCurrency.Remove(__instance.Sender.entityId);
                Wallet.AddAltCurrency(__instance.Sender.CrossplatformId.CombinedString, altCurrency);
            }
        }
    }

    public static bool ClientInfoCollection_GetForNameOrId_Prefix(ref ClientInfo __result, string _nameOrId)
    {
        try
        {
            ClientInfo clientInfo = null;
            if (int.TryParse(_nameOrId, out int entityId))
            {
                clientInfo = GeneralOperations.GetClientInfoFromEntityId(entityId);
                if (clientInfo != null)
                {
                    __result = clientInfo;
                    return false;
                }
            }
            clientInfo = GeneralOperations.GetClientInfoFromName(_nameOrId);
            if (clientInfo != null)
            {
                __result = clientInfo;
                return false;
            }
            if (_nameOrId.Contains("Local_") || _nameOrId.Contains("EOS_") || _nameOrId.Contains("Steam_") || _nameOrId.Contains("XBL_") || _nameOrId.Contains("PSN_") || _nameOrId.Contains("EGS_"))
            {
                if (PlatformUserIdentifierAbs.TryFromCombinedString(_nameOrId, out PlatformUserIdentifierAbs userIdentifier))
                {
                    clientInfo = GeneralOperations.GetClientInfoFromUId(userIdentifier);
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

    public static bool NetPackageDamageEntity_ProcessPackage_Prefix(int ___entityId, int ___attackerEntityId, ItemValue ___attackingItem, ushort ___strength, EnumDamageTypes ___damageTyp, short ___hitBodyPart, bool ___bCritical, bool ___bFatal)
    {
        Entity victim = GeneralOperations.GetEntity(___entityId);
        if (victim != null)
        {
            Entity attacker = GeneralOperations.GetEntity(___attackerEntityId);
            if (attacker != null)
            {
                return ProcessDamage.Exec(victim, attacker, ___attackingItem, ___strength, ___damageTyp, (EnumBodyPartHit)___hitBodyPart, ___bCritical, ___bFatal);
            }
        }
        return true;
    }

    public static void ClientInfo_SendPackage_Postfix(ClientInfo __instance, NetPackage _package)
    {
        if (_package is NetPackageTeleportPlayer || _package is NetPackageEntityTeleport)
        {
            if (!TeleportDetector.Ommissions.Contains(__instance.entityId))
            {
                TeleportDetector.Ommissions.Add(__instance.entityId);
            }
        }
    }

    public static bool PersistentPlayerList_PlaceLandProtectionBlock_Prefix(PersistentPlayerList __instance, Vector3i pos, PersistentPlayerData owner)
    {
        try
        {
            if (LandClaimCount.IsEnabled && __instance != null && pos != null && owner != null)
            {
                if (__instance.m_lpBlockMap.TryGetValue(pos, out PersistentPlayerData persistentPlayerData) && persistentPlayerData != null)
                {
                    persistentPlayerData.RemoveLandProtectionBlock(pos);
                }
                owner.AddLandProtectionBlock(pos);
                LandClaimCount.RemoveExtraLandClaims(owner);
                __instance.m_lpBlockMap[pos] = owner;
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.PersistentPlayerList_PlaceLandProtectionBlock_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool NetPackageEntityAttach_ProcessPackage_Prefix(NetPackageEntityAttach __instance)
    {
        try
        {
            EntityPlayer player = GeneralOperations.GetEntityPlayer(NoVehicleDrone.riderId(__instance));
            if (player != null)
            {
                if (player.IsDead())
                {
                    return false;
                }
                else if (NoVehicleDrone.IsEnabled && !NoVehicleDrone.Exec(__instance, player))
                {
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(player.entityId);
                    if (cInfo != null)
                    {
                        Phrases.Dict.TryGetValue("NoVehicleDrone1", out string phrase);
                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.NetPackageEntityAttach_ProcessPackage_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool LootManager_LootContainerOpened_Prefix(ref TileEntityLootContainer _tileEntity, int _entityIdThatOpenedIt)
    {
        try
        {
            if (_tileEntity != null && _tileEntity is TileEntityLootContainer && _tileEntity.bPlayerStorage &&
                _tileEntity.blockValue.Block != null && _tileEntity.blockValue.Block.GetBlockName() == "VaultBox")
            {
                return Vault.Exec(_entityIdThatOpenedIt, ref _tileEntity);
            }
        }
        catch (Exception e)
        {
            Log.Out("[SERVERTOOLS] Error in Injections.LootManager_LootContainerOpened_Prefix: {0}", e.Message);
        }
        return true;
    }

    public static void NetPackageTileEntity_ProcessPackage_Postfix(NetPackageTileEntity __instance, World _world, int ___clrIdx, Vector3i ___teWorldPos, byte ___handle)
    {
        try
        {
            if (__instance == null || ___teWorldPos == null || !Vault.IsEnabled)
            {
                return;
            }
            TileEntity tileEntity = _world.GetTileEntity(___clrIdx, ___teWorldPos);
            if (tileEntity == null)
            {
                return;
            }
            TileEntityLootContainer lootContainer = (TileEntityLootContainer)tileEntity;
            if (lootContainer != null && lootContainer.bPlayerStorage && lootContainer.blockValue.Block != null &&
                lootContainer.blockValue.Block.GetBlockName() == "VaultBox" && ___handle != 255)
            {
                Vault.UpdateData(lootContainer);
            }
        }
        catch (Exception e)
        {
            Log.Out("[SERVERTOOLS] Error in Injections.NetPackageTileEntity_ProcessPackage_Postfix: {0}", e.Message);
        }
    }

    public static bool GameManager_DropContentOfLootContainerServer_Prefix(BlockValue _bvOld)
    {
        if (_bvOld.Block.GetBlockName() == "VaultBox")
        {
            return false;
        }
        return true;
    }

    public static void GameManager_SavePlayerData_Postfix(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
    {
        if (_cInfo != null && _cInfo.latestPlayerData != null && _playerDataFile != null && DupeLog.IsEnabled)
        {
            DupeLog.Exec(_cInfo, _playerDataFile);
        }
    }

    public static bool BlockLandClaim_HandleDeactivatingCurrentLandClaims_Prefix()
    {
        if (LandClaimCount.IsEnabled)
        {
            return false;
        }
        return true;
    }

    public static bool Log_Out_Prefix(string _txt)
    {
        if (OutputLogBlocker.IsEnabled && OutputLogBlocker.Ommitted.Contains(_txt))
        {
            return false;
        }
        if (_txt.Contains("INF"))
        {
            string modifiedString = _txt.Substring(_txt.IndexOf("INF") + 4);
            if (modifiedString.Equals(GeneralOperations.lastOutput))
            {
                return false;
            }
            GeneralOperations.lastOutput = modifiedString;
        }
        else if (_txt.Contains("WRN"))
        {
            string modifiedString = _txt.Substring(_txt.IndexOf("WRN") + 4);
            if (modifiedString.Equals(GeneralOperations.lastOutput))
            {
                return false;
            }
            GeneralOperations.lastOutput = modifiedString;
        }
        else if (_txt.Contains("ERR"))
        {
            string modifiedString = _txt.Substring(_txt.IndexOf("ERR") + 4);
            if (modifiedString.Equals(GeneralOperations.lastOutput))
            {
                return false;
            }
            GeneralOperations.lastOutput = modifiedString;
        }
        return true;
    }

    public static void EntityAlive_OnEntityDeath_Prefix(EntityAlive __instance, EntityAlive ___entityThatKilledMe, DamageResponse ___RecordedDamage)
    {
        if (__instance != null && ___entityThatKilledMe != null)
        {
            if (__instance is EntityPlayer)
            {
                ClientInfo cInfoAttacker = GeneralOperations.GetClientInfoFromEntityId(___entityThatKilledMe.entityId);
                if (cInfoAttacker != null)
                {
                    if (Wallet.IsEnabled && Wallet.PVP && Wallet.Player_Kill > 0)
                    {
                        Wallet.AddCurrency(cInfoAttacker.CrossplatformId.CombinedString, Wallet.Player_Kill, true);
                    }
                    ClientInfo cInfoVictim = GeneralOperations.GetClientInfoFromEntityId(__instance.entityId);
                    if (cInfoVictim != null)
                    {
                        if (Bounties.IsEnabled)
                        {
                            Bounties.PlayerKilled((EntityPlayer)__instance, (EntityPlayer)___entityThatKilledMe, cInfoVictim, cInfoAttacker);
                        }
                    }
                }
            }
            else if (__instance is EntityZombie && ___entityThatKilledMe is EntityPlayer)
            {
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(___entityThatKilledMe.entityId);
                if (cInfo != null)
                {
                    if (Wallet.IsEnabled && Wallet.Zombie_Kill > 0)
                    {
                        Wallet.AddCurrency(cInfo.CrossplatformId.CombinedString, Wallet.Zombie_Kill, true);
                    }
                    if (BloodmoonWarrior.IsEnabled && BloodmoonWarrior.BloodmoonStarted && BloodmoonWarrior.WarriorList.Contains(cInfo.entityId))
                    {
                        BloodmoonWarrior.KilledZombies[cInfo.entityId] += 1;
                    }
                }
            }
            if (KillNotice.IsEnabled)
            {
                KillNotice.Exec(__instance, ___entityThatKilledMe, ___RecordedDamage);
            }
        }
    }

    public static bool UserIdentifierXbl_WriteCustomData_Prefix(ulong ___xuid)
    {
        if (___xuid == 0UL)
        {
            return false;
        }
        return true;
    }
}
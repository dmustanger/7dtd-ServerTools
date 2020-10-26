using HarmonyLib;
using System;
using System.Reflection;

namespace ServerTools
{
    static class PatchTools
    {
        public static bool Applied = false;

        public static void ApplyPatches()
        {
            try
            {
                if (!Applied)
                {
                    PatchAll();
                    Applied = true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PatchTools.ApplyPatches: {0}", e.Message));
            }
        }

        public static void PatchAll()
        {
            try
            {
                Log.Out("[SERVERTOOLS] Runtime patching initialized");
                Harmony harmony = new Harmony("com.github.servertools.patch");
                MethodInfo original = AccessTools.Method(typeof(EntityAlive), "ProcessDamageResponse");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive.ProcessDamageResponse method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: EntityAlive.ProcessDamageResponse method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Damage log from PvP and protection of PvE spaces may not function as intended"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(Injections).GetMethod("DamageResponse_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: ProcessDamageResponse.prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(GameManager).GetMethod("PlayerLoginRPC");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.PlayerLoginRPC method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.PlayerLoginRPC method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Reserved slots may not function as intended"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(Injections).GetMethod("PlayerLoginRPC_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PlayerLoginRPC.prefix"));
                            return;
                        }
                        MethodInfo postfix = typeof(Injections).GetMethod("PlayerLoginRPC_Postfix");
                        if (postfix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PlayerLoginRPC.postfix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
                    }
                }
                original = typeof(ConnectionManager).GetMethod("ServerConsoleCommand");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: ConnectionManager.ServerConsoleCommand method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: ConnectionManager.ServerConsoleCommand method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Console command log may not function as intended"));
                    }
                    else
                    {
                        MethodInfo postfix = typeof(Injections).GetMethod("ServerConsoleCommand_Postfix");
                        if (postfix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: ServerConsoleCommand.postfix"));
                            return;
                        }
                        harmony.Patch(original, null, new HarmonyMethod(postfix));
                    }
                }
                original = typeof(GameManager).GetMethod("ChangeBlocks");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ChangeBlocks method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ChangeBlocks method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Block logs and detection of the placement of blocks/damage may not function as intended"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(Injections).GetMethod("ChangeBlocks_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: ChangeBlocks.prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageAddRemoveBuff).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageAddRemoveBuff.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageAddRemoveBuff ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageAddRemoveBuff).GetMethod("PackageAddRemoveBuff_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageAddRemoveBuff_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageEntityStatsBuff).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageEntityStatsBuff.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageEntityStatsBuff ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageEntityStatsBuff).GetMethod("PackageEntityStatsBuff_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageEntityStatsBuff_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackagePlayerData).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePlayerData.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePlayerData ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackagePlayerData).GetMethod("PackagePlayerData_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackagePlayerData_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackagePlayerStats).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePlayerStats.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePlayerStats ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackagePlayerStats).GetMethod("PackagePlayerStats_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackagePlayerStats_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageChat).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageChat.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageChat ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageChat).GetMethod("PackageChat_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageChat_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageChunk).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageChunk.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageChunk ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageChunk).GetMethod("PackageChunk_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageChunk_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageDamageEntity).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageDamageEntity.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageDamageEntity ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageDamageEntity).GetMethod("PackageDamageEntity_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageDamageEntity_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageChunkRemove).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageChunkRemove.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageChunkRemove ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageChunkRemove).GetMethod("PackageChunkRemove_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageChunkRemove_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageConsoleCmdServer).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageConsoleCmdServer.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageConsoleCmdServer ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageConsoleCmdServer).GetMethod("PackageConsoleCmdServer_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageConsoleCmdServer_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageGameMessage).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageGameMessage.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageGameMessage ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageGameMessage).GetMethod("PackageGameMessage_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageGameMessage_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageItemReload).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageItemReload.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageItemReload ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageItemReload).GetMethod("PackageItemReload_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageItemReload_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageMapChunks).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageMapChunks.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageMapChunks ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageMapChunks).GetMethod("PackageMapChunks_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageMapChunks_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageChunkRemoveAll).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageChunkRemoveAll.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageChunkRemoveAll ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageChunkRemoveAll).GetMethod("PackageChunkRemoveAll_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageChunkRemoveAll_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageMapPosition).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageMapPosition.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageMapPosition ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageMapPosition).GetMethod("PackageMapPosition_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageMapPosition_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageModifyCVar).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageModifyCVar.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageModifyCVar ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageModifyCVar).GetMethod("PackageModifyCVar_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageModifyCVar_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackagePartyActions).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePartyActions.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePartyActions ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackagePartyActions).GetMethod("PackagePartyActions_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackagePartyActions_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackagePersistentPlayerState).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePersistentPlayerState.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePersistentPlayerState ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackagePersistentPlayerState).GetMethod("PackagePersistentPlayerState_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackagePersistentPlayerState_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageEntityRemove).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageEntityRemove.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageEntityRemove ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageEntityRemove).GetMethod("PackageEntityRemove_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageEntityRemove_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackagePlayerId).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePlayerId.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePlayerId ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackagePlayerId).GetMethod("PackagePlayerId_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackagePlayerId_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageWorldTime).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageWorldTime.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageWorldTime ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageWorldTime).GetMethod("PackageWorldTime_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageWorldTime_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
                original = typeof(NetPackageSetBlock).GetMethod("ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageSetBlock.ProcessPackage method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackageSetBlock ProcessPackage method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Anticheat unable to verify erroneous data verification"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(PackageSetBlock).GetMethod("PackageSetBlock_ProcessPackage_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PackageSetBlock_ProcessPackage_Prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PatchTools.PatchAll: {0}", e.Message));
            }
            Log.Out("[SERVERTOOLS] Runtime patching complete");
        }
    }
}
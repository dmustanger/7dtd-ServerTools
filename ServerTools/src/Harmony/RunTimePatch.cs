using HarmonyLib;
using System;
using System.Reflection;

namespace ServerTools
{
    class RunTimePatch
    {
        public static void PatchAll()
        {
            try
            {
                Log.Out("[SERVERTOOLS] Runtime patching initialized");
                Harmony harmony = new Harmony("com.github.servertools.patch");

                MethodInfo original = AccessTools.Method(typeof(GameManager), "PlayerLoginRPC");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.PlayerLoginRPC Class.Method was not found"));
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

                original = AccessTools.Method(typeof(ConnectionManager), "ServerConsoleCommand");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: ConnectionManager.ServerConsoleCommand Class.Method was not found"));
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

                original = AccessTools.Method(typeof(GameManager), "ChangeBlocks");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ChangeBlocks Class.Method was not found"));
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

                original = AccessTools.Method(typeof(World), "AddFallingBlock");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: World.AddFallingBlock Class.Method was not found"));
                }
                else
                {
                    MethodInfo postfix = typeof(Injections).GetMethod("AddFallingBlock_Postfix");
                    if (postfix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: AddFallingBlock.postfix"));
                        return;
                    }
                    harmony.Patch(original, null, new HarmonyMethod(postfix));
                }

                original = AccessTools.Method(typeof(World), "AddFallingBlocks");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: World.AddFallingBlocks Class.Method was not found"));
                }
                else
                {
                    MethodInfo postfix = typeof(Injections).GetMethod("AddFallingBlocks_Postfix");
                    if (postfix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: AddFallingBlocks.postfix"));
                        return;
                    }
                    harmony.Patch(original, null, new HarmonyMethod(postfix));
                }

                original = AccessTools.Method(typeof(GameManager), "ChatMessageServer");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ChatMessageServer Class.Method was not found"));
                }
                else
                {
                    MethodInfo postfix = typeof(Injections).GetMethod("ChatMessageServer_Postfix");
                    if (postfix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: ChatMessageServer.postfix"));
                        return;
                    }
                    harmony.Patch(original, null, new HarmonyMethod(postfix));
                }

                original = AccessTools.Method(typeof(GameManager), "Cleanup");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.Cleanup Class.Method was not found"));
                }
                else
                {
                    Patches _patchInfo = Harmony.GetPatchInfo(original);
                    if (_patchInfo == null)
                    {
                        MethodInfo Finalizer = typeof(Injections).GetMethod("GameManager_Cleanup_Finalizer");
                        if (Finalizer == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager_Cleanup.Finalizer"));
                            return;
                        }
                        harmony.Patch(original, null, null, null, new HarmonyMethod(Finalizer));
                    }
                }

                original = AccessTools.Method(typeof(GameManager), "CollectEntityServer");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.CollectEntityServer Class.Method was not found"));
                }
                else
                {
                    MethodInfo prefix = typeof(Injections).GetMethod("GameManager_CollectEntityServer_Prefix");
                    if (prefix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager_CollectEntityServer.prefix"));
                        return;
                    }
                    harmony.Patch(original, new HarmonyMethod(prefix), null);
                }

                original = AccessTools.Method(typeof(GameManager), "OpenTileEntityAllowed");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.OpenTileEntityAllowed Class.Method was not found"));
                }
                else
                {
                    MethodInfo prefix = typeof(Injections).GetMethod("GameManager_OpenTileEntityAllowed_Prefix");
                    if (prefix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager_OpenTileEntityAllowed.prefix"));
                        return;
                    }
                    harmony.Patch(original, new HarmonyMethod(prefix), null);
                }

                original = AccessTools.Method(typeof(GameManager), "OpenTileEntityAllowed");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.OpenTileEntityAllowed Class.Method was not found"));
                }
                else
                {
                    MethodInfo postfix = typeof(Injections).GetMethod("GameManager_OpenTileEntityAllowed_Postfix");
                    if (postfix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager_OpenTileEntityAllowed.postfix"));
                        return;
                    }
                    harmony.Patch(original, null, new HarmonyMethod(postfix));
                }

                original = AccessTools.Method(typeof(EntityAlive), "OnEntityDeath");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive.OnEntityDeath Class.Method was not found"));
                }
                else
                {
                    MethodInfo prefix = typeof(Injections).GetMethod("EntityAlive_OnEntityDeath_Prefix");
                    if (prefix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive_OnEntityDeath.prefix"));
                        return;
                    }
                    harmony.Patch(original, new HarmonyMethod(prefix), null);
                }

                original = AccessTools.Method(typeof(ChunkCluster), "AddChunkSync");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: ChunkCluster.AddChunkSync Class.Method was not found"));
                }
                else
                {
                    MethodInfo postfix = typeof(Injections).GetMethod("ChunkCluster_AddChunkSync_Postfix");
                    if (postfix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: ChunkCluster_AddChunkSync.postfix"));
                        return;
                    }
                    harmony.Patch(original, null, new HarmonyMethod(postfix));
                }

                original = AccessTools.Method(typeof(World), "SpawnEntityInWorld");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: World.SpawnEntityInWorld Class.Method was not found"));
                }
                else
                {
                    MethodInfo postfix = typeof(Injections).GetMethod("World_SpawnEntityInWorld_Postfix");
                    if (postfix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: World_SpawnEntityInWorld.postfix"));
                        return;
                    }
                    harmony.Patch(original, null, new HarmonyMethod(postfix));
                }

                original = AccessTools.Method(typeof(GameManager), "ItemReloadServer");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ItemReloadServer Class.Method was not found"));
                }
                else
                {
                    MethodInfo postfix = typeof(Injections).GetMethod("GameManager_ItemReloadServer_Postfix");
                    if (postfix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager_ItemReloadServer.postfix"));
                        return;
                    }
                    harmony.Patch(original, null, new HarmonyMethod(postfix));
                }

                original = AccessTools.Method(typeof(GameManager), "PlayerSpawnedInWorld");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.PlayerSpawnedInWorld Class.Method was not found"));
                }
                else
                {
                    MethodInfo postfix = typeof(Injections).GetMethod("GameManager_PlayerSpawnedInWorld_Postfix");
                    if (postfix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager_PlayerSpawnedInWorld.postfix"));
                        return;
                    }
                    harmony.Patch(original, null, new HarmonyMethod(postfix));
                }

                original = AccessTools.Method(typeof(EntityAlive), "SetDead");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive.SetDead Class.Method was not found"));
                }
                else
                {
                    MethodInfo postfix = typeof(Injections).GetMethod("EntityAlive_SetDead_Postfix");
                    if (postfix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive_SetDead.postfix"));
                        return;
                    }
                    harmony.Patch(original, null, new HarmonyMethod(postfix));
                }

                original = AccessTools.Method(typeof(NetPackagePlayerInventory), "ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePlayerInventory.ProcessPackage Class.Method was not found"));
                }
                else
                {
                    MethodInfo postfix = typeof(Injections).GetMethod("NetPackagePlayerInventory_ProcessPackage_Postfix");
                    if (postfix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePlayerInventory_ProcessPackage_Postfix"));
                        return;
                    }
                    harmony.Patch(original, null, new HarmonyMethod(postfix));
                }

                original = AccessTools.Method(typeof(EntityAlive), "DamageEntity");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive.DamageEntity Class.Method was not found"));
                }
                else
                {
                    MethodInfo prefix = typeof(Injections).GetMethod("EntityAlive_DamageEntity_Prefix");
                    if (prefix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive_DamageEntity_Prefix"));
                        return;
                    }
                    harmony.Patch(original, new HarmonyMethod(prefix), null);
                }

                original = AccessTools.Method(typeof(EntityAlive), "ProcessDamageResponse");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive.ProcessDamageResponse Class.Method was not found"));
                }
                else
                {
                    MethodInfo prefix = typeof(Injections).GetMethod("EntityAlive_ProcessDamageResponse_Prefix");
                    if (prefix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive_ProcessDamageResponse_Prefix"));
                        return;
                    }
                    harmony.Patch(original, new HarmonyMethod(prefix), null);
                }

                original = AccessTools.Method(typeof(ClientInfoCollection), "GetForNameOrId");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: ClientInfoCollection.GetForNameOrId Class.Method was not found"));
                }
                else
                {
                    MethodInfo prefix = typeof(Injections).GetMethod("ClientInfoCollection_GetForNameOrId_Prefix");
                    if (prefix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: ClientInfoCollection_GetForNameOrId_Prefix"));
                        return;
                    }
                    harmony.Patch(original, new HarmonyMethod(prefix), null);
                }

                original = AccessTools.Method(typeof(NetPackagePlayerStats), "ProcessPackage");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePlayerStats.ProcessPackage Class.Method was not found"));
                }
                else
                {
                    MethodInfo prefix = typeof(Injections).GetMethod("NetPackagePlayerStats_ProcessPackage_Prefix");
                    if (prefix == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: NetPackagePlayerStats_ProcessPackage_Prefix"));
                        return;
                    }
                    harmony.Patch(original, new HarmonyMethod(prefix), null);
                }

                Log.Out("[SERVERTOOLS] Runtime patching complete");
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PatchTools.PatchAll: {0}", e.Message));
            }
        }
    }
}
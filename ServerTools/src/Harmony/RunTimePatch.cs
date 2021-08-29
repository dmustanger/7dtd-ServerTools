using HarmonyLib;
using System;
using System.Reflection;

namespace ServerTools
{
    class RunTimePatch
    {
        public static bool Applied = false;

        public static void PatchAll()
        {
            try
            {
                if (!Applied)
                {
                    Log.Out("[SERVERTOOLS] Runtime patching initialized");
                    Harmony harmony = new Harmony("com.github.servertools.patch");

                    MethodInfo original = AccessTools.Method(typeof(GameManager), "PlayerLoginRPC");
                    if (original == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.PlayerLoginRPC method was not found"));
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
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: ConnectionManager.ServerConsoleCommand method was not found"));
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
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ChangeBlocks method was not found"));
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
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: World.AddFallingBlock method was not found"));
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
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: World.AddFallingBlocks method was not found"));
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
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ChatMessageServer method was not found"));
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
                    
                    original = AccessTools.Method(typeof(EntityAlive), "DamageEntity");
                    if (original == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive.DamageEntity method was not found"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(Injections).GetMethod("EntityAlive_DamageEntity_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive_DamageEntity.prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }
                    
                    original = AccessTools.Method(typeof(GameManager), "Cleanup");
                    if (original == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.Cleanup method was not found"));
                    }
                    else
                    {
                        MethodInfo postfix = typeof(Injections).GetMethod("GameManager_Cleanup_Postfix");
                        if (postfix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager_Cleanup.postfix"));
                            return;
                        }
                        harmony.Patch(original, null, new HarmonyMethod(postfix));
                    }
                    
                    original = AccessTools.Method(typeof(ObjectiveTreasureChest), "CalculateTreasurePoint");
                    if (original == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: ObjectiveTreasureChest.CalculateTreasurePoint method was not found"));
                    }
                    else
                    {
                        MethodInfo finalizer = typeof(Injections).GetMethod("ObjectiveTreasureChest_CalculateTreasurePoint_finalizer");
                        if (finalizer == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: ObjectiveTreasureChest_CalculateTreasurePoint.finalizer"));
                            return;
                        }
                        harmony.Patch(original, null, null, null, new HarmonyMethod(finalizer));
                    }
                    
                    original = AccessTools.Method(typeof(EntityAlive), "ProcessDamageResponse");
                    if (original == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive.ProcessDamageResponse method was not found"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(Injections).GetMethod("EntityAlive_ProcessDamageResponse_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive_ProcessDamageResponse.prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }

                    original = AccessTools.Method(typeof(GameManager), "CollectEntityServer");
                    if (original == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.CollectEntityServer method was not found"));
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

                    //original = AccessTools.Method(typeof(World), "SpawnEntityInWorld");
                    //if (original == null)
                    //{
                    //    Log.Out(string.Format("[SERVERTOOLS] Injection failed: World.SpawnEntityInWorld method was not found"));
                    //}
                    //else
                    //{
                    //    MethodInfo prefix = typeof(Injections).GetMethod("World_SpawnEntityInWorld_Prefix");
                    //    if (prefix == null)
                    //    {
                    //        Log.Out(string.Format("[SERVERTOOLS] Injection failed: World_SpawnEntityInWorld.prefix"));
                    //        return;
                    //    }
                    //    harmony.Patch(original, new HarmonyMethod(prefix), null);
                    //}

                    original = AccessTools.Method(typeof(GameManager), "OnApplicationQuit");
                    if (original == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.OnApplicationQuit method was not found"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(Injections).GetMethod("GameManager_OnApplicationQuit_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager_OnApplicationQuit.prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null);
                    }

                    Applied = true;
                    Log.Out("[SERVERTOOLS] Runtime patching complete");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PatchTools.PatchAll: {0}", e.Message));
            }
        }
    }
}
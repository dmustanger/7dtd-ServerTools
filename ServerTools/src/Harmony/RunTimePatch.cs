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
                    MethodInfo original = AccessTools.Method(typeof(EntityAlive), "ProcessDamageResponse");
                    if (original == null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive.ProcessDamageResponse method was not found"));
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
                    original = typeof(GameManager).GetMethod("PlayerLoginRPC");
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
                    original = typeof(ConnectionManager).GetMethod("ServerConsoleCommand");
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
                    original = typeof(GameManager).GetMethod("ChangeBlocks");
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
                    original = typeof(World).GetMethod("AddFallingBlocks");
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
                    original = typeof(GameManager).GetMethod("ChatMessageServer");
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
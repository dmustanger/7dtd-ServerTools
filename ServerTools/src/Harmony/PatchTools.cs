using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace ServerTools
{
    static class PatchTools
    {
        public static bool Applied = false;

        private static readonly Type patchType = typeof(Injections);

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
                        harmony.Patch(original, new HarmonyMethod(prefix), null, null);
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
                        harmony.Patch(original, new HarmonyMethod(prefix), null, null);
                    }
                }
                original = typeof(GameManager).GetMethod("ExplosionServer");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ExplosionServer method was not found"));
                }
                else
                {
                    Patches info = Harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ExplosionServer method is already modified by another mod"));
                        Log.Out(string.Format("[SERVERTOOLS] Injection warning: Explosion prevention in protected spaces may not function as intended"));
                    }
                    else
                    {
                        MethodInfo prefix = typeof(Injections).GetMethod("ExplosionServer_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: ExplosionServer.prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null, null);
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
                        harmony.Patch(original, null, new HarmonyMethod(postfix), null);
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
                        harmony.Patch(original, new HarmonyMethod(prefix), null, null);
                    }
                }
                //original = typeof(World).GetMethod("IsWithinTraderArea");
                //if (original == null)
                //{
                //    Log.Out(string.Format("[SERVERTOOLS] Injection failed: World.IsWithinTraderArea method was not found"));
                //}
                //else
                //{
                //    var info = harmony.GetPatchInfo(original);
                //    if (info != null)
                //    {
                //        Log.Out(string.Format("[SERVERTOOLS] Injection failed: World.IsWithinTraderArea method is already modified by another mod"));
                //    }
                //    else
                //    {
                //        var prefix = typeof(Injections).GetMethod("IsWithinTraderArea_Prefix");
                //        if (prefix == null)
                //        {
                //            Log.Out(string.Format("[SERVERTOOLS] Injection failed: IsWithinTraderArea.prefix"));
                //            return;
                //        }
                //        harmony.Patch(original, new HarmonyMethod(prefix), null, null);
                //    }
                //}
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PatchTools.PatchAll: {0}", e.Message));
            }
            Log.Out("[SERVERTOOLS] Runtime patching complete");
        }
    }
}
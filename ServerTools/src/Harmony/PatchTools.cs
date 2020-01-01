using Harmony;
using System;

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
                    Log.Out("[SERVERTOOLS] Runtime patching initialized");
                    PatchAll();
                    Applied = true;
                    Log.Out("[SERVERTOOLS] Runtime patching complete");
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
                var harmony = HarmonyInstance.Create("com.github.servertools.patch");
                var original = typeof(EntityAlive).GetMethod("ProcessDamageResponse");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive.ProcessDamageResponse method was not found"));
                }
                else
                {
                    var info = harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: EntityAlive.ProcessDamageResponse method is already modified by another mod"));
                    }
                    else
                    {
                        var prefix = typeof(Injections).GetMethod("DamageResponse_Prefix");
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
                    var info = harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.PlayerLoginRPC method is already modified by another mod"));
                    }
                    else
                    {
                        var prefix = typeof(Injections).GetMethod("PlayerLoginRPC_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: PlayerLoginRPC.prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null, null);
                    }
                }
                original = typeof(GameManager).GetMethod("ChangeBlocks");
                if (original == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ChangeBlocks method was not found"));
                }
                else
                {
                    var info = harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ChangeBlocks method is already modified by another mod"));
                    }
                    else
                    {
                        var prefix = typeof(Injections).GetMethod("ChangeBlocks_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: ChangeBlocks.prefix"));
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
                    var info = harmony.GetPatchInfo(original);
                    if (info != null)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Injection failed: GameManager.ExplosionServer method is already modified by another mod"));
                    }
                    else
                    {
                        var prefix = typeof(Injections).GetMethod("ExplosionServer_Prefix");
                        if (prefix == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Injection failed: ExplosionServer.prefix"));
                            return;
                        }
                        harmony.Patch(original, new HarmonyMethod(prefix), null, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PatchTools.PatchAll: {0}", e.Message));
            }
        }
    }
}
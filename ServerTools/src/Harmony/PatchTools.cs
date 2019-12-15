using Harmony;
using System;

namespace ServerTools
{
    static class PatchTools
    {
        public static bool ProcessEntityDamage = false, PlayerLoginRPC = false, ChangeBlocks = false;

        public static void ApplyPatches()
        {
            try
            {
                Log.Out("[SERVERTOOLS] Runtime patching initialized");
                if (!ProcessEntityDamage)
                {
                    PatchProcessEntityDamage();
                }
                if (!ChangeBlocks)
                {
                    PatchChangeBlocks();
                }
                if (!PlayerLoginRPC)
                {
                    PatchPlayerLoginRPC();
                }
                Log.Out("[SERVERTOOLS] Runtime patching complete");
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PatchTools.ApplyPatches: {0}.", e.Message));
            }
        }       

        public static void PatchProcessEntityDamage()
        {
            var harmony = HarmonyInstance.Create("com.github.servertools.patch");
            var original = typeof(EntityAlive).GetMethod("ProcessDamageResponse");
            if (original == null)
            {
                Log.Out(string.Format("[SERVERTOOLS] Injection failed: PatchProcessEntityDamage.original"));
                return;
            }
            var info = harmony.GetPatchInfo(original);
            if (info != null)
            {
                Log.Out(string.Format("[SERVERTOOLS] Injection failed: PatchProcessEntityDamage.info"));
                return;
            }
            var postfix = typeof(Injections).GetMethod("ProcessDamageResponse_Postfix");
            if (postfix == null)
            {
                Log.Out(string.Format("[SERVERTOOLS] Injection failed: PatchProcessEntityDamage.postfix"));
                return;
            }
            harmony.Patch(original, null, new HarmonyMethod(postfix), null);
            ProcessEntityDamage = true;
        }

        public static void PatchPlayerLoginRPC()
        {
            var harmony = HarmonyInstance.Create("com.github.servertools.patch");
            var original = typeof(GameManager).GetMethod("PlayerLoginRPC");
            if (original == null)
            {
                Log.Out(string.Format("[SERVERTOOLS] Injection failed: PatchPlayerLoginRPC.original"));
                return;
            }
            var info = harmony.GetPatchInfo(original);
            if (info != null)
            {
                Log.Out(string.Format("[SERVERTOOLS] Injection failed: PatchPlayerLoginRPC.info"));
                return;
            }
            var prefix = typeof(Injections).GetMethod("PlayerLoginRPC_Prefix");
            if (prefix == null)
            {
                Log.Out(string.Format("[SERVERTOOLS] Injection failed: PatchPlayerLoginRPC.prefix"));
                return;
            }
            harmony.Patch(original, new HarmonyMethod(prefix), null, null);
            PlayerLoginRPC = true;
        }

        public static void PatchChangeBlocks()
        {
            var harmony = HarmonyInstance.Create("com.github.servertools.patch");
            var original = typeof(GameManager).GetMethod("ChangeBlocks");
            if (original == null)
            {
                Log.Out(string.Format("[SERVERTOOLS] Injection failed: PatchChangeBlocks.original"));
                return;
            }
            var info = harmony.GetPatchInfo(original);
            if (info != null)
            {
                Log.Out(string.Format("[SERVERTOOLS] Injection failed: PatchChangeBlocks.info"));
                return;
            }
            var prefix = typeof(Injections).GetMethod("ChangeBlocks_Prefix");
            if (prefix == null)
            {
                Log.Out(string.Format("[SERVERTOOLS] Injection failed: PatchChangeBlocks.prefix"));
                return;
            }
            harmony.Patch(original, new HarmonyMethod(prefix), null, null);
            ChangeBlocks = true;
        }
    }
}
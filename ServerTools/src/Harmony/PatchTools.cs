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
                Log.Out(string.Format("[SERVERTOOLS] Error in PatchTools.ApplyPatches: {0}.", e.Message));
            }
        }

        public static void PatchAll()
        {
            try
            {
                var harmony = HarmonyInstance.Create("com.github.servertools.patch");
                harmony.Patch(original: AccessTools.Method(type: typeof(EntityAlive), name: nameof(EntityAlive.ProcessDamageResponse)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(Injections.DamageResponse_Prefix)));

                harmony.Patch(original: AccessTools.Method(type: typeof(GameManager), name: nameof(GameManager.PlayerLoginRPC)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(Injections.PlayerLoginRPC_Prefix)));

                harmony.Patch(original: AccessTools.Method(type: typeof(GameManager), name: nameof(GameManager.ChangeBlocks)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(Injections.ChangeBlocks_Prefix)));

                harmony.Patch(original: AccessTools.Method(type: typeof(GameManager), name: nameof(GameManager.ExplosionServer)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(Injections.ExplosionServer_Prefix)));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PatchTools.PatchAll: {0}.", e.Message));
            }
        }
    }
}
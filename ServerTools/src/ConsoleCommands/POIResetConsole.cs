using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class POIResetConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Resets POI locations.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. st-rpoi <Size> true/false\n" +
                "1. Resets POI locations to their original state.\n" +
                "*Note* <Size> can be small, medium, large or all.\n" +
                "*Note* true/false controls if a POI containing a player claim will be reset.\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-ResetPoi", "rpoi", "st-rpoi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                    return;
                }
                if (GameManager.Instance == null && GameManager.Instance.GetDynamicPrefabDecorator() == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Failed to reset POI"));
                    return;
                }
                List<PrefabInstance> dynamicPrefabs = GameManager.Instance.GetDynamicPrefabDecorator().GetDynamicPrefabs();
                if (dynamicPrefabs == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Failed to reset POI"));
                    return;
                }
                if (dynamicPrefabs.Count < 1)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Unable to locate any POI"));
                    return;
                }
                string size = _params[0].ToLower();
                if (size != "small" && size != "medium" && size != "large" && size != "all")
                {
                    Log.Out(string.Format("[SERVERTOOLS] Improper size: {0}", size));
                    return;
                }
                if (!bool.TryParse(_params[1].ToLower(), out bool result))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Invalid true or false: {0}", _params[1]));
                    return;
                }
                World world = GameManager.Instance.World;
                ChunkCluster chunkCache = world.ChunkCache;
                ChunkProviderGenerateWorld chunkProviderGenerateWorld = chunkCache.ChunkProvider as ChunkProviderGenerateWorld;
                if (chunkProviderGenerateWorld == null)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Failed to reset POI"));
                    return;
                }
                PrefabInstance prefab;
                Chunk[] neighbors = new Chunk[8];
                Bounds bounds = new Bounds();
                HashSetLong hashSet = new HashSetLong();
                Vector3i max, min;
                long hash;
                int num, num2, num3, num4;
                Dictionary<Vector3i, PersistentPlayerData> landClaims =  GameManager.Instance.persistentPlayers.m_lpBlockMap;
                int prefabCount = dynamicPrefabs.Count;
                for (int i = 0; i < prefabCount; i++)
                {
                    prefab = dynamicPrefabs[i];
                    if (size == "all" || (prefab.boundingBoxSize.Volume() <= 40 && size == "small") || 
                        (prefab.boundingBoxSize.Volume() > 40 && prefab.boundingBoxSize.Volume() <= 80 && size == "medium") ||
                        (prefab.boundingBoxSize.Volume() > 80 && size == "large"))
                    {
                        bounds = prefab.GetAABB();
                        if (!result)
                        {
                            foreach (var entry in landClaims)
                            {
                                if (bounds.Contains(entry.Key))
                                {
                                    continue;
                                }
                            }
                        }
                        max = new Vector3i(bounds.max.x, bounds.max.y, bounds.max.z);
                        min = new Vector3i(bounds.min.x, bounds.min.y, bounds.min.z);
                        num = World.toChunkXZ(min.x);
                        num2 = World.toChunkXZ(min.z);
                        num3 = World.toChunkXZ(max.x);
                        num4 = World.toChunkXZ(max.z);
                        for (int j = num; j <= num3; j++)
                        {
                            for (int k = num2; k <= num4; k++)
                            {
                                hash = WorldChunkCache.MakeChunkKey(j, k);
                                if (!hashSet.Contains(hash))
                                {
                                    hashSet.Add(hash);
                                    Chunk chunk = world.ChunkCache.GetChunkSync(hash);
                                    if (chunk == null)
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (long key in hashSet)
                {
                    if (!chunkProviderGenerateWorld.GenerateSingleChunk(world.ChunkCache, key, true))
                    {
                        SdtdConsole.Instance.Output(string.Format("Failed regenerating chunk at position {0}/{1}", WorldChunkCache.extractX(key) << 4, WorldChunkCache.extractZ(key) << 4));
                    }
                }
                chunkProviderGenerateWorld.Cleanup();
                GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(hashSet);
                Log.Out(string.Format("[SERVERTOOLS] POI reset completed"));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in POIResetConsole.Execute: {0}", e.Message));
            }
        }
    }
}

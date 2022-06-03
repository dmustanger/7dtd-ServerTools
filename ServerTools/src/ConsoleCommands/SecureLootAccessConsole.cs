using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SecureLootAccessConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Attempts to set access to all secure loot.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-sla\n" +
                   "1. Attempts to set access to all secure loot\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-SecureLootAccess", "sla", "st-sla" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (!string.IsNullOrEmpty(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString))
                {
                    LinkedList<Chunk> array = new LinkedList<Chunk>();
                    DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
                    ChunkClusterList chunklist = GameManager.Instance.World.ChunkClusters;
                    for (int i = 0; i < chunklist.Count; i++)
                    {
                        ChunkCluster cluster = chunklist[i];
                        array = cluster.GetChunkArray();
                        foreach (Chunk chunk in array)
                        {
                            tiles = chunk.GetTileEntities();
                            foreach (TileEntity tile in tiles.dict.Values)
                            {
                                if (tile is TileEntitySecureLootContainer)
                                {
                                    TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tile;
                                    if (!SecureLoot.IsUserAllowed(_senderInfo.RemoteClientInfo.CrossplatformId))
                                    {
                                        List<PlatformUserIdentifierAbs> users = SecureLoot.GetUsers();
                                        users.Add(_senderInfo.RemoteClientInfo.CrossplatformId);
                                        SecureLoot.SetModified();
                                    }
                                }
                                else if (tile is TileEntitySecureLootContainerSigned)
                                {
                                    TileEntitySecureLootContainerSigned SecureLoot = (TileEntitySecureLootContainerSigned)tile;
                                    if (!SecureLoot.IsUserAllowed(_senderInfo.RemoteClientInfo.CrossplatformId))
                                    {
                                        List<PlatformUserIdentifierAbs> users = SecureLoot.GetUsers();
                                        users.Add(_senderInfo.RemoteClientInfo.CrossplatformId);
                                        SecureLoot.SetModified();
                                    }
                                }
                            }
                        }
                    }
                }
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Secure loot access set for '{0}' in all loaded areas. Unloaded areas have not changed", _senderInfo.RemoteClientInfo.CrossplatformId.CombinedString));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SecureLootAccessConsole.Execute: {0}", e.Message));
            }
        }
    }
}

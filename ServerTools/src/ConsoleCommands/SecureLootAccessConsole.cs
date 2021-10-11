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
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (!string.IsNullOrEmpty(_senderInfo.RemoteClientInfo.playerId))
                {
                    LinkedList<Chunk> chunkArray = new LinkedList<Chunk>();
                    DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
                    ChunkClusterList chunklist = GameManager.Instance.World.ChunkClusters;
                    for (int i = 0; i < chunklist.Count; i++)
                    {
                        ChunkCluster chunk = chunklist[i];
                        chunkArray = chunk.GetChunkArray();
                        foreach (Chunk _c in chunkArray)
                        {
                            tiles = _c.GetTileEntities();
                            foreach (TileEntity tile in tiles.dict.Values)
                            {
                                TileEntityType type = tile.GetTileEntityType();
                                if (type.ToString().Equals("SecureLoot"))
                                {
                                    TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tile;
                                    if (!SecureLoot.IsUserAllowed(_senderInfo.RemoteClientInfo.playerId))
                                    {
                                        List<string> _users = SecureLoot.GetUsers();
                                        _users.Add(_senderInfo.RemoteClientInfo.playerId);
                                        SecureLoot.SetModified();
                                    }
                                }
                            }
                        }
                    }
                }
                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Secure loot access set for {0} in all loaded areas. Unloaded areas have not changed", _senderInfo.RemoteClientInfo.playerId));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SecureLootAccessConsole.Execute: {0}", e.Message));
            }
        }
    }
}

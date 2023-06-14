using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SecureDoorAccessConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Attempts to set access to all secure door.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-sda\n" +
                   "1. Attempts to set access to all secure door\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-SecureDoorAccess", "sda", "st-sda" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (!string.IsNullOrEmpty(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString))
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
                                if (type.ToString().Equals("SecureDoor"))
                                {
                                    TileEntitySecureDoor SecureDoor = (TileEntitySecureDoor)tile;
                                    if (!SecureDoor.IsUserAllowed(_senderInfo.RemoteClientInfo.CrossplatformId))
                                    {
                                        List<PlatformUserIdentifierAbs> _users = SecureDoor.GetUsers();
                                        _users.Add(_senderInfo.RemoteClientInfo.CrossplatformId);
                                        SecureDoor.SetModified();
                                    }
                                }
                            }
                        }
                    }
                }
                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Door access set for '{0}'", _senderInfo.RemoteClientInfo.CrossplatformId.CombinedString));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SecureDoorAccess.Execute: {0}", e.Message));
            }
        }
    }
}

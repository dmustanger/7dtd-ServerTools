using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SecureDoorAccess : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Attempts to set access to all secure door.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. SecureDoorAccess\n" +
                   "1. Attempts to set access to all secure door\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-SecureDoorAccess", "securedooraccess", "st-sda", "sda" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
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
                                if (type.ToString().Equals("SecureDoor"))
                                {
                                    TileEntitySecureDoor SecureDoor = (TileEntitySecureDoor)tile;
                                    if (!SecureDoor.IsUserAllowed(_senderInfo.RemoteClientInfo.playerId))
                                    {
                                        List<string> _users = SecureDoor.GetUsers();
                                        _users.Add(_senderInfo.RemoteClientInfo.playerId);
                                        SecureDoor.SetModified();
                                    }
                                }
                            }
                        }
                    }
                }
                SdtdConsole.Instance.Output(string.Format("Door access set for {0}", _senderInfo.RemoteClientInfo.playerId));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SecureDoorAccess.Execute: {0}.", e));
            }
        }
    }
}

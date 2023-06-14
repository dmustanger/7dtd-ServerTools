using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SpawnActiveBlocksConsole : ConsoleCmdAbstract
    {
        public static Dictionary<int, Vector3i[]> Vectors = new Dictionary<int, Vector3i[]>();
        public static Dictionary<string, Dictionary<Vector3i, BlockValue>> Undo = new Dictionary<string, Dictionary<Vector3i, BlockValue>>();

        protected override string getDescription()
        {
            return "[ServerTools] - Spawn active blocks in the world";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-sab\n" +
                   "  2. st-sab <BlockName>\n" +
                   "  3. st-sab save <1> or <2>\n" +
                   "  4. st-sab remove\n" +
                   "  5. st-sab undo\n" +
                   "1. Spawns the block you are holding from corner to corner. You must have two saved corner positions\n" +
                   "2. Spawns the specified block from corner to corner. You must have two saved corner positions\n" +
                   "3. Saves the position you are standing as corner 1 or 2\n" +
                   "4. Remove your saved corner positions\n" +
                   "5. Undo the last spawned blocks to their previous value\n" +
                   "*Notes*\n" +
                   "You must build on top of existing blocks to avoid collapse\n" +
                   "There is an intentional pause of five seconds after the blocks spawn. If blocks have collapsed, it will automatically spawn a replacement\n" +
                   "Think of the corners as opposites in a 3d rectangle or square. Everything in between the corners will be set as the block of choice\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-SpawnActiveBlocks", "sab", "st-sab" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 0 && _params.Count != 1 && _params.Count != 2)
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Wrong number of arguments, expected 0 to 2, found '{0}'", _params.Count);
                    return;
                }
                if (_senderInfo.RemoteClientInfo == null)
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] No client info found. Join the server as a client before using this command");
                    return;
                }
                World world = GameManager.Instance.World;
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.entityId);
                if (player == null)
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Unable to locate player data");
                    return;
                }
                if (_params[0].ToLower() == "save")
                {
                    Vector3i playerPos = new Vector3i(player.position);
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count);
                        return;
                    }
                    if (playerPos.y < 2)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Your position is too low. Unable to generate blocks at this world height");
                        return;
                    }
                    if (_params[1] == "1")
                    {
                        if (!Vectors.ContainsKey(player.entityId))
                        {
                            Vector3i[] vectors = new Vector3i[2];
                            vectors[0] = playerPos;
                            Vectors.Add(player.entityId, vectors);
                        }
                        else
                        {
                            Vectors.TryGetValue(player.entityId, out Vector3i[] vectors);
                            vectors[0] = playerPos;
                            Vectors[player.entityId] = vectors;
                        }
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Set corner 1 to '{0}'", playerPos);
                        return;
                    }
                    else if (_params[1] == "2")
                    {
                        if (!Vectors.ContainsKey(player.entityId))
                        {
                            Vector3i[] vectors = new Vector3i[2];
                            vectors[1] = playerPos;
                            Vectors.Add(player.entityId, vectors);
                        }
                        else
                        {
                            Vectors.TryGetValue(player.entityId, out Vector3i[] vectors);
                            vectors[1] = playerPos;
                            Vectors[player.entityId] = vectors;
                        }
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Set corner 2 to '{0}'", playerPos);
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (!Vectors.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] You have no saved corner positions");
                        return;
                    }
                    Vectors.Remove(_senderInfo.RemoteClientInfo.entityId);
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Removed your saved corner positions");
                    return;
                }
                else if (_params[0].ToLower().Equals("undo"))
                {
                    if (!Undo.ContainsKey(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString))
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] You have not spawned any blocks. Unable to undo");
                        return;
                    }
                    Undo.TryGetValue(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString, out Dictionary<Vector3i, BlockValue> undo);
                    List<BlockChangeInfo> blockList = new List<BlockChangeInfo>();
                    foreach (var block in undo)
                    {
                        if (!world.IsChunkAreaLoaded(block.Key.x, block.Key.y, block.Key.z))
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Area is not loaded. Unable to undo maze blocks");
                            return;
                        }
                        blockList.Add(new BlockChangeInfo(0, block.Key, block.Value));
                    }
                    Undo.Remove(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString);
                    GameManager.Instance.SetBlocksRPC(blockList, null);
                    SdtdConsole.Instance.Output("[SERVERTOOLS] The blocks you last spawned have been set to their original value");
                    return;
                }
                else
                {
                    string blockName = "";
                    if (_params.Count == 1)
                    {
                        blockName = _params[0];
                        Block block = Block.GetBlockByName(blockName, false);
                        if (block == null)
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Unable to spawn block. Could not find a block with the name '{0}'", blockName);
                            return;
                        }
                    }
                    else
                    {
                        ItemClass itemClass = player.inventory.holdingItem;
                        blockName = itemClass.Name;
                        Block block = Block.GetBlockByName(blockName, false);
                        if (block == null)
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Unable to spawn block. Could not find a block being held");
                            return;
                        }
                    }
                    BlockValue blockValue = Block.GetBlockValue(blockName);
                    Vector3i playerPos = new Vector3i(player.position);
                    if (!Vectors.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] You have no saved positions. Unable to spawn blocks");
                        return;
                    }
                    Vectors.TryGetValue(_senderInfo.RemoteClientInfo.entityId, out Vector3i[] vectors);
                    if (vectors[0] == null || vectors[1] == null)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] You are missing a saved position. Unable to spawn blocks");
                        return;
                    }
                    Vector3i corner1 = new Vector3i((vectors[0].x <= vectors[1].x) ? vectors[0].x : vectors[1].x, (vectors[0].y <= vectors[1].y) ? vectors[0].y : vectors[1].y, (vectors[0].z <= vectors[1].z) ? vectors[0].z : vectors[1].z);
                    Vector3i corner2 = new Vector3i((vectors[0].x <= vectors[1].x) ? vectors[1].x : vectors[0].x, (vectors[0].y <= vectors[1].y) ? vectors[1].y : vectors[0].y, (vectors[0].z <= vectors[1].z) ? vectors[1].z : vectors[0].z);
                    Dictionary<Vector3i, BlockValue> undo = new Dictionary<Vector3i, BlockValue>();
                    List<BlockChangeInfo> blockList = new List<BlockChangeInfo>();
                    for (int x = corner1.x; x <= corner2.x; x++)
                    {
                        for (int z = corner1.z; z <= corner2.z; z++)
                        {
                            for (int y = corner1.y; y <= corner2.y; y++)
                            {
                                Vector3i blockPosition = new Vector3i(x, y, z);
                                if (!world.IsChunkAreaLoaded(blockPosition.x, blockPosition.y, blockPosition.z))
                                {
                                    SdtdConsole.Instance.Output("[SERVERTOOLS] The blocks you are trying to spawn are outside of a loaded chunk area. Unable to spawn block");
                                    return;
                                }
                                blockList.Add(new BlockChangeInfo(0, blockPosition, blockValue));
                                BlockValue oldBlockValue = world.GetBlock(blockPosition);
                                undo.Add(blockPosition, oldBlockValue);
                            }
                        }
                    }
                    if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString))
                    {
                        Undo[_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString] = undo;
                    }
                    else
                    {
                        Undo.Add(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString, undo);
                    }
                    GameManager.Instance.SetBlocksRPC(blockList, null);
                    Timers.SABGenerationDelayTimer(blockList);
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Spawned active block. Double check integrity of block before continuing");
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SpawnActiveBlocks.Execute: {0}", e.Message));
            }
        }

        public static void Corrections(List<BlockChangeInfo> blockList)
        {
            try
            {
                World world = GameManager.Instance.World;
                List<BlockChangeInfo> blockCorrections = new List<BlockChangeInfo>();
                int listCount = blockList.Count;
                for (int i = 0; i < listCount; i++)
                {
                    BlockValue oldBlockValue = world.GetBlock(blockList[i].pos);
                    if (!oldBlockValue.Equals(blockList[i].blockValue))
                    {
                        blockCorrections.Add(blockList[i]);
                    }
                }
                GameManager.Instance.SetBlocksRPC(blockCorrections, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SpawnActiveBlocks.Corrections: {0}", e.Message));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class MazeConsole : ConsoleCmdAbstract
    {
        public static Dictionary<string, Dictionary<Vector3i, BlockValue>> Undo = new Dictionary<string, Dictionary<Vector3i, BlockValue>>();

        public override string GetDescription()
        {
            return "[ServerTools] - Generate and spawn a maze for players to run through.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-mz add {Blocks} {Floors}\n" +
                   "  2. st-mz add {Blocks} {Floors} {BlockName}\n" +
                   "  3. st-mz undo\n" +
                   "1. Generate a maze with this width of blocks and this many floors\n" +
                   "2. Generate a maze with this width of blocks, floors and inner block name that forms the walls" +
                   "3. Revert the maze last generated to the original blocks" +
                   "*Note*" +
                   "Undo command is limited to the user that spawned the maze. Server restarts remove the old data";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Maze", "mz", "st-mz" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 3 && _params.Count != 4)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 3 or 4, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count == 3 || _params.Count == 4)
                    {
                        if (int.TryParse(_params[1], out int blocks))
                        {
                            if (int.TryParse(_params[2], out int floors))
                            {
                                if (_senderInfo.RemoteClientInfo != null)
                                {
                                    if (blocks < 30)
                                    {
                                        blocks = 30;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Maze size is too small. Maze size increased to 30"));
                                    }
                                    else if (blocks > 120)
                                    {
                                        blocks = 120;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Maze size is too big. Maze size decreased to 120"));
                                    }
                                    if (floors < 1)
                                    {
                                        floors = 1;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Floor count is too low. Floor count set to 1"));
                                    }
                                    else if (floors > 10)
                                    {
                                        floors = 10;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Floor count is too high. Floor count decreased to 10"));
                                    }
                                    if (floors >= 2 && blocks > 110)
                                    {
                                        floors = 1;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 1"));
                                    }
                                    else if (floors >= 3 && blocks > 90)
                                    {
                                        floors = 2;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 2"));
                                    }
                                    else if (floors >= 4 && blocks > 80)
                                    {
                                        floors = 3;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 3"));
                                    }
                                    else if (floors >= 5 && blocks > 70)
                                    {
                                        floors = 4;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 4"));
                                    }
                                    else if (floors >= 6 && blocks > 65)
                                    {
                                        floors = 5;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 5"));
                                    }
                                    else if (floors >= 7 && blocks > 60)
                                    {
                                        floors = 6;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 6"));
                                    }
                                    else if (floors >= 8 && blocks > 55)
                                    {
                                        floors = 7;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 7"));
                                    }
                                    else if (floors >= 9 && blocks > 52)
                                    {
                                        floors = 8;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 8"));
                                    }
                                    else if (floors == 10 && blocks > 50)
                                    {
                                        floors = 9;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 9"));
                                    }
                                    World world = GameManager.Instance.World;
                                    EntityPlayer player = world.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                                    if (player != null)
                                    {
                                        if (player.position.y < 3)
                                        {
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Your position is too low. Unable to generate maze at this world height"));
                                            return;
                                        }
                                        BlockValue groundBlockValue = world.GetBlock(new Vector3i(player.position.x, player.position.y - 1, player.position.z));
                                        if (groundBlockValue.Equals(BlockValue.Air))
                                        {
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Air block detected under you. Unable to generate a maze at this position"));
                                            return;
                                        }
                                        Block steelFloor = Block.GetBlockByName("steelBlock", false);
                                        if (steelFloor != null)
                                        {
                                            Block concreteWalls = Block.GetBlockByName("concreteBlock", false);
                                            if (concreteWalls != null)
                                            {
                                                Block stoneFiller = Block.GetBlockByName("terrStone", false);
                                                if (stoneFiller != null)
                                                {
                                                    Block glassCeiling = Block.GetBlockByName("glassBusinessBlock", false);
                                                    if (glassCeiling != null)
                                                    {
                                                        Block glassBlock = Block.GetBlockByName("glassBulletproofBlock", false);
                                                        if (glassBlock != null)
                                                        {

                                                            Block ladder = Block.GetBlockByName("ladderMetal", false);
                                                            if (ladder != null)
                                                            {
                                                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Maze generation started at player position {0}. Please be patient", player.position));
                                                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Inspect the maze for potential collapse after it spawns"));
                                                                BlockValue steelBlockValue = Block.GetBlockValue("steelBlock");
                                                                BlockValue concreteBlockValue = Block.GetBlockValue("concreteBlock");
                                                                BlockValue stoneBlockValue = Block.GetBlockValue("terrStone");
                                                                BlockValue glassCeilingBlockValue = Block.GetBlockValue("glassBusinessBlock");
                                                                BlockValue glassBlockValue = Block.GetBlockValue("glassBulletproofBlock");
                                                                BlockValue ladderValue = Block.GetBlockValue("ladderMetal");
                                                                Vector3i templateVectors = new Vector3i();
                                                                templateVectors.x = (int)player.position.x - blocks / 2;
                                                                templateVectors.y = (int)player.position.y - 1;
                                                                templateVectors.z = (int)player.position.z + blocks / 2;
                                                                int firstTemplateVectorX = templateVectors.x;
                                                                int firstTemplateVectorZ = templateVectors.z;
                                                                Vector3i pathStart = new Vector3i(firstTemplateVectorX + 1, (int)player.position.y, firstTemplateVectorZ - 1);
                                                                Dictionary<Vector3i, int[]> mazeTemplate = new Dictionary<Vector3i, int[]>();
                                                                Dictionary<Vector3i, string> mazeForm = new Dictionary<Vector3i, string>();
                                                                Dictionary<Vector3i, BlockValue> undo = new Dictionary<Vector3i, BlockValue>();
                                                                BlockValue oldBlockValue = BlockValue.Air;
                                                                for (int i = 1; i <= blocks; i++)
                                                                {
                                                                    if (i > 1)
                                                                    {
                                                                        templateVectors.z--;
                                                                    }
                                                                    for (int j = 1; j <= blocks; j++)
                                                                    {
                                                                        if (j > 1)
                                                                        {
                                                                            templateVectors.x++;
                                                                        }
                                                                        if (world.IsChunkAreaLoaded(templateVectors.x, templateVectors.y, templateVectors.z))
                                                                        {
                                                                            mazeTemplate.Add(templateVectors, new int[] { i, j });
                                                                            mazeForm.Add(templateVectors, "steel");
                                                                        }
                                                                        else
                                                                        {
                                                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Part of the maze is outside of a loaded chunk area. Reduce the size of the maze"));
                                                                            return;
                                                                        }
                                                                    }
                                                                    templateVectors.x = firstTemplateVectorX;
                                                                }
                                                                if (_params.Count == 3)
                                                                {
                                                                    int _levels = floors * 3;
                                                                    for (int i = 1; i <= _levels; i++)
                                                                    {
                                                                        foreach (var _vector in mazeTemplate)
                                                                        {
                                                                            if (_vector.Value[0] == 1 || _vector.Value[0] == blocks || _vector.Value[1] == 1 || _vector.Value[1] == blocks)
                                                                            {
                                                                                mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "glassWall");
                                                                            }
                                                                            else if (i % 3 == 0)
                                                                            {
                                                                                if (i == _levels)
                                                                                {
                                                                                    mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "glass");
                                                                                }
                                                                                else
                                                                                {
                                                                                    mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "glassWall");
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "...");
                                                                            }
                                                                        }
                                                                    }
                                                                    mazeForm = FormPath(mazeForm, pathStart, blocks, floors);
                                                                    if (mazeForm == null)
                                                                    {
                                                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to form the maze. Try again"));
                                                                        return;
                                                                    }
                                                                    List<BlockChangeInfo> clearSpace = new List<BlockChangeInfo>();
                                                                    List<BlockChangeInfo> blockList = new List<BlockChangeInfo>();
                                                                    foreach (var block in mazeForm)
                                                                    {
                                                                        oldBlockValue = world.GetBlock(block.Key);
                                                                        undo.Add(block.Key, oldBlockValue);
                                                                        if (block.Value == "...")
                                                                        {
                                                                            clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, stoneBlockValue));
                                                                        }
                                                                        else if (block.Value == "steel")
                                                                        {
                                                                            clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, steelBlockValue));
                                                                        }
                                                                        else if (block.Value == "glassWall")
                                                                        {
                                                                            clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, glassBlockValue));
                                                                        }
                                                                        else if (block.Value == "wall")
                                                                        {
                                                                            clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, concreteBlockValue));
                                                                        }
                                                                        else if (block.Value == "wallPassage")
                                                                        {
                                                                            clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, concreteBlockValue));
                                                                        }
                                                                        else if (block.Value == "air")
                                                                        {
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                        }
                                                                        else if (block.Value == "path")
                                                                        {
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                        }
                                                                        else if (block.Value == "ladder1")
                                                                        {
                                                                            ladderValue.rotation = 1;
                                                                            clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, ladderValue));
                                                                        }
                                                                        else if (block.Value == "ladder2")
                                                                        {
                                                                            ladderValue.rotation = 2;
                                                                            clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, ladderValue));
                                                                        }
                                                                        else if (block.Value == "ladder3")
                                                                        {
                                                                            ladderValue.rotation = 3;
                                                                            clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, ladderValue));
                                                                        }
                                                                        else if (block.Value == "ladder4")
                                                                        {
                                                                            ladderValue.rotation = 4;
                                                                            clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, ladderValue));
                                                                        }
                                                                        else if (block.Value == "glass")
                                                                        {
                                                                            clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            blockList.Add(new BlockChangeInfo(0, block.Key, glassCeilingBlockValue));
                                                                        }
                                                                    }
                                                                    GameManager.Instance.SetBlocksRPC(clearSpace, null);
                                                                    GameManager.Instance.SetBlocksRPC(blockList, null);
                                                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Maze has been formed. The start of the maze is at {0}", pathStart));
                                                                }
                                                                else if (_params.Count == 4)
                                                                {
                                                                    Block customWall = Block.GetBlockByName(_params[3], false);
                                                                    if (customWall != null)
                                                                    {
                                                                        BlockValue customBlockValue = Block.GetBlockValue(_params[3]);
                                                                        int levels = floors * 3;
                                                                        for (int i = 1; i <= levels; i++)
                                                                        {
                                                                            foreach (var vector in mazeTemplate)
                                                                            {
                                                                                if (vector.Value[0] == 1 || vector.Value[0] == blocks || vector.Value[1] == 1 || vector.Value[1] == blocks)
                                                                                {
                                                                                    mazeForm.Add(new Vector3i(vector.Key.x, vector.Key.y + i, vector.Key.z), "glassWall");
                                                                                }
                                                                                else if (i % 3 == 0)
                                                                                {
                                                                                    if (i == levels)
                                                                                    {
                                                                                        mazeForm.Add(new Vector3i(vector.Key.x, vector.Key.y + i, vector.Key.z), "glass");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        mazeForm.Add(new Vector3i(vector.Key.x, vector.Key.y + i, vector.Key.z), "glassWall");
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    mazeForm.Add(new Vector3i(vector.Key.x, vector.Key.y + i, vector.Key.z), "...");
                                                                                }
                                                                            }
                                                                        }
                                                                        mazeForm = FormPath(mazeForm, pathStart, blocks, floors);
                                                                        if (mazeForm == null)
                                                                        {
                                                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to form the maze. Try again"));
                                                                            return;
                                                                        }
                                                                        List<BlockChangeInfo> clearSpace = new List<BlockChangeInfo>();
                                                                        List<BlockChangeInfo> blockList = new List<BlockChangeInfo>();
                                                                        foreach (var block in mazeForm)
                                                                        {
                                                                            oldBlockValue = world.GetBlock(block.Key);
                                                                            undo.Add(block.Key, oldBlockValue);
                                                                            if (block.Value == "...")
                                                                            {
                                                                                clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, stoneBlockValue));
                                                                            }
                                                                            else if (block.Value == "steel")
                                                                            {
                                                                                clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, steelBlockValue));
                                                                            }
                                                                            else if (block.Value == "glassWall")
                                                                            {
                                                                                clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, glassBlockValue));
                                                                            }
                                                                            else if (block.Value == "wall")
                                                                            {
                                                                                clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, concreteBlockValue));
                                                                            }
                                                                            else if (block.Value == "wallPassage")
                                                                            {
                                                                                clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, concreteBlockValue));
                                                                            }
                                                                            else if (block.Value == "air")
                                                                            {
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            }
                                                                            else if (block.Value == "path")
                                                                            {
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                            }
                                                                            else if (block.Value == "ladder1")
                                                                            {
                                                                                ladderValue.rotation = 1;
                                                                                clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, ladderValue));
                                                                            }
                                                                            else if (block.Value == "ladder2")
                                                                            {
                                                                                ladderValue.rotation = 2;
                                                                                clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, ladderValue));
                                                                            }
                                                                            else if (block.Value == "ladder3")
                                                                            {
                                                                                ladderValue.rotation = 3;
                                                                                clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, ladderValue));
                                                                            }
                                                                            else if (block.Value == "ladder4")
                                                                            {
                                                                                ladderValue.rotation = 4;
                                                                                clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, ladderValue));
                                                                            }
                                                                            else if (block.Value == "glass")
                                                                            {
                                                                                clearSpace.Add(new BlockChangeInfo(0, block.Key, BlockValue.Air));
                                                                                blockList.Add(new BlockChangeInfo(0, block.Key, glassCeilingBlockValue));
                                                                            }
                                                                        }
                                                                        GameManager.Instance.SetBlocksRPC(clearSpace, null);
                                                                        GameManager.Instance.SetBlocksRPC(blockList, null);
                                                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Maze has been spawned. The start of the maze is at {0}", pathStart));
                                                                    }
                                                                    else
                                                                    {
                                                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: {0}", _params[3]));
                                                                        return;
                                                                    }
                                                                }
                                                                if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier))
                                                                {
                                                                    Undo[_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier] = undo;
                                                                }
                                                                else
                                                                {
                                                                    Undo.Add(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier, undo);
                                                                }
                                                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Use command maze undo to reset the maze space"));
                                                                return;
                                                            }
                                                            else
                                                            {
                                                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: ladderMetal"));
                                                                return;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: glassBulletproofBlock"));
                                                            return;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: glassBusinessBlock"));
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: terrStone"));
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: concreteBlock"));
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: steelBlock"));
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to form player info for world position. Join the game first or check for errors"));
                                        return;
                                    }
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to form client info for world position. Join the game first or check for errors"));
                                    return;
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid number of floors: {0}", _params[2]));
                                return;
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid width of blocks: {0}", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 4 or 5, found {0}", _params.Count));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("undo"))
                {
                    if (_params.Count == 1)
                    {
                        if (_senderInfo.RemoteClientInfo != null)
                        {
                            if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier))
                            {
                                Undo.TryGetValue(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier, out Dictionary<Vector3i, BlockValue> undo);
                                World world = GameManager.Instance.World;
                                foreach (var block in undo)
                                {
                                    if (!world.IsChunkAreaLoaded(block.Key.x, block.Key.y, block.Key.z))
                                    {
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Area is not loaded @ {0}. Unable to undo maze blocks", block.Key));
                                        return;
                                    }
                                }
                                List<BlockChangeInfo> blockList = new List<BlockChangeInfo>();
                                foreach (var block in undo)
                                {
                                    blockList.Add(new BlockChangeInfo(0, block.Key, block.Value));
                                }
                                GameManager.Instance.SetBlocksRPC(blockList, null);
                                Undo.Remove(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The maze you last spawned has been undone"));
                                return;
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] You have not spawned a maze. Unable to undo"));
                                return;
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to form client info to run undo command"));
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MazeConsole.Execute: {0}", e.Message));
            }
        }

        public static Dictionary<Vector3i, string> FormPath(Dictionary<Vector3i, string> _mazeForm, Vector3i _startingVector, int _blocks, int _floors)
        {
            try
            {
                Vector3i currentPathVector = _startingVector, nextPathVector, startPathVector = _startingVector, endPathVector = Vector3i.zero, path1 = _startingVector, path2 = _startingVector,
                    path3 = _startingVector, path4 = _startingVector; //pathing vectors
                List<Vector3i> random = new List<Vector3i>(); //potential movements, random pick
                List<Vector3i> pathIndex = new List<Vector3i>(); //alternate paths
                Dictionary<int, Vector3i> pathLength = new Dictionary<int, Vector3i>();
                int maxPath = _blocks * _blocks, endPathLength = 0, currentPathLength = 0;
                for (int i = 1; i <= _floors; i++)
                {
                    _mazeForm[currentPathVector] = "air";
                    _mazeForm[new Vector3i(currentPathVector.x, currentPathVector.y + 1, currentPathVector.z)] = "air";
                    for (int j = 1; j <= maxPath; j++)
                    {
                        path1.x--;
                        path2.x++;
                        path3.z--;
                        path4.z++;
                        if (_mazeForm.ContainsKey(path1))
                        {
                            _mazeForm.TryGetValue(path1, out string blockName);
                            if (blockName == "...")
                            {
                                random.Add(path1);
                            }
                            else if (blockName == "wallPassage")
                            {
                                _mazeForm[path1] = "wall";
                            }
                        }
                        if (_mazeForm.ContainsKey(path2))
                        {
                            _mazeForm.TryGetValue(path2, out string blockName);
                            if (blockName == "...")
                            {
                                random.Add(path2);
                            }
                            else if (blockName == "wallPassage")
                            {
                                _mazeForm[path2] = "wall";
                            }
                        }
                        if (_mazeForm.ContainsKey(path3))
                        {
                            _mazeForm.TryGetValue(path3, out string blockName);
                            if (blockName == "...")
                            {
                                random.Add(path3);
                            }
                            else if (blockName == "wallPassage")
                            {
                                _mazeForm[path3] = "wall";
                            }
                        }
                        if (_mazeForm.ContainsKey(path4))
                        {
                            _mazeForm.TryGetValue(path4, out string blockName);
                            if (blockName == "...")
                            {
                                random.Add(path4);
                            }
                            else if (blockName == "wallPassage")
                            {
                                _mazeForm[path4] = "wall";
                            }
                        }
                        if (random.Count > 1) //multiple path found
                        {
                            pathIndex.Add(currentPathVector);
                            random.RandomizeList();
                            nextPathVector = random[0];
                            random.RemoveAt(0);
                            _mazeForm[nextPathVector] = "air";
                            _mazeForm[new Vector3i(nextPathVector.x, nextPathVector.y + 1, nextPathVector.z)] = "air";
                            for (int k = 0; k < random.Count; k++)
                            {
                                _mazeForm[random[k]] = "wallPassage";
                                _mazeForm[new Vector3i(random[k].x, random[k].y + 1, random[k].z)] = "wallPassage";
                            }
                            random.Clear();
                            path1 = nextPathVector;
                            path2 = nextPathVector;
                            path3 = nextPathVector;
                            path4 = nextPathVector;
                            currentPathVector = nextPathVector;
                        }
                        else if (random.Count == 1) //one path found
                        {
                            nextPathVector = random[0];
                            _mazeForm[nextPathVector] = "air";
                            _mazeForm[new Vector3i(nextPathVector.x, nextPathVector.y + 1, nextPathVector.z)] = "air";
                            random.Clear();
                            path1 = nextPathVector;
                            path2 = nextPathVector;
                            path3 = nextPathVector;
                            path4 = nextPathVector;
                            currentPathVector = nextPathVector;
                        }
                        else //no path found
                        {
                            if (pathIndex.Count > 0)
                            {
                                pathIndex.Reverse();
                                for (int k = 0; k < pathIndex.Count; k++)
                                {
                                    path1 = pathIndex[k];
                                    path2 = pathIndex[k];
                                    path3 = pathIndex[k];
                                    path4 = pathIndex[k];
                                    path1.x--;
                                    path2.x++;
                                    path3.z--;
                                    path4.z++;
                                    if (_mazeForm.ContainsKey(path1))
                                    {
                                        _mazeForm.TryGetValue(path1, out string blockName);
                                        if (blockName == "wallPassage")
                                        {
                                            random.Add(path1);
                                        }
                                    }
                                    if (_mazeForm.ContainsKey(path2))
                                    {
                                        _mazeForm.TryGetValue(path2, out string blockName);
                                        if (blockName == "wallPassage")
                                        {
                                            random.Add(path2);
                                        }
                                    }
                                    if (_mazeForm.ContainsKey(path3))
                                    {
                                        _mazeForm.TryGetValue(path3, out string blockName);
                                        if (blockName == "wallPassage")
                                        {
                                            random.Add(path3);
                                        }
                                    }
                                    if (_mazeForm.ContainsKey(path4))
                                    {
                                        _mazeForm.TryGetValue(path4, out string blockName);
                                        if (blockName == "wallPassage")
                                        {
                                            random.Add(path4);
                                        }
                                    }
                                    if (random.Count > 1)
                                    {
                                        random.RandomizeList();
                                        nextPathVector = random[0];
                                        random.Clear();
                                        _mazeForm[nextPathVector] = "air";
                                        _mazeForm[new Vector3i(nextPathVector.x, nextPathVector.y + 1, nextPathVector.z)] = "air";
                                        pathIndex.Reverse();
                                        path1 = nextPathVector;
                                        path2 = nextPathVector;
                                        path3 = nextPathVector;
                                        path4 = nextPathVector;
                                        currentPathVector = nextPathVector;
                                        break;
                                    }
                                    else if (random.Count == 1)
                                    {
                                        nextPathVector = random[0];
                                        random.Clear();
                                        _mazeForm[nextPathVector] = "air";
                                        _mazeForm[new Vector3i(nextPathVector.x, nextPathVector.y + 1, nextPathVector.z)] = "air";
                                        pathIndex.Remove(currentPathVector);
                                        pathIndex.Reverse();
                                        path1 = nextPathVector;
                                        path2 = nextPathVector;
                                        path3 = nextPathVector;
                                        path4 = nextPathVector;
                                        currentPathVector = nextPathVector;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    currentPathVector = startPathVector;
                    _mazeForm[currentPathVector] = "path";
                    _mazeForm[new Vector3i(currentPathVector.x, currentPathVector.y + 1, currentPathVector.z)] = "path";
                    path1 = currentPathVector;
                    path2 = currentPathVector;
                    path3 = currentPathVector;
                    path4 = currentPathVector;
                    for (int j = 0; j < maxPath; j++)
                    {
                        path1.x--;
                        path2.x++;
                        path3.z--;
                        path4.z++;
                        if (_mazeForm.ContainsKey(path1))
                        {
                            _mazeForm.TryGetValue(path1, out string blockName);
                            if (blockName == "air")
                            {
                                random.Add(path1);
                            }
                        }
                        if (_mazeForm.ContainsKey(path2))
                        {
                            _mazeForm.TryGetValue(path2, out string blockName);
                            if (blockName == "air")
                            {
                                random.Add(path2);
                            }
                        }
                        if (_mazeForm.ContainsKey(path3))
                        {
                            _mazeForm.TryGetValue(path3, out string blockName);
                            if (blockName == "air")
                            {
                                random.Add(path3);
                            }
                        }
                        if (_mazeForm.ContainsKey(path4))
                        {
                            _mazeForm.TryGetValue(path4, out string blockName);
                            if (blockName == "air")
                            {
                                random.Add(path4);
                            }
                        }
                        if (random.Count > 1)
                        {
                            random.RandomizeList();
                            nextPathVector = random[0];
                            random.Clear();
                            _mazeForm[nextPathVector] = "path";
                            _mazeForm[new Vector3i(nextPathVector.x, nextPathVector.y + 1, nextPathVector.z)] = "path";
                            pathLength.Add(j, currentPathVector);
                            path1 = nextPathVector;
                            path2 = nextPathVector;
                            path3 = nextPathVector;
                            path4 = nextPathVector;
                            currentPathVector = nextPathVector;
                            currentPathLength++;
                        }
                        else if (random.Count == 1)
                        {
                            nextPathVector = random[0];
                            random.Clear();
                            _mazeForm[nextPathVector] = "path";
                            _mazeForm[new Vector3i(nextPathVector.x, nextPathVector.y + 1, nextPathVector.z)] = "path";
                            path1 = nextPathVector;
                            path2 = nextPathVector;
                            path3 = nextPathVector;
                            path4 = nextPathVector;
                            currentPathVector = nextPathVector;
                            currentPathLength++;
                        }
                        else
                        {
                            if (currentPathLength > endPathLength)
                            {
                                endPathVector = currentPathVector;
                                endPathLength = currentPathLength;
                            }
                            if (pathLength.Count > 0)
                            {
                                KeyValuePair<int, Vector3i> pathPoint = pathLength.ElementAt(0);
                                pathLength.Remove(pathPoint.Key);
                                path1 = pathPoint.Value;
                                path2 = pathPoint.Value;
                                path3 = pathPoint.Value;
                                path4 = pathPoint.Value;
                                currentPathVector = pathPoint.Value;
                                currentPathLength = pathPoint.Key;
                            }
                            else
                            {
                                currentPathLength = 0;
                                endPathLength = 0;
                                break;
                            }
                        }
                    }
                    if (endPathVector != Vector3i.zero)
                    {
                        bool pathUp = false;
                        path1 = endPathVector;
                        path2 = endPathVector;
                        path3 = endPathVector;
                        path4 = endPathVector;
                        path1.x--;
                        path2.x++;
                        path3.z--;
                        path4.z++;
                        if (!pathUp)
                        {
                            if (_mazeForm.ContainsKey(path1))
                            {
                                _mazeForm.TryGetValue(path1, out string blockName);
                                if (blockName == "wall" || blockName == "wallPassage")
                                {
                                    _mazeForm[endPathVector] = "ladder1";
                                    _mazeForm[new Vector3i(endPathVector.x, endPathVector.y + 1, endPathVector.z)] = "ladder1";
                                    _mazeForm[new Vector3i(endPathVector.x, endPathVector.y + 2, endPathVector.z)] = "ladder1";
                                    pathUp = true;
                                }
                            }
                        }
                        if (!pathUp)
                        {
                            if (_mazeForm.ContainsKey(path2))
                            {
                                _mazeForm.TryGetValue(path2, out string blockName);
                                if (blockName == "wall" || blockName == "wallPassage")
                                {
                                    _mazeForm[endPathVector] = "ladder3";
                                    _mazeForm[new Vector3i(endPathVector.x, endPathVector.y + 1, endPathVector.z)] = "ladder3";
                                    _mazeForm[new Vector3i(endPathVector.x, endPathVector.y + 2, endPathVector.z)] = "ladder3";
                                    pathUp = true;
                                }
                            }
                        }
                        if (!pathUp)
                        {
                            if (_mazeForm.ContainsKey(path3))
                            {
                                _mazeForm.TryGetValue(path3, out string blockName);
                                if (blockName == "wall" || blockName == "wallPassage")
                                {
                                    _mazeForm[endPathVector] = "ladder4";
                                    _mazeForm[new Vector3i(endPathVector.x, endPathVector.y + 1, endPathVector.z)] = "ladder4";
                                    _mazeForm[new Vector3i(endPathVector.x, endPathVector.y + 2, endPathVector.z)] = "ladder4";
                                    pathUp = true;
                                }
                            }
                        }
                        if (!pathUp)
                        {
                            if (_mazeForm.ContainsKey(path4))
                            {
                                _mazeForm.TryGetValue(path4, out string blockName);
                                if (blockName == "wall" || blockName == "wallPassage")
                                {
                                    _mazeForm[endPathVector] = "ladder2";
                                    _mazeForm[new Vector3i(endPathVector.x, endPathVector.y + 1, endPathVector.z)] = "ladder2";
                                    _mazeForm[new Vector3i(endPathVector.x, endPathVector.y + 2, endPathVector.z)] = "ladder2";
                                }
                            }
                        }
                        startPathVector = new Vector3i(endPathVector.x, endPathVector.y + 3, endPathVector.z);
                        currentPathVector = startPathVector;
                        path1 = startPathVector;
                        path2 = startPathVector;
                        path3 = startPathVector;
                        path4 = startPathVector;
                    }
                }
                return _mazeForm;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MazeConsole.FormPath: {0}", e.Message));
            }
            return null;
        }
    }
}

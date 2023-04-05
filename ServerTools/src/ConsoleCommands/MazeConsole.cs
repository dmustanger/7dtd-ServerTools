using System;
using System.Collections;
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
                   "2. Generate a maze with this width of blocks and this many floors using a particular block for the inner walls\n" +
                   "3. Revert the maze last generated to the original blocks" +
                   "*Note*" +
                   "Undo command is limited to the user that spawned the maze. Server shutdown will clear the undo data" +
                   "Use wood, cobblestone, steel, brick or glass for the block name. It will default to concrete if the block can not be found";
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
                    ThreadManager.AddSingleTask(delegate (ThreadManager.TaskInfo _info)
                    {
                        if (_params.Count != 3 && _params.Count != 4)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3 or 4, found {0}", _params.Count));
                            return;
                        }
                        if (!int.TryParse(_params[1], out int blocks))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid width of blocks: {0}", _params[1]));
                            return;
                        }
                        if (!int.TryParse(_params[2], out int floors))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid number of floors: {0}", _params[2]));
                            return;
                        }
                        if (_senderInfo.RemoteClientInfo == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to form client info for world position. Join the game first or check for errors"));
                            return;
                        }
                        if (blocks < 30)
                        {
                            blocks = 30;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Maze size is too small. Maze size increased to 30x30"));
                        }
                        else if (blocks > 120)
                        {
                            blocks = 120;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Maze size is too big. Maze size decreased to 120x120"));
                        }
                        if (floors < 1)
                        {
                            floors = 1;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Floor count is too low. Floor count set to 1"));
                        }
                        else if (floors > 11)
                        {
                            floors = 11;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Floor count is too high. Floor count decreased to 11"));
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
                        else if (floors >= 9 && blocks > 50)
                        {
                            floors = 8;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 8"));
                        }
                        else if (floors >= 10 && blocks > 45)
                        {
                            floors = 9;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 9"));
                        }
                        else if (floors == 11 && blocks > 35)
                        {
                            floors = 10;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 10"));
                        }
                        World world = GameManager.Instance.World;
                        EntityPlayer player = world.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                        if (player == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to form player info for world position. Join the game first or check for errors"));
                            return;
                        }
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
                        Block steelFloors = Block.GetBlockByName("steelShapes:cube");
                        if (steelFloors == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: steelShapes"));
                            return;
                        }
                        Block concreteWalls = Block.GetBlockByName("concreteShapes:cube");
                        if (concreteWalls == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: concreteShapes"));
                            return;
                        }
                        Block stoneFiller = Block.GetBlockByName("terrStone");
                        if (stoneFiller == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: terrStone"));
                            return;
                        }
                        Block glassCeiling = Block.GetBlockByName("glassBusinessBlock");
                        if (glassCeiling == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: glassBusinessBlock"));
                            return;
                        }
                        Block glassBlock = Block.GetBlockByName("glassBulletproofBlock");
                        if (glassBlock == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: glassBulletproofBlock"));
                            return;
                        }
                        Block ladder = Block.GetBlockByName("ladderMetal");
                        if (ladder == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: ladderMetal"));
                            return;
                        }
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Maze generator started at position '{0}'. Please be patient", player.position));
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Inspect the maze for potential collapse after it spawns"));
                        BlockValue steelBlockValue = Block.GetBlockValue("steelShapes:cube");
                        BlockValue concreteBlockValue = Block.GetBlockValue("concreteShapes:cube");
                        BlockValue stoneBlockValue = Block.GetBlockValue("terrStone");
                        BlockValue glassCeilingBlockValue = Block.GetBlockValue("glassBusinessBlock");
                        BlockValue glassBlockValue = Block.GetBlockValue("glassBulletproofBlock");
                        BlockValue ladderValue = Block.GetBlockValue("ladderMetal");
                        if (_params.Count == 4)
                        {
                            switch (_params[3].ToLower())
                            {
                                case "wood":
                                    Block customBlock = Block.GetBlockByName("woodShapes:cube");
                                    if (customBlock != null)
                                    {
                                        concreteBlockValue = Block.GetBlockValue("woodShapes:cube");
                                    }
                                    break;
                                case "cobblestone":
                                    customBlock = Block.GetBlockByName("cobblestoneShapes:cube");
                                    if (customBlock != null)
                                    {
                                        concreteBlockValue = Block.GetBlockValue("cobblestoneShapes:cube");
                                    }
                                    break;
                                case "steel":
                                    customBlock = Block.GetBlockByName("steelShapes:cube");
                                    if (customBlock != null)
                                    {
                                        concreteBlockValue = Block.GetBlockValue("steelShapes:cube");
                                    }
                                    break;
                                case "brick":
                                    customBlock = Block.GetBlockByName("brickShapes:cube");
                                    if (customBlock != null)
                                    {
                                        concreteBlockValue = Block.GetBlockValue("brickShapes:cube");
                                    }
                                    break;
                                case "glass":
                                    concreteBlockValue = Block.GetBlockValue("glassBusinessBlock");
                                    break;
                            }
                        }
                        Vector3i vectors = new Vector3i();
                        vectors.x = (int)player.position.x - blocks / 2;
                        vectors.y = (int)player.position.y - 1;
                        vectors.z = (int)player.position.z - blocks / 2;
                        Vector3i pathStart = new Vector3i(vectors.x + 1, (int)player.position.y, vectors.z + 1);
                        Dictionary<Vector3i, int[]> template = new Dictionary<Vector3i, int[]>();
                        Dictionary<Vector3i, string> blockValues = new Dictionary<Vector3i, string>();
                        Dictionary<Vector3i, BlockValue> undo = new Dictionary<Vector3i, BlockValue>();
                        List<Chunk> chunks = new List<Chunk>();
                        BlockValue oldBlockValue = BlockValue.Air;
                        for (int i = 0; i < blocks; i++)
                        {
                            for (int j = 0; j < blocks; j++)
                            {
                                if (world.IsChunkAreaLoaded(vectors.x, vectors.y, vectors.z))
                                {
                                    Chunk chunk = (Chunk)world.GetChunkFromWorldPos(vectors.x, vectors.y, vectors.z);
                                    if (!chunks.Contains(chunk))
                                    {
                                        chunks.Add(chunk);
                                    }
                                    template.Add(vectors, new int[] { i, j });
                                    blockValues.Add(vectors, "steel");
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Part of the maze is outside of a loaded chunk area. Reduce the size of the maze"));
                                    return;
                                }
                                vectors.z++;
                            }
                            vectors.z -= blocks;
                            vectors.x++;
                        }
                        vectors.x -= blocks;
                        vectors.z -= blocks;
                        int levels = floors * 3;
                        for (int i = 1; i <= levels; i++)
                        {
                            foreach (var vector in template)
                            {
                                if (vector.Value[0] == 0 || vector.Value[1] == 0 || vector.Value[0] == blocks - 1 || vector.Value[1] == blocks - 1)
                                {
                                    blockValues.Add(new Vector3i(vector.Key.x, vector.Key.y + i, vector.Key.z), "glassWall");
                                }
                                else if (i % 3 == 0)
                                {
                                    if (i == levels)
                                    {
                                        blockValues.Add(new Vector3i(vector.Key.x, vector.Key.y + i, vector.Key.z), "glass");
                                    }
                                    else
                                    {
                                        blockValues.Add(new Vector3i(vector.Key.x, vector.Key.y + i, vector.Key.z), "glassWall");
                                    }
                                }
                                else
                                {
                                    blockValues.Add(new Vector3i(vector.Key.x, vector.Key.y + i, vector.Key.z), "...");
                                }
                            }
                        }
                        blockValues[new Vector3i(pathStart.x - 1, pathStart.y, pathStart.z)] = "air";
                        blockValues[new Vector3i(pathStart.x - 1, pathStart.y + 1, pathStart.z)] = "air";
                        blockValues = FormPath(blockValues, pathStart, blocks, floors);
                        if (blockValues == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to form the maze. Try again"));
                            return;
                        }
                        List<BlockChangeInfo> blockList = new List<BlockChangeInfo>();
                        foreach (var blockValue in blockValues)
                        {
                            oldBlockValue = world.GetBlock(blockValue.Key);
                            undo.Add(blockValue.Key, oldBlockValue);
                            switch (blockValue.Value)
                            {
                                case "...":
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, stoneBlockValue));
                                    break;
                                case "steel":
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, steelBlockValue));
                                    break;
                                case "glassWall":
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, glassBlockValue));
                                    break;
                                case "wall":
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, concreteBlockValue));
                                    break;
                                case "wallPassage":
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, concreteBlockValue));
                                    break;
                                case "air":
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, BlockValue.Air));
                                    break;
                                case "path":
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, BlockValue.Air));
                                    break;
                                case "ladder1":
                                    ladderValue.rotation = 1;
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, ladderValue));
                                    break;
                                case "ladder2":
                                    ladderValue.rotation = 2;
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, ladderValue));
                                    break;
                                case "ladder3":
                                    ladderValue.rotation = 3;
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, ladderValue));
                                    break;
                                case "ladder4":
                                    ladderValue.rotation = 4;
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, ladderValue));
                                    break;
                                case "glass":
                                    ladderValue.rotation = 4;
                                    blockList.Add(new BlockChangeInfo(0, blockValue.Key, glassCeilingBlockValue));
                                    break;
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
                        if (blockList.Count >= 3600)
                        {
                            List<BlockChangeInfo> reducedforPacket = blockList.GetRange(3600, blockList.Count - 1);
                            blockList.RemoveRange(3600, blockList.Count - 1);
                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate (ThreadManager.TaskInfo _taskInfo)
                            {
                                ThreadManager.StartCoroutine(SetBlocks(blockList));
                            }, null);
                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate (ThreadManager.TaskInfo _taskInfo)
                            {
                                ThreadManager.StartCoroutine(SetBlocks(reducedforPacket));
                            }, null);
                        }
                        else
                        {
                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate (ThreadManager.TaskInfo _taskInfo)
                            {
                                ThreadManager.StartCoroutine(SetBlocks(blockList));
                            }, null);
                        }
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Maze has been formed. One entrance/exit is on top and the other is in the south west corner"));
                        Timers.MazeGenerationDelayTimer(blockList);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Use command maze undo to reset the blocks to their prior state"));
                        return;
                    }, null, null, true);
                }
                else if (_params[0].ToLower().Equals("undo"))
                {
                    ThreadManager.AddSingleTask(delegate (ThreadManager.TaskInfo _info)
                    {
                        if (_params.Count != 1)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                            return;
                        }
                        if (_senderInfo.RemoteClientInfo == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to form client info to run undo command"));
                            return;
                        }
                        if (!Undo.ContainsKey(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] You have not spawned a maze. Unable to undo"));
                            return;
                        }
                        Undo.TryGetValue(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString, out Dictionary<Vector3i, BlockValue> undo);
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
                        if (blockList.Count >= 3600)
                        {
                            List<BlockChangeInfo> reducedforPacket = blockList.GetRange(3600, blockList.Count - 1);
                            blockList.RemoveRange(3600, blockList.Count - 1);
                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate (ThreadManager.TaskInfo _taskInfo)
                            {
                                ThreadManager.StartCoroutine(SetBlocks(blockList));
                            }, null);
                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate (ThreadManager.TaskInfo _taskInfo)
                            {
                                ThreadManager.StartCoroutine(SetBlocks(reducedforPacket));
                            }, null);
                        }
                        else
                        {
                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate (ThreadManager.TaskInfo _taskInfo)
                            {
                                ThreadManager.StartCoroutine(SetBlocks(blockList));
                            }, null);
                        }
                        Undo.Remove(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The maze you last spawned has been undone"));
                        return;
                    }, null, null, true);
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
                ThreadManager.AddSingleTaskMainThread("Coroutine", delegate (ThreadManager.TaskInfo _taskInfo)
                {
                    ThreadManager.StartCoroutine(SetBlocks(blockCorrections));
                }, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MazeConsole.Corrections: {0}", e.Message));
            }
        }

        public static IEnumerator SetBlocks(List<BlockChangeInfo> blockChanges)
        {
            try
            {
                if (blockChanges != null)
                {
                    GameManager.Instance.SetBlocksRPC(blockChanges, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MazeConsole.SetBlocks: {0}", e.StackTrace));
            }
            yield break;
        }
    }
}

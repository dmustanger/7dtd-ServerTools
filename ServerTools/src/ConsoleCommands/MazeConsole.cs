using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
                   "1. Generate a maze with this width of blocks, floors\n" +
                   "2. Generate a maze with this width of blocks, floors and inner block name that forms the walls" +
                   "3. Revert the maze last generated to the original blocks" +
                   "*Note*" +
                   "Undo command is limited to the user that spawned the maze. Server restarts remove the old data" +
                   "Difficulty: Easy 1, Medium 2, Hard 3. Changes the length of path";
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
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 3 or 4, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count == 3 || _params.Count == 4)
                    {
                        if (int.TryParse(_params[1], out int _blocks))
                        {
                            if (int.TryParse(_params[2], out int _floors))
                            {
                                if (_senderInfo.RemoteClientInfo != null)
                                {
                                    if (_blocks < 30)
                                    {
                                        _blocks = 30;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Maze size is too small. Maze size increased to 30"));
                                    }
                                    else if (_blocks > 120)
                                    {
                                        _blocks = 120;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Maze size is too big. Maze size decreased to 120"));
                                    }
                                    if (_floors < 1)
                                    {
                                        _floors = 1;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Floor count is too low. Floor count set to 1"));
                                    }
                                    else if (_floors > 10)
                                    {
                                        _floors = 10;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Floor count is too high. Floor count increased to 10"));
                                    }
                                    if (_floors >= 2 && _blocks > 110)
                                    {
                                        _floors = 1;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 1"));
                                    }
                                    else if (_floors >= 3 && _blocks > 90)
                                    {
                                        _floors = 2;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 2"));
                                    }
                                    else if (_floors >= 4 && _blocks > 80)
                                    {
                                        _floors = 3;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 3"));
                                    }
                                    else if (_floors >= 5 && _blocks > 70)
                                    {
                                        _floors = 4;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 4"));
                                    }
                                    else if (_floors >= 6 && _blocks > 65)
                                    {
                                        _floors = 5;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 5"));
                                    }
                                    else if (_floors >= 7 && _blocks > 60)
                                    {
                                        _floors = 6;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 6"));
                                    }
                                    else if (_floors >= 8 && _blocks > 55)
                                    {
                                        _floors = 7;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 7"));
                                    }
                                    else if (_floors >= 9 && _blocks > 52)
                                    {
                                        _floors = 8;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 8"));
                                    }
                                    else if (_floors == 10 && _blocks > 50)
                                    {
                                        _floors = 9;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Total block count is too high. Floor count decreased to 9"));
                                    }
                                    World _world = GameManager.Instance.World;
                                    EntityPlayer _player = _world.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                                    if (_player != null)
                                    {
                                        if (_player.position.y < 3)
                                        {
                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Your position is too low. Unable to generate maze at this world height"));
                                            return;
                                        }
                                        BlockValue _groundBlockValue = _world.GetBlock(new Vector3i(_player.position.x, _player.position.y - 1, _player.position.z));
                                        if (_groundBlockValue.Equals(BlockValue.Air))
                                        {
                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Air block detected under you. Unable to generate a maze at this position"));
                                            return;
                                        }
                                        Block _steelFloor = Block.GetBlockByName("steelBlock", false);
                                        if (_steelFloor != null)
                                        {
                                            Block _concreteWalls = Block.GetBlockByName("concreteBlock", false);
                                            if (_concreteWalls != null)
                                            {
                                                Block _stoneFiller = Block.GetBlockByName("terrStone", false);
                                                if (_stoneFiller != null)
                                                {
                                                    Block _glassCeiling = Block.GetBlockByName("glassBusinessBlock", false);
                                                    if (_glassCeiling != null)
                                                    {
                                                        Block _glassBlock = Block.GetBlockByName("glassBulletproofBlock", false);
                                                        if (_glassBlock != null)
                                                        {

                                                            Block _ladder = Block.GetBlockByName("ladderMetal", false);
                                                            if (_ladder != null)
                                                            {
                                                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Maze generation started at player position {0}. Please be patient", _player.position));
                                                                SdtdConsole.Instance.Output(string.Format("Inspect the maze for potential collapse after it spawns"));
                                                                BlockValue _steelBlockValue = Block.GetBlockValue("steelBlock");
                                                                BlockValue _concreteBlockValue = Block.GetBlockValue("concreteBlock");
                                                                BlockValue _stoneBlockValue = Block.GetBlockValue("terrStone");
                                                                BlockValue _glassCeilingBlockValue = Block.GetBlockValue("glassBusinessBlock");
                                                                BlockValue _glassBlockValue = Block.GetBlockValue("glassBulletproofBlock");
                                                                BlockValue _ladderValue = Block.GetBlockValue("ladderMetal");
                                                                Vector3i _templateVectors = new Vector3i();
                                                                _templateVectors.x = (int)_player.position.x - _blocks / 2;
                                                                _templateVectors.y = (int)_player.position.y - 1;
                                                                _templateVectors.z = (int)_player.position.z + _blocks / 2;
                                                                int _firstTemplateVectorX = _templateVectors.x;
                                                                int _firstTemplateVectorZ = _templateVectors.z;
                                                                Vector3i _pathStart = new Vector3i(_firstTemplateVectorX + 1, (int)_player.position.y, _firstTemplateVectorZ - 1);
                                                                Dictionary<Vector3i, int[]> _mazeTemplate = new Dictionary<Vector3i, int[]>();
                                                                Dictionary<Vector3i, string> _mazeForm = new Dictionary<Vector3i, string>();
                                                                Dictionary<Vector3i, BlockValue> _undo = new Dictionary<Vector3i, BlockValue>();
                                                                BlockValue _oldBlockValue = BlockValue.Air;
                                                                for (int i = 1; i <= _blocks; i++)
                                                                {
                                                                    if (i > 1)
                                                                    {
                                                                        _templateVectors.z--;
                                                                    }
                                                                    for (int j = 1; j <= _blocks; j++)
                                                                    {
                                                                        if (j > 1)
                                                                        {
                                                                            _templateVectors.x++;
                                                                        }
                                                                        if (_world.IsChunkAreaLoaded(_templateVectors.x, _templateVectors.y, _templateVectors.z))
                                                                        {
                                                                            _mazeTemplate.Add(_templateVectors, new int[] { i, j });
                                                                            _mazeForm.Add(_templateVectors, "steel");
                                                                        }
                                                                        else
                                                                        {
                                                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Part of the maze is outside of a loaded chunk area. Reduce the size of the maze"));
                                                                            return;
                                                                        }
                                                                    }
                                                                    _templateVectors.x = _firstTemplateVectorX;
                                                                }
                                                                if (_params.Count == 3)
                                                                {
                                                                    int _levels = _floors * 3;
                                                                    for (int i = 1; i <= _levels; i++)
                                                                    {
                                                                        foreach (var _vector in _mazeTemplate)
                                                                        {
                                                                            if (_vector.Value[0] == 1 || _vector.Value[0] == _blocks || _vector.Value[1] == 1 || _vector.Value[1] == _blocks)
                                                                            {
                                                                                _mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "glassWall");
                                                                            }
                                                                            else if (i % 3 == 0)
                                                                            {
                                                                                if (i == _levels)
                                                                                {
                                                                                    _mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "glass");
                                                                                }
                                                                                else
                                                                                {
                                                                                    _mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "glassWall");
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                _mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "...");
                                                                            }
                                                                        }
                                                                    }
                                                                    _mazeForm = FormPath(_mazeForm, _pathStart, _blocks, _floors);
                                                                    if (_mazeForm == null)
                                                                    {
                                                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to form the maze. Try again"));
                                                                        return;
                                                                    }
                                                                    foreach (var _block in _mazeForm)
                                                                    {
                                                                        _oldBlockValue = _world.GetBlock(_block.Key);
                                                                        _undo.Add(_block.Key, _oldBlockValue);
                                                                        if (_block.Value == "...")
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _stoneBlockValue);
                                                                        }
                                                                        else if (_block.Value == "steel")
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _steelBlockValue);
                                                                        }
                                                                        else if (_block.Value == "glassWall")
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _glassBlockValue);
                                                                        }
                                                                        else if (_block.Value == "wall")
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _concreteBlockValue);
                                                                        }
                                                                        else if (_block.Value == "wallPassage")
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _concreteBlockValue);
                                                                        }
                                                                        else if (_block.Value == "air")
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, BlockValue.Air);
                                                                        }
                                                                        else if (_block.Value == "path")
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, BlockValue.Air);
                                                                        }
                                                                        else if (_block.Value == "ladder1")
                                                                        {
                                                                            _ladderValue.rotation = 1;
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                        }
                                                                        else if (_block.Value == "ladder2")
                                                                        {
                                                                            _ladderValue.rotation = 3;
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                        }
                                                                        else if (_block.Value == "ladder3")
                                                                        {
                                                                            _ladderValue.rotation = 3;
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                        }
                                                                        else if (_block.Value == "ladder4")
                                                                        {
                                                                            _ladderValue.rotation = 4;
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                        }
                                                                        else if (_block.Value == "glass")
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _glassCeilingBlockValue);
                                                                        }
                                                                    }
                                                                    Thread.Sleep(1000);
                                                                    foreach (var _block in _mazeForm)
                                                                    {
                                                                        BlockValue _newBlockValue = _world.GetBlock(_block.Key);
                                                                        if (_block.Value == "..." && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _stoneBlockValue);
                                                                        }
                                                                        else if (_block.Value == "steel" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _steelBlockValue);
                                                                        }
                                                                        else if (_block.Value == "glassWall" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _glassBlockValue);
                                                                        }
                                                                        else if (_block.Value == "wall" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _concreteBlockValue);
                                                                        }
                                                                        else if (_block.Value == "wallPassage" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _concreteBlockValue);
                                                                        }
                                                                        else if (_block.Value == "air" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, BlockValue.Air);
                                                                        }
                                                                        else if (_block.Value == "path" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, BlockValue.Air);
                                                                        }
                                                                        else if (_block.Value == "ladder1" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            _ladderValue.rotation = 1;
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                        }
                                                                        else if (_block.Value == "ladder2" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            _ladderValue.rotation = 3;
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                        }
                                                                        else if (_block.Value == "ladder3" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            _ladderValue.rotation = 3;
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                        }
                                                                        else if (_block.Value == "ladder4" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            _ladderValue.rotation = 4;
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                        }
                                                                        else if (_block.Value == "glass" && !_newBlockValue.Equals(_block.Value))
                                                                        {
                                                                            GameManager.Instance.World.SetBlockRPC(_block.Key, _glassCeilingBlockValue);
                                                                        }
                                                                    }
                                                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Maze has been formed. The start of the maze is at {0}", _pathStart));
                                                                }
                                                                else if (_params.Count == 4)
                                                                {
                                                                    Block _customWall = Block.GetBlockByName(_params[3], false);
                                                                    if (_customWall != null)
                                                                    {
                                                                        BlockValue _customBlockValue = Block.GetBlockValue(_params[3]);
                                                                        int _levels = _floors * 3;
                                                                        for (int i = 1; i <= _levels; i++)
                                                                        {
                                                                            foreach (var _vector in _mazeTemplate)
                                                                            {
                                                                                if (_vector.Value[0] == 1 || _vector.Value[0] == _blocks || _vector.Value[1] == 1 || _vector.Value[1] == _blocks)
                                                                                {
                                                                                    _mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "glassWall");
                                                                                }
                                                                                else if (i % 3 == 0)
                                                                                {
                                                                                    if (i == _levels)
                                                                                    {
                                                                                        _mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "glass");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        _mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "glassWall");
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    _mazeForm.Add(new Vector3i(_vector.Key.x, _vector.Key.y + i, _vector.Key.z), "...");
                                                                                }
                                                                            }
                                                                        }
                                                                        _mazeForm = FormPath(_mazeForm, _pathStart, _blocks, _floors);
                                                                        if (_mazeForm == null)
                                                                        {
                                                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to form the maze. Try again"));
                                                                            return;
                                                                        }
                                                                        foreach (var _block in _mazeForm)
                                                                        {
                                                                            _oldBlockValue = _world.GetBlock(_block.Key);
                                                                            _undo.Add(_block.Key, _oldBlockValue);
                                                                            if (_block.Value == "...")
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _stoneBlockValue);
                                                                            }
                                                                            else if (_block.Value == "steel")
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _steelBlockValue);
                                                                            }
                                                                            else if (_block.Value == "glassWall")
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _glassBlockValue);
                                                                            }
                                                                            else if (_block.Value == "wall")
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _customBlockValue);
                                                                            }
                                                                            else if (_block.Value == "wallPassage")
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _customBlockValue);
                                                                            }
                                                                            else if (_block.Value == "air")
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, BlockValue.Air);
                                                                            }
                                                                            else if (_block.Value == "path")
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, BlockValue.Air);
                                                                            }
                                                                            else if (_block.Value == "ladder1")
                                                                            {
                                                                                _ladderValue.rotation = 1;
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                            }
                                                                            else if (_block.Value == "ladder2")
                                                                            {
                                                                                _ladderValue.rotation = 3;
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                            }
                                                                            else if (_block.Value == "ladder3")
                                                                            {
                                                                                _ladderValue.rotation = 3;
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                            }
                                                                            else if (_block.Value == "ladder4")
                                                                            {
                                                                                _ladderValue.rotation = 4;
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                            }
                                                                            else if (_block.Value == "glass")
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _glassCeilingBlockValue);
                                                                            }
                                                                        }
                                                                        Thread.Sleep(1000);
                                                                        foreach (var _block in _mazeForm)
                                                                        {
                                                                            BlockValue _newBlockValue = _world.GetBlock(_block.Key);
                                                                            if (_block.Value == "..." && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _stoneBlockValue);
                                                                            }
                                                                            else if (_block.Value == "steel" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _steelBlockValue);
                                                                            }
                                                                            else if (_block.Value == "glassWall" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _glassBlockValue);
                                                                            }
                                                                            else if (_block.Value == "wall" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _customBlockValue);
                                                                            }
                                                                            else if (_block.Value == "wallPassage" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _customBlockValue);
                                                                            }
                                                                            else if (_block.Value == "air" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, BlockValue.Air);
                                                                            }
                                                                            else if (_block.Value == "path" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, BlockValue.Air);
                                                                            }
                                                                            else if (_block.Value == "ladder1" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                _ladderValue.rotation = 1;
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                            }
                                                                            else if (_block.Value == "ladder2" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                _ladderValue.rotation = 3;
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                            }
                                                                            else if (_block.Value == "ladder3" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                _ladderValue.rotation = 3;
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                            }
                                                                            else if (_block.Value == "ladder4" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                _ladderValue.rotation = 4;
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _ladderValue);
                                                                            }
                                                                            else if (_block.Value == "glass" && !_newBlockValue.Equals(_block.Value))
                                                                            {
                                                                                GameManager.Instance.World.SetBlockRPC(_block.Key, _glassCeilingBlockValue);
                                                                            }
                                                                        }
                                                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Maze has been spawned. The start of the maze is at {0}", _pathStart));
                                                                    }
                                                                    else
                                                                    {
                                                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: {0}", _params[3]));
                                                                        return;
                                                                    }
                                                                }
                                                                if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.playerId))
                                                                {
                                                                    Undo[_senderInfo.RemoteClientInfo.playerId] = _undo;
                                                                }
                                                                else
                                                                {
                                                                    Undo.Add(_senderInfo.RemoteClientInfo.playerId, _undo);
                                                                }
                                                                SdtdConsole.Instance.Output(string.Format("Use command maze undo to reset the maze space"));
                                                                return;
                                                            }
                                                            else
                                                            {
                                                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: ladderMetal"));
                                                                return;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: glassBulletproofBlock"));
                                                            return;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: glassBusinessBlock"));
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: terrStone"));
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: concreteBlock"));
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to find block name: steelBlock"));
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to form player info for world position. Join the game first or check for errors"));
                                        return;
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to form client info for world position. Join the game first or check for errors"));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid number of floors: {0}", _params[2]));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid width of blocks: {0}", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 4 or 5, found {0}", _params.Count));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("undo"))
                {
                    if (_params.Count == 1)
                    {
                        if (_senderInfo.RemoteClientInfo != null)
                        {
                            if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.playerId))
                            {
                                Undo.TryGetValue(_senderInfo.RemoteClientInfo.playerId, out Dictionary< Vector3i, BlockValue> _undo);
                                World _world = GameManager.Instance.World;
                                foreach (var _block in _undo)
                                {
                                    if (!_world.IsChunkAreaLoaded(_block.Key.x, _block.Key.y, _block.Key.z))
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Area is not loaded. Unable to undo maze blocks"));
                                        return;
                                    }
                                }
                                foreach (var _block in _undo)
                                {
                                    GameManager.Instance.World.SetBlockRPC(_block.Key, _block.Value);
                                }
                                foreach (var _block in _undo)
                                {
                                    GameManager.Instance.World.SetBlockRPC(_block.Key, _block.Value);
                                }
                                Undo.Remove(_senderInfo.RemoteClientInfo.playerId);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] The maze you last spawned has been undone"));
                                return;
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] You have not spawned a maze. Unable to undo"));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to form client info to run undo command"));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
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
                Vector3i _currentPathVector = _startingVector, _nextPathVector, _startPathVector = _startingVector, _endPathVector = Vector3i.zero, _path1 = _startingVector, _path2 = _startingVector,
                    _path3 = _startingVector, _path4 = _startingVector; //pathing vectors
                List<Vector3i> _random = new List<Vector3i>(); //potential movements, random pick
                List<Vector3i> _pathIndex = new List<Vector3i>(); //alternate paths
                Dictionary<int, Vector3i> _pathLength = new Dictionary<int, Vector3i>();
                int _maxPath = _blocks * _blocks, _endPathLength = 0, _currentPathLength = 0;
                for (int i = 1; i <= _floors; i++)
                {
                    _mazeForm[_currentPathVector] = "air";
                    _mazeForm[new Vector3i(_currentPathVector.x, _currentPathVector.y + 1, _currentPathVector.z)] = "air";
                    for (int j = 1; j <= _maxPath; j++)
                    {
                        _path1.x--;
                        _path2.x++;
                        _path3.z--;
                        _path4.z++;
                        if (_mazeForm.ContainsKey(_path1))
                        {
                            _mazeForm.TryGetValue(_path1, out string _blockName);
                            if (_blockName == "...")
                            {
                                _random.Add(_path1);
                            }
                            else if (_blockName == "wallPassage")
                            {
                                _mazeForm[_path1] = "wall";
                            }
                        }
                        if (_mazeForm.ContainsKey(_path2))
                        {
                            _mazeForm.TryGetValue(_path2, out string _blockName);
                            if (_blockName == "...")
                            {
                                _random.Add(_path2);
                            }
                            else if (_blockName == "wallPassage")
                            {
                                _mazeForm[_path2] = "wall";
                            }
                        }
                        if (_mazeForm.ContainsKey(_path3))
                        {
                            _mazeForm.TryGetValue(_path3, out string _blockName);
                            if (_blockName == "...")
                            {
                                _random.Add(_path3);
                            }
                            else if (_blockName == "wallPassage")
                            {
                                _mazeForm[_path3] = "wall";
                            }
                        }
                        if (_mazeForm.ContainsKey(_path4))
                        {
                            _mazeForm.TryGetValue(_path4, out string _blockName);
                            if (_blockName == "...")
                            {
                                _random.Add(_path4);
                            }
                            else if (_blockName == "wallPassage")
                            {
                                _mazeForm[_path4] = "wall";
                            }
                        }
                        if (_random.Count > 1) //multiple path found
                        {
                            _pathIndex.Add(_currentPathVector);
                            _random.RandomizeList();
                            _nextPathVector = _random[0];
                            _random.RemoveAt(0);
                            _mazeForm[_nextPathVector] = "air";
                            _mazeForm[new Vector3i(_nextPathVector.x, _nextPathVector.y + 1, _nextPathVector.z)] = "air";
                            for (int k = 0; k < _random.Count; k++)
                            {
                                _mazeForm[_random[k]] = "wallPassage";
                                _mazeForm[new Vector3i(_random[k].x, _random[k].y + 1, _random[k].z)] = "wallPassage";
                            }
                            _random.Clear();
                            _path1 = _nextPathVector;
                            _path2 = _nextPathVector;
                            _path3 = _nextPathVector;
                            _path4 = _nextPathVector;
                            _currentPathVector = _nextPathVector;
                        }
                        else if (_random.Count == 1) //one path found
                        {
                            _nextPathVector = _random[0];
                            _mazeForm[_nextPathVector] = "air";
                            _mazeForm[new Vector3i(_nextPathVector.x, _nextPathVector.y + 1, _nextPathVector.z)] = "air";
                            _random.Clear();
                            _path1 = _nextPathVector;
                            _path2 = _nextPathVector;
                            _path3 = _nextPathVector;
                            _path4 = _nextPathVector;
                            _currentPathVector = _nextPathVector;
                        }
                        else //no path found
                        {
                            if (_pathIndex.Count > 0)
                            {
                                _pathIndex.Reverse();
                                for (int k = 0; k < _pathIndex.Count; k++)
                                {
                                    _path1 = _pathIndex[k];
                                    _path2 = _pathIndex[k];
                                    _path3 = _pathIndex[k];
                                    _path4 = _pathIndex[k];
                                    _path1.x--;
                                    _path2.x++;
                                    _path3.z--;
                                    _path4.z++;
                                    if (_mazeForm.ContainsKey(_path1))
                                    {
                                        _mazeForm.TryGetValue(_path1, out string _blockName);
                                        if (_blockName == "wallPassage")
                                        {
                                            _random.Add(_path1);
                                        }
                                    }
                                    if (_mazeForm.ContainsKey(_path2))
                                    {
                                        _mazeForm.TryGetValue(_path2, out string _blockName);
                                        if (_blockName == "wallPassage")
                                        {
                                            _random.Add(_path2);
                                        }
                                    }
                                    if (_mazeForm.ContainsKey(_path3))
                                    {
                                        _mazeForm.TryGetValue(_path3, out string _blockName);
                                        if (_blockName == "wallPassage")
                                        {
                                            _random.Add(_path3);
                                        }
                                    }
                                    if (_mazeForm.ContainsKey(_path4))
                                    {
                                        _mazeForm.TryGetValue(_path4, out string _blockName);
                                        if (_blockName == "wallPassage")
                                        {
                                            _random.Add(_path4);
                                        }
                                    }
                                    if (_random.Count > 1)
                                    {
                                        _random.RandomizeList();
                                        _nextPathVector = _random[0];
                                        _random.Clear();
                                        _mazeForm[_nextPathVector] = "air";
                                        _mazeForm[new Vector3i(_nextPathVector.x, _nextPathVector.y + 1, _nextPathVector.z)] = "air";
                                        _pathIndex.Reverse();
                                        _path1 = _nextPathVector;
                                        _path2 = _nextPathVector;
                                        _path3 = _nextPathVector;
                                        _path4 = _nextPathVector;
                                        _currentPathVector = _nextPathVector;
                                        break;
                                    }
                                    else if (_random.Count == 1)
                                    {
                                        _nextPathVector = _random[0];
                                        _random.Clear();
                                        _mazeForm[_nextPathVector] = "air";
                                        _mazeForm[new Vector3i(_nextPathVector.x, _nextPathVector.y + 1, _nextPathVector.z)] = "air";
                                        _pathIndex.Remove(_currentPathVector);
                                        _pathIndex.Reverse();
                                        _path1 = _nextPathVector;
                                        _path2 = _nextPathVector;
                                        _path3 = _nextPathVector;
                                        _path4 = _nextPathVector;
                                        _currentPathVector = _nextPathVector;
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
                    _currentPathVector = _startPathVector;
                    _mazeForm[_currentPathVector] = "path";
                    _mazeForm[new Vector3i(_currentPathVector.x, _currentPathVector.y + 1, _currentPathVector.z)] = "path";
                    _path1 = _currentPathVector;
                    _path2 = _currentPathVector;
                    _path3 = _currentPathVector;
                    _path4 = _currentPathVector;
                    for (int j = 0; j < _maxPath; j++)
                    {
                        _path1.x--;
                        _path2.x++;
                        _path3.z--;
                        _path4.z++;
                        if (_mazeForm.ContainsKey(_path1))
                        {
                            _mazeForm.TryGetValue(_path1, out string _blockName);
                            if (_blockName == "air")
                            {
                                _random.Add(_path1);
                            }
                        }
                        if (_mazeForm.ContainsKey(_path2))
                        {
                            _mazeForm.TryGetValue(_path2, out string _blockName);
                            if (_blockName == "air")
                            {
                                _random.Add(_path2);
                            }
                        }
                        if (_mazeForm.ContainsKey(_path3))
                        {
                            _mazeForm.TryGetValue(_path3, out string _blockName);
                            if (_blockName == "air")
                            {
                                _random.Add(_path3);
                            }
                        }
                        if (_mazeForm.ContainsKey(_path4))
                        {
                            _mazeForm.TryGetValue(_path4, out string _blockName);
                            if (_blockName == "air")
                            {
                                _random.Add(_path4);
                            }
                        }
                        if (_random.Count > 1)
                        {
                            _random.RandomizeList();
                            _nextPathVector = _random[0];
                            _random.Clear();
                            _mazeForm[_nextPathVector] = "path";
                            _mazeForm[new Vector3i(_nextPathVector.x, _nextPathVector.y + 1, _nextPathVector.z)] = "path";
                            _pathLength.Add(j, _currentPathVector);
                            _path1 = _nextPathVector;
                            _path2 = _nextPathVector;
                            _path3 = _nextPathVector;
                            _path4 = _nextPathVector;
                            _currentPathVector = _nextPathVector;
                            _currentPathLength++;
                        }
                        else if (_random.Count == 1)
                        {
                            _nextPathVector = _random[0];
                            _random.Clear();
                            _mazeForm[_nextPathVector] = "path";
                            _mazeForm[new Vector3i(_nextPathVector.x, _nextPathVector.y + 1, _nextPathVector.z)] = "path";
                            _path1 = _nextPathVector;
                            _path2 = _nextPathVector;
                            _path3 = _nextPathVector;
                            _path4 = _nextPathVector;
                            _currentPathVector = _nextPathVector;
                            _currentPathLength++;
                        }
                        else
                        {
                            if (_currentPathLength > _endPathLength)
                            {
                                _endPathVector = _currentPathVector;
                                _endPathLength = _currentPathLength;
                            }
                            if (_pathLength.Count > 0)
                            {
                                KeyValuePair<int, Vector3i> _pathPoint = _pathLength.ElementAt(0);
                                _pathLength.Remove(_pathPoint.Key);
                                _path1 = _pathPoint.Value;
                                _path2 = _pathPoint.Value;
                                _path3 = _pathPoint.Value;
                                _path4 = _pathPoint.Value;
                                _currentPathVector = _pathPoint.Value;
                                _currentPathLength = _pathPoint.Key;
                            }
                            else
                            {
                                _currentPathLength = 0;
                                _endPathLength = 0;
                                break;
                            }
                        }
                    }
                    if (_endPathVector != Vector3i.zero)
                    {
                        bool _pathUp = false;
                        _path1 = _endPathVector;
                        _path2 = _endPathVector;
                        _path3 = _endPathVector;
                        _path4 = _endPathVector;
                        _path1.x--;
                        _path2.x++;
                        _path3.z--;
                        _path4.z++;
                        if (!_pathUp)
                        {
                            if (_mazeForm.ContainsKey(_path1))
                            {
                                _mazeForm.TryGetValue(_path1, out string _blockName);
                                if (_blockName == "wall" || _blockName == "wallPassage")
                                {
                                    _mazeForm[_endPathVector] = "ladder1";
                                    _mazeForm[new Vector3i(_endPathVector.x, _endPathVector.y + 1, _endPathVector.z)] = "ladder1";
                                    _mazeForm[new Vector3i(_endPathVector.x, _endPathVector.y + 2, _endPathVector.z)] = "ladder1";
                                    _pathUp = true;
                                }
                            }
                        }
                        if (!_pathUp)
                        {
                            if (_mazeForm.ContainsKey(_path2))
                            {
                                _mazeForm.TryGetValue(_path2, out string _blockName);
                                if (_blockName == "wall" || _blockName == "wallPassage")
                                {
                                    _mazeForm[_endPathVector] = "ladder3";
                                    _mazeForm[new Vector3i(_endPathVector.x, _endPathVector.y + 1, _endPathVector.z)] = "ladder3";
                                    _mazeForm[new Vector3i(_endPathVector.x, _endPathVector.y + 2, _endPathVector.z)] = "ladder3";
                                    _pathUp = true;
                                }
                            }
                        }
                        if (!_pathUp)
                        {
                            if (_mazeForm.ContainsKey(_path3))
                            {
                                _mazeForm.TryGetValue(_path3, out string _blockName);
                                if (_blockName == "wall" || _blockName == "wallPassage")
                                {
                                    _mazeForm[_endPathVector] = "ladder4";
                                    _mazeForm[new Vector3i(_endPathVector.x, _endPathVector.y + 1, _endPathVector.z)] = "ladder4";
                                    _mazeForm[new Vector3i(_endPathVector.x, _endPathVector.y + 2, _endPathVector.z)] = "ladder4";
                                    _pathUp = true;
                                }
                            }
                        }
                        if (!_pathUp)
                        {
                            if (_mazeForm.ContainsKey(_path4))
                            {
                                _mazeForm.TryGetValue(_path4, out string _blockName);
                                if (_blockName == "wall" || _blockName == "wallPassage")
                                {
                                    _mazeForm[_endPathVector] = "ladder2";
                                    _mazeForm[new Vector3i(_endPathVector.x, _endPathVector.y + 1, _endPathVector.z)] = "ladder2";
                                    _mazeForm[new Vector3i(_endPathVector.x, _endPathVector.y + 2, _endPathVector.z)] = "ladder2";
                                }
                            }
                        }
                        _startPathVector = new Vector3i(_endPathVector.x, _endPathVector.y + 3, _endPathVector.z);
                        _currentPathVector = _startPathVector;
                        _path1 = _startPathVector;
                        _path2 = _startPathVector;
                        _path3 = _startPathVector;
                        _path4 = _startPathVector;
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

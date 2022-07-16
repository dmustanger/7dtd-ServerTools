using System;
using System.Collections.Generic;
using System.Threading;

namespace ServerTools
{
    class SpawnActiveBlocks : ConsoleCmdAbstract
    {
        public static Dictionary<int, Vector3i[]> Vectors = new Dictionary<int, Vector3i[]>();
        public static Dictionary<string, Dictionary<Vector3i, BlockValue>> Undo = new Dictionary<string, Dictionary<Vector3i, BlockValue>>();

        public override string GetDescription()
        {
            return "[ServerTools] - Spawn active blocks in the world";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-sab {BlockName}\n" +
                   "  2. st-sab save\n" +
                   "  3. st-sab cancel\n" +
                   "  4. st-sab undo\n" +
                   "1. Spawns the block where you are standing if no corners have been set. If corners are set, this block will spawn from corner to corner\n" +
                   "2. Saves the position you are standing as corner 1 if no other has been set. Saves corner 2 if the first has been set\n" +
                   "3. Cancels the saved corner positions\n" +
                   "4. Reverts the last spawned blocks to their previous value\n" +
                   "*Notes*\n" +
                   "You must build on top or next to existing blocks to avoid collapse\n" +
                   "There is an intentional pause of one second during block spawn. If blocks have collapsed, it will automatically spawn a replacement\n" +
                   "Falling_Blocks_Remover tool will automatically enable during block spawn and then reset to its original state\n" +
                   "Think of the corners as opposites in a 3d rectangle or square. Everything in between the corners will be set as the block of choice\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-SpawnActiveBlocks", "sab", "st-sab" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 2, found '{0}'", _params.Count));
                    return;
                }
                if (_senderInfo.RemoteClientInfo == null)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] No client info found. Join the server as a client before using this command"));
                    return;
                }
                World world = GameManager.Instance.World;
                if (world.Players.dict.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                {
                    EntityPlayer player = world.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                    if (player != null)
                    {
                        Vector3i playerPos = new Vector3i(player.position);
                        if (_params[0].ToLower() == "save")
                        {
                            if (player.position.y < 3)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Your position is too low. Unable to generate blocks at this world height"));
                                return;
                            }
                            if (!Vectors.ContainsKey(player.entityId))
                            {
                                Vector3i[] vectors = new Vector3i[2];
                                vectors[0] = playerPos;
                                Vectors.Add(player.entityId, vectors);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Set corner 1 to your position '{0}'", playerPos));
                                return;
                            }
                            else
                            {
                                Vectors.TryGetValue(player.entityId, out Vector3i[] vectors);
                                vectors[1] = playerPos;
                                Vectors[player.entityId] = vectors;
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Set corner 2 to your position '{0}'", playerPos));
                                return;
                            }
                        }
                        else if (_params[0].ToLower().Equals("cancel"))
                        {
                            if (Vectors.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                            {
                                Vectors.Remove(_senderInfo.RemoteClientInfo.entityId);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Cancelled your saved corner positions"));
                                return;
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] You have no saved corner positions"));
                                return;
                            }
                        }
                        else if (_params[0].ToLower().Equals("undo"))
                        {
                            if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString))
                            {
                                Undo.TryGetValue(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString, out Dictionary<Vector3i, BlockValue> _undo);
                                foreach (var _block in _undo)
                                {
                                    if (!world.IsChunkAreaLoaded(_block.Key.x, _block.Key.y, _block.Key.z))
                                    {
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Area is not loaded. Unable to undo maze blocks"));
                                        return;
                                    }
                                }
                                foreach (var _block in _undo)
                                {
                                    GameManager.Instance.World.SetBlockRPC(_block.Key, _block.Value);
                                }
                                Thread.Sleep(1000);
                                foreach (var _block in _undo)
                                {
                                    BlockValue newBlockValue = world.GetBlock(_block.Key);
                                    if (!newBlockValue.Equals(_block.Value))
                                    {
                                        GameManager.Instance.World.SetBlockRPC(_block.Key, _block.Value);
                                    }
                                }
                                Undo.Remove(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The blocks you last spawned have been set to their original value"));
                                return;
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] You have not spawned any blocks. Unable to undo"));
                                return;
                            }
                        }
                        else
                        {
                            Block _block = Block.GetBlockByName(_params[0], false);
                            if (_block != null)
                            {
                                bool fallingBlocks = FallingBlocks.IsEnabled;
                                FallingBlocks.IsEnabled = true;
                                BlockValue blockValue = Block.GetBlockValue(_params[0]);
                                if (Vectors.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                                {
                                    Vectors.TryGetValue(_senderInfo.RemoteClientInfo.entityId, out Vector3i[] _vectors);
                                    int _vectorPointX1 = _vectors[0].x, _vectorPointX2 = _vectors[1].x, _vectorPointY1 = _vectors[0].y,
                                        _vectorPointY2 = _vectors[1].y, _vectorPointZ1 = _vectors[0].z, _vectorPointZ2 = _vectors[1].z;
                                    Dictionary<Vector3i, BlockValue> _undo = new Dictionary<Vector3i, BlockValue>();
                                    if (_vectorPointX2 < _vectorPointX1)
                                    {
                                        _vectorPointX1 = _vectors[1].x;
                                        _vectorPointX2 = _vectors[0].x;
                                    }
                                    if (_vectorPointY2 < _vectorPointY1)
                                    {
                                        _vectorPointY1 = _vectors[1].y;
                                        _vectorPointY2 = _vectors[0].y;
                                    }
                                    if (_vectorPointZ2 < _vectorPointZ1)
                                    {
                                        _vectorPointZ1 = _vectors[1].z;
                                        _vectorPointZ2 = _vectors[0].z;
                                    }
                                    for (int x = _vectorPointX1; x <= _vectorPointX2; x++)
                                    {
                                        for (int z = _vectorPointZ1; z <= _vectorPointZ2; z++)
                                        {
                                            for (int y = _vectorPointY1; y <= _vectorPointY2; y++)
                                            {
                                                Vector3i _processVector = new Vector3i(x, y, z);
                                                if (!world.IsChunkAreaLoaded(_processVector.x, _processVector.y, _processVector.z))
                                                {
                                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The blocks you are trying to spawn are outside of a loaded chunk area. Unable to spawn block"));
                                                    return;
                                                }
                                                BlockValue oldBlockValue = world.GetBlock(_processVector);
                                                _undo.Add(_processVector, oldBlockValue);
                                                GameManager.Instance.World.SetBlockRPC(_processVector, blockValue);
                                            }
                                        }
                                    }
                                    if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString))
                                    {
                                        Undo[_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString] = _undo;
                                    }
                                    else
                                    {
                                        Undo.Add(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString, _undo);
                                    }
                                    Thread.Sleep(1000);
                                    for (int x = _vectorPointX1; x <= _vectorPointX2; x++)
                                    {
                                        for (int z = _vectorPointZ1; z <= _vectorPointZ2; z++)
                                        {
                                            for (int y = _vectorPointY1; y <= _vectorPointY2; y++)
                                            {
                                                Vector3i processVector = new Vector3i(x, y, z);
                                                BlockValue _newBlockValue = world.GetBlock(processVector);
                                                if (!_newBlockValue.Equals(blockValue))
                                                {
                                                    GameManager.Instance.World.SetBlockRPC(processVector, blockValue);
                                                }
                                            }
                                        }
                                    }
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Spawned active block. Double check integrity of block before continuing"));
                                }
                                else
                                {
                                    Dictionary<Vector3i, BlockValue> _undo = new Dictionary<Vector3i, BlockValue>();
                                    BlockValue oldBlockValue = world.GetBlock(playerPos);
                                    if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString))
                                    {
                                        _undo.Add(playerPos, oldBlockValue);
                                        Undo[_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString] = _undo;
                                    }
                                    else
                                    {
                                        _undo.Add(playerPos, oldBlockValue);
                                        Undo.Add(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString, _undo);
                                    }
                                    GameManager.Instance.World.SetBlockRPC(playerPos, blockValue);
                                    Thread.Sleep(1000);
                                    BlockValue _newBlockValue = world.GetBlock(playerPos);
                                    if (!_newBlockValue.Equals(blockValue))
                                    {
                                        GameManager.Instance.World.SetBlockRPC(playerPos, blockValue);
                                    }
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Spawned active block. Double check integrity of block before continuing"));
                                }
                                FallingBlocks.IsEnabled = fallingBlocks;
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to spawn block. Could not find block name '{0}'", _params[0]));
                                return;
                            }
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player data for current position"));
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player data for current position"));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SpawnActiveBlocks.Execute: {0}", e.Message));
            }
        }
    }
}

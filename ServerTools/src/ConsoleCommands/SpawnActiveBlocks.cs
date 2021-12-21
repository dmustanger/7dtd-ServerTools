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
                   "4. Cancels the saved corner positions\n" +
                   "5. Reverts the last spawned blocks to their previous value\n" +
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
                World _world = GameManager.Instance.World;
                if (_world.Players.dict.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                {
                    EntityPlayer _player = _world.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                    if (_player != null)
                    {
                        Vector3i _playerPos = new Vector3i(_player.position);
                        if (_params[0].ToLower() == "save")
                        {
                            if (_player.position.y < 3)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Your position is too low. Unable to generate blocks at this world height"));
                                return;
                            }
                            if (!Vectors.ContainsKey(_player.entityId))
                            {
                                Vector3i[] _vectors = new Vector3i[2];
                                _vectors[0] = _playerPos;
                                Vectors.Add(_player.entityId, _vectors);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Set corner 1 to your position '{0}'", _playerPos));
                                return;
                            }
                            else
                            {
                                Vectors.TryGetValue(_player.entityId, out Vector3i[] _vectors);
                                _vectors[1] = _playerPos;
                                Vectors[_player.entityId] = _vectors;
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Set corner 2 to your position '{0}'", _playerPos));
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
                            if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier))
                            {
                                Undo.TryGetValue(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier, out Dictionary<Vector3i, BlockValue> _undo);
                                foreach (var _block in _undo)
                                {
                                    if (!_world.IsChunkAreaLoaded(_block.Key.x, _block.Key.y, _block.Key.z))
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
                                    BlockValue _newBlockValue = _world.GetBlock(_block.Key);
                                    if (!_newBlockValue.Equals(_block.Value))
                                    {
                                        GameManager.Instance.World.SetBlockRPC(_block.Key, _block.Value);
                                    }
                                }
                                Undo.Remove(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier);
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
                                bool _fallingBlocks = FallingBlocks.IsEnabled;
                                FallingBlocks.IsEnabled = true;
                                BlockValue _blockValue = Block.GetBlockValue(_params[0]);
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
                                                if (!_world.IsChunkAreaLoaded(_processVector.x, _processVector.y, _processVector.z))
                                                {
                                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The blocks you are trying to spawn are outside of a loaded chunk area. Unable to spawn block"));
                                                    return;
                                                }
                                                BlockValue _oldBlockValue = _world.GetBlock(_processVector);
                                                _undo.Add(_processVector, _oldBlockValue);
                                                GameManager.Instance.World.SetBlockRPC(_processVector, _blockValue);
                                            }
                                        }
                                    }
                                    if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier))
                                    {
                                        Undo[_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier] = _undo;
                                    }
                                    else
                                    {
                                        Undo.Add(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier, _undo);
                                    }
                                    Thread.Sleep(1000);
                                    for (int x = _vectorPointX1; x <= _vectorPointX2; x++)
                                    {
                                        for (int z = _vectorPointZ1; z <= _vectorPointZ2; z++)
                                        {
                                            for (int y = _vectorPointY1; y <= _vectorPointY2; y++)
                                            {
                                                Vector3i _processVector = new Vector3i(x, y, z);
                                                BlockValue _newBlockValue = _world.GetBlock(_processVector);
                                                if (!_newBlockValue.Equals(_blockValue))
                                                {
                                                    GameManager.Instance.World.SetBlockRPC(_processVector, _blockValue);
                                                }
                                            }
                                        }
                                    }
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Spawned active block. Double check integrity of block before continuing"));
                                }
                                else
                                {
                                    Dictionary<Vector3i, BlockValue> _undo = new Dictionary<Vector3i, BlockValue>();
                                    BlockValue _oldBlockValue = _world.GetBlock(_playerPos);
                                    if (Undo.ContainsKey(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier))
                                    {
                                        _undo.Add(_playerPos, _oldBlockValue);
                                        Undo[_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier] = _undo;
                                    }
                                    else
                                    {
                                        _undo.Add(_playerPos, _oldBlockValue);
                                        Undo.Add(_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier, _undo);
                                    }
                                    GameManager.Instance.World.SetBlockRPC(_playerPos, _blockValue);
                                    Thread.Sleep(1000);
                                    BlockValue _newBlockValue = _world.GetBlock(_playerPos);
                                    if (!_newBlockValue.Equals(_blockValue))
                                    {
                                        GameManager.Instance.World.SetBlockRPC(_playerPos, _blockValue);
                                    }
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Spawned active block. Double check integrity of block before continuing"));
                                }
                                FallingBlocks.IsEnabled = _fallingBlocks;
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

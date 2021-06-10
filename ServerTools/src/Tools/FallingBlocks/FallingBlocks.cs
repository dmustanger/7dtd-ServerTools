using System;
using System.Collections.Generic;

namespace ServerTools
{
    class FallingBlocks
    {
        public static bool IsEnabled = false, OutputLog = false;
        public static int Max_Blocks = 10;

        public static void Single(Vector3i _blockPosition)
        {
            try
            {
                if (_blockPosition != null)
                {
                    BlockValue _blockValue = GameManager.Instance.World.GetBlock(_blockPosition);
                    Block _block = _blockValue.Block;
                    if (_block is BlockSleepingBag || _block.IsDecoration || _block.IsPlant() || _block.isMultiBlock || _blockValue.Equals(BlockValue.Air))
                    {
                        return;
                    }
                    else
                    {
                        GameManager.Instance.World.SetBlockRPC(_blockPosition, BlockValue.Air);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in FallingBlocks.Exec: {0}", e.Message));
            }
        }

        public static void Multiple(IList<Vector3i> _blocks)
        {
            try
            {
                if (_blocks != null && _blocks.Count > 0)
                {
                    int _count = 0;
                    for (int i = 0; i < _blocks.Count; i++)
                    {
                        BlockValue _blockValue = GameManager.Instance.World.GetBlock(_blocks[i]);
                        Block _block = _blockValue.Block;
                        if (_block is BlockSleepingBag || _block.IsDecoration || _block.IsPlant() || _block.isMultiBlock || _blockValue.Equals(BlockValue.Air))
                        {
                            continue;
                        }
                        else
                        {
                            GameManager.Instance.World.SetBlockRPC(_blocks[i], BlockValue.Air);
                            _count++;
                        }
                    }
                    if (OutputLog && _count >= Max_Blocks)
                    {
                        EntityPlayer _closestPlayer = GameManager.Instance.World.GetClosestPlayer(_blocks[0].x, _blocks[0].y, _blocks[0].z, -1, 75);
                        if (_closestPlayer != null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Removed {0} falling blocks at {1}. The closest player entity id was {2} named {3} @ {4}", _count, _blocks[0], _closestPlayer.entityId, _closestPlayer.EntityName, _closestPlayer.position));
                        }
                        else
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Removed {0} falling blocks at {1}. No players were located near by", _count, _blocks[0]));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in FallingBlocks.Exec: {0}", e.Message));
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace ServerTools
{
    class FallingBlocks
    {
        public static bool IsEnabled = false, OutputLog = false;
        public static int Max_Blocks = 10;

        public static void Single(World _world, Vector3i _blockPosition)
        {
            try
            {
                if (_blockPosition != null)
                {
                    BlockValue blockValue = _world.GetBlock(_blockPosition);
                    Block block = blockValue.Block;
                    if (block is BlockSleepingBag || block.IsDecoration || block.IsPlant() || block.isMultiBlock || blockValue.Equals(BlockValue.Air))
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
                Log.Out(string.Format("[SERVERTOOLS] Error in FallingBlocks.Single: {0}", e.Message));
            }
        }

        public static void Multiple(World _world, IList<Vector3i> _blocks)
        {
            try
            {
                if (_blocks != null && _blocks.Count > 0)
                {
                    int count = 0;
                    for (int i = 0; i < _blocks.Count; i++)
                    {
                        BlockValue blockValue = _world.GetBlock(_blocks[i]);
                        Block block = blockValue.Block;
                        if (block is BlockSleepingBag || block.IsDecoration || block.IsPlant() || block.isMultiBlock || blockValue.Equals(BlockValue.Air))
                        {
                            continue;
                        }
                        else
                        {
                            GameManager.Instance.World.SetBlockRPC(_blocks[i], BlockValue.Air);
                            count++;
                        }
                    }
                    if (OutputLog && count >= Max_Blocks)
                    {
                        EntityPlayer closestPlayer = _world.GetClosestPlayer(_blocks[0].x, _blocks[0].y, _blocks[0].z, -1, 75);
                        if (closestPlayer != null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' falling blocks at '{1}'. The closest player entity id was '{2}' named '{3}' @ '{4}'", count, _blocks[0], closestPlayer.entityId, closestPlayer.EntityName, closestPlayer.position));
                        }
                        else
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' falling blocks at '{1}'. No players were located near by", count, _blocks[0]));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in FallingBlocks.Multiple: {0}", e.Message));
            }
        }
    }
}

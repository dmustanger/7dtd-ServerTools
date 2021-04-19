using System;

namespace ServerTools
{
    class FallingBlocks
    {
        public static bool IsEnabled = false, OutputLog = false;

        public static void Exec(Vector3i _blockPosition)
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
                        if (OutputLog)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Falling block removed at position {0}", _blockPosition));
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

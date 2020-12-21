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
                    GameManager.Instance.World.SetBlockRPC(_blockPosition, BlockValue.Air);
                    if (OutputLog)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Falling block removed at position {0}", _blockPosition));
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

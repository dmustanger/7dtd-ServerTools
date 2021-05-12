using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class BlockLogger
    {
        public static bool IsEnabled = false;

        private static string _blockFile = string.Format("BlockLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string _blockFilepath = string.Format("{0}/Logs/BlockLogs/{1}", API.ConfigPath, _blockFile);

        public static void Log(string _persistentPlayerId, BlockChangeInfo _bChangeInfo)
        {
            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
            if (_player != null)
            {
                using (StreamWriter sw = new StreamWriter(_blockFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Player named {1} with steam id {2} placed {3} @ {4}.", DateTime.Now, _player.EntityName, _persistentPlayerId, _bChangeInfo.blockValue.Block.GetBlockName(), _bChangeInfo.pos));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
        }
    }
}

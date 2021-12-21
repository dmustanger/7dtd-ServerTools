using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class BlockLogger
    {
        public static bool IsEnabled = false;

        private static readonly string file = string.Format("BlockLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static string Filepath = string.Format("{0}/Logs/BlockLogs/{1}", API.ConfigPath, file);

        public static void PlacedBlock(ClientInfo _cInfo, Block _newBlock, Vector3i _position)
        {
            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("{0}: Id '{1}' '{2}' named '{3}' placed '{4}' @ '{5}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _newBlock.GetBlockName(), _position));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }

        public static void RemovedBlock(ClientInfo _cInfo, Block _oldBlock, Vector3i _position)
        {
            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("{0}: Id '{1}' '{2}' named '{3}' removed '{4}' @ '{5}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _oldBlock.GetBlockName(), _position));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }

        public static void DowngradedBlock(ClientInfo _cInfo, Block _oldBlock, Block _newBlock, Vector3i _position)
        {
            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("{0}: Id '{1}' '{2}' named '{3}' downgraded '{4}' @ '{5}' to '{6}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _oldBlock.GetBlockName(), _position, _newBlock.GetBlockName()));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }

        public static void BrokeBlock(ClientInfo _cInfo, Block _oldBlock, Vector3i _position)
        {
            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
            if (_player != null)
            {
                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Id '{1}' '{2}' named '{3}' broke '{4}' @ '{5}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _oldBlock.GetBlockName(), _position));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
        }
    }
}

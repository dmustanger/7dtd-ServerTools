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


    public static void PlacedBlock(string _persistentPlayerId, Block _newBlock, Vector3i _position)
        {
            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
            if (_player != null)
            {
                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Player named {1} with steam id {2} placed {3} @ {4}", DateTime.Now, _player.EntityName, _persistentPlayerId, _newBlock.GetBlockName(), _position));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static void RemovedBlock(string _persistentPlayerId, Block _oldBlock, Vector3i _position)
        {
            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
            if (_player != null)
            {
                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Player named {1} with steam id {2} removed {3} @ {4}", DateTime.Now, _player.EntityName, _persistentPlayerId, _oldBlock.GetBlockName(), _position));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static void DowngradedBlock(string _persistentPlayerId, Block _oldBlock, Block _newBlock, Vector3i _position)
        {
            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
            if (_player != null)
            {
                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Player named {1} with steam id {2} downgraded {3} @ {4} to {5}", DateTime.Now, _player.EntityName, _persistentPlayerId, _oldBlock.GetBlockName(), _position, _newBlock.GetBlockName()));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static void BrokeBlock(string _persistentPlayerId, Block _oldBlock, Vector3i _position)
        {
            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_persistentPlayerId);
            if (_player != null)
            {
                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Player named {1} with steam id {2} broke {3} @ {4}", DateTime.Now, _player.EntityName, _persistentPlayerId, _oldBlock.GetBlockName(), _position));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
        }
    }
}

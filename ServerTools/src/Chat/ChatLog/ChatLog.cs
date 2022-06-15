using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    public class ChatLog
    {
        public static bool IsEnabled = false;
        private static string file = string.Format("ChatLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filePath = string.Format("{0}/Logs/ChatLogs/{1}", API.ConfigPath, file);

        public static void Exec(string _message, string _playerName, EChatType _type)
        {
            using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("{0} {1} {2}: {3}", DateTime.Now, _type.ToString(), _playerName, _message));
                sw.Flush();
                sw.Close();
            }
        }
    }
}
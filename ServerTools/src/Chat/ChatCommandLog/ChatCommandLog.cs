using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    public class ChatCommandLog
    {
        public static bool IsEnabled = false;
        private static string file = string.Format("CommandLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filePath = string.Format("{0}/Logs/ChatCommandLogs/{1}", API.ConfigPath, file);

        public static void Exec(string _message, ClientInfo _cInfo)
        {
            using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("{0} {1} {2}: {3}", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _message));
                sw.Flush();
                sw.Close();
            }
        }
    }
}
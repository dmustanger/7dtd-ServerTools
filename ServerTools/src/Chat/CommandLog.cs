using System;
using System.IO;

namespace ServerTools
{
    public class CommandLog
    {
        public static bool IsEnabled = false;
        private static string file = string.Format("CommandLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filePath = string.Format("{0}/Logs/CommandLog/{1}", API.ConfigPath, file);

        public static void Exec(string _message, string _playerName)
        {
            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                sw.WriteLine(string.Format("{0} {1}: {2}", DateTime.Now, _playerName, _message));
                sw.Flush();
                sw.Close();
            }
        }
    }
}
using System;
using System.IO;

namespace ServerTools
{
    public class ChatLog
    {
        public static bool IsEnabled = false;

        public static void Send(string _message, string _playerName)
        {
            if (!Directory.Exists(Config._gamepath + "/ChatLogs"))
            {
                Directory.CreateDirectory(Config._gamepath + "/ChatLogs");
            }
            string _file = string.Format("ChatLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
            string _filepath = string.Format("{0}/ChatLogs/{1}", Config._gamepath, _file);
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("{0} {1}: {2}", DateTime.Now, _playerName, _message));
                sw.Flush();
                sw.Close();
            } 
        }
    }
}
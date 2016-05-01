using System;
using System.IO;

namespace ServerTools
{
    public class ChatLog
    {
        public static bool IsEnabled = false;

        public static void Log(string _message, string _playerName)
        {
            if (!Directory.Exists(API.GamePath + "/ChatLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/ChatLogs");
            }
            string _file = string.Format("ChatLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
            string _filepath = string.Format("{0}/ChatLogs/{1}", API.GamePath, _file);
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("{0} {1}: {2}", DateTime.Now, _playerName, _message));
                sw.Flush();
                sw.Close();
            }
        }
    }
}
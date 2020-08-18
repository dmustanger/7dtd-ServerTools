using System;
using System.IO;

namespace ServerTools
{
    class ConsoleCommandLog
    {
        public static bool IsEnabled = false;
        private static string file = string.Format("ConsoleCommandLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/ConsoleCommandLogs/{1}", API.ConfigPath, file);

        public static void Exec(ClientInfo _cInfo, string _cmd)
        {
            if (_cInfo != null && !string.IsNullOrEmpty(_cmd))
            {
                if (!string.IsNullOrEmpty(_cInfo.playerName))
                {
                    using (StreamWriter sw = new StreamWriter(filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}: {1} {2} {3}, executed command '{4}' in the console.", DateTime.Now, _cInfo.playerId, _cInfo.ip, _cInfo.playerName, _cmd));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
                else if (!string.IsNullOrEmpty(_cInfo.ip))
                {
                    using (StreamWriter sw = new StreamWriter(filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}: {1} {2}, executed command '{3}' in the console.", DateTime.Now, _cInfo.playerId, _cInfo.ip, _cmd));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}: {1}, executed command '{2}' in the console.", DateTime.Now, _cInfo.playerId, _cmd));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
        }
    }
}

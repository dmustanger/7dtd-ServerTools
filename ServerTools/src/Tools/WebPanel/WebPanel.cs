using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    public class WebPanel
    {
        public static bool IsEnabled = false;
        public static Dictionary<string, int> LoginAttempts = new Dictionary<string, int>();
        public static Dictionary<string, int> PageHits = new Dictionary<string, int>();
        public static Dictionary<string, DateTime> TimeOut = new Dictionary<string, DateTime>();

        private static readonly string file = string.Format("WebPanelLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string filePath = string.Format("{0}/Logs/WebPanelLogs/{1}", API.ConfigPath, file);

        public static void Writer(string _input)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: {1}", DateTime.Now, _input));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("Error in WebAPI.Writer: {0}", e.Message));
            }
        }
    }
}

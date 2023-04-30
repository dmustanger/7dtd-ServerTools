using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{

    class OutputLog
    {
        public static bool IsEnabled = false, Vehicle_Manager_Off = false;
        public static List<string> ActiveLog = new List<string>();
        public static string lastOutput = "";

        //private static string Debug = string.Format("{0}/{1}", API.ConfigPath, DebugFile);
        //private const string DebugFile = "Debug.txt";

        public static void Exec()
        {
            try
            {
                Log.LogCallbacks += LogAction;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in OutputLog.Exec: {0}", e.Message));
            }
        }

        public static void Shutdown()
        {
            try
            {
                Log.LogCallbacks -= LogAction;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in OutputLog.Shutdown: {0}", e.Message));
            }
        }

        private static void LogAction(string msg, string trace, LogType type)
        {
            try
            {
                ActiveLog.Add(msg);
                //using (StreamWriter sw = new StreamWriter(Debug, true, Encoding.UTF8))
                //{
                //    sw.WriteLine(string.Format("msg: {0} / trace {1} / type {2}", msg, trace, type));
                //    sw.WriteLine();
                //    sw.Flush();
                //    sw.Close();
                //    sw.Dispose();
                //}
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in OutputLog.LogAction: {0}", e.Message));
            }
        }
    }
}

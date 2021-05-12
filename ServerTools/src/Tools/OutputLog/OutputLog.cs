using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{

    class OutputLog
    {
        public static bool IsEnabled = false;
        public static List<string> ActiveLog = new List<string>();
        //private static string Path = string.Format("{0}/Logs/OutputLogs/{1}", API.ConfigPath, LogFile);
        //private const string LogFile = "OutputLog.txt";
        //private static DateTime Date = new DateTime();

        //private static string Debug = string.Format("{0}/{1}", API.ConfigPath, DebugFile);
        //private const string DebugFile = "Debug.txt";

        public static void Exec()
        {
            try
            {
                Logger.Main.LogCallbacks += LogAction;
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
                Logger.Main.LogCallbacks -= LogAction;
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
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in OutputLog.LogAction: {0}", e.Message));
            }
        }
    }
}

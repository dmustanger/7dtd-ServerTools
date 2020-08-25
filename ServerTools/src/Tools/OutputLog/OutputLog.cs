using System;
using System.Collections.Generic;
using System.Linq;
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
                //Date = DateTime.Now;
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
                //string[] _files = Directory.GetFiles(API.ConfigPath + "/Logs/OutputLogs/", "OutputLog.txt", SearchOption.AllDirectories);
                //if (_files != null && _files.Length == 1)
                //{
                //    string _fileName = _files[0];
                //    FileInfo _fInfo = new FileInfo(_fileName);
                //    _fInfo.MoveTo(string.Format("{0}OutputLog_{1}", API.ConfigPath + "/Logs/OutputLogs/", Date.ToString()));
                //}
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
                //if (IsEnabled)
                //{
                //    using (StreamWriter sw = new StreamWriter(Path, true))
                //    {
                //        sw.WriteLine(string.Format("{0}", msg));
                //        sw.Flush();
                //        sw.Close();
                //    }
                //}
                if (BattleLogger.IsEnabled)
                {
                    if (msg.Contains("NET: LiteNetLib: Client disconnect from:") && msg.Contains("(RemoteConnectionClose)"))
                    {
                        string _ip = msg.Replace("NET: LiteNetLib: Client disconnect from: ", "*");
                        _ip = _ip.Split('*').Last();
                        _ip = _ip.Split(':').First();
                        if (!BattleLogger.DisconnectedIp.ContainsKey(_ip))
                        {
                            BattleLogger.DisconnectedIp.Add(_ip, DateTime.Now);
                        }
                        else
                        {
                            BattleLogger.DisconnectedIp[_ip] = DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in OutputLog.LogAction: {0}", e.Message));
            }
        }
    }
}

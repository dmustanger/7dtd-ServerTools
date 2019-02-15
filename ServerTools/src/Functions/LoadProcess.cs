using System;
using System.IO;

namespace ServerTools
{
    class LoadProcess
    {
        public static int Days_Before_Log_Delete = 5;


        public static void Load(int _state)
        {
            if (_state == 1)
            {
                try
                {
                    Log.Out(string.Format("[ServerTools] Checking for save directory {0}", API.ConfigPath));
                    if (!Directory.Exists(API.ConfigPath))
                    {
                        Directory.CreateDirectory(API.ConfigPath);
                        Log.Out(string.Format("[ServerTools] Created directory {0}", API.ConfigPath));
                    }
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath));
                    }
                    Log.Out(string.Format("[ServerTools] Checking for logs directory {0}", API.ConfigPath + "/Logs/ChatLogs"));
                    if (!Directory.Exists(API.ConfigPath + "/Logs/ChatLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/ChatLogs");
                        Log.Out(string.Format("[ServerTools] Created directory {0}", API.ConfigPath + "/Logs/ChatLogs"));
                    }
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath + "/Logs/ChatLogs"));
                    }
                    Log.Out(string.Format("[ServerTools] Checking for logs directory {0}", API.ConfigPath + "/Logs/DetectionLogs"));
                    if (!Directory.Exists(API.ConfigPath + "/Logs/DetectionLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/DetectionLogs");
                        Log.Out(string.Format("[ServerTools] Created directory {0}", API.ConfigPath + "/Logs/DetectionLogs"));
                    }
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath + "/Logs/DetectionLogs"));
                    }
                    Log.Out(string.Format("[ServerTools] Checking for logs directory {0}", API.ConfigPath + "/Logs/BountyLogs"));
                    if (!Directory.Exists(API.ConfigPath + "/Logs/BountyLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/BountyLogs");
                        Log.Out(string.Format("[ServerTools] Created directory {0}", API.ConfigPath + "/Logs/BountyLogs"));
                    }
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath + "/Logs/BountyLogs"));
                    }
                    Log.Out(string.Format("[ServerTools] Checking for logs directory {0}", API.ConfigPath + "/Logs/AuctionLog"));
                    if (!Directory.Exists(API.ConfigPath + "/Logs/AuctionLog"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/AuctionLog");
                        Log.Out(string.Format("[ServerTools] Created directory {0}", API.ConfigPath + "/Logs/AuctionLog"));
                    }
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath + "/Logs/AuctionLog"));
                    }
                    Log.Out(string.Format("[ServerTools] Checking for logs directory {0}", API.ConfigPath + "/Logs/BankLogs"));
                    if (!Directory.Exists(API.ConfigPath + "/Logs/BankLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/BankLogs");
                        Log.Out(string.Format("[ServerTools] Created directory {0}", API.ConfigPath + "/Logs/BankLogs"));
                    }
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath + "/Logs/BankLogs"));
                    }
                    Log.Out(string.Format("[ServerTools] Checking for logs directory {0}", API.ConfigPath + "/Logs/DupeLogs"));
                    if (!Directory.Exists(API.ConfigPath + "/Logs/DupeLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/DupeLogs");
                        Log.Out(string.Format("[ServerTools] Created directory {0}", API.ConfigPath + "/Logs/DupeLogs"));
                    }
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath + "/Logs/DupeLogs"));
                    }
                    Log.Out(string.Format("[ServerTools] Checking for logs directory {0}", API.ConfigPath + "/Logs/PlayerLogs"));
                    if (!Directory.Exists(API.ConfigPath + "/Logs/PlayerLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/PlayerLogs");
                        Log.Out(string.Format("[ServerTools] Created directory {0}", API.ConfigPath + "/Logs/PlayerLogs"));
                    }
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath + "/Logs/PlayerLogs"));
                    }
                    Log.Out(string.Format("[ServerTools] Checking for logs directory {0}", API.ConfigPath + "/Logs/PlayerReports"));
                    if (!Directory.Exists(API.ConfigPath + "/Logs/PlayerReports"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/PlayerReports");
                        Log.Out(string.Format("[ServerTools] Created directory {0}", API.ConfigPath + "/Logs/PlayerReports"));
                    }
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath + "/Logs/PlayerReports"));
                    }
                    Log.Out(string.Format("[ServerTools] Checking for logs directory {0}", API.ConfigPath + "/Logs/PollLogs"));
                    if (!Directory.Exists(API.ConfigPath + "/Logs/PollLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/PollLogs");
                        Log.Out(string.Format("[ServerTools] Created directory {0}", API.ConfigPath + "/Logs/PollLogs"));
                    }
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath + "/Logs/PollLogs"));
                    }


                    string[] files = Directory.GetFiles(API.ConfigPath + "/Logs/DetectionLogs");
                    int _daysBeforeDeleted = (Days_Before_Log_Delete * -1);
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                        {
                            fi.Delete();
                        }
                    }
                    files = Directory.GetFiles(API.ConfigPath + "/Logs/BountyLogs");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                        {
                            fi.Delete();
                        }
                    }
                    files = Directory.GetFiles(API.ConfigPath + "/Logs/AuctionLog");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                        {
                            fi.Delete();
                        }
                    }
                    files = Directory.GetFiles(API.ConfigPath + "/Logs/BankLogs");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                        {
                            fi.Delete();
                        }
                    }
                    files = Directory.GetFiles(API.ConfigPath + "/Logs/DupeLogs");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                        {
                            fi.Delete();
                        }
                    }
                    files = Directory.GetFiles(API.ConfigPath + "/Logs/PlayerLogs");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                        {
                            fi.Delete();
                        }
                    }
                    files = Directory.GetFiles(API.ConfigPath + "/Logs/PlayerReports");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                        {
                            fi.Delete();
                        }
                    }
                    files = Directory.GetFiles(API.ConfigPath + "/Logs/PollLogs");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                        {
                            fi.Delete();
                        }
                    }
                }
                catch
                {
                    Log.Out(string.Format("[ServerTools] Error in creation of directory {0}", API.ConfigPath));
                }
                Load(2);
            }
            if (_state == 2)
            {
                try
                {
                    LoadConfig.Load();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to load the configuration file");
                }
                Load(3);
            }
            if (_state == 3)
            {
                try
                {
                    SQL.Connect();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to connect to an sql database. ST requires this to operate");
                }
                Load(4);
            }
            if (_state == 4)
            {
                try
                {
                    Mods.Load();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to load the tools. Restart the server and check for errors");
                }
                Load(5);
            }
            if (_state == 5)
            {
                try
                {
                    LoadTriggers.LoadXml();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to load the EventTriggers.xml. Check for errors in the file.");
                }
                Load(6);
            }
            if (_state == 6)
            {
                try
                {
                    Phrases.Load();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to load the phrases. Restart the server and check for errors");
                }
                try
                {
                    HowToSetup.Load();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to load the HowToSetup.xml");
                }
                Load(7);
            }
            if (_state == 7)
            {
                if (Fps.IsEnabled)
                {
                    try
                    {
                        Fps.SetTarget();
                    }
                    catch
                    {
                        Log.Out("[ServerTools] Failed to set the target fps");
                    }
                }
                Load(8);
            }
            if (_state == 8)
            {
                Timers.LogAlert();
                Timers.LoadAlert();
                Load(9);
            }
            if (_state == 9)
            {
                RestartVote.Startup = true;
            }
        }
    }
}

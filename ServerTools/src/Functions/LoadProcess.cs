using System;
using System.IO;
using System.Xml;

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
                    Log.Out(string.Format("[ServerTools] Checking for log directories"));
                    if (!Directory.Exists(API.ConfigPath + "/Logs/ChatLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/ChatLogs");
                    }
                    if (!Directory.Exists(API.ConfigPath + "/Logs/DetectionLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/DetectionLogs");
                    }
                    if (!Directory.Exists(API.ConfigPath + "/Logs/BountyLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/BountyLogs");
                    }
                    if (!Directory.Exists(API.ConfigPath + "/Logs/AuctionLog"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/AuctionLog");
                    }
                    if (!Directory.Exists(API.ConfigPath + "/Logs/BankLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/BankLogs");
                    }
                    if (!Directory.Exists(API.ConfigPath + "/Logs/DupeLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/DupeLogs");
                    }
                    if (!Directory.Exists(API.ConfigPath + "/Logs/PlayerLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/PlayerLogs");
                    }
                    if (!Directory.Exists(API.ConfigPath + "/Logs/PlayerReports"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/PlayerReports");
                    }
                    if (!Directory.Exists(API.ConfigPath + "/Logs/PollLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/PollLogs");
                    }
                    if (!Directory.Exists(API.ConfigPath + "/Logs/CommandLogs"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/Logs/CommandLogs");
                    }
                    Log.Out(string.Format("[ServerTools] Directory check completed"));
                    Log.Out(string.Format("[ServerTools] Deleting old logs"));
                    int _daysBeforeDeleted = (Days_Before_Log_Delete * -1);
                    string[] files = Directory.GetFiles(API.ConfigPath + "/Logs/DetectionLogs");
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
                    files = Directory.GetFiles(API.ConfigPath + "/Logs/ChatLogs");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                        {
                            fi.Delete();
                        }
                    }
                    files = Directory.GetFiles(API.ConfigPath + "/Logs/CommandLogs");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                        {
                            fi.Delete();
                        }
                    }
                    Log.Out(string.Format("[ServerTools] Log clean up completed"));
                }
                catch (XmlException e)
                {
                    Log.Out(string.Format("[ServerTools] Error in creation of directory {0}. Error = {1}", API.ConfigPath, e.Message));
                }
                Load(2);
            }
            else if (_state == 2)
            {
                try
                {
                    LoadConfig.Load();
                }
                catch (XmlException e)
                {
                    Log.Out("[ServerTools] Failed to load the configuration file. Error = {0}", e.Message);
                }
                Load(3);
            }
            else if (_state == 3)
            {
                try
                {
                    SQL.Connect();
                }
                catch (XmlException e)
                {
                    Log.Out("[ServerTools] Failed to connect to an sql database. ST requires this to operate. Error = {0}", e.Message);
                }
                //Load(4);
            }
            else if (_state == 4)
            {
                try
                {
                    StateManager.Awake();
                }
                catch (XmlException e)
                {
                    Log.Out("[ServerTools] Failed to load the persistent database bin file. Restart the server and check for errors. Error = {0}", e.Message);
                }
                Load(5);
            }
            else if (_state == 5)
            {
                try
                {
                    Mods.Load();
                }
                catch (XmlException e)
                {
                    Log.Out("[ServerTools] Failed to load the tools. Restart the server and check for errors. Error = {0}", e.Message);
                }
                Load(6);
            }
            else if (_state == 6)
            {
                try
                {
                    LoadTriggers.LoadXml();
                }
                catch (XmlException e)
                {
                    Log.Out("[ServerTools] Failed to load the EventTriggers.xml. Check for errors in the file. Error = {0}", e.Message);
                }
                Load(7);
            }
            else if (_state == 7)
            {
                try
                {
                    Phrases.Load();
                }
                catch (XmlException e)
                {
                    Log.Out("[ServerTools] Failed to load the Phrases.xml. Restart the server and check for errors. Error = {0}", e.Message);
                }
                try
                {
                    HowToSetup.Load();
                }
                catch (XmlException e)
                {
                    Log.Out("[ServerTools] Failed to load the HowToSetup.xml. Error = {0}", e.Message);
                }
                Load(8);
            }
            else if (_state == 8)
            {
                if (Fps.IsEnabled)
                {
                    try
                    {
                        Fps.SetTarget();
                    }
                    catch (XmlException e)
                    {
                        Log.Out("[ServerTools] Failed to set the target fps. Error = {0}", e.Message);
                    }
                }
                Load(9);
            }
            else if (_state == 9)
            {
                RestartVote.Startup = true;
                Load(10);
            }
            else if (_state == 10)
            {
                Timers.LogAlert();
                Timers.LoadAlert();
            }
        }
    }
}

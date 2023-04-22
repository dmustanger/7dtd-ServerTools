using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class AutoBackup
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Backup_Count = 5, Compression_Level = 0;
        public static string Save_Destination = "", Target_Directory = "", Delay = "240";

        private static string EventDelay = "";

        public static void SetDelay(bool _reset)
        {
            if (EventDelay != Delay || _reset)
            {
                EventDelay = Delay;
                EventSchedule.Clear("AutoBackup_");
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        string[] timeSplit = times[i].Split(':');
                        int.TryParse(timeSplit[0], out int hours);
                        int.TryParse(timeSplit[1], out int minutes);
                        DateTime time = DateTime.Today.AddHours(hours).AddMinutes(minutes);
                        EventSchedule.Schedule.Add("AutoBackup_" + time, time);
                    }
                }
                else if (Delay.Contains(":"))
                {
                    string[] timeSplit = Delay.Split(':');
                    int.TryParse(timeSplit[0], out int hours);
                    int.TryParse(timeSplit[1], out int minutes);
                    DateTime time = DateTime.Today.AddHours(hours).AddMinutes(minutes);
                    EventSchedule.Schedule.Add("AutoBackup_" + time, time);
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        DateTime time = DateTime.Now.AddMinutes(delay);
                        EventSchedule.Schedule.Add("AutoBackup_" + time, time);
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Invalid AutoBackup Delay_Between_Saves detected. Use a single integer, 24h time or multiple 24h time entries"));
                        Log.Out(string.Format("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00"));
                    }
                }
            }
        }

        public static void Exec()
        {
            if (!IsRunning)
            {
                if (!Shutdown.ShuttingDown)
                {
                    IsRunning = true;
                    Log.Out(string.Format("[SERVERTOOLS] Starting auto backup process"));
                    ThreadManager.AddSingleTask(delegate (ThreadManager.TaskInfo _taskInfo)
                    {
                        Prepare();
                        IsRunning = false;
                    });
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Autobackup skipped due to Shutdown process"));
                }
            }
            else
            {
                Log.Out(string.Format("[SERVERTOOLS] Autobackup was unable to start. Prior backup process did not complete or is stuck"));
            }
        }

        private static void Prepare()
        {
            try
            {
                string saveDirectory = Target_Directory;
                if (string.IsNullOrEmpty(saveDirectory))
                {
                    saveDirectory = GameIO.GetSaveGameDir();
                    if (string.IsNullOrEmpty(saveDirectory))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Auto backup can not locate a working directory to save. Provide a Save_Directory in your config file and try again."));
                        return;
                    }
                }
                DirectoryInfo saveDirInfo = new DirectoryInfo(saveDirectory);//save dir
                if (saveDirInfo != null)
                {
                    if (string.IsNullOrEmpty(Save_Destination))
                    {
                        if (!Directory.Exists(API.ConfigPath + "/WorldBackup"))
                        {
                            Directory.CreateDirectory(API.ConfigPath + "/WorldBackup");
                            Log.Out(string.Format("[SERVERTOOLS] Auto backup destination folder not found. The folder has been created at '{0}' and backup resumed", API.ConfigPath + "/WorldBackup"));
                        }
                        string[] files = Directory.GetFiles(API.ConfigPath + "/WorldBackup/", "*.zip", SearchOption.AllDirectories);//get files from destination directory. This is the default destination
                        if (files != null && files.Length > Backup_Count)//files are not null and too many exist
                        {
                            DeleteFiles(files);//exec file delete
                            Log.Out(string.Format("[SERVERTOOLS] Auto backup clean up complete"));
                        }
                        DirectoryInfo destDirInfo = new DirectoryInfo(API.ConfigPath + "/WorldBackup/");//destination dir
                        if (destDirInfo != null)
                        {
                            Save(destDirInfo.FullName, saveDirectory);//exec save method
                        }
                    }
                    else
                    {
                        string destination = Save_Destination.RemoveLineBreaks();
                        if (!destination.EndsWith("/"))
                        {
                            destination += "/";
                        }
                        if (!Directory.Exists(destination))
                        {
                            Directory.CreateDirectory(destination);
                            Log.Out(string.Format("[SERVERTOOLS] Auto backup destination folder not found. The folder has been created at '{0}'. Auto backup resumed", destination));
                        }
                        string[] files = Directory.GetFiles(destination, "*.zip", SearchOption.AllDirectories);//get files from save directory. This is a custom location
                        if (files != null && files.Length > Backup_Count)//files are not null or empty
                        {
                            DeleteFiles(files);//exec file delete
                            Log.Out(string.Format("[SERVERTOOLS] Auto backup clean up complete"));
                        }
                        DirectoryInfo destDirInfo = new DirectoryInfo(destination);//destination dir
                        if (destDirInfo != null)
                        {
                            Save(destDirInfo.FullName, saveDirectory);//exec file save
                        }
                        else
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Auto backup could not locate the save directory '{0}'", destination));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.Prepare: {0}", e.Message));
            }
        }

        private static void DeleteFiles(string[] _files)
        {
            if (_files != null)
            {
                List<FileInfo> fileList = new List<FileInfo>();
                for (int i = 0; i < _files.Length; i++)
                {
                    string fileName = _files[i];
                    if (fileName.Contains("Backup"))
                    {
                        FileInfo fInfo = new FileInfo(fileName);
                        fileList.Add(fInfo);
                    }
                }
                if (fileList.Count > 0 && fileList.Count > Backup_Count)
                {
                    fileList.Sort((x, y) => DateTime.Compare(x.CreationTime, y.CreationTime));
                    fileList.Reverse();
                    for (int j = 0; j < fileList.Count; j++)
                    {
                        if (j >= Backup_Count - 1)
                        {
                            FileInfo fInfo = fileList[j];
                            Log.Out(string.Format("[SERVERTOOLS] Auto backup cleanup has deleted {0}", fInfo.Name));
                            fInfo.Delete();
                        }
                    }
                }
            }
        }

        private static void Save(string _destinationDirInfo, string _saveDirectory)
        {
            string[] fileNames = Directory.GetFiles(_saveDirectory, "*", SearchOption.AllDirectories);
            if (fileNames.Length > 0)
            {
                for (int i = 0; i < fileNames.Length; i++)
                {
                    if (fileNames[i].Contains("main.ttw"))
                    {
                        Phrases.Dict.TryGetValue("AutoBackup1", out string phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        string name = string.Format("Backup_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm"));
                        string destination = _destinationDirInfo + string.Format("/Backup_{0}.zip", DateTime.Now.ToString("MM-dd-yy_HH-mm"));
                        using (ZipOutputStream OutputStream = new ZipOutputStream(File.Create(destination)))
                        {
                            OutputStream.UseZip64 = UseZip64.On;
                            if (Compression_Level > 9)
                            {
                                OutputStream.SetLevel(9);
                            }
                            else if (Compression_Level < 0)
                            {
                                OutputStream.SetLevel(0);
                            }
                            else
                            {
                                OutputStream.SetLevel(Compression_Level);
                            }
                            byte[] buffer = new byte[8192];
                            for (int j = 0; j < fileNames.Length; j++)
                            {
                                string file = fileNames[j];
                                ZipEntry entry = new ZipEntry(file);

                                entry.DateTime = DateTime.Now;
                                OutputStream.PutNextEntry(entry);
                                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    int sourceBytes;
                                    do
                                    {
                                        sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                        OutputStream.Write(buffer, 0, sourceBytes);
                                    }
                                    while (sourceBytes > 0);
                                }
                            }
                            fileNames = Directory.GetFiles(API.ConfigPath, "ServerTools.bin", SearchOption.AllDirectories);
                            if (fileNames != null)
                            {
                                for (int j = 0; j < fileNames.Length; j++)
                                {
                                    string file = fileNames[j];
                                    ZipEntry entry = new ZipEntry(file);

                                    entry.DateTime = DateTime.Now;
                                    OutputStream.PutNextEntry(entry);
                                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                    {
                                        int sourceBytes;
                                        do
                                        {
                                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                            OutputStream.Write(buffer, 0, sourceBytes);
                                        }
                                        while (sourceBytes > 0);
                                    }
                                }
                            }
                            OutputStream.Dispose();
                        }
                        Log.Out(string.Format("[SERVERTOOLS] Auto backup completed successfully. File is located at '{0}'. File is named '{1}'", _destinationDirInfo, name + ".zip"));
                        Phrases.Dict.TryGetValue("AutoBackup2", out phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        return;
                    }
                }
            }
        }
    }
}

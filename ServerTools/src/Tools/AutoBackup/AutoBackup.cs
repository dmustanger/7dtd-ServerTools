using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ServerTools
{
    class AutoBackup
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Backup_Count = 5, Compression_Level = 0;
        public static string Destination = "", SaveDirectory = GameIO.GetSaveGameDir(), Delay = "240";
        private static Thread th;

        public static void SetDelay()
        {
            if (EventSchedule.autoBackup != Delay)
            {
                EventSchedule.autoBackup = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("AutoBackup", time);
                                return;
                            }
                        }
                    }
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("AutoBackup", time);
                                return;
                            }
                        }
                    }
                }
                else if (Delay.Contains(":"))
                {
                    if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + Delay + ":00", out DateTime time))
                    {
                        if (DateTime.Now < time)
                        {
                            EventSchedule.Add("AutoBackup", time);
                        }
                        else if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + Delay + ":00", out DateTime secondaryTime))
                        {
                            EventSchedule.Add("AutoBackup", secondaryTime);
                        }
                    }
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        EventSchedule.Add("AutoBackup", DateTime.Now.AddMinutes(delay));
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid AutoBackup Delay_Between_Saves detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                }
            }
        }

        public static void Exec()
        {
            if (!IsRunning && !Shutdown.ShuttingDown)
            {
                try
                {
                    Log.Out("[SERVERTOOLS] Starting auto backup process");
                    IsRunning = true;
                    th = new Thread(new ThreadStart(Prepare))
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.BelowNormal
                    };
                    th.Start();
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.Exec: {0}", e.Message));
                    if (e.Message.Contains("112"))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Auto backup does not have enough free space to work with on the selected drive. Please make space available before operating the backup process."));
                    }
                }
                IsRunning = false;
            }
        }

        private static void Prepare()
        {
            try
            {
                DirectoryInfo saveDirInfo = new DirectoryInfo(SaveDirectory);//save dir
                if (string.IsNullOrEmpty(Destination) && saveDirInfo != null)
                {
                    if (!Directory.Exists(API.ConfigPath + "/WorldBackup"))
                    {
                        Directory.CreateDirectory(API.ConfigPath + "/WorldBackup");
                        Log.Out(string.Format("[SERVERTOOLS] Auto backup destination folder not found. The folder has been created at {0} and backup resumed", API.ConfigPath + "/WorldBackup"));
                    }
                    string[] files = Directory.GetFiles(API.ConfigPath + "/WorldBackup/", "*.zip", SearchOption.AllDirectories);//get files from save directory. This is the default destination
                    if (files != null && files.Length > Backup_Count)//files are not null or empty
                    {
                        DeleteFiles(files);//exec file delete
                        Log.Out("[SERVERTOOLS] Auto backup clean up complete");
                    }
                    DirectoryInfo destDirInfo = new DirectoryInfo(API.ConfigPath + "/WorldBackup/");//destination dir
                    if (destDirInfo != null)
                    {
                        Save(destDirInfo.FullName);//exec save method
                    }
                }
                else if (saveDirInfo != null && !string.IsNullOrEmpty(Destination))
                {
                    if (!Directory.Exists(Destination))
                    {
                        Directory.CreateDirectory(Destination);
                        Log.Out(string.Format("[SERVERTOOLS] Auto backup destination folder not found. The folder has been created at '{0}'. Auto backup resumed", Destination));
                    }
                    string[] files = { };
                    if (!Destination.EndsWith("\\") || !Destination.EndsWith("\n") || !Destination.EndsWith("/"))
                    {
                        files = Directory.GetFiles(Destination + "/", "*.zip", SearchOption.AllDirectories);//get files from save directory. This is a custom location
                    }
                    else
                    {
                        files = Directory.GetFiles(Destination, "*.zip", SearchOption.AllDirectories);//get files from save directory. This is a custom location
                    }
                    if (files != null && files.Length > Backup_Count)//files are not null or empty
                    {
                        DeleteFiles(files);//exec file delete
                        Log.Out("[SERVERTOOLS] Auto backup clean up complete");
                    }
                    DirectoryInfo destDirInfo = new DirectoryInfo(Destination);//destination dir
                    if (destDirInfo != null)
                    {
                        Save(destDirInfo.FullName);//exec file save
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Auto backup could not locate the save directory '{0}'", Destination));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.Prepare: {0}", e.Message));
                th.Abort();
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

        private static void Save(string _destinationDirInfo)
        {
            Phrases.Dict.TryGetValue("AutoBackup1", out string phrase);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            string name = string.Format("Backup_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm"));
            string[] fileNames = Directory.GetFiles(SaveDirectory, "*", SearchOption.AllDirectories);
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
                for (int i = 0; i < fileNames.Length; i++)
                {
                    string file = fileNames[i];
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
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        string file = fileNames[i];
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
                OutputStream.Finish();
                OutputStream.Close();
            }
            Log.Out(string.Format("[SERVERTOOLS] Auto backup completed successfully. File is located at '{0}'. File is named '{1}'", _destinationDirInfo, name + ".zip"));
            Phrases.Dict.TryGetValue("AutoBackup2", out phrase);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }
    }
}

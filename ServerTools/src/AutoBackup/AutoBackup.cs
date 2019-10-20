using System;
using System.IO;

namespace ServerTools
{
    class AutoBackup
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Time_Between_Saves = 240, Days_Before_Save_Delete = 3, Compression_Level = 0;
        public static string Destination = "";
        private static string saveDirectory = GameUtils.GetSaveGameDir();

        public static void Exec()
        {
            if (!IsRunning && !StopServer.stopServerCountingDown)
            {
                try
                {
                    IsRunning = true;
                    Log.Out("[SERVERTOOLS] Starting world backup process");
                    string _parentDirectory = Directory.GetParent(saveDirectory).FullName;
                    string[] _files = Directory.GetFiles(_parentDirectory, "*.zip");
                    DeleteFiles(_files);
                    Log.Out("[SERVERTOOLS] Backup has begun");
                    compressDirectory(saveDirectory, Destination);
                }
                catch (Exception e)
                {
                    IsRunning = false;
                    Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.Run: {0}.", e));
                }
            }
        }

        private static void compressDirectory(string saveDirectory, string Destination)
        {
            try
            {
                string[] _files = Directory.GetFiles(saveDirectory, "*", SearchOption.AllDirectories);
                string _parentDirectory = Directory.GetParent(saveDirectory).FullName;
                Pathfinding.Ionic.Zlib.CompressionLevel _compression = Pathfinding.Ionic.Zlib.CompressionLevel.Default;
                if (Compression_Level == 0)
                {
                    _compression = Pathfinding.Ionic.Zlib.CompressionLevel.None;
                }
                else if (Compression_Level == 1)
                {
                    _compression = Pathfinding.Ionic.Zlib.CompressionLevel.BestSpeed;
                }
                else if (Compression_Level >= 2)
                {
                    _compression = Pathfinding.Ionic.Zlib.CompressionLevel.BestCompression;
                }
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "Starting world backup..." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                if (Destination == "")
                {
                    using (Pathfinding.Ionic.Zip.ZipFile zip = new Pathfinding.Ionic.Zip.ZipFile(_parentDirectory + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm"))))
                    {
                        zip.CompressionLevel =  _compression;
                        foreach (var _c in _files)
                        {
                            zip.AddFile(_c);
                        }
                        zip.Save(Path.ChangeExtension(_parentDirectory + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm")), ".zip"));
                        Log.Out(string.Format("[SERVERTOOLS] World backup completed successfully. File is located and named {0}", _parentDirectory + "_" + DateTime.Now + ".zip"));
                    }
                }
                else
                {
                    using (Pathfinding.Ionic.Zip.ZipFile zip = new Pathfinding.Ionic.Zip.ZipFile(Destination + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm"))))
                    {
                        if (!Directory.Exists(Destination))
                        {
                            Directory.CreateDirectory(Destination);
                            Log.Out(string.Format("[SERVERTOOLS] World backup destination folder not found. The folder has been created at {0} and backup resumed", Destination));
                        }
                        zip.CompressionLevel = _compression;
                        foreach (var _c in _files)
                        {
                            zip.AddFile(_c);
                        }
                        zip.Save(Path.ChangeExtension(Destination + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm")), ".zip"));
                        Log.Out(string.Format("[SERVERTOOLS] World backup completed successfully. File is located and named {0}", Destination + "_" + DateTime.Now + ".zip"));
                    }
                }
                Log.Out(string.Format("[SERVERTOOLS] World backup completed successfully. File is located and named {0}", _parentDirectory + "_" + DateTime.Now + ".zip"));
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "World backup completed successfully" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
            catch (Exception e)
            {
                IsRunning = false;
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.Run: {0}.", e));
            }
            IsRunning = false;
        }

        public static void DeleteFiles(string[] _files)
        {
            int _daysBeforeDeleted = (Days_Before_Save_Delete * -1);
            if (_files != null)
            {
                foreach (string _c in _files)
                {
                    FileInfo _d = new FileInfo(_c);
                    if (_d.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Old backup named {0} was deleted due to its age", _d.Name));
                        _d.Delete();
                    }
                }
            }
            Log.Out("[SERVERTOOLS] Old backup clean up complete");
        }
    }
}

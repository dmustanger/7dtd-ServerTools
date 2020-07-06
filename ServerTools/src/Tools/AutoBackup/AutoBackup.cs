using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class AutoBackup
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Delay = 240, Backup_Count = 5, Compression_Level = 0;
        public static string Destination = "";
        private static string SaveDirectory = GameUtils.GetSaveGameDir();

        public static void Exec()
        {
            if (!IsRunning && !StopServer.CountingDown)
            {
                try
                {
                    Log.Out("[SERVERTOOLS] Starting auto backup process");
                    IsRunning = true;
                    DirectoryInfo _saveDirInfo = new DirectoryInfo(SaveDirectory);//save dir
                    if (string.IsNullOrEmpty(Destination) && _saveDirInfo != null)
                    {
                        if (!Directory.Exists(API.ConfigPath + "/WorldBackup"))
                        {
                            Directory.CreateDirectory(API.ConfigPath + "/WorldBackup");
                            Log.Out(string.Format("[SERVERTOOLS] Auto backup destination folder not found. The folder has been created at {0} and backup resumed", API.ConfigPath + "/WorldBackup"));
                        }
                        string[] _files = Directory.GetFiles(API.ConfigPath + "/WorldBackup/", "*.zip", SearchOption.AllDirectories);//get files from save directory. This is the default destination
                        if (_files != null && _files.Length > Backup_Count)//files are not null or empty
                        {
                            DeleteFiles(_files);//exec file delete
                            Log.Out("[SERVERTOOLS] Auto backup clean up complete");
                        }
                        DirectoryInfo _destDirInfo = new DirectoryInfo(API.ConfigPath + "/WorldBackup/");//destination dir
                        if (_destDirInfo != null)
                        {
                            Save(_destDirInfo);//exec save method
                        }
                    }
                    else if (_saveDirInfo != null && !string.IsNullOrEmpty(Destination))
                    {
                        if (!Directory.Exists(Destination))
                        {
                            Directory.CreateDirectory(Destination);
                            Log.Out(string.Format("[SERVERTOOLS] Auto backup destination folder not found. The folder has been created at {0} and backup resumed", Destination));
                        }
                        string[] _files = { };
                        if (!Destination.EndsWith("\\") || !Destination.EndsWith("\n") || !Destination.EndsWith("/"))
                        {
                            _files = Directory.GetFiles(Destination + "/", "*.zip", SearchOption.AllDirectories);//get files from save directory. This is a custom location
                        }
                        else
                        {
                            _files = Directory.GetFiles(Destination, "*.zip", SearchOption.AllDirectories);//get files from save directory. This is a custom location
                        }
                        if (_files != null && _files.Length > Backup_Count)//files are not null or empty
                        {
                            DeleteFiles(_files);//exec file delete
                            Log.Out("[SERVERTOOLS] Auto backup clean up complete");
                        }
                        DirectoryInfo _destDirInfo = new DirectoryInfo(Destination);//destination dir
                        if (_destDirInfo != null)
                        {
                            Save(_destDirInfo);//exec save method
                        }
                    }
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

        private static void DeleteFiles(string[] _files)
        {
            try
            {
                if (_files != null)
                {
                    List<FileInfo> _fileList = new List<FileInfo>();
                    for (int i = 0; i < _files.Length; i++)
                    {
                        string _fileName = _files[i];
                        if (_fileName.Contains("Backup"))
                        {
                            FileInfo _fInfo = new FileInfo(_fileName);
                            _fileList.Add(_fInfo);
                        }
                    }
                    if (_fileList.Count > 0 && _fileList.Count > Backup_Count)
                    {
                        _fileList.Sort((x, y) => DateTime.Compare(x.CreationTime, y.CreationTime));
                        _fileList.Reverse();
                        for (int j = 0; j < _fileList.Count; j++)
                        {
                            if (j >= Backup_Count - 1)
                            {
                                FileInfo _fInfo = _fileList[j];
                                Log.Out(string.Format("[SERVERTOOLS] Auto backup cleanup has deleted {0}", _fInfo.Name));
                                _fInfo.Delete();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.DeleteFiles: {0}", e.Message));
            }
        }

        private static void Save(DirectoryInfo _destinationDirInfo)
        {
            try
            {
                string[] _files = Directory.GetFiles(SaveDirectory, "*", SearchOption.AllDirectories);
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
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "Starting auto backup. You might experience periods of lag and slow down until complete" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                string _location = _destinationDirInfo.FullName + string.Format("/Backup_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm"));
                string _name = string.Format("Backup_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm"));
                using (Pathfinding.Ionic.Zip.ZipFile zip = new Pathfinding.Ionic.Zip.ZipFile(_location))
                {
                    zip.UseZip64WhenSaving = Pathfinding.Ionic.Zip.Zip64Option.Always;
                    zip.CompressionLevel = _compression;
                    zip.ParallelDeflateThreshold = -1;
                    for (int i = 0; i < _files.Length; i++)
                    {
                        string _file = _files[i];
                        zip.AddFile(_file).FileName = _file.Substring(_file.IndexOf(SaveDirectory));
                    }
                    zip.Save(Path.ChangeExtension(_location, ".zip"));
                    Log.Out(string.Format("[SERVERTOOLS] Auto backup completed successfully. File is located at {0}. File is named {1}", _destinationDirInfo.FullName, _name + ".zip"));
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "Auto backup completed successfully" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.Save: {0}", e.Message));
                if (e.Message.Contains("112"))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Auto backup does not have enough free space to work with on the selected drive. Please make space available before operating the backup process."));
                }
            }
        }
    }
}

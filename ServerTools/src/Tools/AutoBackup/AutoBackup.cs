using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServerTools
{
    class AutoBackup
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Delay = 240, Backup_Count = 5, Compression_Level = 0;
        public static string Destination = "";
        private static string saveDirectory = GameUtils.GetSaveGameDir();

        public static void Exec()
        {
            if (!IsRunning && !StopServer.StopServerCountingDown)
            {
                try
                {
                    IsRunning = true;
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Starting world backup process");
                    Log.Out("[SERVERTOOLS] Starting world backup process");
                    string _parentDirectory = Directory.GetParent(saveDirectory).FullName;
                    string[] _files = Directory.GetFiles(_parentDirectory, "*.zip");
                    DeleteFiles(_files);
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Old backup clean up complete");
                    Log.Out("[SERVERTOOLS] Old backup clean up complete");
                    CompressDirectory(saveDirectory, Destination);
                    IsRunning = false;
                }
                catch (Exception e)
                {
                    IsRunning = false;
                    Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.Exec: {0}.", e));
                    if (e.Message.Contains("112"))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Auto backup does not have enough free space to work with on the selected drive. Please make space available before operating the backup process."));
                    }
                }
            }
        }

        private static void CompressDirectory(string saveDirectory, string Destination)
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
                string _destination;
                if (Destination == "")
                {
                    _destination = "Backup" + _parentDirectory + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm"));
                }
                else
                {
                    _destination = "Backup" + Destination + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm"));
                    if (!Directory.Exists(Destination))
                    {
                        Directory.CreateDirectory(Destination);
                        Log.Out(string.Format("[SERVERTOOLS] World backup destination folder not found. The folder has been created at {0} and backup resumed", Destination));
                    }
                }
                using (Pathfinding.Ionic.Zip.ZipFile zip = new Pathfinding.Ionic.Zip.ZipFile(_destination))
                {
                    zip.UseZip64WhenSaving = Pathfinding.Ionic.Zip.Zip64Option.Always;
                    zip.CompressionLevel = _compression;
                    zip.ParallelDeflateThreshold = -1;
                    foreach (var _c in _files)
                    {
                        zip.AddFile(_c);
                    }
                    zip.Save(Path.ChangeExtension(_destination, ".zip"));
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] World backup completed successfully. File is located and named {0}", _destination + "_" + DateTime.Now + ".zip"));
                    Log.Out(string.Format("[SERVERTOOLS] World backup completed successfully. File is located and named {0}", _destination + "_" + DateTime.Now + ".zip"));
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "World backup completed successfully" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.compressDirectory: {0}.", e));
                if (e.Message.Contains("112"))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Auto backup does not have enough free space to work with on the selected drive. Please make space available before operating the backup process."));
                }
            }
        }

        public static void DeleteFiles(string[] _files)
        {
            try
            {
                if (_files != null)
                {
                    int _counter = 0;
                    List<FileInfo> _fileList = new List<FileInfo>();
                    for (int i = 0; i < _files.Length; i++)
                    {
                        string _fileName = _files[i];
                        if (_fileName.ToLower().Contains("Backup"))
                        {
                            FileInfo _fInfo = new FileInfo(_fileName);
                            _fileList.Add(_fInfo);
                            if (_fileList.Count > Backup_Count)
                            {
                                var sortedDates = _fileList.OrderByDescending(x => x);
                                foreach (FileInfo _info in sortedDates)
                                {
                                    _counter++;
                                    if (_counter > Backup_Count)
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Old backup named {0} was deleted to make space", _info.Name));
                                        _info.Delete();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.DeleteFiles: {0}.", e));
                if (e.Message.Contains("112"))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Auto backup does not have enough free space to work with on the selected drive. Please make space available before operating the backup process."));
                }
            }
        }
    }
}

using Ionic.Zip;
using System;
using System.IO;

namespace ServerTools
{
    class AutoBackup
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Time_Between_Saves = 240, Days_Before_Save_Delete = 3, Compression_Level = 0;
        public static string Destination = "";

        public static void BackupExec()
        {
            if (!IsRunning && !StopServer.stopServerCountingDown)
            {
                IsRunning = true;
                try
                {
                    string saveDirectory = GameUtils.GetSaveGameDir();
                    string[] _files1 = Directory.GetFiles(saveDirectory, "*", SearchOption.AllDirectories);
                    string[] _files2 = { };
                    string _parentDirectory = Directory.GetParent(saveDirectory).FullName;
                    Ionic.Zlib.CompressionLevel _compression = Ionic.Zlib.CompressionLevel.Default;
                    if (Compression_Level == 0)
                    {
                        _compression = Ionic.Zlib.CompressionLevel.None;
                    }
                    else if (Compression_Level == 1)
                    {
                        _compression = Ionic.Zlib.CompressionLevel.BestSpeed;
                    }
                    else if (Compression_Level >= 2)
                    {
                        _compression = Ionic.Zlib.CompressionLevel.BestCompression;
                    }
                    Log.Out("[SERVERTOOLS] Starting world backup");
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "Starting world backup..." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    if (Destination == "")
                    {
                        using (ZipFile zip = new ZipFile(_parentDirectory + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm"))))
                        {
                            _files2 = Directory.GetFiles(_parentDirectory, "*.zip");
                            DeleteFiles(_files2);
                            zip.CompressionLevel = _compression;
                            foreach (var _c in _files1)
                            {
                                zip.AddFile(_c);
                            }
                            zip.Save(Path.ChangeExtension(_parentDirectory + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm")), ".zip"));
                            Log.Out(string.Format("[SERVERTOOLS] World backup completed successfully. File is located and named {0}", _parentDirectory + "_" + DateTime.Now + ".zip"));
                        }
                    }
                    else
                    {
                        using (ZipFile zip = new ZipFile(Destination + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm"))))
                        {
                            if (!Directory.Exists(Destination))
                            {
                                Directory.CreateDirectory(Destination);
                                Log.Out(string.Format("[SERVERTOOLS] World backup destination folder not found. The folder has been created at {0} and backup resumed", Destination));
                            }
                            _files2 = Directory.GetFiles(Destination, "*.zip");
                            DeleteFiles(_files2);
                            zip.CompressionLevel = _compression;
                            foreach (var _c in _files1)
                            {
                                zip.AddFile(_c);
                            }
                            zip.Save(Path.ChangeExtension(Destination + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm")), ".zip"));
                            Log.Out(string.Format("[SERVERTOOLS] World backup completed successfully. File is located and named {0}", Destination + "_" + DateTime.Now + ".zip"));
                        }
                    }
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "World backup completed successfully" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.Run: {0}.", e));
                }
                IsRunning = false;
            }
        }

        public static void DeleteFiles(string[] _files2)
        {
            int _daysBeforeDeleted = (Days_Before_Save_Delete * -1);
            if (_files2 != null)
            {
                foreach (string a in _files2)
                {
                    FileInfo b = new FileInfo(a);
                    if (b.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Old backup named {0} was deleted due to its age", b.Name));
                        b.Delete();
                    }
                }
            }
        }
    }
}

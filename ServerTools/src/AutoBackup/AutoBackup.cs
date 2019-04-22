using Ionic.Zip;
using System;
using System.IO;

namespace ServerTools
{
    class AutoBackup
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Time_Between_Saves = 240, Days_Before_Save_Delete = 3;
        public static string Destination = "";

        public static void BackupExec()
        {
            if (!IsRunning && !StopServer.stopServerCountingDown)
            {
                IsRunning = true;
                try
                {
                    using (ZipFile zip = new ZipFile())
                    {
                        Log.Out("[SERVERTOOLS] Starting world backup");
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "Starting world backup..." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        string saveDirectory = GameUtils.GetSaveGameDir();
                        string[] _files1 = Directory.GetFiles(saveDirectory, "*", SearchOption.AllDirectories);
                        string _parentDirectory = Directory.GetParent(saveDirectory).FullName;
                        string[] _files2 = { };
                        if (Destination == "")
                        {
                            _files2 = Directory.GetFiles(_parentDirectory, "*.zip");
                        }
                        else
                        {
                            if (!Directory.Exists(Destination))
                            {
                                Directory.CreateDirectory(Destination);
                                Log.Out(string.Format("[SERVERTOOLS] World backup destination folder not found. The folder has been created at {0} and backup resumed", Destination));
                            }
                            _files2 = Directory.GetFiles(Destination, "*.zip");
                        }
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
                        if (Destination == "")
                        {
                            foreach (var c in _files1)
                            {
                                zip.AddFile(c, Path.GetDirectoryName(c).Replace(_parentDirectory, string.Empty));
                            }
                            zip.Save(Path.ChangeExtension(_parentDirectory + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm")), ".zip"));
                            Log.Out(string.Format("[SERVERTOOLS] World backup completed successfully. File is located and named {0}", _parentDirectory + "_" + DateTime.Now + ".zip"));
                        }
                        else
                        {
                            foreach (var c in _files1)
                            {
                                zip.AddFile(c, Path.GetDirectoryName(c).Replace(Destination, string.Empty));
                            }
                            zip.Save(Path.ChangeExtension(Destination + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm")), ".zip"));
                            Log.Out(string.Format("[SERVERTOOLS] World backup completed successfully. File is located and named {0}", Destination + "_" + DateTime.Now + ".zip"));
                        }
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "World backup completed successfully" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in AutoBackup.Run: {0}.", e));
                }
                IsRunning = false;
            }
        }
    }
}

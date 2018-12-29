using Ionic.Zip;
using System;
using System.IO;

namespace ServerTools
{
    class AutoBackup
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Time_Between_Saves = 240, Days_Before_Save_Delete = 3;
        private static string saveDirectory = string.Format("{0}", GameUtils.GetSaveGameDir());

        public static void Backup()
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
                        string[] _files1 = Directory.GetFiles(saveDirectory, "*", SearchOption.AllDirectories);
                        string _parentDirectory = Directory.GetParent(saveDirectory).FullName;
                        string[] _files2 = Directory.GetFiles(_parentDirectory, "*.zip");
                        int _daysBeforeDeleted = (Days_Before_Save_Delete * -1);
                        if (_files2 != null)
                        {
                            foreach (string a in _files2)
                            {
                                FileInfo b = new FileInfo(a);
                                if (b.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                                {
                                    Log.Out("[SERVERTOOLS] Starting world backup. Old backups deleted");
                                    b.Delete();
                                }
                            }
                        }
                        foreach (var c in _files1)
                        {
                            zip.AddFile(c, Path.GetDirectoryName(c).Replace(saveDirectory, string.Empty));
                        }
                        zip.Save(Path.ChangeExtension(saveDirectory + string.Format("_{0}", DateTime.Now.ToString("MM-dd-yy_HH-mm")), ".zip"));
                        Log.Out("[SERVERTOOLS] World backup completed successfully");
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "World backup completed successfully" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in Backup.Run: {0}.", e));
                }
                IsRunning = false;
            }
        }
    }
}

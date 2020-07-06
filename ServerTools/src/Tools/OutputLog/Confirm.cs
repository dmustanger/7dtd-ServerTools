using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class Confirm
    {
        public static bool LogFound = false;
        public static string LogDirectory = "";
        public static string LogName = "";

        public static void Exec()
        {
            if (!LogFound && !string.IsNullOrEmpty(Utils.GetApplicationScratchPath()))
            {
                LogDirectory = Utils.GetApplicationScratchPath();
                string[] _txtFiles = Directory.GetFiles(LogDirectory, "*.txt", SearchOption.AllDirectories);
                if (_txtFiles != null)
                {
                    string _fileName = "";
                    DateTime _latestDateTime = DateTime.MinValue;
                    for (int i = 0; i < _txtFiles.Length; i++)
                    {
                        FileInfo _fileInfo = new FileInfo(_txtFiles[i]);
                        if (_fileInfo != null && _fileInfo.CreationTime > _latestDateTime)
                        {
                            _fileName = _fileInfo.FullName;
                            _latestDateTime = _fileInfo.CreationTime;
                        }
                    }
                    if (_fileName != "")
                    {
                        using (FileStream fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                            {
                                string _line = sr.ReadToEnd();
                                if (_line != null)
                                {
                                    if (_line.ToLower().Contains("dedicated server only build") || _line.ToLower().Contains("awake"))
                                    {
                                        LogName = _fileName;
                                        LogFound = true;
                                        Log.Out("-------------------------------");
                                        Log.Out("[SERVERTOOLS] Verified log file");
                                        Log.Out("-------------------------------");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

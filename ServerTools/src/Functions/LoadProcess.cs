using System.IO;

namespace ServerTools
{
    class LoadProcess
    {
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
                    else
                    {
                        Log.Out(string.Format("[ServerTools] Directory found", API.ConfigPath));
                    }
                }
                catch
                {
                    Log.Out(string.Format("[ServerTools] Error in creation of directory {0}", API.ConfigPath));
                }
                Load(2);
            }
            if (_state == 2)
            {
                try
                {
                    LoadConfig.Load();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to load the configuration file");
                }
                Load(3);
            }
            if (_state == 3)
            {
                try
                {
                    SQL.Connect();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to connect to an sql database. ST requires this to operate");
                }
                Load(4);
            }
            if (_state == 4)
            {
                if (File.Exists(string.Format("{0}/ServerTools.bin", GameUtils.GetSaveGameDir())))
                {
                    UpdateToSqlFromBin.Exec();
                }
                Load(5);
            }
            if (_state == 5)
            {
                try
                {
                    Mods.Load();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to load the tools. Restart the server and check for errors");
                }
                Load(6);
            }
            if (_state == 6)
            {
                try
                {
                    LoadTriggers.LoadXml();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to load the EventTriggers.xml. Check for errors in the file.");
                }
                Load(7);
            }
            if (_state == 7)
            {
                try
                {
                    Phrases.Load();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to load the phrases. Restart the server and check for errors");
                }
                try
                {
                    HowToSetup.Load();
                }
                catch
                {
                    Log.Out("[ServerTools] Failed to load the HowToSetup.xml");
                }
                Load(8);
            }
            if (_state == 8)
            {
                if (Fps.IsEnabled)
                {
                    try
                    {
                        Fps.SetTarget();
                    }
                    catch
                    {
                        Log.Out("[ServerTools] Failed to set the target fps");
                    }
                }
                Load(9);
            }
            if (_state == 9)
            {
                Timers.LogAlert();
                Timers.LoadAlert();
                Load(10);
            }
            if (_state == 10)
            {
                RestartVote.Startup = true;
            }
        }
    }
}

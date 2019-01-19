using System.IO;

namespace ServerTools
{
    class LoadProcess
    {
        public static void Load(int _state)
        {
            if (_state == 1)
            {
                if (!Directory.Exists(API.ConfigPath))
                {
                    Directory.CreateDirectory(API.ConfigPath);
                }
                Load(2);
            }
            if (_state == 2)
            {
                LoadConfig.Load();
                Load(3);
                //try
                //{
                //    SQL.Connect();
                //}
                //catch
                //{
                //    Log.Out("ServerTools failed to connect to an sql database. ST requires this to operate.");
                //    Load(3);
                //}
            }
            if (_state == 3)
            {

                try
                {
                    SQL.Connect();
                }
                catch
                {
                    Log.Out("ServerTools failed to connect to an sql database. ST requires this to operate.");
                    //Load(3);
                    return;
                }

                //if (File.Exists(string.Format("{0}/ServerTools.bin", GameUtils.GetSaveGameDir())))
                //{
                //    UpdateToSqlFromBin.Exec();
                //}
                //else
                //{
                //    Load(4);
                //}
                Load(4);
            }
            if (_state == 4)
            {
                //settings now loaded in the first state (1)
                //LoadConfig.Load(); //leaving state 4 in place to avoid breaking anything

                if (File.Exists(string.Format("{0}/ServerTools.bin", GameUtils.GetSaveGameDir())))
                {
                    UpdateToSqlFromBin.Exec();
                }

                Load(5);
            }
            if (_state == 5)
            {
                Mods.Load();
                Load(6);
            }
            if (_state == 6)
            {
                Phrases.Load();
                HowToSetup.Load();
                Load(7);
            }
            if (_state == 7)
            {
                if (Fps.IsEnabled)
                {
                    Fps.SetTarget();
                }
                Load(8);
            }
            if (_state == 8)
            {
                Timers.LogAlert();
                Timers.LoadAlert();
                Load(9);
            }
            if (_state == 9)
            {
                RestartVote.Startup = true;
            }
        }
    }
}

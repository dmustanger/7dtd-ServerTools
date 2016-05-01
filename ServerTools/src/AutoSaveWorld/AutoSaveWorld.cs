using System.Collections.Generic;
using System.Threading;

namespace ServerTools
{
    public class AutoSaveWorld
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static int DelayBetweenWorldSaves = 15;
        private static Thread th;

        public static void Start()
        {
            th = new Thread(new ThreadStart(Save));
            th.IsBackground = true;
            th.Start();
            IsRunning = true;
            Log.Out("[SERVERTOOLS] AutoSaveWorld has started.");
        }

        public static void Stop()
        {
            if (!IsEnabled)
            {
                th.Abort();
                IsRunning = false;
                Log.Out("[SERVERTOOLS] AutoSaveWorld has stopped.");
            }
        }

        private static void Save()
        {
            while (IsEnabled)
            {
                int _playerCount = ConnectionManager.Instance.ClientCount();
                if (_playerCount > 0)
                {
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                    ClientInfo _cInfo = _cInfoList.RandomObject();
                    SdtdConsole.Instance.ExecuteSync("saveworld", _cInfo);
                    Log.Out("[SERVERTOOLS] World Saved.");
                }
                Thread.Sleep(60000 * DelayBetweenWorldSaves);
            }
            Stop();
        }
    }
}
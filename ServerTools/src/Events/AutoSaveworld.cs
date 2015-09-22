using System.Collections.Generic;
using System.Threading;

namespace ServerTools
{
    public class SaveWorld
    {
        public static int DelayBetweenWorldSaves = 15;
        public static bool IsEnabled = false;
        public static Thread th;
        public static bool IsRunning = false;

        public static void Init()
        {
            if (IsEnabled && !IsRunning)
            {
                IsRunning = true;
                StartSave();
            }
        }

        private static void StartSave()
        {
            th = new Thread(new ThreadStart(Save));
            th.IsBackground = true;
            th.Start();
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
        }
    }
}
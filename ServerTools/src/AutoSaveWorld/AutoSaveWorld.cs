using System.Timers;

namespace ServerTools
{
    public class AutoSaveWorld
    {
        private static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        public static int Delay_Between_World_Saves = 15;
        private static System.Timers.Timer t = new System.Timers.Timer();

        public static void StartTimer()
        {
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {
                t.Interval = Delay_Between_World_Saves * 60000;
                t.Start();
                t.Elapsed += new ElapsedEventHandler(Save);
            }
        }

        public static void StopTimer()
        {
            t.Stop();
        }

        private static void Save(object sender, ElapsedEventArgs e)
        {
            SdtdConsole.Instance.ExecuteSync("saveworld", (ClientInfo)null);
            Log.Out("[SERVERTOOLS] World Saved.");
        }
    }
}
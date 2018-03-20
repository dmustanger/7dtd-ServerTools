

namespace ServerTools
{
    public class AutoSaveWorld
    {
        public static bool IsEnabled = false;

        public static void Save()
        {
            SdtdConsole.Instance.ExecuteSync("saveworld", (ClientInfo)null);
            Log.Out("[SERVERTOOLS] World Saved.");
        }
    }
}
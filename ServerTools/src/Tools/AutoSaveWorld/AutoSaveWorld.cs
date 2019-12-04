
namespace ServerTools
{
    public class AutoSaveWorld
    {
        public static bool IsEnabled = false;
        public static int Delay = 60;

        public static void Save()
        {
            Log.Out("[SERVERTOOLS] World save has begun.");
            GameManager.Instance.SaveLocalPlayerData();
            GameManager.Instance.SaveWorld();
            Log.Out("[SERVERTOOLS] World save complete.");
        }
    }
}
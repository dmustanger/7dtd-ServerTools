using System;

namespace ServerTools
{
    public class StateManager
    {
        public static void Awake()
        {
            try
            {
                PersistentContainer.Load();
            }
            catch (Exception e)
            {
                Log.Out("Error in StateManager.Awake: " + e);
            }
        }

        public static void Shutdown()
        {
            try
            {
                PersistentContainer.Instance.Save();
            }
            catch (Exception e)
            {
                Log.Out("Error in StateManager.Shutdown: " + e);
            }
        }
    }
}
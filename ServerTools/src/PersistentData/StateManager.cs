using System;

namespace ServerTools
{
    public class StateManager
    {
        public static void Awake()
        {
            try
            {
                if (PersistentContainer.Instance.Load())
                {
                    Log.Out("[SERVERTOOLS] Player data loaded");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in StateManager.Awake: {0}", e.Message));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in StateManager.Shutdown: {0}", e.Message));
            }
        }
    }
}

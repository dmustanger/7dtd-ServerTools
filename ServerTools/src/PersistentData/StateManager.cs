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
                Log.Out(string.Format("[SERVERTOOLS] Error in StateManager.Awake: {0}", e));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in StateManager.Shutdown: {0}", e));
            }
        }
    }
}
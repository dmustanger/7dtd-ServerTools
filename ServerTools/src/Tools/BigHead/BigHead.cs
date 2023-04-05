

using System.Collections.Generic;

namespace ServerTools
{
    class BigHead
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static void Enable()
        {
            IsRunning = true;
            if (GameManager.Instance.World.Entities.Count < 0)
            {
                return;
            }
            List<Entity> entityList = GameManager.Instance.World.Entities.list;
            if (entityList == null)
            {
                return;
            }
            for (int i = 0; i < entityList.Count; i++)
            {
                if (!(entityList[i] is EntityZombie) || !entityList[i].IsAlive())
                {
                    continue;
                }
                EntityAlive entity = (EntityAlive)entityList[i];
                entity.SetBigHead();
            }
        }

        public static void Disable()
        {
            IsRunning = false;
            if (GameManager.Instance.World.Entities.Count < 0)
            {
                return;
            }
            List<Entity> entityList = GameManager.Instance.World.Entities.list;
            if (entityList == null)
            {
                return;
            }
            for (int i = 0; i < entityList.Count; i++)
            {
                if (!(entityList[i] is EntityZombie) || !entityList[i].IsAlive())
                {
                    continue;
                }
                EntityAlive entity = (EntityAlive)entityList[i];
                entity.ResetHead();
            }
        }
    }
}

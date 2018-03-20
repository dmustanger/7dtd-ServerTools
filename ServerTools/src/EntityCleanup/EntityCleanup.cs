using System.Collections.Generic;

namespace ServerTools
{
    class EntityCleanup
    {
        public static bool IsEnabled = false;
        public static int World_Max = 40, Max_Pile = 10;
        private static List<Entity> Entities = new List<Entity>();
        private static Dictionary<int, int> EntitiesCount = new Dictionary<int, int>();

        public static void EntityCheck()
        {
            World world = GameManager.Instance.World;
            Entities = GameManager.Instance.World.Entities.list;
            if (Entities.Count > World_Max)
            {
                for (int i = 0; i < Entities.Count; i++)
                {
                    int _counter = 0;
                    Entity _entity = Entities[i];
                    if (!_entity.IsClientControlled())
                    {
                        foreach (Entity ent in Entities)
                        {
                            if (ent != null && _entity != ent)
                            {
                                float _distance = _entity.GetDistance(ent);
                                if (_distance <= 2)
                                {
                                    _counter++;
                                }
                            }
                        }
                    }
                    if (_counter >= Max_Pile)
                    {
                        GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                        Log.Out(string.Format("Removed entityId {0}", _entity.entityId));
                    }                   
                }
            }
        }
    }
}

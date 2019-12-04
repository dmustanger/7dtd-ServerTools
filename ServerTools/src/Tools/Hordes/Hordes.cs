using System.Collections.Generic;

namespace ServerTools
{
    class Hordes
    {
        public static bool IsEnabled = false;

        public static void Exec()
        {
            if (!SkyManager.BloodMoon())
            {
                int _playerCount = ConnectionManager.Instance.ClientCount();
                if (_playerCount > 5)
                {
                    int _counter = 0;
                    List<Entity> Entities = GameManager.Instance.World.Entities.list;
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        Entity _entity = Entities[i];
                        if (_entity != null)
                        {
                            if (!_entity.IsClientControlled())
                            {
                                EntityType _type = _entity.entityType;
                                if (_type == EntityType.Zombie)
                                {
                                    _counter++;
                                }
                            }
                        }
                    }
                    if (_counter < 30)
                    {
                        GameManager.Instance.World.aiDirector.GetComponent<AIDirectorWanderingHordeComponent>().SpawnWanderingHorde(false);
                        Log.Out(string.Format("[SERVERTOOLS] Spawned a horde"));
                    }
                }
            }
        }
    }
}


using System.Collections.Generic;

namespace ServerTools
{
    class Fps
    {
        public static bool IsEnabled = false;
        public static int Set_Target = 60, Low_FPS = 5;
        public static string Command_fps = "fps";

        private static int Flags = 0;

        public static void Exec(ClientInfo _cInfo)
        {
            string _fps = GameManager.Instance.fps.Counter.ToString();
            Phrases.Dict.TryGetValue("Fps1", out string _phrase);
            _phrase = _phrase.Replace("{Fps}", _fps);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void SetTarget()
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("SetTargetFps {0}", Set_Target), null);
        }

        public static void LowFPS()
        {
            if (Low_FPS > 0)
            {
                int _fps = (int)GameManager.Instance.fps.Counter;
                if (_fps <= Low_FPS)
                {
                    Flags += 1;
                    if (Flags >= 4)
                    {
                        if (GameManager.Instance.World != null && GameManager.Instance.World.Entities.Count > 0 && GameManager.Instance.World.Players.Count > 0)
                        {
                            List<Entity> _entities = GameManager.Instance.World.Entities.list;
                            List<int> _valid = new List<int>();
                            if (_entities != null)
                            {
                                for (int i = 0; i < _entities.Count; i++)
                                {
                                    Entity _entity1 = _entities[i];
                                    if (_entity1 != null && _entity1 is EntityPlayer && _entity1.IsSpawned())
                                    {
                                        for (int j = 0; j < _entities.Count; j++)
                                        {
                                            Entity _entity2 = _entities[j];
                                            if (_entity2 != null && _entity2 is EntityZombie && !_valid.Contains(_entity2.entityId) && _entity2.IsSpawned() && (_entity1.position - _entity2.position).magnitude <= 200)
                                            {
                                                _valid.Add(_entity2.entityId);
                                            }
                                        }
                                    }
                                }
                                for (int i = 0; i < _entities.Count; i++)
                                {
                                    if (_entities[i] is EntityZombie && !_valid.Contains(_entities[i].entityId))
                                    {
                                        GameManager.Instance.World.RemoveEntity(_entities[i].entityId, EnumRemoveEntityReason.Despawned);
                                    }
                                }
                            }
                        }
                        Flags = 0;
                    }
                }
                else
                {
                    Flags = 0;
                }
            }
        }
    }
}

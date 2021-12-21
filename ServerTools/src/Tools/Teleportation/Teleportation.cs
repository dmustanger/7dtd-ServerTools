using System.Collections.Generic;

namespace ServerTools
{
    class Teleportation
    {
        public static List<int> Teleporting = new List<int>();

        public static bool ZCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            List<Entity> Entities = GameManager.Instance.World.Entities.list;
            for (int i = 0; i < Entities.Count; i++)
            {
                Entity _entity = Entities[i];
                if (_entity != null && _player != _entity && _entity.IsSpawned() && !_entity.IsClientControlled())
                {
                    string _tags = _entity.EntityClass.Tags.ToString();
                    if (_tags.Contains("zombie") || _tags.Contains("hostile"))
                    {
                        EntityAlive entityAlive = PersistentOperations.GetEntityPlayer(_entity.entityId) as EntityAlive;
                        if (entityAlive != null)
                        {
                            if (((_player.position.x - _entity.position.x) * (_player.position.x - _entity.position.x) + (_player.position.z - _entity.position.z) * (_player.position.z - _entity.position.z)) < 75f * 75f)
                            {
                                Phrases.Dict.TryGetValue("Teleport1", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool PCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            List<EntityPlayer> playerList = GameManager.Instance.World.Players.list;
            for (int i = 0; i < playerList.Count; i++)
            {
                EntityPlayer _player2 = playerList[i];
                if (_player2 != null && _player != _player2 && _player2.IsSpawned() && !_player.IsFriendsWith(_player2))
                {
                    if (((_player.position.x - _player2.position.x) * (_player.position.x - _player2.position.x) + (_player.position.z - _player2.position.z) * (_player.position.z - _player2.position.z)) < 125f * 125f)
                    {
                        Phrases.Dict.TryGetValue("Teleport2", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool VehicleCheck(ClientInfo _cInfo)
        {
            EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
            if (player.AttachedToEntity != null)
            {
                Phrases.Dict.TryGetValue("Teleport3", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return true;
            }
            return false;
        }
    }
}

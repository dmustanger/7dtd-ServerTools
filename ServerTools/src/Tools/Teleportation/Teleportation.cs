using System.Collections.Generic;

namespace ServerTools
{
    class Teleportation
    {

        public static bool ZCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            List<Entity> Entities = GameManager.Instance.World.Entities.list;
            for (int i = 0; i < Entities.Count; i++)
            {
                Entity entity = Entities[i];
                if (entity != null && _player != entity && entity.IsSpawned() && entity is EntityZombie)
                {
                    EntityAlive entityAlive = PersistentOperations.GetEntityPlayer(entity.entityId);
                    if (entityAlive != null)
                    {
                        if ((entityAlive.position - entity.position).magnitude <= 100)
                        {
                            Phrases.Dict.TryGetValue("Teleport1", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool PCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            List<EntityPlayer> players = PersistentOperations.ListPlayers();
            if (players.Count > 1)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    EntityPlayer player2 = players[i];
                    if (player2 != null && player2.entityId != _cInfo.entityId && !_player.IsFriendsWith(player2))
                    {
                        if ((_player.position - player2.position).magnitude <= 125)
                        {
                            Phrases.Dict.TryGetValue("Teleport2", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return true;
                        }
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

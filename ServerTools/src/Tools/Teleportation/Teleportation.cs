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
                        EntityAlive _entityAlive = GameManager.Instance.World.Entities.dict[_entity.entityId] as EntityAlive;
                        if (_entityAlive != null && _player == _entityAlive.GetAttackTarget())
                        {
                            if (((_player.position.x - _entity.position.x) * (_player.position.x - _entity.position.x) + (_player.position.z - _entity.position.z) * (_player.position.z - _entity.position.z)) < 50f * 50f)
                            {
                                Phrases.Dict.TryGetValue(851, out string _phrase851);
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase851 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
            List<EntityPlayer> _playerList = GameManager.Instance.World.Players.list;
            for (int i = 0; i < _playerList.Count; i++)
            {
                EntityPlayer _player2 = _playerList[i];
                if (_player2 != null && _player != _player2 && _player2.IsSpawned() && !_player.IsFriendsWith(_player2))
                {
                    if (((_player.position.x - _player2.position.x) * (_player.position.x - _player2.position.x) + (_player.position.z - _player2.position.z) * (_player.position.z - _player2.position.z)) < 100f * 100f)
                    {
                        Phrases.Dict.TryGetValue(852, out string _phrase852);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase852 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool VehicleCheck(ClientInfo _cInfo)
        {
            Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player.AttachedToEntity != null)
            {
                Phrases.Dict.TryGetValue(853, out string _phrase853);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase853 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                return true;
            }
            return false;
        }
    }
}

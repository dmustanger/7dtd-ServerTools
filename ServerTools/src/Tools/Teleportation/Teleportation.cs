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
                                string _phrase820;
                                if (!Phrases.Dict.TryGetValue(820, out _phrase820))
                                {
                                    _phrase820 = " you are too close to a hostile zombie or animal. Command unavailable.";
                                }
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase820 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        string _phrase819;
                        if (!Phrases.Dict.TryGetValue(819, out _phrase819))
                        {
                            _phrase819 = " you are too close to a hostile player. Command unavailable.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase819 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                string _phrase826;
                if (!Phrases.Dict.TryGetValue(826, out _phrase826))
                {
                    _phrase826 = " you can not teleport home with a vehicle.";
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase826 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                return true;
            }
            return false;
        }
    }
}

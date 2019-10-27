using System.Collections.Generic;

namespace ServerTools
{
    class Teleportation
    {

        public static bool ZCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            Entity _ent1 = GameManager.Instance.World.Players.dict[_cInfo.entityId] as Entity;
            List<Entity> Entities = GameManager.Instance.World.Entities.list;
            for (int i = 0; i < Entities.Count; i++)
            {
                Entity _ent2 = Entities[i];
                if (_ent2 != null && _ent1 != _ent2 && _ent2.IsSpawned() && !_ent2.IsClientControlled())
                {
                    string _tags = _ent2.EntityClass.Tags.ToString();
                    if (_tags.Contains("zombie") || _tags.Contains("hostile"))
                    {
                        float distanceSq = _ent2.GetDistanceSq(_ent1.position);
                        if (distanceSq <= 80f * 80f)
                        {
                            EntityAlive _entAlive1 = GameManager.Instance.World.Players.dict[_cInfo.entityId] as EntityAlive;
                            EntityAlive _entAlive2 = GameManager.Instance.World.Entities.dict[_ent2.entityId] as EntityAlive;
                            if (_entAlive1 == _entAlive2.GetAttackTarget())
                            {
                                string _phrase820;
                                if (!Phrases.Dict.TryGetValue(820, out _phrase820))
                                {
                                    _phrase820 = " you are too close to a hostile zombie or animal. Command unavailable.";
                                }
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase820 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
            Entity _ent1 = GameManager.Instance.World.Players.dict[_cInfo.entityId] as Entity;
            List<EntityPlayer> _playerList = GameManager.Instance.World.Players.list;
            for (int i = 0; i < _playerList.Count; i++)
            {
                EntityPlayer _entPlayer2 = _playerList[i];
                if (_entPlayer2 != null && _player != _entPlayer2 && !_entPlayer2.IsDead() && _entPlayer2.Spawned && !_player.IsFriendsWith(_entPlayer2))
                {
                    Entity _ent2 = GameManager.Instance.World.Players.dict[_entPlayer2.entityId] as Entity;
                    float distanceSq = _ent2.GetDistanceSq(_ent1.position);
                    if (distanceSq <= 160f * 160f)
                    {
                        string _phrase819;
                        if (!Phrases.Dict.TryGetValue(819, out _phrase819))
                        {
                            _phrase819 = " you are too close to a player that is not a friend. Command unavailable.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase819 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

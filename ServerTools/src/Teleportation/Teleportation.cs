

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Teleportation
    {

        public static bool ZCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            World world = GameManager.Instance.World;
            List<Entity> Entities = world.Entities.list;
            for (int i = 0; i < Entities.Count; i++)
            {
                Entity _entity = Entities[i];
                if (_entity != null)
                {
                    EntityType _type = _entity.entityType;
                    if (_type == EntityType.Zombie)
                    {
                        Vector3 _pos2 = _entity.GetPosition();
                        if (((int)_player.position.x - (int)_pos2.x) * ((int)_player.position.x - (int)_pos2.x) + ((int)_player.position.z - (int)_pos2.z) * ((int)_player.position.z - (int)_pos2.z) <= 40 * 40)
                        {
                            string _phrase820;
                            if (!Phrases.Dict.TryGetValue(820, out _phrase820))
                            {
                                _phrase820 = "you are too close to a zombie. Command unavailable.";
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase820 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool PCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo2 = _cInfoList[i];
                if (_cInfo2 != null)
                {
                    EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                    if (_player2 != null)
                    {
                        Vector3 _pos2 = _player2.GetPosition();
                        if (((int)_player.position.x - (int)_pos2.x) * ((int)_player.position.x - (int)_pos2.x) + ((int)_player.position.z - (int)_pos2.z) * ((int)_player.position.z - (int)_pos2.z) <= 60 * 60)
                        {
                            if (!_player.IsFriendsWith(_player2))
                            {
                                string _phrase819;
                                if (!Phrases.Dict.TryGetValue(819, out _phrase819))
                                {
                                    _phrase819 = "you are too close to a player that is not a friend. Command unavailable.";
                                }
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase819 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class WorldRadius
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Normal_Player = 8000, Donator = 10000;

        public static void Exec()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                World world = GameManager.Instance.World;
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                    if (Admin.PermissionLevel > Admin_Level)
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (ReservedSlots.IsEnabled)
                        {
                            DateTime _dt;
                            if (ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt))
                            {
                                if (DateTime.Now < _dt)
                                {
                                    DonatorRad(_player);
                                }
                                else
                                {
                                    NormalRad(_player);
                                }
                            }
                            else
                            {
                                NormalRad(_player);
                            }
                        }
                        else
                        {
                            NormalRad(_player);
                        }
                    }
                }
            }
        }

        public static void NormalRad(EntityPlayer _player)
        {
            if ((0 - _player.position.x) * (0 - _player.position.x) + (0 - _player.position.z) * (0 - _player.position.z) >= Normal_Player * Normal_Player)
            {
                int _vec3x, _vec3z;
                if (_player.position.x >= 0)
                {
                    _vec3x = (int)_player.position.x - 6;
                }
                else
                {
                    _vec3x = (int)_player.position.x + 6;
                }
                if (_player.position.z >= 0)
                {
                    _vec3z = (int)_player.position.z - 6;
                }
                else
                {
                    _vec3z = (int)_player.position.z + 6;
                }
                ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_player.entityId);
                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(_vec3x, -1, _vec3z), null, false));
                string _phrase790;
                if (!Phrases.Dict.TryGetValue(790, out _phrase790))
                {
                    _phrase790 = "you have reached the world border.";
                }
                _phrase790 = _phrase790.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase790 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
        }

        public static void DonatorRad(EntityPlayer _player)
        {
            if ((0 - _player.position.x) * (0 - _player.position.x) + (0 - _player.position.z) * (0 - _player.position.z) >= Donator * Donator)
            {
                int _vec3x, _vec3z;
                if (_player.position.x >= 0)
                {
                    _vec3x = (int)_player.position.x - 6;
                }
                else
                {
                    _vec3x = (int)_player.position.x + 6;
                }
                if (_player.position.z >= 0)
                {
                    _vec3z = (int)_player.position.z - 6;
                }
                else
                {
                    _vec3z = (int)_player.position.z + 6;
                }
                ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_player.entityId);
                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(_vec3x, -1, _vec3z), null, false));
                string _phrase790;
                if (!Phrases.Dict.TryGetValue(790, out _phrase790))
                {
                    _phrase790 = "you have reached the world border.";
                }
                _phrase790 = _phrase790.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase790 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}

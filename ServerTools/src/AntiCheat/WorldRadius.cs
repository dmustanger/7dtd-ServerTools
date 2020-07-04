using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools.AntiCheat
{
    class WorldRadius
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Normal_Player = 8000, Reserved = 10000;

        public static void Exec()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
                for (int i = 0; i < ClientInfoList.Count; i++)
                {
                    ClientInfo _cInfo = ClientInfoList[i];
                    GameManager.Instance.adminTools.GetAdmins().TryGetValue(_cInfo.playerId, out AdminToolsClientInfo Admin);
                    if (Admin.PermissionLevel > Admin_Level)
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (_player != null)
                        {
                            if (ReservedSlots.IsEnabled)
                            {
                                DateTime _dt;
                                if (ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt))
                                {
                                    if (DateTime.Now < _dt)
                                    {
                                        DonatorRad(_cInfo, _player);
                                    }
                                    else
                                    {
                                        NormalRad(_cInfo, _player);
                                    }
                                }
                                else
                                {
                                    NormalRad(_cInfo, _player);
                                }
                            }
                            else
                            {
                                NormalRad(_cInfo, _player);
                            }
                        }
                    }
                }
            }
        }

        public static void NormalRad(ClientInfo _cInfo, EntityPlayer _player)
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
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_vec3x, -1, _vec3z), null, false));
                string _phrase790;
                if (!Phrases.Dict.TryGetValue(790, out _phrase790))
                {
                    _phrase790 = "You have reached the world border.";
                }
                _phrase790 = _phrase790.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase790 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void DonatorRad(ClientInfo _cInfo, EntityPlayer _player)
        {
            if ((0 - _player.position.x) * (0 - _player.position.x) + (0 - _player.position.z) * (0 - _player.position.z) >= Reserved * Reserved)
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
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_vec3x, -1, _vec3z), null, false));
                string _phrase790;
                if (!Phrases.Dict.TryGetValue(790, out _phrase790))
                {
                    _phrase790 = "You have reached the world border.";
                }
                _phrase790 = _phrase790.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase790 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

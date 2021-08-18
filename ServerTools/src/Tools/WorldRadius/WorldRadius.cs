using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class WorldRadius
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Normal_Player = 8000, Reserved = 10000;

        public static void Exec()
        {
            if (GameManager.Instance.World.Players.dict.Count > 0)
            {
                List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
                if (ClientInfoList != null && ClientInfoList.Count > 0)
                {
                    for (int i = 0; i < ClientInfoList.Count; i++)
                    {
                        ClientInfo _cInfo = ClientInfoList[i];
                        if (_cInfo != null && _cInfo.playerId != null)
                        {
                            if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                if (_player != null)
                                {
                                    if (ReservedSlots.IsEnabled)
                                    {
                                        if (ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt))
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
                Phrases.Dict.TryGetValue("WorldRadius1", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                Phrases.Dict.TryGetValue("WorldRadius1", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

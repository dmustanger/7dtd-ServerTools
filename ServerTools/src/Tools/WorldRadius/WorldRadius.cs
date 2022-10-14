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
                List<ClientInfo> clientList = GeneralFunction.ClientList();
                if (clientList != null)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo = clientList[i];
                        if (cInfo != null)
                        {
                            if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level ||
                                GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                            {
                                EntityPlayer player = GeneralFunction.GetEntityPlayer(cInfo.entityId);
                                if (player != null)
                                {
                                    if (ReservedSlots.IsEnabled)
                                    {
                                        if (ReservedSlots.Dict.ContainsKey(cInfo.PlatformId.CombinedString))
                                        {
                                            ReservedSlots.Dict.TryGetValue(cInfo.PlatformId.CombinedString, out DateTime dt);
                                            if (DateTime.Now < dt)
                                            {
                                                DonatorRad(cInfo, player);
                                            }
                                            else
                                            {
                                                NormalRad(cInfo, player);
                                            }
                                        }
                                        else if (ReservedSlots.Dict.ContainsKey(cInfo.CrossplatformId.CombinedString))
                                        {
                                            ReservedSlots.Dict.TryGetValue(cInfo.CrossplatformId.CombinedString, out DateTime dt);
                                            if (DateTime.Now < dt)
                                            {
                                                DonatorRad(cInfo, player);
                                            }
                                            else
                                            {
                                                NormalRad(cInfo, player);
                                            }
                                        }
                                        else
                                        {
                                            NormalRad(cInfo, player);
                                        }
                                    }
                                    else
                                    {
                                        NormalRad(cInfo, player);
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
                int vec3x, vec3z;
                if (_player.position.x >= 0)
                {
                    vec3x = (int)_player.position.x - 6;
                }
                else
                {
                    vec3x = (int)_player.position.x + 6;
                }
                if (_player.position.z >= 0)
                {
                    vec3z = (int)_player.position.z - 6;
                }
                else
                {
                    vec3z = (int)_player.position.z + 6;
                }
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(vec3x, -1, vec3z), null, false));
                Phrases.Dict.TryGetValue("WorldRadius1", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void DonatorRad(ClientInfo _cInfo, EntityPlayer _player)
        {
            if ((0 - _player.position.x) * (0 - _player.position.x) + (0 - _player.position.z) * (0 - _player.position.z) >= Reserved * Reserved)
            {
                int vec3x, vec3z;
                if (_player.position.x >= 0)
                {
                    vec3x = (int)_player.position.x - 6;
                }
                else
                {
                    vec3x = (int)_player.position.x + 6;
                }
                if (_player.position.z >= 0)
                {
                    vec3z = (int)_player.position.z - 6;
                }
                else
                {
                    vec3z = (int)_player.position.z + 6;
                }
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(vec3x, -1, vec3z), null, false));
                Phrases.Dict.TryGetValue("WorldRadius1", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

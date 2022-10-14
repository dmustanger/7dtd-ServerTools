using System;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class Stuck
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60;
        public static string Command_stuck = "stuck";

        public static void Exec(ClientInfo _cInfo)
        {
            Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player.AttachedToEntity != null)
            {
                _player.AttachedToEntity.Detach();
                return;
            }
            if (Delay_Between_Uses < 1)
            {
                CheckLP(_cInfo);
            }
            else
            {
                DateTime lastStuck = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastStuck;
                TimeSpan varTime = DateTime.Now - lastStuck;
                double fractionalMinutes = varTime.TotalMinutes;
                int timepassed = (int)fractionalMinutes;
                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                {
                    if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                    {
                        if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Time(_cInfo, timepassed, delay);
                                return;
                            }
                        }
                        else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Time(_cInfo, timepassed, delay);
                                return;
                            }
                        }
                    }
                }
                Time(_cInfo, timepassed, Delay_Between_Uses);
            }
        }

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                CheckLP(_cInfo);
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue("Stuck1", out string _phrase);
                _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase = _phrase.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase = _phrase.Replace("{Command_stuck}", Command_stuck);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CheckLP(ClientInfo _cInfo)
        {
            EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
            if (player != null)
            {
                Vector3 position = player.GetPosition();
                int x = (int)position.x;
                int y = (int)position.y;
                int z = (int)position.z;
                Vector3i vector = new Vector3i(x, y, z);
                PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(player.entityId);
                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(vector, _persistentPlayerData);
                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally || _owner == EnumLandClaimOwner.None)
                {
                    if (CheckStuck(position.x, position.y, position.z))
                    {
                        TeleToSurface(_cInfo, player);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Stuck4", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Stuck2", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        private static bool CheckStuck(float x, float y, float z)
        {
            for (float k = y - 0.25f; k <= (y + 1f); k++)
            {
                for (float i = x - 0.25f; i <= (x + 0.25f); i++)
                {
                    for (float j = z - 0.25f; j <= (z + 0.25f); j++)
                    {
                        BlockValue block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        MaterialBlock _material = Block.list[block.type].blockMaterial;
                        if (block.type == BlockValue.Air.type || _material.IsLiquid || _material.IsPlant || block.Block.IsTerrainDecoration || block.Block.isMultiBlock)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static void TeleToSurface(ClientInfo _cInfo, EntityPlayer _player)
        {
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3((int)_player.position.x, -1, (int)_player.position.z), null, false));
            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastStuck = DateTime.Now;
            PersistentContainer.DataChange = true;
            Phrases.Dict.TryGetValue("Stuck3", out string _phrase);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}

using System;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class Stuck
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60;
        public static string Command90 = "stuck";

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
                DateTime _lastStuck = PersistentContainer.Instance.Players[_cInfo.playerId].LastStuck;
                TimeSpan varTime = DateTime.Now - _lastStuck;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                {
                    if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                    {
                        DateTime _dt;
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                        if (DateTime.Now < _dt)
                        {
                            int _delay = Delay_Between_Uses / 2;
                            Time(_cInfo, _timepassed, _delay);
                            return;
                        }
                    }
                }
                Time(_cInfo, _timepassed, Delay_Between_Uses);
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
                Phrases.Dict.TryGetValue(551, out string _phrase551);
                _phrase551 = _phrase551.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase551 = _phrase551.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase551 = _phrase551.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase551 = _phrase551.Replace("{Command90}", Command90);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase551 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CheckLP(ClientInfo _cInfo)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = world.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.GetPosition();
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            Vector3i _vec3i = new Vector3i(x, y, z);
            PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
            PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(_player.entityId);
            EnumLandClaimOwner _owner = world.GetLandClaimOwner(_vec3i, _persistentPlayerData);
            if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally || _owner == EnumLandClaimOwner.None)
            {
                if (CheckStuck(_position.x, _position.y, _position.z))
                {
                    TeleToSurface(_cInfo, _player);
                }
                else
                {
                    Phrases.Dict.TryGetValue(554, out string _phrase554);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase554 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(552, out string _phrase552);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase552 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
            PersistentContainer.Instance.Players[_cInfo.playerId].LastStuck = DateTime.Now;
            Phrases.Dict.TryGetValue(553, out string _phrase553);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase553 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}

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
                string _phrase920;
                if (!Phrases.Dict.TryGetValue(920, out _phrase920))
                {
                    _phrase920 = " you can only use {CommandPrivate}{Command90} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase920 = _phrase920.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase920 = _phrase920.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase920 = _phrase920.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase920 = _phrase920.Replace("{Command90}", Command90);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase920 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    string _phrase923;
                    if (!Phrases.Dict.TryGetValue(923, out _phrase923))
                    {
                        _phrase923 = " you do not seem to be stuck.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase923 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                string _phrase921;
                if (!Phrases.Dict.TryGetValue(921, out _phrase921))
                {
                    _phrase921 = " you are outside of your claimed space or a friends. Command is unavailable.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase921 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
            string _phrase922;
            if (!Phrases.Dict.TryGetValue(922, out _phrase922))
            {
                _phrase922 = " sending you to the world surface. If you are still stuck, contact an administrator.";
            }
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase922 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            PersistentContainer.Instance.Players[_cInfo.playerId].LastStuck = DateTime.Now;
            PersistentContainer.Instance.Save();
        }
    }
}

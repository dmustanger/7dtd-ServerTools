using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Teleportation
    {

        public static bool ZCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            List<Entity> Entities = GameManager.Instance.World.Entities.list;
            for (int i = 0; i < Entities.Count; i++)
            {
                Entity entity = Entities[i];
                if (entity != null && _player != entity && entity.IsSpawned() && entity is EntityZombie)
                {
                    EntityAlive entityAlive = GeneralFunction.GetEntityPlayer(entity.entityId);
                    if (entityAlive != null)
                    {
                        if ((entityAlive.position - entity.position).magnitude <= 100)
                        {
                            Phrases.Dict.TryGetValue("Teleport1", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool PCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            List<EntityPlayer> players = GeneralFunction.ListPlayers();
            if (players.Count > 1)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    EntityPlayer player2 = players[i];
                    if (player2 != null && player2.entityId != _cInfo.entityId && !_player.IsFriendsWith(player2))
                    {
                        if ((_player.position - player2.position).magnitude <= 125)
                        {
                            Phrases.Dict.TryGetValue("Teleport2", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool VehicleCheck(ClientInfo _cInfo)
        {
            EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
            if (player.AttachedToEntity != null)
            {
                Phrases.Dict.TryGetValue("Teleport3", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return true;
            }
            return false;
        }

        public static void InsideWorld(ClientInfo _cInfo, EntityPlayer _player)
        {
            float x, z, positiveX, positiveY, negativeX, negativeY;
            string gameWorld = GamePrefs.GetString(EnumGamePrefs.GameWorld);
            if (gameWorld.ToLower() == "navezgane")
            {
                positiveX = 2500;
                positiveY = 2500;
                negativeX = -2500;
                negativeY = -2500;
            }
            else
            {
                IChunkProvider chunkProvider = GameManager.Instance.World.ChunkCache.ChunkProvider;
                positiveX = chunkProvider.GetWorldSize().x;
                positiveY = chunkProvider.GetWorldSize().y;
                negativeX = chunkProvider.GetWorldSize().x * -1;
                negativeY = chunkProvider.GetWorldSize().y * -1;
            }
            x = _player.position.x;
            z = _player.position.z;
            bool outside = false;
            if (x >= positiveX)
            {
                outside = true;
                x = positiveX - 10;
            }
            else if (x <= negativeX)
            {
                outside = true;
                x = negativeX + 10;
            }
            if (z >= positiveY)
            {
                outside = true;
                z = positiveY - 10;
            }
            else if (z <= negativeY)
            {
                outside = true;
                z = negativeY + 10;
            }
            if (outside)
            {
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3((int)x, -1, (int)z), null, false));
            }
            return;
        }

        public static void InsideBlock(ClientInfo _cInfo, EntityPlayer _player)
        {
            BlockValue blockValue = GameManager.Instance.World.GetBlock(new Vector3i(_player.position));
            if (!blockValue.isair && !blockValue.isWater && _player.isCollided)
            {
                Timers.InsideBlockTimer(_cInfo, _player.position);
            }
        }

        public static void StillInsideBlock(ClientInfo _cInfo, Vector3 _position)
        {
            EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
            if (player != null && player.position == _position)
            {
                BlockValue blockValue = GameManager.Instance.World.GetBlock(new Vector3i(_position));
                if (!blockValue.isair && !blockValue.isWater && player.isCollided)
                {
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(player.position.x, -1, player.position.z), null, false));
                }
            }
        }
    }
}

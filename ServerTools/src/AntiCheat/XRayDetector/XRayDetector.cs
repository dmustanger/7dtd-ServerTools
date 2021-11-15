using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class XRayDetector
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0;

        public static List<int> BlackScreen = new List<int>();

        private static int AirType = BlockValue.Air.type;

        public static void IsInsideBlocks(ClientInfo _cInfo, EntityPlayer _player)
        {
            Vector3 cameraPosition;
            if (!_player.IsCrouching)
            {
                cameraPosition = new Vector3(_player.position.x, _player.position.y + 1.6f, _player.position.z);
            }
            else
            {
                cameraPosition = new Vector3(_player.position.x, _player.position.y + 1f, _player.position.z);
            }
            World world = GameManager.Instance.World;
            Vector3i blockPosition = World.worldToBlockPos(cameraPosition);
            BlockValue blockValue = world.GetBlock(blockPosition);
            if (blockValue.type != AirType)
            {
                Block block = blockValue.Block;
                if (block.IsCollideMovement)
                {
                    if (block.shape.IsTerrain())
                    {
                        if (blockValue.damage < block.MaxDamage * 0.75)
                        {
                            if (!_player.IsCrouching)
                            {
                                blockPosition.y--;
                                if (world.GetBlock(blockPosition).Block.IsCollideMovement)
                                {
                                    Detected(_cInfo, _player);
                                    return;
                                }
                            }
                            else
                            {
                                Detected(_cInfo, _player);
                                return;
                            }
                        }
                    }
                    else if (block is BlockDrawBridge)
                    {
                        Vector3i parentPos = block.multiBlockPos.GetParentPos(blockPosition, blockValue);
                        if (BlockDoor.IsDoorOpen(world.GetBlock(parentPos).meta) && blockPosition.y == parentPos.y)
                        {
                            Detected(_cInfo, _player);
                            return;
                        }
                    }
                    else if (block.shape.GetName().ToLower().Contains("cube"))
                    {
                        if (!_player.IsCrouching)
                        {
                            blockPosition.y--;
                            if (world.GetBlock(blockPosition).Block.IsCollideMovement)
                            {
                                Detected(_cInfo, _player);
                                return;
                            }
                        }
                        else
                        {
                            Detected(_cInfo, _player);
                            return;
                        }
                    }
                }
            }
            if (BlackScreen.Contains(_cInfo.entityId))
            {
                BlackScreen.Remove(_cInfo.entityId);
                _cInfo.SendPackage(new NetPackageConsoleCmdClient().Setup("ScreenEffect FadeToBlack 0 1", true));
            }
        }

        private static void Detected(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (!BlackScreen.Contains(_cInfo.entityId))
            {
                BlackScreen.Add(_cInfo.entityId);
                _cInfo.SendPackage(new NetPackageConsoleCmdClient().Setup("ScreenEffect FadeToBlack 1 0", true));
                Phrases.Dict.TryGetValue("AntoCheat4", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

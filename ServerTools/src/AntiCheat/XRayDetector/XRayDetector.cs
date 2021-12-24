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
                                    Detected(_cInfo);
                                    return;
                                }
                            }
                            else
                            {
                                Detected(_cInfo);
                                return;
                            }
                        }
                    }
                    else if (block is BlockDrawBridge)
                    {
                        Vector3i parentPos = block.multiBlockPos.GetParentPos(blockPosition, blockValue);
                        if (BlockDoor.IsDoorOpen(world.GetBlock(parentPos).meta) && blockPosition.y == parentPos.y)
                        {
                            Detected(_cInfo);
                            return;
                        }
                    }
                    else
                    {
                        string shape = block.shape.GetName().ToLower();
                        Log.Out(string.Format("[SERVERTOOLS] Shape: {0}", shape));
                        if (shape == "cube" || shape == "cube_frame" || shape == "cube_half" || shape == "cube_0.25m")
                        {
                            if (!_player.IsCrouching)
                            {
                                blockPosition.y--;
                                if (world.GetBlock(blockPosition).Block.IsCollideMovement)
                                {
                                    Detected(_cInfo);
                                    return;
                                }
                            }
                            else
                            {
                                Detected(_cInfo);
                                return;
                            }
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

        private static void Detected(ClientInfo _cInfo)
        {
            if (!BlackScreen.Contains(_cInfo.entityId))
            {
                BlackScreen.Add(_cInfo.entityId);
                _cInfo.SendPackage(new NetPackageConsoleCmdClient().Setup("ScreenEffect FadeToBlack 1 0", true));
                Phrases.Dict.TryGetValue("AntiCheat4", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

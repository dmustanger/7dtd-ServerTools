using HarmonyLib;
using System.Collections.Generic;

namespace ServerTools
{
    class Wall
    {
        public static bool IsEnabled = false, Player_Check = false, Reserved = false;
        public static string Command_wall = "wall";

        public static List<int> WallEnabled = new List<int>();

        private static AccessTools.FieldRef<GameManager, Dictionary<TileEntity, int>> lockedTileEntities = AccessTools.FieldRefAccess<GameManager, Dictionary<TileEntity, int>>("lockedTileEntities");

        public static void Exec(ClientInfo _cInfo)
        {
            if (Reserved)
            {
                if (!ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) && !ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    Phrases.Dict.TryGetValue("Wall4", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                if (!WallEnabled.Contains(_cInfo.entityId))
                {
                    WallEnabled.Add(_cInfo.entityId);
                    Phrases.Dict.TryGetValue("Wall1", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    WallEnabled.Remove(_cInfo.entityId);
                    Phrases.Dict.TryGetValue("Wall2", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else if (!WallEnabled.Contains(_cInfo.entityId))
            {
                WallEnabled.Add(_cInfo.entityId);
                Phrases.Dict.TryGetValue("Wall1", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                WallEnabled.Remove(_cInfo.entityId);
                Phrases.Dict.TryGetValue("Wall2", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void BuildWall(ClientInfo _cInfo, Vector3i _blockPosition, BlockValue _value)
        {
            if (GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, _blockPosition) != EnumLandClaimOwner.Self)
            {
                Phrases.Dict.TryGetValue("Wall3", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            List<Vector3i> potentialPositions = new List<Vector3i>();
            List<Vector3i> wallPositions = new List<Vector3i>();
            int startPosition = _blockPosition.z + 1;
            int endPosition = _blockPosition.z + 10;
            for (int i = startPosition; i <= endPosition; i++)
            {
                if (GameManager.Instance.World.GetBlock(new Vector3i(_blockPosition.x, _blockPosition.y, i)).Equals(_value))
                {
                    if (GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, new Vector3i(_blockPosition.x, _blockPosition.y, i)) == EnumLandClaimOwner.Self)
                    {
                        wallPositions.AddRange(potentialPositions);
                    }
                    break;
                }
                potentialPositions.Add(new Vector3i(_blockPosition.x, _blockPosition.y, i));
            }
            potentialPositions.Clear();
            startPosition = _blockPosition.z - 1;
            endPosition = _blockPosition.z - 10;
            for (int i = startPosition; i >= endPosition; i--)
            {
                if (GameManager.Instance.World.GetBlock(new Vector3i(_blockPosition.x, _blockPosition.y, i)).Equals(_value))
                {
                    if (GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, new Vector3i(_blockPosition.x, _blockPosition.y, i)) == EnumLandClaimOwner.Self)
                    {
                        wallPositions.AddRange(potentialPositions);
                    }
                    break;
                }
                potentialPositions.Add(new Vector3i(_blockPosition.x, _blockPosition.y, i));
            }
            potentialPositions.Clear();
            startPosition = _blockPosition.x + 1;
            endPosition = _blockPosition.x + 10;
            for (int i = startPosition; i <= endPosition; i++)
            {
                if (GameManager.Instance.World.GetBlock(new Vector3i(i, _blockPosition.y, _blockPosition.z)).Equals(_value))
                {
                    if (GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, new Vector3i(i, _blockPosition.y, _blockPosition.z)) == EnumLandClaimOwner.Self)
                    {
                        wallPositions.AddRange(potentialPositions);
                    }
                    break;
                }
                potentialPositions.Add(new Vector3i(i, _blockPosition.y, _blockPosition.z));
            }
            potentialPositions.Clear();
            startPosition = _blockPosition.x - 1;
            endPosition = _blockPosition.x - 10;
            for (int i = startPosition; i >= endPosition; i--)
            {
                if (GameManager.Instance.World.GetBlock(new Vector3i(i, _blockPosition.y, _blockPosition.z)).Equals(_value))
                {
                    if (GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, new Vector3i(i, _blockPosition.y, _blockPosition.z)) == EnumLandClaimOwner.Self)
                    {
                        wallPositions.AddRange(potentialPositions);
                    }
                    break;
                }
                potentialPositions.Add(new Vector3i(i, _blockPosition.y, _blockPosition.z));
            }
            int blockCount = wallPositions.Count;
            if (blockCount < 0)
            {
                return;
            }
            List<Chunk> chunks = new List<Chunk>();
            for (int i = 0; i < blockCount; i++)
            {
                Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(wallPositions[i].x, wallPositions[i].y, wallPositions[i].z);
                if (chunk != null && !chunks.Contains(chunk))
                {
                    chunks.Add(chunk);
                }
            }
            int availableBlocks = 0;
            Dictionary<TileEntity, int> lockedTiles = lockedTileEntities(GameManager.Instance);
            DictionaryList<Vector3i, TileEntity> tiles;
            for (int i = 0; i < chunks.Count; i++)
            {
                if (blockCount == 0)
                {
                    break;
                }
                tiles = chunks[i].GetTileEntities();
                foreach (TileEntity tile in tiles.dict.Values)
                {
                    if (blockCount == 0)
                    {
                        break;
                    }
                    if (tile.GetType().ToString() != "TileEntityLootContainer")
                    {
                        continue;
                    }
                    TileEntityLootContainer lootContainer = (TileEntityLootContainer)tile;
                    if (lootContainer == null || lockedTiles.ContainsKey(lootContainer))
                    {
                        continue;
                    }
                    ItemStack[] items = lootContainer.items;
                    for (int j = 0; j < items.Length; j++)
                    {
                        if (blockCount == 0)
                        {
                            break;
                        }
                        ItemStack item = items[j];
                        if (item == null || item.IsEmpty())
                        {
                            continue;
                        }
                        if (item.itemValue.Equals(_value))
                        {
                            if (items[j].count > blockCount)
                            {
                                availableBlocks += blockCount;
                                items[j].count -= blockCount;
                            }
                            else
                            {
                                availableBlocks += items[j].count;
                                items[j] = ItemStack.Empty.Clone();
                            }
                            lootContainer.SetModified();
                            continue;
                        }
                    }
                }
            }
            if (availableBlocks > 0)
            {
                List<BlockChangeInfo> blockList = new List<BlockChangeInfo>();
                for (int i = 0; i < availableBlocks; i++)
                {
                    blockList.Add(new BlockChangeInfo(0, wallPositions[i], _value));
                }
                GameManager.Instance.SetBlocksRPC(blockList, null);
                Phrases.Dict.TryGetValue("Wall5", out string phrase);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(phrase, string.Empty));
            }
        }
    }
}

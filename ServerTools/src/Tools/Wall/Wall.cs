using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Wall
    {
        public static bool IsEnabled = false, Player_Check = false;
        public static string Command_wall = "wall";

        public static Dictionary<int, Vector3i> WallEnabled = new Dictionary<int, Vector3i>();

        public static void Exec(ClientInfo _cInfo)
        {
            if (!WallEnabled.ContainsKey(_cInfo.entityId))
            {
                WallEnabled.Add(_cInfo.entityId, new Vector3i(Vector3i.zero));
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

        public static void BuildWall(ClientInfo _cInfo, EntityPlayer _player, Vector3i _pos1, Vector3i _pos2, BlockValue _value)
        {
            if ((_pos1.ToVector3() - _pos2.ToVector3()).magnitude > 1)
            {
                if (_pos1.x == _pos2.x || _pos1.z == _pos2.z)
                {
                    List<Vector3i> blockPositions = new List<Vector3i>();
                    int alt;
                    if (_pos1.x > _pos2.x)
                    {
                        alt = _pos2.x;
                        _pos2.x = _pos1.x;
                        _pos1.x = alt;
                    }
                    if (_pos1.y > _pos2.y)
                    {
                        alt = _pos2.y;
                        _pos2.y = _pos1.y;
                        _pos1.y = alt;
                    }
                    if (_pos1.z > _pos2.z)
                    {
                        alt = _pos2.z;
                        _pos2.z = _pos1.z;
                        _pos1.z = alt;
                    }
                    for (int i = _pos1.x; i <= _pos2.x; i++)
                    {
                        for (int j = _pos1.y; j <= _pos2.y; j++)
                        {
                            for (int k = _pos1.z; k <= _pos2.z; k++)
                            {
                                if (GameManager.Instance.World.GetBlock(new Vector3i(i, j, k)).Equals(BlockValue.Air))
                                {
                                    blockPositions.Add(new Vector3i(i, j, k));
                                }
                            }
                        }
                    }
                    if (blockPositions.Count > 0)
                    {
                        string blockName = _value.Block.GetBlockName().Replace(":cube", "");
                        Vector3 position = _player.position;
                        List<Chunk> chunks = new List<Chunk>();
                        List<BlockChangeInfo> blockList = new List<BlockChangeInfo>();
                        DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
                        Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x, (int)position.y, (int)position.z);
                        if (chunk != null && !chunks.Contains(chunk))
                        {
                            chunks.Add(chunk);
                        }
                        chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x - 15, (int)position.y, (int)position.z);
                        if (chunk != null && !chunks.Contains(chunk))
                        {
                            chunks.Add(chunk);
                        }
                        chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x - 30, (int)position.y, (int)position.z);
                        if (chunk != null && !chunks.Contains(chunk))
                        {
                            chunks.Add(chunk);
                        }
                        chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x + 15, (int)position.y, (int)position.z);
                        if (chunk != null && !chunks.Contains(chunk))
                        {
                            chunks.Add(chunk);
                        }
                        chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x + 30, (int)position.y, (int)position.z);
                        if (chunk != null && !chunks.Contains(chunk))
                        {
                            chunks.Add(chunk);
                        }
                        chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x, (int)position.y, (int)position.z - 15);
                        if (chunk != null && !chunks.Contains(chunk))
                        {
                            chunks.Add(chunk);
                        }
                        chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x, (int)position.y, (int)position.z - 30);
                        if (chunk != null && !chunks.Contains(chunk))
                        {
                            chunks.Add(chunk);
                        }
                        chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x, (int)position.y, (int)position.z + 15);
                        if (chunk != null && !chunks.Contains(chunk))
                        {
                            chunks.Add(chunk);
                        }
                        chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x, (int)position.y, (int)position.z + 30);
                        if (chunk != null && !chunks.Contains(chunk))
                        {
                            chunks.Add(chunk);
                        }
                        for (int i = 0; i < chunks.Count; i++)
                        {
                            tiles = chunks[i].GetTileEntities();
                            foreach (TileEntity tile in tiles.dict.Values)
                            {
                                bool modified = false;
                                if (blockPositions.Count > 0 && tile is TileEntitySecureLootContainer)
                                {
                                    TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tile;
                                    if ((SecureLoot.IsUserAllowed(_cInfo.PlatformId) || SecureLoot.IsUserAllowed(_cInfo.CrossplatformId)) && !SecureLoot.IsUserAccessing())
                                    {
                                        ItemStack[] items = SecureLoot.items;
                                        for (int j = 0; j < items.Length; j++)
                                        {
                                            ItemStack item = items[j];
                                            if (blockPositions.Count > 0 && item != null && !item.IsEmpty())
                                            {
                                                if (item.itemValue.ItemClass.GetItemName().Contains(blockName))
                                                {
                                                    int count = item.count;
                                                    for (int k = 0; k < count; k++)
                                                    {
                                                        if (blockPositions.Count > 0)
                                                        {
                                                            blockList.Add(new BlockChangeInfo(0, blockPositions[0], _value));
                                                            blockPositions.RemoveAt(0);
                                                            if (items[j].count > 1)
                                                            {
                                                                items[j].count -= 1;
                                                            }
                                                            else
                                                            {
                                                                items[j] = ItemStack.Empty.Clone();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    modified = true;
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (modified)
                                {
                                    tile.SetModified();
                                }
                            }
                        }
                        if (blockList.Count > 0)
                        {
                            GameManager.Instance.SetBlocksRPC(blockList, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Wall3", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            WallEnabled[_cInfo.entityId] = new Vector3i(Vector3i.zero);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ServerTools
{
    class ExitCommand
    {
        public static bool IsEnabled = false, Drop = false, Remove = false, All = false, Belt = false, Bag = false, Equipment = false;
        public static string Command_exit = "exit", Command_quit = "quit";
        public static int Admin_Level = 0, Exit_Time = 15;
        public static Dictionary<int, Vector3> Players = new Dictionary<int, Vector3>();

        public static void ExitWithoutCommand(ClientInfo _cInfo, string _ip)
        {
            try
            {
                if (OutputLog.ActiveLog.Count > 0)
                {
                    List<string> _log = OutputLog.ActiveLog;
                    _log.Reverse();
                    for (int i = 0; i < 50; i++)
                    {
                        if (!string.IsNullOrEmpty(_log[i]) && _log[i].Contains(_ip) && _log[i].Contains("NET: LiteNetLib: Client disconnect from:"))
                        {
                            if (_log[i].ToLower().Contains("timeout"))
                            {
                                Players.Remove(_cInfo.entityId);
                                return;
                            }
                            else
                            {
                                Players.Remove(_cInfo.entityId);
                                Penalty(_cInfo);
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Players.Remove(_cInfo.entityId);
                Log.Out(string.Format("[SERVERTOOLS] Error in ExitCommand.ExitWithoutCommand: {0}", e.Message));
            }
        }

        private static void Penalty(ClientInfo _cInfo)
        {
            try
            {
                PlayerDataFile pdf = GeneralFunction.GetPlayerDataFileFromUId(_cInfo.CrossplatformId);
                if (pdf != null)
                {
                    Vector3i pos = new Vector3i((int)pdf.ecd.pos.x, (int)pdf.ecd.pos.y, (int)pdf.ecd.pos.z);
                    if (GameManager.Instance.World.IsChunkAreaLoaded(pos.x, pos.y, pos.z))
                    {
                        Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(pos.x, pos.y, pos.z);
                        if (chunk != null)
                        {
                            BlockValue blockValue = Block.GetBlockValue("cntStorageChest");
                            if (blockValue.Block != null)
                            {
                                GameManager.Instance.World.SetBlockRPC(chunk.ClrIdx, pos, blockValue);
                                if (GameManager.Instance.World.GetTileEntity(chunk.ClrIdx, pos) is TileEntityLootContainer tileEntityLootContainer)
                                {
                                    if (All || Bag)
                                    {
                                        for (int i = 0; i < pdf.bag.Length; i++)
                                        {
                                            if (!pdf.bag[i].IsEmpty())
                                            {
                                                tileEntityLootContainer.AddItem(pdf.bag[i]);
                                                pdf.bag[i] = ItemStack.Empty.Clone();
                                            }
                                        }
                                    }
                                    if (All || Belt)
                                    {
                                        for (int i = 0; i < pdf.inventory.Length; i++)
                                        {
                                            if (!pdf.inventory[i].IsEmpty())
                                            {
                                                tileEntityLootContainer.AddItem(pdf.inventory[i]);
                                                pdf.inventory[i] = ItemStack.Empty.Clone();
                                            }
                                        }
                                    }
                                    if (All || Equipment)
                                    {
                                        ItemValue[] _equipmentValues = pdf.equipment.GetItems();
                                        for (int i = 0; i < _equipmentValues.Length; i++)
                                        {
                                            if (!_equipmentValues[i].IsEmpty())
                                            {
                                                ItemStack _itemStack = new ItemStack(_equipmentValues[i], 1);
                                                tileEntityLootContainer.AddItem(_itemStack);
                                                _equipmentValues[i].Clear();
                                            }
                                        }
                                    }
                                    if (tileEntityLootContainer.IsEmpty())
                                    {
                                        GameManager.Instance.World.SetBlockRPC(chunk.ClrIdx, pos, BlockValue.Air);
                                    }
                                    else
                                    {
                                        tileEntityLootContainer.SetModified();
                                    }
                                    GeneralFunction.SavePlayerDataFile(_cInfo.CrossplatformId.ToString(), pdf);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ExitCommand.Penalty: {0}", e.Message));
            }
        }

        public static void ExitWithCommand(int _id)
        {
            try
            {
                ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(_id);
                if (cInfo != null)
                {
                    if (GameManager.Instance.World.Players.dict.ContainsKey(cInfo.entityId))
                    {
                        EntityPlayer player = GameManager.Instance.World.Players.dict[cInfo.entityId];
                        if (player != null)
                        {
                            if (Players.TryGetValue(cInfo.entityId, out Vector3 pos))
                            {
                                if (player.position != pos)
                                {
                                    Phrases.Dict.TryGetValue("ExitCommand1", out string phrase);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                                else
                                {
                                    PlayerDataFile playerDataFile = GeneralFunction.GetPlayerDataFileFromUId(cInfo.CrossplatformId);
                                    if (playerDataFile != null)
                                    {
                                        GeneralFunction.SavePlayerDataFile(cInfo.CrossplatformId.ToString(), playerDataFile);
                                    }
                                    Players.Remove(cInfo.entityId);
                                    Disconnect(cInfo);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ExitCommand.ExitWithCommand: {0}", e.Message));
            }
        }

        public static void Disconnect(ClientInfo _cInfo)
        {
            try
            {
                Phrases.Dict.TryGetValue("ExitCommand3", out string phrase);
                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.CrossplatformId.ToString(), phrase), null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ExitCommand.Disconnect: {0}", e.Message));
            }
        }

        public static void AlertPlayer(ClientInfo _cInfo)
        {
            try
            {
                Phrases.Dict.TryGetValue("ExitCommand2", out string phrase);
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{Command_exit}", Command_exit);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ExitCommand.AlertPlayer: {0}", e.Message));
            }
        }
    }
}

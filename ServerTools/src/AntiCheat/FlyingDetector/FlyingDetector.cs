﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class FlyingDetector
    {
        public static bool IsEnabled = false, AboveGround = true, BelowGround = true;
        public static int Flying_Admin_Level = 0, Total_Flags = 3;

        public static Dictionary<int, int> Flags = new Dictionary<int, int>();

        private static string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static bool IsFlying(Vector3 _postion)
        {
            AboveGround = true;
            BelowGround = true;
            for (float i = _postion.x - 1; i <= (_postion.x + 1); i++)
            {
                for (float j = _postion.y - 2; j <= (_postion.y + 2); j++)
                {
                    for (float k = _postion.z - 1; k <= (_postion.z + 1); k++)
                    {
                        BlockValue blockValue = GameManager.Instance.World.GetBlock(new Vector3i(i, j, k));
                        Block block = blockValue.Block;
                        if (AboveGround)
                        {
                            if (blockValue.type != BlockValue.Air.type || block.isMultiBlock || block.IsCollideMovement)
                            {
                                AboveGround = false;
                            }
                        }
                        else if (BelowGround)
                        {
                            if (blockValue.type == BlockValue.Air.type || block.isMultiBlock || !block.IsCollideMovement ||
                            block is BlockDoor || block is BlockDoorSecure)
                            {
                                BelowGround = false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            if (!AboveGround || !BelowGround)
            {
                return false;
            }
            return true;
        }

        public static void Detected(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (Flags.ContainsKey(_cInfo.entityId))
            {
                Flags.TryGetValue(_cInfo.entityId, out int flags);
                flags++;
                Flags[_cInfo.entityId] = flags;
                if (flags == Total_Flags)
                {
                    List<EntityPlayer> playerList = PersistentOperations.PlayerList();
                    for (int j = 0; j < playerList.Count; j++)
                    {
                        if (playerList[j].entityId != _player.entityId && (int)playerList[j].position.x == (int)_player.position.x && (int)playerList[j].position.z == (int)_player.position.z)
                        {
                            Flags.Remove(_cInfo.entityId);
                            return;
                        }
                    }
                    Timers.Flying_SingleUseTimer(_cInfo, new Vector3i(_player.position));
                }
            }
            else
            {
                Flags.Add(_cInfo.entityId, 1);
            }
        }

        public static void TimerExpired(ClientInfo _cInfo, Vector3i _position)
        {
            if (Flags.ContainsKey(_cInfo.entityId))
            {
                Flags.TryGetValue(_cInfo.entityId, out int flags);
                if (flags >= Total_Flags)
                {
                    Flags.Remove(_cInfo.entityId);
                    Phrases.Dict.TryGetValue("Flying2", out string phrase);
                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, phrase), null);
                    using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(string.Format("Detected '{0}' with steam id '{1}' flying @ {2}", _cInfo.playerName, _cInfo.playerId, _position));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                    Log.Warning("[SERVERTOOLS] Detected '{0}' with steam Id '{1}' flying @ {2}. Steam id has been banned", _cInfo.playerName, _cInfo.playerId, _position);
                    Phrases.Dict.TryGetValue("Flying1", out phrase);
                    phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
        }
    }
}
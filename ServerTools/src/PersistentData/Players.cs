﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

namespace ServerTools
{
    [Serializable]
    public class Players
    {
        public Dictionary<string, Player> players = new Dictionary<string, Player>();
        public Dictionary<string, string> clans = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static List<int> ZonePvE = new List<int>();
        public static List<int> NoFlight = new List<int>();


        public List<string> SteamIDs
        {
            get
            {
                return new List<string>(players.Keys);
            }
        }

        public Player this[string steamId, bool create]
        {
            get
            {
                if (string.IsNullOrEmpty(steamId))
                {
                    return null;
                }
                else if (players.ContainsKey(steamId))
                {
                    return players[steamId];
                }
                else
                {
                    if (create && steamId != null && steamId.Length == 17)
                    {
                        Player p = new Player(steamId);
                        players.Add(steamId, p);
                        return p;
                    }
                    return null;
                }
            }
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            Session.Add(_cInfo.playerId, DateTime.Now);
        }

        public static void Exec()
        {
            List<EntityPlayer> _playerList = GameManager.Instance.World.Players.list;
            for (int i = 0; i < _playerList.Count; i++)
            {
                EntityPlayer _player = _playerList[i];
                if (_player != null)
                {
                    ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_player.entityId);
                    if (_cInfo != null)
                    {
                        if (!_player.IsDead())
                        {
                            if (Zones.IsEnabled)
                            {
                                ZoneCheck(_cInfo, _player);
                            }
                        }
                    }
                }
            }
        }

        public static void ZoneCheck(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (Zones.Box1.Count > 0)
            {
                int _flagCount = 0;
                int _X = (int)_player.position.x;
                int _Y = (int)_player.position.y;
                int _Z = (int)_player.position.z;
                for (int i = 0; i < Zones.Box1.Count; i++)
                {
                    string[] _box = Zones.Box1[i];
                    bool[] _box2 = Zones.Box2[i];
                    if (Zones.BoxCheck(_box, _X, _Y, _Z, _box2))
                    {
                        if (!Zones.ZoneExit.ContainsKey(_player.entityId))
                        {
                            for (int j = 0; j < Zones.Box1.Count; j++)
                            {
                                string[] _box3 = Zones.Box1[j];
                                bool[] _box4 = Zones.Box2[j];
                                if (Zones.BoxCheck(_box3, _X, _Y, _Z, _box4))
                                {
                                    Zones.ZoneExit.Add(_player.entityId, _box3[3]);
                                    if (_box4[1])
                                    {
                                        ZonePvE.Add(_player.entityId);
                                    }
                                    if (Zones.Zone_Message)
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _box3[2] + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    if (_box3[4] != "")
                                    {
                                        Zones.Response(_cInfo, _box3[4]);
                                    }
                                    Zones.reminder.Add(_player.entityId, DateTime.Now);
                                    Zones.reminderMsg.Add(_player.entityId, _box3[5]);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            string _exitMsg;
                            if (Zones.ZoneExit.TryGetValue(_player.entityId, out _exitMsg))
                            {
                                if (_exitMsg != _box[3])
                                {
                                    for (int j = 0; j < Zones.Box1.Count; j++)
                                    {
                                        string[] _box3 = Zones.Box1[j];
                                        bool[] _box4 = Zones.Box2[j];
                                        if (Zones.BoxCheck(_box3, _X, _Y, _Z, _box4))
                                        {
                                            Zones.ZoneExit[_player.entityId] = _box3[3];
                                            if (_box4[1])
                                            {
                                                if (!ZonePvE.Contains(_player.entityId))
                                                {
                                                    ZonePvE.Add(_player.entityId);
                                                }
                                            }
                                            else
                                            {
                                                if (ZonePvE.Contains(_player.entityId))
                                                {
                                                    ZonePvE.Remove(_player.entityId);
                                                }
                                            }
                                            if (Zones.Zone_Message)
                                            {
                                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _box3[2] + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                            if (_box3[4] != "")
                                            {
                                                Zones.Response(_cInfo, _box3[4]);
                                            }
                                            Zones.reminder[_player.entityId] = DateTime.Now;
                                            Zones.reminderMsg[_player.entityId] = _box3[5];
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        _flagCount++;
                        if (_flagCount == Zones.Box1.Count && Zones.ZoneExit.ContainsKey(_player.entityId))
                        {
                            if (Zones.Zone_Message)
                            {
                                string _msg;
                                if (Zones.ZoneExit.TryGetValue(_player.entityId, out _msg))
                                {
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _msg + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            Zones.ZoneExit.Remove(_player.entityId);
                            if (ZonePvE.Contains(_player.entityId))
                            {
                                ZonePvE.Remove(_player.entityId);
                            }
                            Zones.reminder.Remove(_player.entityId);
                            Zones.reminderMsg.Remove(_player.entityId);
                        }
                    }
                }
            }
        }
    }
}
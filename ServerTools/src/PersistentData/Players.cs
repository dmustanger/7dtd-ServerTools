using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    [Serializable]
    public class Players
    {
        public static bool Kill_Notice = false;
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static Dictionary<int, DateTime> DeathTime = new Dictionary<int, DateTime>();
        public static Dictionary<int, Vector3> LastDeathPos = new Dictionary<int, Vector3>();
        public static List<int> Dead = new List<int>();
        public Dictionary<string, Player> players = new Dictionary<string, Player>();
        public Dictionary<string, string> clans = new Dictionary<string, string>();

        public List<string> ClanList
        {
            get
            {
                return new List<string>(clans.Keys);
            }
        }

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

        public void GetClans()
        {
            foreach (string _id in PersistentContainer.Instance.Players.SteamIDs)
            {
                Player p = PersistentContainer.Instance.Players[_id, false];
                if (p.IsClanOwner)
                {
                    if (!clans.ContainsKey(p.ClanName))
                    {
                        if (p.ClanName != null)
                        {
                            clans.Add(p.ClanName, _id);
                        }
                    }
                }
            }
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            Session[_cInfo.playerId] = DateTime.Now;
        }

        public static void KillCount()
        {
            List<EntityPlayer> _playerList = GameManager.Instance.World.Players.list;
            for (int i = 0; i < _playerList.Count; i++)
            {
                EntityPlayer _player = _playerList[i];
                if (_player != null)
                {
                    if (!Dead.Contains(_player.entityId))
                    {
                        if (_player.IsDead())
                        {
                            Dead.Add(_player.entityId);
                            DeathTime[_player.entityId] = DateTime.Now;
                            LastDeathPos[_player.entityId] = _player.position;
                            for (int j = 0; j < _playerList.Count; j++)
                            {
                                EntityPlayer _player2 = _playerList[j];
                                Entity _target = _player2.GetDamagedTarget();
                                if (_target == _player)
                                {
                                    ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_player.entityId);
                                    ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForEntityId(_player2.entityId);
                                    if (_cInfo != null && _cInfo2 != null)
                                    {
                                        if (Kill_Notice)
                                        {
                                            string _holdingItem = _player2.inventory.holdingItem.Name;
                                            if (_holdingItem == "handPlayer")
                                            {
                                                _holdingItem = "Fists of Fury";
                                            }
                                            string _phrase915;
                                            if (!Phrases.Dict.TryGetValue(915, out _phrase915))
                                            {
                                                _phrase915 = "{PlayerName} has killed {Victim} with {Item}.";
                                            }
                                            _phrase915 = _phrase915.Replace("{PlayerName}", _cInfo2.playerName);
                                            _phrase915 = _phrase915.Replace("{Victim}", _cInfo.playerName);
                                            _phrase915 = _phrase915.Replace("{Item}", _holdingItem);
                                            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase915), Config.Server_Response_Name, false, "ServerTools", false);
                                        }
                                        if (Bounties.IsEnabled)
                                        {
                                            int _bounty = PersistentContainer.Instance.Players[_cInfo.playerId, true].Bounty;
                                            if (_bounty > 0)
                                            {
                                                int _oldCoins = PersistentContainer.Instance.Players[_cInfo2.playerId, true].PlayerSpentCoins;
                                                PersistentContainer.Instance.Players[_cInfo2.playerId, true].PlayerSpentCoins = _oldCoins + _bounty;
                                                PersistentContainer.Instance.Players[_cInfo.playerId, true].Bounty = 0;
                                                PersistentContainer.Instance.Save();
                                                string _phrase912;
                                                if (!Phrases.Dict.TryGetValue(912, out _phrase912))
                                                {
                                                    _phrase912 = "{PlayerName} is a bounty hunter! {Victim} was snuffed out.";
                                                }
                                                _phrase912 = _phrase912.Replace("{PlayerName}", _cInfo2.playerName);
                                                _phrase912 = _phrase912.Replace("{Victim}", _cInfo.playerName);
                                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase912), Config.Server_Response_Name, false, "ServerTools", false);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
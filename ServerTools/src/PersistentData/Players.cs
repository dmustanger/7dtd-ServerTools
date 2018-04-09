using System;
using System.Collections.Generic;

namespace ServerTools
{
    [Serializable]
    public class Players
    {
        public static bool Kill_Notice = false;
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static Dictionary<int, int> Kills = new Dictionary<int, int>();
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
                    int _killValue;
                    if (Kills.TryGetValue(_player.entityId, out _killValue))
                    {
                        int _kills = XUiM_Player.GetPlayerKills(_player);
                        if (_killValue != _kills)
                        {
                            Kills[_player.entityId] = _kills;
                            Entity _target = _player.GetDamagedTarget();
                            EntityPlayer _entityTarget = GameManager.Instance.World.Players.dict[_target.entityId];
                            if (_entityTarget != null)
                            {
                                if (Kill_Notice)
                                {
                                    string _holdingItem = _player.inventory.holdingItem.Name;
                                    {
                                        ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_player.entityId);
                                        ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForEntityId(_target.entityId);
                                        string _phrase915;
                                        if (!Phrases.Dict.TryGetValue(915, out _phrase915))
                                        {
                                            _phrase915 = "{PlayerName} has killed {Victim} with {Item}.";
                                        }
                                        _phrase915 = _phrase915.Replace("{PlayerName}", _cInfo.playerName);
                                        _phrase915 = _phrase915.Replace("{Victim}", _cInfo1.playerName);
                                        _phrase915 = _phrase915.Replace("{Item}", _holdingItem);
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase915), Config.Server_Response_Name, false, "ServerTools", false);
                                    }
                                }
                                if (Bounties.IsEnabled)
                                {
                                    ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForEntityId(_target.entityId);
                                    int _bounty = PersistentContainer.Instance.Players[_cInfo1.playerId, true].Bounty;
                                    if (_bounty > 0)
                                    {
                                        ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_player.entityId);
                                        int _oldCoins = PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins;
                                        PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _oldCoins + _bounty;
                                        PersistentContainer.Instance.Players[_cInfo1.playerId, true].Bounty = 0;
                                        PersistentContainer.Instance.Save();
                                        string _phrase912;
                                        if (!Phrases.Dict.TryGetValue(912, out _phrase912))
                                        {
                                            _phrase912 = "{PlayerName} is a bounty hunter! {Victim} was taken out.";
                                        }
                                        _phrase912 = _phrase912.Replace("{PlayerName}", _cInfo.playerName);
                                        _phrase912 = _phrase912.Replace("{Victim}", _cInfo1.playerName);
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
using System;
using System.Collections.Generic;

namespace ServerTools
{
    [Serializable]
    public class PersistentPlayers
    {
        public Dictionary<string, PersistentPlayer> players = new Dictionary<string, PersistentPlayer>();

        public List<string> SteamIDs
        {
            get
            {
                return new List<string>(players.Keys);
            }
        }

        public PersistentPlayer this[string steamId]
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
                    if (steamId != null && steamId.Length == 17 && steamId.StartsWith("7656"))
                    {
                        PersistentPlayer p = new PersistentPlayer(steamId);
                        players.Add(steamId, p);
                        return p;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}

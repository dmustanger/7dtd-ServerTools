using System;
using System.Collections.Generic;

namespace ServerTools
{
    [Serializable]
    public class PersistentPlayers
    {
        public Dictionary<string, PersistentPlayer> Players = new Dictionary<string, PersistentPlayer>();

        public List<string> SteamIDs
        {
            get
            {
                return new List<string>(Players.Keys);
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
                else if (Players.ContainsKey(steamId))
                {
                    return Players[steamId];
                }
                else
                {
                    if (steamId != null && steamId.Length == 17 && steamId.StartsWith("7656119"))
                    {
                        PersistentPlayer p = new PersistentPlayer(steamId);
                        Players.Add(steamId, p);
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

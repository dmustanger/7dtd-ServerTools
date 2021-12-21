using System;
using System.Collections.Generic;

namespace ServerTools
{
    [Serializable]
    public class PersistentPlayers
    {
        public Dictionary<string, PersistentPlayer> Players = new Dictionary<string, PersistentPlayer>();

        public List<string> IDs
        {
            get
            {
                return new List<string>(Players.Keys);
            }
        }

        public PersistentPlayer this[string id]
        {
            get
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (Players.ContainsKey(id))
                    {
                        return Players[id];
                    }
                    else if (id.Contains("_"))
                    {
                        if (ConsoleHelper.ParseParamPartialNameOrId(id, out PlatformUserIdentifierAbs platformUserIdentifierAbs, out ClientInfo clientInfo, true) == 1 && platformUserIdentifierAbs != null)
                        {
                            PersistentPlayer p = new PersistentPlayer(id);
                            Players.Add(id, p);
                            return p;
                        }
                    }
                }
                return null;
            }
        }
    }
}

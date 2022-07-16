using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerTools
{
    [Serializable]
    public class PersistentContainer
    {
        public static bool DataChange = false;

        private static string Filepath = string.Format("{0}/ServerTools.bin", API.ConfigPath);
        private static PersistentContainer instance;
        private PersistentPlayers players;
        private static bool Saving = false;

        private Dictionary<int, int> backpacks;
        private string bannedCountries;
        private Dictionary<int, List<int>> clientMuteList;
        private Dictionary<string, byte[]> connections;
        private Dictionary<string, DateTime> connectionTimeOut;
        private bool countryBan;
        private Dictionary<string, DateTime> blockPickUp;
        private DateTime lastWeather;
        private List<string> protectedZones;
        private string[] pollData;
        private Dictionary<string[], string> pollOld;
        private bool pollOpen;
        private Dictionary<string, bool> pollVote;
        private bool proxyBan;
        private List<string> regionReset;
        private List<string[]> track;
        private Dictionary<string, string> webAuthorizedList;
        private Dictionary<string, DateTime> webAuthorizedTimeList;
        private List<string> webBanList;
        private Dictionary<string, DateTime> webTimeoutList;
        private int worldSeed;
        

        public static PersistentContainer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PersistentContainer();
                }
                return instance;
            }
        }

        public PersistentPlayers Players
        {
            get
            {
                if (players == null)
                {
                    players = new PersistentPlayers();
                }
                return players;
            }
        }

        private PersistentContainer()
        {
        }

        public void Save()
        {
            try
            {
                if (DataChange)
                {
                    if (!Saving)
                    {
                        Saving = true;
                        using (Stream stream = File.Open(Filepath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            BinaryFormatter bFormatter = new BinaryFormatter();
                            bFormatter.Serialize(stream, this);
                        }
                    }
                    DataChange = false;
                }
            }
            catch (Exception e)
            {
                
                Log.Out(string.Format("[SERVERTOOLS] Exception in PersistentContainer.Save: {0}", e.Message));
            }
            Saving = false;
        }

        public bool Load()
        {
            try
            {
                if (File.Exists(Filepath))
                {
                    PersistentContainer obj;
                    using (Stream stream = File.Open(Filepath, FileMode.Open, FileAccess.Read))
                    {
                        BinaryFormatter bFormatter = new BinaryFormatter();
                        obj = (PersistentContainer)bFormatter.Deserialize(stream);
                    }
                    instance = obj;
                    return true;
                }
                else
                {
                    using (Stream stream = File.Open(Filepath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        BinaryFormatter bFormatter = new BinaryFormatter();
                        bFormatter.Serialize(stream, this);
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Exception in PersistentContainer.Load: {0}", e.Message));
            }
            return false;
        }

        public Dictionary<int, int> Backpacks
        {
            get
            {
                return backpacks;
            }
            set
            {
                backpacks = value;
            }
        }

        public string BannedCountries
        {
            get
            {
                return bannedCountries;
            }
            set
            {
                bannedCountries = value;
            }
        }

        public Dictionary<int, List<int>> ClientMuteList
        {
            get
            {
                return clientMuteList;
            }
            set
            {
                clientMuteList = value;
            }
        }

        public Dictionary<string, byte[]> Connections
        {
            get
            {
                return connections;
            }
            set
            {
                connections = value;
            }
        }

        public Dictionary<string, DateTime> ConnectionTimeOut
        {
            get
            {
                return connectionTimeOut;
            }
            set
            {
                connectionTimeOut = value;
            }
        }

        public bool CountryBan
        {
            get
            {
                return countryBan;
            }
            set
            {
                countryBan = value;
            }
        }

        public Dictionary<string, DateTime> BlockPickUp
        {
            get
            {
                return blockPickUp;
            }
            set
            {
                blockPickUp = value;
            }
        }

        public DateTime LastWeather
        {
            get
            {
                return lastWeather;
            }
            set
            {
                lastWeather = value;
            }
        }

        public List<string> ProtectedZones
        {
            get
            {
                return protectedZones;
            }
            set
            {
                protectedZones = value;
            }
        }

        public string[] PollData
        {
            get
            {
                return pollData;
            }
            set
            {
                pollData = value;
            }
        }

        public Dictionary<string[], string> PollOld
        {
            get
            {
                return pollOld;
            }
            set
            {
                pollOld = value;
            }
        }

        public bool PollOpen
        {
            get
            {
                return pollOpen;
            }
            set
            {
                pollOpen = value;
            }
        }

        public Dictionary<string, bool> PollVote
        {
            get
            {
                return pollVote;
            }
            set
            {
                pollVote = value;
            }
        }

        public bool ProxyBan
        {
            get
            {
                return proxyBan;
            }
            set
            {
                proxyBan = value;
            }
        }

        public List<string> RegionReset
        {
            get
            {
                return regionReset;
            }
            set
            {
                regionReset = value;
            }
        }

        public List<string[]> Track
        {
            get
            {
                return track;
            }
            set
            {
                track = value;
            }
        }

        public Dictionary<string, string> WebAuthorizedList
        {
            get
            {
                return webAuthorizedList;
            }
            set
            {
                webAuthorizedList = value;
            }
        }

        public Dictionary<string, DateTime> WebAuthorizedTimeList
        {
            get
            {
                return webAuthorizedTimeList;
            }
            set
            {
                webAuthorizedTimeList = value;
            }
        }

        public List<string> WebBanList
        {
            get
            {
                return webBanList;
            }
            set
            {
                webBanList = value;
            }
        }

        public Dictionary<string, DateTime> WebTimeoutList
        {
            get
            {
                return webTimeoutList;
            }
            set
            {
                webTimeoutList = value;
            }
        }

        public int WorldSeed
        {
            get
            {
                return worldSeed;
            }
            set
            {
                worldSeed = value;
            }
        }
    }
}
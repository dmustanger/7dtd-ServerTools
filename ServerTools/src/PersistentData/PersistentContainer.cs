using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerTools
{
    [Serializable]
    public class PersistentContainer
    {
        private static string filepath = string.Format("{0}/ServerTools.bin", API.ConfigPath);
        private static PersistentContainer instance;
        private PersistentPlayers players;

        private Dictionary<int, int> auctionPrices;
        private Dictionary<int, List<int>> clientMuteList;
        private DateTime lastWeather;
        private string[] pollData;
        private Dictionary<string[], string> pollOld;
        private bool pollOpen;
        private Dictionary<string, bool> pollVote;
        private List<string[]> track;
        private Dictionary<string, string[]> websiteAuthorizedList;
        private Dictionary<string, DateTime> websiteAuthorizedTimeList;
        private List<string> websiteBanList;
        private List<string> websiteClientList;
        private Dictionary<string, DateTime> websiteTimeoutList;

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
            Stream stream = File.Open(filepath, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, this);
            stream.Close();
        }

        public bool Load()
        {
            if (File.Exists(filepath))
            {
                try
                {
                    PersistentContainer obj;
                    Stream stream = File.Open(filepath, FileMode.Open);
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    obj = (PersistentContainer)bFormatter.Deserialize(stream);
                    stream.Close();
                    instance = obj;
                    return true;
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Exception in PersistentContainer.Load: {0}", e.Message));
                }
            }
            return false;
        }

        public Dictionary<int, int> AuctionPrices
        {
            get
            {
                return auctionPrices;
            }
            set
            {
                auctionPrices = value;
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

        public Dictionary<string, string[]> WebsiteAuthorizedList
        {
            get
            {
                return websiteAuthorizedList;
            }
            set
            {
                websiteAuthorizedList = value;
            }
        }

        public Dictionary<string, DateTime> WebsiteAuthorizedTimeList
        {
            get
            {
                return websiteAuthorizedTimeList;
            }
            set
            {
                websiteAuthorizedTimeList = value;
            }
        }

        public List<string> WebsiteBanList
        {
            get
            {
                return websiteBanList;
            }
            set
            {
                websiteBanList = value;
            }
        }

        public List<string> WebsiteClientList
        {
            get
            {
                return websiteClientList;
            }
            set
            {
                websiteClientList = value;
            }
        }

        public Dictionary<string, DateTime> WebsiteTimeoutList
        {
            get
            {
                return websiteTimeoutList;
            }
            set
            {
                websiteTimeoutList = value;
            }
        }
    }
}
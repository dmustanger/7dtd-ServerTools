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
        private bool Saving = false;

        private Dictionary<int, int> auctionPrices;
        private Dictionary<int, List<int>> clientMuteList;
        private DateTime lastWeather;
        private string[] pollData;
        private Dictionary<string[], string> pollOld;
        private bool pollOpen;
        private Dictionary<string, bool> pollVote;
        private List<int[]> protectedSpace;
        private List<string> regionReset;
        private List<string[]> track;
        private Dictionary<string, string[]> webPanelAuthorizedList;
        private Dictionary<string, DateTime> webPanelAuthorizedTimeList;
        private List<string> webPanelBanList;
        private List<string> webPanelClientList;
        private Dictionary<string, DateTime> webPanelTimeoutList;

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
                if (!Saving)
                {
                    Saving = true;
                    Stream stream = File.Open(filepath, FileMode.Create, FileAccess.ReadWrite);
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(stream, this);
                    stream.Close();
                }
                else
                {
                    Timers.SaveDelay();
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
            if (File.Exists(filepath))
            {
                try
                {
                    PersistentContainer obj;
                    Stream stream = File.Open(filepath, FileMode.Open, FileAccess.Read);
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

        public List<int[]> ProtectedSpace
        {
            get
            {
                return protectedSpace;
            }
            set
            {
                protectedSpace = value;
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

        public Dictionary<string, string[]> WebsiteAuthorizedList
        {
            get
            {
                return webPanelAuthorizedList;
            }
            set
            {
                webPanelAuthorizedList = value;
            }
        }

        public Dictionary<string, DateTime> WebsiteAuthorizedTimeList
        {
            get
            {
                return webPanelAuthorizedTimeList;
            }
            set
            {
                webPanelAuthorizedTimeList = value;
            }
        }

        public List<string> WebPanelBanList
        {
            get
            {
                return webPanelBanList;
            }
            set
            {
                webPanelBanList = value;
            }
        }

        public List<string> WebPanelClientList
        {
            get
            {
                return webPanelClientList;
            }
            set
            {
                webPanelClientList = value;
            }
        }

        public Dictionary<string, DateTime> WebPanelTimeoutList
        {
            get
            {
                return webPanelTimeoutList;
            }
            set
            {
                webPanelTimeoutList = value;
            }
        }
    }
}
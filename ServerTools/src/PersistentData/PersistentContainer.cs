using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerTools
{
    [Serializable]
    public class PersistentContainer
    {
        private static string filepath = string.Format("{0}/ServerTools.bin", GameUtils.GetSaveGameDir());
        private PersistentPlayers players;
        private static PersistentContainer instance;

        private DateTime lastWeather;
        private Dictionary<int, List<int>> clientMuteList;
        private List<string> websiteClientList;
        private List<string> websiteBanList;
        private Dictionary<string, string[]> websiteAuthorizedList;
        private Dictionary<string, DateTime> websiteAuthorizedTimeList;
        private Dictionary<string, DateTime> websiteTimeoutList;

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

        public static bool Load()
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
                    Log.Out(string.Format("[SERVERTOOLS] Exception in PersistentContainer.Load: {0}", e));
                }
            }
            return false;
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
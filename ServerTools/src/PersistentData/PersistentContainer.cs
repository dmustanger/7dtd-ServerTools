﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerTools
{
    [Serializable]
    public class PersistentContainer
    {
        public static bool DataChange = false;

        private static string FilePath = string.Format("{0}/ServerTools.bin", API.ConfigPath);
        private static string BackupFilePath = string.Format("{0}/ServerTools_Backup.bin", API.ConfigPath);
        private static PersistentContainer instance;
        private PersistentPlayers players;
        private static bool Saving = false;

        private Dictionary<int, int> backpacks;
        private string bannedCountries;
        private Dictionary<string, DateTime> chunkReset;
        private Dictionary<int, List<int>> clientMuteList;
        private Dictionary<string, DateTime> connectionTimeOut;
        private bool countryBan;
        private Dictionary<string, DateTime> blockPickUp;
        private DateTime lastWeather;
        private int lotteryPot;
        private List<string> protectedZones;
        private string[] pollData;
        private Dictionary<string[], string> pollOld;
        private bool pollOpen;
        private Dictionary<string, bool> pollVote;
        private bool proxyBan;
        private Dictionary<string, DateTime> regionReset;
        private List<string[]> shopLog;
        private List<string[]> track;
        private Dictionary<string, string> webAuthorizedList;
        private Dictionary<string, DateTime> webAuthorizedTimeList;
        private List<string> webBanList;
        private Dictionary<string, DateTime> webTimeoutList;
        private int worldSeed;
        private DateTime savePoint;

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

        public void Save(bool _shutdown)
        {
            try
            {
                if (DataChange && !Saving)
                {
                    Saving = true;
                    DateTime savePoint = DateTime.Now;
                    Instance.SavePoint = savePoint;
                    PersistentContainer container = Instance;
                    using (Stream stream = File.Create(FilePath))
                    {
                        BinaryFormatter bFormatter = new BinaryFormatter();
                        bFormatter.Serialize(stream, container);
                    }
                    if (!_shutdown)
                    {
                        System.Timers.Timer singleUseTimer = new System.Timers.Timer(10000)
                        {
                            AutoReset = false
                        };
                        singleUseTimer.Start();
                        singleUseTimer.Elapsed += (sender, e) =>
                        {
                            singleUseTimer.Stop();
                            using (Stream stream = File.Create(BackupFilePath))
                            {
                                BinaryFormatter bFormatter = new BinaryFormatter();
                                bFormatter.Serialize(stream, container);
                            }
                            singleUseTimer.Close();
                            singleUseTimer.Dispose();
                        };
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
                if (File.Exists(FilePath))
                {
                    PersistentContainer obj;
                    using (Stream stream = File.Open(FilePath, FileMode.Open, FileAccess.Read))
                    {
                        BinaryFormatter bFormatter = new BinaryFormatter();
                        obj = (PersistentContainer)bFormatter.Deserialize(stream);
                    }
                    instance = obj;
                    if ((instance.SavePoint == null || DateTime.Now.AddYears(-10) > instance.SavePoint) && File.Exists(BackupFilePath))
                    {
                        using (Stream stream = File.Open(BackupFilePath, FileMode.Open, FileAccess.Read))
                        {
                            BinaryFormatter bFormatter = new BinaryFormatter();
                            obj = (PersistentContainer)bFormatter.Deserialize(stream);
                        }
                        instance = obj;
                    }
                    return true;
                }
                else
                {
                    using (Stream stream = File.Create(FilePath))
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

        public Dictionary<string, DateTime> ChunkReset
        {
            get
            {
                return chunkReset;
            }
            set
            {
                chunkReset = value;
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

        public int LotteryPot
        {
            get
            {
                return lotteryPot;
            }
            set
            {
                lotteryPot = value;
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

        public Dictionary<string, DateTime> RegionReset
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

        public List<string[]> ShopLog
        {
            get
            {
                return shopLog;
            }
            set
            {
                shopLog = value;
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

        public DateTime SavePoint
        {
            get
            {
                return savePoint;
            }
            set
            {
                savePoint = value;
            }
        }
    }
}
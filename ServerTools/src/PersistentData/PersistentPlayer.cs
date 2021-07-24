using System;
using System.Collections.Generic;

namespace ServerTools
{
    [Serializable]

    public class PersistentPlayer
    {
        private readonly string steamId;
        private Dictionary<int, ItemDataSerializable> auction;
        private Dictionary<int, ItemDataSerializable> auctionReturn;
        private int bank;
        private int bikeId;
        private int bounty;
        private int bountyHunter;
        private string clanInvite;
        private string clanName;
        private bool clanOfficer;
        private bool clanOwner;
        private Dictionary<string, string> clanRequestToJoin;
        private bool countryBanImmune;
        private Dictionary<string, DateTime> customCommandDelays;
        private bool eventOver;
        private string eventReturnPosition;
        private bool eventSpawn;
        private List<List<string>> events;
        private bool firstClaimBlock;
        private int gyroId;
        private bool hardcoreEnabled;
        private List<string[]> hardcoreSavedStats;
        private string[] hardcoreStats;
        private bool highPingImmune;
        private string highPingImmuneName;
        private Dictionary<string, string> homes;
        private DateTime jailDate;
        private string jailName;
        private int jailTime;
        private int jeepId;
        private DateTime lastAnimal;
        private DateTime lastBike;
        private DateTime lastDied;
        private DateTime lastFriendTele;
        private DateTime lastGimme;
        private DateTime lastGyro;
        private DateTime lastHome;
        private DateTime lastJeep;
        private DateTime lastJoined;
        private DateTime lastKillMe;
        private DateTime lastLobby;
        private DateTime lastLog;
        private DateTime lastMarket;
        private DateTime lastMiniBike;
        private DateTime lastMotorBike;
        private DateTime lastNameColorChange;
        private DateTime lastPrayer;
        private DateTime lastPrefixColorChange;
        private DateTime lastScout;
        private DateTime lastStuck;
        private DateTime lastTravel;
        private DateTime lastVote;
        private DateTime lastVoteWeek;
        private DateTime lastWaypoint;
        private string lastWhisper;
        private string lobbyReturnPos;
        private string marketReturnPos;
        private int miniBikeId;
        private int motorBikeId;
        private DateTime muteDate;
        private string muteName;
        private int muteTime;
        private bool newSpawn;
        private string newSpawnPosition;
        private bool oldPlayer;
        private string playerName;
        private int playerWallet;
        private int sessionTime;
        private bool startingItems;
        private int totalTimePlayed;
        private int voteWeekCount;
        private Dictionary<string, string> waypoints;
        private string webPass;
        private DateTime zoneDeathTime;

        public Dictionary<int, ItemDataSerializable> Auction
        {
            get
            {
                return auction;
            }
            set
            {
                auction = value;
            }
        }

        public Dictionary<int, ItemDataSerializable> AuctionReturn
        {
            get
            {
                return auctionReturn;
            }
            set
            {
                auctionReturn = value;
            }
        }

        public int Bank
        {
            get
            {
                return bank;
            }
            set
            {
                bank = value;
            }
        }

        public int BikeId
        {
            get
            {
                return bikeId;
            }
            set
            {
                bikeId = value;
            }
        }

        public int Bounty
        {
            get
            {
                return bounty;
            }
            set
            {
                bounty = value;
            }
        }

        public int BountyHunter
        {
            get
            {
                return bountyHunter;
            }
            set
            {
                bountyHunter = value;
            }
        }

        public string ClanInvite
        {
            get
            {
                return clanInvite;
            }
            set
            {
                clanInvite = value;
            }
        }

        public string ClanName
        {
            get
            {
                return clanName;
            }
            set
            {
                clanName = value;
            }
        }

        public bool ClanOwner
        {
            get
            {
                return clanOwner;
            }
            set
            {
                clanOwner = value;
            }
        }

        public bool ClanOfficer
        {
            get
            {
                return clanOfficer;
            }
            set
            {
                clanOfficer = value;
            }
        }

        public Dictionary<string, string> ClanRequestToJoin
        {
            get
            {
                return clanRequestToJoin;
            }
            set
            {
                clanRequestToJoin = value;
            }
        }

        public bool CountryBanImmune
        {
            get
            {
                return countryBanImmune;
            }
            set
            {
                countryBanImmune = value;
            }
        }

        public Dictionary<string, DateTime> CustomCommandDelays
        {
            get
            {
                return customCommandDelays;
            }
            set
            {
                customCommandDelays = value;
            }
        }

        public bool EventOver
        {
            get
            {
                return eventOver;
            }
            set
            {
                eventOver = value;
            }
        }

        public string EventReturnPosition
        {
            get
            {
                return eventReturnPosition;
            }
            set
            {
                eventReturnPosition = value;
            }
        }

        public bool EventSpawn
        {
            get
            {
                return eventSpawn;
            }
            set
            {
                eventSpawn = value;
            }
        }

        public List<List<string>> Events
        {
            get
            {
                return events;
            }
            set
            {
                events = value;
            }
        }

        public bool FirstClaimBlock
        {
            get
            {
                return firstClaimBlock;
            }
            set
            {
                firstClaimBlock = value;
            }
        }

        public int GyroId
        {
            get
            {
                return gyroId;
            }
            set
            {
                gyroId = value;
            }
        }

        public bool HardcoreEnabled
        {
            get
            {
                return hardcoreEnabled;
            }
            set
            {
                hardcoreEnabled = value;
            }
        }

        public List<string[]> HardcoreSavedStats
        {
            get
            {
                return hardcoreSavedStats;
            }
            set
            {
                hardcoreSavedStats = value;
            }
        }

        public string[] HardcoreStats
        {
            get
            {
                return hardcoreStats;
            }
            set
            {
                hardcoreStats = value;
            }
        }

        public bool HighPingImmune
        {
            get
            {
                return highPingImmune;
            }
            set
            {
                highPingImmune = value;
            }
        }

        public string HighPingImmuneName
        {
            get
            {
                return highPingImmuneName;
            }
            set
            {
                highPingImmuneName = value;
            }
        }

        public Dictionary<string, string> Homes
        {
            get
            {
                return homes;
            }
            set
            {
                homes = value;
            }
        }

        public string JailName
        {
            get
            {
                return jailName;
            }
            set
            {
                jailName = value;
            }
        }

        public int JailTime
        {
            get
            {
                return jailTime;
            }
            set
            {
                jailTime = value;
            }
        }

        public int JeepId
        {
            get
            {
                return jeepId;
            }
            set
            {
                jeepId = value;
            }
        }

        public DateTime LastAnimal
        {
            get
            {
                return lastAnimal;
            }
            set
            {
                lastAnimal = value;
            }
        }

        public DateTime LastBike
        {
            get
            {
                return lastBike;
            }
            set
            {
                lastBike = value;
            }
        }

        public DateTime LastDied
        {
            get
            {
                return lastDied;
            }
            set
            {
                lastDied = value;
            }
        }

        public DateTime LastFriendTele
        {
            get
            {
                return lastFriendTele;
            }
            set
            {
                lastFriendTele = value;
            }
        }

        public DateTime LastGimme
        {
            get
            {
                return lastGimme;
            }
            set
            {
                lastGimme = value;
            }
        }

        public DateTime LastGyro
        {
            get
            {
                return lastGyro;
            }
            set
            {
                lastGyro = value;
            }
        }

        public DateTime LastHome
        {
            get
            {
                return lastHome;
            }
            set
            {
                lastHome = value;
            }
        }

        public DateTime LastJeep
        {
            get
            {
                return lastJeep;
            }
            set
            {
                lastJeep = value;
            }
        }

        public DateTime LastJoined
        {
            get
            {
                return lastJoined;
            }
            set
            {
                lastJoined = value;
            }
        }

        public DateTime LastKillMe
        {
            get
            {
                return lastKillMe;
            }
            set
            {
                lastKillMe = value;
            }
        }

        public DateTime LastLobby
        {
            get
            {
                return lastLobby;
            }
            set
            {
                lastLobby = value;
            }
        }

        public DateTime LastLog
        {
            get
            {
                return lastLog;
            }
            set
            {
                lastLog = value;
            }
        }

        public DateTime LastMarket
        {
            get
            {
                return lastMarket;
            }
            set
            {
                lastMarket = value;
            }
        }

        public DateTime LastMiniBike
        {
            get
            {
                return lastMiniBike;
            }
            set
            {
                lastMiniBike = value;
            }
        }

        public DateTime LastMotorBike
        {
            get
            {
                return lastMotorBike;
            }
            set
            {
                lastMotorBike = value;
            }
        }

        public DateTime LastNameColorChange
        {
            get
            {
                return lastNameColorChange;
            }
            set
            {
                lastNameColorChange = value;
            }
        }

        public DateTime LastPrayer
        {
            get
            {
                return lastPrayer;
            }
            set
            {
                lastPrayer = value;
            }
        }

        public DateTime LastPrefixColorChange
        {
            get
            {
                return lastPrefixColorChange;
            }
            set
            {
                lastPrefixColorChange = value;
            }
        }

        public DateTime LastScout
        {
            get
            {
                return lastScout;
            }
            set
            {
                lastScout = value;
            }
        }

        public DateTime LastStuck
        {
            get
            {
                return lastStuck;
            }
            set
            {
                lastStuck = value;
            }
        }

        public DateTime LastTravel
        {
            get
            {
                return lastTravel;
            }
            set
            {
                lastTravel = value;
            }
        }

        public DateTime LastVote
        {
            get
            {
                return lastVote;
            }
            set
            {
                lastVote = value;
            }
        }

        public DateTime LastVoteWeek
        {
            get
            {
                return lastVoteWeek;
            }
            set
            {
                lastVoteWeek = value;
            }
        }

        public DateTime LastWaypoint
        {
            get
            {
                return lastWaypoint;
            }
            set
            {
                lastWaypoint = value;
            }
        }

        public string LastWhisper
        {
            get
            {
                return lastWhisper;
            }
            set
            {
                lastWhisper = value;
            }
        }

        public string LobbyReturnPos
        {
            get
            {
                return lobbyReturnPos;
            }
            set
            {
                lobbyReturnPos = value;
            }
        }

        public string MarketReturnPos
        {
            get
            {
                return marketReturnPos;
            }
            set
            {
                marketReturnPos = value;
            }
        }

        public int MiniBikeId
        {
            get
            {
                return miniBikeId;
            }
            set
            {
                miniBikeId = value;
            }
        }

        public int MotorBikeId
        {
            get
            {
                return motorBikeId;
            }
            set
            {
                motorBikeId = value;
            }
        }

        public string MuteName
        {
            get
            {
                return muteName;
            }
            set
            {
                muteName = value;
            }
        }

        public int MuteTime
        {
            get
            {
                return muteTime;
            }
            set
            {
                muteTime = value;
            }
        }

        public bool NewSpawn
        {
            get
            {
                return newSpawn;
            }
            set
            {
                newSpawn = value;
            }
        }

        public string NewSpawnPosition
        {
            get
            {
                return newSpawnPosition;
            }
            set
            {
                newSpawnPosition = value;
            }
        }

        public bool OldPlayer
        {
            get
            {
                return oldPlayer;
            }
            set
            {
                oldPlayer = value;
            }
        }

        public string PlayerName
        {
            get
            {
                return playerName;
            }
            set
            {
                playerName = value;
            }
        }

        public int PlayerWallet
        {
            get
            {
                return playerWallet;
            }
            set
            {
                playerWallet = value;
            }
        }

        public int SessionTime
        {
            get
            {
                return sessionTime;
            }
            set
            {
                sessionTime = value;
            }
        }

        public bool StartingItems
        {
            get
            {
                return startingItems;
            }
            set
            {
                startingItems = value;
            }
        }

        public int TotalTimePlayed
        {
            get
            {
                return totalTimePlayed;
            }
            set
            {
                totalTimePlayed = value;
            }
        }

        public int VoteWeekCount
        {
            get
            {
                return voteWeekCount;
            }
            set
            {
                voteWeekCount = value;
            }
        }

        public DateTime JailDate
        {
            get
            {
                return jailDate;
            }
            set
            {
                jailDate = value;
            }
        }

        public DateTime MuteDate
        {
            get
            {
                return muteDate;
            }
            set
            {
                muteDate = value;
            }
        }

        public Dictionary<string, string> Waypoints
        {
            get
            {
                return waypoints;
            }
            set
            {
                waypoints = value;
            }
        }

        public string WebPass
        {
            get
            {
                return webPass;
            }
            set
            {
                webPass = value;
            }
        }

        public DateTime ZoneDeathTime
        {
            get
            {
                return zoneDeathTime;
            }
            set
            {
                zoneDeathTime = value;
            }
        }

        public PersistentPlayer(string steamId)
        {
            this.steamId = steamId;
        }
    }
}

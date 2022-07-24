using System;
using System.Collections.Generic;

namespace ServerTools
{
    [Serializable]

    public class PersistentPlayer
    {
        private readonly string id;
        private Dictionary<int, ItemDataSerializable> auction;
        private Dictionary<int, ItemDataSerializable> auctionReturn;
        private List<string[]> autoPartyInvite;
        private int bank;
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
        private bool familyShareImmune;
        private bool firstClaimBlock;
        private bool hardcoreEnabled;
        private List<string[]> hardcoreSavedStats;
        private string[] hardcoreStats;
        private bool highPingImmune;
        private string highPingImmuneName;
        private Dictionary<string, string> homes;
        private int homeSpots;
        private DateTime jailDate;
        private string jailName;
        private int jailTime;
        private bool jailRelease;
        private DateTime lastAnimal;
        private DateTime lastBed;
        private DateTime lastDied;
        private DateTime lastFriendTele;
        private DateTime lastGamble;
        private DateTime lastGimme;
        private DateTime lastHome;
        private DateTime lastJoined;
        private DateTime lastKillMe;
        private DateTime lastLobby;
        private DateTime lastLog;
        private DateTime lastMarket;
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
        private DateTime muteDate;
        private string muteName;
        private int muteTime;
        private bool newSpawn;
        private string newSpawnPosition;
        private bool oldPlayer;
        private string playerName;
        private int playerWallet;
        private bool proxyBanImmune;
        private Dictionary<string, string> customReturnPositions;
        private int sessionTime;
        private bool startingItems;
        private int totalTimePlayed;
        private Dictionary<int, string[]> vehicles;
        private int voteWeekCount;
        private Dictionary<string, string> waypoints;
        private int waypointSpots;
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

        public List<string[]> AutoPartyInvite
        {
            get
            {
                return autoPartyInvite;
            }
            set
            {
                autoPartyInvite = value;
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

        public bool FamilyShareImmune
        {
            get
            {
                return familyShareImmune;
            }
            set
            {
                familyShareImmune = value;
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

        public int HomeSpots
        {
            get
            {
                return homeSpots;
            }
            set
            {
                homeSpots = value;
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

        public bool JailRelease
        {
            get
            {
                return jailRelease;
            }
            set
            {
                jailRelease = value;
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

        public DateTime LastBed
        {
            get
            {
                return lastBed;
            }
            set
            {
                lastBed = value;
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

        public DateTime LastGamble
        {
            get
            {
                return lastGamble;
            }
            set
            {
                lastGamble = value;
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

        public bool ProxyBanImmune
        {
            get
            {
                return proxyBanImmune;
            }
            set
            {
                proxyBanImmune = value;
            }
        }

        public Dictionary<string, string> CustomReturnPositions
        {
            get
            {
                return customReturnPositions;
            }
            set
            {
                customReturnPositions = value;
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

        public Dictionary<int, string[]> Vehicles
        {
            get
            {
                return vehicles;
            }
            set
            {
                vehicles = value;
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

        public int WaypointSpots
        {
            get
            {
                return waypointSpots;
            }
            set
            {
                waypointSpots = value;
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

        public PersistentPlayer(string id)
        {
            this.id = id;
        }
    }
}

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
        private DateTime customCommand1;
        private DateTime customCommand2;
        private DateTime customCommand3;
        private DateTime customCommand4;
        private DateTime customCommand5;
        private DateTime customCommand6;
        private DateTime customCommand7;
        private DateTime customCommand8;
        private DateTime customCommand9;
        private DateTime customCommand10;
        private DateTime customCommand11;
        private DateTime customCommand12;
        private DateTime customCommand13;
        private DateTime customCommand14;
        private DateTime customCommand15;
        private DateTime customCommand16;
        private DateTime customCommand17;
        private DateTime customCommand18;
        private DateTime customCommand19;
        private DateTime customCommand20;
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
        private string homePosition1;
        private string homePosition2;
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
        private DateTime lastHome1;
        private DateTime lastHome2;
        private DateTime lastJeep;
        private DateTime lastJoined;
        private DateTime lastKillMe;
        private DateTime lastLobby;
        private DateTime lastMarket;
        private DateTime lastMiniBike;
        private DateTime lastMotorBike;
        private DateTime lastLog;
        private DateTime lastPrayer;
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
        private List<ItemValue> vault;
        private int voteWeekCount;
        private Dictionary<string, string> waypoints;
        private string wP;
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

        public DateTime CustomCommand1
        {
            get
            {
                return customCommand1;
            }
            set
            {
                customCommand1 = value;
            }
        }

        public DateTime CustomCommand2
        {
            get
            {
                return customCommand2;
            }
            set
            {
                customCommand2 = value;
            }
        }

        public DateTime CustomCommand3
        {
            get
            {
                return customCommand3;
            }
            set
            {
                customCommand3 = value;
            }
        }

        public DateTime CustomCommand4
        {
            get
            {
                return customCommand4;
            }
            set
            {
                customCommand4 = value;
            }
        }

        public DateTime CustomCommand5
        {
            get
            {
                return customCommand5;
            }
            set
            {
                customCommand5 = value;
            }
        }

        public DateTime CustomCommand6
        {
            get
            {
                return customCommand6;
            }
            set
            {
                customCommand6 = value;
            }
        }

        public DateTime CustomCommand7
        {
            get
            {
                return customCommand7;
            }
            set
            {
                customCommand7 = value;
            }
        }

        public DateTime CustomCommand8
        {
            get
            {
                return customCommand8;
            }
            set
            {
                customCommand8 = value;
            }
        }

        public DateTime CustomCommand9
        {
            get
            {
                return customCommand9;
            }
            set
            {
                customCommand9 = value;
            }
        }

        public DateTime CustomCommand10
        {
            get
            {
                return customCommand10;
            }
            set
            {
                customCommand10 = value;
            }
        }

        public DateTime CustomCommand11
        {
            get
            {
                return customCommand11;
            }
            set
            {
                customCommand11 = value;
            }
        }

        public DateTime CustomCommand12
        {
            get
            {
                return customCommand12;
            }
            set
            {
                customCommand12 = value;
            }
        }

        public DateTime CustomCommand13
        {
            get
            {
                return customCommand13;
            }
            set
            {
                customCommand13 = value;
            }
        }

        public DateTime CustomCommand14
        {
            get
            {
                return customCommand14;
            }
            set
            {
                customCommand14 = value;
            }
        }

        public DateTime CustomCommand15
        {
            get
            {
                return customCommand15;
            }
            set
            {
                customCommand15 = value;
            }
        }

        public DateTime CustomCommand16
        {
            get
            {
                return customCommand16;
            }
            set
            {
                customCommand16 = value;
            }
        }

        public DateTime CustomCommand17
        {
            get
            {
                return customCommand17;
            }
            set
            {
                customCommand17 = value;
            }
        }

        public DateTime CustomCommand18
        {
            get
            {
                return customCommand18;
            }
            set
            {
                customCommand18 = value;
            }
        }

        public DateTime CustomCommand19
        {
            get
            {
                return customCommand19;
            }
            set
            {
                customCommand19 = value;
            }
        }

        public DateTime CustomCommand20
        {
            get
            {
                return customCommand20;
            }
            set
            {
                customCommand20 = value;
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

        public string HomePosition1
        {
            get
            {
                return homePosition1;
            }
            set
            {
                homePosition1 = value;
            }
        }

        public string HomePosition2
        {
            get
            {
                return homePosition2;
            }
            set
            {
                homePosition2 = value;
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

        public DateTime LastHome1
        {
            get
            {
                return lastHome1;
            }
            set
            {
                lastHome1 = value;
            }
        }

        public DateTime LastHome2
        {
            get
            {
                return lastHome2;
            }
            set
            {
                lastHome2 = value;
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

        public List<ItemValue> Vault
        {
            get
            {
                return vault;
            }
            set
            {
                vault = value;
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

        public string WP
        {
            get
            {
                return wP;
            }
            set
            {
                wP = value;
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

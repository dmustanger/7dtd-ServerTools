using System;
using System.Runtime.Serialization;

namespace ServerTools
{
    [Serializable]
    public class PersistentPlayer
    {
        private readonly string steamId;
        [OptionalField]
        private int playerWallet;
        private int bank;
        private int messageCount;
        private int totalTimePlayed;
        private int treeDistance;
        private int viewDistance;
        private int fieldOfView;
        private int auctionId;
        private int auctionItemCount;
        private int auctionItemQuality;
        private int auctionItemPrice;
        private int bounty;
        private int bountyHunter;
        private int jailTime;
        private int muteTime;
        private int sessionTime;
        private int bikeId;
        private int miniBikeId;
        private int motorBikeId;
        private int jeepId;
        private int gyroId;
        private int voteWeekCount;
        private int hardcoreExtraLives;
        private DateTime messageTime;
        private DateTime lastJoined;
        private DateTime lastGimme;
        private DateTime auctionSellDate;
        private DateTime auctionCancelTime;
        private DateTime jailDate;
        private DateTime muteDate;
        private DateTime lastAnimal;
        private DateTime lastDied;
        private DateTime lastFriendTele;
        private DateTime lastLobby;
        private DateTime lastMarket;
        private DateTime lastLog;
        private DateTime lastStuck;
        private DateTime lastKillMe;
        private DateTime lastHome1;
        private DateTime lastHome2;
        private DateTime lastTravel;
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
        private DateTime lastBike;
        private DateTime lastMiniBike;
        private DateTime lastMotorBike;
        private DateTime lastJeep;
        private DateTime lastGyro;
        private DateTime lastVote;
        private DateTime lastVoteWeek;
        private DateTime zoneDeathTime;
        private bool clanOwner;
        private bool clanOfficer;
        private bool firstClaimBlock;
        private bool countryBanImmune;
        private bool highPingImmune;
        private bool newSpawn;
        private bool startingItems;
        private string eventSignUpPosition;
        private string eventName1;
        private string eventName2;
        private string eventName3;
        private string eventName4;
        private string eventName5;
        private string auctionItemName;
        private string clanName;
        private string clanInvite;
        private string jailName;
        private string muteName;
        private string lobbyReturnPos;
        private string marketReturnPos;
        private string homePosition1;
        private string homePosition2;
        private string lastWhisper;
        private string highPingImmuneName;
        private string newSpawnPosition;

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

        public int MessageCount
        {
            get
            {
                return messageCount;
            }
            set
            {
                messageCount = value;
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

        public int TreeDistance
        {
            get
            {
                return treeDistance;
            }
            set
            {
                treeDistance = value;
            }
        }

        public int ViewDistance
        {
            get
            {
                return viewDistance;
            }
            set
            {
                viewDistance = value;
            }
        }

        public int FieldOfView
        {
            get
            {
                return fieldOfView;
            }
            set
            {
                fieldOfView = value;
            }
        }

        public int AuctionId
        {
            get
            {
                return auctionId;
            }
            set
            {
                auctionId = value;
            }
        }

        public int AuctionItemCount
        {
            get
            {
                return auctionItemCount;
            }
            set
            {
                auctionItemCount = value;
            }
        }

        public int AuctionItemQuality
        {
            get
            {
                return auctionItemQuality;
            }
            set
            {
                auctionItemQuality = value;
            }
        }

        public int AuctionItemPrice
        {
            get
            {
                return auctionItemPrice;
            }
            set
            {
                auctionItemPrice = value;
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

        public int HardcoreExtraLives
        {
            get
            {
                return hardcoreExtraLives;
            }
            set
            {
                hardcoreExtraLives = value;
            }
        }

        public DateTime MessageTime
        {
            get
            {
                return messageTime;
            }
            set
            {
                messageTime = value;
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

        public DateTime AuctionSellDate
        {
            get
            {
                return auctionSellDate;
            }
            set
            {
                auctionSellDate = value;
            }
        }

        public DateTime AuctionCancelTime
        {
            get
            {
                return auctionCancelTime;
            }
            set
            {
                auctionCancelTime = value;
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

        public string EventSignUpPosition
        {
            get
            {
                return eventSignUpPosition;
            }
            set
            {
                eventSignUpPosition = value;
            }
        }

        public string EventName1
        {
            get
            {
                return eventName1;
            }
            set
            {
                eventName1 = value;
            }
        }

        public string EventName2
        {
            get
            {
                return eventName2;
            }
            set
            {
                eventName2 = value;
            }
        }

        public string EventName3
        {
            get
            {
                return eventName3;
            }
            set
            {
                eventName3 = value;
            }
        }

        public string EventName4
        {
            get
            {
                return eventName4;
            }
            set
            {
                eventName4 = value;
            }
        }

        public string EventName5
        {
            get
            {
                return eventName5;
            }
            set
            {
                eventName5 = value;
            }
        }

        public string AuctionItemName
        {
            get
            {
                return auctionItemName;
            }
            set
            {
                auctionItemName = value;
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

        public PersistentPlayer(string steamId)
        {
            this.steamId = steamId;
        }
    }
}

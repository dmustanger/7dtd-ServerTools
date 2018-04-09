using System;
using System.Runtime.Serialization;

namespace ServerTools
{
    [Serializable]
    public class Player
    {
        private readonly string steamId;
        [OptionalField]
        private int bounty;
        private int sessionTime;
        private int bikeId;
        private int jailTime;
        private int muteTime;
        private int worldSeedCoins;
        private int zkills;
        private int kills;
        private int deaths;
        private int auctionData;
        private int playerSpentCoins;
        private string[] auctionItem;
        private string lobbyReturn;
        private string newTeleSpawn;
        private string jailName;
        private string muteName;
        private string homeposition;
        private string homeposition2;
        private string clanname;
        private string invitedtoclan;
        private string lastwhisper;
        private DateTime lastLobby;
        private DateTime cancelTime;
        private DateTime sellDate;
        private DateTime log;
        private DateTime jailDate;
        private DateTime muteDate;
        private DateTime lastBike;
        private DateTime lastBackpack;
        private DateTime lastDied;
        private DateTime lastFriendTele;
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
        private DateTime respawnTime;
        private DateTime lastTravel;
        private DateTime lastAnimals;
        private DateTime lastVoteReward;
        private DateTime lastgimme;
        private DateTime lastkillme;
        private DateTime lastsethome;
        private bool firstClaim;
        private bool adminChatColor;
        private bool isclanowner;
        private bool isclanofficer;
        private bool ismuted;
        private bool isjailed;
        private bool isremovedfromjail = true;
        private bool startingItems;

        public string[] AuctionItem
        {
            get
            {
                return auctionItem;
            }
            set
            {
                auctionItem = value;
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

        public int ZKills
        {
            get
            {
                return zkills;
            }
            set
            {
                zkills = value;
            }
        }

        public int Kills
        {
            get
            {
                return kills;
            }
            set
            {
                kills = value;
            }
        }

        public int Deaths
        {
            get
            {
                return deaths;
            }
            set
            {
                deaths = value;
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

        public int WorldSeedCoins
        {
            get
            {
                return worldSeedCoins;
            }
            set
            {
                worldSeedCoins = value;
            }
        }

        public int AuctionData
        {
            get
            {
                return auctionData;
            }
            set
            {
                auctionData = value;
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

        public DateTime Log
        {
            get
            {
                return log;
            }
            set
            {
                log = value;
            }
        }

        public DateTime CancelTime
        {
            get
            {
                return cancelTime;
            }
            set
            {
                cancelTime = value;
            }
        }

        public DateTime SellDate
        {
            get
            {
                return sellDate;
            }
            set
            {
                sellDate = value;
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

        public DateTime LastBackpack
        {
            get
            {
                return lastBackpack;
            }
            set
            {
                lastBackpack = value;
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

        public DateTime RespawnTime
        {
            get
            {
                return respawnTime;
            }
            set
            {
                respawnTime = value;
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

        public int PlayerSpentCoins
        {
            get
            {
                return playerSpentCoins;
            }
            set
            {
                playerSpentCoins = value;
            }
        }

        public bool FirstClaim
        {
            get
            {
                return firstClaim;
            }
            set
            {
                firstClaim = value;
            }
        }

        public bool AdminChatColor
        {
            get
            {
                return adminChatColor;
            }
            set
            {
                adminChatColor = value;
            }
        }

        public DateTime LastVoteReward
        {
            get
            {
                return lastVoteReward;
            }
            set
            {
                lastVoteReward = value;
            }
        }

        public DateTime LastAnimals
        {
            get
            {
                return lastAnimals;
            }
            set
            {
                lastAnimals = value;
            }
        }

        public DateTime LastGimme
        {
            get
            {
                return lastgimme;
            }
            set
            {
                lastgimme = value;
            }
        }

        public DateTime LastKillme
        {
            get
            {
                return lastkillme;
            }
            set
            {
                lastkillme = value;
            }
        }

        public DateTime LastSetHome
        {
            get
            {
                return lastsethome;
            }
            set
            {
                lastsethome = value;
            }
        }

        public string LobbyReturn
        {
            get
            {
                return lobbyReturn;
            }
            set
            {
                lobbyReturn = value;
            }
        }

        public string NewTeleSpawn
        {
            get
            {
                return newTeleSpawn;
            }
            set
            {
                newTeleSpawn = value;
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

        public string HomePosition
        {
            get
            {
                return homeposition;
            }
            set
            {
                homeposition = value;
            }
        }

        public string HomePosition2
        {
            get
            {
                return homeposition2;
            }
            set
            {
                homeposition2 = value;
            }
        }

        public string ClanName
        {
            get
            {
                return clanname;
            }
            set
            {
                clanname = value;
            }
        }

        public bool IsClanOwner
        {
            get
            {
                return isclanowner;
            }
            set
            {
                isclanowner = value;
            }
        }

        public bool IsClanOfficer
        {
            get
            {
                return isclanofficer;
            }
            set
            {
                isclanofficer = value;
            }
        }

        public string InvitedToClan
        {
            get
            {
                return invitedtoclan;
            }
            set
            {
                invitedtoclan = value;
            }
        }

        public string LastWhisper
        {
            get
            {
                return lastwhisper;
            }
            set
            {
                lastwhisper = value;
            }
        }

        public bool IsMuted
        {
            get
            {
                return ismuted;
            }
            set
            {
                ismuted = value;
            }
        }

        public bool IsJailed
        {
            get
            {
                return isjailed;
            }
            set
            {
                isjailed = value;
            }
        }
        
        public bool IsRemovedFromJail
        {
            get
            {
                return isremovedfromjail;
            }
            set
            {
                isremovedfromjail = value;
            }
        }

        public Player(string steamId)
        {
            this.steamId = steamId;
        }
    }
}
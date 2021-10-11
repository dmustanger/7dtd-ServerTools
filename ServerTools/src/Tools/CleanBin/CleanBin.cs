using System;
using System.Collections.Generic;

namespace ServerTools
{
    class CleanBin
    {
        public static bool IsEnabled = false, Auction = false, Bank = false, Bounties = false, Delays = false, 
            Homes = false, Jail = false, Lobby = false, Market = false, New_Spawn_Tele = false, Poll = false, 
            Protected_Spaces = false, Vehicles = false, Wallet = false, Waypoints = false;

        public static void Exec()
        {
            if (Poll)
            {
                PersistentContainer.Instance.PollData = new string[] { };
                PersistentContainer.Instance.PollOld = new Dictionary<string[], string>();
                PersistentContainer.Instance.PollOpen = false;
                PersistentContainer.Instance.PollVote = new Dictionary<string, bool>();
            }
            if (Auction)
            {
                PersistentContainer.Instance.AuctionPrices = new Dictionary<int, int>();
            }
            List<string> steamId = PersistentContainer.Instance.Players.SteamIDs;
            for (int i = 0; i < steamId.Count; i++)
            {
                if (Auction)
                {
                    PersistentContainer.Instance.Players[steamId[i]].Auction = new Dictionary<int, ItemDataSerializable>();
                    PersistentContainer.Instance.Players[steamId[i]].AuctionReturn = new Dictionary<int, ItemDataSerializable>();
                }
                if (Bank)
                {
                    PersistentContainer.Instance.Players[steamId[i]].Bank = 0;
                }
                if (Bounties)
                {
                    PersistentContainer.Instance.Players[steamId[i]].Bounty = 0;
                    PersistentContainer.Instance.Players[steamId[i]].BountyHunter = 0;
                }
                if (Delays)
                {
                    PersistentContainer.Instance.Players[steamId[i]].LastAnimal = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastDied = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastFriendTele = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastGamble = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastGimme = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastHome = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastJoined = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastKillMe = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastLobby = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastLog = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastMarket = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastNameColorChange = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastPrayer = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastPrefixColorChange = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastScout = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastStuck = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastTravel = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastVote = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastVoteWeek = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].LastWaypoint = new DateTime();
                }
                if (Homes)
                {
                    PersistentContainer.Instance.Players[steamId[i]].Homes = new Dictionary<string, string>();
                }
                if (Jail)
                {
                    PersistentContainer.Instance.Players[steamId[i]].JailDate = new DateTime();
                    PersistentContainer.Instance.Players[steamId[i]].JailName = string.Empty;
                    PersistentContainer.Instance.Players[steamId[i]].JailTime = 0;
                }
                if (Lobby)
                {
                    PersistentContainer.Instance.Players[steamId[i]].LobbyReturnPos = string.Empty;
                }
                if (Market)
                {
                    PersistentContainer.Instance.Players[steamId[i]].MarketReturnPos = string.Empty;
                }
                if (Protected_Spaces)
                {
                    
                }
                if (New_Spawn_Tele)
                {
                    PersistentContainer.Instance.Players[steamId[i]].NewSpawn = false;
                    PersistentContainer.Instance.Players[steamId[i]].NewSpawnPosition = string.Empty;
                }
                if (Vehicles)
                {
                    PersistentContainer.Instance.Players[steamId[i]].Vehicles = new Dictionary<int, string[]>();
                }
                if (Wallet)
                {
                    PersistentContainer.Instance.Players[steamId[i]].PlayerWallet = 0;
                }
                if (Waypoints)
                {
                    PersistentContainer.Instance.Players[steamId[i]].Waypoints = new Dictionary<string, string>();
                }
            }
        }
    }
}

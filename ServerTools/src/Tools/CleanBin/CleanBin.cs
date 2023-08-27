using System;
using System.Collections.Generic;

namespace ServerTools
{
    class CleanBin
    {
        public static bool IsEnabled = false, Auction = false, Bank = false, Bounties = false, Chunk_Reset = false, Player_Delays = false,
            Homes = false, Jail = false, Lobby = false, Market = false, New_Spawn_Tele = false, POI_Reset = false, Poll = false,
            Protected_Zones = false, Region_Reset = false, Shop_Log = false, Waypoints = false;

        public static void Exec()
        {
            if (Poll)
            {
                PersistentContainer.Instance.PollData = new string[] { };
                PersistentContainer.Instance.PollOld = new Dictionary<string[], string>();
                PersistentContainer.Instance.PollOpen = false;
                PersistentContainer.Instance.PollVote = new Dictionary<string, bool>();
            }
            if (Shop_Log)
            {
                PersistentContainer.Instance.ShopLog = new List<string[]>();
            }
            List<string> id = PersistentContainer.Instance.Players.IDs;
            for (int i = 0; i < id.Count; i++)
            {
                if (Auction)
                {
                    PersistentContainer.Instance.Players[id[i]].Auction = new Dictionary<int, ItemDataSerializable>();
                    PersistentContainer.Instance.Players[id[i]].AuctionReturn = new Dictionary<int, ItemDataSerializable>();
                }
                if (Bank)
                {
                    PersistentContainer.Instance.Players[id[i]].Bank = 0;
                }
                if (Bounties)
                {
                    PersistentContainer.Instance.Players[id[i]].Bounty = 0;
                    PersistentContainer.Instance.Players[id[i]].BountyHunter = 0;
                }
                if (Chunk_Reset)
                {
                    PersistentContainer.Instance.ChunkReset = new Dictionary<string, DateTime>();
                }
                if (Player_Delays)
                {
                    PersistentContainer.Instance.Players[id[i]].LastAnimal = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastDied = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastFriendTele = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastGamble = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastGimme = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastHome = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastJoined = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastKillMe = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastLobby = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastLog = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastMarket = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastNameColorChange = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastPrayer = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastPrefixColorChange = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastScout = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastStuck = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastTravel = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastVehicle = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastVote = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastVoteWeek = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].LastWaypoint = new DateTime();
                }
                if (Homes)
                {
                    PersistentContainer.Instance.Players[id[i]].Homes = new Dictionary<string, string>();
                }
                if (Jail)
                {
                    PersistentContainer.Instance.Players[id[i]].JailDate = new DateTime();
                    PersistentContainer.Instance.Players[id[i]].JailName = string.Empty;
                    PersistentContainer.Instance.Players[id[i]].JailTime = 0;
                }
                if (Lobby)
                {
                    PersistentContainer.Instance.Players[id[i]].LobbyReturnPos = string.Empty;
                }
                if (Market)
                {
                    PersistentContainer.Instance.Players[id[i]].MarketReturnPos = string.Empty;
                }
                if (Protected_Zones)
                {
                    //ProtectedZones.DisableProtection();
                    PersistentContainer.Instance.ProtectedZones = new List<string>();
                }
                if (New_Spawn_Tele)
                {
                    PersistentContainer.Instance.Players[id[i]].NewSpawn = false;
                    PersistentContainer.Instance.Players[id[i]].NewSpawnPosition = string.Empty;
                }
                if (Region_Reset)
                {
                    PersistentContainer.Instance.RegionReset = new Dictionary<string, DateTime>();
                }
                if (Waypoints)
                {
                    PersistentContainer.Instance.Players[id[i]].Waypoints = new Dictionary<string, string>();
                }
            }
        }

        public static void ClearFirstClaims()
        {
            List<string> id = PersistentContainer.Instance.Players.IDs;
            for (int i = 0; i < id.Count; i++)
            {
                PersistentContainer.Instance.Players[id[i]].FirstClaimBlock = false;
            }
        }
    }
}

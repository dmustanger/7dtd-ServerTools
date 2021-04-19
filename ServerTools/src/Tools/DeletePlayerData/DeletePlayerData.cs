using System;
using System.Collections.Generic;

namespace ServerTools
{

    class DeletePlayerData
    {

        public static void Exec()
        {
            if (GameUtils.WorldTimeToMinutes(GameManager.Instance.World.GetWorldTime()) == 0)
            {
                if (PersistentContainer.Instance.Players.SteamIDs != null && PersistentContainer.Instance.Players.SteamIDs.Count > 0)
                {
                    List<string> playerlist = PersistentContainer.Instance.Players.SteamIDs;
                    for (int i = 0; i < playerlist.Count; i++)
                    {
                        string _steamId = playerlist[i];
                        PersistentPlayer _p = PersistentContainer.Instance.Players[_steamId];
                        if (_p != null)
                        {
                            _p.Auction = new Dictionary<int, ItemDataSerializable>();
                            _p.AuctionReturn = new Dictionary<int, ItemDataSerializable>();
                            _p.Bank = 0;
                            _p.BikeId = 0;
                            _p.Bounty = 0;
                            _p.BountyHunter = 0;
                            _p.ClanInvite = "";
                            _p.ClanName = "";
                            _p.ClanOfficer = false;
                            _p.ClanOwner = false;
                            _p.ClanRequestToJoin = new Dictionary<string, string>();
                            _p.CountryBanImmune = false;
                            _p.CustomCommandDelays = new Dictionary<string, DateTime>();
                            _p.FirstClaimBlock = false;
                            _p.GyroId = 0;
                            _p.HardcoreEnabled = false;
                            _p.HardcoreStats = new string[0];
                            _p.HighPingImmune = false;
                            _p.HighPingImmuneName = "";
                            _p.Homes = new Dictionary<string, string>();
                            _p.JailDate = new DateTime();
                            _p.JailName = "";
                            _p.JailTime = 0;
                            _p.JeepId = 0;
                            _p.LastAnimal = new DateTime();
                            _p.LastBike = new DateTime();
                            _p.LastDied = new DateTime();
                            _p.LastFriendTele = new DateTime();
                            _p.LastGimme = new DateTime();
                            _p.LastGyro = new DateTime();
                            _p.LastHome = new DateTime();
                            _p.LastJeep = new DateTime();
                            _p.LastJoined = new DateTime();
                            _p.LastKillMe = new DateTime();
                            _p.LastLobby = new DateTime();
                            _p.LastLog = new DateTime();
                            _p.LastMarket = new DateTime();
                            _p.LastMiniBike = new DateTime();
                            _p.LastMotorBike = new DateTime();
                            _p.LastStuck = new DateTime();
                            _p.LastTravel = new DateTime();
                            _p.LastVote = new DateTime();
                            _p.LastVoteWeek = new DateTime();
                            _p.LastWhisper = "";
                            _p.LobbyReturnPos = "";
                            _p.MarketReturnPos = "";
                            _p.MiniBikeId = 0;
                            _p.MotorBikeId = 0;
                            _p.MuteDate = new DateTime();
                            _p.MuteName = "";
                            _p.MuteTime = 0;
                            _p.NewSpawn = false;
                            _p.NewSpawnPosition = "";
                            _p.OldPlayer = false;
                            _p.PlayerName = "";
                            _p.PlayerWallet = 0;
                            _p.SessionTime = 0;
                            _p.StartingItems = false;
                            _p.TotalTimePlayed = 0;
                            _p.VoteWeekCount = 0;
                            _p.WP = "";
                            _p.ZoneDeathTime = new DateTime();
                        }
                    }
                    PersistentContainer.Instance.AuctionPrices = new Dictionary<int, int>();
                    PersistentContainer.Instance.ClientMuteList = new Dictionary<int, List<int>>();
                    PersistentContainer.Instance.LastWeather = new DateTime();
                    PersistentContainer.Instance.PollData = new string[0];
                    PersistentContainer.Instance.PollOld = new Dictionary<string[], string>();
                    PersistentContainer.Instance.PollOpen = false;
                    PersistentContainer.Instance.PollVote = new Dictionary<string, bool>();
                    PersistentContainer.Instance.Track = new List<string[]>();
                    Log.Out(string.Format("[SERVERTOOLS] Detected a new or unplayed map and old save data. Old data has been cleared from ServerTools"));
                }
            }
        }
    }
}

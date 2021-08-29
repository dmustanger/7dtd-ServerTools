using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    public class ResetPlayerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Reset a players profile. * WARNING * Can not be undone without a backup.";
        }

        public override string GetHelp()
        {
            return "Usage: st-rpp <steamId/entityId>";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ResetPlayerProfile", "rpp", "st-rpp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (_cInfo != null)
                {
                    Phrases.Dict.TryGetValue("ResetPlayer1", out string _phrase);
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase), null);
                    GameManager.Instance.World.aiDirector.RemoveEntity(PersistentOperations.GetEntityPlayer(_cInfo.playerId));
                    GC.Collect();
                    MemoryPools.Cleanup();
                    ResetProfileExec(_cInfo.playerId);
                }
                else if (_params[0].Length == 17)
                {
                    if (PersistentOperations.GetPersistentPlayerDataFromSteamId(_params[0]) != null)
                    {
                        GC.Collect();
                        MemoryPools.Cleanup();
                        ResetProfileExec(_params[0]);
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Unable to find player data for id: {0}", _params[0]));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player id {0} is not a valid length. Offline players require using their 17 digit steam id", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayerConsole.Execute: {0}", e.Message));
            }
        }

        public static void ResetProfileExec(string _id)
        {
            try
            {
                RemovePersistentData(_id);
                RemoveProfile(_id);
                RemovePlayerData(_id);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayerConsole.ResetProfileExec: {0}", e.Message));
            }
        }

        public static void RemovePersistentData(string _id)
        {
            PersistentOperations.RemoveAllClaims(_id);
            PersistentOperations.RemovePersistentPlayerData(_id);
            PersistentOperations.RemoveAllACL(_id);
        }

        public static void RemoveProfile(string _id)
        {
            try
            {
                string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _id);
                string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _id);
                if (!File.Exists(_filepath))
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Could not find file {0}.map", _id));
                }
                else
                {
                    File.Delete(_filepath);
                }
                if (!File.Exists(_filepath1))
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Could not find file {0}.ttp", _id));
                }
                else
                {
                    File.Delete(_filepath1);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayerConsole.RemoveProfile: {0}", e.Message));
            }
        }

        public static void RemovePlayerData(string _id)
        {
            try
            {
                PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                if (p != null)
                {
                    p.Auction = new Dictionary<int, ItemDataSerializable>();
                    p.AuctionReturn = new Dictionary<int, ItemDataSerializable>();
                    p.Bank = 0;
                    p.Bounty = 0;
                    p.BountyHunter = 0;
                    p.ClanInvite = "";
                    p.ClanName = "";
                    p.ClanOfficer = false;
                    p.ClanOwner = false;
                    p.ClanRequestToJoin = new Dictionary<string, string>();
                    p.CountryBanImmune = false;
                    p.CustomCommandDelays = new Dictionary<string, DateTime>();
                    p.FirstClaimBlock = false;
                    p.HardcoreEnabled = false;
                    p.HardcoreSavedStats = new List<string[]>();
                    p.HardcoreStats = new string[0];
                    p.HighPingImmune = false;
                    p.HighPingImmuneName = "";
                    p.Homes = new Dictionary<string, string>();
                    p.JailDate = new DateTime();
                    p.JailName = "";
                    p.JailTime = 0;
                    p.LastAnimal = new DateTime();
                    p.LastDied = new DateTime();
                    p.LastFriendTele = new DateTime();
                    p.LastGimme = new DateTime();
                    p.LastHome = new DateTime();
                    p.LastJoined = new DateTime();
                    p.LastKillMe = new DateTime();
                    p.LastLobby = new DateTime();
                    p.LastLog = new DateTime();
                    p.LastMarket = new DateTime();
                    p.LastStuck = new DateTime();
                    p.LastTravel = new DateTime();
                    p.LastVote = new DateTime();
                    p.LastVoteWeek = new DateTime();
                    p.LastWhisper = "";
                    p.LobbyReturnPos = "";
                    p.MarketReturnPos = "";
                    p.MuteDate = new DateTime();
                    p.MuteName = "";
                    p.MuteTime = 0;
                    p.NewSpawn = false;
                    p.NewSpawnPosition = "";
                    p.OldPlayer = false;
                    p.PlayerName = "";
                    p.PlayerWallet = 0;
                    p.SessionTime = 0;
                    p.StartingItems = false;
                    p.TotalTimePlayed = 0;
                    p.Vehicles = new Dictionary<int, string[]>();
                    p.VoteWeekCount = 0;
                    p.Waypoints = new Dictionary<string, string>();
                    p.WebPass = "";
                    p.ZoneDeathTime = new DateTime();
                    PersistentContainer.DataChange = true;
                }
                Phrases.Dict.TryGetValue("ResetPlayer2", out string _phrase);
                _phrase = _phrase.Replace("{SteamId}", _id);
                SdtdConsole.Instance.Output(string.Format("{0}", _phrase));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayerConsole.RemovePlayerData: {0}", e.Message));
            }
        }
    }
}
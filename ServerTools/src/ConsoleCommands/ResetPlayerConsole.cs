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
            return "Usage: st-rpp <EOS/EntityId/PlayerName>";
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
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                if (cInfo != null)
                {
                    Phrases.Dict.TryGetValue("ResetPlayer1", out string phrase);
                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", cInfo.CrossplatformId.CombinedString, phrase), null);
                    GameManager.Instance.World.aiDirector.RemoveEntity(GeneralOperations.GetEntityPlayer(cInfo.entityId));
                    GC.Collect();
                    MemoryPools.Cleanup();
                    ResetProfileExec(cInfo.CrossplatformId.CombinedString);
                }
                else if (_params[0].Contains("_"))
                {
                    PersistentPlayerData ppd = GeneralOperations.GetPersistentPlayerDataFromId(_params[0]);
                    if (ppd != null)
                    {
                        GC.Collect();
                        MemoryPools.Cleanup();
                        ResetProfileExec(ppd.UserIdentifier.CombinedString);
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Unable to find player data for id '{0}'", _params[0]));
                    }
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Unable to find player data for id '{0}'", _params[0]));
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
            GeneralOperations.RemoveAllClaims(_id);
            GeneralOperations.RemovePersistentPlayerData(_id);
            GeneralOperations.RemoveAllACL(_id);
        }

        public static void RemoveProfile(string _id)
        {
            try
            {
                string filepath = string.Format("{0}/Player/{1}.map", GameIO.GetSaveGameDir(), _id);
                if (!File.Exists(filepath))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Could not find file '{0}' for player profile reset", filepath));
                    Log.Out(string.Format("[SERVERTOOLS] Could not find file '{0}' for player profile reset", filepath));
                }
                else
                {
                    File.Delete(filepath);
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] File '{0}' deleted for player profile reset", filepath));
                    Log.Out(string.Format("[SERVERTOOLS] File '{0}' deleted for player profile reset", filepath));
                }
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removing .ttp file in two seconds"));
                Timers.ResetPlayerProfileDelayTimer(_id);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayerConsole.RemoveProfile: {0}", e.Message));
            }
        }

        public static void DelayedProfileDeletion(string _id)
        {
            try
            {
                string filepath = string.Format("{0}/Player/{1}.ttp", GameIO.GetSaveGameDir(), _id);
                if (!File.Exists(filepath))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Could not find file '{0}' for player profile reset", filepath));
                    Log.Out(string.Format("[SERVERTOOLS] Could not find file '{0}' for player profile reset", filepath));
                }
                else
                {
                    File.Delete(filepath);
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] File '{0}' deleted for player profile reset", filepath));
                    Log.Out(string.Format("[SERVERTOOLS] File '{0}' deleted for player profile reset", filepath));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayerConsole.DelayedProfileDeletion: {0}", e.Message));
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
                    p.AutoPartyInvite = new List<string[]>();
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
                    p.EventOver = false;
                    p.EventReturnPosition = "";
                    p.EventSpawn = false;
                    p.Events = new List<List<string>>();
                    p.FamilyShareImmune = false;
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
                    p.LastNameColorChange = new DateTime();
                    p.LastPrayer = new DateTime();
                    p.LastPrefixColorChange = new DateTime();
                    p.LastScout = new DateTime();
                    p.LastStuck = new DateTime();
                    p.LastTravel = new DateTime();
                    p.LastVehicle = new DateTime();
                    p.LastVote = new DateTime();
                    p.LastVoteWeek = new DateTime();
                    p.LastWaypoint = new DateTime();
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
                    p.ProxyBanImmune = false;
                    p.SessionTime = 0;
                    p.StartingItems = false;
                    p.TotalTimePlayed = 0;
                    p.VoteWeekCount = 0;
                    p.Waypoints = new Dictionary<string, string>();
                    p.WebPass = "";
                    p.ZoneDeathTime = new DateTime();
                    PersistentContainer.DataChange = true;
                }
                Phrases.Dict.TryGetValue("ResetPlayer2", out string phrase);
                phrase = phrase.Replace("{Id}", _id);
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("{0}", phrase));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayerConsole.RemovePlayerData: {0}", e.Message));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class Hardcore
    {
        public static bool IsEnabled = false, Optional = true;
        public static int Max_Deaths = 9, Max_Extra_Lives = 3, Life_Price = 2000;
        public static string Command_top3 = "top3", Command_score = "score", Command_buy_life = "buy life", Command_hardcore = "hardcore", Command_hardcore_on = "hardcore on";

        public static List<string> NoEntry = new List<string>();

        public static void Check(ClientInfo _cInfo, EntityPlayer _player, bool _addDeath)
        {
            string[] stats = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreStats;
            if (int.TryParse(stats[1], out int deaths))
            {
                if (int.TryParse(stats[2], out int extraLives))
                {
                    int lives;
                    if (_addDeath)
                    {
                        stats[1] = (deaths + 1).ToString();
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreStats = stats;
                        PersistentContainer.DataChange = true;
                        lives = Max_Deaths - (deaths + 1) + extraLives;
                    }
                    else
                    {
                        lives = Max_Deaths - deaths + extraLives;
                    }
                    if (lives > 0)
                    {
                        Phrases.Dict.TryGetValue("Hardcore5", out string phrase);
                        phrase = phrase.Replace("{Value}", lives.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        EndGame(_cInfo, _player, stats);
                    }
                }
            }
        }

        public static void EndGame(ClientInfo _cInfo, EntityPlayer _player, string[] _stats)
        {
            PersistentPlayer p = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString];
            if (p != null)
            {
                GeneralOperations.Session.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime time);
                TimeSpan varTime = DateTime.Now - time;
                double fractionalMinutes = varTime.TotalMinutes;
                int timepassed = (int)fractionalMinutes;
                int finalSession = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].SessionTime += timepassed;
                int.TryParse(_stats[2], out int extraLives);
                string[] newStats = { _cInfo.playerName, finalSession.ToString(), _player.KilledPlayers.ToString(), _player.KilledZombies.ToString(), Max_Deaths + extraLives.ToString(), _player.Score.ToString() };
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats.Count > 0)
                {
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats.Add(newStats);
                }
                else
                {
                    List<string[]> SavedStats = new List<string[]>();
                    SavedStats.Add(newStats);
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats = SavedStats;
                }
                p.Auction = new Dictionary<int, ItemDataSerializable>();
                p.AuctionCount = 0;
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
                p.ExperienceBoost = 0;
                p.FamilyShareImmune = false;
                p.FirstClaimBlock = false;
                p.HardcoreEnabled = false;
                p.HardcoreStats = new string[0];
                p.HighPingImmune = false;
                p.HighPingImmuneName = "";
                p.Homes = new Dictionary<string, string>();
                p.HomeSpots = 0;
                p.JailDate = new DateTime();
                p.JailName = "";
                p.JailTime = 0;
                p.JailRelease = false;
                p.LastAnimal = new DateTime();
                p.LastBed = new DateTime();
                p.LastDied = new DateTime();
                p.LastFriendTele = new DateTime();
                p.LastGamble = new DateTime();
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
                p.CustomReturnPositions = new Dictionary<string, string>();
                p.SessionTime = 0;
                p.StartingItems = false;
                p.TotalTimePlayed = 0;
                p.VoteWeekCount = 0;
                p.Waypoints = new Dictionary<string, string>();
                p.WaypointSpots = 0;
                p.WebPass = "";
                p.ZoneDeathTime = new DateTime();
                PersistentContainer.DataChange = true;
                Disconnect(_cInfo, newStats);
            }
        }

        public static void TopThree(ClientInfo _cInfo)
        {
            int topSession1 = 0, topSession2 = 0, topSession3 = 0, topScore1 = 0, topScore2 = 0, topScore3 = 0;
            string sessionName1 = "", sessionName2 = "", sessionName3 = "", scoreName1 = "", scoreName2 = "", scoreName3 = "";
            List<string> persistentPlayers = PersistentContainer.Instance.Players.IDs;
            for (int i = 0; i < persistentPlayers.Count; i++)
            {
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats.Count > 0)
                {
                    List<string[]> hardcoreSavedStats = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats;
                    for (int j = 0; j < hardcoreSavedStats.Count; j++)
                    {
                        string[] stats = hardcoreSavedStats[j];
                        int.TryParse(stats[1], out int sessionTime);
                        if (sessionTime > topSession1)
                        {
                            topSession3 = topSession2;
                            sessionName3 = sessionName2;
                            topSession2 = topSession1;
                            sessionName2 = sessionName1;
                            topSession1 = sessionTime;
                            sessionName1 = stats[0];
                        }
                        else if (sessionTime > topSession2)
                        {
                            topSession3 = topSession2;
                            sessionName3 = sessionName2;
                            topSession2 = sessionTime;
                            sessionName2 = stats[0];
                        }
                        else if (sessionTime > topSession3)
                        {
                            topSession3 = sessionTime;
                            sessionName3 = stats[0];
                        }
                        int.TryParse(stats[4], out int score);
                        if (score > topScore1)
                        {
                            topScore3 = topScore2;
                            scoreName3 = scoreName2;
                            topScore2 = topScore1;
                            scoreName2 = scoreName1;
                            topScore1 = score;
                            scoreName1 = stats[0];
                        }
                        else if (score > topScore2)
                        {
                            topScore3 = topScore2;
                            scoreName3 = scoreName2;
                            topScore2 = score;
                            scoreName2 = stats[0];
                        }
                        else if (score > topScore3)
                        {
                            topScore3 = score;
                            scoreName3 = stats[0];
                        }
                    }
                }
            }
            if (sessionName1 != "" || scoreName1 != null)
            {
                Phrases.Dict.TryGetValue("Hardcore1", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                Phrases.Dict.TryGetValue("Hardcore2", out phrase);
                phrase = phrase.Replace("{Name1}", sessionName1);
                phrase = phrase.Replace("{Session1}", topSession1.ToString());
                phrase = phrase.Replace("{Name2}", sessionName2);
                phrase = phrase.Replace("{Session2}", topSession2.ToString());
                phrase = phrase.Replace("{Name3}", sessionName3);
                phrase = phrase.Replace("{Session3}", topSession3.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                Phrases.Dict.TryGetValue("Hardcore3", out phrase);
                phrase = phrase.Replace("{Name1}", scoreName1);
                phrase = phrase.Replace("{Score1}", topScore1.ToString());
                phrase = phrase.Replace("{Name2}", scoreName2);
                phrase = phrase.Replace("{Score2}", topScore2.ToString());
                phrase = phrase.Replace("{Name3}", scoreName3);
                phrase = phrase.Replace("{Score3}", topScore3.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Hardcore6", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Score(ClientInfo _cInfo)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats.Count > 0)
                {
                    List<string[]> hardcoreSavedStats = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreSavedStats;
                    for (int i = 0; i < hardcoreSavedStats.Count; i++)
                    {
                        string[] stats = hardcoreSavedStats[i];
                        Phrases.Dict.TryGetValue("Hardcore4", out string phrase);
                        phrase = phrase.Replace("{PlayerName}", stats[0]);
                        phrase = phrase.Replace("{PlayTime}", stats[1]);
                        phrase = phrase.Replace("{PlayerKills}", stats[2]);
                        phrase = phrase.Replace("{ZombieKills}", stats[3]);
                        phrase = phrase.Replace("{Deaths}", stats[4]);
                        phrase = phrase.Replace("{Score}", stats[5]);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Hardcore6", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.Score: {0}", e.Message));
            }
        }

        public static void BuyLife(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (Max_Extra_Lives > 0)
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreEnabled)
                        {
                            string[] stats = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreStats;
                            int.TryParse(stats[2], out int extraLives);
                            if (extraLives < Max_Extra_Lives)
                            {
                                if (Life_Price < 0)
                                {
                                    Life_Price = 0;
                                }
                                int currency = 0, bankCurrency = 0, cost = Life_Price * (extraLives + 1);
                                if (Wallet.IsEnabled)
                                {
                                    currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                                }
                                if (Bank.IsEnabled && Bank.Direct_Payment)
                                {
                                    bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
                                }
                                if (currency + bankCurrency >= cost)
                                {
                                    if (currency > 0)
                                    {
                                        if (currency < cost)
                                        {
                                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                                            cost -= currency;
                                            Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                                        }
                                        else
                                        {
                                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                                        }
                                    }
                                    else
                                    {
                                        Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                                    }
                                    stats[2] = (extraLives + 1).ToString();
                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreStats = stats;
                                    PersistentContainer.DataChange = true;
                                    Phrases.Dict.TryGetValue("Hardcore7", out string phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Hardcore8", out string phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Hardcore9", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Hardcore10", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Hardcore9", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.BuyLife: {0}", e.Message));
            }
        }

        public static void ResetHardcoreProfile(ClientInfo _cInfo)
        {
            try
            {
                string filepath1 = string.Format("{0}/Player/{1}.map", GameIO.GetSaveGameDir(), _cInfo.CrossplatformId.CombinedString);
                string filepath2 = string.Format("{0}/Player/{1}.ttp", GameIO.GetSaveGameDir(), _cInfo.CrossplatformId.CombinedString);
                if (!File.Exists(filepath1))
                {
                    SdtdConsole.Instance.Output(string.Format("Could not find file {0}.map", _cInfo.CrossplatformId.CombinedString));
                }
                else
                {
                    File.Delete(filepath1);
                }
                if (!File.Exists(filepath2))
                {
                    SdtdConsole.Instance.Output(string.Format("Could not find file {0}.ttp", _cInfo.CrossplatformId.CombinedString));
                }
                else
                {
                    File.Delete(filepath2);
                }
                NoEntry.Remove(_cInfo.CrossplatformId.CombinedString);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.ResetHardcoreProfile: {0}", e.Message));
            }
        }

        public static void Disconnect(ClientInfo _cInfo, string[] _stats)
        {
            try
            {
                if (_cInfo != null)
                {
                    Phrases.Dict.TryGetValue("Hardcore4", out string phrase);
                    phrase = phrase.Replace("{PlayerName}", _stats[0]);
                    phrase = phrase.Replace("{PlayTime}", _stats[1]);
                    phrase = phrase.Replace("{PlayerKills}", _stats[2]);
                    phrase = phrase.Replace("{ZombieKills}", _stats[3]);
                    phrase = phrase.Replace("{Deaths}", _stats[4]);
                    phrase = phrase.Replace("{Score}", _stats[5]);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    Timers.DisconnectHardcorePlayer(_cInfo);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.Disconnect: {0}", e.Message));
            }
        }

        public static void KickPlayer(ClientInfo _cInfo)
        {
            try
            {
                NoEntry.Add(_cInfo.CrossplatformId.CombinedString);
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    Phrases.Dict.TryGetValue("Hardcore14", out string phrase);
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} {1}", _cInfo.CrossplatformId.CombinedString, phrase), null);
                }
                GeneralOperations.SavePersistentPlayerDataXML();
                GeneralOperations.RemovePersistentPlayerData(_cInfo.CrossplatformId.CombinedString);
                //GeneralOperations.RemoveAllClaims(_cInfo.CrossplatformId.CombinedString);
                //GeneralOperations.RemoveAllACL(_cInfo.CrossplatformId.CombinedString);
                Timers.HardcoreDeleteFiles(_cInfo);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Hardcore.KickPlayer: {0}", e.Message));
            }
        }
    }
}

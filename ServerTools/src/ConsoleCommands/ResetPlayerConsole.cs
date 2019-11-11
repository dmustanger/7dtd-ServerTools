﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ServerTools
{
    public class ResetPlayerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Reset A Players Profile. * WARNING * Can Not be undone without a backup. You must reload the ServerTools.db and ServerTools.bin file to recover home, wallet and more.";
        }

        public override string GetHelp()
        {
            return "Usage: resetplayerprofile <steamId/entityId>";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ResetPlayerProfile", "resetplayerprofile", "st-rpp", "rpp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (_cInfo != null)
                {
                    string _phrase400;
                    if (!Phrases.Dict.TryGetValue(400, out _phrase400))
                    {
                        _phrase400 = "Reseting players profile.";
                    }
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase400), _cInfo);
                    string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                    string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                    if (!File.Exists(_filepath))
                    {
                        SdtdConsole.Instance.Output(string.Format("Could not find file {0}.map", _params[0]));
                    }
                    else
                    {
                        File.Delete(_filepath);
                    }
                    if (!File.Exists(_filepath1))
                    {
                        SdtdConsole.Instance.Output(string.Format("Could not find file {0}.ttp", _params[0]));
                    }
                    else
                    {
                        File.Delete(_filepath1);
                    }
                    string _sql = string.Format("SELECT steamid FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count != 0)
                    {
                        _sql = string.Format("Delete FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        SQL.FastQuery(_sql, "ResetPlayerConsole");
                    }
                    _result.Dispose();
                    _sql = string.Format("SELECT steamid FROM Waypoints WHERE steamid = '{0}'", _cInfo.playerId);
                    _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count != 0)
                    {
                        _sql = string.Format("Delete FROM Waypoints WHERE steamid = '{0}'", _cInfo.playerId);
                        SQL.FastQuery(_sql, "ResetPlayerConsole");
                    }
                    _result.Dispose();
                    _sql = string.Format("SELECT steamid FROM Hardcore WHERE steamid = '{0}'", _cInfo.playerId);
                    _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count != 0)
                    {
                        _sql = string.Format("Delete FROM Hardcore WHERE steamid = '{0}'", _cInfo.playerId);
                        SQL.FastQuery(_sql, "ResetPlayerConsole");
                    }
                    _result.Dispose();
                    _sql = string.Format("SELECT steamid FROM Tracking WHERE steamid = '{0}'", _cInfo.playerId);
                    _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count != 0)
                    {
                        _sql = string.Format("Delete FROM Tracking WHERE steamid = '{0}'", _cInfo.playerId);
                        SQL.FastQuery(_sql, "ResetPlayerConsole");
                    }
                    _result.Dispose();
                    PersistentPlayer p = PersistentContainer.Instance.Players[_cInfo.playerId];
                    p.AuctionCancelTime = new DateTime();
                    p.AuctionId = 0;
                    p.AuctionItemCount = 0;
                    p.AuctionItemName = "";
                    p.AuctionItemPrice = 0;
                    p.AuctionItemQuality = 0;
                    p.AuctionReturn = false;
                    p.AuctionSellDate = new DateTime();
                    p.Bank = 0;
                    p.BikeId = 0;
                    p.Bounty = 0;
                    p.BountyHunter = 0;
                    p.ClanInvite = "";
                    p.ClanName = "";
                    p.ClanOfficer = false;
                    p.ClanOwner = false;
                    p.ClanRequestToJoin = new List<string[]>();
                    p.CountryBanImmune = false;
                    p.CustomCommand1 = new DateTime();
                    p.CustomCommand2 = new DateTime();
                    p.CustomCommand3 = new DateTime();
                    p.CustomCommand4 = new DateTime();
                    p.CustomCommand5 = new DateTime();
                    p.CustomCommand6 = new DateTime();
                    p.CustomCommand7 = new DateTime();
                    p.CustomCommand8 = new DateTime();
                    p.CustomCommand9 = new DateTime();
                    p.CustomCommand10 = new DateTime();
                    p.CustomCommand11 = new DateTime();
                    p.CustomCommand12 = new DateTime();
                    p.CustomCommand13 = new DateTime();
                    p.CustomCommand14 = new DateTime();
                    p.CustomCommand15 = new DateTime();
                    p.CustomCommand16 = new DateTime();
                    p.CustomCommand17 = new DateTime();
                    p.CustomCommand18 = new DateTime();
                    p.CustomCommand19 = new DateTime();
                    p.CustomCommand20 = new DateTime();
                    p.FirstClaimBlock = false;
                    p.GyroId = 0;
                    p.HardcoreExtraLives = 0;
                    p.HighPingImmune = false;
                    p.HighPingImmuneName = "";
                    p.HomePosition1 = "";
                    p.HomePosition2 = "";
                    p.JailDate = new DateTime();
                    p.JailName = "";
                    p.JailTime = 0;
                    p.JeepId = 0;
                    p.LastAnimal = new DateTime();
                    p.LastBike = new DateTime();
                    p.LastDied = new DateTime();
                    p.LastFriendTele = new DateTime();
                    p.LastGimme = new DateTime();
                    p.LastGyro = new DateTime();
                    p.LastHome1 = new DateTime();
                    p.LastHome2 = new DateTime();
                    p.LastJeep = new DateTime();
                    p.LastJoined = new DateTime();
                    p.LastKillMe = new DateTime();
                    p.LastLobby = new DateTime();
                    p.LastLog = new DateTime();
                    p.LastMarket = new DateTime();
                    p.LastMiniBike = new DateTime();
                    p.LastMotorBike = new DateTime();
                    p.LastStuck = new DateTime();
                    p.LastTravel = new DateTime();
                    p.LastVote = new DateTime();
                    p.LastVoteWeek = new DateTime();
                    p.LastWhisper = "";
                    p.LobbyReturnPos = "";
                    p.MarketReturnPos = "";
                    p.MiniBikeId = 0;
                    p.MotorBikeId = 0;
                    p.MuteDate = new DateTime();
                    p.MuteName = "";
                    p.MuteTime = 0;
                    p.NewSpawn = false;
                    p.NewSpawnPosition = "";
                    p.PlayerName = "";
                    p.PlayerWallet = 0;
                    p.SessionTime = 0;
                    p.StartingItems = false;
                    p.TotalTimePlayed = 0;
                    p.VoteWeekCount = 0;
                    p.ZoneDeathTime = new DateTime();
                    PersistentContainer.Instance.Save();
                    string _phrase401;
                    if (!Phrases.Dict.TryGetValue(401, out _phrase401))
                    {
                        _phrase401 = "You have reset the profile for Player {SteamId}.";
                    }
                    _phrase401 = _phrase401.Replace("{SteamId}", _params[0]);
                    SdtdConsole.Instance.Output(string.Format("{0}", _phrase401));
                }
                else if (_params[0].Length == 17)
                {
                    int _id;
                    if (int.TryParse(_params[0], out _id))
                    {
                        string _phrase400;
                        if (!Phrases.Dict.TryGetValue(400, out _phrase400))
                        {
                            _phrase400 = "Reseting players profile.";
                        }
                        string _steamid = SQL.EscapeString(_params[0]);
                        string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _steamid);
                        string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _steamid);
                        string _sql = string.Format("SELECT steamid FROM Players WHERE steamid = '{0}'", _steamid);
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count != 0)
                        {
                            _sql = string.Format("SELECT steamid FROM Players WHERE steamid = '{0}'", _params[0]);
                            _result = SQL.TQuery(_sql);
                            if (_result.Rows.Count != 0)
                            {
                                _sql = string.Format("Delete FROM Players WHERE steamid = '{0}'", _params[0]);
                                SQL.FastQuery(_sql, "ResetPlayerConsole");
                            }
                            _result.Dispose();
                            _sql = string.Format("SELECT steamid FROM Waypoints WHERE steamid = '{0}'", _params[0]);
                            _result = SQL.TQuery(_sql);
                            if (_result.Rows.Count != 0)
                            {
                                _sql = string.Format("Delete FROM Waypoints WHERE steamid = '{0}'", _params[0]);
                                SQL.FastQuery(_sql, "ResetPlayerConsole");
                            }
                            _result.Dispose();
                            _sql = string.Format("SELECT steamid FROM Hardcore WHERE steamid = '{0}'", _params[0]);
                            _result = SQL.TQuery(_sql);
                            if (_result.Rows.Count != 0)
                            {
                                _sql = string.Format("Delete FROM Hardcore WHERE steamid = '{0}'", _params[0]);
                                SQL.FastQuery(_sql, "ResetPlayerConsole");
                            }
                            _result.Dispose();
                            _sql = string.Format("SELECT steamid FROM Tracking WHERE steamid = '{0}'", _params[0]);
                            _result = SQL.TQuery(_sql);
                            if (_result.Rows.Count != 0)
                            {
                                _sql = string.Format("Delete FROM Tracking WHERE steamid = '{0}'", _params[0]);
                                SQL.FastQuery(_sql, "ResetPlayerConsole");
                            }
                            _result.Dispose();
                        }
                        if (!File.Exists(_filepath))
                        {
                            SdtdConsole.Instance.Output(string.Format("Could not find file {0}.map", _params[0]));
                        }
                        else
                        {
                            File.Delete(_filepath);
                        }
                        if (!File.Exists(_filepath1))
                        {
                            SdtdConsole.Instance.Output(string.Format("Could not find file {0}.ttp", _params[0]));
                        }
                        else
                        {
                            File.Delete(_filepath1);
                        }
                        _result.Dispose();
                        PersistentPlayer p = PersistentContainer.Instance.Players[_params[0]];
                        p.AuctionCancelTime = new DateTime();
                        p.AuctionId = 0;
                        p.AuctionItemCount = 0;
                        p.AuctionItemName = "";
                        p.AuctionItemPrice = 0;
                        p.AuctionItemQuality = 0;
                        p.AuctionReturn = false;
                        p.AuctionSellDate = new DateTime();
                        p.Bank = 0;
                        p.BikeId = 0;
                        p.Bounty = 0;
                        p.BountyHunter = 0;
                        p.ClanInvite = "";
                        p.ClanName = "";
                        p.ClanOfficer = false;
                        p.ClanOwner = false;
                        p.ClanRequestToJoin = new List<string[]>();
                        p.CountryBanImmune = false;
                        p.CustomCommand1 = new DateTime();
                        p.CustomCommand2 = new DateTime();
                        p.CustomCommand3 = new DateTime();
                        p.CustomCommand4 = new DateTime();
                        p.CustomCommand5 = new DateTime();
                        p.CustomCommand6 = new DateTime();
                        p.CustomCommand7 = new DateTime();
                        p.CustomCommand8 = new DateTime();
                        p.CustomCommand9 = new DateTime();
                        p.CustomCommand10 = new DateTime();
                        p.CustomCommand11 = new DateTime();
                        p.CustomCommand12 = new DateTime();
                        p.CustomCommand13 = new DateTime();
                        p.CustomCommand14 = new DateTime();
                        p.CustomCommand15 = new DateTime();
                        p.CustomCommand16 = new DateTime();
                        p.CustomCommand17 = new DateTime();
                        p.CustomCommand18 = new DateTime();
                        p.CustomCommand19 = new DateTime();
                        p.CustomCommand20 = new DateTime();
                        p.FirstClaimBlock = false;
                        p.GyroId = 0;
                        p.HardcoreExtraLives = 0;
                        p.HighPingImmune = false;
                        p.HighPingImmuneName = "";
                        p.HomePosition1 = "";
                        p.HomePosition2 = "";
                        p.JailDate = new DateTime();
                        p.JailName = "";
                        p.JailTime = 0;
                        p.JeepId = 0;
                        p.LastAnimal = new DateTime();
                        p.LastBike = new DateTime();
                        p.LastDied = new DateTime();
                        p.LastFriendTele = new DateTime();
                        p.LastGimme = new DateTime();
                        p.LastGyro = new DateTime();
                        p.LastHome1 = new DateTime();
                        p.LastHome2 = new DateTime();
                        p.LastJeep = new DateTime();
                        p.LastJoined = new DateTime();
                        p.LastKillMe = new DateTime();
                        p.LastLobby = new DateTime();
                        p.LastLog = new DateTime();
                        p.LastMarket = new DateTime();
                        p.LastMiniBike = new DateTime();
                        p.LastMotorBike = new DateTime();
                        p.LastStuck = new DateTime();
                        p.LastTravel = new DateTime();
                        p.LastVote = new DateTime();
                        p.LastVoteWeek = new DateTime();
                        p.LastWhisper = "";
                        p.LobbyReturnPos = "";
                        p.MarketReturnPos = "";
                        p.MiniBikeId = 0;
                        p.MotorBikeId = 0;
                        p.MuteDate = new DateTime();
                        p.MuteName = "";
                        p.MuteTime = 0;
                        p.NewSpawn = false;
                        p.NewSpawnPosition = "";
                        p.PlayerName = "";
                        p.PlayerWallet = 0;
                        p.SessionTime = 0;
                        p.StartingItems = false;
                        p.TotalTimePlayed = 0;
                        p.VoteWeekCount = 0;
                        p.ZoneDeathTime = new DateTime();
                        PersistentContainer.Instance.Save();
                        string _phrase401;
                        if (!Phrases.Dict.TryGetValue(401, out _phrase401))
                        {
                            _phrase401 = "You have reset the profile for Player {SteamId}.";
                        }
                        _phrase401 = _phrase401.Replace("{SteamId}", _params[0]);
                        SdtdConsole.Instance.Output(string.Format("{0}", _phrase401));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player id {0} is not a valid integer", _params[0]));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Player id {0} is not a valid length. Offline players require using their 17 digit steam id", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayerConsole.Run: {0}.", e));
            }
        }
    }
}
using System;
using System.Data;
using System.IO;

//can delete this file a few months after release

namespace ServerTools
{
    public class UpdateToSqlFromBin
    {
        public static void Exec()
        {
            foreach (string _id in PersistentContainer.Instance.Players.SteamIDs)
            {
                string _sql = string.Format("SELECT steamid FROM Players WHERE steamid = '{0}'", _id);
                DataTable _result = SQL.TQuery(_sql);
                if (_result.Rows.Count == 0)
                {
                    Player p = PersistentContainer.Instance.Players[_id, false];
                    DateTime _last_gimme;
                    DateTime _lastkillme;
                    int _bank = p.Bank;
                    int _wallet = p.Wallet;
                    int _playerSpentCoins = p.PlayerSpentCoins;
                    int _hardcoreSessionTime = p.HardcoreSessionTime;
                    int _hardcoreKills = p.HardcoreKills;
                    int _hardcoreZKills = p.HardcoreZKills;
                    int _hardcoreScore = p.HardcoreScore;
                    int _hardcoreDeaths = p.HardcoreDeaths;
                    string _hardcoreName = "Unknown";
                    int _bounty = p.Bounty;
                    int _bountyHunter = p.BountyHunter;
                    int _sessionTime = p.SessionTime;
                    int _bikeId = p.BikeId;
                    DateTime _lastBike;
                    int _jailTime = p.JailTime;
                    string _jailName = "Unknown";
                    DateTime _jailDate;
                    int _muteTime = p.MuteTime;
                    string _muteName = "Unknown";
                    DateTime _muteDate;
                    int _zkills = p.ZKills;
                    int _kills = p.Kills;
                    int _deaths = p.Deaths;
                    string _eventReturn = "Unknown";
                    string _marketReturn = "Unknown";
                    string _lobbyReturn = "Unknown";
                    string _newTeleSpawn = "Unknown";
                    string _homeposition = "Unknown";
                    string _homeposition2 = "Unknown";
                    DateTime _lastsethome;
                    string _lastwhisper = "Unknown";
                    DateTime _lastWaypoint;
                    DateTime _lastMarket;
                    DateTime _lastStuck;
                    DateTime _lastLobby;
                    DateTime _lastLog;
                    DateTime _lastDied;
                    DateTime _lastFriendTele;
                    DateTime _respawnTime;
                    DateTime _lastTravel;
                    DateTime _lastAnimals;
                    DateTime _lastVoteReward;
                    string _firstClaim = "false";
                    string _ismuted = "false";
                    string _isjailed = "false";
                    string _startingItems = "false";
                    string _clanname = "Unknown";
                    string _invitedtoclan = "Unknown";
                    string _isclanowner = "false";
                    string _isclanofficer = "false";
                    DateTime _customCommand1;
                    DateTime _customCommand2;
                    DateTime _customCommand3;
                    DateTime _customCommand4;
                    DateTime _customCommand5;
                    DateTime _customCommand6;
                    DateTime _customCommand7;
                    DateTime _customCommand8;
                    DateTime _customCommand9;
                    DateTime _customCommand10;
                    if (p.LastGimme != null)
                    {
                        _last_gimme = p.LastGimme;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _last_gimme);
                    }
                    if (p.LastKillme != null)
                    {
                        _lastkillme = p.LastKillme;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastkillme);
                    }
                    if (p.HardcoreName != null)
                    {
                        _hardcoreName = p.HardcoreName;
                    }
                    if (p.LastBike != null)
                    {
                        _lastBike = p.LastBike;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastBike);
                    }
                    if (p.JailName != null)
                    {
                        _jailName = p.JailName;
                    }
                    if (p.JailDate != null)
                    {
                        _jailDate = p.JailDate;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _jailDate);
                    }
                    if (p.MuteName != null)
                    {
                        _muteName = p.MuteName;
                    }
                    if (p.MuteDate != null)
                    {
                        _muteDate = p.MuteDate;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _muteDate);
                    }
                    if (p.EventReturn != null)
                    {
                        _eventReturn = p.EventReturn;
                    }
                    if (p.MarketReturn != null)
                    {
                        _marketReturn = p.MarketReturn;
                    }
                    if (p.LobbyReturn != null)
                    {
                        _lobbyReturn = p.LobbyReturn;
                    }
                    if (p.NewTeleSpawn != null)
                    {
                        _newTeleSpawn = p.NewTeleSpawn;
                    }
                    if (p.HomePosition != null)
                    {
                        _homeposition = p.HomePosition;
                    }
                    if (p.HomePosition2 != null)
                    {
                        _homeposition2 = p.HomePosition2;
                    }
                    if (p.LastSetHome != null)
                    {
                        _lastsethome = p.LastSetHome;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastsethome);
                    }
                    if (p.LastWhisper != null)
                    {
                        _lastwhisper = p.LastWhisper;
                    }
                    if (p.LastWaypoint != null)
                    {
                        _lastWaypoint = p.LastWaypoint;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastWaypoint);
                    }
                    if (p.LastMarket != null)
                    {
                        _lastMarket = p.LastMarket;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastMarket);
                    }
                    if (p.LastStuck != null)
                    {
                        _lastStuck = p.LastStuck;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastStuck);
                    }
                    if (p.LastLobby != null)
                    {
                        _lastLobby = p.LastLobby;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastLobby);
                    }
                    if (p.Log != null)
                    {
                        _lastLog = p.Log;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastLog);
                    }
                    if (p.LastDied != null)
                    {
                        _lastDied = p.LastDied;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastDied);
                    }
                    if (p.LastFriendTele != null)
                    {
                        _lastFriendTele = p.LastFriendTele;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastFriendTele);
                    }
                    if (p.RespawnTime != null)
                    {
                        _respawnTime = p.RespawnTime;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _respawnTime);
                    }
                    if (p.LastTravel != null)
                    {
                        _lastTravel = p.LastTravel;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastTravel);
                    }
                    if (p.LastAnimals != null)
                    {
                        _lastAnimals = p.LastAnimals;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastAnimals);
                    }
                    if (p.LastVoteReward != null)
                    {
                        _lastVoteReward = p.LastVoteReward;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _lastVoteReward);
                    }
                    if (p.FirstClaim)
                    {
                        _firstClaim = "true";
                    }
                    if (p.IsMuted)
                    {
                        _ismuted = "true";
                    }
                    if (p.IsJailed)
                    {
                        _isjailed = "true";
                    }
                    if (p.StartingItems)
                    {
                        _startingItems = "true";
                    }
                    if (p.ClanName != null)
                    {
                        _clanname = p.ClanName;
                    }
                    if (p.InvitedToClan != null)
                    {
                        _invitedtoclan = p.InvitedToClan;
                    }
                    if (p.IsClanOwner)
                    {
                        _isclanowner = "true";
                    }
                    if (p.IsClanOfficer)
                    {
                        _isclanofficer = "true";
                    }
                    if (p.CustomCommand1 != null)
                    {
                        _customCommand1 = p.CustomCommand1;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _customCommand1);
                    }
                    if (p.CustomCommand2 != null)
                    {
                        _customCommand2 = p.CustomCommand2;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _customCommand2);
                    }
                    if (p.CustomCommand3 != null)
                    {
                        _customCommand3 = p.CustomCommand3;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _customCommand3);
                    }
                    if (p.CustomCommand4 != null)
                    {
                        _customCommand4 = p.CustomCommand4;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _customCommand4);
                    }
                    if (p.CustomCommand5 != null)
                    {
                        _customCommand5 = p.CustomCommand5;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _customCommand5);
                    }
                    if (p.CustomCommand6 != null)
                    {
                        _customCommand6 = p.CustomCommand6;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _customCommand6);
                    }
                    if (p.CustomCommand7 != null)
                    {
                        _customCommand7 = p.CustomCommand7;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _customCommand7);
                    }
                    if (p.CustomCommand8 != null)
                    {
                        _customCommand8 = p.CustomCommand8;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _customCommand8);
                    }
                    if (p.CustomCommand9 != null)
                    {
                        _customCommand9 = p.CustomCommand9;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _customCommand9);
                    }
                    if (p.CustomCommand10 != null)
                    {
                        _customCommand10 = p.CustomCommand10;
                    }
                    else
                    {
                        DateTime.TryParse("10/29/2000 7:30:00 AM", out _customCommand10);
                    }
                    _hardcoreName = SQL.EscapeString(_hardcoreName);
                    _jailName = SQL.EscapeString(_jailName);
                    _muteName = SQL.EscapeString(_muteName);
                    _clanname = SQL.EscapeString(_clanname);
                    _sql = string.Format("INSERT INTO Players (steamid, last_gimme, lastkillme, bank, wallet, playerSpentCoins, hardcoreSessionTime, hardcoreKills, hardcoreZKills, hardcoreScore, hardcoreDeaths, hardcoreName, bounty, bountyHunter, sessionTime, bikeId, lastBike, jailTime, jailName, jailDate, muteTime, muteName, muteDate, zkills, kills, deaths, eventReturn, marketReturn, lobbyReturn, newTeleSpawn, homeposition, homeposition2, lastsethome, lastwhisper, lastWaypoint, lastMarket, lastStuck, lastLobby, lastLog, lastDied, lastFriendTele, respawnTime, lastTravel, lastAnimals, lastVoteReward, firstClaim, ismuted, isjailed, startingItems, clanname, invitedtoclan, isclanowner, isclanofficer, customCommand1, customCommand2, customCommand3, customCommand4, customCommand5, customCommand6, customCommand7, customCommand8, customCommand9, customCommand10) VALUES ('{0}', '{1}', '{2}', {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, '{11}', {12}, {13}, {14}, {15}, '{16}', {17}, '{18}', '{19}', {20}, '{21}', '{22}', {23}, {24}, {25}, '{26}', '{27}', '{28}', '{29}', '{30}', '{31}', '{32}', '{33}', '{34}', '{35}', '{36}', '{37}', '{38}', '{39}', '{40}', '{41}', '{42}', '{43}', '{44}', '{45}', '{46}', '{47}', '{48}', '{49}', '{50}', '{51}', '{52}', '{53}', '{54}', '{55}', '{56}', '{57}', '{58}', '{59}', '{60}', '{61}', '{62}')", _id, _last_gimme, _lastkillme, _bank, _wallet, _playerSpentCoins, _hardcoreSessionTime, _hardcoreKills, _hardcoreZKills, _hardcoreScore, _hardcoreDeaths, _hardcoreName, _bounty, _bountyHunter, _sessionTime, _bikeId, _lastBike, _jailTime, _jailName, _jailDate, _muteTime, _muteName, _muteDate, _zkills, _kills, _deaths, _eventReturn, _marketReturn, _lobbyReturn, _newTeleSpawn, _homeposition, _homeposition2, _lastsethome, _lastwhisper, _lastWaypoint, _lastMarket, _lastStuck, _lastLobby, _lastLog, _lastDied, _lastFriendTele, _respawnTime, _lastTravel, _lastAnimals, _lastVoteReward, _firstClaim, _ismuted, _isjailed, _startingItems, _clanname, _invitedtoclan, _isclanowner, _isclanofficer, _customCommand1, _customCommand2, _customCommand3, _customCommand4, _customCommand5, _customCommand6, _customCommand7, _customCommand8, _customCommand9, _customCommand10);
                    SQL.FastQuery(_sql);
                    if (p.AuctionItem != null)
                    {
                        string _itemName = p.AuctionItem[1];
                        int _itemCount;
                        int _itemQuality;
                        int _itemPrice;
                        DateTime _cancelTime = p.CancelTime;
                        int.TryParse(p.AuctionItem[0], out _itemCount);
                        int.TryParse(p.AuctionItem[2], out _itemQuality);
                        int.TryParse(p.AuctionItem[3], out _itemPrice);
                        _sql = string.Format("INSERT INTO Auction (steamid, itemName, itemCount, itemQuality, itemPrice, cancelTime) VALUES ('{0}', '{1}', {2}, {3}, {4}, '{5}')", _id, _itemName, _itemCount, _itemQuality, _itemPrice, _cancelTime);
                        SQL.FastQuery(_sql);
                    }
                }
                _result.Dispose();
            }
            string _binpath = string.Format("{0}/ServerTools.bin", GameUtils.GetSaveGameDir());
            File.Delete(_binpath);
            LoadProcess.Load(4);
        }
    }
}

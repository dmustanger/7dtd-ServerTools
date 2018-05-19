using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    public class ResetPlayer : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Reset a players profile. Warning, can not be undone without a backup.";
        }
        public override string GetHelp()
        {
            return "Usage: resetplayerprofile <steamId/entityId>";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-ResetPlayerProfile", "resetplayerprofile", "rpp" };
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
                if (_params[0].Length < 1 || _params[0].Length > 17)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not reset Id: Invalid Id {0}", _params[0]));
                    return;
                }
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (_cInfo != null)
                {
                    string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                    string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _cInfo.playerId);
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    if (p != null)
                    {
                        string _phrase400;
                        if (!Phrases.Dict.TryGetValue(400, out _phrase400))
                        {
                            _phrase400 = "Reseting players profile.";
                        }
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase400), _cInfo);
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
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].BikeId = 0;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = 0;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].SessionTime = 0;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].AuctionData = 0;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].StartingItems = false;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].FirstClaim = false;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOwner = false;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOfficer = false;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].IsMuted = false;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].IsJailed = false;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastStuck = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastLobby = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastStuck = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CancelTime = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].SellDate = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].Log = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].JailDate = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].MuteDate = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastBike = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastBackpack = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastStuck = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastFriendTele = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand1 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand2 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand3 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand4 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand5 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand6 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand7 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand8 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand9 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand10 = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].RespawnTime = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastTravel = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastAnimals = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastVoteReward = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastGimme = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastKillme = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastSetHome = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].RespawnTime = DateTime.Now.AddDays(-5);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LobbyReturn = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].NewTeleSpawn = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].JailName = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].MuteName = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition2 = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].ClanName = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].InvitedToClan = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastWhisper = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerName = null;
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
                        SdtdConsole.Instance.Output(string.Format("Player file {0}.ttp does not exist", _params[0]));
                    }
                }
                else
                {
                    int _value = 0;
                    if (int.TryParse(_params[0], out _value))
                    {
                        string _filepath = string.Format("{0}/Player/{1}.map", GameUtils.GetSaveGameDir(), _value.ToString());
                        string _filepath1 = string.Format("{0}/Player/{1}.ttp", GameUtils.GetSaveGameDir(), _value.ToString());
                        Player p = PersistentContainer.Instance.Players[_value.ToString(), false];
                        if (p != null)
                        {
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
                            PersistentContainer.Instance.Players[_value.ToString(), true].BikeId = 0;
                            PersistentContainer.Instance.Players[_value.ToString(), true].PlayerSpentCoins = 0;
                            PersistentContainer.Instance.Players[_value.ToString(), true].SessionTime = 0;
                            PersistentContainer.Instance.Players[_value.ToString(), true].AuctionData = 0;
                            PersistentContainer.Instance.Players[_value.ToString(), true].StartingItems = false;
                            PersistentContainer.Instance.Players[_value.ToString(), true].FirstClaim = false;
                            PersistentContainer.Instance.Players[_value.ToString(), true].IsClanOwner = false;
                            PersistentContainer.Instance.Players[_value.ToString(), true].IsClanOfficer = false;
                            PersistentContainer.Instance.Players[_value.ToString(), true].IsMuted = false;
                            PersistentContainer.Instance.Players[_value.ToString(), true].IsJailed = false;
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastStuck = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastLobby = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastStuck = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CancelTime = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].SellDate = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].Log = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].JailDate = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].MuteDate = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastBike = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastBackpack = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastStuck = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastFriendTele = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand1 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand2 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand3 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand4 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand5 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand6 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand7 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand8 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand9 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].CustomCommand10 = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].RespawnTime = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastTravel = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastAnimals = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastVoteReward = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastGimme = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastKillme = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastSetHome = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].RespawnTime = DateTime.Now.AddDays(-5);
                            PersistentContainer.Instance.Players[_value.ToString(), true].LobbyReturn = null;
                            PersistentContainer.Instance.Players[_value.ToString(), true].NewTeleSpawn = null;
                            PersistentContainer.Instance.Players[_value.ToString(), true].JailName = null;
                            PersistentContainer.Instance.Players[_value.ToString(), true].MuteName = null;
                            PersistentContainer.Instance.Players[_value.ToString(), true].HomePosition = null;
                            PersistentContainer.Instance.Players[_value.ToString(), true].HomePosition2 = null;
                            PersistentContainer.Instance.Players[_value.ToString(), true].ClanName = null;
                            PersistentContainer.Instance.Players[_value.ToString(), true].InvitedToClan = null;
                            PersistentContainer.Instance.Players[_value.ToString(), true].LastWhisper = null;
                            PersistentContainer.Instance.Players[_value.ToString(), true].PlayerName = null;
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
                            SdtdConsole.Instance.Output(string.Format("Player file {0}.ttp does not exist", _params[0]));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player id {0} is not a valid integer", _params[0]));
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetPlayer.Run: {0}.", e));
            }
        }
    }
}
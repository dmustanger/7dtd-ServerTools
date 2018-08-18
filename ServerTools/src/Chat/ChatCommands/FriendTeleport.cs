using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class FriendTeleport
    {
        public static bool IsEnabled = false, PvP_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static Dictionary<int, int> Dict = new Dictionary<int, int>();
        public static Dictionary<int, DateTime> Dict1 = new Dictionary<int, DateTime>();
        public static Dictionary<int, int> Count = new Dictionary<int, int>();

        public static void ListFriends(ClientInfo _cInfo, string _message)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _player = _cInfoList[i];
                EntityPlayer ent1 = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                EntityPlayer ent2 = GameManager.Instance.World.Players.dict[_player.entityId];
                if (ent1.IsFriendsWith(ent2))
                {
                    string _phrase625;
                    if (!Phrases.Dict.TryGetValue(625, out _phrase625))
                    {
                        _phrase625 = "Your friend {FriendName} with Id # {EntityId} is online.";
                    }
                    _phrase625 = _phrase625.Replace("{FriendName}", _player.playerName);
                    _phrase625 = _phrase625.Replace("{EntityId}", _player.entityId.ToString());
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase625), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    if (!Count.ContainsKey(_cInfo.entityId))
                    {
                        Count.Add(_cInfo.entityId, 1);
                    }
                }
            }
        }

        public static void CheckDelay(ClientInfo _cInfo, string _message, bool _announce)
        {
            bool _donator = false;
            string _sql = string.Format("SELECT lastFriendTele FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            DateTime _lastFriendTele;
            DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastFriendTele);
            _result.Dispose();
            if (Delay_Between_Uses < 1 || _lastFriendTele.ToString() == "10/29/2000 7:30:00 AM")
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo, _message);
                }
                else
                {
                    MessageFriend(_cInfo, _message);
                }
            }
            else
            {
                TimeSpan varTime = DateTime.Now - _lastFriendTele;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                {
                    if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                    {
                        DateTime _dt;
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                        if (DateTime.Now < _dt)
                        {
                            _donator = true;
                            int _newDelay = Delay_Between_Uses / 2;
                            if (_timepassed >= _newDelay)
                            {
                                if (Wallet.IsEnabled && Command_Cost >= 1)
                                {
                                    CommandCost(_cInfo, _message);
                                }
                                else
                                {
                                    MessageFriend(_cInfo, _message);
                                }
                            }
                            else
                            {
                                int _timeleft = _newDelay - _timepassed;
                                string _phrase630;
                                if (!Phrases.Dict.TryGetValue(630, out _phrase630))
                                {
                                    _phrase630 = "{PlayerName} you can only teleport to a friend once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                }
                                _phrase630 = _phrase630.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase630 = _phrase630.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                _phrase630 = _phrase630.Replace("{TimeRemaining}", _timeleft.ToString());
                                if (_announce)
                                {
                                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase630), Config.Server_Response_Name, false, "", false);
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase630), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                        }
                    }
                }
                if (!_donator)
                {
                    if (_timepassed >= Delay_Between_Uses)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            CommandCost(_cInfo, _message);
                        }
                        else
                        {
                            MessageFriend(_cInfo, _message);
                        }
                    }
                    else
                    {
                        int _timeleft = Delay_Between_Uses - _timepassed;
                        string _phrase630;
                        if (!Phrases.Dict.TryGetValue(630, out _phrase630))
                        {
                            _phrase630 = "{PlayerName} you can only teleport to a friend once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                        }
                        _phrase630 = _phrase630.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase630 = _phrase630.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                        _phrase630 = _phrase630.Replace("{TimeRemaining}", _timeleft.ToString());
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase630), Config.Server_Response_Name, false, "", false);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase630), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
            }
        }

        public static void CommandCost(ClientInfo _cInfo, string _message)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (Command_Cost >= 1)
            {
                if (_currentCoins >= Command_Cost)
                {
                    MessageFriend(_cInfo, _message);
                }
                else
                {
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = "{PlayerName} you do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase814), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                MessageFriend(_cInfo, _message);
            }
        }

        public static void MessageFriend(ClientInfo _cInfo, string _message)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (PvP_Check)
            {
                if (Teleportation.PCheck(_cInfo, _player))
                {
                    return;
                }
            }
            if (Zombie_Check)
            {
                if (Teleportation.ZCheck(_cInfo, _player))
                {
                    return;
                }
            }
            int _Id;
            if (!int.TryParse(_message, out _Id))
            {
                string _phrase626;
                if (!Phrases.Dict.TryGetValue(626, out _phrase626))
                {
                    _phrase626 = "This {EntityId} is not valid. Only integers accepted.";
                }
                _phrase626 = _phrase626.Replace("{EntityId}", _Id.ToString());
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase626), Config.Server_Response_Name, false, "ServerTools", false));
                return;
            }
            ClientInfo _cInfo3 = ConnectionManager.Instance.GetClientInfoForEntityId(_Id);
            if (_cInfo3 != null)
            {
                string _phrase627;
                if (!Phrases.Dict.TryGetValue(627, out _phrase627))
                {
                    _phrase627 = "Sent your friend {PlayerName} a teleport request.";
                }
                _phrase627 = _phrase627.Replace("{PlayerName}", _cInfo3.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase627), Config.Server_Response_Name, false, "ServerTools", false));
                string _phrase628;
                if (!Phrases.Dict.TryGetValue(628, out _phrase628))
                {
                    _phrase628 = "{PlayerName} would like to teleport to you. Type /accept in chat to accept the request.";
                }
                _phrase628 = _phrase628.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo3.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase628), Config.Server_Response_Name, false, "ServerTools", false));
                if (Dict.ContainsKey(_cInfo3.entityId))
                {
                    Dict.Remove(_cInfo3.entityId);
                    Dict1.Remove(_cInfo3.entityId);
                    Dict.Add(_cInfo3.entityId, _cInfo.entityId);
                    Dict1.Add(_cInfo3.entityId, DateTime.Now);
                }
                else
                {
                    Dict.Add(_cInfo3.entityId, _cInfo.entityId);
                    Dict1.Add(_cInfo3.entityId, DateTime.Now);
                }
            }
            else
            {
                string _phrase629;
                if (!Phrases.Dict.TryGetValue(629, out _phrase629))
                {
                    _phrase629 = "Did not find EntityId {EntityId}. No teleport request sent.";
                }
                _phrase629 = _phrase629.Replace("{EntityId}", _Id.ToString());
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase629), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void TeleFriend(ClientInfo _cInfo, int _friendToTele)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForEntityId(_friendToTele);
            if (_cInfo2 != null)
            {
                Players.NoFlight.Add(_cInfo2.entityId);
                _cInfo2.SendPackage(new NetPackageTeleportPlayer(new Vector3((int)_player.position.x, (int)_player.position.y, (int)_player.position.z), false));
                string _sql;
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                _sql = string.Format("UPDATE Players SET lastFriendTele = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo2.playerId);
                SQL.FastQuery(_sql);
                string _phrase631;
                if (!Phrases.Dict.TryGetValue(631, out _phrase631))
                {
                    _phrase631 = "Your request was accepted. Teleporting you to your friend.";
                }
                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase631), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}

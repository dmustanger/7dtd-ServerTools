using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class FriendTeleport
    {
        public static bool IsEnabled = false;
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
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (Delay_Between_Uses < 1 || p == null || p.LastFriendTele == null)
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
                TimeSpan varTime = DateTime.Now - p.LastFriendTele;
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
            World world = GameManager.Instance.World;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            int _playerSpentCoins;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
            _result.Dispose();
            int currentCoins = 0;
            int gameMode = world.GetGameMode();
            if (gameMode == 7)
            {
                currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + _playerSpentCoins;
            }
            else
            {
                currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + _playerSpentCoins;
            }
            if (currentCoins >= Command_Cost)
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

        public static void MessageFriend(ClientInfo _cInfo, string _message)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (TeleportDelay.PvP_Check)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = _cInfoList[i];
                    if (_cInfo2 != null)
                    {
                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                        if (_player2 != null)
                        {
                            Vector3 _pos2 = _player2.GetPosition();
                            if (((int)_player.position.x - (int)_pos2.x) * ((int)_player.position.x - (int)_pos2.x) + ((int)_player.position.z - (int)_pos2.z) * ((int)_player.position.z - (int)_pos2.z) <= 50 * 50)
                            {
                                if (!_player.IsFriendsWith(_player2))
                                {
                                    string _phrase819;
                                    if (!Phrases.Dict.TryGetValue(819, out _phrase819))
                                    {
                                        _phrase819 = "{PlayerName} you are too close to a player that is not a friend. Command unavailable.";
                                    }
                                    _phrase819 = _phrase819.Replace("{PlayerName}", _cInfo.playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase819), Config.Server_Response_Name, false, "ServerTools", false));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (TeleportDelay.Zombie_Check)
            {
                World world = GameManager.Instance.World;
                List<Entity> Entities = world.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    if (_entity != null)
                    {
                        EntityType _type = _entity.entityType;
                        if (_type == EntityType.Zombie)
                        {
                            Vector3 _pos2 = _entity.GetPosition();
                            if (((int)_player.position.x - (int)_pos2.x) * ((int)_player.position.x - (int)_pos2.x) + ((int)_player.position.z - (int)_pos2.z) * ((int)_player.position.z - (int)_pos2.z) <= 20 * 20)
                            {
                                string _phrase820;
                                if (!Phrases.Dict.TryGetValue(820, out _phrase820))
                                {
                                    _phrase820 = "{PlayerName} you are too close to a zombie. Command unavailable.";
                                }
                                _phrase820 = _phrase820.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase820), Config.Server_Response_Name, false, "ServerTools", false));
                                return;
                            }
                        }
                    }
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
                if (!TeleportHome.HomeRequest.ContainsKey(_cInfo3.entityId))
                {
                    string _phrase627;
                    if (!Phrases.Dict.TryGetValue(627, out _phrase627))
                    {
                        _phrase627 = "Sent your friend {FriendsName} a teleport request.";
                    }
                    _phrase627 = _phrase627.Replace("{FriendsName}", _cInfo3.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase627), Config.Server_Response_Name, false, "ServerTools", false));
                    string _phrase628;
                    if (!Phrases.Dict.TryGetValue(628, out _phrase628))
                    {
                        _phrase628 = "{SenderName} would like to teleport to you. Type /accept in chat to accept the request.";
                    }
                    _phrase628 = _phrase628.Replace("{SenderName}", _cInfo.playerName);
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
                    DateTime _time;
                    TeleportHome.HomeRequestTime.TryGetValue(_cInfo3.entityId, out _time);
                    TimeSpan varTime = DateTime.Now - _time;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed <= 5)
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the player is handling a request already. Try again in a few minutes.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        TeleportHome.HomeRequest.Remove(_cInfo3.entityId);
                        TeleportHome.HomeRequestTime.Remove(_cInfo3.entityId);
                        TeleportHome.HomeRequestPos.Remove(_cInfo3.entityId);
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
                        string _phrase627;
                        if (!Phrases.Dict.TryGetValue(627, out _phrase627))
                        {
                            _phrase627 = "Sent your friend {FriendsName} a teleport request.";
                        }
                        _phrase627 = _phrase627.Replace("{FriendsName}", _cInfo3.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase627), Config.Server_Response_Name, false, "ServerTools", false));
                        string _phrase628;
                        if (!Phrases.Dict.TryGetValue(628, out _phrase628))
                        {
                            _phrase628 = "{SenderName} would like to teleport to you. Type /accept in chat to accept the request.";
                        }
                        _phrase628 = _phrase628.Replace("{SenderName}", _cInfo.playerName);
                        _cInfo3.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase628), Config.Server_Response_Name, false, "ServerTools", false));
                    }
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
                TeleportDelay.TeleportQue(_cInfo2, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    int _playerSpentCoins;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
                    _result.Dispose();
                    _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins - Command_Cost, _cInfo.playerId);
                    SQL.FastQuery(_sql);
                }
                PersistentContainer.Instance.Players[_cInfo2.playerId, true].LastFriendTele = DateTime.Now;
                PersistentContainer.Instance.Save();
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

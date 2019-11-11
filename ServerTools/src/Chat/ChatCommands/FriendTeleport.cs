using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class FriendTeleport
    {
        public static bool IsEnabled = false, PvP_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static string Command59 = "friend", Command60 = "accept";
        public static Dictionary<int, int> Dict = new Dictionary<int, int>();
        public static Dictionary<int, DateTime> Dict1 = new Dictionary<int, DateTime>();

        public static void ListFriends(ClientInfo _cInfo, string _message)
        {
            bool _found = false;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            List<EntityPlayer> _playerList = GameManager.Instance.World.Players.list;
            for (int i = 0; i < _playerList.Count; i++)
            {
                EntityPlayer _player2 = _playerList[i];
                if (_player2 != null)
                {
                    if (_player != _player2 && _player.IsFriendsWith(_player2))
                    {
                        _found = true;
                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(_player2.entityId);
                        string _phrase625;
                        if (!Phrases.Dict.TryGetValue(625, out _phrase625))
                        {
                            _phrase625 = "Friend = {FriendName}, Id = {EntityId}.";
                        }
                        _phrase625 = _phrase625.Replace("{FriendName}", _cInfo2.playerName);
                        _phrase625 = _phrase625.Replace("{EntityId}", _cInfo2.entityId.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase625 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            if (!_found)
            {
                string _phrase632;
                if (!Phrases.Dict.TryGetValue(632, out _phrase632))
                {
                    _phrase632 = "No friends found online.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase632 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Checks(ClientInfo _cInfo, string _message)
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
            ClientInfo _friend = ConsoleHelper.ParseParamIdOrName(_message);
            if (_friend != null)
            {
                EntityPlayer _friendPlayer = GameManager.Instance.World.Players.dict[_friend.entityId];
                if (_player.IsFriendsWith(_friendPlayer))
                {
                    DateTime _lastFriendTele = PersistentContainer.Instance.Players[_cInfo.playerId].LastFriendTele;
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
                                int _delay = Delay_Between_Uses / 2;
                                Time(_cInfo, _message, _timepassed, _delay);
                                return;
                            }
                        }
                    }
                    Time(_cInfo, _message, _timepassed, Delay_Between_Uses);
                }
                else
                {
                    string _phrase633;
                    if (!Phrases.Dict.TryGetValue(633, out _phrase633))
                    {
                        _phrase633 = "This player is not your friend. You can not request teleport to them.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase633 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void Time(ClientInfo _cInfo, string _message, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
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
                int _timeleft = _delay - _timepassed;
                string _phrase630;
                if (!Phrases.Dict.TryGetValue(630, out _phrase630))
                {
                    _phrase630 = " you can only teleport to a friend once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase630 = _phrase630.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase630 = _phrase630.Replace("{TimeRemaining}", _timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase630 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo, string _message)
        {
            int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
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
                        _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                MessageFriend(_cInfo, _message);
            }
        }

        public static void MessageFriend(ClientInfo _cInfo, string _message)
        {
            int _Id;
            if (!int.TryParse(_message, out _Id))
            {
                string _phrase626;
                if (!Phrases.Dict.TryGetValue(626, out _phrase626))
                {
                    _phrase626 = " this {EntityId} is not valid. Only integers accepted.";
                }
                _phrase626 = _phrase626.Replace("{EntityId}", _Id.ToString());
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase626 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            ClientInfo _cInfo3 = ConnectionManager.Instance.Clients.ForEntityId(_Id);
            if (_cInfo3 != null)
            {
                string _phrase627;
                if (!Phrases.Dict.TryGetValue(627, out _phrase627))
                {
                    _phrase627 = " sent your friend {PlayerName} a teleport request.";
                }
                _phrase627 = _phrase627.Replace("{PlayerName}", _cInfo3.playerName);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase627 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                string _phrase628;
                if (!Phrases.Dict.TryGetValue(628, out _phrase628))
                {
                    _phrase628 = " {PlayerName} would like to teleport to you. Type {CommandPrivate}{Command60} in chat to accept the request.";
                }
                _phrase628 = _phrase628.Replace("{PlayerName}", _cInfo.playerName);
                _phrase628 = _phrase628.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase628 = _phrase628.Replace("{Command60}", Command60);
                ChatHook.ChatMessage(_cInfo3, ChatHook.Player_Name_Color + _cInfo3.playerName + LoadConfig.Chat_Response_Color + _phrase628 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    _phrase629 = " did not find EntityId {EntityId}. No teleport request sent.";
                }
                _phrase629 = _phrase629.Replace("{EntityId}", _Id.ToString());
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase629 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void TeleFriend(ClientInfo _cInfo, int _friendToTele)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(_friendToTele);
            if (_cInfo2 != null)
            {
                _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3((int)_player.position.x, (int)_player.position.y, (int)_player.position.z), null, false));
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                PersistentContainer.Instance.Players[_cInfo.playerId].LastFriendTele = DateTime.Now;
                PersistentContainer.Instance.Save();
                string _phrase631;
                if (!Phrases.Dict.TryGetValue(631, out _phrase631))
                {
                    _phrase631 = " your request was accepted. Teleporting you to your friend.";
                }
                ChatHook.ChatMessage(_cInfo2, ChatHook.Player_Name_Color + _cInfo2.playerName + LoadConfig.Chat_Response_Color + _phrase631 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

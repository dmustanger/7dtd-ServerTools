using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class FriendTeleport
    {
        public static bool IsEnabled = false, Player_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static string Command_friend = "friend", Command_accept = "accept";
        public static Dictionary<int, int> Dict = new Dictionary<int, int>();
        public static Dictionary<int, DateTime> Dict1 = new Dictionary<int, DateTime>();

        public static void ListFriends(ClientInfo _cInfo)
        {
            EntityPlayer _player = PersistentOperations.GetEntity(_cInfo.entityId) as EntityPlayer;
            if (_player != null)
            {
                bool found = false;
                List<EntityPlayer> _playerList = GameManager.Instance.World.Players.list;
                for (int i = 0; i < _playerList.Count; i++)
                {
                    EntityPlayer _player2 = _playerList[i];
                    if (_player2 != null)
                    {
                        if (_player != _player2 && _player.IsFriendsWith(_player2))
                        {
                            ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromEntityId(_player2.entityId);
                            if (_cInfo2 != null)
                            {
                                found = true;
                                Phrases.Dict.TryGetValue("FriendTeleport1", out string _phrase);
                                _phrase = _phrase.Replace("{FriendName}", _cInfo2.playerName);
                                _phrase = _phrase.Replace("{EntityId}", _cInfo2.entityId.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                if (!found)
                {
                    Phrases.Dict.TryGetValue("FriendTeleport8", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void Exec(ClientInfo _cInfo, string _message)
        {
            EntityPlayer _player = PersistentOperations.GetEntity(_cInfo.entityId) as EntityPlayer;
            if (Player_Check)
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
                EntityPlayer _friendPlayer = PersistentOperations.GetEntityPlayer(_friend.playerId);
                if (_friendPlayer != null)
                {
                    if (!_player.IsFriendsWith(_friendPlayer))
                    {
                        Phrases.Dict.TryGetValue("FriendTeleport9", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    DateTime _lastFriendTele = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].LastFriendTele != null)
                    {
                        _lastFriendTele = PersistentContainer.Instance.Players[_cInfo.playerId].LastFriendTele;
                    }
                    TimeSpan varTime = DateTime.Now - _lastFriendTele;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                        {
                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                            if (DateTime.Now < _dt)
                            {
                                int _delay = Delay_Between_Uses / 2;
                                Delay(_cInfo, _friend, _timepassed, _delay);
                                return;
                            }
                        }
                    }
                    Delay(_cInfo, _friend, _timepassed, Delay_Between_Uses);
                    return;
                }
            }
            Phrases.Dict.TryGetValue("FriendTeleport11", out string _phrase1);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void Delay(ClientInfo _cInfo, ClientInfo _friend, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo, _friend);
                }
                else
                {
                    MessageFriend(_cInfo, _friend);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue("FriendTeleport6", out string _phrase);
                _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase = _phrase.Replace("{TimeRemaining}", _timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo, ClientInfo _friend)
        {
            if (Command_Cost >= 1)
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                {
                    MessageFriend(_cInfo, _friend);
                }
                else
                {
                    Phrases.Dict.TryGetValue("FriendTeleport10", out string _phrase);
                    _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                MessageFriend(_cInfo, _friend);
            }
        }

        public static void MessageFriend(ClientInfo _cInfo, ClientInfo _friend)
        {
            if (Dict.ContainsKey(_friend.entityId))
            {
                Dict.Remove(_friend.entityId);
                Dict1.Remove(_friend.entityId);
                Dict.Add(_friend.entityId, _cInfo.entityId);
                Dict1.Add(_friend.entityId, DateTime.Now);
            }
            else
            {
                Dict.Add(_friend.entityId, _cInfo.entityId);
                Dict1.Add(_friend.entityId, DateTime.Now);
            }
            Phrases.Dict.TryGetValue("FriendTeleport3", out string _phrase);
            _phrase = _phrase.Replace("{PlayerName}", _friend.playerName);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Phrases.Dict.TryGetValue("FriendTeleport4", out _phrase);
            _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
            _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
            _phrase = _phrase.Replace("{Command_friend}", Command_friend);
            ChatHook.ChatMessage(_friend, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void TeleFriend(ClientInfo _cInfo, int _invitingFriend)
        {
            ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(_invitingFriend);
            if (_cInfo2 != null)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        if (Wallet.GetCurrentCoins(_cInfo2.playerId) >= Command_Cost)
                        {
                            Wallet.SubtractCoinsFromWallet(_cInfo2.playerId, Command_Cost);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("FriendTeleport10", out string _phrase);
                            _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                            ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                    Phrases.Dict.TryGetValue("FriendTeleport7", out string _phrase1);
                    ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3((int)_player.position.x, (int)_player.position.y, (int)_player.position.z), null, false));
                    PersistentContainer.Instance.Players[_cInfo2.playerId].LastFriendTele = DateTime.Now;
                    PersistentContainer.DataChange = true;
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("FriendTeleport11", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

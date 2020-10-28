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

        public static void ListFriends(ClientInfo _cInfo)
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
                        Phrases.Dict.TryGetValue(361, out string _phrase361);
                        _phrase361 = _phrase361.Replace("{FriendName}", _cInfo2.playerName);
                        _phrase361 = _phrase361.Replace("{EntityId}", _cInfo2.entityId.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase361 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            if (!_found)
            {
                Phrases.Dict.TryGetValue(368, out string _phrase368);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase368 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _message)
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
                            DateTime _dt;
                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                            if (DateTime.Now < _dt)
                            {
                                int _delay = Delay_Between_Uses / 2;
                                Time(_cInfo, _friend, _timepassed, _delay);
                                return;
                            }
                        }
                    }
                    Time(_cInfo, _friend, _timepassed, Delay_Between_Uses);
                }
                else
                {
                    Phrases.Dict.TryGetValue(369, out string _phrase369);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase369 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void Time(ClientInfo _cInfo, ClientInfo _friend, int _timepassed, int _delay)
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
                Phrases.Dict.TryGetValue(366, out string _phrase366);
                _phrase366 = _phrase366.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase366 = _phrase366.Replace("{TimeRemaining}", _timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase366 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo, ClientInfo _friend)
        {
            int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
            if (Command_Cost >= 1)
            {
                if (_currentCoins >= Command_Cost)
                {
                    MessageFriend(_cInfo, _friend);
                }
                else
                {
                    Phrases.Dict.TryGetValue(370, out string _phrase370);
                    _phrase370 = _phrase370.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase370 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
            Phrases.Dict.TryGetValue(363, out string _phrase363);
            _phrase363 = _phrase363.Replace("{PlayerName}", _friend.playerName);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase363 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            Phrases.Dict.TryGetValue(364, out string _phrase364);
            _phrase364 = _phrase364.Replace("{PlayerName}", _cInfo.playerName);
            _phrase364 = _phrase364.Replace("{CommandPrivate}", ChatHook.Command_Private);
            _phrase364 = _phrase364.Replace("{Command60}", Command60);
            ChatHook.ChatMessage(_friend, LoadConfig.Chat_Response_Color + _phrase364 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void TeleFriend(ClientInfo _cInfo, int _friendToTele)
        {
            ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(_friendToTele);
            if (_cInfo2 != null)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3((int)_player.position.x, (int)_player.position.y, (int)_player.position.z), null, false));
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        Wallet.SubtractCoinsFromWallet(_cInfo2.playerId, Command_Cost);
                    }
                    PersistentContainer.Instance.Players[_cInfo2.playerId].LastFriendTele = DateTime.Now;
                    PersistentContainer.Instance.Save();
                    Phrases.Dict.TryGetValue(367, out string _phrase367);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase367 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);

                }
            }
            else
            {
                Phrases.Dict.TryGetValue(371, out string _phrase371);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase371 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

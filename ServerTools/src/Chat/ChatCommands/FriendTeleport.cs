using System;
using System.Collections.Generic;

namespace ServerTools
{
    class FriendTeleport
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60;
        public static Dictionary<int, int> Dict = new Dictionary<int, int>();
        public static Dictionary<int, DateTime> Dict1 = new Dictionary<int, DateTime>();
        public static Dictionary<int, int> Count = new Dictionary<int, int>();

        public static void ListFriends(ClientInfo _cInfo, string _message)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            foreach (var _player in _cInfoList)
            {
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase625), "Server", false, "", false));
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
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (Delay_Between_Uses < 1 || p == null || p.LastFriendTele == null)
            {
                MessageFriend(_cInfo, _message);
            }
            else
            {
                TimeSpan varTime = DateTime.Now - p.LastFriendTele;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (_timepassed >= Delay_Between_Uses)
                {
                    MessageFriend(_cInfo, _message);
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
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase630), "Server", false, "", false);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase630), "Server", false, "", false));
                    }
                }
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
                    _phrase626 = "This {EntityId} is not valid. Only intergers accepted.";
                }
                _phrase626 = _phrase626.Replace("{EntityId}", _Id.ToString());
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase626), "Server", false, "", false));
            }
            ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForEntityId(_Id);
            if (_cInfo2 != null)
            {
                string _phrase627;
                if (!Phrases.Dict.TryGetValue(627, out _phrase627))
                {
                    _phrase627 = "Sent your friend {FriendsName} a teleport request.";
                }
                _phrase627 = _phrase627.Replace("{FriendsName}", _cInfo2.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase627), "Server", false, "", false));
                string _phrase628;
                if (!Phrases.Dict.TryGetValue(628, out _phrase628))
                {
                    _phrase628 = "{SenderName} would like to teleport to you. Type /accept in chat to accept the request.";
                }
                _phrase628 = _phrase628.Replace("{SenderName}", _cInfo.playerName);
                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase628), "Server", false, "", false));
                if (Dict.ContainsKey(_cInfo2.entityId))
                {
                    Dict.Remove(_cInfo2.entityId);
                    Dict1.Remove(_cInfo2.entityId);
                    Dict.Add(_cInfo2.entityId, _cInfo.entityId);
                    Dict1.Add(_cInfo2.entityId, DateTime.Now);
                }
                else
                {
                    Dict.Add(_cInfo2.entityId, _cInfo.entityId);
                    Dict1.Add(_cInfo2.entityId, DateTime.Now);
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase629), "Server", false, "", false));
            }
        }

        public static void TeleFriend(ClientInfo _cInfo, int _friendToTele)
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1}", _friendToTele, _cInfo.entityId), (ClientInfo)null);           
            ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForEntityId(_friendToTele);
            PersistentContainer.Instance.Players[_cInfo2.playerId, true].LastFriendTele = DateTime.Now;
            PersistentContainer.Instance.Save();
            string _phrase631;
            if (!Phrases.Dict.TryGetValue(631, out _phrase631))
            {
                _phrase631 = "Your request was accepted. Teleported you to your friend.";
            }
            _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase631), "Server", false, "", false));
        }
    }
}

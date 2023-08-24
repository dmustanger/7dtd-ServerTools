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
            EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
            if (player != null)
            {
                bool found = false;
                List<EntityPlayer> playerList = GameManager.Instance.World.Players.list;
                for (int i = 0; i < playerList.Count; i++)
                {
                    EntityPlayer player2 = playerList[i];
                    if (player2 != null)
                    {
                        if (player != player2 && player.IsFriendsWith(player2))
                        {
                            ClientInfo cInfo2 = GeneralOperations.GetClientInfoFromEntityId(player2.entityId);
                            if (cInfo2 != null)
                            {
                                found = true;
                                Phrases.Dict.TryGetValue("FriendTeleport1", out string phrase);
                                phrase = phrase.Replace("{FriendName}", cInfo2.playerName);
                                phrase = phrase.Replace("{EntityId}", cInfo2.entityId.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                if (!found)
                {
                    Phrases.Dict.TryGetValue("FriendTeleport8", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void Exec(ClientInfo _cInfo, string _message)
        {
            EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
            if (player != null)
            {
                if (Player_Check)
                {
                    if (Teleportation.PCheck(_cInfo, player))
                    {
                        return;
                    }
                }
                if (Zombie_Check)
                {
                    if (Teleportation.ZCheck(_cInfo, player))
                    {
                        return;
                    }
                }
                ClientInfo friend = ConsoleHelper.ParseParamIdOrName(_message);
                if (friend != null)
                {
                    EntityPlayer friendPlayer = GeneralOperations.GetEntityPlayer(friend.entityId);
                    if (friendPlayer != null)
                    {
                        if (!player.IsFriendsWith(friendPlayer))
                        {
                            Phrases.Dict.TryGetValue("FriendTeleport9", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        DateTime lastFriendTele = DateTime.Now;
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastFriendTele != null)
                        {
                            lastFriendTele = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastFriendTele;
                        }
                        TimeSpan varTime = DateTime.Now - lastFriendTele;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString)  || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                            {
                                if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        int delay = Delay_Between_Uses / 2;
                                        Delay(_cInfo, friend, _timepassed, delay);
                                        return;
                                    }
                                }
                                else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        int delay = Delay_Between_Uses / 2;
                                        Delay(_cInfo, friend, _timepassed, delay);
                                        return;
                                    }
                                }
                            }
                        }
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                        {
                            int delay = Delay_Between_Uses / 2;
                            Delay(_cInfo, friend, _timepassed, delay);
                            return;
                        }
                        Delay(_cInfo, friend, _timepassed, Delay_Between_Uses);
                        return;
                    }
                }
                Phrases.Dict.TryGetValue("FriendTeleport11", out string phrase1);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Delay(ClientInfo _cInfo, ClientInfo _friend, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Command_Cost >= 1)
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
                int timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue("FriendTeleport6", out string phrase);
                phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo, ClientInfo _friend)
        {
            int currency = 0, bankCurrency = 0, cost = Command_Cost;
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
                MessageFriend(_cInfo, _friend);
            }
            else
            {
                Phrases.Dict.TryGetValue("FriendTeleport10", out string phrase);
                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
            _phrase = _phrase.Replace("{Command_accept}", Command_accept);
            ChatHook.ChatMessage(_friend, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void TeleFriend(ClientInfo _cInfo, int _invitingFriend)
        {
            ClientInfo cInfo2 = GeneralOperations.GetClientInfoFromEntityId(_invitingFriend);
            if (cInfo2 != null)
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    Phrases.Dict.TryGetValue("FriendTeleport7", out string phrase1);
                    ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3((int)player.position.x, (int)player.position.y, (int)player.position.z), null, false));
                    PersistentContainer.Instance.Players[cInfo2.CrossplatformId.CombinedString].LastFriendTele = DateTime.Now;
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

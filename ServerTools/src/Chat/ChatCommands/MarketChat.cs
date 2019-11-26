using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class MarketChat
    {
        public static bool IsEnabled = false, Return = false, PvP_Check = false, Zombie_Check = false, Donor_Only = false;
        public static int Delay_Between_Uses = 5, Market_Size = 25, Command_Cost = 0;
        public static string Command51 = "marketback", Command52 = "mback", Command102 = "setmarket", Command103 = "market";
        public static List<int> MarketPlayers = new List<int>();

        public static void Exec(ClientInfo _cInfo)
        {
            if (Donor_Only && ReservedSlots.IsEnabled && !ReservedSlots.DonorCheck(_cInfo.playerId))
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " this command is locked to donors only" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (Delay_Between_Uses < 1)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    MarketTele(_cInfo);
                }
            }
            else
            {
                DateTime _lastMarket = PersistentContainer.Instance.Players[_cInfo.playerId].LastMarket;
                TimeSpan varTime = DateTime.Now - _lastMarket;
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
                            Time(_cInfo, _timepassed, _delay);
                            return;
                        }
                    }
                }
                Time(_cInfo, _timepassed, Delay_Between_Uses);
            }
        }

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    MarketTele(_cInfo);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                string _phrase560;
                if (!Phrases.Dict.TryGetValue(560, out _phrase560))
                {
                    _phrase560 = " you can only use {CommandPrivate}{Command103} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase560 = _phrase560.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase560 = _phrase560.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase560 = _phrase560.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase560 = _phrase560.Replace("{Command103}", MarketChat.Command103);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase560 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
        {
            if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
            {
                MarketTele(_cInfo);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void MarketTele(ClientInfo _cInfo)
        {
            if (SetMarket.Market_Position != "0,0,0" || SetMarket.Market_Position != "0 0 0" || SetMarket.Market_Position != "")
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
                int x, y, z;
                if (Return)
                {
                    Vector3 _position = _player.GetPosition();
                    x = (int)_position.x;
                    y = (int)_position.y;
                    z = (int)_position.z;
                    string _mposition = x + "," + y + "," + z;
                    MarketPlayers.Add(_cInfo.entityId);
                    PersistentContainer.Instance.Players[_cInfo.playerId].MarketReturnPos = _mposition;
                    string _phrase561;
                    if (!Phrases.Dict.TryGetValue(561, out _phrase561))
                    {
                        _phrase561 = " you can go back by typing {CommandPrivate}{Command51} when you are ready to leave the market.";
                    }
                    _phrase561 = _phrase561.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase561 = _phrase561.Replace("{Command51}", MarketChat.Command51);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase561 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                string[] _cords = { };
                if (SetMarket.Market_Position.Contains(","))
                {
                    if (SetMarket.Market_Position.Contains(" "))
                    {
                        SetMarket.Market_Position.Replace(" ", "");
                    }
                    _cords = SetMarket.Market_Position.Split(',').ToArray();
                }
                else if (SetMarket.Market_Position.Contains(" "))
                {
                    _cords = SetMarket.Market_Position.Split(' ').ToArray();
                }
                int.TryParse(_cords[0], out x);
                int.TryParse(_cords[1], out y);
                int.TryParse(_cords[2], out z);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                string _phrase562;
                if (!Phrases.Dict.TryGetValue(562, out _phrase562))
                {
                    _phrase562 = " sent you to the market.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase562 + "[-]",-1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                PersistentContainer.Instance.Players[_cInfo.playerId].LastMarket = DateTime.Now;
                PersistentContainer.Instance.Save();
            }
            else
            {
                string _phrase563;
                if (!Phrases.Dict.TryGetValue(563, out _phrase563))
                {
                    _phrase563 = " the market position is not set.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase563 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SendBack(ClientInfo _cInfo)
        {
            string _lastPos = PersistentContainer.Instance.Players[_cInfo.playerId].MarketReturnPos;
            if (_lastPos != "")
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int x, y, z;
                string[] _cords = { };
                if (SetMarket.Market_Position.Contains(","))
                {
                    if (SetMarket.Market_Position.Contains(" "))
                    {
                        SetMarket.Market_Position.Replace(" ", "");
                    }
                    _cords = SetMarket.Market_Position.Split(',').ToArray();
                }
                else
                {
                    _cords = SetMarket.Market_Position.Split(' ').ToArray();
                }
                int.TryParse(_cords[0], out x);
                int.TryParse(_cords[1], out y);
                int.TryParse(_cords[2], out z);
                if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= Market_Size * Market_Size)
                {
                    string[] _returnCoords = _lastPos.Split(',');
                    int.TryParse(_returnCoords[0], out x);
                    int.TryParse(_returnCoords[1], out y);
                    int.TryParse(_returnCoords[2], out z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                    MarketPlayers.Remove(_cInfo.entityId);
                    PersistentContainer.Instance.Players[_cInfo.playerId].MarketReturnPos = "";
                    PersistentContainer.Instance.Save();
                    string _phrase555;
                    if (!Phrases.Dict.TryGetValue(555, out _phrase555))
                    {
                        _phrase555 = " sent you back to your saved location.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase555 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _phrase564;
                    if (!Phrases.Dict.TryGetValue(564, out _phrase564))
                    {
                        _phrase564 = " you are outside the market. Get inside it and try again.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase564 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " you have no saved return point.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Market
    {
        public static bool IsEnabled = false, Return = false, Player_Check = false, Zombie_Check = false, Donor_Only = false, PvE = false;
        public static int Delay_Between_Uses = 5, Market_Size = 25, Command_Cost = 0;
        public static string Market_Position = "0,0,0", Command51 = "marketback", Command52 = "mback", Command102 = "setmarket", Command103 = "market";
        public static List<int> MarketPlayers = new List<int>();

        public static void Set(ClientInfo _cInfo)
        {
            string[] _command = { Market.Command102 };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command, _cInfo.playerId))
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "You do not have permissions to use this command.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase107 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                string _mposition = x + "," + y + "," + z;
                Market_Position = _mposition;
                string _phrase565;
                if (!Phrases.Dict.TryGetValue(565, out _phrase565))
                {
                    _phrase565 = "You have set the market position as {MarketPosition}.";
                }
                _phrase565 = _phrase565.Replace("{MarketPosition}", Market_Position);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase565 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                LoadConfig.WriteXml();
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (Donor_Only && ReservedSlots.IsEnabled && !ReservedSlots.ReservedCheck(_cInfo.playerId))
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "This command is locked to donors only" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    _phrase560 = "You can only use {CommandPrivate}{Command103} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase560 = _phrase560.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase560 = _phrase560.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase560 = _phrase560.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase560 = _phrase560.Replace("{Command103}", Market.Command103);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase560 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    _phrase814 = "You do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase814 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void MarketTele(ClientInfo _cInfo)
        {
            if (Market.Market_Position != "0,0,0" || Market.Market_Position != "0 0 0" || Market.Market_Position != "")
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
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
                        _phrase561 = "You can go back by typing {CommandPrivate}{Command51} when you are ready to leave the market.";
                    }
                    _phrase561 = _phrase561.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase561 = _phrase561.Replace("{Command51}", Market.Command51);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase561 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                string[] _cords = { };
                if (Market.Market_Position.Contains(","))
                {
                    if (Market.Market_Position.Contains(" "))
                    {
                        Market.Market_Position.Replace(" ", "");
                    }
                    _cords = Market.Market_Position.Split(',').ToArray();
                }
                else if (Market.Market_Position.Contains(" "))
                {
                    _cords = Market.Market_Position.Split(' ').ToArray();
                }
                int.TryParse(_cords[0], out x);
                int.TryParse(_cords[1], out y);
                int.TryParse(_cords[2], out z);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                string _phrase562;
                if (!Phrases.Dict.TryGetValue(562, out _phrase562))
                {
                    _phrase562 = "Sent you to the market.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase562 + "[-]",-1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    _phrase563 = "The market position is not set.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase563 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SendBack(ClientInfo _cInfo)
        {
            if (MarketPlayers.Contains(_cInfo.entityId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    string _lastPos = PersistentContainer.Instance.Players[_cInfo.playerId].MarketReturnPos;
                    if (_lastPos != "")
                    {
                        int x, y, z;
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
                            _phrase555 = "Sent you back to your saved location.";
                        }
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase555 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have no saved return point.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static bool InsideMarket(float _x, float _z)
        {
            int x, z;
            string[] _cords = Market.Market_Position.Split(',').ToArray();
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[2], out z);
            if ((x - _x) * (x - _x) + (z - _z) * (z - _z) <= Market_Size * Market_Size)
            {
                return true;
            }
            return false;
        }
    }
}

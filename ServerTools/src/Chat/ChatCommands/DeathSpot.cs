using System;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class DeathSpot
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 120, Command_Cost = 0;

        public static void DeathDelay(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo, _announce);
                }
                else
                {
                    TeleportPlayer(_cInfo, _announce);
                }
            }
            else
            {
                string _sql = string.Format("SELECT lastDied FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                DateTime _lastDied;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastDied);
                _result.Dispose();
                if (_lastDied.ToString() == "10/29/2000 7:30:00 AM")
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo, _announce);
                    }
                    else
                    {
                        TeleportPlayer(_cInfo, _announce);
                    }
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - _lastDied;
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
                                        CommandCost(_cInfo, _announce);
                                    }
                                    else
                                    {
                                        TeleportPlayer(_cInfo, _announce);
                                    }
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase735;
                                    if (!Phrases.Dict.TryGetValue(735, out _phrase735))
                                    {
                                        _phrase735 = "you can only use /died once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase735 = _phrase735.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase735 = _phrase735.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase735 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase735 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                CommandCost(_cInfo, _announce);
                            }
                            else
                            {
                                TeleportPlayer(_cInfo, _announce);
                            }
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase735;
                            if (!Phrases.Dict.TryGetValue(735, out _phrase735))
                            {
                                _phrase735 = "you can only use /died once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase735 = _phrase735.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase735 = _phrase735.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase735 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase735 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
        }

        public static void CommandCost(ClientInfo _cInfo, bool _announce)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                TeleportPlayer(_cInfo, _announce);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = "you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void TeleportPlayer(ClientInfo _cInfo, bool _announce)
        {
            if (Players.DeathTime.ContainsKey(_cInfo.entityId))
            {
                DateTime _time;
                if (Players.DeathTime.TryGetValue(_cInfo.entityId, out _time))
                {
                    TimeSpan varTime = DateTime.Now - _time;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                        {
                            DateTime _dt;
                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                            if (DateTime.Now > _dt)
                            {
                                int _newTime = _timepassed / 2;
                                _timepassed = _newTime;
                            }
                        }
                    }
                    if (_timepassed < 2)
                    {
                        string _value;
                        if (Players.LastDeathPos.TryGetValue(_cInfo.entityId, out _value))
                        {
                            Players.NoFlight.Add(_cInfo.entityId);
                            int x, y, z;
                            string[] _cords = _value.Split(',');
                            int.TryParse(_cords[0], out x);
                            int.TryParse(_cords[1], out y);
                            int.TryParse(_cords[2], out z);
                            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                            Players.LastDeathPos.Remove(_cInfo.entityId);
                            string _sql;
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                            }
                            _sql = string.Format("UPDATE Players SET lastDied = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                            SQL.FastQuery(_sql);
                            string _phrase736;
                            if (!Phrases.Dict.TryGetValue(736, out _phrase736))
                            {
                                _phrase736 = "teleporting you to your last death position. You can use this again in {DelayBetweenUses} minutes.";
                            }
                            _phrase736 = _phrase736.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase736 = _phrase736.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            if (_announce)
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase736 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase736 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + " your last death occurred too long ago. Command unavailable.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + " you have no death position. Die first.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

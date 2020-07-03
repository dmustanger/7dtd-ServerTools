using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class DeathSpot
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 120, Command_Cost = 0;
        public static string Command61 = "died";
        public static Dictionary<int, DateTime> DeathTime = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> LastDeathPos = new Dictionary<int, string>();

        public static void Exec(ClientInfo _cInfo)
        {
            if (Delay_Between_Uses < 1)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    TeleportPlayer(_cInfo);
                }
            }
            else
            {
                DateTime _lastDied = DateTime.Now;
                if (PersistentContainer.Instance.Players[_cInfo.playerId].LastDied != null)
                {
                    _lastDied = PersistentContainer.Instance.Players[_cInfo.playerId].LastDied;
                }
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
                            int _newDelay = Delay_Between_Uses / 2;
                            Time(_cInfo, _timepassed, _newDelay);
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
                    TeleportPlayer(_cInfo);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                string _phrase735;
                if (!Phrases.Dict.TryGetValue(735, out _phrase735))
                {
                    _phrase735 = "You can only use {CommandPrivate}{Command61} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase735 = _phrase735.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase735 = _phrase735.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase735 = _phrase735.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase735 = _phrase735.Replace("{Command61}", Command61);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase735 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
        {
            int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
            if (_currentCoins >= Command_Cost)
            {
                TeleportPlayer(_cInfo);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = "You do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void TeleportPlayer(ClientInfo _cInfo)
        {
            if (DeathTime.ContainsKey(_cInfo.entityId))
            {
                DateTime _time;
                if (DeathTime.TryGetValue(_cInfo.entityId, out _time))
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
                        if (LastDeathPos.TryGetValue(_cInfo.entityId, out _value))
                        {
                            int x, y, z;
                            string[] _cords = _value.Split(',');
                            int.TryParse(_cords[0], out x);
                            int.TryParse(_cords[1], out y);
                            int.TryParse(_cords[2], out z);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                            DeathTime.Remove(_cInfo.entityId);
                            LastDeathPos.Remove(_cInfo.entityId);
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                            }
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastDied = DateTime.Now;
                            PersistentContainer.Instance.Save();
                            string _phrase736;
                            if (!Phrases.Dict.TryGetValue(736, out _phrase736))
                            {
                                _phrase736 = "Teleporting you to your last death position. You can use this again in {DelayBetweenUses} minutes.";
                            }
                            _phrase736 = _phrase736.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase736 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your last death occurred too long ago. Command unavailable.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have no death position. Die first.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void PlayerKilled(Entity _entity2)
        {
            if (!DeathTime.ContainsKey(_entity2.entityId))
            {
                Vector3 _position = _entity2.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                string _dposition = x + "," + y + "," + z;
                DeathTime.Add(_entity2.entityId, DateTime.Now);
                LastDeathPos.Add(_entity2.entityId, _dposition);
            }
            else
            {
                Vector3 _position = _entity2.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                string _dposition = x + "," + y + "," + z;
                DeathTime[_entity2.entityId] = DateTime.Now;
                LastDeathPos[_entity2.entityId] = _dposition;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Died
    {
        public static bool IsEnabled = false;
        public static int Time = 2, Delay_Between_Uses = 120, Command_Cost = 0;
        public static string Command_died = "died";
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
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                        if (DateTime.Now < _dt)
                        {
                            int _newDelay = Delay_Between_Uses / 2;
                            Delay(_cInfo, _timepassed, _newDelay);
                            return;
                        }
                    }
                }
                Delay(_cInfo, _timepassed, Delay_Between_Uses);
            }
        }

        public static void Delay(ClientInfo _cInfo, int _timepassed, int _delay)
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
                Phrases.Dict.TryGetValue(431, out string _phrase431);
                _phrase431 = _phrase431.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase431 = _phrase431.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase431 = _phrase431.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase431 = _phrase431.Replace("{Command_died}", Command_died);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase431 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                Phrases.Dict.TryGetValue(433, out string _phrase433);
                _phrase433 = _phrase433.Replace("{CoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase433 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void TeleportPlayer(ClientInfo _cInfo)
        {
            if (DeathTime.ContainsKey(_cInfo.entityId))
            {
                if (DeathTime.TryGetValue(_cInfo.entityId, out DateTime _time))
                {
                    TimeSpan varTime = DateTime.Now - _time;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                        {
                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                            if (DateTime.Now > _dt)
                            {
                                int _newTime = _timepassed / 2;
                                _timepassed = _newTime;
                            }
                        }
                    }
                    if (_timepassed < Time)
                    {
                        if (LastDeathPos.TryGetValue(_cInfo.entityId, out string _value))
                        {
                            string[] _cords = _value.Split(',');
                            int.TryParse(_cords[0], out int x);
                            int.TryParse(_cords[1], out int y);
                            int.TryParse(_cords[2], out int z);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                            DeathTime.Remove(_cInfo.entityId);
                            LastDeathPos.Remove(_cInfo.entityId);
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                            }
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastDied = DateTime.Now;
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(432, out string _phrase432);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase432 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(434, out string _phrase434);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase434 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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

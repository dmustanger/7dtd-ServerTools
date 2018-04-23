using System;
using UnityEngine;

namespace ServerTools
{
    class DeathSpot
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 120;

        public static void DeathDelay(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                TeleportPlayer(_cInfo, _announce);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastDied == null)
                {
                    TeleportPlayer(_cInfo, _announce);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastDied;
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
                                    TeleportPlayer(_cInfo, _announce);
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase735;
                                    if (!Phrases.Dict.TryGetValue(735, out _phrase735))
                                    {
                                        _phrase735 = "{PlayerName} you can only use /died once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase735 = _phrase735.Replace("{PlayerName}", _playerName);
                                    _phrase735 = _phrase735.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase735 = _phrase735.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase735), Config.Server_Response_Name, false, "", false);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase735), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= Delay_Between_Uses)
                        {
                            TeleportPlayer(_cInfo, _announce);
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase735;
                            if (!Phrases.Dict.TryGetValue(735, out _phrase735))
                            {
                                _phrase735 = "{PlayerName} you can only use /died once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase735 = _phrase735.Replace("{PlayerName}", _playerName);
                            _phrase735 = _phrase735.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase735 = _phrase735.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase735), Config.Server_Response_Name, false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase735), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
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
                            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastDied = DateTime.Now;
                            PersistentContainer.Instance.Save();
                            string _phrase736;
                            if (!Phrases.Dict.TryGetValue(736, out _phrase736))
                            {
                                _phrase736 = "{PlayerName} teleported you to your last death position. You can use this again in {DelayBetweenUses} minutes.";
                            }
                            _phrase736 = _phrase736.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase736 = _phrase736.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase736), Config.Server_Response_Name, false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase736), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your last death occurred too long ago. Command unavailable.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have no death position. Die first.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}

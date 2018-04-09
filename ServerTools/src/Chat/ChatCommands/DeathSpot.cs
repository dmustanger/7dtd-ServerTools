using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class DeathSpot
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 120;
        public static Dictionary<int, DateTime> Died = new Dictionary<int, DateTime>();
        public static Dictionary<int, Vector3> Position = new Dictionary<int, Vector3>();
        public static List<int> Flag = new List<int>();

        public static void Check()
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo = _cInfoList[i];
                if (!Flag.Contains(_cInfo.entityId))
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player.IsDead())
                    {
                        Vector3 _pos = _player.position;
                        Died.Remove(_cInfo.entityId);
                        Died.Add(_cInfo.entityId, DateTime.Now);
                        Position.Remove(_cInfo.entityId);
                        Position.Add(_cInfo.entityId, _pos);
                        Flag.Add(_cInfo.entityId);
                    }
                }
            }
        }

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
            if (Died.ContainsKey(_cInfo.entityId))
            {
                DateTime _time;
                if (Died.TryGetValue(_cInfo.entityId, out _time))
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
                        Vector3 _value;
                        if (Position.TryGetValue(_cInfo.entityId, out _value))
                        {
                            _cInfo.SendPackage(new NetPackageTeleportPlayer(_value, false));
                            Died.Remove(_cInfo.entityId);
                            Position.Remove(_cInfo.entityId);
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastDied = DateTime.Now;
                            PersistentContainer.Instance.Save();
                            string _phrase736;
                            if (!Phrases.Dict.TryGetValue(736, out _phrase736))
                            {
                                _phrase736 = "{PlayerName} teleported you to your last death position. You can use this again in {DelayBetweenUses} minutes.";
                            }
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
                        Died.Remove(_cInfo.entityId);
                        Position.Remove(_cInfo.entityId);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your last death occurred too long ago. Command unavailable.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}No death position recorded.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}

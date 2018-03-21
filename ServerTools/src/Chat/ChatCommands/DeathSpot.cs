using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class DeathSpot
    {
        public static bool IsEnabled = false;
        public static int Delay = 120;
        public static Dictionary<int, DateTime> Died = new Dictionary<int, DateTime>();
        public static Dictionary<int, Vector3i> Position = new Dictionary<int, Vector3i>();

        public static void DeathDelay(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            if (Delay < 1)
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
                    if (_timepassed >= Delay)
                    {
                        TeleportPlayer(_cInfo, _announce);
                    }
                    else
                    {
                        int _timeleft = Delay - _timepassed;
                        string _phrase735;
                        if (!Phrases.Dict.TryGetValue(735, out _phrase735))
                        {
                            _phrase735 = "{PlayerName} you can only use /died once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                        }
                        _phrase735 = _phrase735.Replace("{PlayerName}", _playerName);
                        _phrase735 = _phrase735.Replace("{DelayBetweenUses}", Delay.ToString());
                        _phrase735 = _phrase735.Replace("{TimeRemaining}", _timeleft.ToString());
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase735), "Server", false, "", false);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase735), "Server", false, "", false));
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
                    if (_timepassed < 2)
                    {
                        Vector3i _value;
                        if (Position.TryGetValue(_cInfo.entityId, out _value))
                        {
                            Vector3 _vec3 = _value.ToVector3();
                            _cInfo.SendPackage(new NetPackageTeleportPlayer(_vec3, false));
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastDied = DateTime.Now;
                            PersistentContainer.Instance.Save();
                            string _phrase736;
                            if (!Phrases.Dict.TryGetValue(736, out _phrase736))
                            {
                                _phrase736 = "Teleported you to your last death position. You can use this again in {DelayBetweenUses} minutes.";
                            }
                            _phrase736 = _phrase736.Replace("{DelayBetweenUses}", Delay.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase736), "Server", false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase736), "Server", false, "", false));
                            }
                        }
                    }
                    else
                    {
                        Died.Remove(_cInfo.entityId);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your last death time is over two minutes. Command unavailable.[-]", Config.Chat_Response_Color), "Server", false, "", false));
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}No death position recorded.[-]", Config.Chat_Response_Color), "Server", false, "", false));
            }
        }
    }
}

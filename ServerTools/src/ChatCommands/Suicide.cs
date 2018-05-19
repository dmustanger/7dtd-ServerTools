using System;

namespace ServerTools
{
    public class Suicide
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60;

        public static void CheckPlayer(ClientInfo _cInfo, bool _announce)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                Kill(_cInfo);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastKillme == null)
                {
                    Kill(_cInfo);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastKillme;
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
                                    Kill(_cInfo);
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase8;
                                    if (!Phrases.Dict.TryGetValue(8, out _phrase8))
                                    {
                                        _phrase8 = "{PlayerName} you can only use /killme, /wrist, /hang, or /suicide once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase8 = _phrase8.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase8 = _phrase8.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase8 = _phrase8.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase8), Config.Server_Response_Name, false, "", false);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase8), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= Delay_Between_Uses)
                        {
                            Kill(_cInfo);
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase8;
                            if (!Phrases.Dict.TryGetValue(8, out _phrase8))
                            {
                                _phrase8 = "{PlayerName} you can only use /killme, /wrist, /hang, or /suicide once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase8 = _phrase8.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase8 = _phrase8.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase8 = _phrase8.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase8), Config.Server_Response_Name, false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase8), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
            }
        }

        private static void Kill(ClientInfo _cInfo)
        {
            Entity _player = GameManager.Instance.World.Entities.dict[_cInfo.entityId];
            _player.DamageEntity(new DamageSource(EnumDamageSourceType.Bullet), 99999, false, 1f);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastKillme = DateTime.Now;
            PersistentContainer.Instance.Save();
        }
    }
}
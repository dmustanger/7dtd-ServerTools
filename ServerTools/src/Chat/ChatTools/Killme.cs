using System;

namespace ServerTools
{
    public class KillMe
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60;

        public static void CheckPlayer(ClientInfo _cInfo, bool _announce)
        {
            if (Delay_Between_Uses < 1)
            {
                KillPlayer(_cInfo);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastKillme == null)
                {
                    KillPlayer(_cInfo);
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
                                int _newTime = _timepassed * 2;
                                _timepassed = _newTime;
                            }
                        }
                    }
                    if (_timepassed >= Delay_Between_Uses)
                    {
                        KillPlayer(_cInfo);
                    }
                    else
                    {
                        int _timeremaining = Delay_Between_Uses - _timepassed;
                        if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                DateTime _dt;
                                ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                if (DateTime.Now < _dt)
                                {
                                    int _newTime = _timeremaining / 2;
                                    _timeremaining = _newTime;
                                    int _newDelay = Delay_Between_Uses / 2;
                                    Delay_Between_Uses = _newDelay;
                                }
                            }
                        }
                        string _phrase8;
                        if (!Phrases.Dict.TryGetValue(8, out _phrase8))
                        {
                            _phrase8 = "{PlayerName} you can only use /killme once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                        }
                        _phrase8 = _phrase8.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase8 = _phrase8.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                        _phrase8 = _phrase8.Replace("{TimeRemaining}", _timeremaining.ToString());
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

        private static void KillPlayer(ClientInfo _cInfo)
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.entityId), (ClientInfo)null);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastKillme = DateTime.Now;
            PersistentContainer.Instance.Save();
        }
    }
}
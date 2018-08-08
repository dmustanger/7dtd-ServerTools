using System;

namespace ServerTools
{
    class Session
    {
        public static bool IsEnabled = false;

        public static void Exec(ClientInfo _cInfo)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null)
            {
                DateTime _time;
                if (Players.Session.TryGetValue(_cInfo.playerId, out _time))
                {
                    TimeSpan varTime = DateTime.Now - _time;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    int _sessionTime = p.SessionTime + _timepassed;
                    string _phrase570;
                    if (!Phrases.Dict.TryGetValue(570, out _phrase570))
                    {
                        _phrase570 = "{PlayerName} your current session is at {TimePassed} minutes. Your total session time is at {TotalTimePassed} minutes.";
                    }
                    _phrase570 = _phrase570.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase570 = _phrase570.Replace("{TimePassed}", _timepassed.ToString());
                    _phrase570 = _phrase570.Replace("{TotalTimePassed}", _sessionTime.ToString());
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase570), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }
    }
}
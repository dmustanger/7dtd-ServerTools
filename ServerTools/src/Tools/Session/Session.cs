using System;

namespace ServerTools
{
    class Session
    {
        public static bool IsEnabled = false;
        public static string Command105 = "session";

        public static void Exec(ClientInfo _cInfo)
        {
            DateTime _time;
            if (PersistentOperations.Session.TryGetValue(_cInfo.playerId, out _time))
            {
                TimeSpan varTime = DateTime.Now - _time;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                int _sessionTime = PersistentContainer.Instance.Players[_cInfo.playerId].SessionTime;
                _sessionTime = _sessionTime + _timepassed;
                string _phrase570;
                if (!Phrases.Dict.TryGetValue(570, out _phrase570))
                {
                    _phrase570 = " your current session is at {TimePassed} minutes. Your total session time is at {TotalTimePassed} minutes.";
                }
                _phrase570 = _phrase570.Replace("{TimePassed}", _timepassed.ToString());
                _phrase570 = _phrase570.Replace("{TotalTimePassed}", _sessionTime.ToString());
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase570 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
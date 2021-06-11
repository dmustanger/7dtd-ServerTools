using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ScoutPlayer
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 10;
        public static string Command129 = "scoutplayer", Command130 = "scout";

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
                    StartScouting(_cInfo);
                }
            }
            else
            {
                DateTime _lastScout = PersistentContainer.Instance.Players[_cInfo.playerId].LastScout;
                TimeSpan varTime = DateTime.Now - _lastScout;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (ReservedSlots.IsEnabled)
                {
                    if (ReservedSlots.Reduced_Delay && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                    {
                        DateTime _dt;
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                        if (DateTime.Now < _dt)
                        {
                            int _delay = Delay_Between_Uses / 2;
                            Time(_cInfo, _timepassed, _delay);
                            return;
                        }
                    }
                }
                Time(_cInfo, _timepassed, Delay_Between_Uses);
            }
        }

        private static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    StartScouting(_cInfo);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue(831, out string _phrase831);
                _phrase831 = _phrase831.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase831 = _phrase831.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase831 = _phrase831.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
                _phrase831 = _phrase831.Replace("{Command129}", Command129);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase831 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void CommandCost(ClientInfo _cInfo)
        {
            if (Wallet.IsEnabled && Command_Cost > 0)
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                {
                    StartScouting(_cInfo);
                }
                else
                {
                    Phrases.Dict.TryGetValue(832, out string _phrase832);
                    _phrase832 = _phrase832.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase832 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                StartScouting(_cInfo);
            }
        }

        private static void StartScouting(ClientInfo _cInfo)
        {
            World world = GameManager.Instance.World;
            List<EntityPlayer> EntityPlayerList = world.Players.list;
            for (int i = 0; i < EntityPlayerList.Count; i++)
            {
                EntityAlive _player2 = EntityPlayerList[i];
                if (_player2 != null && _player2.entityId != _cInfo.entityId && _player2.Spawned && !world.Players.dict[_cInfo.entityId].IsFriendsWith(world.Players.dict[_player2.entityId]))
                {
                    float distanceSq = world.Players.dict[_player2.entityId].GetDistanceSq(world.Players.dict[_cInfo.entityId].position);
                    if (distanceSq <= 200f * 200f)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastScout = DateTime.Now;
                        PersistentContainer.DataChange = true;
                        int _distance = (int)distanceSq;
                        Phrases.Dict.TryGetValue(833, out string _phrase833);
                        _phrase833 = _phrase833.Replace("{Value}", _distance.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase833 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
            }
            PersistentContainer.Instance.Players[_cInfo.playerId].LastScout = DateTime.Now;
            PersistentContainer.DataChange = true;
            Phrases.Dict.TryGetValue(834, out string _phrase834);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase834 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}

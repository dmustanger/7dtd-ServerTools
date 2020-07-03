using System;
using System.Collections.Generic;
using UnityEngine;

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
                string _phrase6;
                if (!Phrases.Dict.TryGetValue(6, out _phrase6))
                {
                    _phrase6 = "You can only use {CommandPrivate}{Command129} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase6 = _phrase6.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase6 = _phrase6.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase6 = _phrase6.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase6 = _phrase6.Replace("{Command129}", Command129);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase6 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = "You do not have enough {Currency} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{Currency}", TraderInfo.CurrencyItem);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase814 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have scouted a trail to a hostile player in the area with in " + (int)distanceSq + " blocks" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastScout = DateTime.Now;
                        PersistentContainer.Instance.Save();
                        return;
                    }
                }
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "No trails or tracks were detected to a nearby hostile player" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            PersistentContainer.Instance.Players[_cInfo.playerId].LastScout = DateTime.Now;
            PersistentContainer.Instance.Save();
        }
    }
}

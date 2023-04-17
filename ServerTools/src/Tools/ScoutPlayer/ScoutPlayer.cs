using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ScoutPlayer
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 10;
        public static string Command_scoutplayer = "scoutplayer", Command_scout = "scout";

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
                DateTime lastScout = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastScout;
                TimeSpan varTime = DateTime.Now - lastScout;
                double fractionalMinutes = varTime.TotalMinutes;
                int timepassed = (int)fractionalMinutes;
                if (ReservedSlots.IsEnabled)
                {
                    if (ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                            {
                                if (DateTime.Now < dt)
                                {
                                    int delay = Delay_Between_Uses / 2;
                                    Time(_cInfo, timepassed, delay);
                                    return;
                                }
                            }
                            else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                            {
                                if (DateTime.Now < dt)
                                {
                                    int delay = Delay_Between_Uses / 2;
                                    Time(_cInfo, timepassed, delay);
                                    return;
                                }
                            }
                        }
                    }
                }
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                {
                    int delay = Delay_Between_Uses / 2;
                    Time(_cInfo, timepassed, delay);
                    return;
                }
                Time(_cInfo, timepassed, Delay_Between_Uses);
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
                int timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue("ScoutPlayer1", out string phrase);
                phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{Command_scoutplayer}", Command_scoutplayer);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void CommandCost(ClientInfo _cInfo)
        {
            if (Wallet.IsEnabled && Command_Cost > 0)
            {
                if (Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString) >= Command_Cost)
                {
                    StartScouting(_cInfo);
                }
                else
                {
                    Phrases.Dict.TryGetValue("ScoutPlayer2", out string phrase);
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    float distanceSq = world.Players.dict[_player2.entityId].GetDistanceSq((_player2 as EntityPlayer).position);
                    if (distanceSq <= 200f * 200f)
                    {
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastScout = DateTime.Now;
                        PersistentContainer.DataChange = true;
                        int _distance = (int)distanceSq;
                        Phrases.Dict.TryGetValue("ScoutPlayer3", out string phrase);
                        phrase = phrase.Replace("{Value}", _distance.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
            }
            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastScout = DateTime.Now;
            PersistentContainer.DataChange = true;
            Phrases.Dict.TryGetValue("ScoutPlayer4", out string phrase1);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}

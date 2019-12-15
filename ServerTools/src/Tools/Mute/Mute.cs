using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class Mute
    {
        public static bool IsEnabled = false, Block_Commands = false;
        public static string Command13 = "mute", Command14 = "unmute";
        private static string[] _cmd = { Command13, Command14 };
        public static List<string> Mutes = new List<string>();

        public static void Add(ClientInfo _cInfo, string _player)
        {
            if (IsEnabled)
            {
                if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
                {
                    string _phrase107;
                    if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                    {
                        _phrase107 = " you do not have permissions to use this command.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase107 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    _player = _player.Replace(Command13 + " ", "");
                    ClientInfo _PlayertoMute = ConsoleHelper.ParseParamIdOrName(_player);
                    if (_PlayertoMute == null)
                    {
                        string _phrase201;
                        if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                        {
                            _phrase201 = " player {Player} was not found.";
                        }
                        _phrase201 = _phrase201.Replace("{Player}", _player);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase201 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        PersistentPlayer p = PersistentContainer.Instance.Players[_player];
                        if (p != null)
                        {
                            int _muteTime = PersistentContainer.Instance.Players[_player].MuteTime;
                            if (_muteTime != 0)
                            {
                                string _phrase202;
                                if (!Phrases.Dict.TryGetValue(202, out _phrase202))
                                {
                                    _phrase202 = " player {Player} is already muted.";
                                }
                                _phrase202 = _phrase202.Replace("{Player}", _player);
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase202 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                MutePlayer(_cInfo, _PlayertoMute);
                            }
                        }
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + " this command is not enabled.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void MutePlayer(ClientInfo _admin, ClientInfo _player)
        {
            Mutes.Add(_player.playerId);
            PersistentContainer.Instance.Players[_player.playerId].MuteTime = 60;
            PersistentContainer.Instance.Players[_player.playerId].MuteName = _player.playerName;
            PersistentContainer.Instance.Players[_player.playerId].MuteDate = DateTime.Now;
            PersistentContainer.Instance.Save();
            string _phrase203;
            if (!Phrases.Dict.TryGetValue(203, out _phrase203))
            {
                _phrase203 = " you have muted {Player} for 60 minutes.";
            }

            _phrase203 = _phrase203.Replace("{Player}", _player.playerName);
            ChatHook.ChatMessage(_admin, LoadConfig.Chat_Response_Color + _admin.playerName  + _phrase203 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void Remove(ClientInfo _cInfo, string _playerName)
        {
            if (IsEnabled)
            {
                if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
                {
                    string _phrase107;
                    if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                    {
                        _phrase107 = " you do not have permissions to use this command.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase107 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    _playerName = _playerName.Replace("unmute ", "");
                    ClientInfo _PlayertoUnMute = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoUnMute == null)
                    {
                        string _phrase201;
                        if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                        {
                            _phrase201 = " player {Player} was not found online.";
                        }
                        _phrase201 = _phrase201.Replace("{Player}", _playerName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase201 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        int _muteTime = PersistentContainer.Instance.Players[_PlayertoUnMute.playerId].MuteTime;
                        if (_muteTime == 0)
                        {
                            string _phrase204;
                            if (!Phrases.Dict.TryGetValue(204, out _phrase204))
                            {
                                _phrase204 = " player {Player} is not muted.";
                            }
                            _phrase204 = _phrase204.Replace("{Player}", _playerName);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase204 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            if (!Mutes.Contains(_PlayertoUnMute.playerId))
                            {
                                string _phrase204;
                                if (!Phrases.Dict.TryGetValue(204, out _phrase204))
                                {
                                    _phrase204 = " player {Player} is not muted.";
                                }
                                _phrase204 = _phrase204.Replace("{Player}", _playerName);
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase204 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Mutes.Remove(_PlayertoUnMute.playerId);
                                PersistentContainer.Instance.Players[_PlayertoUnMute.playerId].MuteTime = 0;
                                PersistentContainer.Instance.Save();
                                string _phrase205;
                                if (!Phrases.Dict.TryGetValue(205, out _phrase205))
                                {
                                    _phrase205 = " you have unmuted {Player}.";
                                }
                                _phrase205 = _phrase205.Replace("{Player}", _playerName);
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase205 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + " this command is not enabled.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void MuteList()
        {
            for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
            {
                string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                {
                    int _muteTime = p.MuteTime;
                    if (_muteTime > 0 || _muteTime == -1)
                    {
                        if (_muteTime == -1)
                        {
                            Mutes.Add(_id);
                        }
                        else
                        {
                            DateTime _muteDate = p.MuteDate;
                            TimeSpan varTime = DateTime.Now - _muteDate;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (_timepassed < _muteTime)
                            {
                                Mutes.Add(_id);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_id].MuteTime = 0;
                                PersistentContainer.Instance.Save();
                            }
                        }
                    }
                }
            }
        }

        public static void Clear()
        {
            if (Mutes.Count > 0)
            {
                for (int i = 0; i < Mutes.Count; i++)
                {
                    string _id = Mutes[i];
                    int _muteTime = PersistentContainer.Instance.Players[_id].MuteTime;
                    if (_muteTime > 0)
                    {
                        DateTime _muteDate = PersistentContainer.Instance.Players[_id].MuteDate;
                        TimeSpan varTime = DateTime.Now - _muteDate;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed >= _muteTime)
                        {
                            Mutes.Remove(_id);
                            PersistentContainer.Instance.Players[_id].MuteTime = 0;
                            PersistentContainer.Instance.Save();
                        }
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class Mute
    {
        public static bool IsEnabled = false, Block_Commands = false;
        public static string Command13 = "mute", Command14 = "unmute", Command119 = "mutelist";
        public static List<string> Mutes = new List<string>();
        public static Dictionary<int, List<int>> PrivateMutes = new Dictionary<int, List<int>>();

        public static void ClientMuteList()
        {
            if (PersistentContainer.Instance.ClientMuteList != null)
            {
                PrivateMutes = PersistentContainer.Instance.ClientMuteList;
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

        public static void Add(ClientInfo _cInfo, string _player)
        {
            _player = _player.Replace(Command13 + " ", "");
            if (!string.IsNullOrEmpty(_player))
            {
                ClientInfo _PlayertoMute = ConsoleHelper.ParseParamIdOrName(_player);
                if (_PlayertoMute == null)
                {
                    string _phrase201;
                    if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                    {
                        _phrase201 = " player {Player} was not found.";
                    }
                    _phrase201 = _phrase201.Replace("{Player}", _player);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase201 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else if (PrivateMutes != null && PrivateMutes.Count > 0 && PrivateMutes.ContainsKey(_cInfo.entityId))
                {
                    List<int> _mutedPlayers;
                    PrivateMutes.TryGetValue(_cInfo.entityId, out _mutedPlayers);
                    if (!_mutedPlayers.Contains(_PlayertoMute.entityId))
                    {
                        _mutedPlayers.Add(_PlayertoMute.entityId);
                        PrivateMutes[_cInfo.entityId] = _mutedPlayers;
                        PersistentContainer.Instance.ClientMuteList = PrivateMutes;
                        PersistentContainer.Instance.Save();
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + string.Format("You have muted player {0}", _PlayertoMute.playerName) + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + string.Format("You have already muted player {0}", _PlayertoMute.playerName) + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }

                }
                else
                {
                    List<int> _mutedPlayers = new List<int>();
                    _mutedPlayers.Add(_PlayertoMute.entityId);
                    PrivateMutes.Add(_cInfo.entityId, _mutedPlayers);
                    PersistentContainer.Instance.ClientMuteList = PrivateMutes;
                    PersistentContainer.Instance.Save();
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + string.Format("You have muted player {0}", _PlayertoMute.playerName) + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void Remove(ClientInfo _cInfo, string _player)
        {
            _player = _player.Replace(Command14 + " ", "");
            if (!string.IsNullOrEmpty(_player))
            {
                int _id;
                if (int.TryParse(_player, out _id))
                {
                    if (PrivateMutes != null && PrivateMutes.Count > 0 && PrivateMutes.ContainsKey(_cInfo.entityId))
                    {
                        List<int> _mutedPlayers;
                        PrivateMutes.TryGetValue(_cInfo.entityId, out _mutedPlayers);
                        if (_mutedPlayers.Contains(_id))
                        {
                            _mutedPlayers.Remove(_id);
                            if (_mutedPlayers.Count > 0)
                            {
                                PrivateMutes[_cInfo.entityId] = _mutedPlayers;
                            }
                            else
                            {
                                PrivateMutes.Remove(_cInfo.entityId);
                            }
                            PersistentContainer.Instance.ClientMuteList = PrivateMutes;
                            PersistentContainer.Instance.Save();
                            PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromEntityId(_id);
                            if (_pdf != null)
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + string.Format("You have removed player named {0} with id {1} from your mute list", _pdf.ecd.entityName, _id) + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + string.Format("You have removed a player with id {0} from your mute list", _id) + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "This player is not on your mute list" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have no muted players" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + string.Format("You must input a invalid id: {0}", _player) + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void List(ClientInfo _cInfo)
        {
            if (PrivateMutes != null && PrivateMutes.Count > 0 && PrivateMutes.ContainsKey(_cInfo.entityId))
            {
                List<int> _mutedPlayers;
                PrivateMutes.TryGetValue(_cInfo.entityId, out _mutedPlayers);
                for (int i = 0; i < _mutedPlayers.Count; i++)
                {
                    int _entityId = _mutedPlayers[i];
                    PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromEntityId(_entityId);
                    if (_pdf != null)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + string.Format("Muted: {0} with id {1}", _pdf.ecd.entityName, _entityId) + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have no muted players" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
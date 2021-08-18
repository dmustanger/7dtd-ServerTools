using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class Mute
    {
        public static bool IsEnabled = false, Block_Commands = false;
        public static string Command_mute = "mute", Command_unmute = "unmute", Command_mutelist = "mutelist";
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
                                PersistentContainer.DataChange = true;
                            }
                        }
                    }
                }
            }
        }

        public static void Add(ClientInfo _cInfo, string _playerEntityId)
        {
            try
            {
                _playerEntityId = _playerEntityId.Replace(Command_mute + " ", "");
                if (!string.IsNullOrEmpty(_playerEntityId))
                {
                    if (int.TryParse(_playerEntityId, out int _id))
                    {
                        ClientInfo _PlayertoMute = ConsoleHelper.ParseParamIdOrName(_playerEntityId);
                        if (_PlayertoMute == null)
                        {
                            Phrases.Dict.TryGetValue("Mute1", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _playerEntityId);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        if (PrivateMutes.ContainsKey(_id))
                        {
                            PrivateMutes.TryGetValue(_id, out List<int> _muted);
                            if (!_muted.Contains(_cInfo.entityId))
                            {
                                _muted.Add(_cInfo.entityId);
                                PrivateMutes[_id] = _muted;
                                PersistentContainer.Instance.ClientMuteList = PrivateMutes;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Mute2", out string _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _PlayertoMute.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Mute3", out string _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _PlayertoMute.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }

                        }
                        else
                        {
                            List<int> _muted = new List<int>();
                            _muted.Add(_cInfo.entityId);
                            PrivateMutes.Add(_id, _muted);
                            PersistentContainer.Instance.ClientMuteList = PrivateMutes;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Mute2", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _PlayertoMute.playerName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Mute8", out string _phrase);
                        _phrase = _phrase.Replace("{EntityId}", _id.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Mute.Add: {0}", e.Message));
            }
        }

        public static void Remove(ClientInfo _cInfo, string _playerEntityId)
        {
            try
            {
                _playerEntityId = _playerEntityId.Replace(Command_unmute + " ", "");
                if (!string.IsNullOrEmpty(_playerEntityId))
                {
                    if (int.TryParse(_playerEntityId, out int _id))
                    {
                        if (PrivateMutes.ContainsKey(_id))
                        {
                            PrivateMutes.TryGetValue(_id, out List<int> _muted);
                            if (_muted.Contains(_cInfo.entityId))
                            {
                                _muted.Remove(_cInfo.entityId);
                                if (_muted.Count > 0)
                                {
                                    PrivateMutes[_id] = _muted;
                                }
                                else
                                {
                                    PrivateMutes.Remove(_id);
                                }
                                PersistentContainer.Instance.ClientMuteList = PrivateMutes;
                                PersistentContainer.DataChange = true;
                                PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromEntityId(_id);
                                if (_pdf != null)
                                {
                                    Phrases.Dict.TryGetValue("Mute4", out string _phrase);
                                    _phrase = _phrase.Replace("{PlayerName}", _pdf.ecd.entityName);
                                    _phrase = _phrase.Replace("{EntityId}", _id.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Mute5", out string _phrase);
                                    _phrase = _phrase.Replace("{EntityId}", _id.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Mute6", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Mute6", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Mute8", out string _phrase);
                        _phrase = _phrase.Replace("{EntityId}", _id.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Mute.Remove: {0}", e.Message));
            }
        }

        public static void List(ClientInfo _cInfo)
        {
            if (PrivateMutes.Count > 0)
            {
                foreach (var _mute in PrivateMutes)
                {
                    if (_mute.Value.Contains(_cInfo.entityId))
                    {
                        PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromEntityId(_mute.Key);
                        if (_pdf != null)
                        {
                            Phrases.Dict.TryGetValue("Mute9", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _pdf.ecd.entityName);
                            _phrase = _phrase.Replace("{EntityId}", _mute.Key.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Mute7", out string _phrase);
                            _phrase = _phrase.Replace("{EntityId}", _mute.Key.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            PersistentContainer.DataChange = true;
                        }
                    }
                }
            }
        }
    }
}
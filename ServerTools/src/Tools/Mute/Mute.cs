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
            for (int i = 0; i < PersistentContainer.Instance.Players.IDs.Count; i++)
            {
                string _id = PersistentContainer.Instance.Players.IDs[i];
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
                    if (int.TryParse(_playerEntityId, out int id))
                    {
                        ClientInfo playerToMute = GeneralFunction.GetClientInfoFromEntityId(id);
                        if (playerToMute == null)
                        {
                            Phrases.Dict.TryGetValue("Mute1", out string phrase);
                            phrase = phrase.Replace("{PlayerName}", _playerEntityId);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        if (PrivateMutes.ContainsKey(id))
                        {
                            PrivateMutes.TryGetValue(id, out List<int> muted);
                            if (!muted.Contains(_cInfo.entityId))
                            {
                                muted.Add(_cInfo.entityId);
                                PrivateMutes[id] = muted;
                                PersistentContainer.Instance.ClientMuteList = PrivateMutes;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Mute2", out string phrase);
                                phrase = phrase.Replace("{PlayerName}", playerToMute.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Mute3", out string phrase);
                                phrase = phrase.Replace("{PlayerName}", playerToMute.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }

                        }
                        else
                        {
                            List<int> muted = new List<int>();
                            muted.Add(_cInfo.entityId);
                            PrivateMutes.Add(id, muted);
                            PersistentContainer.Instance.ClientMuteList = PrivateMutes;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Mute2", out string phrase);
                            phrase = phrase.Replace("{PlayerName}", playerToMute.playerName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Mute8", out string phrase);
                        phrase = phrase.Replace("{EntityId}", id.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    if (int.TryParse(_playerEntityId, out int id))
                    {
                        if (PrivateMutes.ContainsKey(id))
                        {
                            PrivateMutes.TryGetValue(id, out List<int> muted);
                            if (muted.Contains(_cInfo.entityId))
                            {
                                muted.Remove(_cInfo.entityId);
                                if (muted.Count > 0)
                                {
                                    PrivateMutes[id] = muted;
                                }
                                else
                                {
                                    PrivateMutes.Remove(id);
                                }
                                PersistentContainer.Instance.ClientMuteList = PrivateMutes;
                                PersistentContainer.DataChange = true;
                                PlayerDataFile pdf = GeneralFunction.GetPlayerDataFileFromEntityId(id);
                                if (pdf != null)
                                {
                                    Phrases.Dict.TryGetValue("Mute4", out string phrase);
                                    phrase = phrase.Replace("{PlayerName}", pdf.ecd.entityName);
                                    phrase = phrase.Replace("{EntityId}", id.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Mute5", out string phrase);
                                    phrase = phrase.Replace("{EntityId}", id.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Mute6", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Mute6", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Mute8", out string phrase);
                        phrase = phrase.Replace("{EntityId}", id.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                foreach (var mute in PrivateMutes)
                {
                    if (mute.Value.Contains(_cInfo.entityId))
                    {
                        PlayerDataFile pdf = GeneralFunction.GetPlayerDataFileFromEntityId(mute.Key);
                        if (pdf != null)
                        {
                            Phrases.Dict.TryGetValue("Mute9", out string phrase);
                            phrase = phrase.Replace("{PlayerName}", pdf.ecd.entityName);
                            phrase = phrase.Replace("{EntityId}", mute.Key.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Mute7", out string phrase);
                            phrase = phrase.Replace("{EntityId}", mute.Key.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    string id = Mutes[i];
                    int muteTime = PersistentContainer.Instance.Players[id].MuteTime;
                    if (muteTime > 0)
                    {
                        DateTime muteDate = PersistentContainer.Instance.Players[id].MuteDate;
                        TimeSpan varTime = DateTime.Now - muteDate;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int timepassed = (int)fractionalMinutes;
                        if (timepassed >= muteTime)
                        {
                            Mutes.Remove(id);
                            PersistentContainer.Instance.Players[id].MuteTime = 0;
                            PersistentContainer.DataChange = true;
                        }
                    }
                }
            }
        }
    }
}
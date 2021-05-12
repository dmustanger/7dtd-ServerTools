using System;
using System.Collections.Generic;

namespace ServerTools
{
    class KickVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
        public static string Command68 = "kickvote";
        public static List<int> Kick = new List<int>();
        private static ClientInfo _playerKick;

        public static void Vote(ClientInfo _cInfo, string _player)
        {
            try
            {
                if (!VoteOpen)
                {
                    int _playerCount = ConnectionManager.Instance.ClientCount();
                    if (_playerCount >= Players_Online)
                    {
                        int _entityId;
                        if (int.TryParse(_player, out _entityId))
                        {
                            ClientInfo _playerInfo = ConnectionManager.Instance.Clients.ForEntityId(_entityId);
                            if (_playerInfo != null)
                            {
                                if (_playerInfo.playerId != _cInfo.playerId)
                                {
                                    _playerKick = _playerInfo;
                                    VoteOpen = true;
                                    Phrases.Dict.TryGetValue(711, out string _phrase711);
                                    _phrase711 = _phrase711.Replace("{PlayerName}", _playerInfo.playerName);
                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase711 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                    Phrases.Dict.TryGetValue(715, out string _phrase715);
                                    _phrase715 = _phrase715.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
                                    _phrase715 = _phrase715.Replace("{Command70}", RestartVote.Command70);
                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase715 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(716, out string _phrase716);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase716 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue(712, out string _phrase712);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase712 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(713, out string _phrase713);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase713 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KickVote.Vote: {0}", e.Message));
            }
        }

        public static void ProcessKickVote()
        {
            try
            {
                if (Kick.Count > 0)
                {
                    if (Kick.Count >= Votes_Needed)
                    {
                        Phrases.Dict.TryGetValue(720, out string _phrase720);
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _playerKick.entityId, _phrase720), null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(717, out string _phrase717);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase717 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(718, out string _phrase718);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase718 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KickVote.ProcessKickVote: {0}", e.Message));
            }
            Kick.Clear();
            VoteOpen = false;
        }

        public static void List(ClientInfo _cInfo)
        {
            try
            {
                List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
                if (ClientInfoList != null && ClientInfoList.Count > 1)
                {
                    for (int i = 0; i < ClientInfoList.Count; i++)
                    {
                        ClientInfo _cInfo2 = ClientInfoList[i];
                        if (_cInfo2 != _cInfo)
                        {
                            Phrases.Dict.TryGetValue(714, out string _phrase714);
                            _phrase714 = _phrase714.Replace("{PlayerName}", _cInfo2.playerName);
                            _phrase714 = _phrase714.Replace("{Id}", _cInfo2.entityId.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase714 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    Phrases.Dict.TryGetValue(719, out string _phrase719);
                    _phrase719 = _phrase719.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
                    _phrase719 = _phrase719.Replace("{Command68}", Command68);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase719 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(721, out string _phrase721);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase721 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KickVote.List: {0}", e.Message));
            }
        }
    }
}

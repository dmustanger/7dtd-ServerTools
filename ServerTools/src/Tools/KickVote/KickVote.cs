using System;
using System.Collections.Generic;

namespace ServerTools
{
    class KickVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
        public static string Command_kickvote = "kickvote";
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
                        if (int.TryParse(_player, out int _entityId))
                        {
                            ClientInfo _playerInfo = ConnectionManager.Instance.Clients.ForEntityId(_entityId);
                            if (_playerInfo != null)
                            {
                                if (_playerInfo.playerId != _cInfo.playerId)
                                {
                                    _playerKick = _playerInfo;
                                    VoteOpen = true;
                                    Phrases.Dict.TryGetValue("KickVote1", out string _phrase);
                                    _phrase = _phrase.Replace("{PlayerName}", _playerInfo.playerName);
                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                    Phrases.Dict.TryGetValue("KickVote5", out _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_yes}", RestartVote.Command_yes);
                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("KickVote6", out string _phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("KickVote2", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("KickVote3", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                        Phrases.Dict.TryGetValue("KickVote10", out string _phrase);
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _playerKick.entityId, _phrase), null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("KickVote7", out string _phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("KickVote8", out string _phrase);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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
                            Phrases.Dict.TryGetValue("KickVote4", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _cInfo2.playerName);
                            _phrase = _phrase.Replace("{Id}", _cInfo2.entityId.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    Phrases.Dict.TryGetValue("KickVote9", out string _phrase1);
                    _phrase1 = _phrase1.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase1 = _phrase1.Replace("{Command_kickvote}", Command_kickvote);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("KickVote11", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KickVote.List: {0}", e.Message));
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace ServerTools
{
    class KickVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;

        public static List<int> Kick = new List<int>();

        public static string Command_kickvote = "kickvote";

        private static ClientInfo playerKick;

        public static void Vote(ClientInfo _cInfo, string _player)
        {
            try
            {
                if (!VoteOpen)
                {
                    int playerCount = ConnectionManager.Instance.ClientCount();
                    if (playerCount >= Players_Online)
                    {
                        if (int.TryParse(_player, out int entityId))
                        {
                            ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromEntityId(entityId);
                            if (cInfo2 != null)
                            {
                                if (cInfo2.CrossplatformId.CombinedString != _cInfo.CrossplatformId.CombinedString)
                                {
                                    playerKick = cInfo2;
                                    VoteOpen = true;
                                    Phrases.Dict.TryGetValue("KickVote1", out string phrase);
                                    phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                    Phrases.Dict.TryGetValue("KickVote5", out phrase);
                                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    phrase = phrase.Replace("{Command_yes}", RestartVote.Command_yes);
                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("KickVote6", out string phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("KickVote2", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("KickVote3", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                        Phrases.Dict.TryGetValue("KickVote10", out string phrase);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", playerKick.entityId, phrase), null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("KickVote7", out string phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("KickVote8", out string phrase);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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
                List<ClientInfo> clientList = PersistentOperations.ClientList();
                if (clientList != null)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo2 = clientList[i];
                        if (cInfo2 != _cInfo)
                        {
                            Phrases.Dict.TryGetValue("KickVote4", out string phrase);
                            phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                            phrase = phrase.Replace("{Id}", cInfo2.entityId.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    Phrases.Dict.TryGetValue("KickVote9", out string phrase1);
                    phrase1 = phrase1.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase1 = phrase1.Replace("{Command_kickvote}", Command_kickvote);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("KickVote11", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KickVote.List: {0}", e.Message));
            }
        }
    }
}

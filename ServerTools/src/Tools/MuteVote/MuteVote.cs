using System;
using System.Collections.Generic;

namespace ServerTools
{
    class MuteVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;

        public static List<int> Votes = new List<int>();

        public static string Command_mutevote = "mutevote";

        private static ClientInfo playerMute;

        public static void Vote(ClientInfo _cInfo, string _player)
        {
            if (!VoteOpen)
            {
                if (ConnectionManager.Instance.ClientCount() >= Players_Online)
                {
                    if (int.TryParse(_player, out int entityId))
                    {
                        ClientInfo cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(entityId);
                        if (cInfo2 != null)
                        {
                            if (Mute.Mutes.Contains(cInfo2.playerId))
                            {
                                Phrases.Dict.TryGetValue("MuteVote5", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                            playerMute = cInfo2;
                            VoteOpen = true;
                            Phrases.Dict.TryGetValue("MuteVote1", out string phrase1);
                            phrase1 = phrase1.Replace("{PlayerName}", cInfo2.playerName);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            Phrases.Dict.TryGetValue("MuteVote2", out phrase1);
                            phrase1 = phrase1.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            phrase1 = phrase1.Replace("{Command_yes}", RestartVote.Command_yes);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("MuteVote6", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("MuteVote7", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void ProcessMuteVote()
        {
            if (MuteVote.Votes.Count >= Votes_Needed)
            {
                Mute.Mutes.Add(playerMute.playerId);
                PersistentContainer.Instance.Players[playerMute.playerId].MuteTime = 60;
                PersistentContainer.Instance.Players[playerMute.playerId].MuteName = playerMute.playerName;
                PersistentContainer.Instance.Players[playerMute.playerId].MuteDate = DateTime.Now;
                PersistentContainer.DataChange = true;
                Phrases.Dict.TryGetValue("MuteVote3", out string phrase);
                phrase = phrase.Replace("{PlayerName}", playerMute.playerName);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            playerMute = null;
            MuteVote.Votes.Clear();
        }

        public static void List(ClientInfo _cInfo)
        {
            List<ClientInfo> clientList = PersistentOperations.ClientList();
            if (clientList != null)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    ClientInfo cInfo2 = clientList[i];
                    if (cInfo2 != _cInfo)
                    {
                        Phrases.Dict.TryGetValue("MuteVote8", out string phrase);
                        phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                        phrase = phrase.Replace("{Id}", cInfo2.entityId.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);

                    }
                }
                Phrases.Dict.TryGetValue("MuteVote4", out string phrase1);
                phrase1 = phrase1.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase1 = phrase1.Replace("{Command_mutevote}", Command_mutevote);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("MuteVote9", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

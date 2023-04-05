using System.Collections.Generic;

namespace ServerTools
{
    class AutoPartyInvite
    {
        public static bool IsEnabled = false;
        public static string Command_party = "party", Command_party_add = "party add", Command_party_remove = "party remove";

        public static void Party(ClientInfo _cInfo)
        {
            List<string> invites = new List<string>();
            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite != null &&
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite.Count > 0)
            {
                List<string[]> autoInvites = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite;
                for (int i = 0; i < autoInvites.Count; i++)
                {
                    if (!int.TryParse(autoInvites[i][0], out int value))
                    {
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite.Remove(autoInvites[i]);
                        PersistentContainer.DataChange = true;
                        continue;
                    }
                    invites.Add(autoInvites[i][0]);
                    Phrases.Dict.TryGetValue("AutoPartyInvite1", out string phrase);
                    phrase = phrase.Replace("{Value}", autoInvites[i][0].ToString());
                    phrase = phrase.Replace("{Name}", autoInvites[i][1].ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("AutoPartyInvite2", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            PersistentPlayerData ppd1 = GeneralOperations.GetPersistentPlayerDataFromEntityId(_cInfo.entityId);
            if (ppd1 != null)
            {
                List<ClientInfo> clients = GeneralOperations.ClientList();
                for (int i = 0; i < clients.Count; i++)
                {
                    ClientInfo client = clients[i];
                    if (_cInfo.entityId != client.entityId && !invites.Contains(client.entityId.ToString()))
                    {
                        PersistentPlayerData ppd2 = GeneralOperations.GetPersistentPlayerDataFromEntityId(client.entityId);
                        if (ppd2 != null)
                        {
                            if (ppd1.ACL.Contains(client.CrossplatformId) && ppd2.ACL.Contains(_cInfo.CrossplatformId))
                            {
                                Phrases.Dict.TryGetValue("AutoPartyInvite8", out string phrase);
                                phrase = phrase.Replace("{Value}", client.entityId.ToString());
                                phrase = phrase.Replace("{Name}", client.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
        }

        public static void Exec(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (GameManager.Instance.World.Players.dict.Count > 1)
            {
                foreach (KeyValuePair<int, EntityPlayer> player in GeneralOperations.GetEntityPlayers())
                {
                    if (_player.entityId != player.Value.entityId)
                    {
                        if (_player.partyInvites != null && _player.partyInvites.Contains(player.Value))
                        {
                            return;
                        }
                        PersistentPlayerData ppd2 = GeneralOperations.GetPersistentPlayerDataFromEntityId(player.Key);
                        if (ppd2 != null && ppd2.ACL != null)
                        {
                            if (PersistentContainer.Instance.Players[ppd2.UserIdentifier.CombinedString].AutoPartyInvite != null &&
                                PersistentContainer.Instance.Players[ppd2.UserIdentifier.CombinedString].AutoPartyInvite.Count > 0)
                            {
                                List<string[]> autoInvites = PersistentContainer.Instance.Players[ppd2.UserIdentifier.CombinedString].AutoPartyInvite;
                                for (int i = 0; i < autoInvites.Count; i++)
                                {
                                    if (autoInvites[i][0] == _cInfo.entityId.ToString())
                                    {
                                        PersistentPlayerData ppd1 = GeneralOperations.GetPersistentPlayerDataFromEntityId(_player.entityId);
                                        if (ppd1 != null && ppd1.ACL != null && ppd1.ACL.Contains(ppd2.UserIdentifier) && ppd2.ACL.Contains(ppd1.UserIdentifier))
                                        {
                                            EntityPlayer player2 = player.Value;
                                            if ((player2.IsInParty() && player2.IsPartyLead() == player2 && player2.Party != null && player2.Party.MemberList.Count < Constants.cMaxPartySize) || !player2.IsInParty())
                                            {
                                                _player.AddPartyInvite(player.Key);
                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, player.Key, _player.entityId, null));
                                                Phrases.Dict.TryGetValue("AutoPartyInvite7", out string phrase);
                                                phrase = phrase.Replace("{PlayerName}", player2.EntityName);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void Add(ClientInfo _cInfo, string _target)
        {
            if (!int.TryParse(_target, out int entityId))
            {
                Phrases.Dict.TryGetValue("AutoPartyInvite6", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            ClientInfo cInfo2 = GeneralOperations.GetClientInfoFromEntityId(entityId);
            if (cInfo2 != null)
            {
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite.Count > 0)
                {
                    List<string[]> autoInvites = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite;
                    for (int i = 0; i < autoInvites.Count; i++)
                    {
                        if (autoInvites[i][0] == cInfo2.entityId.ToString())
                        {
                            Phrases.Dict.TryGetValue("AutoPartyInvite3", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                    autoInvites.Add(new string[] { cInfo2.entityId.ToString(), cInfo2.playerName });
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite = autoInvites;
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("AutoPartyInvite4", out string phrase1);
                    phrase1 = phrase1.Replace("{Value}", cInfo2.entityId.ToString());
                    phrase1 = phrase1.Replace("{Name}", cInfo2.playerName);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    List<string[]> autoInvites = new List<string[]>();
                    autoInvites.Add(new string[] { cInfo2.entityId.ToString(), cInfo2.playerName });
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite = autoInvites;
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("AutoPartyInvite4", out string phrase);
                    phrase = phrase.Replace("{Value}", cInfo2.entityId.ToString());
                    phrase = phrase.Replace("{Name}", cInfo2.playerName);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void Remove(ClientInfo _cInfo, string _target)
        {
            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite != null &&
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite.Count > 0)
            {
                List<string[]> autoInvites = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite;
                for (int i = 0; i < autoInvites.Count; i++)
                {
                    if (autoInvites[i][0] == _target)
                    {
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AutoPartyInvite.RemoveAt(i);
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("AutoPartyInvite5", out string phrase);
                        phrase = phrase.Replace("{Value}", autoInvites[i][0]);
                        phrase = phrase.Replace("{Name}", autoInvites[i][1]);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                Phrases.Dict.TryGetValue("AutoPartyInvite5", out string phrase1);
                phrase1 = phrase1.Replace("{Value}", _target);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("AutoPartyInvite2", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

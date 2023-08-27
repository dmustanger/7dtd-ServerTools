using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    public class Homes
    {
        public static bool IsEnabled = false, Player_Check = false, Zombie_Check = false, Vehicle_Check = false, Return = false;
        public static int Delay_Between_Uses = 0, Max_Homes = 2, Reserved_Max_Homes = 4, Command_Cost = 0;
        public static string Command_go_home = "go home", Command_home = "home", Command_ho = "ho",
            Command_fhome = "fhome", Command_fho = "fho",
            Command_sethome = "sethome", Command_home_save = "home save", Command_hs = "hs",
            Command_home_delete = "home delete", Command_hd = "hd";
        public static Dictionary<int, DateTime> Invite = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> FriendPosition = new Dictionary<int, string>();

        public static void List(ClientInfo _cInfo)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.Count > 0)
                {
                    if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                    {
                        if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                ListResult(_cInfo, Reserved_Max_Homes);
                                return;
                            }
                        }
                        else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                ListResult(_cInfo, Reserved_Max_Homes);
                                return;
                            }
                        }
                    }
                }
                ListResult(_cInfo, Max_Homes);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.List: {0}", e.Message));
            }
        }

        public static void ListResult(ClientInfo _cInfo, int _homeLimit)
        {
            try
            {
                Dictionary<string, string> homes = new Dictionary<string, string>();
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes != null)
                {
                    homes = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes;
                }
                if (homes.Count > 0)
                {
                    int count = _homeLimit + PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HomeSpots;
                    var homeList = homes.ToArray();
                    for (int i = 0; i < homeList.Length; i++)
                    {
                        if (i <= count)
                        {
                            Phrases.Dict.TryGetValue("Homes2", out string phrase);
                            phrase = phrase.Replace("{Name}", homeList[i].Key);
                            phrase = phrase.Replace("{Position}", homeList[i].Value);
                            phrase = phrase.Replace("{Cost}", Command_Cost.ToString());
                            phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes1", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.ListResult: {0}", e.Message));
            }
        }

        public static void TeleDelay(ClientInfo _cInfo, string _home, bool _friends)
        {
            try
            {
                if (!Event.Teams.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    if (Delay_Between_Uses < 1)
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.Count > 0)
                        {
                            Checks(_cInfo, _home, _friends);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Homes1", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastHome != null)
                        {
                            DateTime lastHome = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastHome;
                            TimeSpan varTime = DateTime.Now - lastHome;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                                {
                                    if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            int delay = Delay_Between_Uses / 2;
                                            Time(_cInfo, _home, timepassed, delay, _friends);
                                            return;
                                        }
                                    }
                                    else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            int delay = Delay_Between_Uses / 2;
                                            Time(_cInfo, _home, timepassed, delay, _friends);
                                            return;
                                        }
                                    }
                                }
                            }
                            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Time(_cInfo, _home, timepassed, delay, _friends);
                                return;
                            }
                            Time(_cInfo, _home, timepassed, Delay_Between_Uses, _friends);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Homes1", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes3", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Homes.TeleDelay: {0}", e.Message);
            }
        }

        private static void Time(ClientInfo _cInfo, string _homeName, int _timepassed, int _delay, bool _friends)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    Checks(_cInfo, _homeName, _friends);
                }
                else
                {
                    int timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Homes4", out string phrase);
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    phrase = phrase.Replace("{Value}", timeleft.ToString());
                    phrase = phrase.Replace("{Command_home}", Command_home);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Homes.Time: {0}", e.Message);
            }
        }

        private static void Checks(ClientInfo _cInfo, string _homeName, bool _friends)
        {
            try
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player == null)
                {
                    return;
                }
                if (Zombie_Check)
                {
                    if (Teleportation.ZCheck(_cInfo, player))
                    {
                        Phrases.Dict.TryGetValue("Teleport1", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                if (Player_Check)
                {
                    if (Teleportation.PCheck(_cInfo, player))
                    {
                        Phrases.Dict.TryGetValue("Teleport2", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                if (Vehicle_Check)
                {
                    Entity attachedEntity = player.AttachedToEntity;
                    if (attachedEntity != null)
                    {
                        Phrases.Dict.TryGetValue("Teleport3", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                if (Command_Cost > 0)
                {
                    CommandCost(_cInfo, _homeName, player.position, _friends);
                }
                else
                {
                    Exec(_cInfo, _homeName, player.position, _friends);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Homes.Checks: {0}", e.Message);
            }
        }

        private static void CommandCost(ClientInfo _cInfo, string _homeName, Vector3 _position, bool _friends)
        {
            try
            {
                int currency = 0, bankCurrency = 0, cost = Command_Cost;
                if (Wallet.IsEnabled)
                {
                    currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                }
                if (Bank.IsEnabled && Bank.Direct_Payment)
                {
                    bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
                }
                if (currency + bankCurrency >= cost)
                {
                    if (currency > 0)
                    {
                        if (currency < cost)
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                            cost -= currency;
                            Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                        }
                        else
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                        }
                    }
                    else
                    {
                        Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                    }
                    Exec(_cInfo, _homeName, _position, _friends);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes6", out string phrase);
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Homes.CommandCost: {0}", e.Message);
            }
        }

        private static void Exec(ClientInfo _cInfo, string _homeName, Vector3 _position, bool _friends)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.ContainsKey(_homeName))
                {
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.TryGetValue(_homeName, out string homePos))
                    {
                        string[] cords = homePos.Split(',');
                        int.TryParse(cords[0], out int x);
                        int.TryParse(cords[1], out int y);
                        int.TryParse(cords[2], out int z);
                        EnumLandClaimOwner claimOwner;
                        Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(x, y, z);
                        if (chunk == null)
                        {
                            claimOwner = EnumLandClaimOwner.Self;
                        }
                        else
                        {
                            claimOwner = GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, new Vector3i(x, y, z));
                        }
                        if (claimOwner == EnumLandClaimOwner.Self || claimOwner == EnumLandClaimOwner.Ally)
                        {
                            if (_friends)
                            {
                                FriendInvite(_cInfo, _position, homePos);
                            }
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastHome = DateTime.Now;
                            PersistentContainer.DataChange = true;
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.Remove(_homeName);
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Homes5", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Homes7", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes7", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Homes.Exec: {0}", e.Message);
            }
        }

        public static void SaveClaimCheck(ClientInfo _cInfo, string _home)
        {
            try
            {
                if (!Event.Teams.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                    if (player != null)
                    {
                        Vector3 position = player.GetPosition();
                        EnumLandClaimOwner owner = GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, new Vector3i(position.x, position.y, position.z));
                        if (owner == EnumLandClaimOwner.Ally || owner == EnumLandClaimOwner.Self)
                        {
                            ReservedCheck(_cInfo, _home);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Homes8", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes3", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Homes.SaveClaimCheck: {0}", e.Message));
            }
        }

        private static void ReservedCheck(ClientInfo _cInfo, string _home)
        {
            try
            {
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                    {
                        if (DateTime.Now < dt)
                        {
                            SaveHome(_cInfo, _home, Reserved_Max_Homes);
                            return;
                        }
                    }
                    else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                    {
                        if (DateTime.Now < dt)
                        {
                            SaveHome(_cInfo, _home, Reserved_Max_Homes);
                            return;
                        }
                    }
                }
                SaveHome(_cInfo, _home, Max_Homes);
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Homes.ReservedCheck: {0}", e.Message);
            }
        }

        private static void SaveHome(ClientInfo _cInfo, string _homeName, int _homeTotal)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_homeName))
                {
                    Phrases.Dict.TryGetValue("Homes17", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.Count > 0)
                {      
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.Count < _homeTotal + PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HomeSpots)
                    {
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                        if (player != null)
                        {
                            Vector3 position = player.GetPosition();
                            int x = (int)position.x;
                            int y = (int)position.y;
                            int z = (int)position.z;
                            string wposition = x + "," + y + "," + z;
                            if (!PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.ContainsKey(_homeName))
                            {
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.Add(_homeName, wposition);
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Homes9", out string phrase);
                                phrase = phrase.Replace("{Name}", _homeName);
                                phrase = phrase.Replace("{Position}", wposition);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Homes10", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Homes11", out string phrase);
                        phrase = phrase.Replace("{Value}", _homeTotal.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else if (_homeTotal > 0)
                {
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                    if (player != null)
                    {
                        Dictionary<string, string> homes = new Dictionary<string, string>();
                        Vector3 position = player.GetPosition();
                        int x = (int)position.x;
                        int y = (int)position.y;
                        int z = (int)position.z;
                        string wposition = x + "," + y + "," + z;
                        homes.Add(_homeName, wposition);
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes = homes;
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("Homes12", out string phrase);
                        phrase = phrase.Replace("{Name}", _homeName);
                        phrase = phrase.Replace("{Position}", wposition);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes11", out string phrase);
                    phrase = phrase.Replace("{Value}", _homeTotal.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Homes.SaveHome: {0}", e.Message);
            }
        }

        public static void DelHome(ClientInfo _cInfo, string _homeName)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.ContainsKey(_homeName))
                {
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Homes.Remove(_homeName);
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("Homes13", out string phrase);
                    phrase = phrase.Replace("{Name}", _homeName);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Homes7", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Homes.DelHome: {0}", e.Message);
            }
        }

        public static void FriendInvite(ClientInfo _cInfo, Vector3 _position, string _destination)
        {
            try
            {
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    List<ClientInfo> clientList = GeneralOperations.ClientList();
                    if (clientList != null)
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            EntityPlayer player2 = GeneralOperations.GetEntityPlayer(cInfo2.entityId);
                            if (player2 != null)
                            {
                                if (player.IsFriendsWith(player2) && player.GetDistance(player2) <= 25)
                                {
                                    Phrases.Dict.TryGetValue("Homes14", out string phrase);
                                    phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    phrase = phrase.Replace("{Command_go_home}", Command_go_home);
                                    ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    Phrases.Dict.TryGetValue("Homes15", out phrase);
                                    phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    if (Invite.ContainsKey(cInfo2.entityId))
                                    {
                                        Invite.Remove(cInfo2.entityId);
                                        FriendPosition.Remove(cInfo2.entityId);
                                    }
                                    Invite.Add(cInfo2.entityId, DateTime.Now);
                                    FriendPosition.Add(cInfo2.entityId, _destination);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Homes.FriendInvite: {0}", e.Message);
            }
        }

        public static void FriendHome(ClientInfo _cInfo)
        {
            try
            {
                Invite.TryGetValue(_cInfo.entityId, out DateTime dt);
                {
                    TimeSpan varTime = DateTime.Now - dt;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timepassed = (int)fractionalMinutes;
                    if (timepassed <= 2)
                    {
                        FriendPosition.TryGetValue(_cInfo.entityId, out string pos);
                        {
                            string[] cords = pos.Split(',');
                            int.TryParse(cords[0], out int x);
                            int.TryParse(cords[1], out int y);
                            int.TryParse(cords[2], out int z);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                            Invite.Remove(_cInfo.entityId);
                            FriendPosition.Remove(_cInfo.entityId);
                        }
                    }
                    else
                    {
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                        Phrases.Dict.TryGetValue("Homes16", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Homes.FriendHome: {0}", e.Message);
            }
        }
    }
}
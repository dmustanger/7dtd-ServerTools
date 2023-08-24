using Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ServerTools
{
    class RIO
    {
        public static bool IsEnabled = false;
        public static string Directory = "", Command_rio = "rio";
        public static int Bet = 25;

        public static Dictionary<string, int> Access = new Dictionary<string, int>();
        public static Dictionary<string, int[]> Tables = new Dictionary<string, int[]>();
        public static Dictionary<string, Dictionary<int, string>> Events = new Dictionary<string, Dictionary<int, string>>();
        public static Dictionary<string, Dictionary<string, int>> Claims = new Dictionary<string, Dictionary<string, int>>();

        private static readonly string NumSet = "1928374650", DieSet = "136425";

        public static void Exec(ClientInfo _cInfo)
        {
            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay)
            {
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "Overlay is set off. Steam browser is disabled" + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            string ip = _cInfo.ip;
            bool duplicate = false;
            List<ClientInfo> clientList = GeneralOperations.ClientList();
            if (clientList != null && clientList.Count > 1)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    ClientInfo cInfo = clientList[i];
                    if (cInfo != null && cInfo.entityId != _cInfo.entityId && ip == cInfo.ip)
                    {
                        duplicate = true;
                        break;
                    }
                }
            }
            long ipLong = NetworkUtils.ToInt(_cInfo.ip);
            if (duplicate || (ipLong >= NetworkUtils.ToInt("10.0.0.0") && ipLong <= NetworkUtils.ToInt("10.255.255.255")) ||
                (ipLong >= NetworkUtils.ToInt("172.16.0.0") && ipLong <= NetworkUtils.ToInt("172.31.255.255")) ||
                (ipLong >= NetworkUtils.ToInt("192.168.0.0") && ipLong <= NetworkUtils.ToInt("192.168.255.255")) ||
                _cInfo.ip == "127.0.0.1")
            {
                string securityId = "";
                for (int i = 0; i < 10; i++)
                {
                    string pass = CreatePassword(4);
                    if (!Access.ContainsKey(pass))
                    {
                        securityId = pass;
                        if (!Access.ContainsValue(_cInfo.entityId))
                        {
                            Access.Add(securityId, _cInfo.entityId);
                        }
                        else
                        {
                            if (Access.Count > 0)
                            {
                                var accessList = Access.ToArray();
                                for (int j = 0; j < accessList.Length; j++)
                                {
                                    if (accessList[j].Value == _cInfo.entityId)
                                    {
                                        Access.TryGetValue(accessList[j].Key, out int id);
                                        var tables = Tables.ToArray();
                                        for (int k = 0; k < tables.Length; k++)
                                        {
                                            int[] players = tables[k].Value;
                                            for (int l = 0; l < players.Length; l++)
                                            {
                                                if (players[l] == id)
                                                {
                                                    players[l] = -1;
                                                    Tables[tables[k].Key] = players;
                                                    if (Events.TryGetValue(tables[k].Key, out Dictionary<int, string> events))
                                                    {
                                                        events.Add(events.Count, "Left╚" + (l + 1));
                                                    }
                                                }
                                            }
                                        }
                                        Access.Remove(accessList[j].Key);
                                        if (WebAPI.Authorized.ContainsValue(accessList[j].Key))
                                        {
                                            var authorizedList = WebAPI.Authorized.ToArray();
                                            for (int k = 0; k < authorizedList.Length; k++)
                                            {
                                                if (authorizedList[k].Value == accessList[j].Key)
                                                {
                                                    WebAPI.Authorized.Remove(authorizedList[k].Key);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            Access.Add(securityId, _cInfo.entityId);
                        }
                        break;
                    }
                }
                Phrases.Dict.TryGetValue("Rio1", out string phrase);
                phrase = phrase.Replace("{Value}", securityId);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserRio", true));
            }
            else
            {
                if (Access.ContainsValue(_cInfo.entityId))
                {
                    var clients = Access.ToArray();
                    for (int i = 0; i < clients.Length; i++)
                    {
                        if (clients[i].Value == _cInfo.entityId)
                        {
                            Access.TryGetValue(clients[i].Key, out int id);
                            Access.Remove(clients[i].Key);
                            Access.Add(ip, _cInfo.entityId);
                            var tables = Tables.ToArray();
                            for (int k = 0; k < tables.Length; k++)
                            {
                                int[] players = tables[k].Value;
                                for (int l = 0; l < players.Length; l++)
                                {
                                    if (players[l] == id)
                                    {
                                        players[l] = -1;
                                        Tables[tables[k].Key] = players;
                                        if (Events.TryGetValue(tables[k].Key, out Dictionary<int, string> events))
                                        {
                                            events.Add(events.Count, "Left╚" + (l + 1));
                                        }
                                    }
                                }
                            }
                            if (WebAPI.Authorized.ContainsValue(clients[i].Key))
                            {
                                var authorizedList = WebAPI.Authorized.ToArray();
                                for (int k = 0; k < authorizedList.Length; k++)
                                {
                                    if (authorizedList[k].Value == clients[i].Key)
                                    {
                                        WebAPI.Authorized.Remove(authorizedList[k].Key);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                else if (Access.ContainsKey(ip))
                {
                    Access.TryGetValue(ip, out int id);
                    Access[ip] = _cInfo.entityId;
                    var tables = Tables.ToArray();
                    for (int k = 0; k < tables.Length; k++)
                    {
                        int[] players = tables[k].Value;
                        for (int l = 0; l < players.Length; l++)
                        {
                            if (players[l] == id)
                            {
                                players[l] = -1;
                                Tables[tables[k].Key] = players;
                                if (Events.TryGetValue(tables[k].Key, out Dictionary<int, string> events))
                                {
                                    events.Add(events.Count, "Left╚" + (l + 1));
                                }
                            }
                        }
                    }
                    var authorizedList = WebAPI.Authorized.ToArray();
                    for (int k = 0; k < authorizedList.Length; k++)
                    {
                        if (authorizedList[k].Key == ip)
                        {
                            WebAPI.Authorized.Remove(authorizedList[k].Key);
                        }
                    }
                }
                else
                {
                    Access.Add(ip, _cInfo.entityId);
                }
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserRio", true));
            }
        }

        public static string CreatePassword(int _length)
        {
            string pass = string.Empty;
            Random rnd = new System.Random();
            for (int i = 0; i < _length; i++)
            {
                pass += GeneralOperations.NumSet.ElementAt(rnd.Next(0, 10));
            }
            return pass;
        }

        public static string CreateTable()
        {
            string tableNumber = string.Empty;
            Random rnd = new System.Random();
            for (int i = 0; i < 4; i++)
            {
                tableNumber += NumSet.ElementAt(rnd.Next(0, 10));
            }
            return tableNumber;
        }

        public static string GetRoll()
        {
            string dice = string.Empty;
            Random rnd = new System.Random();
            for (int i = 0; i < 5; i++)
            {
                dice += DieSet.ElementAt(rnd.Next(0, 6)) + ",";
            }
            dice = dice.Remove(dice.Length - 1);
            return dice;
        }

        public static void RemovePlayer(ClientInfo _cInfo)
        {
            var tables = Tables.ToArray();
            for (int i = 0; i < tables.Length; i++)
            {
                if (tables[i].Value.Contains(_cInfo.entityId))
                {

                    break;
                }
            }
        }
    }
}

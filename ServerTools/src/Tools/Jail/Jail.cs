using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class Jail
    {
        public static bool IsEnabled = false, Jail_Shock = false;
        public static int Jail_Size = 8;
        public static string Command_set = "setjail", Command_forgive = "forgive";
        public static string Jail_Position = "0,0,0";

        public static SortedDictionary<string, Vector3> ReleasePosition = new SortedDictionary<string, Vector3>();
        public static List<string> Jailed = new List<string>();

        public static void SetJail(ClientInfo _cInfo)
        {
            string[] _command1 = { Command_set };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command1, _cInfo))
            {
                Phrases.Dict.TryGetValue("Jail10", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    int x = (int)player.position.x;
                    int y = (int)player.position.y;
                    int z = (int)player.position.z;
                    string sposition = x + "," + y + "," + z;
                    Jail_Position = sposition;
                    Config.WriteXml();
                    Phrases.Dict.TryGetValue("Jail3", out string phrase);
                    phrase = phrase.Replace("{JailPosition}", Jail_Position);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        private static void PutPlayerInJail(ClientInfo _cInfo, ClientInfo _PlayertoJail)
        {
            string[] cords = Jail_Position.Split(',');
            int.TryParse(cords[0], out int x);
            int.TryParse(cords[1], out int y);
            int.TryParse(cords[2], out int z);
            _PlayertoJail.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
            Jailed.Add(_PlayertoJail.CrossplatformId.CombinedString);
            PersistentContainer.Instance.Players[_PlayertoJail.CrossplatformId.CombinedString].JailTime = 60;
            PersistentContainer.Instance.Players[_PlayertoJail.CrossplatformId.CombinedString].JailName= _PlayertoJail.playerName;
            PersistentContainer.Instance.Players[_PlayertoJail.CrossplatformId.CombinedString].JailDate = DateTime.Now;
            PersistentContainer.DataChange = true;
            Phrases.Dict.TryGetValue("Jail1", out string phrase);
            ChatHook.ChatMessage(_PlayertoJail, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            if (Jail_Shock)
            {
                Phrases.Dict.TryGetValue("Jail8", out phrase);
                ChatHook.ChatMessage(_PlayertoJail, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            Phrases.Dict.TryGetValue("Jail6", out phrase);
            phrase = phrase.Replace("{PlayerName}", _PlayertoJail.playerName);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void StatusCheck()
        {
            if (Jailed.Count > 0)
            {
                for (int i = 0; i < Jailed.Count; i++)
                {
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(Jailed[i]);
                    if (cInfo != null)
                    {
                        EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                        if (player.Spawned && player.IsAlive())
                        {
                            string[] cords = Jail_Position.Split(',');
                            int.TryParse(cords[0], out int x);
                            int.TryParse(cords[1], out int y);
                            int.TryParse(cords[2], out int z);
                            Vector3 vector = player.position;
                            if ((x - vector.x) * (x - vector.x) + (z - vector.z) * (z - vector.z) >= Jail_Size * Jail_Size) 
                            {
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                                if (Jail_Shock)
                                {
                                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("buff buffShocked", true));
                                    Phrases.Dict.TryGetValue("Jail9", out string phrase);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void Clear()
        {
            for (int i = 0; i < Jailed.Count; i++)
            {
                string id = Jailed[i];
                if (Jailed.Contains(id))
                {
                    int jailTime = PersistentContainer.Instance.Players[id].JailTime;
                    DateTime jailDate = PersistentContainer.Instance.Players[id].JailDate;
                    if (jailTime > 0)
                    {
                        TimeSpan varTime = DateTime.Now - jailDate;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int timepassed = (int)fractionalMinutes;
                        if (timepassed >= jailTime)
                        {
                            ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(id);
                            if (cInfo != null)
                            {
                                EntityPlayer player = GameManager.Instance.World.Players.dict[cInfo.entityId];
                                if (player.IsSpawned())
                                {
                                    Jailed.Remove(id);
                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailTime = 0;
                                    PersistentContainer.DataChange = true;
                                    EntityBedrollPositionList _position = player.SpawnPoints;
                                    if (_position.Count > 0)
                                    {
                                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), null, false));
                                    }
                                    else
                                    {
                                        Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
                                    }
                                    Phrases.Dict.TryGetValue("Jail2", out string phrase);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Jailed.Remove(id);
                                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailTime = 0;
                                PersistentContainer.DataChange = true;
                            }
                        }
                    }
                }
            }
        }

        public static void JailList()
        {
            for (int i = 0; i < PersistentContainer.Instance.Players.IDs.Count; i++)
            {
                string id = PersistentContainer.Instance.Players.IDs[i];
                PersistentPlayer p = PersistentContainer.Instance.Players[id];
                {
                    int jailTime = p.JailTime;
                    if (jailTime > 0 || jailTime == -1)
                    {
                        if (jailTime == -1)
                        {
                            Jailed.Add(id);
                        }
                        else
                        {
                            DateTime jailDate = p.JailDate;
                            TimeSpan varTime = DateTime.Now - jailDate;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int timepassed = (int)fractionalMinutes;
                            if (timepassed < jailTime)
                            {
                                Jailed.Add(id);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[id].JailTime = 0;
                                PersistentContainer.DataChange = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
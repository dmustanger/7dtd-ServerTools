using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class Jail
    {
        public static bool IsEnabled = false, Jail_Shock = false;
        public static int Jail_Size = 8;
        public static string Command_forgive = "forgive";
        public static string Jail_Position = "0,0,0";

        public static SortedDictionary<string, Vector3> ReleasePosition = new SortedDictionary<string, Vector3>();
        public static List<string> Jailed = new List<string>();

        public static void StatusCheck()
        {
            if (Jailed.Count > 0)
            {
                for (int i = 0; i < Jailed.Count; i++)
                {
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(Jailed[i]);
                    if (cInfo != null)
                    {
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
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
                            ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(id);
                            if (cInfo != null)
                            {
                                EntityPlayer player = GameManager.Instance.World.Players.dict[cInfo.entityId];
                                if (player.IsSpawned())
                                {
                                    Jailed.Remove(id);
                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailTime = 0;
                                    PersistentContainer.DataChange = true;
                                    Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
                                    Phrases.Dict.TryGetValue("Jail2", out string phrase);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Jailed.Remove(id);
                                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailTime = 0;
                                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailRelease = true;
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
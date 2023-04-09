using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class BloodMoans
    {
        public static bool IsEnabled = false;
        public static int Countdown = 20;

        public static void Exec()
        {
            List<ClientInfo> clients = GeneralOperations.ClientList();
            if (clients == null || clients.Count < 1)
            {
                return;
            }
            System.Random random = new System.Random();
            int audioNumber;
            for (int i = 0; i < clients.Count; i++)
            {
                ClientInfo cInfo = clients[i];
                if (cInfo != null)
                {
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                    if (player != null)
                    {
                        audioNumber = random.Next(1, 19);
                        switch (audioNumber)
                        {
                            case 1:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "harvest_animal", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 2:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "spideralert", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 3:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "glassdestroy", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 4:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "traderneutralcough", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 5:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "rustle", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 6:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "wolfsense", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 7:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "avalanche", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 8:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "hulkvomitattack", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 9:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "zombiefemalealert", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 10:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "close_shutters_metal", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 11:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "door_wood_open", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 12:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "stonehitglass", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 13:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "zombiemalealert", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 14:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "zombieferalalert", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 15:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "gib", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 16:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "zombiefemalescoutalert", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 17:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "door_metal_open", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                            case 18:
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(player.position, "breakleg", AudioRolloffMode.Linear, 20, cInfo.entityId));
                                break;
                        }
                    }
                }
            }
            Countdown = random.Next(20, 31);
        }
    }
}

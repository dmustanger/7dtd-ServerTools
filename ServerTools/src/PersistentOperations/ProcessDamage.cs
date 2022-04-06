using HarmonyLib;
using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class ProcessDamage
    {
        public static int lastEntityKilled;

        public static AccessTools.FieldRef<NetPackageDamageEntity, ushort> strength = AccessTools.FieldRefAccess<NetPackageDamageEntity, ushort>("strength");
        public static AccessTools.FieldRef<NetPackageDamageEntity, bool> bFatal = AccessTools.FieldRefAccess<NetPackageDamageEntity, bool>("bFatal");
        public static AccessTools.FieldRef<NetPackageDamageEntity, int> entityId = AccessTools.FieldRefAccess<NetPackageDamageEntity, int>("entityId");

        private static AccessTools.FieldRef<NetPackageDamageEntity, int> attackerEntityId = AccessTools.FieldRefAccess<NetPackageDamageEntity, int>("attackerEntityId");
        private static AccessTools.FieldRef<NetPackageDamageEntity, ItemValue> attackingItem = AccessTools.FieldRefAccess<NetPackageDamageEntity, ItemValue>("attackingItem");

        private static readonly string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);

        public static bool Exec(NetPackageDamageEntity __instance)
        {
            try
            {
                Entity victim = PersistentOperations.GetEntity(entityId(__instance));
                if (victim != null)
                {
                    Entity attacker = PersistentOperations.GetEntity(attackerEntityId(__instance));
                    if (attacker != null)
                    {
                        if (victim is EntityPlayer)
                        {
                            ClientInfo cInfoVictim = PersistentOperations.GetClientInfoFromEntityId(victim.entityId);
                            if (cInfoVictim != null)
                            {
                                EntityPlayer victimPlayer = victim as EntityPlayer;
                                if (attacker is EntityPlayer)
                                {
                                    ClientInfo cInfoAttacker = PersistentOperations.GetClientInfoFromEntityId(attacker.entityId);
                                    if (cInfoAttacker != null)
                                    {
                                        EntityPlayer attackingPlayer = attacker as EntityPlayer;
                                        if (attackingItem(__instance) != null)
                                        {
                                            if (DamageDetector.IsEnabled && !DamageDetector.IsValidPvP(victim as EntityPlayer, cInfoAttacker, strength(__instance), attackingItem(__instance)))
                                            {
                                                return true;
                                            }
                                            if (InfiniteAmmo.IsEnabled && attackingItem(__instance).ItemClass.IsGun())
                                            {
                                                int slot = attackingPlayer.inventory.holdingItemIdx;
                                                if (InfiniteAmmo.Exec(cInfoAttacker, attackingPlayer, slot, attackingItem(__instance)))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                        if (NewPlayerProtection.IsEnabled)
                                        {
                                            if (NewPlayerProtection.IsProtected(victimPlayer))
                                            {
                                                Phrases.Dict.TryGetValue("NewPlayerProtection2", out string phrase);
                                                ChatHook.ChatMessage(cInfoAttacker, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return true;
                                            }
                                            else if (NewPlayerProtection.IsProtected(attackingPlayer))
                                            {
                                                Phrases.Dict.TryGetValue("NewPlayerProtection1", out string phrase);
                                                ChatHook.ChatMessage(cInfoVictim, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return true;
                                            }
                                        }
                                        if (Zones.IsEnabled && PersistentOperations.Player_Killing_Mode == 3 && !Zones.IsValid(cInfoVictim, cInfoAttacker))
                                        {
                                            return true;
                                        }
                                        if (Lobby.IsEnabled && Lobby.PvE && (Lobby.LobbyPlayers.Contains(victimPlayer.entityId) || Lobby.LobbyPlayers.Contains(attackingPlayer.entityId)))
                                        {
                                            Lobby.PvEViolation(cInfoAttacker);
                                            return true;
                                        }
                                        if (Market.IsEnabled && Market.PvE && (Market.MarketPlayers.Contains(victimPlayer.entityId) || Market.MarketPlayers.Contains(attackingPlayer.entityId)))
                                        {
                                            Market.PvEViolation(cInfoAttacker);
                                            return true;
                                        }
                                        float distance = attackingPlayer.GetDistance(victimPlayer);
                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' @ '{4}' hit '{5}' '{6}' named '{7}' @ '{8}' using '{9}' for '{10}' damage. Distance '{11}'", DateTime.Now, cInfoAttacker.PlatformId.CombinedString, cInfoAttacker.CrossplatformId.CombinedString, cInfoAttacker.playerName, cInfoAttacker.latestPlayerData.ecd.pos, cInfoVictim.PlatformId.CombinedString, cInfoVictim.CrossplatformId.CombinedString, cInfoVictim.playerName, cInfoVictim.latestPlayerData.ecd.pos, attackingItem(__instance).ItemClass.GetLocalizedItemName() ?? attackingItem(__instance).ItemClass.GetItemName(), strength(__instance), distance));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        if (bFatal(__instance) && victimPlayer.IsAlive() && lastEntityKilled != victimPlayer.entityId)
                                        {
                                            lastEntityKilled = victimPlayer.entityId;
                                            if (KillNotice.IsEnabled && KillNotice.PvP)
                                            {
                                                KillNotice.PlayerKilledPlayer(cInfoVictim, victimPlayer, cInfoAttacker, attackingPlayer, attackingItem(__instance), strength(__instance));
                                            }
                                            if (Bounties.IsEnabled)
                                            {
                                                Bounties.PlayerKilled(victimPlayer, attackingPlayer, cInfoVictim, cInfoAttacker);
                                            }
                                            if (Wallet.IsEnabled && Wallet.PVP && Wallet.Player_Kill > 0)
                                            {
                                                Wallet.AddCurrency(cInfoAttacker.CrossplatformId.CombinedString, Wallet.Player_Kill);
                                            }
                                            if (MagicBullet.IsEnabled && !MagicBullet.Kill.Contains(cInfoAttacker.entityId))
                                            {
                                                MagicBullet.Kill.Add(cInfoAttacker.entityId);
                                            }
                                        }
                                    }
                                }
                                else if (attacker is EntityZombie)
                                {
                                    if (NewPlayerProtection.IsEnabled && NewPlayerProtection.IsProtected(victimPlayer))
                                    {
                                        return true;
                                    }
                                    if (KillNotice.IsEnabled && KillNotice.Zombie_Kills)
                                    {
                                        int[] attack = new int[] { attacker.entityId, strength(__instance) };
                                        if (KillNotice.Damage.ContainsKey(victim.entityId))
                                        {
                                            KillNotice.Damage[victim.entityId] = attack;
                                        }
                                        else
                                        {
                                            KillNotice.Damage.Add(victim.entityId, attack);
                                        }
                                    }
                                }
                                else if (attacker is EntityAnimal)
                                {
                                    if (NewPlayerProtection.IsEnabled && NewPlayerProtection.IsProtected(victimPlayer))
                                    {
                                        return true;
                                    }
                                    if (KillNotice.IsEnabled && KillNotice.Animal_Kills)
                                    {
                                        int[] attack = new int[] { attacker.entityId, strength(__instance) };
                                        if (KillNotice.Damage.ContainsKey(victim.entityId))
                                        {
                                            KillNotice.Damage[victim.entityId] = attack;
                                        }
                                        else
                                        {
                                            KillNotice.Damage.Add(victim.entityId, attack);
                                        }
                                    }
                                }
                            }
                        }
                        else if (victim is EntityZombie && attacker is EntityPlayer)
                        {
                            ClientInfo cInfoAttacker = PersistentOperations.GetClientInfoFromEntityId(attacker.entityId);
                            if (cInfoAttacker != null)
                            {
                                EntityPlayer attackingPlayer = attacker as EntityPlayer;
                                if (attackingItem(__instance) != null)
                                {
                                    if (DamageDetector.IsEnabled && !DamageDetector.IsValidEntityDamage(attackingPlayer, cInfoAttacker, strength(__instance), attackingItem(__instance)))
                                    {
                                        return true;
                                    }
                                    if (InfiniteAmmo.IsEnabled && attackingItem(__instance).ItemClass.IsGun())
                                    {
                                        int slot = attackingPlayer.inventory.holdingItemIdx;
                                        if (InfiniteAmmo.Exec(cInfoAttacker, attackingPlayer, slot, attackingItem(__instance)))
                                        {
                                            return true;
                                        }
                                    }
                                    if (bFatal(__instance) && victim.IsAlive() && lastEntityKilled != victim.entityId)
                                    {
                                        lastEntityKilled = victim.entityId;
                                        if (Wallet.IsEnabled && Wallet.Zombie_Kill > 0)
                                        {
                                            Wallet.AddCurrency(cInfoAttacker.CrossplatformId.CombinedString, Wallet.Zombie_Kill);
                                        }
                                        if (BloodmoonWarrior.IsEnabled && BloodmoonWarrior.BloodmoonStarted && BloodmoonWarrior.WarriorList.Contains(cInfoAttacker.entityId))
                                        {
                                            if (BloodmoonWarrior.KilledZombies.TryGetValue(cInfoAttacker.entityId, out int killedZ))
                                            {
                                                BloodmoonWarrior.KilledZombies[cInfoAttacker.entityId] += 1;
                                            }
                                            else
                                            {
                                                BloodmoonWarrior.KilledZombies.Add(cInfoAttacker.entityId, 1);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    return true;
                                }
                                if (PersistentOperations.IsBloodmoon() && Market.IsEnabled && Market.MarketPlayers.Contains(cInfoAttacker.entityId))
                                {
                                    Phrases.Dict.TryGetValue("Market12", out string phrase);
                                    ChatHook.ChatMessage(cInfoAttacker, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return true;
                                }
                                if (PersistentOperations.IsBloodmoon() && Lobby.IsEnabled && Lobby.LobbyPlayers.Contains(cInfoAttacker.entityId))
                                {
                                    Phrases.Dict.TryGetValue("Lobby12", out string phrase);
                                    ChatHook.ChatMessage(cInfoAttacker, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return true;
                                }
                                int distance = (int)attackingPlayer.GetDistance(victim);
                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' @ '{4}' hit '{5}' named '{6}' @ '{7}' using '{8}' for '{9}' damage. Distance '{10}'", DateTime.Now, cInfoAttacker.PlatformId.CombinedString, cInfoAttacker.CrossplatformId.CombinedString, cInfoAttacker.playerName, cInfoAttacker.latestPlayerData.ecd.pos, victim.entityId, victim.EntityClass.entityClassName, victim.position, attackingItem(__instance).ItemClass.GetLocalizedItemName() ?? attackingItem(__instance).ItemClass.GetItemName(), strength(__instance), distance));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                        }
                        else if (victim is EntityAnimal && attacker is EntityPlayer)
                        {
                            ClientInfo cInfoAttacker = PersistentOperations.GetClientInfoFromEntityId(attacker.entityId);
                            if (cInfoAttacker != null)
                            {
                                EntityPlayer attackingPlayer = attacker as EntityPlayer;
                                if (attackingItem(__instance) != null)
                                {
                                    if (DamageDetector.IsEnabled && !DamageDetector.IsValidEntityDamage(attackingPlayer, cInfoAttacker, strength(__instance), attackingItem(__instance)))
                                    {
                                        return true;
                                    }
                                    if (InfiniteAmmo.IsEnabled && attackingItem(__instance).ItemClass.IsGun())
                                    {
                                        int slot = attackingPlayer.inventory.holdingItemIdx;
                                        if (InfiniteAmmo.Exec(cInfoAttacker, attackingPlayer, slot, attackingItem(__instance)))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProcessDamage.Exec: {0}", e.Message));
            }
            return false;
        }
    }
}

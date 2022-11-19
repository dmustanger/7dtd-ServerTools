using HarmonyLib;
using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class ProcessDamage
    {
        public static int lastEntityKilled;

        public static AccessTools.FieldRef<NetPackageDamageEntity, ushort> Strength = AccessTools.FieldRefAccess<NetPackageDamageEntity, ushort>("strength");
        public static AccessTools.FieldRef<NetPackageDamageEntity, bool> BFatal = AccessTools.FieldRefAccess<NetPackageDamageEntity, bool>("bFatal");
        public static AccessTools.FieldRef<NetPackageDamageEntity, int> EntityId = AccessTools.FieldRefAccess<NetPackageDamageEntity, int>("entityId");
        public static AccessTools.FieldRef<NetPackageDamageEntity, int> AttackerEntityId = AccessTools.FieldRefAccess<NetPackageDamageEntity, int>("attackerEntityId");
        public static AccessTools.FieldRef<NetPackageDamageEntity, EnumDamageTypes> DamageTyp = AccessTools.FieldRefAccess<NetPackageDamageEntity, EnumDamageTypes>("damageTyp");
        public static AccessTools.FieldRef<NetPackageDamageEntity, int> HitBodyPart = AccessTools.FieldRefAccess<NetPackageDamageEntity, int>("hitBodyPart");
        public static AccessTools.FieldRef<NetPackageDamageEntity, ItemValue> AttackingItem = AccessTools.FieldRefAccess<NetPackageDamageEntity, ItemValue>("attackingItem");

        private static readonly string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);

        public static bool Exec(NetPackageDamageEntity __instance, Entity _victim, Entity _attacker)
        {
            try
            {
                if (_victim is EntityPlayer && _victim.IsAlive())
                {
                    ClientInfo cInfoVictim = GeneralFunction.GetClientInfoFromEntityId(_victim.entityId);
                    if (cInfoVictim != null)
                    {
                        EntityPlayer victimPlayer = _victim as EntityPlayer;
                        if (_attacker is EntityPlayer && victimPlayer.isEntityRemote)
                        {
                            ClientInfo cInfoAttacker = GeneralFunction.GetClientInfoFromEntityId(_attacker.entityId);
                            if (cInfoAttacker != null)
                            {
                                EntityPlayer attackingPlayer = _attacker as EntityPlayer;
                                if (AttackingItem(__instance) != null)
                                {
                                    if (DamageDetector.IsEnabled)
                                    {
                                        if (DamageDetector.LogEnabled)
                                        {
                                            float distance = attackingPlayer.GetDistance(victimPlayer);
                                            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                            {
                                                sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' @ '{4}' hit '{5}' '{6}' named '{7}' @ '{8}' using '{9}' for '{10}' damage. Distance '{11}'", DateTime.Now, cInfoAttacker.PlatformId.CombinedString, cInfoAttacker.CrossplatformId.CombinedString, cInfoAttacker.playerName, cInfoAttacker.latestPlayerData.ecd.pos, cInfoVictim.PlatformId.CombinedString, cInfoVictim.CrossplatformId.CombinedString, cInfoVictim.playerName, cInfoVictim.latestPlayerData.ecd.pos, AttackingItem(__instance).ItemClass.GetLocalizedItemName() ?? AttackingItem(__instance).ItemClass.GetItemName(), Strength(__instance), distance));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                        }
                                        if (!DamageDetector.IsValidPvP(_victim as EntityPlayer, cInfoAttacker, Strength(__instance), AttackingItem(__instance)))
                                        {
                                            return true;
                                        }
                                    }
                                    if (InfiniteAmmo.IsEnabled && AttackingItem(__instance).ItemClass != null && AttackingItem(__instance).ItemClass.IsGun())
                                    {
                                        int slot = attackingPlayer.inventory.holdingItemIdx;
                                        InfiniteAmmo.Exec(cInfoAttacker, attackingPlayer, slot, AttackingItem(__instance));
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
                                if (BFatal(__instance) && victimPlayer.IsAlive() && lastEntityKilled != victimPlayer.entityId)
                                {
                                    lastEntityKilled = victimPlayer.entityId;
                                    if (KillNotice.IsEnabled && KillNotice.PvP)
                                    {
                                        KillNotice.PlayerKilledPlayer(cInfoVictim, victimPlayer, cInfoAttacker, attackingPlayer, AttackingItem(__instance), Strength(__instance));
                                    }
                                    if (Bounties.IsEnabled)
                                    {
                                        Bounties.PlayerKilled(victimPlayer, attackingPlayer, cInfoVictim, cInfoAttacker);
                                    }
                                    if (Wallet.IsEnabled && Wallet.PVP && Wallet.Player_Kill > 0)
                                    {
                                        Wallet.AddCurrency(cInfoAttacker.CrossplatformId.CombinedString, Wallet.Player_Kill, true);
                                    }
                                    if (MagicBullet.IsEnabled && !MagicBullet.Kill.Contains(cInfoAttacker.entityId))
                                    {
                                        MagicBullet.Kill.Add(cInfoAttacker.entityId);
                                    }
                                }
                            }
                        }
                        else if (_attacker is EntityZombie || _attacker is EntityAnimal || !_attacker.isEntityRemote)
                        {
                            if (NewPlayerProtection.IsEnabled && NewPlayerProtection.IsProtected(victimPlayer))
                            {
                                return true;
                            }
                            if (KillNotice.IsEnabled && KillNotice.Other)
                            {
                                int[] attack = new int[] { _attacker.entityId, Strength(__instance) };
                                if (KillNotice.Damage.ContainsKey(_victim.entityId))
                                {
                                    KillNotice.Damage[_victim.entityId] = attack;
                                }
                                else
                                {
                                    KillNotice.Damage.Add(_victim.entityId, attack);
                                }
                            }
                        }
                    }
                }
                else if (_victim is EntityZombie && _victim.IsAlive() && _attacker is EntityPlayer)
                {
                    ClientInfo cInfoAttacker = GeneralFunction.GetClientInfoFromEntityId(_attacker.entityId);
                    if (cInfoAttacker != null)
                    {
                        EntityPlayer attackingPlayer = _attacker as EntityPlayer;
                        if (AttackingItem(__instance) != null)
                        {
                            if (DamageDetector.IsEnabled)
                            {
                                if (DamageDetector.LogEnabled)
                                {
                                    float distance = attackingPlayer.GetDistance(_victim);
                                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                    {
                                        sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' @ '{4}' hit '{5}' named '{6}' @ '{7}' using '{8}' for '{9}' damage. Distance '{10}'", DateTime.Now, cInfoAttacker.PlatformId.CombinedString, cInfoAttacker.CrossplatformId.CombinedString, cInfoAttacker.playerName, cInfoAttacker.latestPlayerData.ecd.pos, _victim.entityId, _victim.EntityClass.entityClassName, _victim.position, AttackingItem(__instance).ItemClass.GetLocalizedItemName() ?? AttackingItem(__instance).ItemClass.GetItemName(), Strength(__instance), distance));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                }
                                if (!DamageDetector.IsValidEntityDamage(attackingPlayer, cInfoAttacker, Strength(__instance), AttackingItem(__instance)))
                                {
                                    return true;
                                }
                            }
                            if (InfiniteAmmo.IsEnabled && AttackingItem(__instance).ItemClass != null && AttackingItem(__instance).ItemClass.IsGun())
                            {
                                int slot = attackingPlayer.inventory.holdingItemIdx;
                                InfiniteAmmo.Exec(cInfoAttacker, attackingPlayer, slot, AttackingItem(__instance));
                            }
                            if (BFatal(__instance) && _victim.IsAlive() && lastEntityKilled != _victim.entityId)
                            {
                                lastEntityKilled = _victim.entityId;
                                if (Wallet.IsEnabled && Wallet.Zombie_Kill > 0)
                                {
                                    Wallet.AddCurrency(cInfoAttacker.CrossplatformId.CombinedString, Wallet.Zombie_Kill, true);
                                }
                                if (BloodmoonWarrior.IsEnabled && BloodmoonWarrior.BloodmoonStarted && BloodmoonWarrior.WarriorList.Contains(cInfoAttacker.entityId))
                                {
                                    BloodmoonWarrior.KilledZombies[cInfoAttacker.entityId] += 1;
                                }
                            }
                        }
                        else
                        {
                            return true;
                        }
                        if (Market.IsEnabled && Market.Damage_Z && GeneralFunction.IsBloodmoon() && Market.MarketPlayers.Contains(cInfoAttacker.entityId))
                        {
                            Phrases.Dict.TryGetValue("Market12", out string phrase);
                            ChatHook.ChatMessage(cInfoAttacker, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return true;
                        }
                        if (Lobby.IsEnabled && Lobby.Damage_Z && GeneralFunction.IsBloodmoon() && Lobby.LobbyPlayers.Contains(cInfoAttacker.entityId))
                        {
                            Phrases.Dict.TryGetValue("Lobby12", out string phrase);
                            ChatHook.ChatMessage(cInfoAttacker, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return true;
                        }
                    }
                }
                else if (_victim is EntityAnimal && _victim.IsAlive() && _attacker is EntityPlayer)
                {
                    ClientInfo cInfoAttacker = GeneralFunction.GetClientInfoFromEntityId(_attacker.entityId);
                    if (cInfoAttacker != null)
                    {
                        EntityPlayer attackingPlayer = _attacker as EntityPlayer;
                        if (AttackingItem(__instance) != null)
                        {
                            if (DamageDetector.IsEnabled && !DamageDetector.IsValidEntityDamage(attackingPlayer, cInfoAttacker, Strength(__instance), AttackingItem(__instance)))
                            {
                                return true;
                            }
                            if (InfiniteAmmo.IsEnabled && AttackingItem(__instance).ItemClass != null && AttackingItem(__instance).ItemClass.IsGun())
                            {
                                int slot = attackingPlayer.inventory.holdingItemIdx;
                                InfiniteAmmo.Exec(cInfoAttacker, attackingPlayer, slot, AttackingItem(__instance));
                            }
                        }
                    }
                }
                if (BFatal(__instance) && KillNotice.IsEnabled && KillNotice.Misc && _victim.IsAlive())
                {
                    lastEntityKilled = _victim.entityId;
                    ClientInfo cInfoVictim = GeneralFunction.GetClientInfoFromEntityId(_victim.entityId);
                    if (cInfoVictim != null)
                    {
                        KillNotice.MiscDeath(cInfoVictim, DamageTyp(__instance));
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

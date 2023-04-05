using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class ProcessDamage
    {
        private static readonly string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);

        public static bool Exec(Entity _victim, Entity _attacker, ItemValue ___attackingItem, ushort ___strength)
        {
            try
            {
                if (_victim is EntityPlayer && _victim.IsAlive())
                {
                    ClientInfo cInfoVictim = GeneralOperations.GetClientInfoFromEntityId(_victim.entityId);
                    if (cInfoVictim != null)
                    {
                        EntityPlayer victimPlayer = _victim as EntityPlayer;
                        if (_attacker is EntityPlayer)
                        {
                            ClientInfo cInfoAttacker = GeneralOperations.GetClientInfoFromEntityId(_attacker.entityId);
                            if (cInfoAttacker != null)
                            {
                                EntityPlayer attackingPlayer = _attacker as EntityPlayer;
                                if (___attackingItem != null)
                                {
                                    if (DamageDetector.LogEnabled)
                                    {
                                        float distance = attackingPlayer.GetDistance(victimPlayer);
                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' @ '{4}' hit '{5}' '{6}' named '{7}' @ '{8}' using '{9}' for '{10}' damage. Distance '{11}'", DateTime.Now, cInfoAttacker.PlatformId.CombinedString, cInfoAttacker.CrossplatformId.CombinedString, cInfoAttacker.playerName, cInfoAttacker.latestPlayerData.ecd.pos, cInfoVictim.PlatformId.CombinedString, cInfoVictim.CrossplatformId.CombinedString, cInfoVictim.playerName, cInfoVictim.latestPlayerData.ecd.pos, ___attackingItem.ItemClass.GetLocalizedItemName() ?? ___attackingItem.ItemClass.GetItemName(), ___strength, distance));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                    }
                                    if (DamageDetector.IsEnabled && !DamageDetector.IsValidPvP(_victim as EntityPlayer, cInfoAttacker, ___strength, ___attackingItem))
                                    {
                                        return false;
                                    }
                                    if (InfiniteAmmo.IsEnabled && ___attackingItem.ItemClass != null && ___attackingItem.ItemClass.IsGun())
                                    {
                                        int slot = attackingPlayer.inventory.holdingItemIdx;
                                        InfiniteAmmo.Exec(cInfoAttacker, attackingPlayer, slot, ___attackingItem);
                                    }
                                    if (Bounties.IsEnabled)
                                    {
                                        Bounties.PlayerKilled(victimPlayer, attackingPlayer, cInfoVictim, cInfoAttacker);
                                    }
                                    if (Wallet.IsEnabled && Wallet.PVP && Wallet.Player_Kill > 0)
                                    {
                                        Wallet.AddCurrency(cInfoAttacker.CrossplatformId.CombinedString, Wallet.Player_Kill, true);
                                    }
                                }
                                if (NewPlayerProtection.IsEnabled)
                                {
                                    if (NewPlayerProtection.IsProtected(victimPlayer))
                                    {
                                        Phrases.Dict.TryGetValue("NewPlayerProtection2", out string phrase);
                                        ChatHook.ChatMessage(cInfoAttacker, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        return false;
                                    }
                                    else if (NewPlayerProtection.IsProtected(attackingPlayer))
                                    {
                                        Phrases.Dict.TryGetValue("NewPlayerProtection1", out string phrase);
                                        ChatHook.ChatMessage(cInfoVictim, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        return false;
                                    }
                                }
                                if (Lobby.IsEnabled && Lobby.PvE && Lobby.IsLobby(victimPlayer.position) || Lobby.IsLobby(attackingPlayer.position))
                                {
                                    Lobby.PvEViolation(cInfoAttacker);
                                    return false;
                                }
                                if (Market.IsEnabled && Market.PvE && Market.IsMarket(victimPlayer.position) || Market.IsMarket(attackingPlayer.position))
                                {
                                    Market.PvEViolation(cInfoAttacker);
                                    return false;
                                }
                            }
                        }
                        else if (NewPlayerProtection.IsEnabled && NewPlayerProtection.IsProtected(victimPlayer))
                        {
                            return false;
                        }
                    }
                }
                else if (_victim is EntityZombie && _attacker is EntityPlayer)
                {
                    ClientInfo cInfoAttacker = GeneralOperations.GetClientInfoFromEntityId(_attacker.entityId);
                    if (cInfoAttacker != null)
                    {
                        EntityPlayer attackingPlayer = _attacker as EntityPlayer;
                        if (___attackingItem != null)
                        {
                            if (DamageDetector.LogEnabled)
                            {
                                float distance = attackingPlayer.GetDistance(_victim);
                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' @ '{4}' hit '{5}' named '{6}' @ '{7}' using '{8}' for '{9}' damage. Distance '{10}'", DateTime.Now, cInfoAttacker.PlatformId.CombinedString, cInfoAttacker.CrossplatformId.CombinedString, cInfoAttacker.playerName, cInfoAttacker.latestPlayerData.ecd.pos, _victim.entityId, _victim.EntityClass.entityClassName, _victim.position, ___attackingItem.ItemClass.GetLocalizedItemName() ?? ___attackingItem.ItemClass.GetItemName(), ___strength, distance));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                            if (DamageDetector.IsEnabled && !DamageDetector.IsValidEntityDamage(attackingPlayer, cInfoAttacker, ___strength, ___attackingItem))
                            {
                                return false;
                            }
                            if (InfiniteAmmo.IsEnabled && ___attackingItem.ItemClass != null && ___attackingItem.ItemClass.IsGun())
                            {
                                int slot = attackingPlayer.inventory.holdingItemIdx;
                                InfiniteAmmo.Exec(cInfoAttacker, attackingPlayer, slot, ___attackingItem);
                            }
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
                }
                else if (_victim is EntityAnimal && _attacker is EntityPlayer)
                {
                    ClientInfo cInfoAttacker = GeneralOperations.GetClientInfoFromEntityId(_attacker.entityId);
                    if (cInfoAttacker != null)
                    {
                        EntityPlayer attackingPlayer = _attacker as EntityPlayer;
                        if (___attackingItem != null)
                        {
                            if (DamageDetector.LogEnabled)
                            {
                                float distance = attackingPlayer.GetDistance(_victim);
                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' @ '{4}' hit '{5}' named '{6}' @ '{7}' using '{8}' for '{9}' damage. Distance '{10}'", DateTime.Now, cInfoAttacker.PlatformId.CombinedString, cInfoAttacker.CrossplatformId.CombinedString, cInfoAttacker.playerName, cInfoAttacker.latestPlayerData.ecd.pos, _victim.entityId, _victim.EntityClass.entityClassName, _victim.position, ___attackingItem.ItemClass.GetLocalizedItemName() ?? ___attackingItem.ItemClass.GetItemName(), ___strength, distance));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                            if (DamageDetector.IsEnabled && !DamageDetector.IsValidEntityDamage(attackingPlayer, cInfoAttacker, ___strength, ___attackingItem))
                            {
                                return false;
                            }
                            if (InfiniteAmmo.IsEnabled && ___attackingItem.ItemClass != null && ___attackingItem.ItemClass.IsGun())
                            {
                                int slot = attackingPlayer.inventory.holdingItemIdx;
                                InfiniteAmmo.Exec(cInfoAttacker, attackingPlayer, slot, ___attackingItem);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProcessDamage.Exec: {0}", e.Message));
            }
            return true;
        }
    }
}

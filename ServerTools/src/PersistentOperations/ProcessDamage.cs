using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class ProcessDamage
    {
        public static int lastEntityKilled;

        private static readonly string file = string.Format("DamageLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/DamageLogs/{1}", API.ConfigPath, file);

        public static bool Exec(EntityAlive _victim, DamageSource _damageSource, int _strength)
        {
            try
            {
                if (_victim != null && _damageSource != null && _strength > 1)
                {
                    int sourceId = _damageSource.getEntityId();
                    if (sourceId > 0)
                    {
                        if (_victim is EntityPlayer)
                        {
                            EntityPlayer player1 = _victim as EntityPlayer;
                            ClientInfo cInfo1 = PersistentOperations.GetClientInfoFromEntityId(_victim.entityId);
                            if (cInfo1 != null)
                            {
                                Entity attackingEntity = PersistentOperations.GetEntity(sourceId);
                                if (attackingEntity != null)
                                {
                                    if (attackingEntity is EntityPlayer)
                                    {
                                        EntityPlayer player2 = attackingEntity as EntityPlayer;
                                        ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromEntityId(attackingEntity.entityId);
                                        if (cInfo2 != null)
                                        {
                                            if (_damageSource.AttackingItem != null)
                                            {
                                                int slot = player2.inventory.holdingItemIdx;
                                                ItemValue itemValue = cInfo2.latestPlayerData.inventory[slot].itemValue;
                                                if (DamageDetector.IsEnabled && !DamageDetector.IsValidPvP(player1, cInfo2, _strength, _damageSource.AttackingItem))
                                                {
                                                    return false;
                                                }
                                                if (_damageSource.AttackingItem.ItemClass.IsGun() && InfiniteAmmo.Exec(cInfo2, player2, slot, itemValue))
                                                {
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                            if (NewPlayerProtection.IsEnabled)
                                            {
                                                if (player1.Progression.Level < NewPlayerProtection.Level)
                                                {
                                                    Phrases.Dict.TryGetValue("NewPlayerProtection1", out string phrase);
                                                    ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                    return false;
                                                }
                                                else if (player2.Progression.Level < NewPlayerProtection.Level)
                                                {
                                                    Phrases.Dict.TryGetValue("NewPlayerProtection2", out string phrase);
                                                    ChatHook.ChatMessage(cInfo1, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                    return false;
                                                }
                                            }
                                            if (Zones.IsEnabled && PersistentOperations.Player_Killing_Mode == 3 && !Zones.IsValid(cInfo1, cInfo2))
                                            {
                                                return false;
                                            }
                                            if (Lobby.IsEnabled && Lobby.PvE && (Lobby.LobbyPlayers.Contains(player1.entityId) || Lobby.LobbyPlayers.Contains(player2.entityId)))
                                            {
                                                Lobby.PvEViolation(cInfo2);
                                                return false;
                                            }
                                            if (Market.IsEnabled && Market.PvE && (Market.MarketPlayers.Contains(player1.entityId) || Market.MarketPlayers.Contains(player2.entityId)))
                                            {
                                                Market.PvEViolation(cInfo2);
                                                return false;
                                            }
                                            float distance = player2.GetDistance(player1);
                                            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                            {
                                                sw.WriteLine(string.Format("{0}: '{1}' named '{2}' hit '{3}' named '{4}' using '{5}' for '{6}' damage @ '{7}' total distance '{8}'", DateTime.Now, cInfo2.playerId, cInfo2.playerName, cInfo1.playerId, cInfo1.playerName, _damageSource.AttackingItem.ItemClass.GetLocalizedItemName() ?? _damageSource.AttackingItem.ItemClass.GetItemName(), _strength, player1.position, distance));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            if (_victim.RecordedDamage.Fatal || _strength >= _victim.Health && lastEntityKilled != _victim.entityId)
                                            {
                                                lastEntityKilled = _victim.entityId;
                                                if (KillNotice.IsEnabled && KillNotice.PvP)
                                                {
                                                    KillNotice.PlayerKilledPlayer(cInfo1, player1, cInfo2, player2, _damageSource.AttackingItem, _strength);
                                                }
                                                if (Bounties.IsEnabled)
                                                {
                                                    Bounties.PlayerKilled(player1, player2, cInfo1, cInfo2);
                                                }
                                                if (Wallet.IsEnabled && Wallet.PVP && Wallet.Player_Kill > 0)
                                                {
                                                    Wallet.AddCurrency(cInfo2.playerId, Wallet.Player_Kill);
                                                }
                                            }
                                        }
                                    }
                                    else if (attackingEntity is EntityZombie)
                                    {
                                        if (KillNotice.IsEnabled && KillNotice.Zombie_Kills)
                                        {
                                            int[] attack = new int[] { attackingEntity.entityId, _strength };
                                            if (KillNotice.ZombieDamage.ContainsKey(_victim.entityId))
                                            {
                                                KillNotice.ZombieDamage[_victim.entityId] = attack;
                                            }
                                            else
                                            {
                                                KillNotice.ZombieDamage.Add(_victim.entityId, attack);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (_victim is EntityZombie)
                        {
                            ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(sourceId);
                            if (cInfo != null)
                            {
                                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.playerId);
                                if (player != null)
                                {
                                    if (_damageSource.AttackingItem != null)
                                    {
                                        int slot = player.inventory.holdingItemIdx;
                                        ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                                        if (DamageDetector.IsEnabled && !DamageDetector.IsValidEntityDamage(_victim, cInfo, _strength, _damageSource.AttackingItem))
                                        {
                                            return false;
                                        }
                                        if (_damageSource.AttackingItem.ItemClass.IsGun() && InfiniteAmmo.Exec(cInfo, player, slot, itemValue))
                                        {
                                            return false;
                                        }
                                        if ((_victim.RecordedDamage.Fatal || _strength >= _victim.Health) && Wallet.IsEnabled && Wallet.Zombie_Kill > 0 && lastEntityKilled != _victim.entityId)
                                        {
                                            lastEntityKilled = _victim.entityId;
                                            Wallet.AddCurrency(cInfo.playerId, Wallet.Zombie_Kill);
                                            if (BloodmoonWarrior.IsEnabled && BloodmoonWarrior.BloodmoonStarted && BloodmoonWarrior.WarriorList.Contains(cInfo.playerId))
                                            {
                                                if (BloodmoonWarrior.KilledZombies.TryGetValue(cInfo.playerId, out int killedZ))
                                                {
                                                    BloodmoonWarrior.KilledZombies[cInfo.playerId] += 1;
                                                }
                                                else
                                                {
                                                    BloodmoonWarrior.KilledZombies.Add(cInfo.playerId, 1);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    if (PersistentOperations.IsBloodmoon() && Market.IsEnabled && Market.MarketPlayers.Contains(cInfo.entityId))
                                    {
                                        Phrases.Dict.TryGetValue("Market12", out string _phrase);
                                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        return false;
                                    }
                                    if (PersistentOperations.IsBloodmoon() && Lobby.IsEnabled && Lobby.LobbyPlayers.Contains(cInfo.entityId))
                                    {
                                        Phrases.Dict.TryGetValue("Lobby12", out string _phrase);
                                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        return false;
                                    }
                                    int distance = (int)player.GetDistance(_victim);
                                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                    {
                                        sw.WriteLine(string.Format("{0}: {1} \"{2}\" hit \"{3}\" with entity id {4} using {5} for {6} damage @ {7}. Distance: {8}", DateTime.Now, cInfo.playerId, cInfo.playerName, _victim.EntityName, _victim.entityId, _damageSource.AttackingItem.ItemClass.GetLocalizedItemName() ?? _damageSource.AttackingItem.ItemClass.GetItemName(), _strength, _victim.position, distance));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                }
                            }
                        }
                        else if (_victim is EntityAnimal)
                        {
                            ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(sourceId);
                            if (cInfo != null)
                            {
                                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.playerId);
                                if (player != null && _damageSource.AttackingItem != null)
                                {
                                    int slot = player.inventory.holdingItemIdx;
                                    ItemValue itemValue = cInfo.latestPlayerData.inventory[slot].itemValue;
                                    if (DamageDetector.IsEnabled && !DamageDetector.IsValidEntityDamage(_victim, cInfo, _strength, itemValue))
                                    {
                                        return false;
                                    }
                                    if (itemValue.ItemClass.IsGun() && InfiniteAmmo.Exec(cInfo, player, slot, itemValue))
                                    {
                                        return false;
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
            return true;
        }
    }
}

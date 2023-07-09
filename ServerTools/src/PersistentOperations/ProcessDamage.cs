using System;

namespace ServerTools
{
    class ProcessDamage
    {
        private static ClientInfo cInfoVictim, cInfoAttacker;
        private static string newEntry;

        public static bool Exec(Entity _victim, Entity _attacker, ItemValue ___attackingItem, ushort ___strength, EnumDamageTypes ___damageTyp, EnumBodyPartHit ___hitBodyPart, bool ___bCritical, bool ___bFatal)
        {
            try
            {
                if (_victim == null || _attacker == null || ___attackingItem == null)
                {
                    return true;
                }
                if (_victim is EntityPlayer)
                {
                    cInfoVictim = GeneralOperations.GetClientInfoFromEntityId(_victim.entityId);
                    if (cInfoVictim == null)
                    {
                        return true;
                    }
                    if (_attacker is EntityPlayer)
                    {
                        cInfoAttacker = GeneralOperations.GetClientInfoFromEntityId(_attacker.entityId);
                        if (cInfoAttacker == null)
                        {
                            return true;
                        }
                        if (DamageDetector.IsEnabled)
                        {
                            if (!DamageDetector.IsValidPlayerDamage(_victim, cInfoAttacker, ___strength, ___attackingItem))
                            {
                                return false;
                            }
                            if (DamageDetector.LogEnabled)
                            {
                                float distance = _attacker.GetDistance(_victim);
                                newEntry = string.Format("{0}: '{1}' '{2}' named '{3}' @ '{4}' '{5}' '{6}' '{7}' named '{8}' @ '{9}' using '{10}' for '{11}' damage in the '{12}'. Distance '{13}' Critical '{14}' Fatal '{15}'", DateTime.Now, cInfoAttacker.PlatformId.CombinedString, cInfoAttacker.CrossplatformId.CombinedString, cInfoAttacker.playerName, _attacker.serverPos, ___damageTyp.ToString(), cInfoVictim.PlatformId.CombinedString, cInfoVictim.CrossplatformId.CombinedString, cInfoVictim.playerName, _victim.serverPos, ___attackingItem.ItemClass.GetLocalizedItemName() ?? ___attackingItem.ItemClass.GetItemName(), ___strength, ___hitBodyPart.ToString(), distance, ___bCritical, ___bFatal);
                                DamageDetector.DamageLog.Enqueue(newEntry);
                            }
                        }
                        if (InfiniteAmmo.IsEnabled && ___attackingItem.ItemClass != null && ___attackingItem.ItemClass.IsGun())
                        {
                            InfiniteAmmo.Exec(cInfoAttacker, cInfoAttacker.latestPlayerData.selectedInventorySlot, ___attackingItem);
                        }
                        if (NewPlayerProtection.IsEnabled)
                        {
                            if (NewPlayerProtection.IsProtected(_victim as EntityPlayer))
                            {
                                Phrases.Dict.TryGetValue("NewPlayerProtection2", out string phrase);
                                ChatHook.ChatMessage(cInfoAttacker, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                            else if (NewPlayerProtection.IsProtected(_attacker as EntityPlayer))
                            {
                                Phrases.Dict.TryGetValue("NewPlayerProtection1", out string phrase);
                                ChatHook.ChatMessage(cInfoVictim, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                        }
                        if (Lobby.IsEnabled && Lobby.PvE && (Lobby.IsLobby(_victim.serverPos) || Lobby.IsLobby(_attacker.serverPos)))
                        {
                            Lobby.PvEViolation(cInfoAttacker);
                            return false;
                        }
                        if (Market.IsEnabled && Market.PvE && (Market.IsMarket(_victim.serverPos) || Market.IsMarket(_attacker.serverPos)))
                        {
                            Market.PvEViolation(cInfoAttacker);
                            return false;
                        }
                    }
                    else if (NewPlayerProtection.IsEnabled && NewPlayerProtection.IsProtected(_victim as EntityPlayer))
                    {
                        return false;
                    }
                }
                else if (_victim is EntityZombie && _attacker is EntityPlayer)
                {
                    cInfoAttacker = GeneralOperations.GetClientInfoFromEntityId(_attacker.entityId);
                    if (cInfoAttacker == null)
                    {
                        return true;
                    }
                    if (DamageDetector.IsEnabled)
                    {
                        if (!DamageDetector.IsValidEntityDamage(_attacker, cInfoAttacker, ___strength, ___attackingItem))
                        {
                            return false;
                        }
                        if (DamageDetector.LogEnabled)
                        {
                            float distance = _attacker.GetDistance(_victim);
                            newEntry = string.Format("{0}: '{1}' '{2}' named '{3}' @ '{4}' '{5}' '{6}' named '{7}' @ '{8}' using '{9}' for '{10}' damage in the '{11}'. Distance '{12}' Critical '{13}' Fatal '{14}'", DateTime.Now, cInfoAttacker.PlatformId.CombinedString, cInfoAttacker.CrossplatformId.CombinedString, cInfoAttacker.playerName, _attacker.serverPos, ___damageTyp.ToString(), _victim.entityId, _victim.EntityClass.entityClassName, _victim.position, ___attackingItem.ItemClass.GetLocalizedItemName() ?? ___attackingItem.ItemClass.GetItemName(), ___strength, ___hitBodyPart.ToString(), distance, ___bCritical, ___bFatal);
                            DamageDetector.DamageLog.Enqueue(newEntry);
                        }
                    }
                    if (InfiniteAmmo.IsEnabled && ___attackingItem.ItemClass != null && ___attackingItem.ItemClass.IsGun())
                    {
                        InfiniteAmmo.Exec(cInfoAttacker, cInfoAttacker.latestPlayerData.selectedInventorySlot, ___attackingItem);
                    }

                }
                else if (_victim is EntityAnimal && _attacker is EntityPlayer)
                {
                    cInfoAttacker = GeneralOperations.GetClientInfoFromEntityId(_attacker.entityId);
                    if (cInfoAttacker == null)
                    {
                        return true;
                    }
                    if (DamageDetector.IsEnabled)
                    {
                        if (!DamageDetector.IsValidEntityDamage(_attacker, cInfoAttacker, ___strength, ___attackingItem))
                        {
                            return false;
                        }
                        if (DamageDetector.LogEnabled)
                        {
                            float distance = _attacker.GetDistance(_victim);
                            newEntry = string.Format("{0}: '{1}' '{2}' named '{3}' @ '{4}' '{5}' '{6}' named '{7}' @ '{8}' using '{9}' for '{10}' damage in the '{11}'. Distance '{12}' Critical '{13}' Fatal '{14}'", DateTime.Now, cInfoAttacker.PlatformId.CombinedString, cInfoAttacker.CrossplatformId.CombinedString, cInfoAttacker.playerName, _attacker.serverPos, ___damageTyp.ToString(), _victim.entityId, _victim.EntityClass.entityClassName, _victim.position, ___attackingItem.ItemClass.GetLocalizedItemName() ?? ___attackingItem.ItemClass.GetItemName(), ___strength, ___hitBodyPart.ToString(), distance, ___bCritical, ___bFatal);
                            DamageDetector.DamageLog.Enqueue(newEntry);
                        }
                    }
                    if (InfiniteAmmo.IsEnabled && ___attackingItem.ItemClass != null && ___attackingItem.ItemClass.IsGun())
                    {
                        InfiniteAmmo.Exec(cInfoAttacker, cInfoAttacker.latestPlayerData.selectedInventorySlot, ___attackingItem);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in ProcessDamage.Exec: {0}", e.Message);
            }
            return true;
        }
    }
}

using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class ProcessDamage
    {
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
                            EntityPlayer _player1 = _victim as EntityPlayer;
                            ClientInfo _cInfo1 = PersistentOperations.GetClientInfoFromEntityId(_victim.entityId);
                            if (_cInfo1 != null)
                            {
                                Entity attackingEntity = PersistentOperations.GetEntity(sourceId);
                                if (attackingEntity != null)
                                {
                                    if (attackingEntity is EntityPlayer)
                                    {
                                        EntityPlayer _player2 = attackingEntity as EntityPlayer;
                                        ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromEntityId(attackingEntity.entityId);
                                        if (cInfo2 != null)
                                        {
                                            if (NewPlayerProtection.IsEnabled)
                                            {
                                                if (_player1.Progression.Level < NewPlayerProtection.Level)
                                                {
                                                    Phrases.Dict.TryGetValue("NewPlayerProtection1", out string _phrase);
                                                    ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                    return false;
                                                }
                                                else if (_player2.Progression.Level < NewPlayerProtection.Level)
                                                {
                                                    Phrases.Dict.TryGetValue("NewPlayerProtection2", out string _phrase);
                                                    ChatHook.ChatMessage(_cInfo1, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                    return false;
                                                }
                                            }
                                            int _distance = (int)_player2.GetDistance(_player1);
                                            using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                            {
                                                sw.WriteLine(string.Format("{0}: {1} \"{2}\" hit \"{3}\" with entity id {4} using {5} for {6} damage @ {7}. Distance: {8}", DateTime.Now, cInfo2.playerId, cInfo2.playerName, _player1.EntityName, _player1.entityId, _damageSource.AttackingItem.ItemClass.GetLocalizedItemName() ?? _damageSource.AttackingItem.ItemClass.GetItemName(), _strength, _player1.position, _distance));
                                                sw.WriteLine();
                                                sw.Flush();
                                                sw.Close();
                                            }
                                            if (DamageDetector.IsEnabled && !DamageDetector.IsValidPvP(_player1, cInfo2, _strength, _damageSource.AttackingItem))
                                            {
                                                return false;
                                            }
                                            if (Zones.IsEnabled && (Zones.ZonePlayer.ContainsKey(_player1.entityId) || Zones.ZonePlayer.ContainsKey(_player2.entityId)) && !Zones.IsValid(_player1, cInfo2, _player2))
                                            {
                                                return false;
                                            }
                                            if (Lobby.IsEnabled && Lobby.PvE && (Lobby.LobbyPlayers.Contains(_player1.entityId) || Lobby.LobbyPlayers.Contains(_player2.entityId)))
                                            {
                                                Lobby.PvEViolation(cInfo2);
                                                return false;
                                            }
                                            if (Market.IsEnabled && Market.PvE && (Market.MarketPlayers.Contains(_player1.entityId) || Market.MarketPlayers.Contains(_player2.entityId)))
                                            {
                                                Market.PvEViolation(cInfo2);
                                                return false;
                                            }
                                            if (KillNotice.IsEnabled && KillNotice.PvP && _victim.RecordedDamage.Fatal)
                                            {
                                                KillNotice.PlayerKilledPlayer(_cInfo1, _player1, cInfo2, _player2, _damageSource.AttackingItem, _strength);
                                            }
                                            if (Wallet.IsEnabled)
                                            {
                                                if (Wallet.PVP && Wallet.Player_Kills > 0)
                                                {
                                                    Wallet.AddCoinsToWallet(cInfo2.playerId, Wallet.Player_Kills);
                                                }
                                                else if (Wallet.Player_Kills > 0)
                                                {
                                                    Wallet.SubtractCoinsFromWallet(cInfo2.playerId, Wallet.Player_Kills);
                                                }
                                            }
                                            if (Bounties.IsEnabled)
                                            {
                                                Bounties.PlayerKilled(_player1, _player2, _cInfo1, cInfo2);
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
                                    if (_damageSource.AttackingItem != null)
                                    {
                                        int distance = (int)player.GetDistance(_victim);
                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: {1} \"{2}\" hit \"{3}\" with entity id {4} using {5} for {6} damage @ {7}. Distance: {8}", DateTime.Now, cInfo.playerId, cInfo.playerName, _victim.EntityName, _victim.entityId, _damageSource.AttackingItem.ItemClass.GetLocalizedItemName() ?? _damageSource.AttackingItem.ItemClass.GetItemName(), _strength, _victim.position, distance));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        if (DamageDetector.IsEnabled && !DamageDetector.IsValidEntityDamage(_victim, cInfo, _strength, _damageSource.AttackingItem))
                                        {
                                            return false;
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
            return true;
        }
    }
}

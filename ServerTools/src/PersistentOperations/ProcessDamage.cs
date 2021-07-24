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

        public static bool Exec(EntityAlive __instance, DamageSource _dmgSource, int _strength)
        {
            try
            {
                if (__instance != null && _dmgSource != null && _strength > 1)
                {
                    int _sourceId = _dmgSource.getEntityId();
                    if (_sourceId > 0)
                    {
                        if (__instance is EntityPlayer)
                        {
                            ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromEntityId(_sourceId);
                            if (_cInfo2 != null)
                            {
                                EntityPlayer _player2 = PersistentOperations.GetEntityPlayer(_cInfo2.playerId);
                                if (_player2 != null)
                                {
                                    EntityPlayer _player1 = (EntityPlayer)__instance;
                                    int _distance = (int)_player2.GetDistance(_player1);
                                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                    {
                                        sw.WriteLine(string.Format("{0}: {1} \"{2}\" hit \"{3}\" with entity id {4} using {5} for {6} damage @ {7}. Distance: {8}", DateTime.Now, _cInfo2.playerId, _cInfo2.playerName, _player1.EntityName, _player1.entityId, _dmgSource.AttackingItem.ItemClass.GetLocalizedItemName() ?? _dmgSource.AttackingItem.ItemClass.GetItemName(), _strength, _player1.position, _distance));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    if (DamageDetector.IsEnabled && !DamageDetector.IsValidPvP(_player1, _cInfo2, _strength, _dmgSource.AttackingItem))
                                    {
                                        return false;
                                    }
                                    if (Zones.IsEnabled && (Zones.ZonePlayer.ContainsKey(_player1.entityId) || Zones.ZonePlayer.ContainsKey(_player2.entityId)) && !Zones.IsValid(_player1, _cInfo2, _player2))
                                    {
                                        return false;
                                    }
                                    if (Lobby.IsEnabled && Lobby.PvE && (Lobby.LobbyPlayers.Contains(_player1.entityId) || Lobby.LobbyPlayers.Contains(_player2.entityId)))
                                    {
                                        Lobby.PvEViolation(_cInfo2);
                                        return false;
                                    }
                                    if (Market.IsEnabled && Market.PvE && (Market.MarketPlayers.Contains(_player1.entityId) || Market.MarketPlayers.Contains(_player2.entityId)))
                                    {
                                        Market.PvEViolation(_cInfo2);
                                        return false;
                                    }
                                    if (KillNotice.IsEnabled && KillNotice.Show_Damage)
                                    {
                                        KillNotice.ProcessStrength(_player1, _strength);
                                    }
                                }
                            }
                        }
                        else if (__instance is EntityZombie)
                        {
                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_sourceId);
                            if (_cInfo != null)
                            {
                                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                                if (_player != null)
                                {
                                    if (PersistentOperations.IsBloodmoon() && Market.IsEnabled && Market.MarketPlayers.Contains(_cInfo.entityId))
                                    {
                                        bool _posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_player.position, 15, out int _x, out int _y, out int _z, new Vector3(Market.Market_Size, 5, Market.Market_Size), true);
                                        if (!_posFound)
                                        {
                                            _posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_player.position, 15, out _x, out _y, out _z, new Vector3(Market.Market_Size + 20, 20, Market.Market_Size + 20), true);
                                        }
                                        if (_posFound)
                                        {
                                            Market.MarketPlayers.Remove(_cInfo.entityId);
                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, -1, _z), null, false));
                                        }
                                        else
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Player was detected attempting to kill zombies inside of the Market during bloodmoon. Unable to find a suitable respawn location near by"));
                                        }
                                        return false;
                                    }
                                    if (PersistentOperations.IsBloodmoon() && Lobby.IsEnabled && Lobby.LobbyPlayers.Contains(_cInfo.entityId))
                                    {
                                        bool _posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_player.position, 15, out int _x, out int _y, out int _z, new Vector3(Lobby.Lobby_Size, 5, Lobby.Lobby_Size), true);
                                        if (!_posFound)
                                        {
                                            _posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_player.position, 15, out _x, out _y, out _z, new Vector3(Lobby.Lobby_Size + 20, 20, Lobby.Lobby_Size + 20), true);
                                        }
                                        if (_posFound)
                                        {
                                            Market.MarketPlayers.Remove(_cInfo.entityId);
                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, -1, _z), null, false));
                                        }
                                        else
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Player was detected attempting to kill zombies inside of the Lobby during bloodmoon. Unable to find a suitable respawn location near by"));
                                        }
                                        return false;
                                    }
                                    if (_dmgSource.AttackingItem != null)
                                    {
                                        int _distance = (int)_player.GetDistance(__instance);
                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: {1} \"{2}\" hit \"{3}\" with entity id {4} using {5} for {6} damage @ {7}. Distance: {8}", DateTime.Now, _cInfo.playerId, _cInfo.playerName, __instance.EntityName, __instance.entityId, _dmgSource.AttackingItem.ItemClass.GetLocalizedItemName() ?? _dmgSource.AttackingItem.ItemClass.GetItemName(), _strength, __instance.position, _distance));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        if (DamageDetector.IsEnabled && !DamageDetector.IsValidEntityDamage(__instance, _cInfo, _strength, _dmgSource.AttackingItem))
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

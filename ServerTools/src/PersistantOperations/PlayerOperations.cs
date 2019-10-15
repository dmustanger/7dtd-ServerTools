using System;
using System.Collections.Generic;
using System.Data;

namespace ServerTools
{
    class PlayerOperations
    {
        public static bool IsRunning = false, IsRunning2 = false;
        public Dictionary<string, string> clans = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static List<int> Died = new List<int>();

        public static void PlayerCheck()
        {
            if (!IsRunning2)
            {
                IsRunning2 = true;
                try
                {
                    if (ConnectionManager.Instance.ClientCount() > 0)
                    {
                        List<EntityPlayer> EntityPlayerList = GameManager.Instance.World.Players.list;
                        for (int i = 0; i < EntityPlayerList.Count; i++)
                        {
                            EntityPlayer _player = EntityPlayerList[i];
                            if (_player != null)
                            {
                                ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_player.entityId);
                                if (_cInfo != null)
                                {
                                    if (!_player.IsDead() && _player.Spawned)
                                    {
                                        if (Zones.IsEnabled)
                                        {
                                            Zones.ZoneCheck(_cInfo, _player);
                                        }
                                        if (GodModeFlight.IsEnabled)
                                        {
                                            GodModeFlight.GodCheck(_cInfo);
                                        }
                                    }
                                    else if (_player.IsDead())
                                    {
                                        if (!Died.Contains(_player.entityId))
                                        {
                                            Died.Add(_player.entityId);
                                            if (KillNotice.IsEnabled || Bounties.IsEnabled || Zones.IsEnabled)
                                            {
                                                for (int j = 0; j < EntityPlayerList.Count; j++)
                                                {
                                                    EntityPlayer _player2 = EntityPlayerList[j];
                                                    if (_player != _player2)
                                                    {
                                                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(_player2.entityId);
                                                        if (_cInfo2 != null)
                                                        {
                                                            Entity _target = _player2.GetDamagedTarget();
                                                            if (_player == _target)
                                                            {
                                                                if (KillNotice.IsEnabled)
                                                                {
                                                                    string _holdingItem = _player2.inventory.holdingItem.Name;
                                                                    ItemValue _itemValue = ItemClass.GetItem(_holdingItem, true);
                                                                    if (_itemValue.type != ItemValue.None.type)
                                                                    {
                                                                        _holdingItem = _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name;
                                                                    }
                                                                    KillNotice.Notice(_cInfo, _cInfo2, _holdingItem);
                                                                }
                                                                if (Bounties.IsEnabled)
                                                                {
                                                                    Bounties.PlayerKilled(_player, _player2, _cInfo, _cInfo2);
                                                                }
                                                                if (Zones.IsEnabled)
                                                                {
                                                                    Zones.Check(_cInfo, _cInfo2);
                                                                }
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            if (DeathSpot.IsEnabled)
                                            {
                                                DeathSpot.PlayerKilled(_player);
                                            }
                                            if (Wallet.IsEnabled && Wallet.Lose_On_Death)
                                            {
                                                Wallet.ClearWallet(_cInfo);
                                            }
                                            if (Event.Open && Event.PlayersTeam.ContainsKey(_cInfo.playerId))
                                            {
                                                PersistentContainer.Instance.Players[_cInfo.playerId].EventRespawn = true;
                                                PersistentContainer.Instance.Save();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Log.Error("[SERVERTOOLS] Static player operations failed");
                }
                IsRunning2 = false;
            }
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            Session.Add(_cInfo.playerId, DateTime.Now);
        }
    }
}

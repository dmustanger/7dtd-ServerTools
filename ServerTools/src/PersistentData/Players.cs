using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace ServerTools
{
    public class Players
    {
        public static bool IsRunning = false;
        public Dictionary<string, string> clans = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static List<int> Died = new List<int>();
        private static Thread th;

        public static void Load()
        {
            Start();
            IsRunning = true;
        }

        public static void Unload()
        {
            th.Abort();
            IsRunning = false;
        }

        private static void Start()
        {
            th = new Thread(new ThreadStart(PlayerCheck));
            th.IsBackground = true;
            th.Start();
        }

        private static void PlayerCheck()
        {
            while (IsRunning)
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
                                        GodModeFlight.GodFlightCheck(_cInfo);
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
                                            string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                            DataTable _result = SQL.TQuery(_sql);
                                            int _playerSpentCoins;
                                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
                                            _result.Dispose();
                                            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                                            if (_currentCoins >= 1)
                                            {
                                                _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins - _currentCoins, _cInfo.playerId);
                                                SQL.FastQuery(_sql, "Players");
                                            }
                                        }
                                        if (Event.Open && Event.PlayersTeam.ContainsKey(_cInfo.playerId))
                                        {
                                            string _sql = string.Format("UPDATE Players SET eventReturn = 'true' WHERE steamid = '{0}'", _cInfo.playerId);
                                            SQL.FastQuery(_sql, "Players");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(500);
            }
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            Session.Add(_cInfo.playerId, DateTime.Now);
        }
    }
}
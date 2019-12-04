using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class Tracking
    {
        public static bool IsEnabled = false;
        public static int Rate = 30, Days_Before_Log_Delete = 3;

        public static void Exec()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                List<ClientInfo> ClientInfoList = ConnectionManager.Instance.Clients.List.ToList();
                for (int i = 0; i < ClientInfoList.Count; i++)
                {
                    ClientInfo _cInfo = ClientInfoList[i];
                    if (_cInfo != null)
                    {
                        EntityAlive _player = (EntityAlive)GameManager.Instance.World.GetEntity(_cInfo.entityId);
                        if (_player != null)
                        {
                            string _pos = (int)_player.position.x + " " + (int)_player.position.y + " " + (int)_player.position.z;
                            string _holdingItem = _player.inventory.holdingItem.Name;
                            ItemValue _itemValue = ItemClass.GetItem(_holdingItem, true);
                            if (_itemValue.type != ItemValue.None.type)
                            {
                                _holdingItem = _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name;
                            }
                            string _sql = string.Format("INSERT INTO Tracking (dateTime, position, steamId, playerName, holding) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", DateTime.Now.ToString() ,_pos, _cInfo.playerId, _cInfo.playerName, _holdingItem);
                            SQL.FastQuery(_sql, "Tracking");
                        }
                    }
                }
            }
        }

        public static void Cleanup()
        {
            if (Tracking.IsEnabled)
            {
                Log.Out("Database tracking log cleanup started");
                string _sql = string.Format("DELETE FROM Tracking WHERE dateTime < DATETIME('{0}')", DateTime.Now.AddDays(Days_Before_Log_Delete * -1).ToString());
                SQL.FastQuery(_sql, "Tracking");
                Log.Out("Database tracking log cleanup complete");
            }
        }
    }
}
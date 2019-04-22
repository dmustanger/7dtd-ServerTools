using System;
using System.Collections.Generic;
using System.Data;
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
                            string _sql = string.Format("INSERT INTO Tracking (dateTime, position, steamId, playerName, holding) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", DateTime.Now.ToString(), _pos, _cInfo.playerId, _cInfo.playerName, _holdingItem);
                            SQL.FastQuery(_sql, "Tracking");
                        }
                    }
                }
            }
        }

        public static void Cleanup()
        {
            string _sql = string.Format("SELECT * FROM Tracking ORDER BY dateTime ASC");
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                for (int i = 0; i < _result.Rows.Count; i++)
                {
                    DataRow _row = _result.Rows[i];
                    if (_row != null)
                    {
                        DateTime _dateTime;
                        DateTime.TryParse(_row.ItemArray.GetValue(1).ToString(), out _dateTime);
                        DateTime _dateCheck = _dateTime.AddDays(Days_Before_Log_Delete);
                        if (_dateCheck <= DateTime.Now)
                        {
                            int _id;
                            int.TryParse(_row.ItemArray.GetValue(0).ToString(), out _id);
                            _sql = string.Format("DELETE FROM Tracking WHERE Id = {0}", _id);
                            SQL.FastQuery(_sql, "Tracking");
                        }
                    }
                }
            }
            _result.Dispose();
        }
    }
}
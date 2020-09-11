using System;
using System.Collections.Generic;

namespace ServerTools
{
    class Track
    {
        public static bool IsEnabled = false;

        public static void Exec()
        {
            if (GameManager.Instance.World.Players.dict.Count > 0)
            {
                List<string[]> _track = new List<string[]>();
                if (PersistentContainer.Instance.Track != null)
                {
                    _track = PersistentContainer.Instance.Track;
                }
                List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                if (_cInfoList != null && _cInfoList.Count > 0)
                {
                    for (int i = 0; i < _cInfoList.Count; i++)
                    {
                        ClientInfo _cInfo = _cInfoList[i];
                        if (_cInfo != null && _cInfo.playerId != null)
                        {
                            if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                if (_player != null && _player.IsSpawned())
                                {
                                    string _pos = (int)_player.position.x + "," + (int)_player.position.y + "," + (int)_player.position.z;
                                    string _holdingItem = _player.inventory.holdingItem.Name;
                                    if (!string.IsNullOrEmpty(_player.inventory.holdingItem.Name))
                                    {
                                        ItemValue _itemValue = ItemClass.GetItem(_holdingItem, true);
                                        if (_itemValue.type != ItemValue.None.type)
                                        {
                                            _holdingItem = _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name;
                                        }
                                        string[] _newData = { DateTime.Now.ToString(), _pos, _cInfo.playerId, _player.EntityName, _holdingItem };
                                        _track.Add(_newData);
                                    }
                                    else
                                    {
                                        string[] _newData = { DateTime.Now.ToString(), _pos, _cInfo.playerId, _player.EntityName, "nothing" };
                                        _track.Add(_newData);
                                    }
                                }
                            }
                        }
                    }
                }
                PersistentContainer.Instance.Track = _track;
                PersistentContainer.Instance.Save();
            }
        }

        public static void Cleanup()
        {
            if (PersistentContainer.Instance.Track != null && PersistentContainer.Instance.Track.Count > 0)
            {
                Log.Out("[SERVERTOOLS] Deleting old tracking logs");
                List<string[]> _trackLog = PersistentContainer.Instance.Track;
                for (int i = 0; i < _trackLog.Count; i++)
                {
                    string[] _trackData = _trackLog[i];
                    DateTime.TryParse(_trackData[0], out DateTime _date);
                    if (_date.AddDays(2) >= DateTime.Now)
                    {
                        _trackLog.Remove(_trackData);
                    }
                }
                PersistentContainer.Instance.Track = _trackLog;
                PersistentContainer.Instance.Save();
            }
            Log.Out("[SERVERTOOLS] Tracking log clean up completed");
        }
    }
}
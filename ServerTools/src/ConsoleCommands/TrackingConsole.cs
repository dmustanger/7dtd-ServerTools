using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class TrackingConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Player Tracking. Check if players have been tracked with in the specified range.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Tracking off\n" +
                   "  2. Tracking on\n" +
                   "  3. Tracking <Hours> <Block Range>\n" +
                   "  4. Tracking <Hours> <Block Range> <X> <Y> <Z>\n" +
                   "1. Turn off the tracking tool\n" +
                   "2. Turn on the tracking tool\n" +
                   "3. Shows the players that have been tracked within the time and range specified from your current location\n" +
                   "4. Shows the players that have been tracked within the time, range and location specified\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Tracking", "tracking", "track" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count == 3 || _params.Count == 4 || _params.Count > 5)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, 2 or 5, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    Tracking.IsEnabled = false;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Tracking has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Tracking.IsEnabled = true;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Tracking has been set to on"));
                    return;
                }
                else if (_params.Count == 2)
                {
                    int _hours, _range;
                    if (int.TryParse(_params[0], out _hours))
                    {
                        if (int.TryParse(_params[1], out _range))
                        {
                            List<string> PlayerId = new List<string>();
                            string _sql = string.Format("SELECT * FROM Tracking ORDER BY dateTime DESC");
                            DataTable _result = SQL.TQuery(_sql);
                            if (_result.Rows.Count > 0)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                                SdtdConsole.Instance.Output(string.Format("Tracking results at a range of {0} blocks:", _range));
                                bool _found = false;
                                foreach (DataRow row in _result.Rows)
                                {
                                    DateTime _dateTime;
                                    DateTime.TryParse(row.ItemArray.GetValue(0).ToString(), out _dateTime);
                                    if (_dateTime.AddHours(_hours) >= DateTime.Now)
                                    {
                                        string[] _cords = row.ItemArray.GetValue(1).ToString().Split(' ');
                                        int _x, _y, _z;
                                        int.TryParse(_cords[0], out _x);
                                        int.TryParse(_cords[1], out _y);
                                        int.TryParse(_cords[2], out _z);
                                        Vector3 _trackedVecPos = new Vector3(_x, _y, _z);
                                        if (RangeCheck(_player.position, _trackedVecPos, _range))
                                        {
                                            _found = true;
                                            string _playerId = row.ItemArray.GetValue(2).ToString();
                                            string _playerName = row.ItemArray.GetValue(3).ToString();
                                            string _itemHeld = row.ItemArray.GetValue(4).ToString();
                                            if (!PlayerId.Contains(_playerId))
                                            {
                                                PlayerId.Add(_playerId);
                                                SdtdConsole.Instance.Output(string.Format("Player: {0}, SteamId: {1}, Time: {2}, Position: {3} {4} {5}, Item Held: {6}", _playerName, _playerId, _dateTime, _x, _y, _z, _itemHeld));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (!_found)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Tracking results found nobody at this time and range inside your current position"));
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("No tracking positions are recorded in the database"));
                            }
                            _result.Dispose();
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid argument {1}.", _params[1]));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                    }
                    return;
                }
                else if (_params.Count == 5)
                {
                    int _hours, _range, _worldX, _worldY, _worldZ;
                    if (int.TryParse(_params[0], out _hours))
                    {
                        if (int.TryParse(_params[1], out _range))
                        {
                            if (int.TryParse(_params[2], out _worldX))
                            {
                                if (int.TryParse(_params[3], out _worldY))
                                {
                                    if (int.TryParse(_params[4], out _worldZ))
                                    {
                                        List<string> PlayerId = new List<string>();
                                        string _sql = string.Format("SELECT * FROM Tracking ORDER BY dateTime DESC");
                                        DataTable _result = SQL.TQuery(_sql);
                                        if (_result.Rows.Count > 0)
                                        {
                                            SdtdConsole.Instance.Output(string.Format("Tracking results at a range of {0} blocks:", _range));
                                            bool _found = false;
                                            foreach (DataRow row in _result.Rows)
                                            {
                                                DateTime _dateTime;
                                                DateTime.TryParse(row.ItemArray.GetValue(0).ToString(), out _dateTime);
                                                if (_dateTime.AddHours(_hours) >= DateTime.Now)
                                                {
                                                    string[] _cords = row.ItemArray.GetValue(1).ToString().Split(' ');
                                                    int _x, _y, _z;
                                                    int.TryParse(_cords[0], out _x);
                                                    int.TryParse(_cords[1], out _y);
                                                    int.TryParse(_cords[2], out _z);
                                                    Vector3 _worldVecPos = new Vector3(_worldX, _worldY, _worldZ);
                                                    Vector3 _trackedVecPos = new Vector3(_x, _y, _z);
                                                    if (RangeCheck(_worldVecPos, _trackedVecPos, _range))
                                                    {
                                                        _found = true;
                                                        string _playerId = row.ItemArray.GetValue(2).ToString();
                                                        string _playerName = row.ItemArray.GetValue(3).ToString();
                                                        string _itemHeld = row.ItemArray.GetValue(4).ToString();
                                                        if (!PlayerId.Contains(_playerId))
                                                        {
                                                            PlayerId.Add(_playerId);
                                                            SdtdConsole.Instance.Output(string.Format("Player: {0}, SteamId: {1}, Time: {2}, Position: {3} {4} {5}, Item Held: {6}", _playerName, _playerId, _dateTime, _x, _y, _z, _itemHeld));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            if (!_found)
                                            {
                                                SdtdConsole.Instance.Output(string.Format("Tracking results found nobody at this time and range inside the specified position"));
                                            }
                                        }
                                        else
                                        {
                                            SdtdConsole.Instance.Output(string.Format("No tracking positions are recorded in the database"));
                                        }
                                        _result.Dispose();
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("Invalid argument {4}.", _params[4]));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("Invalid argument {3}.", _params[3]));
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Invalid argument {2}.", _params[2]));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid argument {1}.", _params[1]));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                    }
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid arguments."));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TrackingConsole.Run: {0}.", e));
            }
        }

        public static bool RangeCheck(Vector3 _playerX, Vector3 _trackedVecPos, int _range)
        {
            int _distance = (int)Vector3.Distance(_playerX, _trackedVecPos);
            if (_distance <= _range)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

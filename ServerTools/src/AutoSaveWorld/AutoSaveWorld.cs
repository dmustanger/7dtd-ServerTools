using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class AutoSaveWorld
    {
        public static bool IsEnabled = false;

        public static void Save()
        {
            int _playerCount = ConnectionManager.Instance.ClientCount();
            if (_playerCount > 0)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    if (_cInfo != null)
                    {
                        SdtdConsole.Instance.ExecuteSync("saveworld", _cInfo);
                        Log.Out("[SERVERTOOLS] World Saved.");
                        break;
                    }
                }
            }
        }
    }
}
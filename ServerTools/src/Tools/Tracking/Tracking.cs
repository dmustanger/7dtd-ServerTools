using System;
using System.Collections.Generic;

namespace ServerTools
{
    class Track
    {
        public static void Exec()
        {
            List<ClientInfo> clientList = GeneralFunction.ClientList();
            if (clientList != null && clientList.Count > 0)
            {
                List<string[]> track = PersistentContainer.Instance.Track;
                for (int i = 0; i < clientList.Count; i++)
                {
                    ClientInfo cInfo = clientList[i];
                    if (cInfo != null)
                    {
                        EntityPlayer player = GeneralFunction.GetEntityPlayer(cInfo.entityId);
                        if (player != null && player.IsSpawned())
                        {
                            string pos = (int)player.position.x + "," + (int)player.position.y + "," + (int)player.position.z;
                            string holdingItem = player.inventory.holdingItem.Name;
                            if (!string.IsNullOrEmpty(player.inventory.holdingItem.Name))
                            {
                                ItemValue itemValue = ItemClass.GetItem(holdingItem, true);
                                if (itemValue.type != ItemValue.None.type)
                                {
                                    holdingItem = itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name;
                                }
                                string[] newData = { DateTime.Now.ToString(), pos, cInfo.CrossplatformId.CombinedString, player.EntityName, holdingItem };
                                track.Add(newData);
                            }
                            else
                            {
                                string[] newData = { DateTime.Now.ToString(), pos, cInfo.CrossplatformId.CombinedString, player.EntityName, "nothing" };
                                track.Add(newData);
                            }
                        }
                    }
                }
                PersistentContainer.Instance.Track = track;
                PersistentContainer.DataChange = true;
            }
        }

        public static void Cleanup()
        {
            try
            {
                if (PersistentContainer.Instance != null && PersistentContainer.Instance.Track != null && PersistentContainer.Instance.Track.Count > 0)
                {
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
                    PersistentContainer.DataChange = true;
                    Log.Out("[SERVERTOOLS] Tracking log clean up complete");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Tracking.Cleanup: {0}", e.Message));
            }
        }
    }
}
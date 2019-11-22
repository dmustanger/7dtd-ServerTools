﻿using System;
using System.Collections.Generic;

namespace ServerTools
{
    class PlayerOperations
    {
        private static bool IsRunning = false;
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        //private static List<int> Died = new List<int>();

        public static void PlayerCheck()
        {
            if (!IsRunning)
            {
                IsRunning = true;
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
                                        if (GodMode.IsEnabled)
                                        {
                                            GodMode.GodCheck(_cInfo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    IsRunning = false;
                    Log.Out(string.Format("[SERVERTOOLS] Error in PlayerOperations: {0}.", e.Message));
                }
                IsRunning = false;
            }
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            Session.Add(_cInfo.playerId, DateTime.Now);
        }
    }
}

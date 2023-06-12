using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class TrackingConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Check if players have been tracked with in the specified range and location.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-track <Hours> <Block Range>\n" +
                   "  2. st-track <Hours> <Block Range> <X> <Y> <Z>\n" +
                   "1. List players tracked with in the specified hours and block range from your current location\n" +
                   "2. List players tracked with in the specified hours and block range from a specific location\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-Tracking", "track", "st-track" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 5)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or 5, found '{0}'", _params.Count));
                    return;
                }
                if (_params.Count == 2)
                {
                    bool _found = false;
                    if (int.TryParse(_params[0], out int hours))
                    {
                        if (hours > 48)
                        {
                            hours = 48;
                        }
                        if (int.TryParse(_params[1], out int range))
                        {
                            EntityPlayer player = GeneralOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.entityId);
                            if (player != null)
                            {
                                List<string[]> tracking = PersistentContainer.Instance.Track;
                                for (int i = 0; i < tracking.Count; i++)
                                {
                                    string[] trackData = tracking[i];
                                    DateTime.TryParse(trackData[0], out DateTime date);
                                    if (date.AddHours(hours) >= DateTime.Now)
                                    {
                                        string[] cords = trackData[1].Split(',');
                                        int.TryParse(cords[0], out int x);
                                        int.TryParse(cords[1], out int y);
                                        int.TryParse(cords[2], out int z);
                                        Vector3 trackedVecPos = new Vector3(x, y, z);
                                        if (RangeCheck(player.position, trackedVecPos, range))
                                        {
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Player '{0}' Id '{1}' Time '{2}' Position '{3}' Item Held '{4}'", trackData[3], trackData[2], trackData[0], trackData[1], trackData[4]));
                                        }
                                    }
                                }
                            }
                            if (!_found)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Tracking log lists nobody at this time and range from your current position");
                            }
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                        return;
                    }
                }
                else if (_params.Count == 5)
                {
                    if (PersistentContainer.Instance.Track != null && PersistentContainer.Instance.Track.Count > 0)
                    {
                        bool found = false;
                        if (int.TryParse(_params[0], out int hours))
                        {
                            if (int.TryParse(_params[1], out int range))
                            {
                                if (int.TryParse(_params[2], out int worldX))
                                {
                                    if (int.TryParse(_params[3], out int worldY))
                                    {
                                        if (int.TryParse(_params[4], out int worldZ))
                                        {
                                            EntityPlayer player = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                                            if (player != null)
                                            {
                                                List<string[]> tracking = PersistentContainer.Instance.Track;
                                                for (int i = 0; i < tracking.Count; i++)
                                                {
                                                    string[] trackData = tracking[i];
                                                    DateTime.TryParse(trackData[0], out DateTime date);
                                                    if (date.AddHours(hours) >= DateTime.Now)
                                                    {
                                                        Vector3 trackedVecPos = new Vector3(worldX, worldY, worldZ);
                                                        if (RangeCheck(player.position, trackedVecPos, range))
                                                        {
                                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Player '{0}' Id '{1}' Time '{2}' Position '{3}' Item Held '{4}'", trackData[3], trackData[2], trackData[0], trackData[1], trackData[4]));
                                                        }
                                                    }
                                                }
                                            }
                                            if (!found)
                                            {
                                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Tracking results found nobody at this time and range inside the specified position"));
                                            }
                                        }
                                        else
                                        {
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[4]));
                                        }
                                    }
                                    else
                                    {
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[3]));
                                    }
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[2]));
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[1]));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                        }
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Tracking log has no data");
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Invalid arguments");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TrackingConsole.Execute: {0}", e.Message));
            }
        }

        public static bool RangeCheck(Vector3 _playerX, Vector3 _trackedVecPos, int _range)
        {
            int distance = (int)Vector3.Distance(_playerX, _trackedVecPos);
            if (distance <= _range)
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

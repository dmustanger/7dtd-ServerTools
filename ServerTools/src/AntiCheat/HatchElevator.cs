using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServerTools
{
    class HatchElevator
    {
        public static bool IsEnabled = false;
        public static int DaysBeforeDeleted = 5, MaxPing = 300;
        private static List<int> Flag = new List<int>();
        public static SortedDictionary<int, int> LastPositionY = new SortedDictionary<int, int>();

        public static void DetectionLogsDir()
        {
            if (!Directory.Exists(API.GamePath + "/DetectionLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/DetectionLogs");
            }

            string[] files = Directory.GetFiles(API.GamePath + "/DetectionLogs");
            int _daysBeforeDeleted = (DaysBeforeDeleted * -1);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                {
                    fi.Delete();
                }
            }
        }

        public static void AutoHatchCheck()
        {           
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    HatchCheck(_cInfo, _player);
                }
            }
        }

        public static void HatchCheck(ClientInfo _cInfo, EntityPlayer ep)
        {
            if (_cInfo.ping < MaxPing)
            {
                int x = (int)ep.position.x;
                int y = (int)ep.position.y;
                int z = (int)ep.position.z;
                int Id = ep.entityId;

                if (LastPositionY.ContainsKey(Id))
                {
                    for (int i = x - 1; i <= (x + 1); i++)
                    {
                        for (int j = z - 1; j <= (z + 1); j++)
                        {
                            for (int k = y - 2; k <= (y + 1); k++)
                            {
                                BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                                if (Block.Block.blockID != 788 || Block.Block.blockID != 389 || Block.Block.blockID != 949)
                                {List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                                    if (Block.Block.blockID == 1251 || Block.Block.blockID == 1252 || Block.Block.blockID == 1253 ||
                                        Block.Block.blockID == 1463 || Block.Block.blockID == 1464 || Block.Block.blockID == 1465 ||
                                        Block.Block.blockID == 1469 || Block.Block.blockID == 1470 || Block.Block.blockID == 1471)
                                    {
                                        if (Flag.Contains(Id))
                                        {
                                            int _lastY;
                                            if (LastPositionY.TryGetValue(Id, out _lastY))
                                            {
                                                int heightChange = (_lastY - y);
                                                if (heightChange < -6)
                                                {
                                                    _cInfo.SendPackage(new NetPackageConsoleCmdClient("buff " + "brokenLeg", true));
                                                    _cInfo.SendPackage(new NetPackageConsoleCmdClient("buff " + "stunned", true));
                                                    LastPositionY[Id] = y;
                                                    Flag.Remove(Id);
                                                    string _phrase720;
                                                    if (!Phrases.Dict.TryGetValue(705, out _phrase720))
                                                    {
                                                        _phrase720 = "you are stunned and have broken your leg while smashing yourself through hatches. Ouch!";
                                                    }
                                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase720 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                    string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                                    string _filepath = string.Format("{0}/DetectionLogs/{1}", API.GamePath, _file);
                                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("{0} {1} steamId {2} was detected using a hatch elevator @ {3} X, {4} Y, {5} Z", DateTime.Now, _cInfo.playerName, _cInfo.playerId, ep.position.x, ep.position.y, ep.position.z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }
                                                    Log.Out(string.Format("[SERVERTOOLS] Detected {0} using a hatch elevator. Applied stun and broke leg", ep.entityId));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LastPositionY[Id] = y;
                                            Flag[Id] = Id;
                                        }
                                    }
                                    else
                                    {
                                        LastPositionY[Id] = y;
                                        if (Flag.Contains(Id))
                                        {
                                            Flag.Remove(Id);
                                        }
                                        break;
                                    }
                                }
                                else
                                {
                                    LastPositionY[Id] = y;
                                    if (Flag.Contains(Id))
                                    {
                                        Flag.Remove(Id);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    LastPositionY[Id] = y;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class CommandJailConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Turn on/off, add, remove, or list players in jail";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-jl off\n" +
                "  2. st-jl on\n" +
                "  3. st-jl add <EOS/EntityId/PlayerName> <Time>\n" +
                "  4. st-jl remove <EOS/EntityId/PlayerName>" +
                "  5. st-jl list\n" +
                "1. Turn off jail\n" +
                "2. Turn on jail\n" +
                "3. Adds a Id to the jail list for a specific time in minutes\n" +
                "4. Removes a Id from the jail list\n" +
                "5. Lists all Id in the jail list" +
                "*Note Use -1 for time to jail indefinitely*";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Jail", "jl", "st-jl" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 3)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Jail.IsEnabled)
                    {
                        Jail.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Jail has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Jail is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Jail.IsEnabled)
                    {
                        Jail.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Jail has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Jail is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found {0}", _params.Count));
                        return;
                    }
                    if (Jail.Jailed.Contains(_params[1]))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id. {0} is already in the Jail list", _params[1]));
                        return;
                    }
                    if (!int.TryParse(_params[2], out int jailTime))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Jail time is not valid '{0}'", _params[2]));
                        return;
                    }
                    if (Jail.Jail_Position == "0,0,0" || Jail.Jail_Position == "0 0 0" || Jail.Jail_Position == "")
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not put a player in jail. The jail position has not been set"));
                        return;
                    }
                    else
                    {
                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                        if (cInfo != null)
                        {
                            if (Jail.Jailed.Contains(cInfo.CrossplatformId.CombinedString))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player with Id '{0}' is already in jail", _params[1]));
                                return;
                            }
                            else
                            {
                                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                                if (player != null && player.IsSpawned())
                                {
                                    string[] cords = Jail.Jail_Position.Split(',');
                                    int.TryParse(cords[0], out int x);
                                    int.TryParse(cords[1], out int y);
                                    int.TryParse(cords[2], out int z);
                                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                                }
                                Jail.Jailed.Add(cInfo.CrossplatformId.CombinedString);
                                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailTime = jailTime;
                                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailName = cInfo.playerName;
                                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailDate = DateTime.Now;
                                PersistentContainer.DataChange = true;
                                if (jailTime > 0)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] You have put '{0}' named '{1}' in jail for '{2}' minutes", cInfo.CrossplatformId.CombinedString, cInfo.playerName, jailTime));
                                    Phrases.Dict.TryGetValue("Jail1", out string phrase);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (jailTime == -1)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] You have put '{0}' named '{1}' in jail forever", cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                    Phrases.Dict.TryGetValue("Jail1", out string phrase);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                        else
                        {
                            if (PersistentContainer.Instance.Players[_params[1]] != null)
                            {
                                if (_params[1].Contains("_"))
                                {
                                    Jail.Jailed.Add(_params[1]);
                                    PersistentContainer.Instance.Players[_params[1]].JailTime = jailTime;
                                    PersistentContainer.Instance.Players[_params[1]].JailName = cInfo.playerName;
                                    PersistentContainer.Instance.Players[_params[1]].JailDate = DateTime.Now;
                                    PersistentContainer.DataChange = true;
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player with Id '{0}' can not be found online but has been set for jail", _params[1]));
                                    return;
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid EOS Id '{0}' ", _params[1]));
                                    return;
                                }
                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    else
                    {
                        if (!Jail.Jailed.Contains(_params[1]))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player with Id '{0}' is not in jail", _params[1]));
                            return;
                        }
                        else
                        {
                            ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                            if (cInfo != null)
                            {
                                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                                if (player != null)
                                {
                                    EntityBedrollPositionList position = player.SpawnPoints;
                                    Jail.Jailed.Remove(cInfo.CrossplatformId.CombinedString);
                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailTime = 0;
                                    PersistentContainer.DataChange = true;
                                    if (position != null && position.Count > 0)
                                    {
                                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(position[0].x, -1, position[0].z), null, false));
                                    }
                                    else
                                    {
                                        Vector3[] pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(pos[0].x, -1, pos[0].z), null, false));
                                    }
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] You have released Id '{0}' from jail", _params[1]));
                                    return;
                                }
                            }
                            else
                            {
                                if (PersistentContainer.Instance.Players[_params[1]] != null)
                                {
                                    if (_params[1].Contains("_"))
                                    {
                                        Jail.Jailed.Remove(_params[1]);
                                        PersistentContainer.Instance.Players[_params[1]].JailTime = 0;
                                        PersistentContainer.DataChange = true;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player with Id '{0}' can not be found online but has been set for jail", _params[1]));
                                        return;
                                    }
                                    else
                                    {
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid EOS Id '{0}' ", _params[1]));
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (Jail.Jailed.Count == 0)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There are no Id on the jail list");
                        return;
                    }
                    else
                    {
                        for (int i = 0; i < Jail.Jailed.Count; i++)
                        {
                            string id = Jail.Jailed[i];
                            int jailTime = PersistentContainer.Instance.Players[id].JailTime;
                            string jailName = PersistentContainer.Instance.Players[id].JailName;
                            if (jailTime > 0)
                            {
                                DateTime jailDate = PersistentContainer.Instance.Players[id].JailDate;
                                TimeSpan varTime = DateTime.Now - jailDate;
                                double fractionalMinutes = varTime.TotalMinutes;
                                int timepassed = (int)fractionalMinutes;
                                int timeleft = jailTime - timepassed;
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' named '{1}' is jailed for {2} more minutes", id, jailName, timeleft));
                            }
                            else if (jailTime == -1)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' named '{1}' is jailed forever", id, jailName));
                            }
                        }
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CommandJailConsole.Execute: {0}", e.Message));
            }
        }
    }
}
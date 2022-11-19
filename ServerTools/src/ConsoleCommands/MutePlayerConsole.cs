using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class MutedConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Mutes a players chat.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-mt add <EOS/EntityId/PlayerName>\n" +
                "  2. st-mt add <EOS/EntityId/PlayerName> <time>\n" +
                "  3. st-mt remove <EOS>\n" +
                "  4. st-mt list\n" +
                "1. Adds a Id to the mute list for 60 minutes\n" +
                "2. Adds a Id to the mute list for a specific time\n" +
                "3. Removes a Id from the mute list\n" +
                "4. Lists all Id in the mute list\n" +
                "*Note Use -1 for time to mute indefinitely*";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Mute", "mt", "st-mt" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (Mute.IsEnabled)
            {
                try
                {
                    if (_params.Count < 1 || _params.Count > 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found '{0}'", _params.Count));
                        return;
                    }
                    if (_params[0].ToLower().Equals("add"))
                    {
                        if (_params.Count < 2 || _params.Count > 3)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or 3, found '{0}'", _params.Count));
                            return;
                        }
                        if (!_params[1].Contains("_"))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid Id {0}", _params[1]));
                            return;
                        }
                        int muteTime = 60;
                        if (_params[2] != null)
                        {
                            if (int.TryParse(_params[2], out int value))
                                muteTime = value;
                        }
                        ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(_params[1]);
                        if (cInfo != null)
                        {
                            if (Mute.Mutes.Contains(cInfo.PlatformId.CombinedString) || Mute.Mutes.Contains(cInfo.CrossplatformId.CombinedString))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' '{1}' named '{2}' is already muted", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                return;
                            }
                            else
                            {
                                if (muteTime == -1)
                                {
                                    Mute.Mutes.Add(cInfo.PlatformId.CombinedString);
                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteTime = -1;
                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteName = cInfo.playerName;
                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteDate = DateTime.Now;
                                    PersistentContainer.DataChange = true;
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' '{1}' named '{2}' has been muted indefinitely", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                    return;
                                }
                                else
                                {
                                    Mute.Mutes.Add(cInfo.CrossplatformId.CombinedString);
                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteTime = muteTime;
                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteName = cInfo.playerName;
                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteDate = DateTime.Now;

                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' '{1}' named '{2}' has been muted for '{3}' minutes", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, muteTime));
                                    return;
                                }
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player with Id '{0}' can not be found", _params[1]));
                            return;
                        }
                    }
                    else if (_params[0].ToLower().Equals("remove"))
                    {
                        if (_params.Count != 2)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                            return;
                        }
                        if (!_params[1].Contains("_"))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid Id '{0}'", _params[1]));
                            return;
                        }
                        string id = _params[1];
                        if (Mute.Mutes.Contains(id))
                        {
                            ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(id);
                            if (cInfo != null)
                            {
                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + "You have been unmuted[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            Mute.Mutes.Remove(id);
                            PersistentContainer.Instance.Players[id].MuteTime = 0;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' has been unmuted", id));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' is not muted", id));
                            return;
                        }
                    }
                    else if (_params[0].ToLower().Equals("list"))
                    {
                        if (_params.Count != 1)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                            return;
                        }
                        if (Mute.Mutes.Count == 0)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] No players are muted"));
                            return;
                        }
                        else
                        {
                            for (int i = 0; i < Mute.Mutes.Count; i++)
                            {
                                string _id = Mute.Mutes[i];
                                int _muteTime = PersistentContainer.Instance.Players[_id].MuteTime;
                                string _muteName = PersistentContainer.Instance.Players[_id].MuteName;
                                if (_muteTime > 0)
                                {
                                    DateTime _muteDate = PersistentContainer.Instance.Players[_id].MuteDate;
                                    TimeSpan varTime = DateTime.Now - _muteDate;
                                    double fractionalMinutes = varTime.TotalMinutes;
                                    int _timepassed = (int)fractionalMinutes;
                                    int _timeleft = _muteTime - _timepassed;
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Muted player: Id '{0}' named '{1}' has '{2}' minutes remaining", _id, _muteName, _timeleft));
                                }
                                else if (_muteTime == -1)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Muted player: Id '{0}' named '{1}' for eternity", _id, _muteName));
                                }
                            }
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                        return;
                    }
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in ConsoleCommandMuteConsole.Execute: {0}", e.Message));
                }
            }
            else
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Mute is not enabled");
                return;
            }
        }
    }
}
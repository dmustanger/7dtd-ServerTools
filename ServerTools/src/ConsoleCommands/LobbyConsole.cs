using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class LobbyConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Set, turn on or turn off the lobby.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-lob off" +
                "  2. st-lob on" +
                "  3. st-lob set" +
                "  4. st-lob set {X} {Y} {Z}" +
                "1. Turn off lobby\n" +
                "2. Turn on lobby\n" +
                "3. Sets the lobby position to your current location\n" +
                "4. Sets the lobby position to the specified x y z location\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Lobby", "lob", "st-lob" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (Lobby.IsEnabled)
                    {
                        Lobby.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Lobby has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Lobby is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (!Lobby.IsEnabled)
                    {
                        Lobby.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Lobby has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Lobby is already on"));
                        return;
                    }
                }
                else if (_params[0] == "set")
                {
                    if (_params.Count != 1 && _params.Count != 4)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 4, found '{0}'", _params.Count));
                        return;
                    }
                    else if (_params.Count == 1)
                    {
                        ClientInfo cInfo = _senderInfo.RemoteClientInfo;
                        if (cInfo != null)
                        {
                            EntityPlayer player = GeneralFunction.GetEntityPlayer(cInfo.entityId);
                            if (player != null)
                            {
                                Vector3 position = player.GetPosition();
                                int x = (int)position.x;
                                int y = (int)position.y;
                                int z = (int)position.z;
                                string lposition = x + "," + y + "," + z;
                                Lobby.LobbyBounds.center = new Vector3(x, y, z);
                                int size = Lobby.Lobby_Size * 2;
                                Lobby.LobbyBounds.size = new Vector3(size, size, size);
                                Lobby.Lobby_Position = lposition;
                                Phrases.Dict.TryGetValue("Lobby2", out string phrase);
                                phrase = phrase.Replace("{Position}", lposition);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] {0}", phrase));
                                Config.WriteXml();
                                Config.LoadXml();
                            }
                        }
                    }
                    else
                    {
                        if (int.TryParse(_params[1], out int x))
                        {
                            if (int.TryParse(_params[2], out int y))
                            {
                                if (int.TryParse(_params[3], out int z))
                                {
                                    string lposition = x + "," + y + "," + z;
                                    Lobby.LobbyBounds.center = new Vector3(x, y, z);
                                    int size = Lobby.Lobby_Size * 2;
                                    Lobby.LobbyBounds.size = new Vector3(size, size, size);
                                    Lobby.Lobby_Position = lposition;
                                    Phrases.Dict.TryGetValue("Lobby2", out string phrase);
                                    phrase = phrase.Replace("{Position}", lposition);
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] {0}", phrase));
                                    Config.WriteXml();
                                    Config.LoadXml();
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[3]));
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[3]));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[3]));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in LobbyConsole.Execute: {0}", e.Message));
            }
        }
    }
}

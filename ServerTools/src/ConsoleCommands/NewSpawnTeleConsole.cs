using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class NewSpawnTeleConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools]- Enable or disable new spawn tele.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-nst off\n" +
                   "  2. st-nst on\n" +
                   "  3. st-nst set {X} {Y} {Z}\n" +
                   "  4. st-nst set\n" +
                   "1. Turn off new spawn tele\n" +
                   "2. Turn on new spawn tele\n" +
                   "3. Set the position a new player will teleport using the specified x y z location\n" +
                   "4. Set the position a new player will teleport using your current location\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-NewSpawnTele", "nst", "st-nst" };
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
                    if (NewSpawnTele.IsEnabled)
                    {
                        NewSpawnTele.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] New spawn tele has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] New spawn tele is already off"));
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
                    if (!NewSpawnTele.IsEnabled)
                    {
                        NewSpawnTele.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] New spawn tele has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] New spawn tele is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("set"))
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
                            EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                            if (player != null)
                            {
                                Vector3 position = player.GetPosition();
                                int x = (int)position.x;
                                int y = (int)position.y;
                                int z = (int)position.z;
                                string sposition = x + "," + y + "," + z;
                                NewSpawnTele.New_Spawn_Tele_Position = sposition;
                                Config.WriteXml();
                                Config.LoadXml();
                                Phrases.Dict.TryGetValue("NewSpawnTele1", out string phrase);
                                phrase = phrase.Replace("{Position}", sposition);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] '{0}'", phrase));
                                return;
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
                                    string sposition = x + "," + y + "," + z;
                                    NewSpawnTele.New_Spawn_Tele_Position = sposition;
                                    Config.WriteXml();
                                    Config.LoadXml();
                                    Phrases.Dict.TryGetValue("NewSpawnTele1", out string phrase);
                                    phrase = phrase.Replace("{Position}", sposition);
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] '{0}'", phrase));
                                    return;
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[3]));
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[2]));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[1]));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in NewSpawnTeleConsole.Execute: {0}", e.Message));
            }
        }
    }
}
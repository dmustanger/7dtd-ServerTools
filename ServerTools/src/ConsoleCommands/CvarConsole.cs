using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class CvarConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Edit or list the current cvar of a player";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-cvar edit <ID/EntityId/PlayerName> <CvarName> <Value>\n" +
                   "  2. st-cvar list <ID/EntityId/PlayerName>\n" +
                   "1. Edit a player's cvar value\n" +
                   "2. Lists the cvars currently applied to a player\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Cvar", "cv", "st-cv" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 4)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or 4, found {0}", _params.Count));
                    return;
                }
                else if (_params[0].ToLower().Equals("edit"))
                {
                    if (_params.Count != 4)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 4, found {0}", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[3], out int value))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[3]));
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                        if (player != null)
                        {
                            if (player.Buffs.CVars.ContainsKey(_params[2]))
                            {
                                player.Buffs.SetCustomVar(_params[2], value);
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageModifyCVar>().Setup(player, _params[2], value));
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' with cvar named '{1}' has been set to value '{2}'", cInfo.playerName, _params[2], _params[3]));
                                return;
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' does not have a cvar named '{1}'", cInfo.playerName, _params[2]));
                                return;
                            }
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                        if (player != null)
                        {
                            if (player.Buffs.CVars.Count > 0)
                            {
                                List<KeyValuePair<string, float>> cvars = player.Buffs.CVars.ToList();
                                for (int i = 0; i < cvars.Count; i++)
                                {
                                    KeyValuePair<string, float> cvar = cvars[i];
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Cvar name '{0}' value '{1}'", cvar.Key, cvar.Value));
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' has no cvar", cInfo.playerName));
                            }
                        }
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online", _params[1]));
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CvarConsole.Execute: {0}", e.Message));
            }
        }
    }
}

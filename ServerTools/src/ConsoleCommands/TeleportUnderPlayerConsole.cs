using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class TeleportUnderPlayerConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Teleports you under the target player.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-tup <EntityId/PlayerName/EOS>\n" +
                   "1. Teleports you under the target player\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-TeleportUnderPlayer", "tup", "st-tup" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params.Count == 1)
                {
                    ClientInfo cInfo = _senderInfo.RemoteClientInfo;
                    ClientInfo cInfo2 = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                    if (cInfo2 != null)
                    {
                        EntityPlayer player2 = GeneralOperations.GetEntityPlayer(cInfo2.entityId);
                        if ((int)player2.position.y < 13)
                        {
                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3((int)player2.position.x, 0, (int)player2.position.z), null, false));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Teleport successful"));
                            return;
                        }
                        else
                        {
                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3((int)player2.position.x, (int)player2.position.y - 10, (int)player2.position.z), null, false));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Teleport successful"));
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' not found", _params[0]));
                        return;
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
                Log.Out(string.Format("[SERVERTOOLS] Error in TeleportUnderPlayerConsole.Run: {0}", e.Message));
            }
        }
    }
}

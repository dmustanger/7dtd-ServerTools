using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class TeleportUnderPlayerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Teleports you under the target player.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. TeleportUnderPlayer <EntityId/SteamId>\n" +
                   "1. Teleports you under the target player\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-TeleportUnderPlayer", "tup", "st-tup" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params.Count == 1)
                {
                    ClientInfo _cInfo = _senderInfo.RemoteClientInfo;
                    ClientInfo _cInfo2 = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (_cInfo2 != null)
                    {
                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                        if ((int)_player2.position.y < 13)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3((int)_player2.position.x, 0, (int)_player2.position.z), null, false));
                            SdtdConsole.Instance.Output(string.Format("Teleport successful"));
                            return;
                        }
                        else
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3((int)_player2.position.x, (int)_player2.position.y - 10, (int)_player2.position.z), null, false));
                            SdtdConsole.Instance.Output(string.Format("Teleport successful"));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player id {0} not found.", _params[0]));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
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

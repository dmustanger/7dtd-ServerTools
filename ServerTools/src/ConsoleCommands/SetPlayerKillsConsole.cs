﻿using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SetPlayerKillsConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Set the value of player kills for the specified player";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-spk <EOS/EntityId/PlayerName> <Value>\n" +
                   "1. Sets the value of player kills\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-SetPlayerKills", "spk", "st-spk" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                    return;
                }
                if (!int.TryParse(_params[1], out int value))
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid value '{0}'", _params[1]));
                    return;
                }
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                if (cInfo != null)
                {
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                    if (player != null)
                    {
                        player.KilledPlayers = value;
                        player.bPlayerStatsChanged = true;
                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(player));
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player kills for Id '{0}' '{1}' named '{2}' has been set to '{3}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, value));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' not found", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SetPlayerKillsConsole.Execute: {0}", e.Message));
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SetDeathsConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Set the value of deaths for the specified player";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-deaths <EOS/EntityId/PlayerName> <Value>\n" +
                   "1. Sets the value of deaths\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-SetDeaths", "deaths", "st-deaths" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                    return;
                }
                if (!int.TryParse(_params[1], out int value))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid value '{0}'", _params[1]));
                    return;
                }
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                if (cInfo != null)
                {
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                    if (player != null)
                    {
                        player.Died = value;
                        player.bPlayerStatsChanged = true;
                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(player));
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Deaths for Id '{0}' '{1}' named '{2}' has been set to '{3}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, value));
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' not found", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SetDeathsConsole.Execute: {0}", e.Message));
            }
        }
    }
}

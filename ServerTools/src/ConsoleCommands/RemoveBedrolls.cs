using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTools
{
    class RemoveBedrolls : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Remove the bedrolls of a player while they are online.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-rb <Id/EntityId/PlayerName>\n" +
                   "1. Remove all bedrolls for the specified player. Player must be online\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-RemoveBedrolls", "rb", "st-rb" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                else
                {
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                    if (cInfo != null && cInfo.loginDone && !GameEventManager.GameEventSequences.ContainsKey("action_remove_bedrolls"))
                    {
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                        if (player != null)
                        {
                            Log.Out("[SERVERTOOLS] Unable to find player data for id '{0}'", _params[0]);
                            return;
                        }
                        GameEventManager.Current.HandleAction("action_remove_bedrolls", null, player, false);
                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup("action_remove_bedrolls", cInfo.entityId, "", "", NetPackageGameEventResponse.ResponseTypes.Approved));
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Unable to find player data for id '{0}'", _params[0]);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RemoveBedrolls.Execute: {0}", e.Message));
            }
        }
    }
}

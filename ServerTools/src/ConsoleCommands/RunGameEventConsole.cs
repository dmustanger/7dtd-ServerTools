using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RunGameEventConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Runs a game event on a player";
        }

        public override string GetHelp()
        {
            return "Runs a event from the gameevent.xml on the specific player\n" +
                "Usage: st-rge <EOS/EntityId/PlayerName> <eventName>\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-RunGameEvent", "rge", "st-rge" };
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
                ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (cInfo != null)
                {
                    EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                    if (player != null)
                    {
                        if (GameEventManager.GameEventSequences.ContainsKey(_params[1]))
                        {
                            GameEventManager.Current.HandleAction(_params[1], null, player, false, "");
                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(_params[1], cInfo.playerName, "", "", NetPackageGameEventResponse.ResponseTypes.Approved));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Ran game event named '{0}' on player '{1}'", _params[1], cInfo.CrossplatformId.CombinedString));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate '{0}' in the game events list", _params[1]));
                            return;
                        }
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RunGameEventConsole.Execute: {0}", e.Message));
            }
        }
    }
}

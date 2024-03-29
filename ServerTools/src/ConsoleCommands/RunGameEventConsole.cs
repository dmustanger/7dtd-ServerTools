﻿using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RunGameEventConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Runs a game event on a player";
        }

        protected override string getHelp()
        {
            return "Runs a event from the gameevent.xml on the specific player\n" +
                "Usage: st-rge <EOS/EntityId/PlayerName> <eventName>\n" +
                "*Note* this may not work correctly with all events. It is programmed in a generic form\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-RunGameEvent", "rge", "st-rge" };
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
                ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (cInfo != null)
                {
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                    if (player != null)
                    {
                        if (GameEventManager.GameEventSequences.ContainsKey(_params[1]))
                        {
                            GameEventManager.Current.HandleAction(_params[1], null, player, false, "");
                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(_params[1], cInfo.entityId, "", "", NetPackageGameEventResponse.ResponseTypes.Approved));
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Ran game event named '{0}' on player '{1}'", _params[1], cInfo.CrossplatformId.CombinedString));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate '{0}' in the game events list", _params[1]));
                            return;
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online", _params[0]));
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
